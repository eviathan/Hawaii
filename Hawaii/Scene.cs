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

        public Dictionary<Guid, Node> Nodes { get; } = new();

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
            if (parentId != null && Nodes.TryGetValue(parentId.Value, out var parentNode))
            {
                parentNode.AddChild(node);
            }

            Nodes[node.Id] = node;
            TransformChanged?.Invoke(node.Id);
        }
        
        public void ClearNodes()
        {
            Nodes.Clear();
            AddNode(RootNode);
        }

        public Transform GetTransform(Guid id)
        {
            return Nodes.TryGetValue(id, out var node)
                ? node.Transform
                : new Transform();
        }

        public void InvalidateTransform(Guid id)
        {
            TransformChanged?.Invoke(id);
        }

        public Matrix3x2 GetWorldTransform(Guid nodeId)
        {
            Matrix3x2 transform = Matrix3x2.Identity;
            Guid? currentId = nodeId;
            while (currentId != null)
            {
                if (!Nodes.TryGetValue(currentId.Value, out var currentNode))
                    break;
                transform = GetParentTransform(currentId.Value) * transform;
                currentId = currentNode?.Parent?.Id;
            }
            return transform;
        }

        public Matrix3x2 GetParentTransform(Guid nodeId)
        {
            Matrix3x2 transform;
            var local = GetTransform(nodeId);
            var node = Nodes[nodeId];
            var localBounds = node.GetLocalBounds();
            Vector2 anchorOffset = node.GetCenterOffset();

            // Local transform: scale, rotate, translate
            Matrix3x2 localMatrix =
                Matrix3x2.CreateScale(local.Scale) *
                Matrix3x2.CreateRotation(local.Rotation.DegreesToRadians()) *
                Matrix3x2.CreateTranslation(local.Position);

            bool hasParent = node.Parent != null;

            if (!hasParent)
            {
                transform = Matrix3x2.CreateTranslation(-anchorOffset) * localMatrix;
            }
            else
            {
                var parentNode = node.Parent;

                Matrix3x2 parentTransform = GetWorldTransform(parentNode.Id);
                Vector2 parentAnchorOffset = parentNode.GetCenterOffset();

                if (node.IgnoreAncestorScale)
                {
                    // Extract parent's transformation components
                    var parentWorldPos = Vector2.Transform(Vector2.Zero, parentTransform);
                    float parentRotation = (float)Math.Atan2(parentTransform.M21, parentTransform.M11);
                    var parentScale = parentTransform.GetScale();

                    // Apply parent's scale only to the node's position, not its size
                    Vector2 scaledPosition = local.Position * parentScale;
                    Matrix3x2 adjustedLocalMatrix =
                        Matrix3x2.CreateScale(local.Scale) *  // Node's own scale only
                        Matrix3x2.CreateRotation(local.Rotation.DegreesToRadians()) *
                        Matrix3x2.CreateTranslation(scaledPosition);

                    // Position in parent's world space with rotation and translation
                    Matrix3x2 positionTransform =
                        Matrix3x2.CreateRotation(parentRotation) *
                        Matrix3x2.CreateTranslation(parentWorldPos);

                    transform = Matrix3x2.CreateTranslation(-anchorOffset) * adjustedLocalMatrix * positionTransform;
                }
                else
                {
                    Matrix3x2 parentOffsetMatrix = Matrix3x2.CreateTranslation(parentAnchorOffset);
                    transform = Matrix3x2.CreateTranslation(-anchorOffset) * localMatrix * parentOffsetMatrix * parentTransform;
                }
            }

            return transform;
        }

        public RectF GetWorldBounds(Guid nodeId)
        {
            var bounds = new RectF();
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

            return bounds;
        }

        public IEnumerable<Node> GetNodesInDrawOrder()
        {
            return TraverseDepthFirst(RootNode);
        }

        private IEnumerable<Node> TraverseDepthFirst(Node node)
        {
            yield return node;

            foreach (var child in node.Children)
                foreach (var descendant in TraverseDepthFirst(child))
                    yield return descendant;
        }

        private void InvalidateNode(Guid nodeId)
        {
            if (!Nodes.TryGetValue(nodeId, out var node))
                return;

            foreach (var child in node.Children)
                InvalidateNode(child.Id);

            InvalidateView?.Invoke();
        }

        internal void BuildCaches()
        {
            ClearNodes();
        }
    }
}