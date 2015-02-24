using wiki_down.core.storage;

namespace wiki_down.core
{
    public interface ISystemAuditService
    {
        void Audit(string area, AuditAction action, string path, string actionedBy, int revision);
    }
}