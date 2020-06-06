using System;
using System.Collections.Generic;

namespace SmokeTest.Shared.Engine
{
    public class FormReport
    {
        public FormReport()
        {
            AdditionalLinks = new List<Uri>();
            Status = FormStatus.New;
        }

        public Uri Url { get; set; }

        public string Id { get; set; }

        public string Method { get; set; }

        public string Action { get; set; }

        public FormStatus Status { get; set; }

        public List<Uri> AdditionalLinks { get; set; }
    }

}
