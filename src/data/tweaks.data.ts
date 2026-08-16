import { Tweak } from '../types';

export const TWEAKS_DB: Tweak[] = [
  {
    id: 'sys_restore_point',
    title: { pt: 'Criar Ponto de Restauração', en: 'Create Restore Point' },
    description: { pt: 'Cria um ponto de restauração do sistema Windows antes de aplicar mudanças.', en: 'Creates a Windows system restore point before applying changes.' },
    registryDetails: {
      pt: 'Checkpoint-Computer -Description "Vantage OS Restore Point" -RestorePointType "MODIFY_SETTINGS"',
      en: 'Checkpoint-Computer -Description "Vantage OS Restore Point" -RestorePointType "MODIFY_SETTINGS"'
    },
    category: 'System',
    recommendedFor: ['Checkup', 'Gaming', 'All'] as any,
  },

  {
    id: 't-001',
    title: { pt: 'Desativar Telemetria do Windows', en: 'Disable Windows Telemetry' },
    description: { pt: 'Bloqueia o envio de dados de diagnóstico e uso para a Microsoft.', en: 'Blocks sending diagnostic and usage data to Microsoft.' },
    registryDetails: { 
      pt: 'HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection -> AllowTelemetry = 0\nServiços DiagTrack e dmwappushservice desabilitados.', 
      en: 'HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection -> AllowTelemetry = 0\nServices DiagTrack and dmwappushservice disabled.' 
    },
    category: 'Privacy',
    recommendedFor: ['Gaming', 'Checkup', 'All'],
  },
  {
    id: 't-002',
    title: { pt: 'Otimizar Agendador de Pacotes de Rede', en: 'Optimize Network Packet Scheduler' },
    description: { pt: 'Reduz a latência de rede limitando a largura de banda reservada.', en: 'Reduces network latency by limiting reserved bandwidth.' },
    registryDetails: {
      pt: 'HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Psched -> NonBestEffortLimit = 0',
      en: 'HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Psched -> NonBestEffortLimit = 0'
    },
    category: 'Network',
    recommendedFor: ['Gaming', 'All'],
  },
  {
    id: 't-003',
    title: { pt: 'Desabilitar Serviços Inúteis', en: 'Disable Useless Services' },
    description: { pt: 'Desativa serviços em segundo plano como Fax, Print Spooler (se não usar impressora), etc.', en: 'Disables background services like Fax, Print Spooler (if not using a printer), etc.' },
    registryDetails: {
      pt: 'HKLM\\SYSTEM\\CurrentControlSet\\Services -> Spooler (Start=4), Fax (Start=4)',
      en: 'HKLM\\SYSTEM\\CurrentControlSet\\Services -> Spooler (Start=4), Fax (Start=4)'
    },
    category: 'Performance',
    recommendedFor: ['Checkup', 'All'],
  },
  {
    id: 't-004',
    title: { pt: 'Ativar Plano de Energia de Alto Desempenho', en: 'Enable High Performance Power Plan' },
    description: { pt: 'Força o sistema a priorizar a performance sobre a economia de energia.', en: 'Forces the system to prioritize performance over power savings.' },
    registryDetails: {
      pt: 'powercfg -setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c\nDesativa Suspensão Híbrida e Hibernação via PowerCFG.',
      en: 'powercfg -setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c\nDisables Hybrid Sleep and Hibernation via PowerCFG.'
    },
    category: 'System',
    recommendedFor: ['Gaming'],
  },
  {
    id: 't-005',
    title: { pt: 'Desativar Animações do Windows', en: 'Disable Windows Animations' },
    description: { pt: 'Remove efeitos visuais e animações para tornar a interface mais responsiva.', en: 'Removes visual effects and animations to make the UI more responsive.' },
    registryDetails: {
      pt: 'HKCU\\Control Panel\\Desktop -> UserPreferencesMask = 90,12,03,80\nHKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects -> VisualFXSetting = 2',
      en: 'HKCU\\Control Panel\\Desktop -> UserPreferencesMask = 90,12,03,80\nHKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects -> VisualFXSetting = 2'
    },
    category: 'UI',
    recommendedFor: ['Performance', 'All'] as any,
  },
  {
    id: 't-006',
    title: { pt: 'Limpar Arquivos Temporários', en: 'Clean Temporary Files' },
    description: { pt: 'Remove lixo do sistema, caches de atualização e arquivos da lixeira.', en: 'Removes system junk, update caches, and recycle bin files.' },
    registryDetails: {
      pt: 'Limpa %TEMP%, C:\\Windows\\Temp, e C:\\Windows\\SoftwareDistribution\\Download via PowerShell.',
      en: 'Clears %TEMP%, C:\\Windows\\Temp, and C:\\Windows\\SoftwareDistribution\\Download via PowerShell.'
    },
    category: 'System',
    recommendedFor: ['Checkup', 'All'],
  },
  {
    id: 't-007',
    title: { pt: 'Desativar Cortana e Bing Search', en: 'Disable Cortana and Bing Search' },
    description: { pt: 'Remove a integração web da pesquisa do menu Iniciar e desativa a assistente.', en: 'Removes web integration from Start menu search and disables the assistant.' },
    registryDetails: {
      pt: 'HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search -> BingSearchEnabled = 0\nHKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search -> AllowCortana = 0',
      en: 'HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search -> BingSearchEnabled = 0\nHKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search -> AllowCortana = 0'
    },
    category: 'Privacy',
    recommendedFor: ['Gaming', 'Checkup', 'All'],
  },
  {
    id: 't-008',
    title: { pt: 'Otimização de Tela Cheia', en: 'Fullscreen Optimizations' },
    description: { pt: 'Desativa o FSO (Fullscreen Optimizations) globalmente para melhorar FPS.', en: 'Disables FSO (Fullscreen Optimizations) globally to improve FPS.' },
    registryDetails: {
      pt: 'HKCU\\System\\GameConfigStore -> GameDVR_FSEBehaviorMode = 2\nHKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR -> AppCaptureEnabled = 0',
      en: 'HKCU\\System\\GameConfigStore -> GameDVR_FSEBehaviorMode = 2\nHKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR -> AppCaptureEnabled = 0'
    },
    category: 'Performance',
    recommendedFor: ['Gaming'],
  }
];