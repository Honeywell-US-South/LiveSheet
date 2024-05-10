using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using LiteDB;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Serialization;

namespace LiveSheet.Parts.Ports;

public abstract class LivePort : PortModel
{
    protected LivePort(LiveNode parent, bool input, bool singularInputOnly = true, string? name = null,
        object? boundProperty = null) : base(parent,
        input ? PortAlignment.Left : PortAlignment.Right)
    {
        IsInput = input;
        Guid = $"port-{(IsInput ? "input" : "output")} {parent.Ports.Count}";
        SingularInputOnly = singularInputOnly;
        Name = name ?? string.Empty;
        BoundProperty = boundProperty;
    }

    public object? BoundProperty { get; private set; }
    public string Name { get; private set; }
    public bool IsInput { get; private set; }
    public bool SingularInputOnly { get; private set; }

    [LiveSerialize] public string Guid { get; set; }

    public virtual PortType PortType { get; set; } = PortType.None;

    public List<BaseLinkModel> GetAllValidLinks()
    {
        List<BaseLinkModel> links = new List<BaseLinkModel>();
        Links.ToList().ForEach(link =>
        {
            if (link.Target.Model is LivePort && link.Source.Model is LivePort)
            {
                links.Add(link);
            }
        });
        return links;
    }

    public bool HasLinks()
    {
        return GetAllValidLinks().Any();
    }

    public int LinkCount()
    {
        return GetAllValidLinks().Count;
    }

    public BaseLinkModel? GetFirstLink()
    {
        return GetAllValidLinks().FirstOrDefault();
    }


    public BsonValue GetBsonValue()
    {
        if (HasLinks())
        {
            var link = GetFirstLink();
            if (link != null)
            {
                return GetInputValue(this, link);
            }
        }

        return BsonValue.Null;
    }

    public List<BsonValue> GetBsonValues()
    {
        var values = new List<BsonValue>();
        if (HasLinks())
        {
            var validLinks = GetAllValidLinks();
            foreach (var link in validLinks)
            {
                var val = GetInputValue(this, link);
                if (val != BsonValue.Null)
                {
                    values.Add(val);
                }
            }
        }

        return values;
    }

    public BsonValue GetInputValue(LivePort port, BaseLinkModel link)
    {
        if (link.Source.Model is not LivePort sp || link.Target.Model is not LivePort tp) return BsonValue.Null;
        var p = sp == port ? tp : sp;
        if (p.Parent is LiveNode node)
        {
            return node.Value;
        }

        return BsonValue.Null;
    }

    public override bool CanAttachTo(ILinkable other)
    {
        if (other is LivePort port)
        {
            if (port.Parent == Parent) // Can't connect to self
            {
                return false;
            }

            if (port.IsInput == IsInput) // Can't connect to same interface type
            {
                return false;
            }

            if (this is { IsInput: true, SingularInputOnly: true }) // Check for max inputs on this
            {
                if (LinkCount() != 0)
                {
                    return false;
                }
            }
            else if (port is { IsInput: true, SingularInputOnly: true }) // Check for max inputs on other
            {
                if (port.LinkCount() != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }
}