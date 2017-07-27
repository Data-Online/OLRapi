using OLRapi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OLRapi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            //var zz = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");

            ViewBag.EventUID = Settings.EventUID; // "992fdb3b-7715-4c41-a591-1558899fc91a";
            return View();
        }

        public ActionResult Test()
        {
            ViewBag.Title = "Test Page";
            //var zz = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");

            ViewBag.EventUID = Settings.EventUID; // "992fdb3b-7715-4c41-a591-1558899fc91a";
            return View();
        }

        public ActionResult RegisterMe()
        {
            ViewBag.Title = "Naturally Dunedin";
            //var zz = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");

            ViewBag.EventUID = Settings.EventUID; // "992fdb3b-7715-4c41-a591-1558899fc91a";
            return View();
        }


    }
}
