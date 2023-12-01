using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using schoolEnrollment.Models;
using schoolEnrollment.Utitlty;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace schoolEnrollment.Controllers
{
    public class CourseController : Controller
    {
        DataContext dataContext = new DataContext();
        public IActionResult CourseManament()
        {
            return View();
        }
        public IActionResult CourseDetail()
        {
            string downloadStr = "";
            string loginStr = "";
            string id = HttpContext.Request.Query["id"].ToString();
            var userid = HttpContext.Session.GetInt32("User_Id");
            if (userid == null || userid.ToString() == "")
            {
                loginStr = "<a type=\"button\" class=\"layui-btn layui-btn-radius\" href=\"javascript:void(0)\" onclick=\"ToLogin()\">Login</a>";
            }

            if (id!="")
            {
                var EnrollResult = from el in dataContext.Enrollment
                                   join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                                   from ec in enrollmentCourses.DefaultIfEmpty()
                                   join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                                   from eu in enrollmentUsers.DefaultIfEmpty()
                                   where el.Course_Id == Convert.ToInt32(id)
                                   select new
                                   {
                                       el.Enrollment_Id,
                                       Course_Id = el.Course_Id,
                                       Course_Name = ec.Course_Name,
                                       Enrollment_Time = el.Enrollment_Time,
                                       Enrollment_Status = el.Enrollment_Status,
                                       Payment_Amount = el.Payment_Amount,
                                       Payment_Time = el.Payment_Time == null ? "" : el.Payment_Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                       Payment_Status = el.Payment_Status
                                   };

                if (EnrollResult != null && EnrollResult.Count() > 0)
                {
                    var qua = EnrollResult.ToList();
                    if (qua[0].Payment_Status.ToLower().Equals("paid"))
                    {
                        downloadStr = "<a type=\"button\" class=\"layui-btn layui-btn-radius\" href=\"javascript:void(0)\" onclick=\"DownLoadVideo()\">DownLoad Course</a>";
                    }
                }
            }

            ViewBag.ctrIfLogin = loginStr;

            var result = dataContext.Course.Where(x => x.Course_Id.Equals(Convert.ToInt32(id)));
            if (result != null && result.Count() > 0)
            {
                Course course = result.ToList()[0];

                if (course.Course_File!=null)
                {
                    ViewBag.ctrIfLogin = loginStr+downloadStr;
                }

                ViewBag.ctrimg = "<img src=\"data:image/png;base64," + Convert.ToBase64String(course.Course_Img) + "\" style=\"height: 190px;\">";
                
                return View("CourseDetail", course);
            }
            else
            {
                return View("CourseDetail", null);
            }

        }
        public IActionResult AddCourse()
        {
            if (HttpContext.Request.Query["id"].ToString()=="")
            {
                Course course = new Course();
                course.Start_Time = DateTime.Now;
                course.End_Time = DateTime.Now.AddDays(1);
                return View("AddCourse", course);
            }
            else
            {
                string id = HttpContext.Request.Query["id"].ToString();
                var result = dataContext.Course.Where(x=>x.Course_Id.Equals(Convert.ToInt32(id)));
                if (result != null && result.Count() > 0)
                {
                    Course course = result.ToList()[0];

                    ViewBag.ctrimg = "<img src=\"data:image/png;base64,"+Convert.ToBase64String(course.Course_Img)+ "\" style=\"height: 90px;\">";
                    if (course.Course_File!=null)
                    {
                        ViewBag.ctrvideo = "<img src=\"/images/video.png\" style=\"height: 160px;\">";
                    }
                    
                    return View("AddCourse", course);
                }
                else
                {
                    Course course = new Course();
                    course.Start_Time = DateTime.Now;
                    course.End_Time = DateTime.Now.AddDays(1);
                    return View("AddCourse", course);
                }
            }

        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return base.RedirectToAction("Index", "MainView");

        }

        public string GetCourseTableData()
        {
            
            string GetStrJson = "";
            try
            {
                var result = from cl in dataContext.Course
                             where (string.IsNullOrEmpty(HttpContext.Request.Query["course_name"].ToString()) ? 1 == 1 : cl.Course_Name.Contains(HttpContext.Request.Query["course_name"].ToString()))
                             select new 
                             {
                                 cl.Course_Id,
                                 cl.Course_Name,
                                 cl.Start_Time,
                                 cl.End_Time,
                                 cl.Course_Hours,
                                 cl.Course_Price,
                                 cl.Course_Description,
                                 cl.Course_Img
                             };
                
                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    JArray jArray = new JArray();

                    for (int i = 0; i < qua.Count; i++)
                    {
                        JObject jObject = new JObject();

                        Type type = qua[i].GetType();// obj为传入的对象参数
                                                  //获取类名
                        String className = type.Name;
                        //获取所有公有属性
                        PropertyInfo[] info = type.GetProperties();
                        // 遍历所有属性
                        foreach (PropertyInfo proInfo in info)
                        {
                            if (proInfo.Name.ToLower().Equals("course_file")){
                                continue;
                            }
                            if (proInfo.Name.ToLower().Equals("course_img"))
                            {
                                byte[] data = (byte[])proInfo.GetValue(qua[i], null);
                                if (data!=null)
                                {
                                    jObject.Add(proInfo.Name, Convert.ToBase64String(data));
                                }
                                else
                                {
                                    jObject.Add(proInfo.Name, "");
                                }
                            }
                            else
                            {
                                jObject.Add(proInfo.Name, proInfo.GetValue(qua[i], null).ToString());
                            }
                        }

                        jArray.Add(jObject);
                    }
                    GetStrJson = jArray.ToString();

                }
                else
                {
                    ViewBag.msg = "";

                    GetStrJson = "[]";
                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                GetStrJson = "[]";
            }

            return GetStrJson;
        }
        public string ModifyCourse(string formdata,string course_id)
        {
            JObject resData = new JObject();
            resData.Add("state",-1);
            resData.Add("msg", "");
            resData.Add("Course_Id", "");

            string result = "";
            try
            {
                JObject data = JObject.Parse(formdata);

                data.Remove("file");

                if (string.IsNullOrEmpty(course_id))
                {

                    Course course = JsonConvert.DeserializeObject<Course>(data.ToString());

                    dataContext.Course.Add(course);
                    dataContext.SaveChanges();
                    dataContext.Entry(course);//返回插入的记录并注入到userEntity,关键是这句

                    int value = course.Course_Id;

                    resData["state"] = 1;
                    resData["msg"] = "Add Course Success";
                    resData["Course_Id"] = course.Course_Id;
                }
                else
                {
                    Course course = JsonConvert.DeserializeObject<Course>(data.ToString());
                    course.Course_Id = Convert.ToInt32(course_id);
                    dataContext.Course.Attach(course);

                    dataContext.Entry(course).Property(x => x.Course_Name).IsModified = true;
                    dataContext.Entry(course).Property(x => x.Start_Time).IsModified = true;
                    dataContext.Entry(course).Property(x => x.End_Time).IsModified = true;
                    dataContext.Entry(course).Property(x => x.Course_Hours).IsModified = true;
                    dataContext.Entry(course).Property(x => x.Course_Price).IsModified = true;
                    dataContext.Entry(course).Property(x => x.Course_Description).IsModified = true;

                    dataContext.SaveChanges();

                    resData["state"] = 1;
                    resData["msg"] = "Edit Course Success";
                    resData["Course_Id"] = course_id;
                }
            }
            catch (Exception ex)
            {
                resData["state"] = -1;
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }

        public string OnUploadFilePicture(string course_id)
        {

            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            if (course_id=="")
            {
                resData["msg"] = "Course Id can not be empty!";
                return resData.ToString() ;

            }
            if (Request.Form.Files.Count<1)
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
                Course course = new Course();
                course.Course_Id = Convert.ToInt32(course_id);
                course.Course_Img = img;
                dataContext.Course.Attach(course);

                dataContext.Entry(course).Property(x => x.Course_Img).IsModified = true;

                dataContext.SaveChanges();

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
        public string OnUploadFileVideo(string course_id)
        {

            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            if (course_id == "")
            {
                resData["msg"] = "Course Id can not be empty!";
                return resData.ToString();

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
                Course course = new Course();
                course.Course_Id = Convert.ToInt32(course_id);
                course.Course_File = img;
                dataContext.Course.Attach(course);

                dataContext.Entry(course).Property(x => x.Course_File).IsModified = true;

                dataContext.SaveChanges();

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

        public string DeleteCourse(string course_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            if (string.IsNullOrEmpty(course_id) || course_id.ToLower().Equals("false"))
            {
                resData["state"] = -1;
                resData["msg"] = "Course_Id can not be empty!";
            }
            else
            {
                int idtmp = -1;
                if (course_id != "")
                {
                    idtmp = Convert.ToInt32(course_id);
                }

                Course author = dataContext.Set<Course>().Where(p => p.Course_Id == idtmp).FirstOrDefault();
                if (author != null)
                {
                    dataContext.Set<Course>().Remove(author);
                    dataContext.SaveChanges();

                    resData["state"] = 1;
                    resData["msg"] = "Delete Success";
                }
                else
                {
                    resData["msg"] = "Delete Fail";
                }
            }

            return resData.ToString();
        }

        public string AddCourseToSelectList(string course_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            if (string.IsNullOrEmpty(course_id))
            {
                resData["state"] = -1;
                resData["msg"] = "Course_Id can not be empty!";

                return resData.ToString();  
            }
            int? userid = HttpContext.Session.GetInt32("User_Id");
            if (userid == null || userid.Value.ToString() == "")
            {
                resData["state"] = -1;
                resData["msg"] = "Please Login first!";

                return resData.ToString();
            }

            try
            {
                SelectList selectList = new SelectList();
                selectList.Select_Time = DateTime.Now;
                selectList.Course_Id = Convert.ToInt32(course_id);
                selectList.User_Id = userid.Value;


                var result = dataContext.SelectList.Where(x => x.User_Id.Equals(userid.Value) && x.Course_Id.Equals(Convert.ToInt32(course_id)));

                if (result != null && result.Count() > 0)
                {
                    
                    resData["state"] = -1;
                    resData["msg"] = "The Course already in the list";
                }
                else
                {

                    dataContext.SelectList.Add(selectList);
                    dataContext.SaveChanges();

                    resData["state"] = 1;
                    resData["msg"] = "Add List Success!";

                }

            }
            catch (Exception ex)
            {
                resData["state"] = -1;
                resData["msg"] = ex.Message;

            }

            return resData.ToString();
        }

        public IActionResult DownloadCourse()
        {

            byte[] files = null;
            var userid = HttpContext.Session.GetInt32("User_Id");
            string filename = "";
            if (HttpContext.Request.Query["id"].ToString() != "")
            {
                string id = HttpContext.Request.Query["id"].ToString();
                var EnrollResult = from el in dataContext.Enrollment
                                   join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                                   from ec in enrollmentCourses.DefaultIfEmpty()
                                   join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                                   from eu in enrollmentUsers.DefaultIfEmpty()
                                   where el.Course_Id == Convert.ToInt32(id)&&el.User_Id== userid
                                   select new
                                   {
                                       el.Enrollment_Id,
                                       Course_Id = el.Course_Id,
                                       Course_Name = ec.Course_Name,
                                       Course_File = ec.Course_File,
                                       Enrollment_Time = el.Enrollment_Time,
                                       Enrollment_Status = el.Enrollment_Status,
                                       Payment_Amount = el.Payment_Amount,
                                       Payment_Time = el.Payment_Time == null ? "" : el.Payment_Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                       Payment_Status = el.Payment_Status
                                   };

                if (EnrollResult != null && EnrollResult.Count() > 0)
                {
                    var qua = EnrollResult.ToList();
                    if (qua[0].Payment_Status.ToLower().Equals("paid"))
                    {
                        files = qua[0].Course_File;
                        filename = qua[0].Course_Name;
                    }
                }
            }

            return File(files, "application/octet-stream",$"course_{filename}_{DateTime.Now.Ticks}.mp4");

        }
    }
}
