namespace Bitshift.Services.Abstractions
{
    public interface ILauncher
    {
        void OpenUrl(string url);
        bool RelaunchAsAdministrator();
    }
}
