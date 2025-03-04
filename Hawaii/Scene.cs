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
            RootNode = new RootNode(this);
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

            AddNode(RootNode);
        }

        public Transform GetTransform(Guid id)
        {
            return _transforms.TryGetValue(id, out var transform)
                ? transform
                : new Transform();
        }

        public void SetTransform(Guid id, Transform transform)
        {
            _transforms[id] = transform;
            TransformChanged?.Invoke(id);
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

        public Matrix3x2 GetParentTransform(Guid nodeId)
        {
            if (_worldTransformCache.TryGetValue(nodeId, out var transform))
                return transform;

            var local = GetTransform(nodeId);
            var node = Nodes[nodeId];

            // Get the node's local bounds and anchor offset
            var localBounds = node.GetLocalBounds();
            Vector2 anchorOffset = node.GetCenterOffset();

            // Local transform: scale, rotate, translate (no anchor offset yet)
            Matrix3x2 localMatrix =
                Matrix3x2.CreateScale(local.Scale) *
                Matrix3x2.CreateRotation(local.Rotation * MathF.PI / 180f) *
                Matrix3x2.CreateTranslation(local.Position);

            bool hasParent = HierarchyMap[nodeId].HasValue;

            if (!hasParent)
            {
                // For root nodes, apply anchor offset directly
                transform = Matrix3x2.CreateTranslation(-anchorOffset) * localMatrix;
            }
            else
            {
                // Get parent's world transform
                Matrix3x2 parentTransform = GetParentTransform(HierarchyMap[nodeId].Value);
                var parentNode = Nodes[HierarchyMap[nodeId].Value];
                Vector2 parentAnchorOffset = parentNode.GetCenterOffset();

                if (node.IgnoreAncestorScale)
                {
                    Vector2 parentWorldPosition = Vector2.Transform(Vector2.Zero, parentTransform);
                    float parentWorldRotation = (float)Math.Atan2(parentTransform.M21, parentTransform.M11);
                    Matrix3x2 adjustedParentTransform =
                        Matrix3x2.CreateRotation(parentWorldRotation) *
                        Matrix3x2.CreateTranslation(parentWorldPosition);
                    transform = Matrix3x2.CreateTranslation(-anchorOffset) * localMatrix * adjustedParentTransform;
                }
                else
                {
                    // Apply parent's anchor offset to shift child's position relative to parent's center
                    Matrix3x2 parentOffsetMatrix = Matrix3x2.CreateTranslation(parentAnchorOffset);
                    transform = Matrix3x2.CreateTranslation(-anchorOffset) * localMatrix * parentOffsetMatrix * parentTransform;
                }
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

                var effectiveBounds = node.IgnoreAncestorScale
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

            var childIds = HierarchyMap
                .Where(kvp => kvp.Value == node.Id)
                .Select(kvp => kvp.Key);

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

            var childIds = HierarchyMap
                .Where(kvp => kvp.Value == nodeId)
                .Select(kvp => kvp.Key);

            foreach (var childId in childIds)
                InvalidateNode(childId);

            InvalidateView?.Invoke();
        }

        internal void BuildCaches()
        {
            ClearNodes();
        }
    }
}