using System.Collections.Generic;
using Bitshift.Models;

namespace Bitshift.Services
{
    internal static class TweakCatalog
    {
        public static IReadOnlyList<TweakDefinition> Create()
        {
            return new[]
            {
                new TweakDefinition("sys_restore_point", "01_restore_point.bat", "tweakRestorePoint", "tweakRestorePointDesc", "Criar Ponto de Restauração", "Gera um ponto de restauração no Windows antes de qualquer alteração.", "system"),
                new TweakDefinition("perf_services", "02_services_manual.bat", "tweakServices", "tweakServicesDesc", "Serviços Desnecessários para Manual", "Altera serviços de telemetria, biometria e Xbox para inicialização sob demanda.", "performance"),
                new TweakDefinition("priv_recall", "03_disable_recall.bat", "tweakRecall", "tweakRecallDesc", "Desativar Windows Recall (Copilot+)", "Bloqueia a captura contínua de tela e logs de IA do Windows 11 Recall.", "privacy"),
                new TweakDefinition("priv_telemetry", "04_disable_telemetry.bat", "tweakTelemetry", "tweakTelemetryDesc", "Desativar Telemetria & Diagnósticos", "Define o nível de telemetria e diagnósticos do sistema para o mínimo de segurança.", "privacy"),
                new TweakDefinition("perf_power", "05_power_plan.bat", "tweakPower", "tweakPowerDesc", "Plano de Energia de Alto Desempenho", "Ativa o plano High Performance nativo e desativa suspensão de portas USB e discos.", "performance"),
                new TweakDefinition("ui_visual", "06_visual_tweaks.bat", "tweakVisual", "tweakVisualDesc", "Otimizações de Fluidez & Visual", "Acelera animações de janelas, menus e resposta de cliques na interface.", "interface"),
                new TweakDefinition("net_tcp", "07_network_tweaks.bat", "tweakNetwork", "tweakNetworkDesc", "Otimização de Rede & Latência (TCP)", "Ajusta parâmetros de buffer TCP/IP, acelera resolução DNS e diminui ping de pacotes.", "network"),
                new TweakDefinition("priv_privacy", "08_privacy_tweaks.bat", "tweakPrivacy", "tweakPrivacyDesc", "Privacidade Avançada & Rastreamento", "Desativa ID de publicidade, sugestões de inicialização e histórico de digitação.", "privacy"),
                new TweakDefinition("sys_startup", "09_startup_cleanup.bat", "tweakStartup", "tweakStartupDesc", "Limpeza de Inicialização", "Desativa atrasos artificiais de boot no registro do Windows no carregamento de aplicativos ao inicializar.", "system"),
                new TweakDefinition("sys_bloatware", "11_remove_bloatware.bat", "tweakBloatware", "tweakBloatwareDesc", "Remoção de Bloatware & Apps UWP", "Remove pacotes promocionais do Windows (Cortana, Notícias, Jogos pré-instalados).", "system"),
                new TweakDefinition("sys_cleanup", "12_system_cleanup.bat", "tweakCleanup", "tweakCleanupDesc", "Limpeza Profunda de Cache e Temporários", "Limpa caches de atualização do Windows, arquivos temporários de usuários e Prefetch.", "system"),
                new TweakDefinition("sys_win11_debloat", "13_win11_debloat.bat", "tweakWin11Debloat", "tweakWin11DebloatDesc", "Debloat Windows 11 Completo", "Expurga apps UWP, Copilot, Recall, widgets da barra de tarefas e anúncios patrocinados.", "system")
            };
        }
    }
}
