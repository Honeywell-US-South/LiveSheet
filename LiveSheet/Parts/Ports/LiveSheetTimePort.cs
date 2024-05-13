using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;

namespace LiveSheet.Parts.Ports;

public class LiveSheetTimePort : LivePort
{
    public LiveSheetTimePort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent, input, singularInputOnly, name, boundProperty)
    {
    }

    public override PortType PortType => PortType.LiveSheetTime;

    public LiveSheetTime GetLiveSheetTimeValue() => new LiveSheetTime(GetBsonValue().AsString);
    public DateTime GetDateValue() => new LiveSheetTime(GetBsonValue().AsString);
    public override bool CanAttachTo(ILinkable other)
    {
        if (other is not LiveSheetTimePort)
        {
            return false;
        }

        return base.CanAttachTo(other);
    }
}