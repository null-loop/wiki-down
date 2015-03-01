using System;
using System.Web.Http;
using wiki_down.core;

namespace wiki_down.content.server.controllers.API
{
    public class ArticleController : ApiController
    {
        private const int DefaultPageSize = 20;
        private readonly IArticleService _articleService;
        private readonly IArticleMetaDataService _articleMetaDataService;

        public ArticleController(IArticleService articleService, IArticleMetaDataService articleMetaDataService)
        {
            _articleService = articleService;
            _articleMetaDataService = articleMetaDataService;
        }

        public IHttpActionResult GetByGlobalId(string globalId)
        {
            if (!Ids.IsValidGlobalIdFormat(globalId)) return NotFound();

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
            if (!Ids.IsValidPathFormat(path)) return NotFound();
            throw new NotImplementedException();
        }

        public IHttpActionResult GetInitialMetaDataByGlobalId(string globalId)
        {
            if (!Ids.IsValidGlobalIdFormat(globalId)) return NotFound();
            // return set of meta-data, first history page, navigation and statistics - once we can generate all those!

            var historyPage = _articleMetaDataService.GetHistoryPageByGlobalId(globalId, 0, DefaultPageSize);
            var metaData = _articleMetaDataService.GetCompleteMetaDataByGlobalId(globalId);
            var navigationStructure = _articleMetaDataService.GetNavigationStructureByGlobalId(globalId);
            var statisticsData = _articleMetaDataService.GetStatisticsDataByGlobalId(globalId);

            return Ok(CreateArticleInitialMetaData(metaData, historyPage, navigationStructure, statisticsData));
 
        }

        private ApiArticleIntialMetaData CreateArticleInitialMetaData(IArticleExtendedMetaData metaData, IArticleHistoryPage historyPage, IArticleNavigationStructure navigationStructure, IArticleStatistics statisticsData)
        {
            return new ApiArticleIntialMetaData()
            {

            };
        }

        public IHttpActionResult GetHistoryByGlobalId(string globalId, int page = 0, int pageSize = DefaultPageSize)
        {
            if (!Ids.IsValidGlobalIdFormat(globalId)) return NotFound();
            if (page < 0) return NotFound();
            const int maxPageSize = 200;
            if (pageSize > maxPageSize) return BadRequest("pageSize too large. Max size is " + maxPageSize);
            throw new NotImplementedException();
        }
    }

    public class ApiArticleIntialMetaData
    {
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
