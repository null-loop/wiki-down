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
            if (!Ids.IsValidGlobalId(globalId)) return HttpNotFound();
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
                Keywords = article.Keywords
            });
        }

        public ActionResult ViewArticleByGlobalIdWithTemplate(string globalId, string template)
        {
            if (!Ids.IsValidGlobalId(globalId)) return HttpNotFound();
            var articleContent = _generatedArticleContentService.GetGeneratedArticleContentByGlobalId(globalId, ArticleContentFormat.Html);
            throw new NotImplementedException();
        }

        public ActionResult ViewArticleByPath(string path)
        {
            path = path.Replace("_", ".");
            if (!Ids.IsValidPath(path)) return HttpNotFound();
            var articleContent = _generatedArticleContentService.GetGeneratedArticleContentByPath(path, ArticleContentFormat.Html);
            var article = _articleService.GetArticleByPath(path);
            return BuildResult(articleContent, article);
        }

        public ActionResult ViewArticleByPathWithTemplate(string path, string template)
        {
            if (!Ids.IsValidPath(path)) return HttpNotFound();
            var articleContent = _generatedArticleContentService.GetGeneratedArticleContentByPath(path, ArticleContentFormat.Html);
            throw new NotImplementedException();
        }
    }
}