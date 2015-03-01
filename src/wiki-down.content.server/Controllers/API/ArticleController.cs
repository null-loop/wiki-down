using System;
using System.Linq;
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
            var statistics = _articleMetaDataService.GetStatisticsByGlobalId(globalId);
            var activeDraft = _articleService.GetDraftMetaData("home", "anonymous");


            return Ok(CreateArticleInitialMetaData(metaData, historyPage, navigationStructure, statistics, activeDraft));
 
        }

        private ApiArticleIntialMetaData CreateArticleInitialMetaData(IExtendedArticleMetaData metaData, IArticleHistoryPage historyPage, IArticleNavigationStructure navigationStructure, IArticleStatistics statistics, IExtendedArticleMetaData activeDraft)
        {
            return new ApiArticleIntialMetaData()
            {
                MetaData = ConvertMetaData(metaData),
                History = ConvertHistory(historyPage),
                Statistics = ConvertStatistics(statistics),
                NavigationStructure = ConvertNavigationStructure(navigationStructure),
                ActiveDraft = ConvertActiveDraft(activeDraft)
            };
        }

        private ApiArticleDraftMetaData ConvertActiveDraft(IExtendedArticleMetaData activeDraft)
        {
            if (activeDraft != null)
            {
                return new ApiArticleDraftMetaData()
                {
                    Revision = activeDraft.Revision,
                    RevisedOn = activeDraft.RevisedOn,
                    RevisedBy = activeDraft.RevisedBy,
                    HasActiveDraft = true
                };
            }
            else
            {
                return new ApiArticleDraftMetaData()
                {
                    HasActiveDraft = false
                };
            }
            
        }

        private ApiArticleNavigationStructure ConvertNavigationStructure(IArticleNavigationStructure navigationStructure)
        {
            return new ApiArticleNavigationStructure();
        }

        private ApiArticleStatistics ConvertStatistics(IArticleStatistics statistics)
        {
            return new ApiArticleStatistics();
        }

        private ApiArticleExtendedMetaData ConvertMetaData(IExtendedArticleMetaData metaData)
        {
            return new ApiArticleExtendedMetaData();
        }

        private ApiArticleHistoryMetaData ConvertHistory(IArticleHistoryPage historyPage)
        {
            return new ApiArticleHistoryMetaData()
            {
                GlobalId = historyPage.GlobalId,
                Page = historyPage.Page,
                PageSize = historyPage.PageSize,
                //TODO:History entries!
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

    public class ApiArticleDraftMetaData
    {
        public bool HasActiveDraft { get; set; }
        public string RevisedBy { get; set; }
        public DateTime RevisedOn { get; set; }
        public int Revision { get; set; }
    }

    public class ApiArticleHistoryMetaData
    {
        public string GlobalId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ApiArticleIntialMetaData
    {
        
        public ApiArticleExtendedMetaData MetaData { get; set; }
        public ApiArticleHistoryMetaData History { get; set; }
        public ApiArticleNavigationStructure NavigationStructure { get; set; }
        public ApiArticleStatistics Statistics { get; set; }
        public ApiArticleDraftMetaData ActiveDraft { get; set; }
    }

    public class ApiArticleNavigationStructure
    {
    }

    public class ApiArticleStatistics
    {
    }

    public class ApiArticleExtendedMetaData
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
