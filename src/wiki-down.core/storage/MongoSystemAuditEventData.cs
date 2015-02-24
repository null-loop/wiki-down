using System;
using MongoDB.Bson;

namespace wiki_down.core.storage
{
    public class MongoSystemAuditEventData : IMongoData
    {
        public ObjectId Id { get; set; }

        public string Area { get; set; }

        public AuditAction Action { get; set; }

        public string Path { get; set; }

        public string ActionedBy { get; set; }

        public DateTime ActionedOn { get; set; }

        public int Revision { get; set; }
    }
}