using System;
using System.Web.Mvc;
using wiki_down.content.server.viewmodels;
using wiki_down.core;

namespace wiki_down.content.server.controllers
{
    public class ArticleViewerController : Controller
    {

        private IArticleService _articleService;
        private IGeneratedArticleContentService _generatedArticleContentService;

        public ArticleViewerController(IGeneratedArticleContentService generatedArticleContentService, IArticleService articleService)
        {
            _generatedArticleContentService = generatedArticleContentService;
            _articleService = articleService;
        }

        public ActionResult ViewArticleByGlobalId(string globalId)
        {
            if (!Ids.IsValidGlobalIdFormat(globalId)) return HttpNotFound();
            var articleContent = _generatedArticleContentService.GetGeneratedArticleContentByGlobalId(globalId, ArticleContentFormat.Html);
            var article = _articleService.GetArticleByGlobalId(globalId);
            return BuildResult(articleContent, article);
        }

        private ActionResult BuildResult(IArticleContent articleContent, IArticle article)
        {
            return View("article", new ArticleContentViewModel()
            {
                Content = articleContent.Content,
                Format = articleContent.Format,
                GeneratedBy = articleContent.GeneratedBy,
                GeneratedOn = articleContent.GeneratedOn,
                Path = articleContent.Path,
                GlobalId = articleContent.GlobalId,
                Title = article.Title,
                Revision = article.Revision,
                RevisedBy = article.RevisedBy,
                RevisedOn = article.RevisedOn,
                Keywords = article.Keywords,
            });
        }

        public ActionResult ViewArticleByGlobalIdWithTemplate(string globalId, string template)
        {
            if (!Ids.IsValidGlobalIdFormat(globalId)) return HttpNotFound();
            var articleContent = _generatedArticleContentService.GetGeneratedArticleContentByGlobalId(globalId, ArticleContentFormat.Html);
            throw new NotImplementedException();
        }

    }
}