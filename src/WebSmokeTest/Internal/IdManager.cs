using System;
using System.Threading;

using SmokeTest.Shared;

namespace SmokeTest.Internal
{
    internal sealed class IdManager : IIdManager
    {
        #region Private Members

        private static long _lastId = DateTime.UtcNow.Ticks;

        #endregion Private Members

        #region IIdManager Methods

        public long GenerateId()
        {
            long Result = DateTime.UtcNow.Ticks;

            while (Result.Equals(_lastId))
            {
                Thread.Sleep(0);
                Result = DateTime.UtcNow.Ticks;
            }

            _lastId = Result;

            return Result;
        }

        #endregion IIdManager Methods
    }
}
