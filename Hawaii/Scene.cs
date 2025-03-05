using System.Numerics;
using Hawaii.Extensions;
using Hawaii.Nodes;

namespace Hawaii
{
    public class Scene
    {
        public Node RootNode { get; }

        public Dictionary<Guid, Node> Nodes { get; } = new();

        public Action? InvalidateView { get; set; }

        public event Action<Guid>? TransformChanged;

        public Scene(SceneCamera camera)
        {
            RootNode = new RootNode(this, camera);
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

        public Matrix3x2 GetWorldTransform(Guid nodeId)
        {
            var transform = Matrix3x2.Identity;
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
       
        public void InvalidateTransform(Guid id)
        {
            TransformChanged?.Invoke(id);
        }

        // TODO: REIMPLEMENT AND SIMPLIFY THIS
        public Matrix3x2 GetParentTransform(Guid nodeId)
        {
            var transform = Matrix3x2.Identity;
            var node = Nodes[nodeId];
            var local = node.Transform;
            var localBounds = node.GetLocalBounds();
            var originOffset = node.GetOriginOffset();
            var alignmentOffset = node.GetAlignmentOffset();

            // Local transform: scale, rotate, translate (including alignment)
            var localMatrix =
                Matrix3x2.CreateScale(local.Scale) *
                Matrix3x2.CreateRotation(local.Rotation.DegreesToRadians()) *
                Matrix3x2.CreateTranslation(local.Position + alignmentOffset);

            var hasParent = node.Parent != null;

            if (!hasParent)
            {
                transform = Matrix3x2.CreateTranslation(-originOffset) * localMatrix;
            }
            else
            {
                var parentNode = node.Parent;
                var parentTransform = GetWorldTransform(parentNode.Id);
                var parentAnchorOffset = parentNode.GetOriginOffset();

                //if (node.IgnoreAncestorScale)
                //{
                //    var parentWorldPos = Vector2.Transform(Vector2.Zero, parentTransform);
                //    float parentRotation = (float)Math.Atan2(parentTransform.M21, parentTransform.M11);
                //    var parentScale = parentTransform.GetScale();

                //    var scaledPosition = (local.Position + alignmentOffset) * parentScale;
                //    var adjustedLocalMatrix =
                //        Matrix3x2.CreateScale(local.Scale) *
                //        Matrix3x2.CreateRotation(local.Rotation.DegreesToRadians()) *
                //        Matrix3x2.CreateTranslation(scaledPosition);

                //    var positionTransform =
                //        Matrix3x2.CreateRotation(parentRotation) *
                //        Matrix3x2.CreateTranslation(parentWorldPos);

                //    transform = Matrix3x2.CreateTranslation(-originOffset) * adjustedLocalMatrix * positionTransform;
                //}
                //else
                //{
                var parentOffsetMatrix = Matrix3x2.CreateTranslation(parentAnchorOffset);
                    transform = Matrix3x2.CreateTranslation(-originOffset) * localMatrix * parentOffsetMatrix * parentTransform;
                //}
            }

            return transform;
        }

        public IEnumerable<Node> GetNodesInDrawOrder()
        {
            return RootNode.TraverseDepthFirst();
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