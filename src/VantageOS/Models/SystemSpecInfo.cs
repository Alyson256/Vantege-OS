namespace VantageOS.Models
{
    public class SpecItem
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class SpecCategory
    {
        public string Name { get; set; } = string.Empty;
        public string ColorKey { get; set; } = "Blue500Brush";
        public List<SpecItem> Items { get; set; } = new();
    }

    public class SystemSpecInfo
    {
        public List<SpecCategory> Categories { get; set; } = new();
    }
}
