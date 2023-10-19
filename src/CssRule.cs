namespace Cascadium;

class CssRule
{
    public string? Selector { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    public override String ToString() => $"{Selector} [{Properties.Count}]";
    public int Order { get; set; }
}