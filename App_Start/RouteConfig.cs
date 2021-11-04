﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LostCampStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute("ShopOrder", "Shop/{action}/{id}", new
            //{
            //    controller = "Shop",
            //    action = "myorderdet",
            //    id = UrlParameter.Optional
            //}, new[] { "LostCampStore.Controllers" });

            routes.MapRoute("Account", "Account/{action}/{id}", new
            {
                controller = "Account",
                action = "Index",
                id = UrlParameter.Optional
            }, new[] { "LostCampStore.Controllers" });

            routes.MapRoute("Cart", "Cart/{action}/{id}", new
            {
                controller = "Cart",
                action = "Index",
                id = UrlParameter.Optional
            }, new[] { "LostCampStore.Controllers" });

            

            routes.MapRoute("SidebarPartial", "Pages/SidebarPartial", new { controller = "Pages", action = "SidebarPartial" },
                new[] { "LostCampStore.Controllers" });

            routes.MapRoute("Shop", "Shop/{action}/{name}", new { controller = "Shop", action = "Index",
                name =UrlParameter.Optional }, new[] { "LostCampStore.Controllers" });

            routes.MapRoute("PagesMenuPartial", "Pages/PagesMenuPartial", new { controller = "Pages", action = "PagesMenuPartial" }, new[] { "LostCampStore.Controllers" });

            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new[] { "LostCampStore.Controllers" });

            routes.MapRoute("Default", "", new { controller = "Pages", action = "Index" }, new[] { "LostCampStore.Controllers" });


            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
