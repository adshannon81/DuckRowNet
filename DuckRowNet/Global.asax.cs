using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DuckRowNet
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().StartsWith("http://www.duckrow"))
                {
                    string newUrl = string.Empty;
                    if (HttpContext.Current.Items["UrlRewritingNet.UrlRewriter.VirtualUrl"] != null)
                        newUrl = "http://duckrow.net" + HttpContext.Current.Items["UrlRewritingNet.UrlRewriter.VirtualUrl"].ToString();
                    else
                        newUrl = HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Replace("http://www.duckrow", "http://duckrow");

                    Response.Status = "301 Moved Permanently";
                    Response.StatusCode = 301;
                    Response.StatusDescription = "Moved Permanently";
                    Response.AddHeader("Location", newUrl);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

    }
}
