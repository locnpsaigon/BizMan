using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizMan.Controllers
{
    public class ErrorController : BaseController
    {
        //
        // GET: /Error/AccessDenied/
        public ActionResult AccessDenied(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult Message(string message)
        {
            ViewBag.Message = message;
            return View();
        }

    }
}
