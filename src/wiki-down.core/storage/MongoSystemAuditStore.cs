using System;
using MongoDB.Driver.Builders;

namespace wiki_down.core.storage
{
    public class MongoSystemAuditStore : MongoStorage<MongoSystemAuditEventData>, ISystemAuditService
    {
        public MongoSystemAuditStore() : base("sys-audit")
        {
        }

        public void Audit(string area, AuditAction action, string path, string actionedBy, int revision)
        {
            GetCollection().Insert(new MongoSystemAuditEventData()
            {
                Area = area,
                Action = action,
                ActionedBy = actionedBy,
                Path = path,
                ActionedOn = DateTime.UtcNow,
                Revision = revision
            });
        }

        public override void Configure()
        {
            var collection = GetCollection();

            collection.CreateIndex(new IndexKeysBuilder().Descending("ActionedOn"), IndexOptions.SetUnique(false));
            collection.CreateIndex(new IndexKeysBuilder().Descending("Path"), IndexOptions.SetUnique(false));
            collection.CreateIndex(new IndexKeysBuilder().Descending("Area"), IndexOptions.SetUnique(false));
        }
    }
}