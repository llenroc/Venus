namespace Venus.Infrastructure
{
    public interface IFileService
    {
        void WriteData(string filePath, byte[] data);
        byte[] ReadData(string filePath);
    }
}