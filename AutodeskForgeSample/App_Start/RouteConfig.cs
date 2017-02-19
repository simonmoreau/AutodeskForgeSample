using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutodeskForgeSample.Controllers;
using MvcCodeRouting;

namespace AutodeskForgeSample
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //// If you don't have "HomeController", choose another controller you have.
            //// MvcCodeRouting will look for all controllers in the same namespace and sub-namespaces as the one specified here.
            //routes.MapCodeRoutes(typeof(TokenController), new CodeRoutingSettings
            //{
            //    UseImplicitIdToken = true
            //});

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            
        }
    }
}
