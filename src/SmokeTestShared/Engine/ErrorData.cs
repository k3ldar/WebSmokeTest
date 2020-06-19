using System;

namespace SmokeTest.Shared.Engine
{
    public sealed class ErrorData
    {
        public ErrorData()
        {

        }

        public ErrorData(in Exception error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            DateTime = DateTime.UtcNow;
        }

        public ErrorData(in Exception error, in Uri uri)
            : this (error)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public ErrorData(in Exception error, in Uri uri, in Uri missingLink,
            in Uri originatingLink)
            : this(error, uri)
        {
            MissingLink = missingLink ?? uri;
            OriginatingLink = originatingLink ?? uri;
        }

        public DateTime DateTime { get; set; }

        public Exception Error { get; set; }

        public Uri Uri { get; set; }

        public Uri MissingLink { get; set; }

        public Uri OriginatingLink { get; set; }

        public int Index { get; set; }
    }
}
