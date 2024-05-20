using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;

namespace LiveSheet.Parts.Ports;

public class LiveNumericPort : LivePort
{
    public LiveNumericPort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent, input, singularInputOnly, name, boundProperty)
    {
    }

    public override PortType PortType => PortType.Numeric;
    public decimal GetDecimalValue() => GetBsonValue();
    public int GetIntValue() => GetBsonValue().AsInt32;
    public double GetDoubleValue() => GetBsonValue();
    public long GetLongValue() => GetBsonValue();

    public override bool CanAttachTo(ILinkable other)
    {
        if (other is not LiveNumericPort)
        {
            return false;
        }

        return base.CanAttachTo(other);
    }
}