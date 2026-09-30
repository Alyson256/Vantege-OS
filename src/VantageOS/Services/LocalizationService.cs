using System;
using System.Collections.Generic;

namespace VantageOS.Services
{
    public class LocalizationService : ILocalizationService
    {
        private string _currentLanguage = "pt";
        public string CurrentLanguage => _currentLanguage;
        public event Action? LanguageChanged;

        // Dicionário completo portado do LanguageContext.tsx
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            // Tooltips
            ["tooltipDpc"] = new() { ["pt"] = "Mede o tempo máximo gasto por drivers processando tarefas adiadas. Valores altos causam engasgos (stuttering) no sistema.", ["en"] = "Measures the maximum time spent by drivers processing deferred tasks. High values cause system stuttering." },
            ["tooltipIsr"] = new() { ["pt"] = "Mede o tempo que o processador gasta lidando com interrupções de hardware (como mouse/teclado). Valores altos causam atrasos (input lag).", ["en"] = "Measures the time the processor spends handling hardware interrupts (like mouse/keyboard). High values cause input lag." },
            ["tooltipPagefaults"] = new() { ["pt"] = "Ocorre quando o sistema precisa buscar no disco dados que deveriam estar na RAM. Quanto menor, mais rápido o sistema responde.", ["en"] = "Occurs when the system has to fetch data from the disk that should be in the RAM. The lower, the faster the system responds." },
            ["tooltipReliability"] = new() { ["pt"] = "Indica a capacidade do sistema de lidar com áudio, vídeo e jogos sem engasgos, quedas de quadros ou desfoque de movimento.", ["en"] = "Indicates the system's ability to handle audio, video, and gaming without stutters, dropped frames, or motion blur." },
            ["tooltipCpu"] = new() { ["pt"] = "Unidade Central de Processamento. Mede o nível de atividade do processador principal.", ["en"] = "Central Processing Unit. Measures the activity level of the main processor." },
            ["tooltipGpu"] = new() { ["pt"] = "Unidade de Processamento Gráfico. Mede o nível de atividade da placa de vídeo.", ["en"] = "Graphics Processing Unit. Measures the activity level of the graphics card." },
            ["tooltipRam"] = new() { ["pt"] = "Memória de Acesso Aleatório. Mede a quantidade de memória volátil em uso.", ["en"] = "Random Access Memory. Measures the amount of volatile memory in use." },
            ["tooltipDisk"] = new() { ["pt"] = "Mede a taxa de leitura e escrita do armazenamento (SSD/HDD).", ["en"] = "Measures the read and write rate of the system storage (SSD/HDD)." },
            ["tooltipEnergy"] = new() { ["pt"] = "Mede o consumo elétrico estimado do sistema em tempo real (Watts).", ["en"] = "Measures the estimated electrical power consumption of the system in real-time (Watts)." },

            // General UI
            ["coreEngine"] = new() { ["pt"] = "Motor Central", ["en"] = "Core Engine" },
            ["theme"] = new() { ["pt"] = "Tema", ["en"] = "Theme" },
            ["dark"] = new() { ["pt"] = "Escuro", ["en"] = "Dark" },
            ["light"] = new() { ["pt"] = "Claro", ["en"] = "Light" },
            ["language"] = new() { ["pt"] = "Idioma", ["en"] = "Language" },
            ["energy"] = new() { ["pt"] = "Energia", ["en"] = "Power" },
            ["waitingData"] = new() { ["pt"] = "Aguardando...", ["en"] = "Waiting..." },
            ["cancel"] = new() { ["pt"] = "Cancelar", ["en"] = "Cancel" },
            ["apply"] = new() { ["pt"] = "Aplicar", ["en"] = "Apply" },
            ["applying"] = new() { ["pt"] = "Aplicando...", ["en"] = "Applying..." },

