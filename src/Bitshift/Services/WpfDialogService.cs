using System.Windows;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public sealed class WpfDialogService : IDialogService
    {
        public bool Confirm(string title, string message, DialogMessageType type = DialogMessageType.Question)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                ToImage(type)) == MessageBoxResult.Yes;
        }

        public void Show(string title, string message, DialogMessageType type = DialogMessageType.Information)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                ToImage(type));
        }

        private static MessageBoxImage ToImage(DialogMessageType type)
        {
            return type switch
            {
                DialogMessageType.Question => MessageBoxImage.Question,
                DialogMessageType.Warning => MessageBoxImage.Warning,
                DialogMessageType.Error => MessageBoxImage.Error,
                _ => MessageBoxImage.Information
            };
        }
    }
}
