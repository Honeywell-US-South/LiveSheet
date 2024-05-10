using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;
using LiveSheet.Utilities;

namespace LiveSheet;

public class LiveSheetLogic
{
    public LiveSheetDiagram Diagram { get; set; }
    public bool Enabled { get; private set; } = false;

    public LiveSheetLogic(LiveSheetDiagram diagram)
    {
        this.Diagram = diagram;
        this.Diagram.Nodes.Added += NodeAdded;
        this.Diagram.Nodes.Removed += NodeRemoved;
        this.Diagram.Links.Added += OnLinkAdded;
        this.Diagram.Links.Removed += OnLinkRemoved;
    }

    public void EnableLogic()
    {
        if (Enabled) return;

        Enabled = true;
    }

    public void DisableLogic()
    {
        if (!Enabled) return;
        Enabled = false;
    }


    private void OnValueChanged(NodeModel obj)
    {
        if (!Enabled)
            return;

        bool success = false;
        if (obj is LiveNode node && node.GetOutputPorts().Any(x => x.HasLinks()))
        {
            success = node.TryUpdate();
        }
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        link.TargetChanged += OnLinkTargetChanged;
    }

    private void OnLinkTargetChanged(BaseLinkModel link, Anchor oldTarget, Anchor newTarget)
    {
        if (link.Selected)
            return;


        if (link.Source is SinglePortAnchor sspa && link.Target is SinglePortAnchor tspa)
        {
            if (sspa.Port is LivePort sbm && tspa.Port is LivePort tbm)
            {
                // TOOD: Check For Circular Links
                if (Diagram.CheckForCircularLinks())
                {
                    Diagram.Links.Remove(link);
                    return;
                }


                // If the source is the target or if both are inputs or outputs, remove the link
                if (sbm == tbm || sbm.IsInput == tbm.IsInput)
                {
                    Diagram.Links.Remove(link);
                    return;
                }

                // If the link is made backwards, reverse it
                if (sbm.IsInput == true && tbm.IsInput == false)
                {
                    Diagram.Links.Remove(link);
                    Diagram.Links.Add(new LinkModel(tbm, sbm));
                }

                if (tbm.Parent is LiveNode node)
                {
                    node.Process();
                }
            }
            else
            {
                Diagram.Links.Remove(link);
            }
        }
    }

    private void OnLinkRemoved(BaseLinkModel link)
    {
        if (link.Source.Model is LivePort sp)
        {
            if (sp.Parent is LiveNode node)
            {
                node.Process();
            }
        }

        if (link.Target.Model is LivePort tp)
        {
            if (tp.Parent is LiveNode node)
            {
                node.Process();
            }
        }

        link.TargetChanged -= OnLinkTargetChanged;
    }


    private void NodeRemoved(NodeModel node)
    {
        if (node is LiveNode liveNode)
        {
            liveNode.ValueChanged -= OnValueChanged;
        }
    }


    private void NodeAdded(NodeModel node)
    {
        if (node is LiveNode liveNode)
        {
            liveNode.ValueChanged += OnValueChanged;
        }
    }

    public void Dispose()
    {
        Diagram.Nodes.Added -= NodeAdded;
        Diagram.Nodes.Removed -= NodeRemoved;
        Diagram.Links.Added -= OnLinkAdded;
        Diagram.Links.Removed -= OnLinkRemoved;
    }
}