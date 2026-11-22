# BITSHIFT

> Aplicativo desktop nativo para Windows, desenvolvido em C# e WPF, com telemetria, ferramentas de otimização e interface em português brasileiro e inglês.

[🇧🇷 Português (Brasil)](./README.md) · [🇺🇸 English](./src/Bitshift/Services/documents/readmeENG.md)

**Desenvolvimento:** [Alyson Araújo](https://github.com/Alyson256) · [Repositório](https://github.com/Alyson256/Vantege-OS)

## Sobre o projeto

O BITSHIFT é um aplicativo WPF organizado com MVVM e injeção de dependências. A interface conversa com contratos de serviço; no modo de desenvolvimento, implementações mock permitem experimentar a UI sem executar scripts ou coletar dados reais do hardware.

O projeto está em desenvolvimento ativo. O código da aplicação está em um único projeto .NET; as abstrações de serviço não são, por enquanto, um backend ou uma biblioteca separados.

## Funcionalidades atuais

- **Visão geral e especificações:** exibe métricas disponíveis de CPU, GPU, memória e disco, além de informações do hardware.
- **PC Boost:** oferece ações de limpeza de memória e arquivos e seleção de perfis de energia.
- **Customização:** reúne ajustes do sistema em categorias, incluindo rede, privacidade e interface.
- **Configurações e Sobre:** incluem idioma, tema, bandeja do sistema e links do projeto.
- **Interface localizada:** português brasileiro e inglês, com troca de idioma nas configurações.

O modelo de telemetria atual não fornece temperatura nem latência DPC/ISR; portanto, o projeto não apresenta essas métricas como recursos disponíveis.

## Tecnologias

- C# e .NET 10 para Windows
- WPF, XAML e WPF-UI
- MVVM com CommunityToolkit.Mvvm
- Injeção de dependências com Microsoft.Extensions.DependencyInjection
- WMI e APIs do Windows nas implementações reais dos serviços

## Requisitos

- Windows 10 (build 19041 ou superior) ou Windows 11
- .NET 10 SDK
- Visual Studio 2022 com a carga de trabalho de desenvolvimento para desktop .NET, ou VS Code com o SDK instalado

## Compilar e executar

Execute os comandos na raiz do repositório, usando PowerShell:

```powershell
dotnet restore .\Bitshift.slnx
dotnet build .\Bitshift.slnx --configuration Debug
dotnet run --project .\src\Bitshift\Bitshift.csproj --configuration Debug
```

Uma compilação Debug usa serviços mock por padrão. A configuração Release usa serviços reais por padrão; operações que aplicam ajustes podem alterar o Windows.

## Download e verificação

O workflow de release gera `Bitshift-win-x64-portable.zip` e `SHA256SUMS.txt`. Baixe os dois arquivos da página de Releases ou do artefato `Bitshift-win-x64` de uma execução manual do workflow e mantenha-os na mesma pasta.

Para conferir o SHA-256 do arquivo ZIP no PowerShell:

```powershell
$checksum = Select-String -Path .\SHA256SUMS.txt -Pattern '^\s*([0-9a-fA-F]{64})\s+Bitshift-win-x64-portable\.zip$' | Select-Object -First 1
if (-not $checksum) { throw "Hash do ZIP não encontrado em SHA256SUMS.txt." }
$expected = $checksum.Matches[0].Groups[1].Value
$actual = (Get-FileHash .\Bitshift-win-x64-portable.zip -Algorithm SHA256).Hash
if ($actual -ne $expected) { throw "O hash não confere; não execute o arquivo." }
"SHA-256 verificado."
```

O executável ainda não é assinado digitalmente. O SmartScreen ou o antivírus pode exibir um alerta; confirme a origem do download e o hash antes de executar.

### Testar o workflow sem publicar um Release

No GitHub, abra **Actions → Build and Release Windows Desktop App → Run workflow**, escolha a branch `ui` e mantenha a versão de teste `0.0.0-dev` (ou informe outra versão válida). Essa execução compila e disponibiliza o artefato `Bitshift-win-x64`, mas não cria nem publica um GitHub Release.

## Modo mock

Os mocks fornecem dados de exemplo e simulam telemetria, especificações, otimizações, ajustes, atualizações, saída de scripts e resultados de UAC. O mock de execução de scripts nunca executa comandos. Configurações, localização, tema do Windows, bandeja e diálogos WPF continuam reais.

Defina as variáveis no PowerShell antes de iniciar o aplicativo:

```powershell
$env:BITSHIFT_MOCK_SCENARIO = "slow"
$env:BITSHIFT_MOCK_DELAY_MS = "1000"
$env:BITSHIFT_MOCK_UPDATE_AVAILABLE = "true"
dotnet run --project .\src\Bitshift\Bitshift.csproj --configuration Debug
```

| Variável | Valores | Uso |
|---|---|---|
| `BITSHIFT_USE_MOCK_SERVICES` | `true` ou `false` | Substitui o padrão (mock em Debug, real em Release). |
| `BITSHIFT_MOCK_SCENARIO` | `normal`, `slow`, `empty`, `error`, `uac-denied` | Simula estados comuns da interface. |
| `BITSHIFT_MOCK_DELAY_MS` | `0` a `30000` | Define o atraso simulado em milissegundos. |
| `BITSHIFT_MOCK_UPDATE_AVAILABLE` | `true` ou `false` | Simula uma atualização disponível. |

O cenário `slow` impõe pelo menos 2,5 segundos de atraso. Para evitar operações reais, mantenha `BITSHIFT_USE_MOCK_SERVICES=true` e execute em Debug.

## Estrutura do projeto

```text
src/Bitshift/
├── Models/                 # Dados e resultados usados pelos serviços e pela UI
├── Services/
│   ├── Abstractions/       # Contratos consumidos pelos ViewModels
│   ├── Localization/       # Textos em português e inglês
│   └── Mocks/              # Implementações simuladas para desenvolvimento
├── Scripts/                # Scripts .BAT usados pelos serviços reais
├── Themes/                 # Recursos e estilos XAML
├── ViewModels/             # Estado e comandos da interface
└── Views/                  # Telas WPF
```

O registro de dependências e a seleção entre mocks e serviços reais ficam em `src\Bitshift\App.xaml.cs`.

## Idiomas da interface

O código de idioma `pt` representa português brasileiro; `en` representa inglês. As traduções ficam em `src\Bitshift\Services\Localization\`. Ao adicionar um texto, inclua as duas traduções e registre as novas chaves na classe de traduções apropriada.

## Uso responsável

Os serviços reais e scripts podem consultar ou alterar configurações do Windows. Revise as ações antes de aplicá-las, mantenha cópias de segurança e conceda elevação pelo UAC somente quando necessário. Os mocks de Debug não executam os scripts.
