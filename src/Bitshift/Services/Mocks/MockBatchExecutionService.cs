using System;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockBatchExecutionService : IBatchExecutionService
    {
        private readonly MockServiceOptions _options;

        public event Action<string>? OutputReceived;

        public MockBatchExecutionService(MockServiceOptions options)
        {
            _options = options;
        }

        public bool IsAdministrator() => false;

        public string GetScriptsDirectory() => "<mock-scripts>";

        public Task<ScriptExecutionResult> ExecuteScriptAsync(
            string scriptRelativeName,
            string? arguments = null,
            CancellationToken cancellationToken = default)
        {
            return SimulateAsync($"script {scriptRelativeName}", runAsAdmin: false, cancellationToken);
        }

        public Task<ScriptExecutionResult> ExecuteBatchContentAsync(
            string batchContent,
            bool runAsAdmin = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(batchContent))
            {
                return Task.FromResult(new ScriptExecutionResult(
                    false,
                    -1,
                    string.Empty,
                    "Conteúdo do script .BAT está vazio."));
            }

            return SimulateAsync("conteúdo .BAT", runAsAdmin, cancellationToken);
        }

        public Task<ScriptExecutionResult> ExecuteCommandAsync(
            string command,
            string? arguments = null,
            bool runAsAdmin = false,
            CancellationToken cancellationToken = default)
        {
            return SimulateAsync($"comando {command} {arguments}".Trim(), runAsAdmin, cancellationToken);
        }

        public Task<ScriptExecutionResult> ExecutePowerShellAsync(
            string powerShellScript,
            bool runAsAdmin = false,
            CancellationToken cancellationToken = default)
        {
            return SimulateAsync("script PowerShell", runAsAdmin, cancellationToken);
        }

        private async Task<ScriptExecutionResult> SimulateAsync(
            string operation,
            bool runAsAdmin,
            CancellationToken cancellationToken)
        {
            if (!_options.IsEmpty)
            {
                OutputReceived?.Invoke($"[MOCK] Simulando {operation}.");
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _options.SimulateDelayAsync(450, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                const string cancelled = "[AVISO] Operação mock cancelada.";
                OutputReceived?.Invoke(cancelled);
                return new ScriptExecutionResult(false, -1, cancelled, "Operação cancelada.");
            }

            if (_options.IsUacDenied)
            {
                const string denied = "[ERRO] UAC negado (simulado); nenhum comando foi executado.";
                OutputReceived?.Invoke(denied);
                return new ScriptExecutionResult(false, 1223, denied, "UAC negado (simulado).");
            }

            if (_options.IsError)
            {
                string error = $"[ERRO] Falha simulada ao executar {operation}.";
                OutputReceived?.Invoke(error);
                return new ScriptExecutionResult(false, 1, error, error);
            }

            if (_options.IsEmpty)
            {
                return new ScriptExecutionResult(true, 0, string.Empty, string.Empty);
            }

            string output = $"[MOCK] {operation} concluído sem executar comandos reais.";
            OutputReceived?.Invoke(output);
            OutputReceived?.Invoke("[OK] Simulação concluída.");
            return new ScriptExecutionResult(true, 0, output, string.Empty);
        }
    }
}