            // System Specs
            ["systemSpecs"] = new() { ["pt"] = "Especificações do PC", ["en"] = "PC Specifications" },
            ["systemSpecsDesc"] = new() { ["pt"] = "Visualização detalhada do hardware conectado.", ["en"] = "Detailed view of connected hardware." },
            ["processor"] = new() { ["pt"] = "Processador (CPU)", ["en"] = "Processor (CPU)" },
            ["graphics"] = new() { ["pt"] = "Placa de Vídeo (GPU)", ["en"] = "Graphics (GPU)" },
            ["motherboard"] = new() { ["pt"] = "Placa-mãe", ["en"] = "Motherboard" },
            ["memory"] = new() { ["pt"] = "Memória (RAM)", ["en"] = "Memory (RAM)" },
            ["storage"] = new() { ["pt"] = "Armazenamento", ["en"] = "Storage" },
            ["os"] = new() { ["pt"] = "Sistema Operacional", ["en"] = "Operating System" },
            ["lblModel"] = new() { ["pt"] = "Modelo", ["en"] = "Model" },
            ["lblCores"] = new() { ["pt"] = "Núcleos/Threads", ["en"] = "Cores/Threads" },
            ["lblBaseClock"] = new() { ["pt"] = "Clock Base", ["en"] = "Base Clock" },
            ["lblBoostClock"] = new() { ["pt"] = "Clock Boost", ["en"] = "Boost Clock" },
            ["lblCacheL3"] = new() { ["pt"] = "Cache L3", ["en"] = "L3 Cache" },
            ["lblVram"] = new() { ["pt"] = "VRAM", ["en"] = "VRAM" },
            ["lblDriver"] = new() { ["pt"] = "Driver", ["en"] = "Driver" },
            ["lblDirectx"] = new() { ["pt"] = "DirectX", ["en"] = "DirectX" },
            ["lblManufacturer"] = new() { ["pt"] = "Fabricante", ["en"] = "Manufacturer" },
            ["lblChipset"] = new() { ["pt"] = "Chipset", ["en"] = "Chipset" },
            ["lblBiosVersion"] = new() { ["pt"] = "Versão da BIOS", ["en"] = "BIOS Version" },
            ["lblTotalCapacity"] = new() { ["pt"] = "Capacidade Total", ["en"] = "Total Capacity" },
            ["lblSpeed"] = new() { ["pt"] = "Velocidade", ["en"] = "Speed" },
            ["lblType"] = new() { ["pt"] = "Tipo", ["en"] = "Type" },
            ["lblSlotsUsed"] = new() { ["pt"] = "Slots Usados", ["en"] = "Used Slots" },
            ["lblDisk1"] = new() { ["pt"] = "Disco 1 (C:)", ["en"] = "Disk 1 (C:)" },
            ["lblDisk2"] = new() { ["pt"] = "Disco 2 (D:)", ["en"] = "Disk 2 (D:)" },
            ["lblHealth"] = new() { ["pt"] = "Saúde", ["en"] = "Health" },
            ["lblPartitionType"] = new() { ["pt"] = "Tipo de Partição", ["en"] = "Partition Type" },
            ["lblEdition"] = new() { ["pt"] = "Edição", ["en"] = "Edition" },
            ["lblVersion"] = new() { ["pt"] = "Versão", ["en"] = "Version" },
            ["lblArchitecture"] = new() { ["pt"] = "Arquitetura", ["en"] = "Architecture" },
            ["lblSecureBoot"] = new() { ["pt"] = "Boot Seguro", ["en"] = "Secure Boot" },

            // Dashboard
            ["dashboard"] = new() { ["pt"] = "Dashboard", ["en"] = "Dashboard" },
            ["dashDesc"] = new() { ["pt"] = "Visão geral do sistema e limpeza rápida.", ["en"] = "System overview and quick cleaning." },
            ["sysUse"] = new() { ["pt"] = "Uso do Sistema ao Vivo", ["en"] = "Live System Usage" },
            ["latAnalysis"] = new() { ["pt"] = "Análise de Latência do Sistema", ["en"] = "System Latency Analysis" },
            ["reliability"] = new() { ["pt"] = "Confiabilidade para Real-Time", ["en"] = "Real-Time Reliability" },
            ["excellent"] = new() { ["pt"] = "Excelente", ["en"] = "Excellent" },
            ["ramClean"] = new() { ["pt"] = "Limpeza Rápida de RAM", ["en"] = "Quick RAM Clean" },
            ["optimizing"] = new() { ["pt"] = "Otimizando cache...", ["en"] = "Optimizing cache..." },
            ["freed"] = new() { ["pt"] = "Memória Liberada!", ["en"] = "Memory Freed!" },
            ["optComplete"] = new() { ["pt"] = "Otimização Concluída", ["en"] = "Optimization Complete" },
            ["ramFreedMsg"] = new() { ["pt"] = "de RAM liberados com sucesso.", ["en"] = "of RAM successfully freed." },

