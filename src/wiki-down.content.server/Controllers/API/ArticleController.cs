using System;
using System.Web.Http;
using wiki_down.core;

namespace wiki_down.content.server.Controllers.API
{
    public class ArticleController : ApiController
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        public IHttpActionResult GetByGlobalId(string globalId)
        {
            if (_articleService.HasArticleByGlobalId(globalId))
            {
                var article = _articleService.GetArticleByGlobalId(globalId);
                return Ok(new ApiArticle()
                {
                    Title = article.Title,
                    GlobalId = article.GlobalId,
                    Path = article.Path,
                    ParentArticlePath = article.ParentArticlePath,
                    AllowChildren = article.IsAllowedChildren,
                    Indexed = article.ShowInIndex,
                    Markdown = article.Markdown
                });
            }
            else
            {
                return NotFound();
            }
        }

        public IHttpActionResult GetByPath(string path)
        {
            throw new NotImplementedException();
        }
    }

    public class ApiArticle
    {
        public string GlobalId { get; set; }

        public string Path { get; set; }

        public string Title { get; set; }

        public string ParentArticlePath { get; set; }

        public bool AllowChildren { get; set; }

        public bool Indexed { get; set; }

        public string Markdown { get; set; }
    }
}
