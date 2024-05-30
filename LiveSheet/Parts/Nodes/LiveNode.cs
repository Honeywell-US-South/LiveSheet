using System.Collections.Concurrent;
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

    public event Action<NodeModel>? SizeChanging;

    private BsonValue _value = BsonValue.Null;


    protected LiveNode(Point position) : base(position)
    {
    }

    public virtual bool AllowForInputPortGrowth { get; private set; } = false;


    [LiveSerialize] public string Guid { get; set; } = System.Guid.NewGuid().ToString();

    [LiveSerialize] public Type NodeType => GetType();

    [LiveSerialize]
    public Point NodePosition
    {
        get => Position;
        set => Position = value;
    }

    [LiveSerialize]
    public Size? NodeSize
    {
        get => Size;
        set => Size = value;
    }


    [LiveSerialize] public string NodeAlias { get; set; } = string.Empty;
    public virtual string NodeName { get; } = string.Empty;


    public Action<LiveNode>? ValueChanged { get; set; }
    public DateTime LastUpdate { get; private set; } = DateTime.Now.ToUniversalTime();

    [LiveSerialize]
    public object RawValue
    {
        get => _value.RawValue;
        set => _value = new BsonValue(value);
    }


    public BsonValue Value
    {
        get => _value;
        set
        {
            try
            {
                if (Value != value)
                {
                    _value = value;
                    LastUpdate = DateTime.Now.ToUniversalTime();
                    OnValueChanged(value);
                }

                ValueChanged?.Invoke(this);
            }
            catch
            {
            }
        }
    }

    [LiveSerialize] public List<LiveLink> LiveLinks => GetParentConnections();

    public virtual void Dispose()
    {
        // Ignore
    }

    public void SilentSetValue(BsonValue value)
    {
        _value = value;
    }

    public string GetNodeDisplayName()
    {
        return string.IsNullOrEmpty(NodeAlias) ? NodeName : NodeAlias;
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
        Refresh();
    }


    private List<LiveLink> GetParentConnections()
    {
        List<LiveLink> l = new();
        var ports = GetInputPorts();
        ports.ForEach(port =>
        {
            var links = port.Links.ToList();
            links.ForEach(link => { l.Add(new LiveLink(link)); });
        });
        return l;
    }


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
            var effectedNodes = new List<EffectedNode>();

            // Initialize a thread-safe collection to hold the results
            var threadSafeEffectedNodes = new ConcurrentBag<EffectedNode>();

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
            });

            // convert the ConcurrentBag back to collection type
            effectedNodes = new List<EffectedNode>(threadSafeEffectedNodes);

            //process all notes at once
            Parallel.ForEach(effectedNodes, node => { node.Node.Process(effectedNodes); });
        }
        catch
        {
            success = false;
        }

        return success;
    }

    public bool OkToProcess(List<EffectedNode>? effectedNodes = null)
    {
        var inputPorts = GetInputPorts();

        if (effectedNodes == null) return true;
        if (effectedNodes?.Count > 0) //has node. Check to see if can process
        {
            List<PortModel> checkPorts = new List<PortModel>();

            //find self (port activated the update) and exclude from check for parent
            foreach (var port in inputPorts)
            {
                if (port.HasLinks())
                {
                    var isSelf = effectedNodes.FirstOrDefault(x => x.Link.Source == (port.Links.Count > 0 ? port.Links[0].Source : null));
                    if (isSelf == null)
                    {
                        checkPorts.Add(port);
                    }
                }
            }


            if (checkPorts.Count > 0)
            {
                foreach (var port in checkPorts) //check these ports
                {
                    if (HasEffectedParent(port, effectedNodes ?? new()))
                    {
                        return false; //don't do the update at this point
                    }
                }
            }
        }
        return true;
    }

    public static bool HasEffectedParent(PortModel port, List<EffectedNode> effectedNodes)
    {
        if ((port.Links.Count > 0 ? port.Links[0].Source : null) is SinglePortAnchor anchor)
        {
            if (anchor.Port.Parent is LiveNode parent)
            {
                if (parent == null || effectedNodes.Count == 0) return false;

                if (effectedNodes.Any(x => x.Node.Guid == parent.Guid))
                {
                    return true;
                }
                else
                {
                    foreach (var pport in parent.Ports)
                    {
                        if (port.Links[0].Source is SinglePortAnchor panchor)
                        {
                            if (panchor.Port.Parent is LiveNode grandParent)
                            {
                                if (grandParent != parent)
                                {
                                    if (HasEffectedParent(pport, effectedNodes ?? new()))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }


                    }

                }
            }
        }

        return false;
    }
}