using System.Web.Mvc;

namespace wiki_down.content.server.Controllers
{
    public class EditorController : Controller
    {
        public ActionResult PageTemplate(string template)
        {
            return View();
        }

        public ActionResult ArticleTemplate(string template)
        {
            return View();
        }
    }
}