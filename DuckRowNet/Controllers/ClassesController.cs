﻿using DuckRowNet.Helpers;
using DuckRowNet.Helpers.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DuckRowNet.Controllers
{
    public class ClassesController : Controller
    {
        // GET: Classes
        public ActionResult Index(string companyName = "", string classID = "")
        {
            var hostUrl = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

            CompanyDetails companyDetails = new CompanyDetails();
            GroupClass gClass = new GroupClass();
            string subCategory = "";

            DAL db = new DAL();

            var pageURL = "";
            if (classID != "")
            {
                gClass = db.selectClassDetail(classID);
                if (gClass != null)
                {
                    companyDetails = Authenticate.Validate(companyName);
                    if (gClass.Company != companyDetails.Name)
                    {
                        throw new HttpException(404, "Page Not Found");
                        //return RedirectToAction("Index", "Classes");
                    }
                    //Logger.LogWarning("Classes.cshtml", "gClass is null", "company: " + companyDetails.Name + " classID: " + classID + "<br/>UserAgent" + Request.UserAgent + "<br/>UserHostName" + Request.UserHostName, "gClass is null");
                    //if (String.IsNullOrEmpty(companyName))
                    //{
                    //    return RedirectToAction("Index", "Classes");
                    //}
                    //else
                    //{
                    //    return RedirectToAction("Index", "Classes", new { companyName = companyName, classID = "", classSubCategory = "" });
                    //}
                }
                else
                {
                    throw new HttpException(404, "Page Not Found");
                }
            }
            else
            {
                if (companyName != "")
                {
                    companyDetails = Authenticate.Validate(companyName);

                    if(companyDetails == null)
                    {
                        throw new HttpException(404, "Page Not Found");
                        //return RedirectToAction("Index", "Classes", new { companyName = "", classID = "", classSubCategory = "" });
                    }
                    pageURL = hostUrl + "/Classes/" + companyDetails.Name;
                }
            }

            ViewBag.UnconfirmedRequests = new List<ClientAttendance>();
            if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
            {
                List<ClientAttendance> requests = db.getUnconfirmedRequests(companyDetails.Name);
                ViewBag.UnconfirmedRequests = requests;
            }

            if (companyDetails.Name == "")
            {
                throw new HttpException(404, "Page Not Found");
                //return RedirectToAction("Index", "Classes");
            }

            TempData["CompanyName"] = companyName;
            TempData["CompanyDetails"] = companyDetails;
            TempData["GroupClass"] = gClass;

            Session["subCategory"] = subCategory;

            return View();
        }

        public ActionResult Page(int page = 1)
        {
            ViewBag.Page = page;
            Session["subCategory"] = "";

            return View();
        }



        public ActionResult ClassDetail(string companyName = "", string subCategory = "", int year = 0, 
                int month = 0, int day = 0, string classSlug = "")
        {
            var hostUrl = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

            CompanyDetails companyDetails = new CompanyDetails();
            GroupClass gClass = new GroupClass();

            DAL db = new DAL();
            
            gClass = db.selectClassDetailFromSlug(Request.Url.AbsolutePath);

            if (gClass != null)
            {
                companyDetails = Authenticate.Validate(companyName);
                if (gClass.Company != companyDetails.Name)
                {
                    throw new HttpException(404, "Page Not Found");
                }
                else
                {
                    if (subCategory != Functions.convertToSlug(gClass.SubCategoryName))
                    {
                        throw new HttpException(404, "Page Not Found");
                    }
                    else
                    {
                        DateTime gDate = new DateTime(year, month, day);
                        if (gClass.StartDate.ToString("yyyy-MM-dd") != gDate.ToString("yyyy-MM-dd"))
                        {
                            throw new HttpException(404, "Page Not Found");
                        }
                    }
                }

            }
            else
            { 
                throw new HttpException(404, "Page Not Found");
            }

            

            ViewBag.UnconfirmedRequests = new List<ClientAttendance>();
            if (companyDetails.Name != "" && Authenticate.Admin(companyDetails.Name))
            {
                List<ClientAttendance> requests = db.getUnconfirmedRequests(companyDetails.Name);
                ViewBag.UnconfirmedRequests = requests;
            }

            TempData["CompanyName"] = companyName;
            TempData["CompanyDetails"] = companyDetails;
            TempData["GroupClass"] = gClass;

            Session["subCategory"] = "";

            return View();
        }

    }
}