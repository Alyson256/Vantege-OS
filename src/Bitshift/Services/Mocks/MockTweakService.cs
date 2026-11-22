using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services.Mocks
{
    public sealed class MockTweakService : ITweakService
    {
        private readonly IBatchExecutionService _batchService;
        private readonly MockServiceOptions _options;
        private readonly List<TweakSummary> _tweaks = TweakCatalog.Create()
            .Select(definition => new TweakSummary(definition, false))
            .ToList();

        public MockTweakService(IBatchExecutionService batchService, MockServiceOptions options)
        {
            _batchService = batchService;
            _options = options;
        }

        public IReadOnlyList<TweakSummary> GetAllTweaks()
        {
            return _options.IsEmpty ? Array.Empty<TweakSummary>() : _tweaks.ToArray();
        }

        public async Task<ScriptExecutionResult> ExecuteTweakAsync(
            string tweakId,
            CancellationToken cancellationToken = default)
        {
            int index = _tweaks.FindIndex(tweak => tweak.Definition.Id == tweakId);
            if (index < 0)
            {
                throw new ArgumentException($"Unknown tweak ID: {tweakId}", nameof(tweakId));
            }

            TweakSummary tweak = _tweaks[index];
            var result = await _batchService.ExecuteScriptAsync(
                tweak.Definition.ScriptFile,
                cancellationToken: cancellationToken);

            if (result.Success)
            {
                _tweaks[index] = tweak with { IsApplied = true };
            }

            return result;
        }

        public async Task<ScriptExecutionResult> UndoAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _batchService.ExecuteScriptAsync(
                "10_undo_all.bat",
                arguments: "/y",
                cancellationToken: cancellationToken);

            if (result.Success)
            {
                for (int i = 0; i < _tweaks.Count; i++)
                {
                    _tweaks[i] = _tweaks[i] with { IsApplied = false };
                }
            }

            return result;
        }
    }
}
