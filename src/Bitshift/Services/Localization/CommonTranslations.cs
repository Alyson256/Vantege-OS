using System.Collections.Generic;

namespace Bitshift.Services.Localization
{
    internal static class CommonTranslations
    {
        public static void Register(IDictionary<string, Dictionary<string, string>> target)
        {
            // Tooltips
            target["tooltipDpc"] = new() { ["pt"] = "Mede o pico de tempo de drivers processando tarefas adiadas. Valores altos causam engasgos (stuttering) no sistema.", ["en"] = "Measures peak driver latency processing deferred tasks. High values cause system stuttering." };
            target["tooltipIsr"] = new() { ["pt"] = "Mede o tempo que o processador gasta lidando com interrupções de hardware (como mouse/teclado). Valores altos causam atrasos (input lag).", ["en"] = "Measures the time the processor spends handling hardware interrupts (like mouse/keyboard). High values cause input lag." };
            target["tooltipPagefaults"] = new() { ["pt"] = "Ocorre quando o sistema precisa buscar no disco dados que deveriam estar na RAM. Quanto menor, mais rápido o sistema responde.", ["en"] = "Occurs when the system has to fetch data from the disk that should be in the RAM. The lower, the faster the system responds." };
            target["tooltipReliability"] = new() { ["pt"] = "Indica a capacidade do sistema de lidar com áudio, vídeo e jogos sem engasgos, quedas de quadros ou desfoque de movimento.", ["en"] = "Indicates the system's ability to handle audio, video, and gaming without stutters, dropped frames, or motion blur." };
            target["tooltipCpu"] = new() { ["pt"] = "Unidade Central de Processamento. Mede o nível de atividade do processador principal.", ["en"] = "Central Processing Unit. Measures the activity level of the main processor." };
            target["tooltipGpu"] = new() { ["pt"] = "Unidade de Processamento Gráfico. Mede o nível de atividade da placa de vídeo.", ["en"] = "Graphics Processing Unit. Measures the activity level of the graphics card." };
            target["tooltipRam"] = new() { ["pt"] = "Memória de Acesso Aleatório. Mede a quantidade de memória volátil em uso.", ["en"] = "Random Access Memory. Measures the amount of volatile memory in use." };
            target["tooltipDisk"] = new() { ["pt"] = "Mede a taxa de leitura e escrita do armazenamento (SSD/HDD).", ["en"] = "Measures the read and write rate of the system storage (SSD/HDD)." };
            target["tooltipEnergy"] = new() { ["pt"] = "Mede o consumo elétrico estimado do sistema em tempo real (Watts).", ["en"] = "Measures the estimated electrical power consumption of the system in real-time (Watts)." };

            // General UI & Sidebar
            target["brandTagline"] = new() { ["pt"] = "seu sistema suas regras", ["en"] = "your system your rules" };
            target["coreEngine"] = new() { ["pt"] = "Motor Central", ["en"] = "Core Engine" };
            target["theme"] = new() { ["pt"] = "Tema", ["en"] = "Theme" };
            target["dark"] = new() { ["pt"] = "Escuro", ["en"] = "Dark" };
            target["light"] = new() { ["pt"] = "Claro", ["en"] = "Light" };
            target["language"] = new() { ["pt"] = "Idioma", ["en"] = "Language" };
            target["energy"] = new() { ["pt"] = "Energia", ["en"] = "Power" };
            target["waitingData"] = new() { ["pt"] = "Aguardando...", ["en"] = "Waiting..." };
            target["cancel"] = new() { ["pt"] = "Cancelar", ["en"] = "Cancel" };
            target["apply"] = new() { ["pt"] = "Aplicar", ["en"] = "Apply" };
            target["applying"] = new() { ["pt"] = "Aplicando...", ["en"] = "Applying..." };

            // Sidebar Footer, Updates & About
            target["about"] = new() { ["pt"] = "Sobre", ["en"] = "About" };
            target["checkUpdates"] = new() { ["pt"] = "Verificar atualizações", ["en"] = "Check for updates" };
            target["checkingUpdates"] = new() { ["pt"] = "Verificando...", ["en"] = "Checking..." };
            target["upToDate"] = new() { ["pt"] = "Atualizado", ["en"] = "Up to date" };
            target["updateAvailable"] = new() { ["pt"] = "Atualize", ["en"] = "Update" };
            target["updateError"] = new() { ["pt"] = "Indisponível", ["en"] = "Unavailable" };
            target["openRelease"] = new() { ["pt"] = "Baixar no GitHub", ["en"] = "Download on GitHub" };
            target["sidebarProfile"] = new() { ["pt"] = "Perfil", ["en"] = "Profile" };
            target["sidebarColor"] = new() { ["pt"] = "Cor", ["en"] = "Color" };
            target["sidebarAdminBtn"] = new() { ["pt"] = "Modo ADM (UAC)", ["en"] = "Admin Mode (UAC)" };
            target["modeAdmin"] = new() { ["pt"] = "Modo Administrador (Elevado)", ["en"] = "Administrator Mode (Elevated)" };
            target["modeStandard"] = new() { ["pt"] = "Modo Seguro (Ring-3)", ["en"] = "Standard Mode (Ring-3)" };
        }
    }
}
