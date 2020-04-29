using System;

namespace WebSmokeTest.Engine
{
    public sealed class FormTextArea
    {
        public FormTextArea()
        {
            Id = String.Empty;
            Name = String.Empty;
            Rows = String.Empty;
            Columns = String.Empty;
            Value = String.Empty;
        }
        public string Id { get; set; }

        public string Name { get; set; }

        public string Rows { get; set; }

        public string Columns { get; set; }

        public string Value { get; set; }
    }
}
