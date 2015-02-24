using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoJavascriptFunctionData
    {
        public string Id { get; set; }
        public BsonJavaScript Value { get; set; }
    }
}