using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;

namespace LiveSheet.Parts.Links;

public class LiveLink
{
    public LiveLink()
    {
    }

    public LiveLink(string sourceGuid, string sourcePortGuid, string targetGuid, string targetPortGuid)
    {
        SourceGuid = sourceGuid;
        SourcePortGuid = sourcePortGuid;
        TargetGuid = targetGuid;
        TargetPortGuid = targetPortGuid;
    }

    public LiveLink(BaseLinkModel link)
    {
        if (link.Source is SinglePortAnchor sspa && link.Target is SinglePortAnchor tspa)
            if (sspa.Port is LivePort sbm && tspa.Port is LivePort tbm)
                if (sbm.Parent is LiveNode sn && tbm.Parent is LiveNode tn)
                {
                    SourceGuid = sn.Guid;
                    SourcePortGuid = sbm.Guid;
                    TargetGuid = tn.Guid;
                    TargetPortGuid = tbm.Guid;
                }
    }

    public string SourceGuid { get; set; } = string.Empty;
    public string SourcePortGuid { get; set; } = string.Empty;
    public string TargetGuid { get; set; } = string.Empty;
    public string TargetPortGuid { get; set; } = string.Empty;
}