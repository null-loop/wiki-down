using System;

namespace wiki_down.core
{
    public interface IRevisable
    {
        int Revision { get; }

        string RevisedBy { get; }

        DateTime RevisedOn { get; }

        bool IsAllowedChildren { get; set; }
    }
}