using System;

namespace wiki_down.core.templates
{
    public class ArticlePredicateTemplateSelectionStrategy : ITemplateSelectionStrategy
    {
        private readonly Func<IArticle, bool> _predicate;
        private readonly string _template;

        public ArticlePredicateTemplateSelectionStrategy(Func<IArticle, bool> predicate, string template)
        {
            _predicate = predicate;
            _template = template;
        }

        public string SelectTemplate(IArticle article)
        {
            return _predicate(article) ? _template : null;
        }
    }
}