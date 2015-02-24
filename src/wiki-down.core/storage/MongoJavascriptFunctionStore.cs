using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoJavascriptFunctionStore : MongoStorage, IJavascriptFunctionService
    {
        public MongoJavascriptFunctionStore() : base("system.js")
        {
        }

        public string GetFunction(string functionName)
        {
            var collection = GetCollection<MongoJavascriptFunctionData>();
            var function = collection.Find(Query.EQ("_id", functionName)).FirstOrDefault();
            if (function == null) return null;
            return function.Value.Code;
        }

        public void StoreFunction(string functionName, string function)
        {
            var collection = GetCollection<MongoJavascriptFunctionData>();
            var data = new MongoJavascriptFunctionData()
            {
                Id = functionName,
                Value = function
            };
            var hasExisting = collection.Find(Query.EQ("_id", functionName)).Any();
            if (hasExisting)
            {
                collection.Save(data);
            }
            else
            {
                collection.Insert(data);
            }
        }
    }
}
