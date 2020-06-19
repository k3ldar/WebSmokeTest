using System;

namespace SmokeTest.Reports.Models
{
    public class ErrorDataModel
    {
        public ErrorDataModel(in string error, in Uri uri, in Uri missingLink,
            in Uri originatingLink)
        {
            if (String.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));

            Error = error;
            Uri = uri;
            MissingLink = missingLink;
            OriginatingLink = originatingLink;
        }

        public DateTime DateTime { get; set; }

        public string Error { get; set; }

        public Uri Uri { get; set; }

        public Uri MissingLink { get; set; }

        public Uri OriginatingLink { get; set; }

        public int Index { get; set; }
    }
}
