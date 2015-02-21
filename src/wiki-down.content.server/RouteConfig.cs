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

            // article/g/markdown-examples
            routes.MapRoute(name: "ArticleView.GlobalId", url: "article/g/{globalId}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByGlobalId" });
            // article/p/home.markdown-examples
            routes.MapRoute(name: "ArticleView.Path", url: "article/p/{path}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByPath" });
            // article/t/g/examples/markdown-examples
            routes.MapRoute(name: "ArticleView.Template.GlobalId", url: "article/t/g/{template}/{globalId}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByGlobalIdWithTemplate" });
            // article/t/p/examples/markdown-examples
            routes.MapRoute(name: "ArticleView.Template.Path", url: "article/t/p/{template}/{path}", defaults: new { controller = "ArticleViewer", action = "ViewArticleByPathWithTemplate" });

            routes.MapRoute(name: "Home", url: "", defaults: new { controller = "Home", action = "Index" });


        }
    }
}