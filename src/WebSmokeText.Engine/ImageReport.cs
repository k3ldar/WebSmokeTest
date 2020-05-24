using System;

namespace SmokeTest.Engine
{
    public sealed class ImageReport
    {
        private byte[] _imageData;

        public ImageReport()
        {
            Bytes = new byte[] { };
        }

        public ImageReport(in string url)
            : this()
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; set; }

        public byte[] Bytes
        {
            get
            {
                return _imageData;
            }

            set
            {
                _imageData = value;

                if (Size == 0)
                    Size = _imageData.Length;
            }
        }

        public double LoadTime { get; set; }

        public int Size { get; set; }
    }
}
