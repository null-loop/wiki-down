using System.Web.Http;

namespace wiki_down.content.server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("API.Articles.Get.Path", "api/article/p/{path}", new {controller = "Article", action = "GetByPath"});
            config.Routes.MapHttpRoute("API.Articles.Get.GlobalId", "api/article/g/{globalId}", new { controller = "Article", action = "GetByGlobalId" });

            config.Routes.MapHttpRoute("API.Articles.GetInitialMetaData.GlobalId", "api/article-meta/g/{globalId}", new { controller = "Article", action = "GetInitialMetaDataByGlobalId" });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            var jsonFormatter = config.Formatters.JsonFormatter;
            config.Formatters.Remove(jsonFormatter);
            config.Formatters.Clear();
            config.Formatters.Add(jsonFormatter);
        }
    }
}