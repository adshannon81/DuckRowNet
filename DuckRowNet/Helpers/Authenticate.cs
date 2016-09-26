using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DuckRowNet.Helpers.Object;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using DuckRowNet.Models;

namespace DuckRowNet.Helpers
{
    public class Authenticate
    {
        [Authorize]
        public static bool IsAuthenticated()
        {
            return true;
        }

        public static CompanyDetails Validate(string company)
        {
            DAL db = new DAL();

            CompanyDetails companyDetails = db.getCompanyDetails(company);

            if (companyDetails == null)
            {
                company = "Duck Row";
            }

            return companyDetails;

        }

        public static bool Admin(string company)
        {
            if (HttpContext.Current.User.IsInRole("admin_" + company)) 
            {
                return true;
            }
            else if (HttpContext.Current.User.IsInRole("admin"))
            {
                return true;
            }
            return false;
        }

        [Authorize]
        public static bool IsUserInRole(string companyName, string role)
        {
            var IsMember = false;

            string id = HttpContext.Current.User.Identity.GetUserId();
            var context = new ApplicationDbContext();
            var userRoles = context.Users
                                .Where(u => u.Id == id)
                                .SelectMany(u => u.Roles)
                                .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                                .ToList();

            if (role == "admin")
            {
                if (userRoles.Any(u =>  u.Name == "admin_" + companyName))
                {
                    IsMember = true;

                }
            }
            else if (role == "editor")
            {
                if(userRoles.Any(u => u.Name == "editor_" + companyName 
                                    || u.Name == "admin_" + companyName )) {
                    IsMember = true;
                }
            }
            else if (role == "basic")
            {
                if (userRoles.Any(u => u.Name == "editor_" + companyName
                                     || u.Name == "admin_" + companyName
                                     || u.Name == "basic_" + companyName))
                {
                    IsMember = true;

                }
            }
            else if (Roles.IsUserInRole("admin"))
            {
                IsMember = true;
            }

            return IsMember;
        }

        public static void ValidSubscription(CompanyDetails companyDetails, Boolean edit = false)
        {
            if (Convert.ToDateTime(companyDetails.Subscription).AddDays(7) <= DateTime.Now)
            {
                HttpContext.Current.Response.Redirect("~/Admin/Billing/" + companyDetails.Name, false);
            }

            DAL db = new DAL();

            int i = 0;
            if (edit)
            {
                i++;
            }

            if ((i + companyDetails.SubType.MaxClasses) <= db.getClassCount(companyDetails))
            {
                HttpContext.Current.Response.Redirect("~/Admin/Billing/" + companyDetails.Name, false);
            }
        }

        [Authorize]
        public static bool IsEnrolled(CompanyDetails companyDetails, GroupClass gClass)
        {
            bool valid = false;

            valid = IsUserInRole(companyDetails.Name, "admin");

            if(!valid)
            {
                DAL db = new DAL();
                if(db.IsEnrolled(gClass.ID.ToString(), HttpContext.Current.User.Identity.Name))
                {
                    valid = true;
                }
            }

            return false;
        }

        public static string ValidateStrict(string company)
        {
            DAL db = new DAL();

            //if the company does not exist then check further
            if (!db.checkCompanyExists(company))
            {
                return "DuckRow";

            }

            return company;

        }

        public static bool IsAdvertUser(string advertID)
        {
            DAL db = new DAL();
            if (!HttpContext.Current.User.Identity.IsAuthenticated ||  !db.hasAdvertAccess(HttpContext.Current.User.Identity.GetUserId().ToString(), advertID))
            {
                return false;
            }
            return true;
        }

    }
}