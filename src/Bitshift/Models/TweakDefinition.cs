namespace Bitshift.Models
{
    public sealed record TweakDefinition(
        string Id,
        string ScriptFile,
        string NameKey,
        string DescKey,
        string DefaultName,
        string DefaultDesc,
        string Category);

    public sealed record TweakSummary(TweakDefinition Definition, bool IsApplied);
}
