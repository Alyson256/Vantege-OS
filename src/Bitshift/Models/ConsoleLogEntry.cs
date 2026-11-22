using System;
using System.Windows.Media;

namespace Bitshift.Models
{
    public enum LogStatusLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public sealed class ConsoleLogEntry
    {
        private static readonly Brush SuccessAccentBrush = CreateFrozenBrush("#10B981");
        private static readonly Brush SuccessTextBrush = CreateFrozenBrush("#6EE7B7");
        private static readonly Brush WarningAccentBrush = CreateFrozenBrush("#F59E0B");
        private static readonly Brush WarningTextBrush = CreateFrozenBrush("#FCD34D");
        private static readonly Brush ErrorAccentBrush = CreateFrozenBrush("#F43F5E");
        private static readonly Brush ErrorTextBrush = CreateFrozenBrush("#FDA4AF");
        private static readonly Brush InfoAccentBrush = CreateFrozenBrush("#71717A");
        private static readonly Brush InfoTextBrush = CreateFrozenBrush("#D4D4D8");

        public string Timestamp { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public LogStatusLevel Level { get; init; } = LogStatusLevel.Info;
        public string IconGlyph { get; init; } = "\uE76C";
        public Brush AccentBrush { get; init; } = InfoAccentBrush;
        public Brush TextBrush { get; init; } = InfoTextBrush;

        private static Brush CreateFrozenBrush(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
            brush.Freeze();
            return brush;
        }

        public static ConsoleLogEntry FromRawLine(string rawLine)
        {
            string message = (rawLine ?? string.Empty).Trim();
            LogStatusLevel level = Classify(message);

            return level switch
            {
                LogStatusLevel.Error => new ConsoleLogEntry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Message = message,
                    Level = level,
                    IconGlyph = "\uEA39",
                    AccentBrush = ErrorAccentBrush,
                    TextBrush = ErrorTextBrush
                },
                LogStatusLevel.Warning => new ConsoleLogEntry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Message = message,
                    Level = level,
                    IconGlyph = "\uE7BA",
                    AccentBrush = WarningAccentBrush,
                    TextBrush = WarningTextBrush
                },
                LogStatusLevel.Success => new ConsoleLogEntry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Message = message,
                    Level = level,
                    IconGlyph = "\uE73E",
                    AccentBrush = SuccessAccentBrush,
                    TextBrush = SuccessTextBrush
                },
                _ => new ConsoleLogEntry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Message = message,
                    Level = LogStatusLevel.Info,
                    IconGlyph = "\uE76C",
                    AccentBrush = InfoAccentBrush,
                    TextBrush = InfoTextBrush
                }
            };
        }

        public static LogStatusLevel Classify(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return LogStatusLevel.Info;

            string upper = line.ToUpperInvariant();

            if (upper.Contains("[ERRO]")
                || upper.Contains("[ERR]")
                || upper.Contains("[ERROR]")
                || upper.Contains("[FAIL]")
                || upper.Contains("— FAILED")
                || upper.Contains("FAILED")
                || upper.Contains("FALHA")
                || upper.Contains("EXCEÇÃO")
                || upper.Contains("EXCEPTION")
                || upper.Contains("ACCESS DENIED")
                || upper.Contains("ACESSO NEGADO"))
            {
                return LogStatusLevel.Error;
            }

            if (upper.Contains("[AVISO]")
                || upper.Contains("[WARN]")
                || upper.Contains("[SKIP]")
                || upper.Contains("[!]")
                || upper.Contains("SKIPPED")
                || upper.Contains("SKIPPING")
                || upper.Contains("IGNORADO")
                || upper.Contains("NOT FOUND")
                || upper.Contains("NÃO ENCONTRADO")
                || upper.Contains("ALREADY")
                || upper.Contains("CANCELAD"))
            {
                return LogStatusLevel.Warning;
            }

            if (upper.Contains("[OK]")
                || upper.Contains("✓")
                || upper.Contains("SUCESSO")
                || upper.Contains("SUCCESS")
                || upper.Contains("CONCLUÍD")
                || upper.Contains("COMPLETED")
                || upper.Contains("REMOVED:")
                || upper.Contains("APPLIED"))
            {
                return LogStatusLevel.Success;
            }

            return LogStatusLevel.Info;
        }
    }
}
