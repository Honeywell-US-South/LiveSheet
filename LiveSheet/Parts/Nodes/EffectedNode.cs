using Blazor.Diagrams.Core.Models.Base;

namespace LiveSheet.Parts.Nodes;

public class EffectedNode
{
    public EffectedNode()
    {
    }

    public EffectedNode(BaseLinkModel link, LiveNode node)
    {
        Link = link;
        Node = node;
    }

    public BaseLinkModel Link { get; set; }
    public LiveNode Node { get; set; }
}