using SmokeTest.Shared;

namespace SmokeTest.UnitTests.Mocks
{
    public class SaveDataMock : ISaveData
    {
        public bool Save<T>(T data, in string path, in string fileName)
        {
            return true;
        }
    }
}
