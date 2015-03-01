using wiki_down.core.storage;

namespace wiki_down.core.config
{
    public interface ILoggingConfiguration
    {
        long MaximumDataStoreSize { get; set; }

        LoggingLevel MinimumLoggingLevel { get; set; }
    }

    public interface ICoreConfiguration
    {
        bool AllowMultipleRoots { get; set; }
    }
}