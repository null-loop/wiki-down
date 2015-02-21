namespace wiki_down.core.templates
{
    public interface ITemplateSelectionStrategy
    {
        string SelectTemplate(IArticle article);
    }
}