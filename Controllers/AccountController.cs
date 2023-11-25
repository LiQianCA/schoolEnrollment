using Microsoft.AspNetCore.Mvc;
using OnlineCourse.Models;
using OnlineCourse.Utitlty;
using System.Data;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace OnlineCourse.Controllers
{
    public class AccountController : Controller
    {
        DataContext dataContext = new DataContext();
        public IActionResult Index(Users user)
        {
            ViewBag.ctrrole = BindRoleData(false);
            return View(user);
        }
        public IActionResult Register(Users user)
        {
            ViewBag.ctrrole = BindRoleData(true);
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
                return View("Index", new Users()
                {

                });
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                return View("Index", new Users()
                {

                });
            }
        }
    }
}
