using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using schoolEnrollment.Models;
using schoolEnrollment.Utitlty;
using Stripe;
using System.Data;
using System.IO;
using System.Xml.Linq;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace schoolEnrollment.Controllers
{
    public class AccountController : Controller
    {
        DataContext dataContext = new DataContext();
        AzureStorage azureStorage = new AzureStorage();
        public IActionResult Index(Users user)
        {
            //ViewBag.ctrrole = BindRoleData(false);
            return View(user);
        }

        public IActionResult MyInfo()
        {
            var userid = HttpContext.Session.GetInt32("User_Id");
            var result = dataContext.Users.Where(x => x.User_Id.Equals(userid));
            Users users = new Users();
            if (result != null && result.Count() > 0)
            {
                users = result.ToList()[0];

               

                        imgstr = "<img id=\"userimg\" src=\"data:image/png;base64," + Convert.ToBase64String(bytes) + "\" style=\"height: 190px;\">";
                        break;
                    }
                }
                ViewBag.ctrimg = imgstr;

                return View(users);
            }
            else
            {
                return View(users);
            }
                
        }
        public IActionResult AjaxLogin(Users user)
        {
            //ViewBag.ctrrole = BindRoleData(false);
            return View(user);
        }

        public IActionResult Register(Users user)
        {
            return View(user);
        }
        public string BindRoleData(bool stuonly)
        {
            var roles = dataContext.Role.Select(x => new { Role_Id = x.Role_Id, Role_Name = x.Role_Name });
            if (stuonly)
            {
                roles = dataContext.Role.Where(x=>x.Role_Name.Equals("student")).Select(x => new { Role_Id = x.Role_Id, Role_Name = x.Role_Name });
            }

            string selectstr = "<div style=\"display:inline-block; width:75%; float:right;\"><select id=\"role_id\" name=\"role_id\"  value=\"@(Model.Role_Id)\">{0}</select></div>";
            string optionstr = "<option value=\"\">--Please Select--</option>";
            if (roles.Count() > 0)
            {
                foreach (var role in roles)
                {
                    optionstr = optionstr + "<option value=\"" + role.Role_Id + "\">" + role.Role_Name + "</option>";
                }

            }

            selectstr = string.Format(selectstr, optionstr);

            return selectstr;
        }
        // 提交登录
        [HttpPost]
        public ActionResult Login(Users user)
        {
            try
            {
                var result = dataContext.Users.Where(x => x.User_Account.Equals(user.User_Account.ToString()) && x.User_Password.Equals(user.User_Password.ToString()));
                
                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    HttpContext.Session.SetInt32("User_Id", qua[0].User_Id);

                    HttpContext.Session.SetString("Name", qua[0].Name);
                    HttpContext.Session.SetInt32("Role_Id", qua[0].Role_Id);
                    HttpContext.Session.SetString("Email", qua[0].Email==null?"": qua[0].Email);

                    if (qua[0].Role_Id.Equals(1))
                    {
                        return base.Redirect("/MainView/AdminIndex");
                    }
                    else
                    {
                        return base.Redirect("/MainView/Index");
                    }
                    
                }
                else
                {
                    ViewBag.msg = "UserName or Password Wrong";

                    return View("Index", new Users()
                    {

                    });
                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                return View("Index", new Users()
                {

                });
            }
        }
        // 提交登录
        [HttpPost]
        public string ToLogin(string formdata)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            try
            {
                JObject userData = JObject.Parse(formdata);
                var result = dataContext.Users.Where(x => x.User_Account.Equals(userData["user_account"].ToString()) && x.User_Password.Equals(userData["user_password"].ToString()));

                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    HttpContext.Session.SetInt32("User_Id", qua[0].User_Id);

                    HttpContext.Session.SetString("Name", qua[0].Name);
                    HttpContext.Session.SetInt32("Role_Id", qua[0].Role_Id);
                    HttpContext.Session.SetString("Email", qua[0].Email == null ? "" : qua[0].Email);

                    resData["state"] = 1;
                    resData["msg"] = "Login Success";
                }
                else
                {
                    resData["msg"] = "The account does not exist";
                }

            }
            catch (Exception ex)
            {
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }

        // 提交登录
        [HttpPost]
        public string DoRegister(string formdata)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            try
            {
                JObject userData = JObject.Parse(formdata);

                Users user = new Users();
                user.Role_Id = 2;
                user.User_Account = userData["user_account"].ToString();
                user.Name = userData["name"].ToString();
                user.User_Password = userData["user_password"].ToString();
                user.Phone = userData["phone"].ToString();
                user.Email = userData["email"].ToString();
                
                var result = dataContext.Users.Where(x => x.User_Account.Equals(user.User_Account));

                if (result != null && result.Count() > 0)
                {
                    ViewBag.msg = "The account already exists!";

                    resData["msg"] = "The account already exists!";
                }
                else
                {
                    dataContext.Users.Add(user);
                    dataContext.SaveChanges();

                    resData["state"] = 1;
                    resData["msg"] = "Register success!";
                    
                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public string OnUploadFilePicture(string user_id)
        {

            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            var userid = HttpContext.Session.GetInt32("User_Id");
            var result = dataContext.Users.Where(x => x.User_Id.Equals(userid));
            Users users = new Users();

            if (result != null && result.Count() > 0)
            {
                users = result.ToList()[0];
               
            }

            if (Request.Form.Files.Count < 1)
            {
                resData["msg"] = "The File can not be empty!";
                return resData.ToString();

            }

            try
            {
                var imgFile = Request.Form.Files[0];

                byte[] img = null;

                using (var memoryStream = new MemoryStream())
                {
                    imgFile.CopyTo(memoryStream);
                    img = memoryStream.ToArray();
                }
                
                string filename = users.Name + "_" + users.User_Id;

                BlobResponseDto blobResponseDto = azureStorage.UploadAsync(img, filename).Result;

                resData["state"] = 1;
                resData["msg"] = "Add Course Success";

            }
            catch (Exception ex)
            {
                resData["state"] = -1;
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }

        public string DeleteImage()
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            var userid = HttpContext.Session.GetInt32("User_Id");
            var result = dataContext.Users.Where(x => x.User_Id.Equals(userid));
            Users users = new Users();

            if (result != null && result.Count() > 0)
            {
                users = result.ToList()[0];

            }

            try
            {
                
                string filename = users.Name + "_" + users.User_Id;

                BlobResponseDto blobResponseDto = azureStorage.DeleteAsync(filename).Result;

                resData["state"] = 1;
                resData["msg"] = "Delete Success";

            }
            catch (Exception ex)
            {
                resData["state"] = -1;
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }
    }
}
