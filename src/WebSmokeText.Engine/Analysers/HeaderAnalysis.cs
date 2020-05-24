using System;

namespace SmokeTest.Engine
{
    public sealed class HeaderAnalysis
    {
        public string Charset { get; set; }

        public string ViewPort { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public string Title { get; set; }

        public int CssDocuments { get; set; }

        public int Scripts { get; set; }

        public void Clear()
        {
            Charset = String.Empty;
            Author = String.Empty;
            Description = String.Empty;
            Keywords = String.Empty;
            Title = String.Empty;

            CssDocuments = 0;
            Scripts = 0;
        }
    }
}
