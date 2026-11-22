using System.Collections.Generic;

namespace Bitshift.Services.Localization
{
    internal static class TweaksTranslations
    {
        public static void Register(IDictionary<string, Dictionary<string, string>> target)
        {
            target["custom"] = new() { ["pt"] = "Customização", ["en"] = "Customization" };
            target["customTitle"] = new() { ["pt"] = "Scripts Customizados", ["en"] = "Custom Scripts" };
            target["customDesc"] = new() { ["pt"] = "Selecione opções individuais para montar sua otimização ideal.", ["en"] = "Select individual options to build your ideal optimization." };
            target["applySelected"] = new() { ["pt"] = "Aplicar Selecionados", ["en"] = "Apply Selected" };
            target["executing"] = new() { ["pt"] = "Executando...", ["en"] = "Executing..." };
            target["applied"] = new() { ["pt"] = "Aplicado!", ["en"] = "Applied!" };
            target["catPerf"] = new() { ["pt"] = "Desempenho", ["en"] = "Performance" };
            target["catPriv"] = new() { ["pt"] = "Privacidade", ["en"] = "Privacy" };
            target["catNet"] = new() { ["pt"] = "Rede", ["en"] = "Network" };
            target["catSys"] = new() { ["pt"] = "Sistema", ["en"] = "System" };
            target["catUi"] = new() { ["pt"] = "Interface", ["en"] = "Interface" };
            target["selectAll"] = new() { ["pt"] = "Marcar Todos", ["en"] = "Select All" };
            target["clearAll"] = new() { ["pt"] = "Desmarcar Todos", ["en"] = "Uncheck All" };
            target["confirmExec"] = new() { ["pt"] = "Confirmar Execução", ["en"] = "Confirm Execution" };

            // Batch Tweaks
            target["tweakRestorePoint"] = new() { ["pt"] = "Criar Ponto de Restauração", ["en"] = "Create System Restore Point" };
            target["tweakRestorePointDesc"] = new() { ["pt"] = "Gera um ponto de restauração no Windows antes de qualquer alteração.", ["en"] = "Creates a Windows restore point before applying any changes." };
            target["tweakServices"] = new() { ["pt"] = "Serviços Desnecessários para Manual", ["en"] = "Set Non-Essential Services to Manual" };
            target["tweakServicesDesc"] = new() { ["pt"] = "Altera serviços de telemetria, biometria e Xbox para inicialização sob demanda.", ["en"] = "Sets telemetry, biometrics, and Xbox services to on-demand manual start." };
            target["tweakRecall"] = new() { ["pt"] = "Desativar Windows Recall (Copilot+)", ["en"] = "Disable Windows Recall (Copilot+)" };
            target["tweakRecallDesc"] = new() { ["pt"] = "Bloqueia a captura contínua de tela e logs de IA do Windows 11 Recall.", ["en"] = "Blocks background screen captures and AI logging from Windows 11 Recall." };
            target["tweakTelemetry"] = new() { ["pt"] = "Desativar Telemetria & Diagnósticos", ["en"] = "Reduce Telemetry & Diagnostics" };
            target["tweakTelemetryDesc"] = new() { ["pt"] = "Define o nível de telemetria e diagnósticos do sistema para o mínimo de segurança.", ["en"] = "Sets system telemetry and diagnostics to minimal security level." };
            target["tweakPower"] = new() { ["pt"] = "Plano de Energia de High Performance", ["en"] = "High Performance Power Plan" };
            target["tweakPowerDesc"] = new() { ["pt"] = "Ativa o plano High Performance nativo e desativa suspensão de portas USB e discos.", ["en"] = "Activates native High Performance plan and disables USB/disk sleep latency spikes." };
            target["tweakVisual"] = new() { ["pt"] = "Otimizações de Fluidez & Visual", ["en"] = "Visual Fluidity & Latency Tweaks" };
            target["tweakVisualDesc"] = new() { ["pt"] = "Acelera animações de janelas, menus e resposta de cliques na interface.", ["en"] = "Accelerates window animations, menu speeds, and input responsiveness." };
            target["tweakNetwork"] = new() { ["pt"] = "Otimização de Rede & Latência (TCP)", ["en"] = "Network & Latency Optimization (TCP)" };
            target["tweakNetworkDesc"] = new() { ["pt"] = "Ajusta parâmetros de buffer TCP/IP, acelera resolução DNS e diminui ping de pacotes.", ["en"] = "Tunes TCP/IP buffers, accelerates DNS caching, and reduces packet ping jitter." };
            target["tweakPrivacy"] = new() { ["pt"] = "Privacidade Avançada & Rastreamento", ["en"] = "Advanced Privacy & Ad Tracking" };
            target["tweakPrivacyDesc"] = new() { ["pt"] = "Desativa ID de publicidade, sugestões de inicialização e histórico de digitação.", ["en"] = "Disables advertising ID, start menu suggestions, and typing history tracking." };
            target["tweakStartup"] = new() { ["pt"] = "Limpeza de Inicialização", ["en"] = "Startup Cleanup & Delay Removal" };
            target["tweakStartupDesc"] = new() { ["pt"] = "Desativa atrasos artificiais de boot no registro do Windows no carregamento de aplicativos.", ["en"] = "Removes artificial startup delay keys in the Windows registry during system logon." };
            target["tweakBloatware"] = new() { ["pt"] = "Remoção de Bloatware & Apps UWP", ["en"] = "Remove Bloatware & UWP Apps" };
            target["tweakBloatwareDesc"] = new() { ["pt"] = "Remove pacotes promocionais do Windows (Cortana, Notícias, Jogos pré-instalados).", ["en"] = "Removes preinstalled promotional Windows apps and useless background UWP packages." };
            target["tweakCleanup"] = new() { ["pt"] = "Limpeza Profunda de Cache e Temporários", ["en"] = "Deep Cache & Temp Cleanup" };
            target["tweakCleanupDesc"] = new() { ["pt"] = "Limpa caches de atualização do Windows, arquivos temporários de usuários e Prefetch.", ["en"] = "Clears Windows Update caches, user temp folders, crash dumps, and stale prefetch." };
            target["tweakWin11Debloat"] = new() { ["pt"] = "Debloat Windows 11 Completo", ["en"] = "Complete Windows 11 Debloat" };
            target["tweakWin11DebloatDesc"] = new() { ["pt"] = "Expurga apps UWP, Copilot, Recall, widgets da barra de tarefas e anúncios patrocinados.", ["en"] = "Removes UWP bloatware, Copilot, Recall, taskbar widgets, and sponsored suggestions." };
        }
    }
}
