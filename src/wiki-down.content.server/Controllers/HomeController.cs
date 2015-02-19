using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace GSIP.Tools.WikiDown.ContentServer.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return new TransferToRouteResult("ArticleView",new RouteValueDictionary()
            {
                {"globalId","Home"}
            });
        }
    }
}