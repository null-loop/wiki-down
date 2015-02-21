using System;
using System.Runtime.Serialization;

namespace wiki_down.core
{
    [Serializable]
    public class MissingDraftException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MissingDraftException()
        {
        }

        public MissingDraftException(string message) : base(message)
        {
        }

        public MissingDraftException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MissingDraftException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}