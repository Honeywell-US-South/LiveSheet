using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;

namespace LiveSheet.Parts.Ports;

public class LiveLogicPort : LivePort
{
    public LiveLogicPort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent, input, singularInputOnly, name, boundProperty)
    {
    }

    public override PortType PortType => PortType.Logic;
    public bool GetBooleanValue() => GetBsonValue();

    public override bool CanAttachTo(ILinkable other)
    {
        if (other is not LiveLogicPort)
        {
            return false;
        }

        return base.CanAttachTo(other);
    }
}