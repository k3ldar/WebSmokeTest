using System;

namespace WebSmokeTest.Engine
{
    public sealed class FormButton
    {
        public FormButton()
        {
            Name = String.Empty;
            Type = String.Empty;
            Value = String.Empty;
            Id = String.Empty;
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string Id { get; set; }
    }
}
