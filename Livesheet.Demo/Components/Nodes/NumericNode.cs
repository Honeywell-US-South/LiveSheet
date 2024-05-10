using Blazor.Diagrams.Core.Geometry;
using LiteDB;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;
using LiveSheet.Parts.Serialization;

namespace Livesheet.Demo.Components.Nodes;

public class NumericNode : LiveNode
{
    public NumericNode() : base(new(0, 0))
    {
        Random r = new();
        this.SilentSetValue(r.Next(0, 100));
        AddPort(new LiveNumericPort(this, input: false));
    }

    public override string NodeName => "Numeric Node";
}