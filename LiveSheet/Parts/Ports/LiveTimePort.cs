using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;

namespace LiveSheet.Parts.Ports;

public class LiveTimePort : LivePort
{
    public LiveTimePort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent, input, singularInputOnly, name, boundProperty)
    {
    }

    public override PortType PortType => PortType.Time;

    public LiveSheetTime GetLiveSheetTimeValue() => new LiveSheetTime(GetBsonValue().AsString);
    public DateTime GetDateValue() => new LiveSheetTime(GetBsonValue().AsString);
    public override bool CanAttachTo(ILinkable other)
    {
        if (other is not LiveTimePort)
        {
            return false;
        }

        return base.CanAttachTo(other);
    }
}