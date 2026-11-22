using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public class TrayIconService : ITrayIconService
    {
        private readonly ILocalizationService _localizationService;
        private WinForms.NotifyIcon? _notifyIcon;
        private WinForms.ContextMenuStrip? _contextMenu;
        private WinForms.ToolStripMenuItem? _openItem;
        private WinForms.ToolStripMenuItem? _cleanRamItem;
        private WinForms.ToolStripMenuItem? _exitItem;
        private Icon? _trayIconBitmap;
        private Font? _menuFont;
        private Font? _boldMenuFont;

        private Action? _onOpenRequested;
        private Func<Task>? _onCleanRamRequested;
        private Action? _onExitRequested;
        private bool _isCleaningRam;
        private bool _disposed;

        public bool IsVisible => _notifyIcon?.Visible ?? false;

        public TrayIconService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            _localizationService.LanguageChanged += RefreshLocalizedLabels;
        }

        public void Initialize(Action onOpenRequested, Func<Task> onCleanRamRequested, Action onExitRequested)
        {
            if (_notifyIcon != null || _disposed) return;

            _onOpenRequested = onOpenRequested;
            _onCleanRamRequested = onCleanRamRequested;
            _onExitRequested = onExitRequested;

            _menuFont = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Regular);
            _boldMenuFont = new Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);

            _contextMenu = new WinForms.ContextMenuStrip
            {
                Renderer = new WinForms.ToolStripProfessionalRenderer(new DarkTrayColorTable()),
                ShowImageMargin = false,
                Font = _menuFont
            };

            _openItem = new WinForms.ToolStripMenuItem
            {
                Font = _boldMenuFont,
                ForeColor = Color.White
            };
            _openItem.Click += (_, _) => DispatchToUi(() => _onOpenRequested?.Invoke());

            _cleanRamItem = new WinForms.ToolStripMenuItem
            {
                ForeColor = Color.FromArgb(228, 228, 231)
            };
            _cleanRamItem.Click += async (_, _) => await HandleCleanRamClickAsync();

            _exitItem = new WinForms.ToolStripMenuItem
            {
                ForeColor = Color.FromArgb(251, 113, 133)
            };
            _exitItem.Click += (_, _) => DispatchToUi(() => _onExitRequested?.Invoke());

            _contextMenu.Items.Add(_openItem);
            _contextMenu.Items.Add(_cleanRamItem);
            _contextMenu.Items.Add(new WinForms.ToolStripSeparator());
            _contextMenu.Items.Add(_exitItem);

            _trayIconBitmap = LoadApplicationIcon();

            _notifyIcon = new WinForms.NotifyIcon
            {
                Icon = _trayIconBitmap,
                ContextMenuStrip = _contextMenu,
                Visible = false
            };

            _notifyIcon.MouseClick += (_, e) =>
            {
                if (e.Button == WinForms.MouseButtons.Left)
                {
                    DispatchToUi(() => _onOpenRequested?.Invoke());
                }
            };

            _notifyIcon.DoubleClick += (_, _) => DispatchToUi(() => _onOpenRequested?.Invoke());

            RefreshLocalizedLabels();
        }

        public void SetVisible(bool visible)
        {
            if (_notifyIcon == null || _disposed) return;
            _notifyIcon.Visible = visible;
        }

        public void ShowNotification(string title, string message, bool isError = false)
        {
            if (_notifyIcon == null || _disposed || !_notifyIcon.Visible) return;

            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = isError
                ? WinForms.ToolTipIcon.Error
                : WinForms.ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(2500);
        }

        private async Task HandleCleanRamClickAsync()
        {
            if (_isCleaningRam || _onCleanRamRequested == null) return;
            _isCleaningRam = true;

            try
            {
                await _onCleanRamRequested();
            }
            finally
            {
                _isCleaningRam = false;
            }
        }

        private void RefreshLocalizedLabels()
        {
            if (_notifyIcon == null || _openItem == null || _cleanRamItem == null || _exitItem == null)
            {
                return;
            }

            string tooltip = _localizationService.Get("trayTooltip");
            _notifyIcon.Text = tooltip.Length > 63 ? tooltip.Substring(0, 63) : tooltip;
            _openItem.Text = _localizationService.Get("trayOpen");
            _cleanRamItem.Text = _localizationService.Get("trayCleanRam");
            _exitItem.Text = _localizationService.Get("trayExit");
        }

        private static void DispatchToUi(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }

        private static Icon LoadApplicationIcon()
        {
            try
            {
                var resourceStream = Application.GetResourceStream(
                    new Uri("pack://application:,,,/Assets/bitshift.ico"));
                if (resourceStream?.Stream != null)
                {
                    using var stream = resourceStream.Stream;
                    return new Icon(stream);
                }
            }
            catch
            {
                // Fallback safely if icon resource fails to load
            }

            return (Icon)SystemIcons.Application.Clone();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _localizationService.LanguageChanged -= RefreshLocalizedLabels;

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _contextMenu?.Dispose();
            _contextMenu = null;

            _trayIconBitmap?.Dispose();
            _trayIconBitmap = null;

            _boldMenuFont?.Dispose();
            _boldMenuFont = null;

            _menuFont?.Dispose();
            _menuFont = null;

            GC.SuppressFinalize(this);
        }

        private sealed class DarkTrayColorTable : WinForms.ProfessionalColorTable
        {
            private static readonly Color Surface = Color.FromArgb(20, 20, 29);
            private static readonly Color Hover = Color.FromArgb(39, 39, 42);
            private static readonly Color BorderColor = Color.FromArgb(63, 63, 70);

            public override Color ToolStripDropDownBackground => Surface;
            public override Color ImageMarginGradientBegin => Surface;
            public override Color ImageMarginGradientMiddle => Surface;
            public override Color ImageMarginGradientEnd => Surface;
            public override Color MenuBorder => BorderColor;
            public override Color MenuItemBorder => Hover;
            public override Color MenuItemSelected => Hover;
            public override Color MenuItemSelectedGradientBegin => Hover;
            public override Color MenuItemSelectedGradientEnd => Hover;
            public override Color SeparatorDark => BorderColor;
            public override Color SeparatorLight => BorderColor;
        }
    }
}
