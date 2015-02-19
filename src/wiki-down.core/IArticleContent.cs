namespace wiki_down.core
{
    public interface IArticleContent
    {
        ArticleContentFormat Format { get; }

        string GeneratedBy { get; }

        string Content { get; set; }
    }
}