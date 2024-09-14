using Cascadium.Object;
using System;

namespace Cascadium;

public class CascadiumException : Exception
{
    public int Line { get; private set; }
    public int Column { get; private set; }
    public string LineText { get; private set; }

    internal CascadiumException(TokenDebugInfo snapshot, string input, string message) : base(message)
    {
        this.Line = snapshot.Line;
        this.Column = snapshot.Column;

        string[] lines = input.Split('\n');
        if (lines.Length >= snapshot.Line)
        {
            this.LineText = lines[snapshot.Line - 1];
        }
        else
        {
            this.LineText = "";
        }
    }
}
