using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;

namespace LiveSheet.Parts.Ports;

public class LiveStringPort : LivePort
{
    public LiveStringPort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent, input, singularInputOnly, name, boundProperty)
    {
    }

    public override PortType PortType => PortType.String;
    public string GetStringValue() => GetBsonValue();

    public override bool CanAttachTo(ILinkable other)
    {
        if (other is not LiveStringPort)
        {
            return false;
        }

        return base.CanAttachTo(other);
    }
}