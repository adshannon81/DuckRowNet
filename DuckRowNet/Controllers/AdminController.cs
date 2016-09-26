using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DuckRowNet.Helpers.Object;
using DuckRowNet.Helpers;
using System.IO;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;

namespace DuckRowNet.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }



        [Authorize]
        public ActionResult Create(string companyName = "")
        {
            Session["copyClass"] = null;

            DAL db = new DAL();

            if (companyName == "")
            {
                if (String.IsNullOrEmpty(ViewBag.Companyname))
                {
                    
                    PersonDetails currentUser = new PersonDetails();

                    currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
                    companyName = currentUser.CompanyName;
                }
                else
                {
                    companyName = ViewBag.CompanyName;
                }
            }

            if (String.IsNullOrEmpty(companyName))
            {
                return RedirectToAction("Register", "Setup");
            }

            ViewBag.NoLocations = true;
            if(db.getLocationList(companyName).Count() > 0)
            {
                ViewBag.NoLocations = false;
            }

            ViewBag.CompanyDetails = Authenticate.Validate(companyName);

            if (!Authenticate.IsUserInRole(ViewBag.CompanyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
                //return RedirectToAction("Login", "Account", new { returnUrl = HttpContext.Request.Url.ToString() });
            }

            return View();
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult CreateClass(string companyName = "")
        {
            DAL db = new DAL();

            PersonDetails currentUser = new PersonDetails();

            ViewBag.isValid = true;
            if (companyName == "")
            {
                if (String.IsNullOrEmpty(ViewBag.Companyname))
                {
                    currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
                    companyName = currentUser.CompanyName;
                }
                else
                {
                    companyName = ViewBag.CompanyName;
                }
            }

            ViewBag.CompanyDetails = Authenticate.Validate(companyName);
            ViewBag.CompanyName = companyName;

            if (ViewBag.CompanyDetails.Name == "")
            {
                return RedirectToAction("Company", "Setup");
            }
            if (!Authenticate.IsUserInRole(ViewBag.CompanyDetails.Name, "editor"))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = HttpContext.Request.Url.ToString() });
            }
            Authenticate.ValidSubscription(ViewBag.CompanyDetails);


            GroupClass gClass = new GroupClass();

            GroupClass copyClass = (GroupClass)Session["copyClass"];
            if (copyClass != null)
            {
                gClass.UpdateClass(copyClass);
            }

            gClass.IsCourse = false;
            gClass.Company = companyName;

            DateTime aDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            if (!String.IsNullOrEmpty(Request.QueryString["date"]))
            {
                var tempDate = Request.QueryString["date"];
                aDate = Convert.ToDateTime(Request.QueryString["date"]);

                gClass.StartDate = aDate;
            }

            var addLocation = "../Admin/AddLocation/" + ViewBag.CompanyDetails.Name;
            var addType = "../Admin/AddType/" + ViewBag.CompanyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;
            string[] adminIDList = new string[5];
            adminIDList[0] = User.Identity.GetUserId();// WebSecurity.CurrentUserId; // HttpContext.User.Identity;

            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";


            if (Session["tempClass"] != null)
            {
                gClass = (GroupClass)Session["tempClass"];
            }

            ViewBag.GroupClass = gClass;
            ViewBag.Locations = db.getLocationList(ViewBag.CompanyDetails.Name);
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Admins = db.getUsers(ViewBag.CompanyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateClass(DuckRowNet.Helpers.Object.GroupClass gClass, string companyName = "")
        {
            CompanyDetails companyDetails = new CompanyDetails();
            if (companyName == "")
            {
                companyDetails = Authenticate.Validate(Request.Form["company"]);
            }
            else {

                companyDetails = Authenticate.Validate(companyName);
            }

            if (companyDetails.Name == "")
            {

                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
                //Response.Redirect("~/AdminOnly/" + companyDetails.Name, false);
            }
            Authenticate.ValidSubscription(companyDetails);

            var adminIDList = Request.Form["adminUsers"].ToString().Split(',');
            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";

            var addLocation = "../Admin/AddLocation/" + companyDetails.Name;
            var addType = "../Admin/AddType/" + companyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;

            //GroupClass gClass = new GroupClass();
            gClass.IsCourse = false;
            DateTime aDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            if (!String.IsNullOrEmpty(Request.QueryString["date"]))
            {
                var tempDate = Request.QueryString["date"];
                aDate = Convert.ToDateTime(Request.QueryString["date"]);

                gClass.StartDate = aDate;
            }

            if (ViewBag.gClass != null)
            {
                gClass = ViewBag.gClass;
            }

            Boolean validateForm = false;
            if (Request.Form["validateForm"].ToString() == "True")
            {
                validateForm = true;
            }

            //Error Checks
            if (validateForm && String.IsNullOrEmpty(Request.Form["className"].ToString()))
            {
                nameErrorMessage = "You must specify a class name.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classStartDate"].ToString()))
            {
                dateErrorMessage = "You must specify a start date.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classLocation"].ToString()))
            {
                locationErrorMessage = "You must specify a location.";
                isValid = false;
            }

            bool IsPrivate = true;
            if (Request.Form["classPrivate"] == "no")
            {
                IsPrivate = false;
            }

            bool AllowDropIn = true;
            if (Request.Form["allowDropIn"] == "no")
            {
                AllowDropIn = false;
            }

            bool AllowPayment = false;
            if (Request.Form["classAllowPayment"] == "yes")
            {
                AllowPayment = true;
            }

            bool AbsorbFee = true;
            if (Request.Form["classAbsorbFee"] == "no" && AllowPayment)
            {
                AbsorbFee = false;
            }

            bool AllowReservation = false;
            if (Request.Form["classAllowReservation"] == "yes")
            {
                AllowReservation = true;
            }

            bool AutoReservation = false;
            if (Request.Form["classAutoReservation"] == "yes" && AllowReservation)
            {
                AutoReservation = true;
            }


            string repeatDays = "";
            if (Request.Form["Monday"] == "Monday")
            {
                repeatDays = "Monday";
            }
            if (Request.Form["Tuesday"] == "Tuesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Tuesday";
            }
            if (Request.Form["Wednesday"] == "Wednesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Wednesday";
            }
            if (Request.Form["Thursday"] == "Thursday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Thursday";
            }
            if (Request.Form["Friday"] == "Friday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Friday";
            }
            if (Request.Form["Saturday"] == "Saturday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Saturday";
            }
            if (Request.Form["Sunday"] == "Sunday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Sunday";
            }

            gClass = new GroupClass(
                    Guid.NewGuid(),
                    companyDetails.Name,
                    companyDetails.ImagePath,
                    Request.Form["className"],
                    Request.Form["classCategory"],
                    "",
                    Request.Form["classSubCategory"].Split('_').First(),
                    "",
                    Convert.ToInt16(Request.Form["classDuration"].Replace(" minutes", "")),
                    Request.Form["classRepeat"],
                    Convert.ToInt16(Request.Form["repeatEvery"]),
                    repeatDays,
                    1,
                    Convert.ToDateTime(Request.Form["classStartDate"] + " " + Request.Form["hourStart"] + ":" + Request.Form["minuteStart"]),
                    Convert.ToInt16(Request.Form["classMaxCapacity"]),
                    Convert.ToDouble(Request.Form["classCostOfCourse"]),
                    Convert.ToDouble(Request.Form["classCostOfSession"]),
                    adminIDList, //WebSecurity.CurrentUserId.ToString(), // db.getUserID(userEmail),
                    "", //admin user firstname lastname. not needed at this time so no point looking it up.
                    Convert.ToInt16(Request.Form["classLocation"]),
                    "",
                    "",
                    "",
                    "",
                    Request.Form["classDescription"],
                    gClass.Image,
                    gClass.IsCourse,
                    AllowDropIn,
                    AbsorbFee,
                    AllowReservation,
                    AutoReservation,
                    AllowPayment,
                    IsPrivate,
                    "Class"
                );

            gClass.EndDate = Convert.ToDateTime(Request.Form["classEndDate"] + " 23:59:59");
            gClass.CalculateNumberOfLessions();


            if (gClass.Repeated == Functions.Repeat.never)
            {
                gClass.NumberOfLessons = 1;
            }


            if (Request.Form["validateForm"].ToString() == "addLocation")
            {
                Session["tempClass"] = gClass;
                Response.Redirect(addLocation, false);
            }
            else
            {
                Session["tempClass"] = null;
            }

            DAL db = new DAL();

            if (validateForm && isValid)
            {
                //check all users are free
                foreach (string admin in adminIDList)
                {

                    if (!db.checkClientIsFree(gClass, admin, companyDetails.Name))
                    {
                        isValid = false;
                        createErrorMessage = admin + " already has a booking at this time.";
                        break;
                    }
                }

                if (isValid)
                {
                    WebImage photo = WebImage.GetImageFromRequest();

                    if (photo != null)
                    {
                        gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                        gClass.Image = HttpUtility.HtmlEncode(gClass.SubCategoryName.Replace(" ", "-")) + "-" + Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        gClass.Image = @"~\Images\ClassImages\" + gClass.Image;

                        //photo.Resize(width: 280, height: 150, preserveAspectRatio: true, preventEnlarge: false);
                        photo.Save(gClass.Image, null, false);
                        Functions.ResizeImage(gClass.Image, 350, 250, false);
                        Session["tempClass"] = gClass;
                    }
                    else if (!String.IsNullOrEmpty(Request.Form["ImageName"]))
                    {
                        gClass.Image = Request.Form["ImageName"];
                    }

                    //create class
                    try
                    {
                        gClass.ID = Guid.NewGuid();
                        var result = gClass.CreateClass();

                        if (!result.StartsWith("Error"))
                        {
                            gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                            //redirect if success          
                            //Response.Redirect("~/Classes/" + companyDetails.Name + "/" + gClass.ID.ToString(), false);
                            
                            return RedirectToAction("Index", "Classes", new { companyName = companyDetails.Name,
                                subCategory = Functions.convertToSlug(gClass.SubCategoryName),
                                year = gClass.StartDate.ToString("yyyy"),
                                month = gClass.StartDate.ToString("MM"),
                                day = gClass.StartDate.ToString("dd"),
                                classSlug = Functions.convertToSlug(gClass.Name) });

                        }
                        else
                        {
                            isValid = false;
                            createErrorMessage = result;
                            //createErrorMessage = "An error occurred writing to the database. Please try again. ";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        isValid = false;
                        createErrorMessage = ex.ToString();
                        Logger.LogWarning("Admin/CreateClass", "CreateClass", companyDetails.Name + ":" + gClass.Name, ex.Message, ex);

                    }
                }
            }

            ViewBag.CompanyDetails = companyDetails;
            ViewBag.GroupClass = gClass;
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Locations = db.getLocationList(companyDetails.Name);
            ViewBag.Admins = db.getUsers(companyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.isValid = isValid;
            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View(gClass);
        }


        [Authorize]
        [ValidateInput(false)]
        public ActionResult CreateCourse(string companyName = "")
        {
            DAL db = new DAL();

            PersonDetails currentUser = new PersonDetails();

            ViewBag.isValid = true;
            if (companyName == "")
            {
                if (String.IsNullOrEmpty(ViewBag.Companyname))
                {
                    currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
                    companyName = currentUser.CompanyName;
                }
                else
                {
                    companyName = ViewBag.CompanyName;
                }
            }

            ViewBag.CompanyDetails = Authenticate.Validate(companyName);
            ViewBag.CompanyName = companyName;

            if (ViewBag.CompanyDetails.Name == "")
            {
                return RedirectToAction("Company", "Setup");
            }
            if (!Authenticate.IsUserInRole(ViewBag.CompanyDetails.Name, "editor"))
            {
                return RedirectToAction("Login", "Account");
            }
            Authenticate.ValidSubscription(ViewBag.CompanyDetails);


            GroupClass gClass = new GroupClass();

            GroupClass copyClass = (GroupClass)Session["copyClass"];
            if (copyClass != null)
            {
                gClass = copyClass;
            }

            gClass.IsCourse = false;
            gClass.Company = companyName;

            DateTime aDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            if (!String.IsNullOrEmpty(Request.QueryString["date"]))
            {
                var tempDate = Request.QueryString["date"];
                aDate = Convert.ToDateTime(Request.QueryString["date"]);

                gClass.StartDate = aDate;
            }

            var addLocation = "../Admin/AddLocation/" + ViewBag.CompanyDetails.Name;
            var addType = "../Admin/AddType/" + ViewBag.CompanyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;
            string[] adminIDList = new string[5];
            adminIDList[0] = User.Identity.GetUserId();// WebSecurity.CurrentUserId; // HttpContext.User.Identity;

            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";


            if (Session["tempClass"] != null)
            {
                gClass = (GroupClass)Session["tempClass"];
            }

            ViewBag.GroupClass = gClass;
            ViewBag.Locations = db.getLocationList(ViewBag.CompanyDetails.Name);
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Admins = db.getUsers(ViewBag.CompanyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateCourse(DuckRowNet.Helpers.Object.GroupClass gClass, string companyName = "")
        {
            CompanyDetails companyDetails = new CompanyDetails();
            if (companyName == "")
            {
                companyDetails = Authenticate.Validate(Request.Form["company"]);
            }
            else {

                companyDetails = Authenticate.Validate(companyName);
            }

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }
            Authenticate.ValidSubscription(companyDetails);

            var adminIDList = Request.Form["adminUsers"].ToString().Split(',');
            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";

            var addLocation = "../Admin/AddLocation/" + companyDetails.Name;
            var addType = "../Admin/AddType/" + companyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;

            //GroupClass gClass = new GroupClass();
            gClass.IsCourse = true;
            DateTime aDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            if (!String.IsNullOrEmpty(Request.QueryString["date"]))
            {
                var tempDate = Request.QueryString["date"];
                aDate = Convert.ToDateTime(Request.QueryString["date"]);

                gClass.StartDate = aDate;
            }

            if (ViewBag.gClass != null)
            {
                gClass = ViewBag.gClass;
            }

            Boolean validateForm = false;
            if (Request.Form["validateForm"].ToString() == "True")
            {
                validateForm = true;
            }

            //Error Checks
            if (validateForm && String.IsNullOrEmpty(Request.Form["className"].ToString()))
            {
                nameErrorMessage = "You must specify a class name.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classStartDate"].ToString()))
            {
                dateErrorMessage = "You must specify a start date.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classLocation"].ToString()))
            {
                locationErrorMessage = "You must specify a location.";
                isValid = false;
            }

            bool IsPrivate = true;
            if (Request.Form["classPrivate"] == "no")
            {
                IsPrivate = false;
            }

            bool AllowDropIn = true;
            if (Request.Form["allowDropIn"] == "no")
            {
                AllowDropIn = false;
            }

            bool AllowPayment = false;
            if (Request.Form["classAllowPayment"] == "yes")
            {
                AllowPayment = true;
            }

            bool AbsorbFee = true;
            if (Request.Form["classAbsorbFee"] == "no" && AllowPayment)
            {
                AbsorbFee = false;
            }

            bool AllowReservation = false;
            if (Request.Form["classAllowReservation"] == "yes")
            {
                AllowReservation = true;
            }

            bool AutoReservation = false;
            if (Request.Form["classAutoReservation"] == "yes" && AllowReservation)
            {
                AutoReservation = true;
            }


            string repeatDays = "";
            if (Request.Form["Monday"] == "Monday")
            {
                repeatDays = "Monday";
            }
            if (Request.Form["Tuesday"] == "Tuesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Tuesday";
            }
            if (Request.Form["Wednesday"] == "Wednesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Wednesday";
            }
            if (Request.Form["Thursday"] == "Thursday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Thursday";
            }
            if (Request.Form["Friday"] == "Friday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Friday";
            }
            if (Request.Form["Saturday"] == "Saturday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Saturday";
            }
            if (Request.Form["Sunday"] == "Sunday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Sunday";
            }

            gClass = new GroupClass(
                    Guid.NewGuid(),
                    companyDetails.Name,
                    companyDetails.ImagePath,
                    Request.Form["className"],
                    Request.Form["classCategory"],
                    "",
                    Request.Form["classSubCategory"].Split('_').First(),
                    "",
                    Convert.ToInt16(Request.Form["classDuration"].Replace(" minutes", "")),
                    Request.Form["classRepeat"],
                    Convert.ToInt16(Request.Form["repeatEvery"]),
                    repeatDays,
                    Convert.ToInt16(Request.Form["numberOfLessons"]),
                    Convert.ToDateTime(Request.Form["classStartDate"] + " " + Request.Form["hourStart"] + ":" + Request.Form["minuteStart"]),
                    Convert.ToInt16(Request.Form["classMaxCapacity"]),
                    Convert.ToDouble(Request.Form["classCostOfCourse"]),
                    Convert.ToDouble(Request.Form["classCostOfSession"]),
                    adminIDList, //WebSecurity.CurrentUserId.ToString(), // db.getUserID(userEmail),
                    "", //admin user firstname lastname. not needed at this time so no point looking it up.
                    Convert.ToInt16(Request.Form["classLocation"]),
                    "",
                    "",
                    "",
                    "",
                    Request.Form["classDescription"],
                    gClass.Image,
                    gClass.IsCourse,
                    AllowDropIn,
                    AbsorbFee,
                    AllowReservation,
                    AutoReservation,
                    AllowPayment,
                    IsPrivate,
                    "Course"
                );

            //gClass.EndDate = Convert.ToDateTime(Request.Form["classEndDate"] + " 23:59:59");
            //gClass.CalculateNumberOfLessions();


            if (gClass.Repeated == Functions.Repeat.never)
            {
                gClass.NumberOfLessons = 1;
            }


            if (Request.Form["validateForm"].ToString() == "addLocation")
            {
                Session["tempClass"] = gClass;
                Response.Redirect(addLocation, false);
            }
            else
            {
                Session["tempClass"] = null;
            }

            DAL db = new DAL();

            if (validateForm && isValid)
            {
                //check all users are free
                foreach (string admin in adminIDList)
                {

                    if (!db.checkClientIsFree(gClass, admin, companyDetails.Name))
                    {
                        isValid = false;
                        createErrorMessage = admin + " already has a booking at this time.";
                        break;
                    }
                }

                if (isValid)
                {
                    WebImage photo = WebImage.GetImageFromRequest();

                    if (photo != null)
                    {
                        gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                        gClass.Image = HttpUtility.HtmlEncode(gClass.SubCategoryName.Replace(" ", "-")) + "-" + Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        gClass.Image = @"~\Images\ClassImages\" + gClass.Image;

                        //photo.Resize(width: 280, height: 150, preserveAspectRatio: true, preventEnlarge: false);
                        photo.Save(gClass.Image, null, false);
                        Functions.ResizeImage(gClass.Image, 350, 250, false);
                        Session["tempClass"] = gClass;
                    }
                    else if (!String.IsNullOrEmpty(Request.Form["ImageName"]))
                    {
                        gClass.Image = Request.Form["ImageName"];
                    }

                    //create class
                    try
                    {
                        gClass.ID = Guid.NewGuid();
                        var result = gClass.CreateClass();

                        if (!result.StartsWith("Error"))
                        {
                            gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                            //redirect if success          
                            return RedirectToAction("Index", "Classes", new { companyName = companyDetails.Name, classSubCategory = gClass.SubCategoryName.Replace(" ", "-"), classID = gClass.ID.ToString() });

                        }
                        else
                        {
                            isValid = false;
                            createErrorMessage = result;
                            //createErrorMessage = "An error occurred writing to the database. Please try again. ";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        isValid = false;
                        createErrorMessage = ex.ToString();
                        Logger.LogWarning("Admin/CreateClass", "CreateClass", companyDetails.Name + ":" + gClass.Name, ex.Message, ex);

                    }
                }
            }

            ViewBag.CompanyDetails = companyDetails;
            ViewBag.GroupClass = gClass;
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Locations = db.getLocationList(companyDetails.Name);
            ViewBag.Admins = db.getUsers(companyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.isValid = isValid;
            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View(gClass);
        }


        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit(string companyName = "", string classID = "")
        {
            ViewBag.isValid = true;
            ViewBag.CompanyDetails = Authenticate.Validate(companyName);

            if (ViewBag.CompanyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(ViewBag.CompanyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }
            Authenticate.ValidSubscription(ViewBag.CompanyDetails);
            DAL db = new DAL();

            GroupClass gClass_original = db.selectClassDetail(classID);
            GroupClass gClass = new GroupClass(gClass_original);

            var addLocation = "../Admin/AddLocation/" + ViewBag.CompanyDetails.Name;
            var addType = "../Admin/AddType/" + ViewBag.CompanyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;
            string[] adminIDList = new string[5];
            adminIDList[0] = User.Identity.GetUserId();// WebSecurity.CurrentUserId; // HttpContext.User.Identity;

            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";

            ViewBag.GroupClass_Original = gClass_original;
            ViewBag.GroupClass = gClass;
            ViewBag.Locations = db.getLocationList(ViewBag.CompanyDetails.Name);
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Admins = db.getUsers(ViewBag.CompanyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View(gClass_original);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(DuckRowNet.Helpers.Object.GroupClass gClass, string companyName = "", string classID = "")
        {
            CompanyDetails companyDetails = Authenticate.Validate(companyName);

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }
            Authenticate.ValidSubscription(companyDetails);

            var adminIDList = Request.Form["adminUsers"].ToString().Split(',');

            //Validation
            var isValid = true;
            var nameErrorMessage = "";
            var dateErrorMessage = "";
            var locationErrorMessage = "";
            var createErrorMessage = "";
            var repeatErrorMessage = "";

            var addLocation = "../Admin/AddLocation/" + companyDetails.Name;
            var addType = "../Admin/AddType/" + companyDetails.Name;
            var userEmail = HttpContext.User.Identity.Name;

            //GroupClass gClass = new GroupClass();
            //gClass.IsCourse = false;

            DAL db = new DAL();
            GroupClass gClass_original = db.selectClassDetail(classID);

            DateTime aDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            if (!String.IsNullOrEmpty(Request.QueryString["date"]))
            {
                var tempDate = Request.QueryString["date"];
                aDate = Convert.ToDateTime(Request.QueryString["date"]);

                gClass.StartDate = aDate;
            }


            Boolean validateForm = false;
            if (Request.Form["validateForm"].ToString() == "True")
            {
                validateForm = true;
            }

            //Error Checks
            if (validateForm && String.IsNullOrEmpty(Request.Form["className"].ToString()))
            {
                nameErrorMessage = "You must specify a class name.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classStartDate"].ToString()))
            {
                dateErrorMessage = "You must specify a start date.";
                isValid = false;
            }
            if (validateForm && String.IsNullOrEmpty(Request.Form["classLocation"].ToString()))
            {
                locationErrorMessage = "You must specify a location.";
                isValid = false;
            }

            bool IsPrivate = true;
            if (Request.Form["classPrivate"] == "no")
            {
                IsPrivate = false;
            }

            bool AllowDropIn = true;
            if (Request.Form["allowDropIn"] == "no")
            {
                AllowDropIn = false;
            }

            bool AllowPayment = false;
            if (Request.Form["classAllowPayment"] == "yes")
            {
                AllowPayment = true;
            }

            bool AbsorbFee = true;
            if (Request.Form["classAbsorbFee"] == "no" && AllowPayment)
            {
                AbsorbFee = false;
            }

            bool AllowReservation = false;
            if (Request.Form["classAllowReservation"] == "yes")
            {
                AllowReservation = true;
            }

            bool AutoReservation = false;
            if (Request.Form["classAutoReservation"] == "yes" && AllowReservation)
            {
                AutoReservation = true;
            }


            string repeatDays = "";
            if (Request.Form["Monday"] == "Monday")
            {
                repeatDays = "Monday";
            }
            if (Request.Form["Tuesday"] == "Tuesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Tuesday";
            }
            if (Request.Form["Wednesday"] == "Wednesday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Wednesday";
            }
            if (Request.Form["Thursday"] == "Thursday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Thursday";
            }
            if (Request.Form["Friday"] == "Friday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Friday";
            }
            if (Request.Form["Saturday"] == "Saturday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Saturday";
            }
            if (Request.Form["Sunday"] == "Sunday")
            {
                if (repeatDays != "")
                {
                    repeatDays += "|";
                }
                repeatDays += "Sunday";
            }

            var numberOfLessons = 1;
            if (gClass_original.ClassType == "Course")
            {
                numberOfLessons = Convert.ToInt16(Request.Form["numberOfLessons"]);
            }

            gClass = new GroupClass(
                    gClass_original.ID,
                    companyDetails.Name,
                    companyDetails.ImagePath,
                    Request.Form["className"],
                    Request.Form["classCategory"],
                    "",
                    Request.Form["classSubCategory"].Split('_').First(),
                    "",
                    Convert.ToInt16(Request.Form["classDuration"].Replace(" minutes", "")),
                    Request.Form["classRepeat"],
                    Convert.ToInt16(Request.Form["repeatEvery"]),
                    repeatDays,
                    numberOfLessons,
                    Convert.ToDateTime(Request.Form["classStartDate"] + " " + Request.Form["hourStart"] + ":" + Request.Form["minuteStart"]),
                    Convert.ToInt16(Request.Form["classMaxCapacity"]),
                    Convert.ToDouble(Request.Form["classCostOfCourse"]),
                    Convert.ToDouble(Request.Form["classCostOfSession"]),
                    adminIDList, //WebSecurity.CurrentUserId.ToString(), // db.getUserID(userEmail),
                    "", //admin user firstname lastname. not needed at this time so no point looking it up.
                    Convert.ToInt16(Request.Form["classLocation"]),
                    "",
                    "",
                    "",
                    "",
                    Request.Form["classDescription"],
                    gClass.Image,
                    gClass.IsCourse,
                    AllowDropIn,
                    AbsorbFee,
                    AllowReservation,
                    AutoReservation,
                    AllowPayment,
                    IsPrivate,
                    gClass_original.ClassType
                );

            if (gClass_original.ClassType == "Class")
            {
                gClass.EndDate = Convert.ToDateTime(Request.Form["classEndDate"] + " 23:59:59");
                gClass.CalculateNumberOfLessions();
            }

            if (gClass.Repeated == Functions.Repeat.never)
            {
                gClass.NumberOfLessons = 1;
            }


            if (Request.Form["validateForm"].ToString() == "addLocation")
            {
                Session["tempClass"] = gClass;
                Response.Redirect(addLocation, false);
            }
            else
            {
                Session["tempClass"] = null;
            }

            if (validateForm && isValid)
            {
                //check all users are free
                foreach (string admin in adminIDList)
                {

                    if (!db.checkClientIsFree(gClass, admin, companyDetails.Name))
                    {
                        isValid = false;
                        createErrorMessage = admin + " already has a booking at this time.";
                        break;
                    }
                }

                if (isValid)
                {
                    WebImage photo = WebImage.GetImageFromRequest();

                    if (photo != null)
                    {
                        gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                        gClass.Image = HttpUtility.HtmlEncode(gClass.SubCategoryName.Replace(" ", "-")) + "-" + gClass.ID.ToString() + Path.GetExtension(photo.FileName);
                        gClass.Image = @"~\Images\ClassImages\" + gClass.Image;


                        //photo.Resize(width: 280, height: 150, preserveAspectRatio: true, preventEnlarge: false);
                        photo.Save(gClass.Image, null, false);
                        Functions.ResizeImage(gClass.Image, 350, 250, false);
                        Session["tempClass"] = gClass;
                    }
                    else if (!String.IsNullOrEmpty(Request.Form["ImageName"]))
                    {
                        gClass.Image = Request.Form["ImageName"];
                    }

                    //create class
                    try
                    {
                        //gClass.ID = Guid.NewGuid();
                        var result = gClass.UpdateClass(gClass_original);

                        if (!result.StartsWith("Error"))
                        {
                            gClass.SubCategoryName = db.getSubCategoryName(gClass.SubCategoryID);
                            //redirect if success          
                            return RedirectToAction("Index", "Classes", new { companyName = companyDetails.Name, classSubCategory = gClass.SubCategoryName.Replace(" ", "-"), classID = gClass.ID.ToString() });

                        }
                        else
                        {
                            isValid = false;
                            createErrorMessage = result;
                            //createErrorMessage = "An error occurred writing to the database. Please try again. ";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        isValid = false;
                        createErrorMessage = ex.ToString();
                        Logger.LogWarning("Admin/Edit", "Edit", companyDetails.Name + ":" + gClass.Name, ex.Message, ex);

                    }
                }
            }

            ViewBag.CompanyDetails = companyDetails;
            ViewBag.GroupClass = gClass;
            ViewBag.AdminIDList = adminIDList;
            ViewBag.Locations = db.getLocationList(companyDetails.Name);
            ViewBag.Admins = db.getUsers(companyDetails);
            ViewBag.Categories = db.getCategoryList();

            ViewBag.isValid = isValid;
            ViewBag.NameErrorMessage = nameErrorMessage;
            ViewBag.DateErrorMessage = dateErrorMessage;
            ViewBag.LocationErrorMessage = locationErrorMessage;
            ViewBag.CreateErrorMessage = createErrorMessage;
            ViewBag.RepeatErrorMessage = repeatErrorMessage;

            return View(gClass);
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult Delete(string companyName = "", string classID = "")
        {
            ViewBag.isValid = true;
            ViewBag.CompanyDetails = Authenticate.Validate(companyName);

            if (ViewBag.CompanyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }

            if (!Authenticate.IsUserInRole(ViewBag.CompanyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }
            Authenticate.ValidSubscription(ViewBag.CompanyDetails);
            DAL db = new DAL();

            GroupClass gClass = db.selectClassDetail(classID);
            
            if(gClass == null)
            {
                gClass = db.selectAdvertDetail(classID);
                gClass.IsAdvert = true;
            }
            else
            {
                gClass.IsAdvert = false;
            }

            ViewBag.Result = false;
            var hostUrl = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            ViewBag.ReturnURL = hostUrl + VirtualPathUtility.ToAbsolute("~/Classes/" + companyName);

            if (gClass != null && companyName == gClass.Company && !gClass.IsAdvert)
            {
                //validate user
                if (!Authenticate.IsUserInRole(companyName, "admin"))
                {
                    return RedirectToAction("Index", "Classes");
                }
                else
                {
                    try
                    {
                        List<PersonDetails> attendees = db.getAttendeeList(gClass.ID.ToString());

                        if (gClass.ClassDates.Last() > DateTime.Now)
                        {
                            foreach (PersonDetails attendee in attendees)
                            {
                                Email.SendClassDeleted(attendee.Email, "", gClass);
                            }
                        }

                        ViewBag.Result = db.deleteClass(gClass.ID.ToString());
                    }
                    catch (Exception ex)
                    {
                        var s = ex;
                        ViewBag.Result = false;
                        Logger.LogWarning("Admin/DeleteClass", "DeleteClass", companyName + ":ClassID-" + classID, ex.Message, ex);
                    }
                }

            }
            else if (gClass != null && companyName == gClass.Company && gClass.IsAdvert)
            {
                if (db.hasAdvertAccess(HttpContext.User.Identity.GetUserId().ToString(), classID))
                {
                    ViewBag.Result = db.deleteAdvert(classID);
                }
            }
            else
            {
                ViewBag.Result = false;
                Logger.LogWarning("Admin/DeleteClass", "DeleteClass", "user-" + HttpContext.User.Identity.Name + ":ClassID-" + classID, "user " + HttpContext.User.Identity.Name + " attempted to delete ClassID-" + classID);
            }

            return View(gClass);
        }



        [ValidateInput(false)]
        public ActionResult AddLocation(string companyName = "", string LocationID = "", string urlReferrer = "")
        {

            var userID = HttpContext.User.Identity.ToString();

            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DuckRowNet.Models.LocationModel location = new Models.LocationModel();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "admin"))
            {
                return RedirectToAction("Index", "Classes");
            }

            location.LocationID = LocationID;
            location.Longitude = "-6.253044480255085";
            location.Latitude = "53.34854052286669";
            location.Zoom = "7";
            location.Name = "";
            location.Description = "";
            location.Address1 = "";
            location.Address2 = "";
            location.City = "";
            location.State = "";
            location.Postcode = "";

            ViewBag.task = "Create";
            ViewBag.isValid = true;
            ViewBag.errorMessage = "";
            ViewBag.successMessage = "";

            ViewBag.urlReferrer = urlReferrer;

            DAL db = new DAL();
            var country = db.getCompanyCountry(companyDetails.Name);
            var countryCode = companyDetails.Country;

            if (location.LocationID != "")
            {
                IEnumerable<dynamic> loc = db.getLocationDetails(location.LocationID, companyDetails.Name);

                if (loc.Count() > 0)
                {
                    location.Longitude = loc.ElementAt(0).longitude;
                    location.Latitude = loc.ElementAt(0).latitude;
                    location.Name = loc.ElementAt(0).Name;
                    location.Description = loc.ElementAt(0).Description;
                    location.Address1 = loc.ElementAt(0).Address1;
                    location.Address2 = loc.ElementAt(0).Address2;
                    location.City = loc.ElementAt(0).City;
                    location.State = loc.ElementAt(0).State;
                    location.Postcode = loc.ElementAt(0).Postcode;
                    ViewBag.task = "Update";
                    location.Zoom = "14";
                    location.IncludeSearch = false;
                }
            }

            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddLocation(DuckRowNet.Models.LocationModel location, string companyName = "", string LocationID = "", string urlReferrer = "")
        {
            location.IncludeSearch = false;

            ViewBag.isValid = true;

            if (location.Longitude == "-6.253044480255085" && location.Latitude == "53.34854052286669")
            {
                //check a name exists and coordinates.
                ViewBag.errorMessage = "You must click the map to select a location";
                ViewBag.isValid = false;
            }
            else if (String.IsNullOrEmpty(location.Name))
            {
                location.IncludeSearch = false;
                ViewBag.errorMessage = "You must enter a name for this location";
                ViewBag.isValid = false;
            }
            else if (String.IsNullOrEmpty(location.State))
            {
                if (location.Country == "IE")
                {
                    ViewBag.errorMessage = "You must select a County for this location";
                }
                else
                {
                    ViewBag.errorMessage = "You must select a County/State for this location";
                }
                ViewBag.isValid = false;
            }


            if (ViewBag.isValid)
            {
                if (String.IsNullOrEmpty(location.LocationID))
                {
                    DAL db = new DAL();
                    //check this company does not already have a location by this name.
                    if (db.getLocationID(location.Name, companyName) != "")
                    {
                        ViewBag.errorMessage = "Your Company already has a location by this name";
                        ViewBag.isValid = false;
                    }
                    else
                    {
                        //create new location
                        try
                        {

                            if (db.AddLocation(location.Name, location.Longitude, location.Latitude, location.Description, location.Address1,
                                    location.Address2, location.City, location.State, location.Postcode, companyName))
                            {
                                //redirect back if success          
                                if (!String.IsNullOrEmpty(urlReferrer))
                                {
                                    Response.Redirect(urlReferrer, false);
                                }
                                else
                                {
                                    return RedirectToAction("Create", "Admin", new { companyName = companyName, classID = "" });
                                }
                            }
                            else
                            {
                                ViewBag.isValid = false;
                                ViewBag.errorMessage = "An error occurred writing to the database. Please try again.";
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.isValid = false;
                            ViewBag.errorMessage = ex.Message;
                            Logger.LogWarning("App_Code/Helper/Location", "AddLocation", companyName + ":location-" + location.Name, ex.Message, ex);

                        }
                    }
                }
                else
                {
                    //udpate the ID
                    try
                    {
                        DAL db = new DAL();
                        if (db.UpdateLocation(location.LocationID, location.Name, location.Longitude, location.Latitude, location.Description, location.Address1,
                                location.Address2, location.City, location.State, location.Postcode, companyName))
                        {
                            ViewBag.isValid = false;
                            ViewBag.successMessage = "This Location has been successfully updated!";
                        }
                        else
                        {
                            ViewBag.isValid = false;
                            ViewBag.errorMessage = "An error occurred writing to the database. Please try again.";
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.isValid = false;
                        ViewBag.errorMessage = ex.Message;
                        Logger.LogWarning("App_Code/Helper/Location", "UpdateLocaiton", companyName + ":location-" + location.Name, ex.Message, ex);
                    }

                }


            }
                return View(location);
        }


        [ValidateInput(false)]
        public ActionResult AddMapLocation(string companyName = "", string LocationID = "", string urlReferrer = "")
        {

            var userID = HttpContext.User.Identity.ToString();

            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DuckRowNet.Models.LocationModel location = new Models.LocationModel();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "admin"))
            {
                return RedirectToAction("Index", "Classes");
            }

            location.LocationID = LocationID;
            location.Longitude = "-6.253044480255085";
            location.Latitude = "53.34854052286669";
            location.Zoom = "7";
            location.Name = "";
            location.Description = "";
            location.Address1 = "";
            location.Address2 = "";
            location.City = "";
            location.State = "";
            location.Postcode = "";

            ViewBag.task = "Create";
            ViewBag.isValid = true;
            ViewBag.errorMessage = "";
            ViewBag.successMessage = "";

            ViewBag.urlReferrer = urlReferrer;

            DAL db = new DAL();
            var country = db.getCompanyCountry(companyDetails.Name);
            var countryCode = companyDetails.Country;

            if (location.LocationID != "")
            {
                IEnumerable<dynamic> loc = db.getLocationDetails(location.LocationID, companyDetails.Name);

                if (loc.Count() > 0)
                {
                    location.Longitude = loc.ElementAt(0).longitude;
                    location.Latitude = loc.ElementAt(0).latitude;
                    location.Name = loc.ElementAt(0).Name;
                    location.Description = loc.ElementAt(0).Description;
                    location.Address1 = loc.ElementAt(0).Address1;
                    location.Address2 = loc.ElementAt(0).Address2;
                    location.City = loc.ElementAt(0).City;
                    location.State = loc.ElementAt(0).State;
                    location.Postcode = loc.ElementAt(0).Postcode;
                    ViewBag.task = "Update";
                    location.Zoom = "14";
                    location.IncludeSearch = false;
                }
            }

            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddMapLocation(DuckRowNet.Models.LocationModel location, string companyName = "", string LocationID = "", string urlReferrer = "")
        {
            location.IncludeSearch = false;

            ViewBag.isValid = true;

            if (location.Longitude == "-6.253044480255085" && location.Latitude == "53.34854052286669")
            {
                //check a name exists and coordinates.
                ViewBag.errorMessage = "You must click the map to select a location";
                ViewBag.isValid = false;
            }
            else if (String.IsNullOrEmpty(location.Name))
            {
                location.IncludeSearch = false;
                ViewBag.errorMessage = "You must enter a name for this location";
                ViewBag.isValid = false;
            }
            else if (String.IsNullOrEmpty(location.State))
            {
                if (location.Country == "IE")
                {
                    ViewBag.errorMessage = "You must select a County for this location";
                }
                else
                {
                    ViewBag.errorMessage = "You must select a County/State for this location";
                }
                ViewBag.isValid = false;
            }


            if (ViewBag.isValid)
            {
                if (String.IsNullOrEmpty(location.LocationID))
                {
                    DAL db = new DAL();
                    //check this company does not already have a location by this name.
                    if (db.getLocationID(location.Name, companyName) != "")
                    {
                        ViewBag.errorMessage = "Your Company already has a location by this name";
                        ViewBag.isValid = false;
                    }
                    else
                    {
                        //create new location
                        try
                        {

                            if (db.AddLocation(location.Name, location.Longitude, location.Latitude, location.Description, location.Address1,
                                    location.Address2, location.City, location.State, location.Postcode, companyName))
                            {
                                //redirect back if success          
                                if (!String.IsNullOrEmpty(urlReferrer))
                                {
                                    Response.Redirect(urlReferrer, false);
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Classes", new { companyName = companyName });
                                }
                            }
                            else
                            {
                                ViewBag.isValid = false;
                                ViewBag.errorMessage = "An error occurred writing to the database. Please try again.";
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.isValid = false;
                            ViewBag.errorMessage = ex.Message;
                            Logger.LogWarning("App_Code/Helper/Location", "AddLocation", companyName + ":location-" + location.Name, ex.Message);

                        }
                    }
                }
                else
                {
                    //udpate the ID
                    try
                    {
                        DAL db = new DAL();
                        if (db.UpdateLocation(location.LocationID, location.Name, location.Longitude, location.Latitude, location.Description, location.Address1,
                                location.Address2, location.City, location.State, location.Postcode, companyName))
                        {
                            ViewBag.isValid = false;
                            ViewBag.successMessage = "This Location has been successfully updated!";
                        }
                        else
                        {
                            ViewBag.isValid = false;
                            ViewBag.errorMessage = "An error occurred writing to the database. Please try again.";
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.isValid = false;
                        ViewBag.errorMessage = ex.Message;
                        Logger.LogWarning("App_Code/Helper/Location", "UpdateLocaiton", companyName + ":location-" + location.Name, ex.Message, ex);
                    }

                }


            }
            return View(location);
        }


        [Authorize]
        [HttpGet]
        public ActionResult AddClient(string companyName = "", string urlReferrer = "")
        {
            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DAL db = new DAL();
            ViewBag.IsValid = true;

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "admin"))
            {
                return RedirectToAction("Index", "Classes");
            }

            PersonDetails person = new PersonDetails();
            ViewBag.UrlReferrer = urlReferrer;

            return View(person);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddClient(PersonDetails person, string companyName = "", string urlReferrer = "")
        {
            ViewBag.IsValid = true;
            if (!ModelState.IsValid)
            {
                return View(person);
            }

            DAL db = new DAL();

            if (db.UpdateClient("0", person.FirstName, person.LastName, person.Email, person.Phone, person.Address1, person.Address2,
                person.City, person.State, person.Country, person.Postcode, companyName))
            {
                //redirect back if success          
                if (urlReferrer != "")
                {
                    Response.Redirect(urlReferrer, false);
                }
                else
                {
                    return RedirectToAction("Clients", "Admin", new { companyName = companyName });
                }
            }
            else
            {
                ViewBag.IsValid = false;
                ViewBag.ErrorMessage = "An error occurred writing to the database. Please try again.";
            }

            ViewBag.UrlReferrer = urlReferrer;

            return View(person);
        }


        [Authorize]
        public ActionResult Clients(string companyName = "")
        {
            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DAL db = new DAL();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }

            List<PersonDetails> clients = db.getClients(companyDetails.Name);
            var companyCounty = db.getCompanyCountry(companyDetails.Name);

            ViewBag.Clients = clients;
            ViewBag.CompanyName = companyDetails.Name;

            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Client(string companyName = "", string classID = "")
        {
            PersonDetails person = new PersonDetails();
            person.ID = classID;
            person.Type = Functions.PersonType.User;

            ViewBag.IsValid = true;
            if (!ModelState.IsValid)
            {
                return View(person);
            }

            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DAL db = new DAL();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }


            if (person.ID.StartsWith("c_"))
            {
                person.ID = person.ID.Substring(2);
                person.Type = Functions.PersonType.Client;
            }

            person.GetDetails();

            //IEnumerable<dynamic> ClientDetail = db.getClientDetail(companyDetails.Name, person);
            ViewBag.CompanyDetails = companyDetails;

            return View(person);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Client(PersonDetails person, string companyName = "", string classID = "")
        {
            ViewBag.IsValid = true;
            if (!ModelState.IsValid && Request["buttonClick"] != "Delete")
            {
                return View(person);
            }

            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DAL db = new DAL();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.IsUserInRole(companyDetails.Name, "editor"))
            {
                return RedirectToAction("Index", "Classes");
            }


            if (person.ID.StartsWith("c_"))
            {
                person.ID = person.ID.Substring(2);
                person.Type = Functions.PersonType.Client;
            }

            //person.GetDetails();

            //IEnumerable<dynamic> ClientDetail = db.getClientDetail(companyDetails.Name, person);
            ViewBag.CompanyDetails = companyDetails;


            var buttonClick = Request["buttonClick"];
            string urlReferrer = "";

            if (buttonClick == "Delete")
            {
                urlReferrer = Request.Form["urlReferrer"];
                db.deleteClient(person.ID, companyDetails.Name);

                //redirect back if success
                if (!String.IsNullOrEmpty(urlReferrer))
                {
                    Response.Redirect(urlReferrer, false);
                }
                else
                {
                    return RedirectToAction("Clients", "Admin", new { companyName = companyDetails.Name });
                }

            }
            else
            {
                urlReferrer = Request.Form["urlReferrer"];

                //check a name exists?
                if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
                {
                    ViewBag.ErrorMessage = "You must enter the name of your client";
                    ViewBag.IsValid = false;
                }

                if (ViewBag.IsValid)
                {

                    if (db.UpdateClient(person, companyDetails.Name))
                    {
                        //redirect back if success
                        if (urlReferrer != "")
                        {
                            Response.Redirect(urlReferrer, false);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Classes", new { companyName = companyDetails.Name });
                        }
                    }
                    else
                    {
                        ViewBag.IsValid = false;
                        ViewBag.ErrorMessage = "An error occurred writing to the database. Please try again.";
                    }

                }
            }

            return View(person);
        }



        [Authorize]
        [ValidateInput(false)]
        public ActionResult CreateAdvert(string companyName = "", string classID = "")
        {
            DAL db = new DAL();

            PersonDetails currentUser = new PersonDetails();

            ViewBag.isValid = true;
            if (companyName == "")
            {
                if (String.IsNullOrEmpty(ViewBag.Companyname))
                {
                    currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
                    companyName = currentUser.CompanyName;
                }
                else
                {
                    companyName = ViewBag.CompanyName;
                }
            }

            ViewBag.CompanyDetails = Authenticate.Validate(companyName);
            ViewBag.CompanyName = companyName;

            if (String.IsNullOrEmpty(classID))
            {
                ViewBag.Advert = new GroupClass(true);
                ViewBag.Advert.Company = companyName;
            }
            else if (db.hasAdvertAccess(HttpContext.User.Identity.GetUserId().ToString(), classID))
            {
                ViewBag.Advert = db.selectAdvertDetail(classID);
            }
            ViewBag.freeAdvert = false;
            ViewBag.success = false;
            ViewBag.CreateErrorMessage = "";
            ViewBag.Counties = db.listCounties("IE");

            if (String.IsNullOrEmpty(companyName))
            {
                if (db.getMyAdvertCount(HttpContext.User.Identity.GetUserId()) < 1)
                {
                    ViewBag.freeAdvert = true;
                }
            }
            else {
                Authenticate.ValidSubscription(ViewBag.CompanyDetails);
                if (db.getCompanyAdvertCount(companyName) < 1)
                {
                    ViewBag.freeAdvert = true;
                }
            }

            if (String.IsNullOrEmpty(classID))
            {
                return View();
            }
            else
            {
                return View(ViewBag.Advert);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateAdvert(DuckRowNet.Helpers.Object.GroupClass advert, string companyName = "", string task = "")
        {
            DAL db = new DAL();

            PersonDetails currentUser = new PersonDetails();

            var companyID = "";

            if (companyName == "")
            {
                if (String.IsNullOrEmpty(ViewBag.Companyname))
                {
                    currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
                    companyName = currentUser.CompanyName;
                }
                else
                {
                    companyName = ViewBag.CompanyName;
                }
            }

            ViewBag.isValid = true;
            ViewBag.freeAdvert = false;
            ViewBag.CreateErrorMessage = "";

            ViewBag.Task = task.ToLower();

            var email = HttpContext.User.Identity.Name;

            if (String.IsNullOrEmpty(companyName))
            {
                if (db.getMyAdvertCount(HttpContext.User.Identity.GetUserId()) < 1)
                {
                    ViewBag.freeAdvert = true;
                }
            }
            else {
                ViewBag.CompanyDetails = Authenticate.Validate(companyName);
                companyID = ViewBag.CompanyDetails.ID.ToString();
                Authenticate.ValidSubscription(ViewBag.CompanyDetails);
                if (db.getCompanyAdvertCount(companyName) < 1)
                {
                    ViewBag.freeAdvert = true;
                }

                if (!String.IsNullOrEmpty(ViewBag.CompanyDetails.Email))
                {
                    email = ViewBag.CompanyDetails.Email;
                }
            }

            Boolean validateForm = false;
            if (Request.Form["validateForm"].ToString() == "True")
            {
                validateForm = true;
            }

            //Error Checks
            if (validateForm && String.IsNullOrEmpty(Request.Form["advertName"].ToString()))
            {
                ViewBag.CreateErrorMessage = "You must specify a title.";
                ViewBag.IsValid = false;
            }

            advert.Name = Request.Form["advertName"];
            advert.Description = Request.Unvalidated().Form["advertDescription"];
            //advert.Image = imageFile;
            advert.Phone = Request.Form["advertPhone"];
            advert.ContactName = Request.Form["advertContactName"];
            advert.Email = email;
            advert.Website = Request.Form["advertWebsite"];
            advert.CategoryID = Request.Form["advertCategory"];
            advert.SubCategoryID = Request.Form["advertSubCategory"].Split('_').First();
            //advert.Cost = cost;
            advert.Address1 = Request.Form["advertAddress1"];
            advert.Address2 = Request.Form["advertAddress2"];
            advert.City = Request.Form["advertCity"];
            advert.State = Request.Form["advertState"];

            //Session["advert"] = advert;

            if (validateForm && ViewBag.IsValid)
            {
                WebImage photo = WebImage.GetImageFromRequest();
                string imageFile = "";

                if (photo != null)
                {
                    advert.SubCategoryName = db.getSubCategoryName(advert.SubCategoryID);
                    imageFile = HttpUtility.HtmlEncode(advert.SubCategoryName.Replace(" ", "-")) + "-" + Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                    imageFile = @"~\Images\ClassImages\" + imageFile;

                    //photo.Resize(width: 280, height: 150, preserveAspectRatio: true, preventEnlarge: false);
                    photo.Save(imageFile, null, false);
                    Functions.ResizeImage(imageFile, 350, 250, false);
                }
                else if (!String.IsNullOrEmpty(Request.Form["ImageName"]))
                {
                    imageFile = Request.Form["ImageName"];
                }


                double cost = 3;
                if (ViewBag.Task != "edit" && !ViewBag.freeAdvert)
                {
                    cost = Convert.ToDouble(Request.Form["advertCost"].ToString().Replace("€", ""));
                }


                string billingID = "";
                if (ViewBag.freeAdvert)
                {
                    billingID = "";
                }

                string advertID = Request.Form["advertID"].ToString();
                if (String.IsNullOrEmpty(advertID) || advertID == new Guid().ToString())
                {
                    advertID = Guid.NewGuid().ToString();
                    task = "new";
                }

                advert = new GroupClass(
                        new Guid(advertID),
                        Request.Form["advertName"],
                        Request.Unvalidated().Form["advertDescription"],
                        imageFile,
                        email,
                        Request.Form["advertPhone"],
                        Request.Form["advertContactName"],
                        Request.Form["advertWebsite"],
                        Request.Form["advertCategory"],
                        "",
                        Request.Form["advertSubCategory"].Split('_').First(),
                        "",
                        "", //Request.Form["advertType"],
                        cost,
                        Request.Form["advertAddress1"],
                        Request.Form["advertAddress2"],
                        Request.Form["advertCity"],
                        Request.Form["advertState"],
                        DateTime.Now,
                        companyName,
                        HttpContext.User.Identity.GetUserId(),
                        billingID,
                        true
                    );

                advert.CompanyID = new Guid(companyID);

                try
                {
                    advert.IsActive = true;
                    advert.CreateAdvert();

                    return RedirectToAction("Index", "Advert", new { advertCategory = advert.CategoryName.Replace(" ", "-"), advertID = advert.ID.ToString() });

                }
                catch (System.Exception ex)
                {
                    ViewBag.isValid = false;
                    ViewBag.CreateErrorMessage = ex.ToString();
                    Logger.LogWarning("Account/CreateAdvert", "CreateAdvert", "user <" + HttpContext.User.Identity.Name + ">", ex.Message + "<br/>" + ex.StackTrace);

                }

            }

            return View(advert);
        }


        [Authorize]
        [ValidateInput(false)]
        public ActionResult ReservationConfirm(string companyName = "", string classID = "")
        {
            CompanyDetails companyDetails = Authenticate.Validate(companyName);
            DAL db = new DAL();

            if (companyDetails.Name == "")
            {
                return RedirectToAction("Index", "Classes");
            }
            if (!Authenticate.Admin(companyDetails.Name))
            {
                return RedirectToAction("Index", "Classes", new { companyName = companyDetails.Name });
            }
            
            


            return View();
        }


    }
}