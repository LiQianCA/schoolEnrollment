using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using schoolEnrollment.Models;
using schoolEnrollment.Utitlty;
using Stripe;
using Stripe.Checkout;
using System.Linq;
using System.Reflection;

namespace schoolEnrollment.Controllers
{
    public class MainViewController : Controller
    {
        DataContext dataContext = new DataContext();
        AzureStorage azureStorage = new AzureStorage();
        public IActionResult Index()
        {
            var imgstr = "https://unpkg.com/outeres@0.0.10/demo/avatar/1.jpg";
            var userid = HttpContext.Session.GetInt32("User_Id");
            if (userid != null&& userid.ToString()!="")
            {
                var userResult = dataContext.Users.Where(x => x.User_Id.Equals(userid));
                Users users = new Users();
                if (userResult != null && userResult.Count() > 0)
                {
                    users = userResult.ToList()[0];

                    var userimgList = azureStorage.ListAsync().Result;

                    
                    for (int i = 0; i < userimgList.Count; i++)
                    {
                        if (userimgList[i].Name.Equals(users.Name + "_" + users.User_Id))
                        {
                            BlobDto file = azureStorage.DownloadAsync(userimgList[i].Name).Result;
                            
                            byte[] bytes = new byte[file.Content.Length];
                            file.Content.Read(bytes, 0, bytes.Length);

                            imgstr = "data:image/png;base64," + Convert.ToBase64String(bytes);
                            break;
                        }
                    }
                    
                }

                //ViewBag.ctrIfLogin = "<button type=\"button\" class=\"layui-btn layui-btn-radius\">My Info</button><button type=\"button\" class=\"layui-btn layui-btn-normal layui-btn-radius\">My Course</button>";

                ViewBag.ctrIfLogin = $"<ul style=\"margin-top:-10px;\" class=\"layui-nav layui-bg-gray\"><li class=\"layui-nav-item\" lay-unselect><a href=\"javascript:;\"><img src=\"{imgstr}\" class=\"layui-nav-img\"></a><dl class=\"layui-nav-child\"><dd><a  href=\"javascript:void(0)\" onclick=\"ToSelectList()\">My SelectList</a></dd><dd><a href=\"javascript:;\" onclick=\"MyInfo()\">MyInfo</a></dd><hr><dd style=\"text-align: center;\"><a href=\"/MainView/Logout\">Logout</a></dd></dl></li></ul>";
            }
            else
            {
                ViewBag.ctrIfLogin = "<a type=\"button\" class=\"layui-btn layui-btn-radius\" href=\"/Account/Index\">Login</a><a type=\"button\" class=\"layui-btn layui-btn-normal layui-btn-radius\" href=\"/Account/Register\">Sign Up</a>";

            }

            string topCourses = "";
            string topCurrentCourseMsg = "";
            
            var result = dataContext.Course;
            if (result != null && result.Count() > 0)
            {
                var qua = result.ToList();

                JArray jArray = new JArray();

                for (int i = 0; i < qua.Count; i++)
                {
                    string courseMsg = "";
                    if (i == 0)
                    {
                        topCurrentCourseMsg = $"<div class=\"layui-card\" style=\"height:250px;\"><div class=\"layui-card-header\"  style=\"background-color: #fafafa;\">Course Msg</div><div id=\"currentCourseMsg\" class=\"layui-card-body\">Course Name:{qua[i].Course_Name}<br>Start Time:{qua[i].Start_Time}<br>End Time:{qua[i].End_Time}<br>Course Hours:{qua[i].Course_Hours}<br>Course Price:{qua[i].Course_Price}<br>Course Description:{qua[i].Course_Description}<br></div></div>";
                    }
                    if (i<3)
                    {
                        courseMsg = $"Course Name:{qua[i].Course_Name}<br>Start Time:{qua[i].Start_Time}<br>End Time:{qua[i].End_Time}<br>Course Hours:{qua[i].Course_Hours}<br>Course Price:{qua[i].Course_Price}<br>Course Description:{qua[i].Course_Description}<br>";
                        topCourses = topCourses+ $"<div><a id=\"{qua[i].Course_Id}\" href=\"javascript:void(0)\" onclick=\"JumpToCourseDetail({qua[i].Course_Id})\"><img src=\"data:image/png;base64," + Convert.ToBase64String(qua[i].Course_Img)+ $"\" datamsg=\"{courseMsg}\"></a></div>";
                    }

                    string imgBase64 = "";
                    if (qua[i].Course_Img!=null)
                    {
                        imgBase64 = $"data:image/png;base64,{Convert.ToBase64String(qua[i].Course_Img)}";
                    }
                }
                
            }
            ViewBag.ctrTopCourses = topCourses;
            ViewBag.ctrTopCurrentCourseMsg = topCurrentCourseMsg;
            return View();
        }
        public IActionResult AdminIndex()
        {
            var userid = HttpContext.Session.GetInt32("User_Id");
            if (userid != null && userid.ToString() != "")
            {
                //ViewBag.ctrIfLogin = "<button type=\"button\" class=\"layui-btn layui-btn-normal layui-btn-radius\">My Course</button>";
            }
            else
            {
                ViewBag.ctrIfLogin = "<a type=\"button\" class=\"layui-btn layui-btn-radius\" href=\"/Account/Index\">Login</a><a type=\"button\" class=\"layui-btn layui-btn-normal layui-btn-radius\" href=\"/Account/Register\">Sign Up</a>";

            }
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return base.RedirectToAction("Index", "MainView");

        }

