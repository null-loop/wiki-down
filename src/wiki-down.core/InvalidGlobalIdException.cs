using System;
using System.Runtime.Serialization;

namespace wiki_down.core
{
    [Serializable]
    public class InvalidGlobalIdException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidGlobalIdException()
        {
        }

        public InvalidGlobalIdException(string message) : base(message)
        {
        }

        public InvalidGlobalIdException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidGlobalIdException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}