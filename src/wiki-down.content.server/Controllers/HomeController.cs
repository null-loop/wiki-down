using System.Web.Mvc;
using System.Web.Routing;

namespace wiki_down.content.server.controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return new TransferToRouteResult("ArticleView.GlobalId",new RouteValueDictionary()
            {
                {"globalId","home"}
            });
        }
    }
}