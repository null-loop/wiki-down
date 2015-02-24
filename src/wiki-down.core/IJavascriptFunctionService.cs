namespace wiki_down.core
{
    public interface IJavascriptFunctionService
    {
        string GetFunction(string functionName);
        void StoreFunction(string functionName, string function);
    }
}