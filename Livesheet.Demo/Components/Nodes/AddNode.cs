using Blazor.Diagrams.Core.Geometry;
using LiteDB;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;

namespace Livesheet.Demo.Components.Nodes;

public class AddNode : LiveNode
{
    public AddNode() : base(new(0, 0))
    {
        this.SilentSetValue(0);
        AddPort(new LiveNumericPort(this, true));
        AddPort(new LiveNumericPort(this, true));
        AddPort(new LiveNumericPort(this, false));
    }

    public override string NodeName => "Add Node";

    public override void Process()
    {
        var i1 = Ports[0];
        var i2 = Ports[1];
        if (i1 is LiveNumericPort inputA && i2 is LiveNumericPort inputB)
        {
            BsonValue inputAValue = inputA.HasLinks() ? inputA.GetBsonValue() : new(0);
            BsonValue inputBValue = inputB.HasLinks() ? inputB.GetBsonValue() : new(0);
            Value = inputAValue + inputBValue;
        }
        else
        {
            Value = new(0);
        }

        base.Process();
    }
}