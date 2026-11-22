# BITSHIFT

> A native Windows desktop application built with C# and WPF, featuring telemetry, optimization tools, and a Brazilian Portuguese and English interface.

[🇧🇷 Português (Brasil)](../../../../README.md) · [🇺🇸 English](./readmeENG.md)

**Development:** [Alyson Araújo](https://github.com/Alyson256) · [Repository](https://github.com/Alyson256/Vantege-OS)

## About

BITSHIFT is a WPF application organized with MVVM and dependency injection. The UI consumes service contracts; in development mode, mock implementations let you explore the UI without running scripts or collecting real hardware data.

The project is under active development. The application currently lives in a single .NET project; its service abstractions are not a separate backend or library.

## Current features

- **Overview and specifications:** displays available CPU, GPU, memory, and disk metrics, along with hardware details.
- **PC Boost:** provides memory and file cleanup actions and power profile selection.
- **Customization:** groups system tweaks into categories, including network, privacy, and interface.
- **Settings and About:** include language, theme, system tray, and project links.
- **Localized interface:** Brazilian Portuguese and English, switchable in settings.

The current telemetry model does not provide temperature or DPC/ISR latency, so the project does not present those metrics as available features.

## Technology

- C# and .NET 10 for Windows
- WPF, XAML, and WPF-UI
- MVVM with CommunityToolkit.Mvvm
- Dependency injection with Microsoft.Extensions.DependencyInjection
- WMI and Windows APIs in the real service implementations

## Requirements

- Windows 10 (build 19041 or later) or Windows 11
- .NET 10 SDK
- Visual Studio 2022 with the .NET desktop development workload, or VS Code with the SDK installed

## Build and run

Run these commands from the repository root in PowerShell:

```powershell
dotnet restore .\Bitshift.slnx
dotnet build .\Bitshift.slnx --configuration Debug
dotnet run --project .\src\Bitshift\Bitshift.csproj --configuration Debug
```

A Debug build uses mock services by default. Release uses real services by default; operations that apply tweaks can change Windows.

## Download and verification

The release workflow produces `Bitshift-win-x64-portable.zip` and `SHA256SUMS.txt`. Download both files from the Releases page or from the `Bitshift-win-x64` artifact of a manual workflow run, and keep them in the same folder.

To verify the ZIP's SHA-256 hash in PowerShell:

```powershell
$checksum = Select-String -Path .\SHA256SUMS.txt -Pattern '^\s*([0-9a-fA-F]{64})\s+Bitshift-win-x64-portable\.zip$' | Select-Object -First 1
if (-not $checksum) { throw "ZIP hash not found in SHA256SUMS.txt." }
$expected = $checksum.Matches[0].Groups[1].Value
$actual = (Get-FileHash .\Bitshift-win-x64-portable.zip -Algorithm SHA256).Hash
if ($actual -ne $expected) { throw "The hash does not match; do not run the file." }
"SHA-256 verified."
```

The executable is not digitally signed yet. SmartScreen or antivirus software may show a warning; verify the download source and hash before running it.

### Test the workflow without publishing a release

On GitHub, open **Actions → Build and Release Windows Desktop App → Run workflow**, select the `ui` branch, and keep the `0.0.0-dev` test version (or enter another valid version). This run builds and uploads the `Bitshift-win-x64` artifact but does not create or publish a GitHub Release.

## Mock mode

Mocks provide sample data and simulate telemetry, specifications, optimizations, tweaks, updates, script output, and UAC outcomes. The mock script executor never runs commands. Settings, localization, Windows theme, tray, and WPF dialogs remain real.

Set environment variables in PowerShell before starting the application:

```powershell
$env:BITSHIFT_MOCK_SCENARIO = "slow"
$env:BITSHIFT_MOCK_DELAY_MS = "1000"
$env:BITSHIFT_MOCK_UPDATE_AVAILABLE = "true"
dotnet run --project .\src\Bitshift\Bitshift.csproj --configuration Debug
```

| Variable | Values | Purpose |
|---|---|---|
| `BITSHIFT_USE_MOCK_SERVICES` | `true` or `false` | Overrides the default (mock in Debug, real in Release). |
| `BITSHIFT_MOCK_SCENARIO` | `normal`, `slow`, `empty`, `error`, `uac-denied` | Simulates common UI states. |
| `BITSHIFT_MOCK_DELAY_MS` | `0` to `30000` | Sets the simulated delay in milliseconds. |
| `BITSHIFT_MOCK_UPDATE_AVAILABLE` | `true` or `false` | Simulates an available update. |

The `slow` scenario enforces a delay of at least 2.5 seconds. To avoid real system operations, keep `BITSHIFT_USE_MOCK_SERVICES=true` and run a Debug build.

## Project structure

```text
src/Bitshift/
├── Models/                 # Data and result models used by services and UI
├── Services/
│   ├── Abstractions/       # Contracts consumed by ViewModels
│   ├── Localization/       # Portuguese and English strings
│   └── Mocks/              # Simulated implementations for development
├── Scripts/                # .BAT scripts used by real services
├── Themes/                 # XAML resources and styles
├── ViewModels/             # UI state and commands
└── Views/                  # WPF screens
```

Dependency registration and mock/real service selection are in `src\Bitshift\App.xaml.cs`.

## UI languages

The `pt` language code represents Brazilian Portuguese; `en` represents English. Translations are in `src\Bitshift\Services\Localization\`. When adding UI text, provide both translations and register new keys in the appropriate translation class.

## Responsible use

Real services and scripts can inspect or change Windows settings. Review actions before applying them, keep backups, and grant UAC elevation only when needed. Debug mocks do not execute scripts.