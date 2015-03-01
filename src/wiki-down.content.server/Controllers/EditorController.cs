using System.Web.Mvc;
using wiki_down.core;

namespace wiki_down.content.server.controllers
{
    public class EditorController : Controller
    {
        private readonly IJavascriptFunctionService _javascriptFunctionService;

        public EditorController(IJavascriptFunctionService javascriptFunctionService)
        {
            _javascriptFunctionService = javascriptFunctionService;
        }

        public ActionResult PageTemplate(string template)
        {
            //TODO:Templating!
            return View();
        }

        public ActionResult ArticleTemplate(string template)
        {
            //TODO:Templating!
            return View();
        }

        [OutputCache(Duration = 300)]
        public ActionResult StoredJavascriptFunction(string functionName)
        {
            var function = _javascriptFunctionService.GetFunction(functionName);

            function = "function " + functionName + function.Substring(8);

            return JavaScript(function);
        }
    }
}