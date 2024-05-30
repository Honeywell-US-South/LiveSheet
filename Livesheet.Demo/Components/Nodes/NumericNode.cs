using Blazor.Diagrams.Core.Geometry;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;

namespace Livesheet.Demo.Components.Nodes;

public class NumericNode : LiveNode
{
    public NumericNode() : base(new Point(0, 0))
    {
        Random r = new();
        SilentSetValue(r.Next(0, 100));
        AddPort(new LiveNumericPort(this, false));
    }

    public override string NodeName => "Numeric Node";
}