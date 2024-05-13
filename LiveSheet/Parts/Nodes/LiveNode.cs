using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using LiteDB;
using LiveSheet.Parts.Links;
using LiveSheet.Parts.Ports;
using LiveSheet.Parts.Serialization;
using System.Collections.Concurrent;

namespace LiveSheet.Parts.Nodes;

public abstract class LiveNode : NodeModel, IDisposable
{
    private BsonValue _value = BsonValue.Null;
    public virtual bool AllowForInputPortGrowth { get; private set; } = false;

    public void SilentSetValue(BsonValue value)
    {
        _value = value;
    }


    protected LiveNode(Point position) : base(position)
    {
    }


    [LiveSerialize] public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [LiveSerialize] public Type NodeType => GetType();

    [LiveSerialize]
    public Point NodePosition
    {
        get => Position;
        set => Position = value;
    }

    [LiveSerialize] public string NodeAlias { get; set; } = string.Empty;
    public virtual string NodeName { get; private set; } = string.Empty;

    public string GetNodeDisplayName()
    {
        return string.IsNullOrEmpty(NodeAlias) ? NodeName : NodeAlias;
    }


    public Action<LiveNode>? ValueChanged { get; set; }
    public DateTime LastUpdate { get; private set; } = DateTime.Now.ToUniversalTime();

    [LiveSerialize]
    public object RawValue
    {
        get => _value.RawValue;
        set => _value = new(value);
    }


    public BsonValue Value
    {
        get => _value;
        set
        {
            if (Value != value)
            {
                _value = value;
                LastUpdate = DateTime.Now.ToUniversalTime();
                OnValueChanged(value);
            }
            ValueChanged?.Invoke(this);
            
        }
    }

    public virtual void Dispose()
    {
        // Ignore
    }

    public void AddPort(LivePort port)
    {
        base.AddPort(port);
    }

    public override bool CanAttachTo(ILinkable other)
    {
        return false;
    }

    public virtual void Process(List<EffectedNode>? effectedNodes = null)
    {
    }


    protected virtual void OnValueChanged(BsonValue value)
    {
        // Ignore
        this.Refresh();
    }


    private List<LiveLink> GetParentConnections()
    {
        List<LiveLink> l = new();
        var ports = GetInputPorts();
        ports.ForEach(port =>
        {
            var links = port.Links.ToList();
            links.ForEach(link => { l.Add(new(link)); });
        });
        return l;
    }

    [LiveSerialize] public List<LiveLink> LiveLinks => GetParentConnections();


    public virtual BsonValue GetInputValue(LivePort port, LinkModel link)
    {
        if (link is { Source: SinglePortAnchor sp, Target: SinglePortAnchor tp })
        {
            var p = sp.Port == port ? sp : tp;
            if (p.Port.Parent is LiveNode node) return node.Value;
        }

        return BsonValue.Null;
    }

    public List<LivePort> GetLivePorts()
    {
        return Ports.Where(p => p is LivePort).Cast<LivePort>().ToList();
    }

    public List<LivePort> GetInputPorts()
    {
        return GetLivePorts().Where(p => p.IsInput).ToList();
    }

    public List<LivePort> GetOutputPorts()
    {
        return GetLivePorts().Where(p => !p.IsInput).ToList();
    }

    public bool TryUpdate()
    {
        var success = true;
        try
        {
            var outputs = GetOutputPorts();
            var links = outputs.SelectMany(p => p.Links).ToList();
            List<EffectedNode> effectedNodes = new List<EffectedNode>();

            // Initialize a thread-safe collection to hold the results
            ConcurrentBag<EffectedNode> threadSafeEffectedNodes = new ConcurrentBag<EffectedNode>();

            Parallel.ForEach(links, link =>
            {
                if (link.Source is SinglePortAnchor sp && link.Target is SinglePortAnchor tp)
                {
                    if (sp.Port is LivePort source && tp.Port is LivePort)
                    {
                        var inputPort = source.IsInput ? sp : tp;
                        if (inputPort.Port.Parent is LiveNode node)
                            threadSafeEffectedNodes.Add(new EffectedNode(link, node));
                    }
                }
            });

            // convert the ConcurrentBag back to collection type
            effectedNodes = new List<EffectedNode>(threadSafeEffectedNodes);

            //process all notes at once
            Parallel.ForEach(effectedNodes, node =>
            {
                node.Node.Process(effectedNodes);
            });

        }
        catch
        {
            success = false;
        }

        return success;
    }

    
}