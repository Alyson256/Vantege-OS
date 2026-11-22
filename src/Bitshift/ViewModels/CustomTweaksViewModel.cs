using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Bitshift.Models;
using Bitshift.Services;
using Bitshift.Services.Abstractions;

namespace Bitshift.ViewModels
{
    public partial class CustomTweaksViewModel : ObservableObject, IDisposable
    {
        private readonly ITweakService _tweakService;
        private readonly IBatchExecutionService _batchService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private bool _disposed;

        private readonly ObservableCollection<TweakItem> _allTweaks = new();
        public ObservableCollection<TweakItem> FilteredTweaks { get; } = new();
        public ObservableCollection<ConsoleLogEntry> ConsoleLogs { get; } = new();

        [ObservableProperty]
        private string activeCategory = "all";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySelectedCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearAllCommand))]
        [NotifyCanExecuteChangedFor(nameof(UndoAllCommand))]
        private bool isExecuting;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySelectedCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearAllCommand))]
        private bool hasSelectedTweaks;

        [ObservableProperty]
        private string executionStatus = "Pronto para aplicar";

        [ObservableProperty]
        private string selectedCountText = "0 selecionados";

        // Localized labels
        [ObservableProperty] private string pageTitle = "Scripts Customizados";
        [ObservableProperty] private string pageDesc = "Selecione opções individuais para montar sua otimização ideal.";
        [ObservableProperty] private string applySelectedLabel = "Aplicar Selecionados";
        [ObservableProperty] private string selectAllLabel = "Marcar Todos";
        [ObservableProperty] private string clearAllLabel = "Desmarcar Todos";

        public CustomTweaksViewModel(
            ITweakService tweakService,
            IBatchExecutionService batchService,
            ILocalizationService localizationService,
            IDialogService dialogService)
        {
            _tweakService = tweakService;
            _batchService = batchService;
            _localizationService = localizationService;
            _dialogService = dialogService;

            _batchService.OutputReceived += OnBatchOutputReceived;
            _localizationService.LanguageChanged += UpdateLocalizedTexts;

            foreach (var summary in _tweakService.GetAllTweaks())
            {
                var definition = summary.Definition;
                var tweak = new TweakItem
                {
                    Id = definition.Id,
                    ScriptFile = definition.ScriptFile,
                    NameKey = definition.NameKey,
                    DescKey = definition.DescKey,
                    DefaultName = definition.DefaultName,
                    DefaultDesc = definition.DefaultDesc,
                    Category = definition.Category,
                    IsApplied = summary.IsApplied
                };
                _allTweaks.Add(tweak);
                tweak.PropertyChanged += OnTweakPropertyChanged;
            }

            UpdateLocalizedTexts();
            RefreshFilteredList();
        }

        private void OnTweakPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TweakItem.IsSelected))
            {
                UpdateSelectedCount();
            }
        }

        private void OnBatchOutputReceived(string line)
        {
            if (_disposed) return;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                if (_disposed) return;
                ConsoleLogs.Insert(0, ConsoleLogEntry.FromRawLine(line));
                if (ConsoleLogs.Count > 100)
                {
                    ConsoleLogs.RemoveAt(ConsoleLogs.Count - 1);
                }
            });
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = _localizationService.Get("customTitle");
            PageDesc = _localizationService.Get("customDesc");
            ApplySelectedLabel = _localizationService.Get("applySelected");
            SelectAllLabel = _localizationService.Get("selectAll");
            ClearAllLabel = _localizationService.Get("clearAll");

            foreach (var tweak in _allTweaks)
            {
                string localizedName = _localizationService.Get(tweak.NameKey);
                string localizedDesc = _localizationService.Get(tweak.DescKey);
                tweak.LocalizedName = localizedName == tweak.NameKey ? tweak.DefaultName : localizedName;
                tweak.LocalizedDesc = localizedDesc == tweak.DescKey ? tweak.DefaultDesc : localizedDesc;
            }
        }

        [RelayCommand]
        private void SetCategory(string category)
        {
            ActiveCategory = category;
            RefreshFilteredList();
        }

        private void RefreshFilteredList()
        {
            FilteredTweaks.Clear();
            var filtered = ActiveCategory switch
            {
                "performance" => _allTweaks.Where(t => t.Category == "performance"),
                "privacy" => _allTweaks.Where(t => t.Category == "privacy"),
                "network" => _allTweaks.Where(t => t.Category == "network"),
                "system" => _allTweaks.Where(t => t.Category == "system"),
                "interface" => _allTweaks.Where(t => t.Category == "interface"),
                _ => _allTweaks
            };

            foreach (var tweak in filtered)
            {
                FilteredTweaks.Add(tweak);
            }

            UpdateSelectedCount();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var tweak in FilteredTweaks)
            {
                tweak.IsSelected = true;
            }
            UpdateSelectedCount();
        }

        private bool CanClearAll() => HasSelectedTweaks && !IsExecuting;

        [RelayCommand(CanExecute = nameof(CanClearAll))]
        private void ClearAll()
        {
            if (!_dialogService.Confirm(
                    "Desmarcar Todos os Ajustes",
                    "Deseja desmarcar todos os ajustes selecionados em todas as categorias?"))
            {
                return;
            }

            foreach (var tweak in _allTweaks)
            {
                tweak.IsSelected = false;
            }
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            int count = _allTweaks.Count(t => t.IsSelected);
            SelectedCountText = $"{count} selecionado(s)";
            HasSelectedTweaks = count > 0;
        }

        private bool CanApplySelected() => HasSelectedTweaks && !IsExecuting;

        [RelayCommand(CanExecute = nameof(CanApplySelected))]
        private async Task ApplySelectedAsync()
        {
            var selected = _allTweaks.Where(t => t.IsSelected).ToList();
            if (selected.Count == 0 || IsExecuting) return;

            if (!_dialogService.Confirm(
                    "Confirmar Otimizações",
                    $"Deseja realmente aplicar os {selected.Count} scripts de otimização selecionados no Windows?"))
            {
                return;
            }

            IsExecuting = true;
            bool hadFailures = false;
            try
            {
                ExecutionStatus = "Executando otimizações selecionadas...";
                foreach (var tweak in selected)
                {
                    ExecutionStatus = $"Executando: {tweak.LocalizedName}...";
                    var result = await _tweakService.ExecuteTweakAsync(tweak.Id);
                    tweak.IsApplied = result.Success;
                    if (!result.Success)
                    {
                        hadFailures = true;
                        ExecutionStatus = string.IsNullOrWhiteSpace(result.Error)
                            ? $"Falha ao executar: {tweak.LocalizedName}."
                            : $"Falha ao executar {tweak.LocalizedName}: {result.Error}";
                    }
                }

                if (!hadFailures)
                {
                    ExecutionStatus = "Otimizações aplicadas com sucesso!";
                }
            }
            catch (Exception ex)
            {
                ExecutionStatus = $"Erro ao aplicar otimizações: {ex.Message}";
            }
            finally
            {
                IsExecuting = false;
            }

            await Task.Delay(3000);
            if (!_disposed && !IsExecuting)
            {
                ExecutionStatus = "Pronto para aplicar";
            }
        }

        private bool CanUndoAll() => !IsExecuting;

        [RelayCommand(CanExecute = nameof(CanUndoAll))]
        private async Task UndoAllAsync()
        {
            if (IsExecuting) return;

            if (!_dialogService.Confirm(
                    "Confirmar Reversão (Undo)",
                    "Deseja reverter as otimizações para os padrões do Windows?",
                    DialogMessageType.Warning))
            {
                return;
            }

            IsExecuting = true;
            try
            {
                ExecutionStatus = "Revertendo alterações para padrões do Windows...";
                var res = await _tweakService.UndoAllAsync();
                if (res.Success)
                {
                    foreach (var tweak in _allTweaks)
                    {
                        tweak.IsApplied = false;
                    }
                    ExecutionStatus = "Configurações restauradas com sucesso!";
                }
                else
                {
                    ExecutionStatus = string.IsNullOrWhiteSpace(res.Error)
                        ? "Reversão finalizada com avisos."
                        : $"Falha ao reverter: {res.Error}";
                }
            }
            catch (Exception ex)
            {
                ExecutionStatus = $"Erro ao reverter: {ex.Message}";
            }
            finally
            {
                IsExecuting = false;
            }
        }

        [RelayCommand]
        private void ClearLogs()
        {
            ConsoleLogs.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var tweak in _allTweaks)
            {
                tweak.PropertyChanged -= OnTweakPropertyChanged;
            }
            _batchService.OutputReceived -= OnBatchOutputReceived;
            _localizationService.LanguageChanged -= UpdateLocalizedTexts;
            GC.SuppressFinalize(this);
        }
    }
}
