using System.Linq;

namespace wiki_down.core
{
    public static class Ids
    {
        private static readonly char[] ValidGlobalIdChars = new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','0','1','2','3','4','5','6','7','8','9','(',')','-','_'};

        public static bool IsValidPathFormat(string path)
        {
            var r = path.Replace(".", "");
            if (!IsValidGlobalIdFormat(r)) return false;
            if (path.Contains("..")) return false;
            return true;
        }

        public static bool IsValidGlobalIdFormat(string globalId)
        {
            return globalId.All(c => ValidGlobalIdChars.Contains(c));
        }

        public static void ValidateGlobalId(string globalId)
        {
            if (!IsValidGlobalIdFormat(globalId))
            {
                throw new InvalidGlobalIdException("Invalid global id : " + globalId);
            }
        }

        public static void ValidatePath(string path)
        {
            if (!IsValidPathFormat(path))
            {
                throw new InvalidPathException("Invalid path : " + path);
            }
        }
    }
}