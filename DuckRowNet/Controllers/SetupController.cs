using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DuckRowNet.Models;
using DuckRowNet.Helpers;
using DuckRowNet.Helpers.Object;
using System.IO;
using System.Web.Helpers;
using System.Text.RegularExpressions;
using reCAPTCHA.MVC;

namespace DuckRowNet.Controllers
{
    [Authorize]
    public class SetupController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public SetupController()
        {
        }

        public SetupController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: Setup
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Register()
        {
            ViewBag.ClaimOwnership = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidator(
            ErrorMessage = "Invalid input captcha.",
            RequiredMessage = "The captcha field is required.")]
        public ActionResult Register(SetupRegisterViewModel model, string returnUrl)
        {
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            bool alphanumeric = true;
            var t = Request.Params["g-recaptcha-response"].ToString();
            if (!r.IsMatch(model.Company))
            {
                alphanumeric = false;
                ModelState.AddModelError("Company", "Company Name must only contain Letters and Numbers");
                ViewBag.ErrorMessage = "Company Name must only contain Letters and Numbers";
            }
            ViewBag.ClaimOwnership = "";
            if (alphanumeric && ModelState.IsValid)
            {
                var companyName = model.Company;
                DAL db = new DAL();

                if (db.checkCompanyExists(companyName))
                {
                    ModelState.AddModelError("Company", "Company Name already exists");
                    ViewBag.ErrorMessage = "Company Name already exists";
                    ViewBag.ClaimOwnership = companyName;
                }
                else
                {
                    return RedirectToAction("Confirm", "Setup", new { companyName = companyName, subscription = model.Subscription, returnUrl = returnUrl });
                    //Response.Redirect("Confirm?companyName=" + companyName + "&subscription=" + model.Subscription + "&returnUrl=" + returnUrl, false);

                }
            }
            return View(model);
        }


        public ActionResult Confirm(string companyName, string subscription, string returnUrl)
        {
            var userID = User.Identity;

            CompanyDetails companyDetails = new CompanyDetails(companyName);
            companyDetails.Email = userID.Name;
            companyDetails.Update();

            DAL db = new DAL();

            //PersonDetails currentUser = db.getUserDetails(HttpContext.User.Identity.Name);
            //currentUser.CompanyID = companyDetails.ID;
            
            //create new role for this company
            var roleID = db.insertCompanyRole(companyName, companyName);
            roleID = db.insertCompanyRole("admin_" + companyName, companyName);
            db.insertUserRole(User.Identity.GetUserId(), roleID);
            roleID = db.insertCompanyRole("basic_" + companyName, companyName);
            roleID = db.insertCompanyRole("editor_" + companyName, companyName);

            Subscription sub = new Subscription(companyDetails,
                    "free",
                    DateTime.Now,
                    DateTime.Now.AddYears(50),
                    DateTime.Now.AddSeconds(1), //Convert.ToDateTime(decoder["LASTPAYMENTDATE"]),
                    "0",
                    "0",
                    0,
                    "Active",
                    "",
                    "free",
                    Convert.ToInt16(subscription)
                    );

            //Helper.Email.SendCompanyRegistrationEmail(email, companyName);
            
            return RedirectToAction("Company", "Setup", new { id = companyName });

            //return View();
        }


        public ActionResult Company(string id = "")
        {
            DAL db = new DAL();
            CompanyDetails companyDetails = db.getCompanyDetails(id);

            if (companyDetails == null || String.IsNullOrEmpty(companyDetails.Name))
            {
                return RedirectToAction("Register", "Setup");
            }
            ViewBag.CompanyDetails = companyDetails;
            ViewBag.Message = "";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Company(SetupCompanyViewModel model)
        {
            DAL db = new DAL();

            CompanyDetails companyDetails = db.getCompanyDetails(model.Name);
            companyDetails.Phone = model.Phone;
            companyDetails.Email = model.Email;
            companyDetails.Description = System.Net.WebUtility.HtmlDecode(model.Description);
            companyDetails.Address1 = model.Address1;
            companyDetails.Address2 = model.Address2;
            companyDetails.City = model.City;
            companyDetails.State = model.County;
            companyDetails.Country = model.Country;
            companyDetails.URL = model.Website;
            companyDetails.FaceBookURL = model.Facebook;
            companyDetails.PaypalEmail = model.Paypal;

            WebImage photo = WebImage.GetImageFromRequest();
            if (photo != null)
            {
                companyDetails.ImagePath = HttpUtility.HtmlEncode(companyDetails.Name.Replace(" ", "-")) + DateTime.Now.ToString("-MMddss") + Path.GetExtension(photo.FileName);
                companyDetails.ImagePath = @"~\Images\CompanyImages\" +  companyDetails.ImagePath;

                //photo.Resize(width: 280, height: 150, preserveAspectRatio: true, preventEnlarge: false);
                photo.Save(companyDetails.ImagePath, null, false);
                Functions.ResizeImage(companyDetails.ImagePath, 350, 250, false);
            }


            companyDetails.Update();

            ViewBag.CompanyDetails = companyDetails;
            ViewBag.Message = "Company Details Updated :)";

            return View(model);
        }

    }
}