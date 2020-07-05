using System;

namespace SmokeTest.Shared
{
    public sealed class NameValueModel
    {
        public NameValueModel(in string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(NameValueModel));

            Name = name;
            Value = name;
        }

        public NameValueModel(in string name, in string value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(NameValueModel));

            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            Name = name;
            Value = value;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }
    }
}
