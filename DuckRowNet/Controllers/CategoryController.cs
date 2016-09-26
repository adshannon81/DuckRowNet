using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DuckRowNet.Controllers
{
    public class CategoryController : Controller
    {
        public ActionResult CategoryList(string subCategory)
        {
            ViewBag.SubCategory = subCategory;

            return View();
        }

 
    }

}