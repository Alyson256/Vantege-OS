using CommunityToolkit.Mvvm.ComponentModel;

namespace Bitshift.Models
{
    public partial class TweakItem : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public string ScriptFile { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string DescKey { get; set; } = string.Empty;
        public string DefaultName { get; set; } = string.Empty;
        public string DefaultDesc { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "performance", "privacy", "network", "system", "interface"

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isApplied;

        [ObservableProperty]
        private bool isExecuting;

        [ObservableProperty]
        private string localizedName = string.Empty;

        [ObservableProperty]
        private string localizedDesc = string.Empty;
    }
}
