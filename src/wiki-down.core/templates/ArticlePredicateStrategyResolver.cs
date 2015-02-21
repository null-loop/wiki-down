using System;
using System.Collections.Generic;

namespace wiki_down.core.templates
{
    public class ArticlePredicateStrategyResolver : ITemplateStrategyResolver
    {
        private readonly List<ITemplateSelectionStrategy> _strategies;
        private readonly Func<IArticle, bool> _predicate;

        public ArticlePredicateStrategyResolver(List<ITemplateSelectionStrategy> strategies, Func<IArticle, bool> predicate)
        {
            _strategies = strategies;
            _predicate = predicate;
        }

        public void Use(string template)
        {
            _strategies.Add(new ArticlePredicateTemplateSelectionStrategy(_predicate, template));
        }
    }
}