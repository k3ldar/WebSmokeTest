using System.Collections.Generic;

using SmokeTest.Shared.Interfaces;

namespace SmokeTest.UnitTests.Mocks
{
    public class LoadDataMock : ILoadData
    {
        public T Load<T>(in string path, in string fileName)
        {
            return default;
        }

        public void Load<T>(in List<T> dataList, in string path, in string fileExtension)
        {

        }
    }
}
