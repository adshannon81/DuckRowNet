using DuckRowNet.Helpers;
using DuckRowNet.Helpers.Object;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace DuckRowNet.Controllers
{
    public class jsonController : Controller
    {
        // GET: json
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EventList(string id = "")
        {
            Response.AddHeader("Content-Type", "application/json");

            var returnURL = "";
            if (Request.UrlReferrer != null)
            {
                returnURL = Request.UrlReferrer.ToString();
            }


            var callback = Request.Params["callback"];

            DAL db = new DAL();

            var company = "";
            var subCategories = "";
            var instructor = "";
            var active = "";

            if (Request.Params["name"] != null && Request.Params["name"] != "undefined")
            {
                company = Request.Params["name"];
            }
            if (Request.Params["subcategories"] != null && Request.Params["subcategories"] != "undefined")
            {
                subCategories = Request.Params["subcategories"];
            }
            if (Request.Params["instructor"] != null && Request.Params["instructor"] != "undefined")
            {
                instructor = Request.Params["instructor"];
            }
            if (Request.Params["active"] != null && Request.Params["active"] != "undefined")
            {
                active = Request.Params["active"];
            }

            List<string> subCategoryList = new List<string>();
            foreach (string s in subCategories.Split(','))
            {
                if (!String.IsNullOrEmpty(s))
                {
                    subCategoryList.Add(s);
                }
            }
            List<string> instructorList = new List<string>();
            foreach (string s in instructor.Split(','))
            {
                if (!String.IsNullOrEmpty(s))
                {
                    instructorList.Add(s);
                }
            }



            company = Authenticate.ValidateStrict(company);


            if (company != "DuckRow")
            {
                ViewBag.Result = "{\"events\":[";

                var companyURL = db.getCompanyURL(company);

                List<GroupClass> classes = db.selectCurrentPublicClasses(company, subCategoryList, instructorList);
                classes = classes.OrderBy(c => c.NextClass()).ToList();

                if (active == "future-only")
                {
                    classes = classes.Where(c => c.StartDate >= DateTime.Now).ToList();

                }
                else if (active == "running-only")
                {
                    classes = classes.Where(c => c.StartDate < DateTime.Now).ToList();
                }

                var detailView = "";
                if (Request.Params["detail"] != null)
                {
                    detailView = Request.Params["detail"];
                }

                int count = 1;

                if (detailView == "full")
                {
                    foreach (var item in classes)
                    {
                        var image = "";
                        var companyImage = "";
                        var description = "";

                        if (!String.IsNullOrEmpty(item.Image))
                        {
                            image = item.Image;
                        }
                        if (!String.IsNullOrEmpty(item.CompanyImage))
                        {
                            companyImage = item.CompanyImage;
                        }

                        description = Regex.Replace(item.Description, "<[^>]*>", string.Empty);
                        description = Regex.Replace(description, "\\r\\n", string.Empty);
                        if (description.Length >= 85)
                        {
                            description = description.Substring(0, 76) + "......";
                        }

                        ViewBag.Result += "{\"startDate\":\"" + item.StartDate.ToString("dddd, dd MMM") + "\"," +
                            "\"startDay\":\"" + item.StartDate.ToString("dd") + "\"," +
                            "\"startMonth\":\"" + item.StartDate.ToString("MMM") + "\"," +
                            "\"startTime\":\"" + item.StartDate.ToString("HH:mm") + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"nextDate\":\"" + item.NextClass().ToString("ddd, dd MMM") + "\"," +
                            "\"nextTime\":\"" + item.NextClass().ToString("HH:mm") + "\", " + // " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"name\":\"" + item.Name + "\", " +
                            "\"company_image\":\"" + companyImage + "\", " +
                            "\"image\":\"http://duckrow.net" + image.Substring(1).Replace("\\", "/") + "\", " +
                            "\"desc\":\"" + description + "\", " +
                            "\"subCategory\":\"" + item.SubCategoryName + "\", " +
                            "\"cat\":\"cat-" + item.CategoryID.ToString() + "\", " +
                            "\"location\":\"" + item.LocationName + "\", " +
                            "\"classType\":\"" + item.ClassType + "\", " +
                            "\"remainingCapacity\":\"" + item.RemainingCapacity.ToString() + "\", " +
                            "\"id\":\"" + item.ID.ToString() + "\", " +
                            "\"url\":\"http://duckrow.net" + item.Slug + "\"} ";

                        if (count < classes.Count)
                        {
                            ViewBag.Result += ",";
                        }


                    }
                }
                else
                {
                    foreach (var item in classes)
                    {

                        ViewBag.Result += "{\"startDate\":\"" + item.StartDate.ToString("dddd, dd MMM") + "\"," +
                            "\"startDay\":\"" + item.StartDate.ToString("dd") + "\"," +
                            "\"startMonth\":\"" + item.StartDate.ToString("MMM") + "\"," +
                            "\"startTime\":\"" + item.StartDate.ToString("HH:mm") + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"nextDate\":\"" + item.NextClass().ToString("ddd, dd MMM") + "\"," +
                            "\"nextTime\":\"" + item.NextClass().ToString("HH:mm") + "\", " + // + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"name\":\"" + item.Name + "\", " +
                            "\"location\":\"" + item.LocationName + "\", " +
                            "\"remainingCapacity\":\"" + item.RemainingCapacity.ToString() + "\", " +
                            "\"id\":\"" + item.ID.ToString() + "\", " +
                            "\"url\":\"" + item.Slug + "\"} ";

                        if (count < classes.Count)
                        {
                            ViewBag.Result += ",";
                        }


                    }
                }

                //}
                ViewBag.Result += "]}";

                ViewBag.Result = callback + "(" + ViewBag.Result + ")";

            }

            return View();
        }


        public ActionResult InfiniteScroll()
        {
            Response.AddHeader("Content-Type", "application/json");

            var returnURL = "";
            if (Request.UrlReferrer != null)
            {
                returnURL = Request.UrlReferrer.ToString();
            }


            var callback = Request.Params["callback"];

            DAL db = new DAL();

            var company = "";
            var subCategories = "";
            var instructor = "";
            var active = "";

            if (Request.Params["name"] != null && Request.Params["name"] != "undefined")
            {
                company = Request.Params["name"];
            }
            if (Request.Params["subcategories"] != null && Request.Params["subcategories"] != "undefined")
            {
                subCategories = Request.Params["subcategories"];
            }
            if (Request.Params["instructor"] != null && Request.Params["instructor"] != "undefined")
            {
                instructor = Request.Params["instructor"];
            }
            if (Request.Params["active"] != null && Request.Params["active"] != "undefined")
            {
                active = Request.Params["active"];
            }

            List<string> subCategoryList = new List<string>();
            foreach (string s in subCategories.Split(','))
            {
                if (!String.IsNullOrEmpty(s))
                {
                    subCategoryList.Add(s);
                }
            }
            List<string> instructorList = new List<string>();
            foreach (string s in instructor.Split(','))
            {
                if (!String.IsNullOrEmpty(s))
                {
                    instructorList.Add(s);
                }
            }



            company = Authenticate.ValidateStrict(company);


            if (company != "DuckRow")
            {
                ViewBag.Result = "{\"events\":[";

                var companyURL = db.getCompanyURL(company);

                List<GroupClass> classes = db.selectCurrentPublicClasses(company, subCategoryList, instructorList);
                classes = classes.OrderBy(c => c.NextClass()).ToList();

                if (active == "future-only")
                {
                    classes = classes.Where(c => c.StartDate >= DateTime.Now).ToList();

                }
                else if (active == "running-only")
                {
                    classes = classes.Where(c => c.StartDate < DateTime.Now).ToList();
                }

                var detailView = "";
                if (Request.Params["detail"] != null)
                {
                    detailView = Request.Params["detail"];
                }

                int count = 1;

                if (detailView == "full")
                {
                    foreach (var item in classes)
                    {
                        var image = "";
                        var companyImage = "";
                        var description = "";

                        if (!String.IsNullOrEmpty(item.Image))
                        {
                            image = item.Image;
                        }
                        if (!String.IsNullOrEmpty(item.CompanyImage))
                        {
                            companyImage = item.CompanyImage;
                        }

                        description = Regex.Replace(item.Description, "<[^>]*>", string.Empty);
                        description = Regex.Replace(description, "\\r\\n", string.Empty);
                        if (description.Length >= 85)
                        {
                            description = description.Substring(0, 76) + "......";
                        }

                        ViewBag.Result += "{\"startDate\":\"" + item.StartDate.ToString("dddd, dd MMM") + "\"," +
                            "\"startDay\":\"" + item.StartDate.ToString("dd") + "\"," +
                            "\"startMonth\":\"" + item.StartDate.ToString("MMM") + "\"," +
                            "\"startTime\":\"" + item.StartDate.ToString("HH:mm") + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"nextDate\":\"" + item.NextClass().ToString("ddd, dd MMM") + "\"," +
                            "\"nextTime\":\"" + item.NextClass().ToString("HH:mm") + "\", " + // " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"name\":\"" + item.Name + "\", " +
                            "\"company_image\":\"" + companyImage + "\", " +
                            "\"image\":\"http://duckrow.net" + image.Substring(1).Replace("\\", "/") + "\", " +
                            "\"desc\":\"" + description + "\", " +
                            "\"subCategory\":\"" + item.SubCategoryName + "\", " +
                            "\"cat\":\"cat-" + item.CategoryID.ToString() + "\", " +
                            "\"location\":\"" + item.LocationName + "\", " +
                            "\"classType\":\"" + item.ClassType + "\", " +
                            "\"remainingCapacity\":\"" + item.RemainingCapacity.ToString() + "\", " +
                            "\"id\":\"" + item.ID.ToString() + "\", " +
                            "\"url\":\"http://duckrow.net" + item.Slug + "\"} ";

                        if (count < classes.Count)
                        {
                            ViewBag.Result += ",";
                        }


                    }
                }
                else
                {
                    foreach (var item in classes)
                    {

                        ViewBag.Result += "{\"startDate\":\"" + item.StartDate.ToString("dddd, dd MMM") + "\"," +
                            "\"startDay\":\"" + item.StartDate.ToString("dd") + "\"," +
                            "\"startMonth\":\"" + item.StartDate.ToString("MMM") + "\"," +
                            "\"startTime\":\"" + item.StartDate.ToString("HH:mm") + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"nextDate\":\"" + item.NextClass().ToString("ddd, dd MMM") + "\"," +
                            "\"nextTime\":\"" + item.NextClass().ToString("HH:mm") + "\", " + // + " - " + item.EndDate.ToString("HH:mm") + "\", " +
                            "\"name\":\"" + item.Name + "\", " +
                            "\"location\":\"" + item.LocationName + "\", " +
                            "\"remainingCapacity\":\"" + item.RemainingCapacity.ToString() + "\", " +
                            "\"id\":\"" + item.ID.ToString() + "\", " +
                            "\"url\":\"" + item.Slug + "\"} ";

                        if (count < classes.Count)
                        {
                            ViewBag.Result += ",";
                        }


                    }
                }

                //}
                ViewBag.Result += "]}";

                ViewBag.Result = callback + "(" + ViewBag.Result + ")";

            }

            return View();
        }

        // GET: json
        public ActionResult Menu(string id = "")
        {
            var menuItem = id;
            List<SelectListItem> menuList = new List<SelectListItem>();

            if (menuItem == "Category")
            {
                DAL db = new DAL();

                foreach (var cType in db.getCategoryList("DuckRow"))
                {
                    if (cType.Name != "Appointment" && cType.Name != "Advert")
                    {
                        menuList.Add(new SelectListItem { Value = cType.ID.ToString(), Text = cType.Name });
                    }
                }

                //return Json(menuList, JsonRequestBehavior.AllowGet);
            }
            else if (menuItem == "SubCategory")
            {
                var categoryID = Request.Params["CategoryID"];
                DAL db = new DAL();

                foreach (var cType in db.getSubCategoryList("DuckRow", Convert.ToInt16(categoryID)))
                {
                    if (cType.Name != "Appointment" && cType.Name != "Advert")
                    {
                        menuList.Add(new SelectListItem { Value = cType.ID.ToString(), Text = cType.Name });
                    }
                }
            }

            var jsonSerialiser = new JavaScriptSerializer();
            ViewBag.Result = jsonSerialiser.Serialize(menuList);


            return View();
        }

        public ActionResult Function(string id = "")
        {
            var userID = User.Identity.GetUserId();
            CompanyDetails companyDetails = Authenticate.Validate(id);

            ViewBag.Result = false;
            if (HttpContext.User.Identity.IsAuthenticated && companyDetails != null && Authenticate.IsUserInRole(companyDetails.Name, "admin"))
            {
                DAL db = new DAL();

                if (Request.Params["function"] == "updateRole")
                {
                    if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
                    {
                        var updateUserID = Request.Params["user"];
                        var role = Request.Params["role"];
                        ViewBag.Result = db.updateRole(updateUserID, role, companyDetails.Name);
                    }
                }
                if (Request.Params["function"] == "removeuser")
                {
                    if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
                    {
                        var updateUserID = Request.Params["user"];
                        ViewBag.Result = db.deleteRoles(updateUserID, companyDetails.Name);
                    }

                }
                if (Request.Params["function"] == "confirmRequest")
                {
                    if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
                    {
                        var guid = Request.Params["guid"];
                        var booking = db.getBookingDetails(guid);
                        var message = "";

                        if (db.confirmBookingID(guid))
                        {
                            message = "";
                            //
                            GroupClass classItem = db.selectClassDetail(booking.ElementAt(0).ClassID.ToString());
                            PersonDetails clientDetails = db.getUserDetails(booking.ElementAt(0).UserID.ToString(), booking.ElementAt(0).UserType);

                            message = Email.SendReservationConfirmed(clientDetails.FirstName, clientDetails.LastName,
                                    clientDetails.Email, booking.ElementAt(0).UserType, classItem, classItem.Name,
                                    classItem.StartDate, classItem.Repeated.ToString(),
                                    classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                    companyDetails.Name, comments: "");

                            ViewBag.Result = true;

                        }
                    }

                }
                if (Request.Params["function"] == "cancelRequest")
                {
                    if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
                    {
                        var guid = Request.Params["guid"];
                        var booking = db.getBookingDetails(guid);
                        var message = "";

                        if (db.cancelBookingID(guid))
                        {
                            message = "";
                            //
                            GroupClass classItem = db.selectClassDetail(booking.ElementAt(0).ClassID.ToString());
                            PersonDetails clientDetails = db.getUserDetails(booking.ElementAt(0).UserID.ToString(), booking.ElementAt(0).UserType);

                            message = Email.SendReservationCancelled(classItem, clientDetails.FirstName, clientDetails.LastName,
                                    clientDetails.Email, classItem.ID.ToString(), classItem.Name,
                                    classItem.StartDate.ToString(), classItem.Repeated.ToString(),
                                    classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                    companyDetails.Name);

                            ViewBag.Result = true;

                        }
                    }

                }

                if (Request.Params["function"] == "updateAttendee")
                {
                    if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
                    {
                        var details = Request.Params["details"];
                        var val = Request.Params["val"];
                        //should be format paid_date_user
                        try
                        {
                            var detail = details.Split('_');
                            if (detail[0] == "paid")
                            {
                                if (detail[1] == "all")
                                {
                                    ViewBag.Result = db.clientPaid(detail[2], Convert.ToBoolean(val));
                                }
                            }
                            else if (detail[0] == "cancelled")
                            {
                                if (detail[1] == "all")
                                {
                                    ViewBag.Result = db.clientCancelled(detail[2], Convert.ToBoolean(val));
                                }
                                else
                                {
                                    //format yyyyMMddHHssMM
                                    DateTime bookingDate = new DateTime(Convert.ToInt16(detail[1].Substring(0, 4)),
                                        Convert.ToInt16(detail[1].Substring(4, 2)),
                                        Convert.ToInt16(detail[1].Substring(6, 2)),
                                        Convert.ToInt16(detail[1].Substring(8, 2)),
                                        Convert.ToInt16(detail[1].Substring(10, 2)),
                                        Convert.ToInt16(detail[1].Substring(12, 2)));
                                    var bookingID = db.getBookingID(detail[2].ToString(), bookingDate);
                                    if (bookingID == 0)
                                    {
                                        //throw.Exception;
                                    }
                                    ViewBag.Result = db.clientSingleCancelled(bookingID.ToString(), Convert.ToBoolean(val));
                                }
                            }




                        }
                        catch (Exception ex)
                        {


                        }
                    }
                }
            }
            else
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    ViewBag.Result = "login";
                }
                else if (Request.Params["task"] == "addfavourite")
                {
                    DAL db = new DAL();
                    var classID = Request.Params["id"];
                    bool success = db.AddFavourite(userID, classID);
                    if (success)
                    {
                        ViewBag.Result = "Added";
                    }

                }
                else if (Request.Params["task"] == "removefavourite")
                {
                    DAL db = new DAL();
                    var classID = Request.Params["id"];
                    bool success = db.RemoveFavourite(userID, classID);
                    if (success)
                    {
                        ViewBag.Result = "Removed";
                    }
                }
                else if (Request.Params["task"] == "claimOwnership")
                {
                    DAL db = new DAL();
                    var companyName = Request.Params["company"];
                    bool success = Email.sendClaimOwnership(HttpContext.User.Identity.GetUserId(), companyName);
                    if (success)
                    {
                        ViewBag.Result = "Request Sent";
                    }
                }

            }
            return View();
        }

        [HttpPostAttribute]
        public ActionResult Mail(string id = "")
        {
            var name = Request.Params["Name"];
            var toEmail = Request.Params["Email"];
            var subject = Request.Params["Subject"];
            var message = Request.Params["Message"];
            var issue = "";
            var fromEmail = Request.Params["FromEmail"]; ;
            if (Request.Params["Issue"] != null)
            {
                issue = Request.Params["Issue"];
            }
            if (Request.Params["Issue"] != null)
            {
                issue = Request.Params["Issue"];
            }

            ViewBag.Result = "Success";


            DAL db = new DAL();

            toEmail = Request.Form["emailTo"];
            fromEmail = Request.Form["emailFrom"];
            subject = Request.Form["emailSubject"];
            message = Request.Form["emailBody"];
            var company = Request.Form["company"];
            CompanyDetails companyDetails = db.getCompanyDetails(company);

            string attachment = "";

            if (Authenticate.Admin(companyDetails.Name))
            {

                if (Request.Files.Count > 0)
                {
                    var aFile = Request.Files[0];
                    attachment = Server.MapPath(@"~\Images\ClassImages\" + companyDetails.Name + " - " +
                        Path.GetFileNameWithoutExtension(aFile.FileName) + DateTime.Now.ToString(" dd-MM-yyyy") + Path.GetExtension(aFile.FileName));
                    aFile.SaveAs(attachment);
                }

                IEnumerable<string> attachments = new string[] { attachment };

                //check is admin for company
                if (!Email.SendClientEmail(companyDetails, name, toEmail, fromEmail, subject, message, attachments))
                {
                    ViewBag.Result = "Fail";
                }

            }
            else {

                if (!Email.SendContactEmail(name, fromEmail, toEmail, subject, message))
                {
                    ViewBag.Result = "Fail";
                }

            }

            return View();
        }
    }
}