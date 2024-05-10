using Blazor.Diagrams;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Options;
using LiveSheet.Parts.Links;
using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;
using LiveSheet.Parts.Serialization;
using Newtonsoft.Json;

namespace LiveSheet;

public static class LiveSheetHelper
{
    public static BlazorDiagram NewLiveSheet(
        Func<Blazor.Diagrams.Core.Diagram, ValueTask>? Save = null,
        Func<Blazor.Diagrams.Core.Diagram, ValueTask>? Copy = null,
        Func<Blazor.Diagrams.Core.Diagram, ValueTask>? Paste = null
    )
    {
        var options = new BlazorDiagramOptions()
        {
            AllowMultiSelection = true,
            Zoom = { Enabled = true, Inverse = true },
            Links =
            {
                DefaultRouter = new NormalRouter(),
                DefaultPathGenerator = new SmoothPathGenerator(),
                EnableSnapping = true,
                SnappingRadius = 4
            },
            GridSize = 20,
        };

        BlazorDiagram diagram = new BlazorDiagram(options);

        if (diagram.GetBehavior<KeyboardShortcutsBehavior>() is { } kbs)
        {
            if (Save != null)
            {
                kbs.SetShortcut("s", ctrl: true, shift: false, alt: false, Save);
            }

            if (Copy != null)
            {
                kbs.SetShortcut("c", ctrl: true, shift: false, alt: false, Copy);
            }

            if (Paste != null)
            {
                kbs.SetShortcut("v", ctrl: true, shift: false, alt: false, Paste);
            }
        }


        //TODO: Registration 


        return diagram;
    }

