namespace Bitshift.Services.Abstractions
{
    public interface IClipboardService
    {
        bool TryCopy(string text);
    }
}
