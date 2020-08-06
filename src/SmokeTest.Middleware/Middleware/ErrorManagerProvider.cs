using System;

using PluginManager.Abstractions;

using SharedPluginFeatures;

namespace SmokeTest.Middleware
{
    public class ErrorManagerProvider : IErrorManager
    {
        #region Private Members

        private readonly ILogger _logger;

        #endregion Private Members

        #region Constructor

        public ErrorManagerProvider(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion Constructor

        #region IErrorManager Methods

        public void ErrorRaised(in ErrorInformation errorInformation)
        {
            _logger.AddToLog(PluginManager.LogLevel.Error, errorInformation.Error);
        }

        public bool MissingPage(in string path, ref string replacePath)
        {
            return false;
        }

        #endregion IErrorManager Methods
    }
}
