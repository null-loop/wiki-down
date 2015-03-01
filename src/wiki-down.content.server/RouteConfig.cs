using System.Web.Mvc;
using System.Web.Routing;
using wiki_down.core.templates;

namespace wiki_down.content.server
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var defaultTemplate = Templates.Default();

            // editor/templates/page/homepage
            routes.MapRoute(name: "Editor.PageTemplate", url: "editor/templates/page/{template}", defaults: new { controller = "Editor", action = "PageTemplate", template = defaultTemplate });
            // editor/templates/article/homepage
            routes.MapRoute(name: "Editor.ArticleTemplate", url: "editor/templates/article/{template}", defaults: new { controller = "Editor", action = "ArticleTemplate", template = defaultTemplate });
            // editor/javascript/wikidown_markdown_to_html
            routes.MapRoute(name: "Editor.StoredJavascriptFunction", url: "editor/javascript/{functionName}", defaults: new { controller = "Editor", action = "StoredJavascriptFunction"});

            // article/markdown-examples
            routes.MapRoute(name: "ArticleView.GlobalId", url: "article/{globalId}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByGlobalId" });
            // article/t/examples/markdown-examples
            routes.MapRoute(name: "ArticleView.Template.GlobalId", url: "article/t/{template}/{globalId}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByGlobalIdWithTemplate" });

            // sys

            // sys/logging

            // sys/audit

            // sys/config

            // home
            routes.MapRoute(name: "Home", url: "", defaults: new { controller = "Home", action = "Index" });

            // robots

            routes.MapRoute(name: "Robots.txt", url: "robots.txt", defaults: new {controller = "Utility", action = "Robots"});
        }
    }
}