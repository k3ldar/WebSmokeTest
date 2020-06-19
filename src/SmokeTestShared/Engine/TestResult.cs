using System;
using System.Collections.Generic;
using System.Text;

namespace SmokeTest.Shared.Engine
{
    public sealed class TestResult
    {
        public string Name { get; set; }

        public int Position { get; set; }

        public decimal TimeTaken { get; set; }

        public int ErrorCount { get; set; }
    }
}
