using DuckRowNet.Helpers;
using DuckRowNet.Helpers.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DuckRowNet.Controllers
{
    public class AdvertController : Controller
    {
        // GET: Advert
        public ActionResult Index(string advertCategory = "", string advertID = "")
        {
            if(advertID == "")
            {
                if (advertCategory == "")
                {
                    return RedirectToAction("", "Classes");
                }
                advertID = advertCategory;
            }

            var hostUrl = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            GroupClass gClass = null;


            DAL db = new DAL();

            gClass = db.selectAdvertDetail(advertID);

            CompanyDetails companyDetails;
            if (gClass == null || String.IsNullOrEmpty(gClass.Company))
            {
                return RedirectToAction("", "Classes");
                //companyDetails = Authenticate.Validate("DuckRow");
            }
            else {
                companyDetails = Authenticate.Validate(gClass.Company);
            }

            bool isAdminUser = false;

            if (gClass != null)
            {
                isAdminUser = Authenticate.IsAdvertUser(gClass.ID.ToString());
            }

            if(gClass == null) // || !isAdminUser)
            {
                return RedirectToAction("Index", "Classes");
            }

            ViewBag.Advert = gClass;
            ViewBag.AdminUser = isAdminUser;


            return View();
        }
    }
}