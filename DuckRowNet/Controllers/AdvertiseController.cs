using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DuckRowNet.Helpers;

namespace DuckRowNet.Controllers
{
    public class AdvertiseController : Controller
    {
        // GET: Advertise
        public ActionResult Index(string classSubCategory = "")
        {
            DAL db = new DAL();

            IEnumerable<dynamic> subCategories = db.getSubCategoryList();
            bool found = false;

            ViewBag.SubCategoryName = "";

            foreach(dynamic item in subCategories)
            {
                var i = item.Name.Replace(" ", "-");
                if(String.Compare(classSubCategory.ToLower(), i.ToLower()) == 0)
                {
                    ViewBag.SubCategoryName = item.Name;
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                throw new HttpException(404, "Page Not Found");
            }

            return View();
        }
    }
}