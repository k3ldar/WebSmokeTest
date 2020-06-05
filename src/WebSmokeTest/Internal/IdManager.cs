using System;
using System.Threading;

using SmokeTest.Shared;

namespace SmokeTest.Internal
{
    internal sealed class IdManager : IIdManager
    {
        #region Private Members

        private static long _lastId = DateTime.Now.Ticks;

        #endregion Private Members

        #region IIdManager Methods

        public long GenerateId()
        {
            long Result = DateTime.Now.Ticks;

            while (Result.Equals(_lastId))
            {
                Thread.Sleep(0);
                Result = DateTime.Now.Ticks;
            }

            _lastId = Result;

            return Result;
        }

        #endregion IIdManager Methods
    }
}