    public static Dictionary<string, object> SerializeNodeProperties(this LiveNode node)
    {
        var properties = new Dictionary<string, object>();

        foreach (var prop in node.GetType().GetProperties()
                     .Where(prop => Attribute.IsDefined(prop, typeof(LiveSerialize))))
            try
            {
                var value = prop.GetValue(node);
                if (value != null) properties.Add(prop.Name, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing property {prop.Name}: {ex.Message}");
            }

        return properties;
    }


    public static string SerializeLiveSheet(this LiveSheetDiagram diagram)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new LiveSheetSerializeConverter() },
            Formatting = Formatting.None // Optional: for nicer output
        };
        string json = JsonConvert.SerializeObject(diagram, settings);
        return json;
    }

    public static LiveSheetDiagram DeserializeLiveSheet(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new();

        LiveSheetDiagram diagram = new();

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new LiveSheetSerializeConverter() },
            Formatting = Formatting.None // Optional: for nicer output
        };
        try
        {
            diagram = JsonConvert.DeserializeObject<LiveSheetDiagram>(json, settings) ?? new();
        }
        catch
        {
            diagram = new();
        }

        return diagram;
    }


    public static string SaveLiveSheetData(this LiveSheetDiagram diagram)
    {
        var nodes = diagram.GetLiveNodes();
        var result = new List<Dictionary<string, object>>();

        foreach (var node in nodes)
            try
            {
                var serializedProperties = node.SerializeNodeProperties();
                result.Add(serializedProperties);
            }
            catch
            {
                // ignored
            }

        return JsonConvert.SerializeObject(result);
    }


    public static string Copy(this LiveSheetDiagram diagram)
    {
        var nodes = diagram.GetSelectedModels().Cast<LiveNode>().ToList();
        var result = new List<Dictionary<string, object>>();

        foreach (var node in nodes)
            try
            {
                var serializedProperties = node.SerializeNodeProperties();
                result.Add(serializedProperties);
            }
            catch
            {
                // ignored
            }

        return JsonConvert.SerializeObject(result);
    }

    public static void FitToScreen(this LiveSheetDiagram diagram, int margin = 300)
    {
        var nodes = diagram.Nodes.ToList();
        GroupModel group = new GroupModel(nodes);
        var temp = diagram.Groups.Add(group);
        diagram.SelectModel(temp, false);
        diagram.ZoomToFit(margin);
        if (group.Size != null)
        {
            var width = group.Size.Width * diagram.Zoom;
            var scaledMargin = margin * diagram.Zoom;
            if (diagram.Container != null)
            {
                var deltaX = (diagram.Container.Width / 2) - (width / 2) - scaledMargin;
                diagram.UpdatePan(deltaX, 0);
            }
        }

        diagram.Groups.Remove(temp);
        diagram.UnselectAll();
    }

    public static Point GetCenterOfScreen(this LiveSheetDiagram? diagram)
    {
        if (diagram == null)
        {
            return Point.Zero;
        }
        else
        {
            if (diagram.Container == null) return Point.Zero;
            var x = (diagram.Container.Width / 2 - diagram.Pan.X) / diagram.Zoom;
            var y = (diagram.Container.Height / 2 - diagram.Pan.Y) / diagram.Zoom;
            return new Point(x, y);
        }
    }

    public static void Clear(this LiveSheetDiagram diagram)
    {
        diagram.Nodes.Clear();
        diagram.Links.Clear();
        diagram.Groups.Clear();
    }

    public static void ClearSelection(this LiveSheetDiagram diagram)
    {
        diagram.UnselectAll();
    }

    public static void CenterOnNode(this LiveSheetDiagram diagram, LiveNode node, double margin = 0)
    {
        diagram.SelectModel(node, unselectOthers: true);
        diagram.ZoomToFit(margin);

        if (node.Size != null)
        {
            var width = node.Size.Width * diagram.Zoom;
            var scaledMargin = margin * diagram.Zoom;
            if (diagram.Container != null)
            {
                var deltaX = (diagram.Container.Width / 2) - (width / 2) - scaledMargin;
                diagram.UpdatePan(deltaX, 0);
            }
        }
    }

    public static LiveNode? AddNode(this LiveSheetDiagram diagram, LiveNode node, Point? position = null,
        string? nodeData = null)
    {
        Point point = position != null ? position : diagram.GetCenterOfScreen();
        LiveNode? newNode = null;
        if (Activator.CreateInstance(node.GetType()) is LiveNode model)
        {
            newNode = model;
        }


        if (newNode == null) return null;
        newNode.SetPosition(point.X, point.Y);

        if (!string.IsNullOrEmpty(nodeData))
        {
            JsonConvert.PopulateObject(nodeData, newNode);
            if (diagram.Nodes.Cast<LiveNode>().FirstOrDefault(x => x.Guid == newNode.Guid) is { } existing)
            {
                newNode.Guid = Guid.NewGuid().ToString();
                if (existing.Position == newNode.Position)
                {
                    newNode.SetPosition(existing.Position.X + 20, existing.Position.Y - 20);
                }
            }
        }

        return diagram.Nodes.Add(newNode);
    }


    public static void LoadLinks(this LiveSheetDiagram diagram, List<LiveLink> links)
    {
        foreach (var link in links)
        {
            var sourceNode = diagram.Nodes.Cast<LiveNode>().FirstOrDefault(x => x.Guid == link.SourceGuid);
            var targetNode = diagram.Nodes.Cast<LiveNode>().FirstOrDefault(x => x.Guid == link.TargetGuid);

            if (sourceNode is { } source && targetNode is { } target)
            {
                var sourcePort = sourceNode.Ports.Cast<LivePort>().FirstOrDefault(x => x.Guid == link.SourcePortGuid);
                var targetPort = targetNode.Ports.Cast<LivePort>().FirstOrDefault(x => x.Guid == link.TargetPortGuid);

                if (sourcePort is { } sourcePortModel && targetPort is { } targetPortModel)
                {
                    diagram.Links.Add(new LinkModel(sourcePortModel, targetPortModel));
                }
            }
        }
    }


    public static void LoadLiveSheetData(this LiveSheetDiagram diagram, string nodesData)
    {
        var nodes = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(nodesData);


        List<LiveLink> links = new();

        foreach (var node in nodes ?? new List<Dictionary<string, object>>())
        {
            bool nodeDeserializtionFailure = false;
            try
            {
                var nodeType = node["NodeType"].ToString() ?? string.Empty;
                var type = Type.GetType(nodeType);
                if (type != null)
                {
                    if (Activator.CreateInstance(type) is LiveNode model)
                    {
                        diagram.AddNode(model, nodeData: JsonConvert.SerializeObject(node));
                    }
                }
            }
            catch
            {
                nodeDeserializtionFailure = true;
            }

            if (!nodeDeserializtionFailure)
            {
                try
                {
                    var nodeLinks =
                        JsonConvert.DeserializeObject<List<LiveLink>>(node["LiveLinks"].ToString() ?? string.Empty);

                    if (nodeLinks != null && nodeLinks.Any())
                    {
                        links.AddRange(nodeLinks);
                    }
                }
                catch
                {
                    // Ignore
                }
            }
        }

        diagram.LoadLinks(links);
    }
}