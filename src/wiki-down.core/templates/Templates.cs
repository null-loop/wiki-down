using System;
using System.Collections.Generic;

namespace wiki_down.core.templates
{
    public static class Templates
    {
        public static void DefaultTo(string defaultTemplate)
        {
            _defaultTemplate = defaultTemplate;
            For(a => true).Use(_defaultTemplate);
        }

        private static readonly List<ITemplateSelectionStrategy> Strategies = new List<ITemplateSelectionStrategy>();
        private static string _defaultTemplate;

        public static string Select(IArticle article)
        {
            foreach (var strategy in Strategies)
            {
                var r = strategy.SelectTemplate(article);
                if (r != null) return r;
            }
            return "default";
        }

        public static ITemplateStrategyResolver For(Func<IArticle, bool> predicate)
        {
            return new ArticlePredicateStrategyResolver(Strategies, predicate);
        }

        public static string Default()
        {
            return _defaultTemplate;
        }
    }
}