using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VantageOS.Models;
using VantageOS.Services;

namespace VantageOS.ViewModels
{
    public partial class SystemSpecsViewModel : ObservableObject
    {
        private readonly ISystemSpecService _specService;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string titleLabel = "Especificações do PC";

        [ObservableProperty]
        private string descriptionLabel = "Visualização detalhada do hardware conectado.";

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private ObservableCollection<SpecCategoryDisplay> categories = new();

        public SystemSpecsViewModel(ISystemSpecService specService, ILocalizationService localizationService)
        {
            _specService = specService;
            _localizationService = localizationService;

            _localizationService.LanguageChanged += OnLanguageChanged;
            UpdateLocalizedTexts();

            _ = LoadSpecsAsync();
        }

        private void OnLanguageChanged()
        {
            UpdateLocalizedTexts();
            // Re-localize loaded categories
            foreach (var cat in Categories)
            {
                cat.Name = _localizationService.Get(cat.NameKey);
                foreach (var item in cat.Items)
                {
                    item.Label = _localizationService.Get(item.LabelKey);
                }
            }
        }

        private void UpdateLocalizedTexts()
        {
            TitleLabel = _localizationService.Get("systemSpecs");
            DescriptionLabel = _localizationService.Get("systemSpecsDesc");
        }

        private async Task LoadSpecsAsync()
        {
            IsLoading = true;
            var specs = await _specService.GetSpecsAsync();

            var displayCats = new ObservableCollection<SpecCategoryDisplay>();
            foreach (var cat in specs.Categories)
            {
                var display = new SpecCategoryDisplay
                {
                    NameKey = cat.Name,
                    Name = _localizationService.Get(cat.Name),
                    ColorKey = cat.ColorKey
                };

                foreach (var item in cat.Items)
                {
                    display.Items.Add(new SpecItemDisplay
                    {
                        LabelKey = item.Label,
                        Label = _localizationService.Get(item.Label),
                        Value = item.Value
                    });
                }

                displayCats.Add(display);
            }

            Categories = displayCats;
            IsLoading = false;
        }
    }

    // Display models with observable properties for live localization
    public partial class SpecCategoryDisplay : ObservableObject
    {
        [ObservableProperty]
        private string nameKey = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string colorKey = "Blue500Brush";

        public ObservableCollection<SpecItemDisplay> Items { get; set; } = new();
    }

    public partial class SpecItemDisplay : ObservableObject
    {
        [ObservableProperty]
        private string labelKey = string.Empty;

        [ObservableProperty]
        private string label = string.Empty;

        [ObservableProperty]
        private string value = string.Empty;
    }
}
