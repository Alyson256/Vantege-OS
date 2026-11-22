using System.Collections.Generic;

namespace Bitshift.Services.Localization
{
    internal static class SettingsTranslations
    {
        public static void Register(IDictionary<string, Dictionary<string, string>> target)
        {
            target["settings"] = new() { ["pt"] = "Configurações", ["en"] = "Settings" };
            target["settingsDesc"] = new() { ["pt"] = "Personalize o comportamento da janela, bandeja do sistema e preferências do aplicativo.", ["en"] = "Customize window behavior, system tray integration, and application preferences." };
            target["settingsTraySection"] = new() { ["pt"] = "Bandeja do Sistema (System Tray)", ["en"] = "System Tray" };
            target["settingsMinimizeToTray"] = new() { ["pt"] = "Minimizar para a bandeja do sistema", ["en"] = "Minimize to system tray" };
            target["settingsMinimizeToTrayDesc"] = new() { ["pt"] = "Ao clicar em minimizar, oculta a barra de tarefas e ativa o ícone do BITSHIFT na bandeja do sistema.", ["en"] = "When minimizing, hides the taskbar icon and activates the BITSHIFT system tray icon." };
            target["settingsTrayNotify"] = new() { ["pt"] = "Exibir notificação ao minimizar", ["en"] = "Show notification on minimize" };
            target["settingsTrayNotifyDesc"] = new() { ["pt"] = "Exibe um aviso discreto do Windows informando que o BITSHIFT segue ativo em segundo plano.", ["en"] = "Displays a subtle Windows toast indicating BITSHIFT remains active in the background." };
            target["settingsGeneralSection"] = new() { ["pt"] = "Preferências Gerais", ["en"] = "General Preferences" };
            target["settingsLanguageTitle"] = new() { ["pt"] = "Idioma da Interface", ["en"] = "Interface Language" };
            target["settingsLanguageDesc"] = new() { ["pt"] = "Define o idioma usado em telas, logs e menu da bandeja.", ["en"] = "Sets the display language used across views, logs, and tray menu." };
            target["settingsThemeTitle"] = new() { ["pt"] = "Tema da Aparência", ["en"] = "Appearance Theme" };
            target["settingsThemeDesc"] = new() { ["pt"] = "Alterna entre modo escuro e modo claro na interface.", ["en"] = "Switches between dark mode and light mode." };
            target["settingsAccentTitle"] = new() { ["pt"] = "Cor de Destaque do Windows", ["en"] = "Windows Accent Color" };
            target["settingsAccentDesc"] = new() { ["pt"] = "Sincroniza automaticamente com a cor configurada no tema do Windows 10/11.", ["en"] = "Automatically syncs with your Windows 10/11 desktop accent color." };
            target["settingsReloadAccent"] = new() { ["pt"] = "Recarregar Cor", ["en"] = "Reload Accent" };
            target["settingsResetDefaults"] = new() { ["pt"] = "Restaurar Padrões", ["en"] = "Reset Defaults" };
            target["settingsTestMinimize"] = new() { ["pt"] = "Minimizar Agora", ["en"] = "Minimize Now" };
            target["settingsAutoUpdate"] = new() { ["pt"] = "Verificar atualizações ao iniciar", ["en"] = "Check for updates on startup" };
            target["settingsAutoUpdateDesc"] = new() { ["pt"] = "Consulta automaticamente o repositório oficial ao abrir o BITSHIFT para verificar novas versões.", ["en"] = "Automatically checks the official repository when BITSHIFT starts for new versions." };

            // System Tray
            target["trayTooltip"] = new() { ["pt"] = "BITSHIFT — Otimizador do Windows", ["en"] = "BITSHIFT — Windows Optimizer" };
            target["trayOpen"] = new() { ["pt"] = "Abrir BITSHIFT", ["en"] = "Open BITSHIFT" };
            target["trayCleanRam"] = new() { ["pt"] = "Limpar cache de memória RAM", ["en"] = "Clean RAM memory cache" };
            target["trayExit"] = new() { ["pt"] = "Sair", ["en"] = "Exit" };
            target["trayMinimizedMsg"] = new() { ["pt"] = "O BITSHIFT foi minimizado para a bandeja do sistema. Clique com o botão direito no ícone para opções rápidas.", ["en"] = "BITSHIFT was minimized to the system tray. Right-click the icon for quick actions." };
            target["trayRamCleanedMsg"] = new() { ["pt"] = "Cache de memória RAM limpo com sucesso!", ["en"] = "RAM memory cache cleaned successfully!" };
            target["trayRamErrorMsg"] = new() { ["pt"] = "Não foi possível limpar o cache de memória RAM.", ["en"] = "Unable to clean RAM memory cache." };
        }
    }
}
