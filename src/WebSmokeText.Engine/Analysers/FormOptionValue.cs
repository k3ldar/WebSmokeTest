using System;

namespace WebSmokeTest.Engine
{
    public sealed class FormOptionValue
    {
        public FormOptionValue()
        {
            Value = String.Empty;
            Selected = String.Empty;
            Text = String.Empty;
        }

        public string Value { get; set; }

        public string Selected { get; set; }

        public string Text { get; set; }
    }
}
