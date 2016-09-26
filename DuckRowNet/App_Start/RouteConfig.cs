using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DuckRowNet
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Images/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("Widgets/{*pathInfo}");

            routes.MapRoute(
                name: "Classes",
                url: "Classes/{companyName}/{classSubCategory}/{classID}",
                defaults: new
                {
                    controller = "Classes",
                    action = "Index",
                    companyName = UrlParameter.Optional,
                    classSubCategory = UrlParameter.Optional,
                    classID = UrlParameter.Optional
                }
            );


            routes.MapRoute(
                name: "Advertise",
                url: "Advertise/{classSubCategory}",
                defaults: new
                {
                    controller = "Advertise",
                    action = "Index",
                    classSubCategory = UrlParameter.Optional
                }
            );


            routes.MapRoute(
                name: "Advert",
                url: "Advert/{advertCategory}/{advertID}",
                defaults: new
                {
                    controller = "Advert",
                    action = "Index",
                    advertCategory = UrlParameter.Optional,
                    advertID = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "Admin-Classes",
                url: "Admin/{action}/{companyName}/{classID}",
                defaults: new
                {
                    controller = "Admin",
                    action = "Index",
                    companyName = UrlParameter.Optional,
                    classID = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "Reserve-Classes",
                url: "Reserve/{action}/{companyName}/{classID}",
                defaults: new
                {
                    controller = "Reserve",
                    action = "Confirmation",
                    companyName = UrlParameter.Optional,
                    classID = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "Error",
                url: "Whoops/",
                defaults: new { controller = "Home", action = "Error", companyName = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "Companyclasses",
            //    url: "{companyName}/",
            //    defaults: new { controller = "Classes", action = "Index", companyName = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "Classes/{action}/{id}/",
                defaults: new { controller = "Classes", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Page",
                url: "Page/{page}/",
                defaults: new { controller = "Classes", action = "Page", page = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{id}/",
                defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Home",
                url: "Home/{action}/{id}/",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "json",
                url: "json/{action}/{id}/",
                defaults: new { controller = "json", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Manage",
                url: "Manage/{action}/{id}/",
                defaults: new { controller = "Manage", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Reserve",
                url: "Reserve/{action}/{id}/",
                defaults: new { controller = "Reserve", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Setup",
                url: "Setup/{action}/{id}/",
                defaults: new { controller = "Setup", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Category List",
                url: "Category/{subCategory}",
                defaults: new
                {
                    controller = "Category",
                    action = "CategoryList",
                    subCategory = UrlParameter.Optional
                }
            ); ///Subcat

            routes.MapRoute(
                name: "Class Details",
                url: "{companyName}/{subCategory}/{year}/{month}/{day}/{classSlug}",
                defaults: new { controller = "Classes", action = "ClassDetail", companyName = UrlParameter.Optional,
                    subCategory = UrlParameter.Optional,
                    year = UrlParameter.Optional,
                    month = UrlParameter.Optional,
                    day = UrlParameter.Optional,
                    classSlug = UrlParameter.Optional
                },
                constraints: new {year = @"^\d{4}$" }
            ); ///companyName/Subcat/year/month/day/classname



            routes.MapRoute(
                name: "CatchAll",
                url: "{controller}/{action}/{id}/",
                defaults: new { controller = "Classes", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
