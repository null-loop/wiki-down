using wiki_down.core.templates;

namespace wiki_down.content.server
{
    public static class TemplatesConfig
    {
        public static void Configure()
        {
            Templates.For(a => string.IsNullOrEmpty(a.ParentArticlePath)).Use("homepage");
            Templates.DefaultTo("default");
        }
    }
}