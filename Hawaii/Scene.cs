using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Nodes;

namespace Hawaii;

public class Scene
{
    public Node RootNode { get; }

    public Dictionary<Guid, Guid?> HierarchyMap { get; }  = new();

    public Dictionary<Guid, Node> Nodes { get; } = new();

    private readonly Dictionary<Guid, Matrix3x2> _worldTransformCache = new();
    
    private readonly ISceneService _sceneService;

    private readonly Dictionary<Guid, RectF> _worldBoundsCache = new();

    private readonly List<RectF> _dirtyRegions = new();

    public Scene(ISceneService sceneService)
    {
        _sceneService = sceneService;
        _sceneService.TransformChanged += InvalidateNode;
        
        RootNode = new CanvasNode();
        AddNode(RootNode);
    }

    public void AddNode(Node node, Guid? parentId = null)
    {
        Nodes[node.Id] = node;
        HierarchyMap[node.Id] = parentId;
        MarkDirty(node.Id);
    }

    public void RemoveNode(Node node)
    {
        //if (!_nodes.ContainsKey(node.Id)) return;

        //var children = node.Children.ToList();

        //foreach (var child in children)
        //    RemoveNode(child);

        //_nodes.Remove(node.Id);
        //Guid? parentId = _parentDictionary.ContainsKey(node.Id) ? _parentDictionary[node.Id] : null;
        //_parentDictionary.Remove(node.Id);
        //_worldTransformCache.Remove(node.Id);
        //_worldBoundsCache.Remove(node.Id);

        //if (parentId.HasValue && _nodes.ContainsKey(parentId.Value))
        //    MarkDirty(parentId.Value);
        //else
        //    foreach (var topNode in _nodes.Values.Where(n => !_parentDictionary.ContainsKey(n.Id)))
        //        MarkDirty(topNode.Id);
    }

    public void ClearNodes()
    {
        Nodes.Clear();
        HierarchyMap.Clear();
        _worldTransformCache.Clear();
        _worldBoundsCache.Clear();
        _dirtyRegions.Clear();
    }

    public Matrix3x2 GetWorldTransform(Guid nodeId)
    {
        if (_worldTransformCache.TryGetValue(nodeId, out var transform)) 
            return transform;
        
        var local = _sceneService.GetTransform(nodeId);
        var node = Nodes[nodeId];
        var localMatrix = node.PropagateScale
            ? Matrix3x2.CreateScale(local.Scale) *
              Matrix3x2.CreateRotation(local.Rotation * MathF.PI / 180f) *
              Matrix3x2.CreateTranslation(local.Position)
            : Matrix3x2.CreateTranslation(local.Position) *
              Matrix3x2.CreateRotation(local.Rotation * MathF.PI / 180f);

        transform = HierarchyMap[nodeId].HasValue
            ? localMatrix * GetWorldTransform(HierarchyMap[nodeId].Value)
            : localMatrix;
        _worldTransformCache[nodeId] = transform;
        return transform;
    }

    public RectF GetWorldBounds(Guid nodeId)
    {
        if (!_worldBoundsCache.TryGetValue(nodeId, out var bounds))
        {
            var node = Nodes[nodeId];
            var localBounds = node.GetLocalBounds();
            var worldTransform = GetWorldTransform(nodeId);
            var localScale = _sceneService.GetTransform(nodeId).Scale;

            // Scale bounds only if not propagated (since propagation is in world transform)
            var effectiveBounds = node.PropagateScale
                ? localBounds
                : new RectF(0, 0, localBounds.Width * localScale.X, localBounds.Height * localScale.Y);

            var corners = new[]
            {
                Vector2.Transform(new Vector2(effectiveBounds.Left, effectiveBounds.Top), worldTransform),
                Vector2.Transform(new Vector2(effectiveBounds.Right, effectiveBounds.Top), worldTransform),
                Vector2.Transform(new Vector2(effectiveBounds.Right, effectiveBounds.Bottom), worldTransform),
                Vector2.Transform(new Vector2(effectiveBounds.Left, effectiveBounds.Bottom), worldTransform)
            };
            var minX = corners.Min(p => p.X);
            var maxX = corners.Max(p => p.X);
            var minY = corners.Min(p => p.Y);
            var maxY = corners.Max(p => p.Y);
            bounds = new RectF(minX, minY, maxX - minX, maxY - minY);
            _worldBoundsCache[nodeId] = bounds;
        }
        return bounds;
    }

    public IEnumerable<Node> GetNodesInDrawOrder()
    {
        return TraverseDepthFirst(RootNode);
    }

    private IEnumerable<Node> TraverseDepthFirst(Node node)
    {
        yield return node;
        
        var childIds = HierarchyMap.Where(kvp => kvp.Value == node.Id).Select(kvp => kvp.Key);
        
        foreach (var childId in childIds)
        {
            if (!Nodes.TryGetValue(childId, out var child))
                continue;
            
            foreach (var descendant in TraverseDepthFirst(child))
            {
                yield return descendant;
            }
        }
    }

    public void MarkDirty(Guid nodeId)
    {
        if (Nodes.ContainsKey(nodeId))
        {
            _dirtyRegions.Add(GetWorldBounds(nodeId));
            foreach (var child in Nodes[nodeId].Children)
                if (Nodes.ContainsKey(child.Id))
                    MarkDirty(child.Id);
        }
    }

    public RectF GetDirtyRegion()
    {
        if (_dirtyRegions.Count == 0)
            return RectF.Zero;

        var minX = _dirtyRegions.Min(rect => rect.Left);
        var minY = _dirtyRegions.Min(rect => rect.Top);
        var maxX = _dirtyRegions.Max(rect => rect.Right);
        var maxY = _dirtyRegions.Max(rect => rect.Bottom);
        
        _dirtyRegions.Clear();

        return new RectF(minX, minY, maxX - minX, maxY - minY);
    }

    public void SetTransform(Guid nodeId, Transform transform)
    {
        _sceneService.SetTransform(nodeId, transform);
    }

    private void InvalidateNode(Guid nodeId)
    {
        _worldTransformCache.Remove(nodeId);
        _worldBoundsCache.Remove(nodeId);
        
        MarkDirty(nodeId);
        
        var childIds = HierarchyMap
            .Where(kvp => kvp.Value == nodeId)
            .Select(kvp => kvp.Key);
        
        foreach (var childId in childIds)
            InvalidateNode(childId);
    }
}