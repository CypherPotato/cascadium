using Cascadium.Object;
using System;

namespace Cascadium;

public class CascadiumException : Exception
{
    public int Line { get; private set; }
    public int Column { get; private set; }
    public string LineText { get; private set; }

    internal CascadiumException(TokenDebugInfo snapshot, string message) : base(message)
    {
        Line = snapshot.Line;
        Column = snapshot.Column;
        LineText = snapshot.LineText;
    }
}
