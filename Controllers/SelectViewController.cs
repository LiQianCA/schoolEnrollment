using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using schoolEnrollment.Models;
using schoolEnrollment.Utitlty;
using System.Reflection;
using static NuGet.Packaging.PackagingConstants;

namespace schoolEnrollment.Controllers
{
    public class SelectViewController : Controller
    {
        DataContext dataContext = new DataContext();
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Name");
            if (username != null && username.ToString() != "")
            {
                ViewBag.welcomeStr = "WelCome,"+ username;
            }

            return View();
        }

        public string GetSelectCourseTableData()
        {
            var userid = HttpContext.Session.GetInt32("User_Id");
            string GetStrJson = "";
            try
            {

                //var result = dataContext.SelectList.GroupJoin(dataContext.Course , selectlist=> selectlist.Course_Id,course=>course.Course_Id, (selectlist, selectlistCourses) => new
                //{
                //    selectlist.Select_Id,
                //    Course_Name = selectlistCourses.Select(co => co.Course_Name).FirstOrDefault(),
                //    Start_Time = selectlistCourses.Select(co => co.Start_Time).FirstOrDefault(),
                //    End_Time = selectlistCourses.Select(co => co.End_Time).FirstOrDefault(),
                //    Course_Hours = selectlistCourses.Select(co => co.Course_Hours).FirstOrDefault(),
                //    Course_Price = selectlistCourses.Select(co => co.Course_Price).FirstOrDefault(),
                //    selectlist.Select_Time,
                //});

                var result = from sl in dataContext.SelectList
                             join cl in dataContext.Course on sl.Course_Id equals cl.Course_Id into selectCourses
                             from sc in selectCourses.DefaultIfEmpty()
                             join ul in dataContext.Users on sl.User_Id equals ul.User_Id into selectUsers
                             from su in selectUsers.DefaultIfEmpty()
                             join el in dataContext.Enrollment on new { sl.Course_Id, sl.User_Id } equals new {el.Course_Id, el.User_Id } into selectEnrolls
                             from se in selectEnrolls.DefaultIfEmpty()
                             where sl.User_Id == userid
                             select new {
                                 sl.Select_Id,
                                 sl.Course_Id,
                                 Course_Name = sc.Course_Name,
                                 Start_Time = sc.Start_Time,
                                 End_Time = sc.End_Time,
                                 Course_Hours = sc.Course_Hours,
                                 Course_Price = sc.Course_Price,
                                 Enrollment_Status = se.Enrollment_Status,
                                 Payment_Status = se.Payment_Status,
                                 sl.Select_Time,
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

        public string ApplyCourse(int selectid)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            //if (selectid=="")
            //{
            //    resData["msg"] = "Select Id can not be empty!";
            //    return resData.ToString();
            //}
            try
            {
                var userid = HttpContext.Session.GetInt32("User_Id");

                var result = from sl in dataContext.SelectList
                             join cl in dataContext.Course on sl.Course_Id equals cl.Course_Id into selectCourses
                             from sc in selectCourses.DefaultIfEmpty()
                             join ul in dataContext.Users on sl.User_Id equals ul.User_Id into selectUsers
                             from su in selectUsers.DefaultIfEmpty()
                             join el in dataContext.Enrollment on new { sl.Course_Id, sl.User_Id } equals new { el.Course_Id, el.User_Id } into selectEnrolls
                             from se in selectEnrolls.DefaultIfEmpty()
                             where sl.User_Id == userid && sl.Select_Id == Convert.ToInt32(selectid)
                             select new
                             {
                                 sl.Select_Id,
                                 Course_Id = sc.Course_Id,
                                 Course_Name = sc.Course_Name,
                                 Start_Time = sc.Start_Time,
                                 End_Time = sc.End_Time,
                                 Course_Hours = sc.Course_Hours,
                                 Course_Price = sc.Course_Price,
                                 Enrollment_Status = se.Enrollment_Status,
                                 Payment_Status = se.Payment_Status,
                                 sl.Select_Time,
                             };

                if (result != null && result.Count() > 0)
                {
                    var qua = result.ToList();

                    if (string.IsNullOrEmpty(qua[0].Enrollment_Status))
                    {
                        Enrollment enrollment = new Enrollment();
                        enrollment.Enrollment_Status = "Applied";
                        enrollment.Course_Id = qua[0].Course_Id;
                        enrollment.User_Id = userid.Value;
                        enrollment.Enrollment_Time = DateTime.Now;
                        enrollment.Payment_Status = "Unpaid";
                        dataContext.Enrollment.Add(enrollment);
                        dataContext.SaveChanges();

                        resData["state"] = 1;
                        resData["msg"] = "Apply Success!";
                    }
                    else
                    {
                        resData["msg"] = "This Course already Apply!";
                    }
                }
                else
                {
                    resData["msg"] = "Select Id does not exist!";
                    
                }
            }
            catch(Exception ex)
            {
                resData["msg"] = ex.Message;
            }

            return resData.ToString();
        }
        public string StudentRefundCourse(string course_id)
        {
            JObject resData = new JObject();
            resData.Add("state", -1);
            resData.Add("msg", "");

            var result = from el in dataContext.Enrollment
                         where el.Course_Id == Convert.ToInt32(course_id)
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return base.RedirectToAction("Index", "MainView");

        }

    }
}
