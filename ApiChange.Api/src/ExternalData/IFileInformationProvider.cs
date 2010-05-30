

namespace ApiChange.ExternalData
{
    public interface IFileInformationProvider
    {
        string GetCheckinUser(string fileName);
        UserInfo GetInformationFromFile(string fileName);
    }
}