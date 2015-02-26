using System;

namespace wiki_down.core
{
    public interface ISystemConfigurationService
    {
        TConfiguration GetConfiguration<TConfiguration>() where TConfiguration:class;
        TConfiguration GetConfiguration<TConfiguration>(string systemName) where TConfiguration : class;

        void SetDefaultConfiguration<T>(Action<T> action, string author) where T : class;

        void SetConfiguration<T>(Action<T> action, string system, string author) where T : class;
    }
}