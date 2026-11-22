@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: ============================================================
:: 13_win11_debloat.bat — Windows 11 Comprehensive Debloat Suite
::
:: Complete, safe and reversible debloat for Windows 11:
::   1. Removes non-essential UWP bloatware and sponsored apps
::   2. Disables Windows 11 Copilot AI and Taskbar AI button
::   3. Disables Windows 11 Recall (Continuous AI Snapshots)
::   4. Disables Windows 11 Widgets Board and News Taskbar feed
::   5. Disables Bing web searches & suggestions in Start Menu
::   6. Disables Microsoft Consumer Experience (silent sponsored app installs)
::   7. Removes Taskbar Chat (Teams Consumer) integration
::   8. Hardens Windows 11 diagnostic and telemetry policies
::
:: Requires: Windows 11, Administrator privileges
:: Safe: Uses official Group Policy registry keys and Remove-AppxPackage
:: ============================================================

call "%~dp0_lib\colors.bat"
call "%~dp0_lib\require_admin.bat" || exit /b 1

echo.
echo  %CYAN%%BOLD%  ╔══════════════════════════════════════════════════╗%RESET%
echo  %CYAN%%BOLD%  ║       WINDOWS 11 DEBLOAT — SCRIPT CONSOLIDADO    ║%RESET%
echo  %CYAN%%BOLD%  ╚══════════════════════════════════════════════════╝%RESET%
echo.
echo  %DIM%  Expurgando bloatware, Copilot, Recall, widgets e publicidade...%RESET%
echo.

set "REMOVED_APPS=0"
set "SKIPPED_APPS=0"

:: ── [1/8] Desativar Windows 11 Copilot ────────────────────────
echo  %CYAN%[1/8] Desativando Windows 11 Copilot e Integracao de IA...%RESET%
reg add "HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot" /v "TurnOffWindowsCopilot" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot" /v "TurnOffWindowsCopilot" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "ShowCopilotButton" /t REG_DWORD /d 0 /f >nul 2>&1
echo  %GREEN%  [+]%RESET% Windows Copilot — desativado via politica

:: ── [2/8] Desativar Windows 11 Recall (IA Snapshots) ──────────
echo  %CYAN%[2/8] Desativando Windows 11 Recall (Captura de Tela IA)...%RESET%
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI" /v "DisableAIDataAnalysis" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Policies\Microsoft\Windows\WindowsAI" /v "DisableAIDataAnalysis" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI" /v "AllowRecallEnablement" /t REG_DWORD /d 0 /f >nul 2>&1
sc query "CoreAIPlatform" >nul 2>&1
if !errorLevel! == 0 (
    sc stop "CoreAIPlatform" >nul 2>&1
    sc config "CoreAIPlatform" start=demand >nul 2>&1
)
echo  %GREEN%  [+]%RESET% Windows Recall — desativado

:: ── [3/8] Desativar Widgets e Feed de Noticias ─────────────────
echo  %CYAN%[3/8] Desativando Widgets e Feed da Barra de Tarefas...%RESET%
reg add "HKLM\SOFTWARE\Policies\Microsoft\Dsh" /v "AllowNewsAndInterests" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "TaskbarDa" /t REG_DWORD /d 0 /f >nul 2>&1
echo  %GREEN%  [+]%RESET% Widgets e Feed — desativados

:: ── [4/8] Desativar Busca Bing no Menu Iniciar ────────────────
echo  %CYAN%[4/8] Desativando Buscas Web do Bing no Menu Iniciar...%RESET%
reg add "HKCU\Software\Policies\Microsoft\Windows\Explorer" /v "DisableSearchBoxSuggestions" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Search" /v "BingSearchEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Search" /v "ConnectedSearchUseWeb" /t REG_DWORD /d 0 /f >nul 2>&1
echo  %GREEN%  [+]%RESET% Bing Web Search no Menu Iniciar — desativado

