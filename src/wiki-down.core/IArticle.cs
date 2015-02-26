using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace wiki_down.core
{
    public interface IArticle : IExistsInArticleTree, IRevisable, ITitlable, IIndexable
    {
        List<string> Keywords { get; }

        string Markdown { get; }
    }

    public static class Ids
    {
        private static readonly char[] ValidGlobalIdChars = new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3','4','5','6','7','8','9','(',')','-','_'};

        public static bool IsValidPath(string path)
        {
            var r = path.Replace(".", "");
            if (!IsValidGlobalId(r)) return false;
            if (path.Contains("..")) return false;
            return true;
        }

        public static bool IsValidGlobalId(string globalId)
        {
            return globalId.All(c => ValidGlobalIdChars.Contains(c));
        }

        public static void ValidateGlobalId(string globalId)
        {
            if (!IsValidGlobalId(globalId))
            {
                throw new InvalidGlobalIdException("Invalid global id : " + globalId);
            }
        }

        public static void ValidatePath(string path)
        {
            if (!IsValidPath(path))
            {
                throw new InvalidPathException("Invalid path : " + path);
            }
        }
    }

    [Serializable]
    public class InvalidPathException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidPathException()
        {
        }

        public InvalidPathException(string message) : base(message)
        {
        }

        public InvalidPathException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidPathException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

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