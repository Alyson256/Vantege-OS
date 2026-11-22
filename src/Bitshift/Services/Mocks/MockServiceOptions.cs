using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bitshift.Services.Mocks
{
    public enum MockScenario
    {
        Normal,
        Slow,
        Empty,
        Error,
        UacDenied
    }

    public sealed class MockServiceOptions
    {
        public MockScenario Scenario { get; }
        public int? DelayOverrideMilliseconds { get; }
        public bool UpdateAvailable { get; }

        public bool IsEmpty => Scenario == MockScenario.Empty;
        public bool IsError => Scenario == MockScenario.Error;
        public bool IsUacDenied => Scenario == MockScenario.UacDenied;

        private MockServiceOptions(MockScenario scenario, int? delayOverrideMilliseconds, bool updateAvailable)
        {
            Scenario = scenario;
            DelayOverrideMilliseconds = delayOverrideMilliseconds;
            UpdateAvailable = updateAvailable;
        }

        public static MockServiceOptions FromEnvironment()
        {
            string? scenarioValue = Environment.GetEnvironmentVariable("BITSHIFT_MOCK_SCENARIO");
            MockScenario scenario = MockScenario.Normal;
            string normalizedScenario = scenarioValue?.Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(scenarioValue) &&
                (!Enum.TryParse(normalizedScenario, true, out scenario) || !Enum.IsDefined(scenario)))
            {
                throw new InvalidOperationException(
                    $"Invalid BITSHIFT_MOCK_SCENARIO value: '{scenarioValue}'.");
            }

            int? delayOverrideMilliseconds = null;
            string? delayValue = Environment.GetEnvironmentVariable("BITSHIFT_MOCK_DELAY_MS");
            if (!string.IsNullOrWhiteSpace(delayValue))
            {
                if (!int.TryParse(delayValue, out int parsedDelay) || parsedDelay < 0 || parsedDelay > 30000)
                {
                    throw new InvalidOperationException(
                        "BITSHIFT_MOCK_DELAY_MS must be between 0 and 30000.");
                }

                delayOverrideMilliseconds = parsedDelay;
            }

            bool updateAvailable = false;
            string? updateValue = Environment.GetEnvironmentVariable("BITSHIFT_MOCK_UPDATE_AVAILABLE");
            if (!string.IsNullOrWhiteSpace(updateValue) && !bool.TryParse(updateValue, out updateAvailable))
            {
                throw new InvalidOperationException(
                    "BITSHIFT_MOCK_UPDATE_AVAILABLE must be true or false.");
            }

            return new MockServiceOptions(scenario, delayOverrideMilliseconds, updateAvailable);
        }

        public Task SimulateDelayAsync(int normalDelayMilliseconds, CancellationToken cancellationToken = default)
        {
            int delay = DelayOverrideMilliseconds ?? normalDelayMilliseconds;
            if (Scenario == MockScenario.Slow)
            {
                delay = Math.Max(2500, delay);
            }

            return delay == 0 ? Task.CompletedTask : Task.Delay(delay, cancellationToken);
        }

        public void ThrowIfError()
        {
            if (IsError)
            {
                throw new InvalidOperationException("Falha simulada pelo modo mock.");
            }
        }
    }
}
