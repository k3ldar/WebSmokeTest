namespace WebSmokeTest.Engine
{
    public sealed class WebPageResults
    {
        internal WebPageResults(string page)
        {
            Page = page;
            Maximum = double.MinValue;
            Minimum = double.MaxValue;
        }

        internal string Page { get; private set; }

        internal int Requests { get; private set; }

        internal double Minimum { get; private set; }

        internal double Maximum { get; private set; }

        internal double Average { get; private set; }

        private double TotalTime { get; set; }

        internal void AddRequest(string page, double timeTaken)
        {
            Requests++;

            if (timeTaken < Minimum)
                Minimum = timeTaken;

            if (timeTaken > Maximum)
                Maximum = timeTaken;

            TotalTime += timeTaken;

            Average = TotalTime / Requests;
        }
    }
}
