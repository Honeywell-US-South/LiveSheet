using LiveSheet.Parts.Nodes;
using LiveSheet.Parts.Ports;

namespace LiveSheet.Utilities;

internal static class CircularLinkChecker
{
    private static readonly Dictionary<string, List<string>> _adjacencyList = new();

    private static void AddEdge(string source, string target)
    {
        if (!_adjacencyList.ContainsKey(source)) _adjacencyList[source] = new List<string>();

        _adjacencyList[source].Add(target);
    }

    private static bool HasCycle()
    {
        var visited = new HashSet<string>();
        var stack = new HashSet<string>();

        foreach (var node in _adjacencyList.Keys)
            if (!visited.Contains(node))
                if (Dfs(node, visited, stack))
                    return true;

        return false;
    }

    private static bool Dfs(string node, HashSet<string> visited, HashSet<string> stack)
    {
        visited.Add(node);
        stack.Add(node);

        if (_adjacencyList.ContainsKey(node))
            foreach (var neighbor in _adjacencyList[node])
                if (!visited.Contains(neighbor))
                {
                    if (Dfs(neighbor, visited, stack)) return true;
                }
                else if (stack.Contains(neighbor))
                {
                    return true;
                }

        stack.Remove(node);
        return false;
    }

    public static bool CheckForCircularLinks(this LiveSheetDiagram diagram)
    {
        _adjacencyList.Clear();
        // Get all links
        foreach (var l in diagram.Links.ToList())
            if (l.Source.Model is LivePort sp && l.Target.Model is LivePort tp)
            {
                // Get Value Node and Output Node
                var sourceIsInput = sp!.IsInput ? true : false;
                if (sp.Parent is LiveNode sn && tp.Parent is LiveNode tn)
                {
                    if (sourceIsInput)
                        AddEdge(sn.Guid, tn.Guid);
                    else
                        AddEdge(tn.Guid, sn.Guid);
                }
            }

        return HasCycle();
    }
}