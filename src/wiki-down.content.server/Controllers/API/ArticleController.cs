using System.Web.Http;

namespace wiki_down.content.server.Controllers.API
{
    public class ArticleController : ApiController
    {
        public IHttpActionResult Get(string id)
        {
            return Ok(new Article()
            {
                Title = "This Is Test Content!",
                GlobalId = "TestContent",
                Path = "Home.TestContent",
                ParentArticlePath = "Home",

                AllowChildren = true,
                Indexed = false,
                Draft = true,
                Markdown = "#This is some test content\r\nIt's good isn't it?\r\n##More tea vicar?\r\nI can put whatever I want in here!"
            });
        }
    }

    public class Article
    {
        public string GlobalId { get; set; }

        public string Path { get; set; }

        public string Title { get; set; }

        public string ParentArticlePath { get; set; }

        public bool AllowChildren { get; set; }

        public bool Indexed { get; set; }

        public bool Draft { get; set; }

        public string Markdown { get; set; }
    }
}
