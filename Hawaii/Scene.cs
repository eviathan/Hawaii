using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Extensions;
using Hawaii.Interfaces;
using Hawaii.Nodes;

namespace Hawaii
{
    public class Scene
    {
        public Node RootNode { get; }
        public Dictionary<Guid, Guid?> HierarchyMap { get; } = new();
        public Dictionary<Guid, Node> Nodes { get; } = new();
        private readonly Dictionary<Guid, Transform> _transforms = [];
        private readonly Dictionary<Guid, Matrix3x2> _worldTransformCache = new();
        private readonly Dictionary<Guid, RectF> _worldBoundsCache = new();
        private readonly List<RectF> _dirtyRegions = new();
        public Action? InvalidateView { get; set; }
        public event Action<Guid>? TransformChanged;

        public Scene()
        {
            RootNode = new CanvasNode(this);
            AddNode(RootNode);
            TransformChanged += InvalidateNode;
        }

        public void AddNode(Node node, Guid? parentId = null)
        {
            Nodes[node.Id] = node;
            HierarchyMap[node.Id] = parentId;
            MarkDirty(node.Id);
        }
        
        public void ClearNodes()
        {
            Nodes.Clear();
            HierarchyMap.Clear();
            _transforms.Clear();
            _worldTransformCache.Clear();
            _worldBoundsCache.Clear();
            _dirtyRegions.Clear();
        }

        public Matrix3x2 GetWorldTransform(Guid nodeId)
        {
            Matrix3x2 transform = Matrix3x2.Identity;
            var currentId = nodeId;
            while (currentId != Guid.Empty)
            {
                transform = GetParentTransform(currentId) * transform;
                currentId = HierarchyMap[currentId] ?? Guid.Empty;
            }
            return transform;
        }

        public Transform GetTransform(Guid id)
        {
            return _transforms.TryGetValue(id, out var transform) ? transform : new Transform();
        }

        public void SetTransform(Guid id, Transform transform)
        {
            _transforms[id] = transform;
            TransformChanged?.Invoke(id);
        }

        public Matrix3x2 GetParentTransform(Guid nodeId)
        {
            if (_worldTransformCache.TryGetValue(nodeId, out var transform)) 
                return transform;

            var local = GetTransform(nodeId);
            var node = Nodes[nodeId];
            Vector2 adjustedPosition = local.Position;
            Matrix3x2 localMatrix;

            // Apply alignment if not root
            if (HierarchyMap[nodeId].HasValue && node.Alignment != Alignment.None)
            {
                var parent = Nodes[HierarchyMap[nodeId].Value];
                Vector2 parentSize = new Vector2(parent.Size.Width, parent.Size.Height);
                Vector2 nodeSize = new Vector2(node.Size.Width, node.Size.Height);
                Vector2 alignmentOffset = node.Alignment switch
                {
                    Alignment.Center => (parentSize - nodeSize) / 2,
                    Alignment.TopLeft => Vector2.Zero,
                    Alignment.TopRight => new Vector2(parentSize.X - nodeSize.X, 0),
                    Alignment.BottomLeft => new Vector2(0, parentSize.Y - nodeSize.Y),
                    Alignment.BottomRight => parentSize - nodeSize,
                    _ => Vector2.Zero
                };
                adjustedPosition += alignmentOffset;
            }

            // Compute center offset based on Anchor
            Vector2 centerOffset = node.Center switch
            {
                Anchor.TopLeft => Vector2.Zero,
                Anchor.TopCenter => new Vector2(node.Size.Width / 2, 0),
                Anchor.TopRight => new Vector2(node.Size.Width, 0),
                Anchor.CenterLeft => new Vector2(0, node.Size.Height / 2),
                Anchor.Center => new Vector2(node.Size.Width / 2, node.Size.Height / 2),
                Anchor.CenterRight => new Vector2(node.Size.Width, node.Size.Height / 2),
                Anchor.BottomLeft => new Vector2(0, node.Size.Height),
                Anchor.BottomCenter => new Vector2(node.Size.Width / 2, node.Size.Height),
                Anchor.BottomRight => new Vector2(node.Size.Width, node.Size.Height),
                _ => Vector2.Zero
            };

            // Base transform: Start with parent transform if exists
            Matrix3x2 parentMatrix = HierarchyMap[nodeId].HasValue
                ? GetParentTransform(HierarchyMap[nodeId].Value)
                : Matrix3x2.Identity;

            // Local transform: Apply scale only if PropagateScale
            localMatrix = Matrix3x2.CreateTranslation(-centerOffset) *
                          (node.PropagateScale ? Matrix3x2.CreateScale(local.Scale) : Matrix3x2.Identity) *
                          Matrix3x2.CreateRotation(local.Rotation * MathF.PI / 180f) *
                          Matrix3x2.CreateTranslation(adjustedPosition + centerOffset);

            // Combine with parent transform
            transform = localMatrix * parentMatrix;

            // Position mode adjustments (only affect position)
            if (node.Position == PositionMode.Static)
            {
                transform = Matrix3x2.CreateTranslation(adjustedPosition) * parentMatrix; // No local scale/rotation
            }

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
                var localScale = GetTransform(nodeId).Scale;

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

        private void InvalidateNode(Guid nodeId)
        {
            _worldTransformCache.Remove(nodeId);
            _worldBoundsCache.Remove(nodeId);
            MarkDirty(nodeId);
            var childIds = HierarchyMap.Where(kvp => kvp.Value == nodeId).Select(kvp => kvp.Key);
            foreach (var childId in childIds)
                InvalidateNode(childId);
            InvalidateView?.Invoke();
        }
    }
}