using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DuckRowNet.Models;
using DuckRowNet.Helpers;
using DuckRowNet.Helpers.Object;

namespace DuckRowNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            DAL db = new DAL();
            CompanyDetails companyDetails = Authenticate.Validate("DuckRow");
            ViewBag.CompanyDetails = companyDetails;

            List<GroupClass> classes = new List<GroupClass>();

            DateTime startDate = new DateTime(2000, 1, 1);
            DateTime endDate = DateTime.Now.AddYears(20);

            string location = "";
            string search = "";

            classes = db.searchAllPublicClasses(location, search, companyDetails.Name);
            ViewBag.Classes = classes;

            List<String> categories = new List<String>();
            foreach (var item in classes)
            {
                if (!categories.Contains(item.CategoryName))
                {
                    categories.Add(item.CategoryName);
                }
            }
            categories.Sort();
            ViewBag.Categories = categories;

            List<String> locations = new List<String>();
            foreach (var item in classes)
            {
                if (!locations.Contains(item.State) && !String.IsNullOrEmpty(item.State))
                {
                    locations.Add(item.State);
                }
            }
            locations.Sort();
            ViewBag.Locations = locations;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error()
        {

            return View();
        }
    }
}