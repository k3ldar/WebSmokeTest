using System;

namespace SmokeTest.Shared.Engine
{
    public class FormInput
    {
        public FormInput()
        {
            Value = String.Empty;
            Type = String.Empty;
            Id = String.Empty;
            Name = String.Empty;
            Contents = String.Empty;
        }
        internal string Value { get; set; }

        public string Type { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Contents { get; set; }
    }
}
