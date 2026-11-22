using System.Collections.Generic;

namespace Bitshift.Services.Localization
{
    internal static class DashboardTranslations
    {
        public static void Register(IDictionary<string, Dictionary<string, string>> target)
        {
            target["dashboard"] = new() { ["pt"] = "Dashboard", ["en"] = "Dashboard" };
            target["dashDesc"] = new() { ["pt"] = "Visão geral do sistema e limpeza rápida.", ["en"] = "System overview and quick cleaning." };
            target["sysUse"] = new() { ["pt"] = "Uso do Sistema ao Vivo", ["en"] = "Live System Usage" };
            target["latAnalysis"] = new() { ["pt"] = "Análise de Latência do Sistema", ["en"] = "System Latency Analysis" };
            target["reliability"] = new() { ["pt"] = "Confiabilidade para Real-Time", ["en"] = "Real-Time Reliability" };
            target["excellent"] = new() { ["pt"] = "Excelente", ["en"] = "Excellent" };
            target["ramClean"] = new() { ["pt"] = "Limpeza Rápida de RAM", ["en"] = "Quick RAM Clean" };
            target["optimizing"] = new() { ["pt"] = "Otimizando cache...", ["en"] = "Optimizing cache..." };
            target["freed"] = new() { ["pt"] = "Memória Liberada!", ["en"] = "Memory Freed!" };
            target["optComplete"] = new() { ["pt"] = "Otimização Concluída", ["en"] = "Optimization Complete" };
            target["ramFreedMsg"] = new() { ["pt"] = "de RAM liberados com sucesso.", ["en"] = "of RAM successfully freed." };

            // RAM & Disk cleanup
            target["ramTitle"] = new() { ["pt"] = "Otimização de RAM", ["en"] = "RAM Optimization" };
            target["ramDesc"] = new() { ["pt"] = "Libera memória em cache e processos suspensos", ["en"] = "Frees cached memory and suspended processes" };
            target["ramUsed"] = new() { ["pt"] = "Em Uso", ["en"] = "In Use" };
            target["ramAvailable"] = new() { ["pt"] = "Disponível", ["en"] = "Available" };
            target["optimize"] = new() { ["pt"] = "Otimizar", ["en"] = "Optimize" };
            target["optimizingRam"] = new() { ["pt"] = "Otimizando...", ["en"] = "Optimizing..." };
            target["optimized"] = new() { ["pt"] = "Otimizado!", ["en"] = "Optimized!" };
            target["junkFilesTitle"] = new() { ["pt"] = "Limpeza de Disco", ["en"] = "Disk Cleanup" };
            target["junkFilesDesc"] = new() { ["pt"] = "Arquivos temporários e cache do sistema", ["en"] = "Temporary files and system cache" };
            target["tempFiles"] = new() { ["pt"] = "Arquivos Temporários", ["en"] = "Temp Files" };
            target["freeSpace"] = new() { ["pt"] = "Espaço Livre", ["en"] = "Free Space" };
            target["cleanJunk"] = new() { ["pt"] = "Limpar Lixo", ["en"] = "Clean Junk" };
            target["cleaningJunk"] = new() { ["pt"] = "Limpando...", ["en"] = "Cleaning..." };
            target["junkCleaned"] = new() { ["pt"] = "Limpo!", ["en"] = "Cleaned!" };
        }
    }
}
