using System;
using System.Runtime.Serialization;

namespace wiki_down.core
{
    [Serializable]
    public class MissingArticleException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MissingArticleException()
        {
        }

        public MissingArticleException(string message) : base(message)
        {
        }

        public MissingArticleException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MissingArticleException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}