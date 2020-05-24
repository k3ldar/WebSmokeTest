namespace SmokeTest.Shared
{
    public interface ISaveData
    {
        bool Save<T>(T data, in string path, in string fileName);
    }
}
