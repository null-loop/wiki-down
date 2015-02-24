using System.Collections.Generic;

namespace wiki_down.core
{
    public interface IArticle : IExistsInArticleTree, IRevisable, ITitlable, IIndexable
    {
        List<string> Keywords { get; }

        string Markdown { get; }
    }
}