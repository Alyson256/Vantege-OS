namespace Bitshift.Services.Abstractions
{
    public enum DialogMessageType
    {
        Information,
        Question,
        Warning,
        Error
    }

    public interface IDialogService
    {
        bool Confirm(string title, string message, DialogMessageType type = DialogMessageType.Question);
        void Show(string title, string message, DialogMessageType type = DialogMessageType.Information);
    }
}
