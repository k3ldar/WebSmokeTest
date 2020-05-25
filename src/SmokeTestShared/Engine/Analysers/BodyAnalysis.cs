using System.Collections.Generic;

namespace SmokeTest.Shared.Engine
{
    public sealed class BodyAnalysis
    {
        #region Constructors

        public BodyAnalysis()
        {
            Forms = new List<FormAnalysis>();
        }

        #endregion Constructors

        public int Div { get; set; }

        public int Header1 { get; set; }

        public int Header2 { get; set; }

        public int Header3 { get; set; }

        public int Header4 { get; set; }

        public int Header5 { get; set; }

        public int Header6 { get; set; }

        public int CssDocuments { get; set; }

        public int Scripts { get; set; }

        public int OrderedList { get; set; }

        public int UnorderedList { get; set; }

        public List<FormAnalysis> Forms { get; set; }

        public void Clear()
        {
            Header1 = 0;
            Header2 = 0;
            Header3 = 0;
            Header4 = 0;
            Header5 = 0;
            Header6 = 0;
            CssDocuments = 0;
            Scripts = 0;
            OrderedList = 0;
            UnorderedList = 0;
            Div = 0;

            Forms.ForEach(f => f.Clear());
        }
    }
}
