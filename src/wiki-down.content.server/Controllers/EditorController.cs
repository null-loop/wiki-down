using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GSIP.Tools.WikiDown.ContentServer.Controllers
{
    public class EditorController : Controller
    {
        // GET: Editor
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PageTemplate()
        {
            return View();
        }

        public ActionResult ArticleTemplate()
        {
            return View();
        }
    }
}