            // RAM & Disk cleanup
            ["ramTitle"] = new() { ["pt"] = "Otimização de RAM", ["en"] = "RAM Optimization" },
            ["ramDesc"] = new() { ["pt"] = "Libera memória em cache e processos suspensos", ["en"] = "Frees cached memory and suspended processes" },
            ["ramUsed"] = new() { ["pt"] = "Em Uso", ["en"] = "In Use" },
            ["ramAvailable"] = new() { ["pt"] = "Disponível", ["en"] = "Available" },
            ["optimize"] = new() { ["pt"] = "Otimizar", ["en"] = "Optimize" },
            ["optimizingRam"] = new() { ["pt"] = "Otimizando...", ["en"] = "Optimizing..." },
            ["optimized"] = new() { ["pt"] = "Otimizado!", ["en"] = "Optimized!" },
            ["junkFilesTitle"] = new() { ["pt"] = "Limpeza de Disco", ["en"] = "Disk Cleanup" },
            ["junkFilesDesc"] = new() { ["pt"] = "Arquivos temporários e cache do sistema", ["en"] = "Temporary files and system cache" },
            ["tempFiles"] = new() { ["pt"] = "Arquivos Temporários", ["en"] = "Temp Files" },
            ["freeSpace"] = new() { ["pt"] = "Espaço Livre", ["en"] = "Free Space" },
            ["cleanJunk"] = new() { ["pt"] = "Limpar Lixo", ["en"] = "Clean Junk" },
            ["cleaningJunk"] = new() { ["pt"] = "Limpando...", ["en"] = "Cleaning..." },
            ["junkCleaned"] = new() { ["pt"] = "Limpo!", ["en"] = "Cleaned!" },

            // Profiles
            ["profiles"] = new() { ["pt"] = "Perfis de Uso", ["en"] = "Usage Profiles" },
            ["profTitle"] = new() { ["pt"] = "Perfis de Otimização", ["en"] = "Optimization Profiles" },
            ["profDesc"] = new() { ["pt"] = "Aplique configurações pré-definidas com um único clique.", ["en"] = "Apply pre-defined configurations with a single click." },
            ["profGamer"] = new() { ["pt"] = "Modo Gamer", ["en"] = "Gamer Mode" },
            ["profGamerDesc"] = new() { ["pt"] = "Suspende automaticamente serviços de segundo plano não essenciais e ativa o plano de energia de alto desempenho.", ["en"] = "Automatically suspends non-essential background services and activates the high-performance power plan." },
            ["profCheck"] = new() { ["pt"] = "Check-up & Limpeza", ["en"] = "Check-up & Clean" },
            ["profCheckDesc"] = new() { ["pt"] = "Reseta configurações para padrão de estabilidade, limpa caches profundos e remove telemetria.", ["en"] = "Resets settings to stability defaults, deeply cleans caches and removes telemetry." },
            ["applyProf"] = new() { ["pt"] = "Aplicar Perfil", ["en"] = "Apply Profile" },
            ["activeProf"] = new() { ["pt"] = "Perfil Ativo", ["en"] = "Active Profile" },
            ["confirmApply"] = new() { ["pt"] = "Aplicar Otimizações", ["en"] = "Apply Optimizations" },
            ["viewCustomize"] = new() { ["pt"] = "Visualizar & Customizar", ["en"] = "View & Customize" },

            // Custom Tweaks
            ["custom"] = new() { ["pt"] = "Customização", ["en"] = "Customization" },
            ["customTitle"] = new() { ["pt"] = "Scripts Customizados", ["en"] = "Custom Scripts" },
            ["customDesc"] = new() { ["pt"] = "Selecione opções individuais para montar sua otimização ideal.", ["en"] = "Select individual options to build your ideal optimization." },
            ["applySelected"] = new() { ["pt"] = "Aplicar Selecionados", ["en"] = "Apply Selected" },
            ["executing"] = new() { ["pt"] = "Executando...", ["en"] = "Executing..." },
            ["applied"] = new() { ["pt"] = "Aplicado!", ["en"] = "Applied!" },
            ["catPerf"] = new() { ["pt"] = "Desempenho", ["en"] = "Performance" },
            ["catPriv"] = new() { ["pt"] = "Privacidade", ["en"] = "Privacy" },
            ["catNet"] = new() { ["pt"] = "Rede", ["en"] = "Network" },
            ["catSys"] = new() { ["pt"] = "Sistema", ["en"] = "System" },
            ["catUi"] = new() { ["pt"] = "Interface", ["en"] = "Interface" },
            ["toggleAll"] = new() { ["pt"] = "Alternar Todos", ["en"] = "Toggle All" },
            ["confirmExec"] = new() { ["pt"] = "Confirmar Execução", ["en"] = "Confirm Execution" },

