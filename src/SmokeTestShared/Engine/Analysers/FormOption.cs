using System;
using System.Collections.Generic;

namespace SmokeTest.Shared.Engine
{
    public class FormOption
    {
        public FormOption()
        {
            Options = new List<FormOptionValue>();
            Type = String.Empty;
            Id = String.Empty;
            Name = String.Empty;
            Contents = String.Empty;
        }

        public List<FormOptionValue> Options { get; set; }

        public string Type { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Contents { get; set; }
    }
}
