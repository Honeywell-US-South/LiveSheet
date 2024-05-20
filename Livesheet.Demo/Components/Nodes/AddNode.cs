using Blazor.Diagrams.Core.Geometry;
using LiteDB;
using LiveSheet;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;

namespace Livesheet.Demo.Components.Nodes;

public class AddNode : LiveNode
{
    public AddNode() : base(new Point(0, 0))
    {
        SilentSetValue(0);
        AddPort(new LiveNumericPort(this, true));
        AddPort(new LiveNumericPort(this, true));
        AddPort(new LiveNumericPort(this, false));
    }

    public override string NodeName => "Add Node";

    public override void Process(List<EffectedNode>? effectedNodes = null)
    {
        if (this.OkToProcess(effectedNodes))
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
        }
    }
}