        public IActionResult PaySuccess()
        {
            string id = HttpContext.Request.Query["id"].ToString();
            var result = dataContext.Course.Where(x => x.Course_Id.Equals(Convert.ToInt32(id)));
            if (result != null && result.Count() > 0)
            {
                Course course = result.ToList()[0];

                ViewBag.ctrimg = "<img src=\"data:image/png;base64," + Convert.ToBase64String(course.Course_Img) + "\" style=\"height: 190px;\">";
                
                string enrollmentid = HttpContext.Request.Query["enrollmentid"].ToString();
                if (!string.IsNullOrEmpty(enrollmentid))
                {
                    Enrollment enrollment = new Enrollment();
                    enrollment.Enrollment_Id = Convert.ToInt32(enrollmentid);
                    enrollment.Enrollment_Status = "Enrolled";
                    enrollment.Enrollment_Time = DateTime.Now;
                    enrollment.Payment_Status = "Paid";
                    enrollment.Payment_Time = DateTime.Now;
                    dataContext.Enrollment.Attach(enrollment);

                    dataContext.Entry(enrollment).Property(x => x.Enrollment_Status).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Enrollment_Time).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Payment_Status).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Payment_Time).IsModified = true;
                    dataContext.SaveChanges();

                }


                return View("PaySuccess", course);
            }
            else
            {
                return View("PaySuccess", new Course());
            }

        }
        public IActionResult PayFail()
        {
            return View();

        }
        public IActionResult CoursePay()
        {
            string id = HttpContext.Request.Query["id"].ToString();
            var result = dataContext.Course.Where(x => x.Course_Id.Equals(Convert.ToInt32(id)));
            if (result != null && result.Count() > 0)
            {
                Course course = result.ToList()[0];

                ViewBag.ctrimg = "<img src=\"data:image/png;base64," + Convert.ToBase64String(course.Course_Img) + "\" style=\"height: 190px;\">";

                return View("CoursePay", course);
            }
            else
            {
                return View("CoursePay", null);
            }
        }


        public string GetCurrentPageCourses(int pagelimit, int pageindex,string searchstr)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");
            resData.Add("data", "");
            resData.Add("datatotalcount", 0);
            resData.Add("datacount", 0);

            try
            {
                string liCourses = "";
                var result = dataContext.Course.Where(x =>(string.IsNullOrEmpty(searchstr)?1==1: x.Course_Name.Contains(searchstr)));
                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();
                    if (result.Count() >= pagelimit * (pageindex))
                    {
                        qua = result.Skip(pagelimit * (pageindex-1)).Take(pagelimit).ToList();
                    }
                    else
                    {
                        qua = result.Skip(pagelimit * (pageindex - 1)).ToList();
                    }

                    JArray jArray = new JArray();

                    for (int i = 0; i < qua.Count; i++)
                    {
                        if (i == 0)
                        {
                            liCourses = "<ul id=\"ullist\" class=\"brick-list clearfix\">";
                        }

                        string imgBase64 = "";
                        if (qua[i].Course_Img != null)
                        {
                            imgBase64 = $"data:image/png;base64,{Convert.ToBase64String(qua[i].Course_Img)}";
                        }

                        liCourses = liCourses + $"<li class=\"brick-item brick-item-m brick-item-m-2\"><a href=\"javascript:void(0)\" onclick=\"JumpToCourseDetail({qua[i].Course_Id})\"><div class=\"figure figure-img\"><img height=\"160\" alt=\"{qua[i].Course_Name}\" src=\"{imgBase64}\"></div><h3 class=\"title\">\r\nName:{qua[i].Course_Name}\r\n</h3><p class=\"desc\">Desc:{qua[i].Course_Description}</p><p class=\"price\"><span class=\"num\">Price:{qua[i].Course_Price}</span><span>$</span></p></a></li>";

                        if (i == qua.Count - 1)
                        {
                            liCourses = liCourses + "</ul>";
                        }
                    }

                    resData["state"] = 1;
                    resData["datatotalcount"] = result.Count();
                    resData["datacount"] = qua.Count;
                    resData["data"] = liCourses;
                }

            }
            catch (Exception ex)
            {
                resData["state"] = -1;
                resData["msg"] = ex.Message;
            }
            
            return resData.ToString();
        }

        public int GetAllCourseCount(string searchstr)
        {
            try
            {
                var result = dataContext.Course.Where(x => (string.IsNullOrEmpty(searchstr) ? 1 == 1 : x.Course_Name.Contains(searchstr)));
                if (result!=null&&result.Count()>0)
                {
                    return result.Count();
                }
                else
                {
                    return 0;
                }
            }
            catch(Exception ex)
            {
                return 0;
            }
            
        }

        public string GetEnrollmentTableData(string course_name)
        {
            string GetStrJson = "";
            try
            {
                var result = from el in dataContext.Enrollment
                             join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                             from ec in enrollmentCourses.DefaultIfEmpty()
                             join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                             from eu in enrollmentUsers.DefaultIfEmpty()
                             where (string.IsNullOrEmpty(HttpContext.Request.Query["course_name"].ToString())?1==1:ec.Course_Name.Contains(HttpContext.Request.Query["course_name"].ToString()))
                             select new
                             {
                                 el.Enrollment_Id,
                                 Course_Id = el.Course_Id,
                                 Course_Name = ec.Course_Name,
                                 Enrollment_Time = el.Enrollment_Time,
                                 Enrollment_Status = el.Enrollment_Status,
                                 Payment_Amount = el.Payment_Amount,
                                 Payment_Time = el.Payment_Time==null?"": el.Payment_Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                 Payment_Status = el.Payment_Status
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
                            if (proInfo.GetValue(qua[i], null) == null)
                            {
                                jObject.Add(proInfo.Name, "");
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
                    GetStrJson = "[]";
                }
            }
            catch (Exception ex)
            {
                GetStrJson = "[]";
            }

            return GetStrJson;
        }

        public string ApproveCourse(int enrollment_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            try
            {
                var result = from el in dataContext.Enrollment
                             join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                             from ec in enrollmentCourses.DefaultIfEmpty()
                             join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                             from eu in enrollmentUsers.DefaultIfEmpty()
                             where el.Enrollment_Id.Equals(enrollment_id)
                             select new
                             {
                                 el.Enrollment_Id,
                                 Course_Id = el.Course_Id,
                                 Email = eu.Email,
                                 Name = eu.Name,
                                 Course_Name = ec.Course_Name,
                                 Enrollment_Time = el.Enrollment_Time,
                                 Enrollment_Status = el.Enrollment_Status,
                                 Payment_Amount = el.Payment_Amount,
                                 Payment_Time = el.Payment_Time == null ? "" : el.Payment_Time.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                 Payment_Status = el.Payment_Status
                             };

                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    if (!string.IsNullOrEmpty(qua[0].Enrollment_Status)&& qua[0].Enrollment_Status.Equals("Applied"))
                    {
                        Enrollment enrollment = new Enrollment();
                        enrollment.Enrollment_Id = enrollment_id;
                        enrollment.Enrollment_Status = "Approved";
                        enrollment.Enrollment_Time = DateTime.Now;
                        dataContext.Enrollment.Attach(enrollment);

                        dataContext.Entry(enrollment).Property(x => x.Enrollment_Status).IsModified = true;
                        dataContext.Entry(enrollment).Property(x => x.Enrollment_Time).IsModified = true;
                        dataContext.SaveChanges();

                        Course course1 = new Course();
                        var resultCourse = dataContext.Course.Where(x => x.Course_Id.Equals(qua[0].Course_Id));
                        if (resultCourse != null && resultCourse.Count() > 0)
                        {
                            course1 = resultCourse.ToList()[0];

                        }

                        byte[] file = CreateCoursePDF(qua[0].Name, course1);
                        MailHelper.SendMail(qua[0].Email, "", "Acceptance Letter", GetEmailContent(qua[0].Name, course1), file);
                        resData["state"] = 1;
                        resData["msg"] = "Approve Success!";
                    }
                    else
                    {
                        resData["msg"] = "This Enrollment can not be Approve again!";
                    }
                    
                }
                else
                {
                    resData["msg"] = "The Enrollment is not exsit!";
                }
            }
            catch (Exception ex)
            {
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }

        public string RejectCourse(int enrollment_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            try
            {
                var result = from el in dataContext.Enrollment
                             join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                             from ec in enrollmentCourses.DefaultIfEmpty()
                             join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                             from eu in enrollmentUsers.DefaultIfEmpty()
                             where el.Enrollment_Id.Equals(enrollment_id)
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

                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    if (!string.IsNullOrEmpty(qua[0].Enrollment_Status) && qua[0].Enrollment_Status.Equals("Applied"))
                    {
                        Enrollment enrollment = new Enrollment();
                        enrollment.Enrollment_Id = enrollment_id;
                        enrollment.Enrollment_Status = "Rejected";
                        enrollment.Enrollment_Time = DateTime.Now;
                        dataContext.Enrollment.Attach(enrollment);

                        dataContext.Entry(enrollment).Property(x => x.Enrollment_Status).IsModified = true;
                        dataContext.Entry(enrollment).Property(x => x.Enrollment_Time).IsModified = true;
                        dataContext.SaveChanges();

                        resData["state"] = 1;
                        resData["msg"] = "Reject Success!";
                    }
                    else
                    {
                        resData["msg"] = "This Enrollment can not be Reject again!";
                    }

                }
                else
                {
                    resData["msg"] = "The Enrollment is not exsit!";
                }
            }
            catch (Exception ex)
            {
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }

        public string DeleteEnrollment(int enrollment_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            Enrollment author = dataContext.Set<Enrollment>().Where(p => p.Enrollment_Id == enrollment_id).FirstOrDefault();
            if (author != null)
            {
                dataContext.Set<Enrollment>().Remove(author);
                dataContext.SaveChanges();

                resData["state"] = 1;
                resData["msg"] = "Delete Success";
            }
            else
            {
                resData["msg"] = "Delete Fail";
            }

            return resData.ToString();
        }

        public byte[] CreateCoursePDF(string userName,Course courseData)
        {
            Rectangle pageSize = new Rectangle(762, 1024);
            Document document = new Document(pageSize, 10, 10, 120, 80);
            MemoryStream file = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(document, file);
            document.Open();
            document.AddTitle("Acceptance Letter");
            document.Add(new iTextSharp.text.Paragraph("Dear " + userName + ":"));
            document.Add(new iTextSharp.text.Paragraph("Congratulations on being admitted to our school course.Here are some basic information about our course, please make sure to read it before the course starts." ));
            document.Add(new iTextSharp.text.Paragraph("Course Name:" + courseData.Course_Name.ToString()));
            document.Add(new iTextSharp.text.Paragraph("Start Time:" + courseData.Start_Time.ToString("yyyy-MM-dd HH:mm:ss")));
            document.Add(new iTextSharp.text.Paragraph("End Time:" + courseData.End_Time.ToString("yyyy-MM-dd HH:mm:ss")));
            document.Add(new iTextSharp.text.Paragraph("Course Hours:" + courseData.Course_Hours.ToString()));
            document.Add(new iTextSharp.text.Paragraph("Course Price:" + courseData.Course_Price.ToString()));
            document.Add(new iTextSharp.text.Paragraph("Course Description:" + courseData.Course_Description.ToString()));

            document.Close();
            writer.Close();

            return file.ToArray();
        }

        public string GetEmailContent(string userName, Course courseData)
        {
            string content = "";
            content = content + "Dear " + userName + ":<br/>";
            content = content+ "Congratulations on being admitted to our school course.Here are some basic information about our course, please make sure to read it before the course starts.<br/>";
            content = content + "Course Name:" + courseData.Course_Name.ToString()+ "<br/>";
            content = content + "Start Time:" + courseData.Start_Time.ToString("yyyy-MM-dd HH:mm:ss") + "<br/>";
            content = content + "End Time:" + courseData.End_Time.ToString("yyyy-MM-dd HH:mm:ss") + "<br/>";
            content = content + "Course Hours:" + courseData.Course_Hours.ToString() + "<br/>";
            content = content + "Course Price:" + courseData.Course_Price.ToString() + "<br/>";
            content = content + "Course Description:" + courseData.Course_Description.ToString() + "<br/>";

            return content;
        }

        public string ToPayCourse(string course_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            var result = from el in dataContext.Enrollment
                         join cl in dataContext.Course on el.Course_Id equals cl.Course_Id into enrollmentCourses
                         from ec in enrollmentCourses.DefaultIfEmpty()
                         join ul in dataContext.Users on el.User_Id equals ul.User_Id into enrollmentUsers
                         from eu in enrollmentUsers.DefaultIfEmpty()
                         where el.Course_Id == Convert.ToInt32(course_id)
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
            if (result != null && result.Count() > 0)
            {
                var qua = result.ToList();

                if (!string.IsNullOrEmpty(qua[0].Enrollment_Status) && qua[0].Enrollment_Status.Equals("Approved"))
                {
                    // 密钥
                    StripeConfiguration.ApiKey = "sk_test_51OHTYEGQSLeOCjaBp7Ir3YtM33tzdh3IpN07jaZP7ncnwTfaFDaVi5BeFJMEK97usiA3wsim2rjyqiHoTgGMLQIz003F5gJnZ3";

                    var options = new SessionCreateOptions
                    {
                        LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                     Price = "price_1OHmqZGQSLeOCjaB8EHwglWX",
                    Quantity = 1,
                  },
                },
                        
                        Mode = "subscription",
                        SuccessUrl = "http://"+Request.Host.ToString() + "/MainView/PaySuccess?id="+ qua[0].Course_Id+ "&enrollmentid="+ qua[0].Enrollment_Id,
                        CancelUrl = "http://" + Request.Host.ToString() + "/MainView/PayFail",
                    };
                    
                    var service = new SessionService();
                    Session session = service.Create(options);

                    resData["state"] = 1;
                    resData["msg"] = session.Url;
                }
                else
                {
                    resData["msg"] = "This Course can not be Pay!";
                }

            }
            else
            {
                resData["msg"] = "The Course is not exsit!";
            }

            return resData.ToString();
        }
        public string AdminRefundCourse(string enrollment_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            var result = from el in dataContext.Enrollment
                         where el.Enrollment_Id == Convert.ToInt32(enrollment_id)
                         select new
                         {
                             el.Enrollment_Id,
                             Course_Id = el.Course_Id,
                             Enrollment_Status = el.Enrollment_Status,
                             Payment_Status = el.Payment_Status
                         };
            if (result != null && result.Count() > 0)
            {
                var qua = result.ToList();

                if (!string.IsNullOrEmpty(qua[0].Enrollment_Status) && qua[0].Enrollment_Status.Equals("Enrolled") && qua[0].Payment_Status.Equals("Paid"))
                {
                    Enrollment enrollment = new Enrollment();
                    enrollment.Enrollment_Id = qua[0].Enrollment_Id;
                    enrollment.Enrollment_Status = "Canceled";
                    enrollment.Enrollment_Time = DateTime.Now;
                    enrollment.Payment_Status = "Refund";
                    enrollment.Payment_Time = DateTime.Now;
                    dataContext.Enrollment.Attach(enrollment);

                    dataContext.Entry(enrollment).Property(x => x.Enrollment_Status).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Enrollment_Time).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Payment_Status).IsModified = true;
                    dataContext.Entry(enrollment).Property(x => x.Payment_Time).IsModified = true;

                    dataContext.SaveChanges();

                    resData["state"] = 1;
                    resData["msg"] = "Refund Success";
                }
                else
                {
                    resData["msg"] = "This Course can not be Refund!";
                }

            }
            else
            {
                resData["msg"] = "The Course is not exsit!";
            }

            return resData.ToString();
        }
    }
}
