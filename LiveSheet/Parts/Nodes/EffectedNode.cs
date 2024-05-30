using Blazor.Diagrams.Core.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSheet.Parts.Nodes
{
public class EffectedNode
{
        public BaseLinkModel Link { get; set; }
        public LiveNode Node { get; set; }
        public EffectedNode() { }
    public EffectedNode(BaseLinkModel link, LiveNode node)
    {
        Link = link;
        Node = node;
    }
    }
}