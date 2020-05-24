using System.Collections.Generic;

namespace SmokeTest.Shared.Interfaces
{
    public interface ILoadData
    {
        T Load<T>(in string path, in string fileName);

        void Load<T>(in List<T> dataList, in string path, in string fileExtension);
    }
}
