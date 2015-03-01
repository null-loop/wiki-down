using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoConfigurationArticleData : IMongoData
    {
        public ObjectId Id { get; set; }

        public string System { get; set; }

        public string ConfigurationSetName { get; set; }

        public int Revision { get; set; }

        public DateTime RevisedOn { get; set; }

        public string RevisedBy { get; set; }
    }
}