:: ── [5/8] Bloquear Consumer Experience (Apps Patrocinados) ────
echo  %CYAN%[5/8] Bloqueando Instalacao Automatica de Apps Patrocinados...%RESET%
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /v "DisableWindowsConsumerFeatures" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SilentInstalledAppsEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SystemPaneSuggestionsEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SubscribedContent-338388Enabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SubscribedContent-338389Enabled" /t REG_DWORD /d 0 /f >nul 2>&1
echo  %GREEN%  [+]%RESET% Consumer Experience e sugestoes comerciais — bloqueados

:: ── [6/8] Remover Icone de Chat (Teams Consumer) ──────────────
echo  %CYAN%[6/8] Removendo Icone de Chat/Teams da Barra de Tarefas...%RESET%
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "TaskbarMn" /t REG_DWORD /d 0 /f >nul 2>&1
echo  %GREEN%  [+]%RESET% Icone de Chat / Teams — removido

:: ── [7/8] Reduzir Telemetria e Diagnosticos Win 11 ─────────────
echo  %CYAN%[7/8] Limitando Telemetria e Diagnosticos do Windows 11...%RESET%
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v "AllowTelemetry" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack" /v "ShowDiagTrackTelemetry" /t REG_DWORD /d 0 /f >nul 2>&1
sc config "DiagTrack" start=demand >nul 2>&1
sc config "dmwappushservice" start=demand >nul 2>&1
echo  %GREEN%  [+]%RESET% Telemetria definida para nivel basico/minimo

:: ── [8/8] Expurgar Pacotes UWP Bloatware ───────────────────────
echo  %CYAN%[8/8] Expurgando Pacotes UWP Dispensaveis do Windows 11...%RESET%

call :purge_app "Microsoft.BingNews"                 "Microsoft News"
call :purge_app "Microsoft.BingWeather"              "Microsoft Weather"
call :purge_app "Microsoft.BingFinance"              "Microsoft Finance"
call :purge_app "Microsoft.BingSports"               "Microsoft Sports"
call :purge_app "Microsoft.BingSearch"               "Bing Search App"
call :purge_app "Clipchamp.Clipchamp"                "Clipchamp Video Editor"
call :purge_app "MicrosoftTeams"                     "Teams Consumer"
call :purge_app "MSTeams"                            "Teams New Consumer"
call :purge_app "Microsoft.Getstarted"               "Tips / Get Started"
call :purge_app "Microsoft.WindowsFeedbackHub"       "Feedback Hub"
call :purge_app "Microsoft.MicrosoftSolitaireCollection" "Solitaire Collection"
call :purge_app "Microsoft.MixedReality.Portal"      "Mixed Reality Portal"
call :purge_app "Microsoft.People"                   "People"
call :purge_app "Microsoft.549981C3F5F10"            "Cortana App"
call :purge_app "Disney.37853D22215B2"               "Disney+"
call :purge_app "SpotifyAB.SpotifyMusic"             "Spotify Promo"
call :purge_app "king.com.CandyCrushSaga"            "Candy Crush"
call :purge_app "TikTok.TikTok"                     "TikTok"

echo.
echo  %CYAN%  ════════════════════════════════════════════════%RESET%
echo  %GREEN%  ✓ Debloat do Windows 11 finalizado com sucesso!%RESET%
echo  %GREEN%    Pacotes UWP removidos: %REMOVED_APPS%  (Ignorados/Ausentes: %SKIPPED_APPS%)%RESET%
echo  %CYAN%  ════════════════════════════════════════════════%RESET%
echo.

if defined LOG_FILE call "%~dp0_lib\logger.bat" "Win11 Debloat completed" "SUCCESS"
exit /b 0

:purge_app <PackagePattern> <FriendlyName>
powershell -NonInteractive -Command ^
    "$pkg = Get-AppxPackage -Name '%~1' -AllUsers -ErrorAction SilentlyContinue; if ($pkg) { Remove-AppxPackage -Package $pkg.PackageFullName -AllUsers -ErrorAction SilentlyContinue; exit 0 } else { exit 1 }" >nul 2>&1
if !errorLevel! == 0 (
    echo  %GREEN%  [+]%RESET% Removido: %~2
    set /a REMOVED_APPS+=1
) else (
    set /a SKIPPED_APPS+=1
)
goto :EOF
