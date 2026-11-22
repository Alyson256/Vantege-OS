using System.Collections.Generic;

namespace Bitshift.Services.Localization
{
    internal static class SpecsTranslations
    {
        public static void Register(IDictionary<string, Dictionary<string, string>> target)
        {
            target["systemSpecs"] = new() { ["pt"] = "Especificações do PC", ["en"] = "PC Specifications" };
            target["systemSpecsDesc"] = new() { ["pt"] = "Visualização detalhada do hardware conectado.", ["en"] = "Detailed view of connected hardware." };
            target["processor"] = new() { ["pt"] = "Processador (CPU)", ["en"] = "Processor (CPU)" };
            target["graphics"] = new() { ["pt"] = "Placa de Vídeo (GPU)", ["en"] = "Graphics (GPU)" };
            target["motherboard"] = new() { ["pt"] = "Placa-mãe", ["en"] = "Motherboard" };
            target["memory"] = new() { ["pt"] = "Memória (RAM)", ["en"] = "Memory (RAM)" };
            target["storage"] = new() { ["pt"] = "Armazenamento", ["en"] = "Storage" };
            target["os"] = new() { ["pt"] = "Sistema Operacional", ["en"] = "Operating System" };
            target["technologies"] = new() { ["pt"] = "Tecnologias & Segurança", ["en"] = "Technologies & Security" };
            target["displayNetwork"] = new() { ["pt"] = "Exibição & Rede", ["en"] = "Display & Network" };
            target["lblModel"] = new() { ["pt"] = "Modelo", ["en"] = "Model" };
            target["lblCores"] = new() { ["pt"] = "Núcleos/Threads", ["en"] = "Cores/Threads" };
            target["lblBaseClock"] = new() { ["pt"] = "Clock Base", ["en"] = "Base Clock" };
            target["lblBoostClock"] = new() { ["pt"] = "Clock Boost", ["en"] = "Boost Clock" };
            target["lblCacheL3"] = new() { ["pt"] = "Cache L3", ["en"] = "L3 Cache" };
            target["lblVram"] = new() { ["pt"] = "VRAM", ["en"] = "VRAM" };
            target["lblDriver"] = new() { ["pt"] = "Driver", ["en"] = "Driver" };
            target["lblDirectx"] = new() { ["pt"] = "DirectX", ["en"] = "DirectX" };
            target["lblManufacturer"] = new() { ["pt"] = "Fabricante", ["en"] = "Manufacturer" };
            target["lblChipset"] = new() { ["pt"] = "Chipset", ["en"] = "Chipset" };
            target["lblBiosVersion"] = new() { ["pt"] = "Versão da BIOS", ["en"] = "BIOS Version" };
            target["lblTotalCapacity"] = new() { ["pt"] = "Capacidade Total", ["en"] = "Total Capacity" };
            target["lblSpeed"] = new() { ["pt"] = "Velocidade", ["en"] = "Speed" };
            target["lblType"] = new() { ["pt"] = "Tipo", ["en"] = "Type" };
            target["lblSlotsUsed"] = new() { ["pt"] = "Slots Usados", ["en"] = "Used Slots" };
            target["lblDisk1"] = new() { ["pt"] = "Disco 1 (C:)", ["en"] = "Disk 1 (C:)" };
            target["lblDisk2"] = new() { ["pt"] = "Disco 2 (D:)", ["en"] = "Disk 2 (D:)" };
            target["lblHealth"] = new() { ["pt"] = "Saúde", ["en"] = "Health" };
            target["lblPartitionType"] = new() { ["pt"] = "Tipo de Partição", ["en"] = "Partition Type" };
            target["lblEdition"] = new() { ["pt"] = "Edição", ["en"] = "Edition" };
            target["lblVersion"] = new() { ["pt"] = "Versão", ["en"] = "Version" };
            target["lblArchitecture"] = new() { ["pt"] = "Arquitetura", ["en"] = "Architecture" };
            target["lblSecureBoot"] = new() { ["pt"] = "Boot Seguro", ["en"] = "Secure Boot" };
            target["lblSocket"] = new() { ["pt"] = "Soquete", ["en"] = "Socket" };
            target["lblPartNumber"] = new() { ["pt"] = "Part Number", ["en"] = "Part Number" };
            target["lblResolution"] = new() { ["pt"] = "Resolução & Taxa", ["en"] = "Resolution & Refresh" };
            target["lblNetworkAdapter"] = new() { ["pt"] = "Adaptador Ativo", ["en"] = "Active Adapter" };
            target["lblNetworkLink"] = new() { ["pt"] = "Velocidade de Link", ["en"] = "Link Speed" };
        }
    }
}
