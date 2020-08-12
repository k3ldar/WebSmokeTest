using System.Collections.Generic;

using SharedPluginFeatures;

namespace SmokeTest.Middleware
{
    public class SeoProvider : ISeoProvider
    {
        #region ISeoProvider Methods

        public bool AddKeyword(in string route, in string keyword)
        {
            return false;
        }

        public bool AddKeywords(in string route, in List<string> keyword)
        {
            return false;
        }

        public bool GetSeoDataForRoute(in string route, out string title, out string metaDescription, out string author, out List<string> keywords)
        {
            title = "Smoke Test";
            metaDescription = title;
            author = title;
            keywords = new List<string>();
            return false;
        }

        public bool RemoveKeyword(in string route, in string keyword)
        {
            return false;
        }

        public bool RemoveKeywords(in string route, in List<string> keyword)
        {
            return false;
        }

        public bool UpdateAuthor(in string route, in string author)
        {
            return false;
        }

        public bool UpdateDescription(in string route, in string description)
        {
            return false;
        }

        public bool UpdateTitle(in string route, in string title)
        {
            return false;
        }

        #endregion ISeoProvider Methods
    }
}