            // Apps
            ["appsTab"] = new() { ["pt"] = "Aplicativos", ["en"] = "Apps" },
            ["appsDesc"] = new() { ["pt"] = "Instale pacotes essenciais via PowerShell silenciosamente.", ["en"] = "Install essential packages silently via PowerShell." },
            ["catBrowsers"] = new() { ["pt"] = "Navegadores", ["en"] = "Browsers" },
            ["catGaming"] = new() { ["pt"] = "Jogos", ["en"] = "Gaming" },
            ["catComms"] = new() { ["pt"] = "Comunicação", ["en"] = "Communication" },
            ["catUtilities"] = new() { ["pt"] = "Utilitários", ["en"] = "Utilities" },
            ["install"] = new() { ["pt"] = "Instalar", ["en"] = "Install" },
            ["requested"] = new() { ["pt"] = "Solicitado", ["en"] = "Requested" },

            // Network
            ["networkTab"] = new() { ["pt"] = "Rede & DNS", ["en"] = "Network & DNS" },
            ["networkDesc"] = new() { ["pt"] = "Analise sua latência, altere o DNS e redefina adaptadores.", ["en"] = "Analyze your latency, change DNS and reset adapters." },
            ["pingLabel"] = new() { ["pt"] = "Latência Atual (8.8.8.8)", ["en"] = "Current Latency (8.8.8.8)" },
            ["statusOnline"] = new() { ["pt"] = "Conexão Estável", ["en"] = "Stable Connection" },
            ["dnsLabel"] = new() { ["pt"] = "Servidor DNS Preferencial", ["en"] = "Preferred DNS Server" },
            ["dnsAuto"] = new() { ["pt"] = "Automático (Padrão)", ["en"] = "Automatic (Default)" },
            ["netResetTitle"] = new() { ["pt"] = "Redefinição Profunda", ["en"] = "Deep Reset" },
            ["netResetDesc"] = new() { ["pt"] = "Limpa o cache DNS, renova o IP e reseta os catálogos Winsock.", ["en"] = "Clears DNS cache, renews IP, and resets Winsock catalogs." },
            ["runReset"] = new() { ["pt"] = "Resetar Rede", ["en"] = "Reset Network" },

            // Support
            ["support"] = new() { ["pt"] = "Suporte", ["en"] = "Support" },
            ["fbTitle"] = new() { ["pt"] = "Sobre & Suporte", ["en"] = "About & Support" },
            ["fbDesc"] = new() { ["pt"] = "Contribua com o projeto, relate bugs ou faça uma doação.", ["en"] = "Contribute to the project, report bugs, or make a donation." },

            // History
            ["historyTitle"] = new() { ["pt"] = "Histórico de Execução", ["en"] = "Execution History" },
            ["historyDesc"] = new() { ["pt"] = "Últimas otimizações aplicadas e seus status.", ["en"] = "Latest applied optimizations and their status." },
            ["statusSuccess"] = new() { ["pt"] = "Sucesso", ["en"] = "Success" },
            ["statusError"] = new() { ["pt"] = "Erro", ["en"] = "Error" },
            ["emptyHistory"] = new() { ["pt"] = "Nenhum histórico disponível.", ["en"] = "No history available yet." },
        };

        public string Get(string key)
        {
            if (_translations.TryGetValue(key, out var entry) &&
                entry.TryGetValue(_currentLanguage, out var value))
            {
                return value;
            }
            return key; // fallback: return the key itself
        }

        public void SetLanguage(string language)
        {
            if (language != "pt" && language != "en") return;
            _currentLanguage = language;
            LanguageChanged?.Invoke();
        }
    }
}
