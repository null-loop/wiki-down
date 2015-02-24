namespace wiki_down.core
{
    public interface ISystemLoggingService
    {
        void Debug(string system, string area, string type, string message);
        void Info(string system, string area, string type, string message);
        void Warn(string system, string area, string type, string message);
        void Error(string system, string area, string type, string message);
        void Fatal(string system, string area, string type, string message);
    }
}