using System;

namespace SmokeTest.Configuration.Models
{
    public class TestConfigurationViewDetailsModel
    {
        #region Constructors

        public TestConfigurationViewDetailsModel(in string name, in string uniqueId)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrEmpty(uniqueId))
                throw new ArgumentNullException(nameof(uniqueId));

            Name = name;
            UniqueId = uniqueId;
        }

        #endregion Constructors

        #region Properties

        public string Name { get; private set; }

        public string UniqueId { get; private set; }

        #endregion Properties
    }
}
