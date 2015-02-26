using System;

namespace wiki_down.core
{
    public interface IArticleContent
    {
        ArticleContentFormat Format { get; }

        string GeneratedBy { get; }

        string Content { get; }

        DateTime GeneratedOn { get; }

        string GlobalId { get; }

        string Path { get; }
    }
}