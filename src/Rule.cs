namespace Cascadium;

internal class Rule
{
    public string? Selector { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    public override String ToString() => $"{Selector} [{Properties.Count}]";
    public int Order { get; set; }

    internal bool Exported { get; set; } = true;

    internal Int32 GetPropertiesHashCode()
    {
        int n = 0, j = 1;

        // the property order should impact on the hash code
        // so
        // foo: bar
        // bar: foo
        //
        // is different than
        //
        // bar: foo
        // foo: bar

        foreach (var kp in Properties)
        {
            n += (kp.Key.GetHashCode() + kp.Value.GetHashCode()) / 2;
            n *= j;
            j++;
        }
        return n / Properties.Count;
    }

}