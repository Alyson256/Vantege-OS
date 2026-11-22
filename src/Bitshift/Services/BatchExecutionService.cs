using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public class BatchExecutionService : IBatchExecutionService
    {
        public event Action<string>? OutputReceived;

        public bool IsAdministrator()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public string GetScriptsDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string scriptsDir = Path.Combine(baseDir, "Scripts");
            if (Directory.Exists(scriptsDir)) return scriptsDir;

            string currentDir = Directory.GetCurrentDirectory();
            scriptsDir = Path.Combine(currentDir, "Scripts");
            if (Directory.Exists(scriptsDir)) return scriptsDir;

            var dir = new DirectoryInfo(baseDir);
            for (int i = 0; i < 4 && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "Scripts");
                if (Directory.Exists(candidate)) return candidate;

                string candidateSrc = Path.Combine(dir.FullName, "src", "Bitshift", "Scripts");
                if (Directory.Exists(candidateSrc)) return candidateSrc;

                dir = dir.Parent;
            }

            return Path.Combine(baseDir, "Scripts");
        }

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(string scriptRelativeName, string? arguments = null, CancellationToken cancellationToken = default)
        {
            string scriptsDir = GetScriptsDirectory();
            string scriptPath = Path.Combine(scriptsDir, scriptRelativeName);

            if (!File.Exists(scriptPath))
            {
                string errMsg = $"[ERRO] Script não encontrado: {scriptPath}";
                OutputReceived?.Invoke(errMsg);
                return new ScriptExecutionResult(false, -1, string.Empty, errMsg);
            }

            string scriptDir = Path.GetDirectoryName(scriptPath) ?? scriptsDir;
            string command = "cmd.exe";
            string args = $"/c \"\"{scriptPath}\" {arguments ?? string.Empty}\"";

            OutputReceived?.Invoke($">> Executando: {scriptRelativeName} {arguments ?? ""}");

            return await RunProcessInternalAsync(command, args, scriptDir, cancellationToken);
        }

        public async Task<ScriptExecutionResult> ExecuteBatchContentAsync(
            string batchContent,
            bool runAsAdmin = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(batchContent))
            {
                const string emptyErr = "[!] Conteúdo do script .BAT está vazio.";
                OutputReceived?.Invoke(emptyErr);
                return new ScriptExecutionResult(false, -1, string.Empty, emptyErr);
            }

            string tempFolder = Path.Combine(Path.GetTempPath(), "Bitshift");
            Directory.CreateDirectory(tempFolder);
            string tempScriptPath = Path.Combine(tempFolder, $"bitshift_exec_{Guid.NewGuid():N}.bat");

            try
            {
                string normalized = batchContent.Replace("\r\n", "\n", StringComparison.Ordinal)
                                                .Replace("\n", "\r\n", StringComparison.Ordinal);
                await File.WriteAllTextAsync(tempScriptPath, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);

                OutputReceived?.Invoke($">> Executando script dinâmico ({Path.GetFileName(tempScriptPath)})...");
                if (runAsAdmin && !IsAdministrator())
                {
                    OutputReceived?.Invoke("[INFO] Solicitando elevação de privilégios UAC (Administrador)...");
                }

                string args = $"/c \"\"{tempScriptPath}\"\"";
                return await RunProcessInternalAsync("cmd.exe", args, tempFolder, cancellationToken, runAsAdmin);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempScriptPath))
                    {
                        File.Delete(tempScriptPath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BatchExecutionService] Temp script cleanup skipped: {ex.Message}");
                }
            }
        }

        public async Task<ScriptExecutionResult> ExecuteCommandAsync(string command, string? arguments = null, bool runAsAdmin = false, CancellationToken cancellationToken = default)
        {
            OutputReceived?.Invoke($">> Executando comando: {command} {arguments ?? ""}");
            return await RunProcessInternalAsync(command, arguments ?? string.Empty, Directory.GetCurrentDirectory(), cancellationToken, runAsAdmin);
        }

        public async Task<ScriptExecutionResult> ExecutePowerShellAsync(string powerShellScript, bool runAsAdmin = false, CancellationToken cancellationToken = default)
        {
            OutputReceived?.Invoke(">> Executando script PowerShell...");
            string command = "powershell.exe";
            string encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(powerShellScript));
            string args = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedScript}";
            return await RunProcessInternalAsync(command, args, Directory.GetCurrentDirectory(), cancellationToken, runAsAdmin);
        }

        private async Task<ScriptExecutionResult> RunProcessInternalAsync(
            string fileName,
            string arguments,
            string workingDirectory,
            CancellationToken cancellationToken,
            bool runAsAdmin = false)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            bool shouldElevate = runAsAdmin && !IsAdministrator();

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
            };

            if (shouldElevate)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.EnvironmentVariables["AUTO_CONFIRM"] = "1";
                startInfo.EnvironmentVariables["NON_INTERACTIVE"] = "1";
                startInfo.EnvironmentVariables["CI"] = "1";
            }

            try
            {
                using var process = new Process { StartInfo = startInfo };

                if (!startInfo.UseShellExecute)
                {
                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            outputBuilder.AppendLine(e.Data);
                            OutputReceived?.Invoke(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            errorBuilder.AppendLine(e.Data);
                            OutputReceived?.Invoke($"[ERR] {e.Data}");
                        }
                    };
                }

                if (!process.Start())
                {
                    return new ScriptExecutionResult(false, -1, string.Empty, "Falha ao iniciar o processo.");
                }

                if (!startInfo.UseShellExecute)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                await process.WaitForExitAsync(cancellationToken);

                bool success = process.ExitCode == 0;
                string finalOutput = outputBuilder.ToString();
                string finalError = errorBuilder.ToString();

                OutputReceived?.Invoke(success
                    ? $"[OK] Finalizado com sucesso (Código {process.ExitCode})"
                    : $"[AVISO] Finalizado com aviso/código {process.ExitCode}");

                return new ScriptExecutionResult(success, process.ExitCode, finalOutput, finalError);
            }
            catch (OperationCanceledException)
            {
                OutputReceived?.Invoke("[AVISO] Operação cancelada pelo usuário.");
                return new ScriptExecutionResult(false, -1, outputBuilder.ToString(), "Operação cancelada.");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"[ERRO] Exceção na execução: {ex.Message}");
                return new ScriptExecutionResult(false, -1, outputBuilder.ToString(), ex.Message);
            }
        }
    }
}
