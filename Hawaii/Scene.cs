using System.Numerics;
using Hawaii.Extensions;
using Hawaii.Nodes;

namespace Hawaii
{
    public class Scene
    {
        public readonly SceneCamera Camera;

        public Node RootNode { get; }

        public Dictionary<Guid, Node> Nodes { get; } = new();

        public Action? InvalidateView { get; set; }

        public event Action<Guid>? TransformChanged;

        public Scene(SceneCamera camera)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));

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
       
        public void InvalidateTransform(Guid id)
        {
            TransformChanged?.Invoke(id);
        }

        private Matrix3x2 GetLocalTransform(Node node)
        {
            var local = node.Transform;
            var originOffset = node.GetOriginOffset();
            var alignmentOffset = node.GetAlignmentOffset();

            var localMatrix =
                Matrix3x2.CreateTranslation(-originOffset)
                * Matrix3x2.CreateScale(local.Scale)
                * Matrix3x2.CreateRotation(local.Rotation.DegreesToRadians())
                * Matrix3x2.CreateTranslation(local.Position + alignmentOffset);

            return localMatrix;

        }

        public Matrix3x2 GetWorldTransform(Guid nodeId)
        {
            var transform = Matrix3x2.Identity;
            
            if (!Nodes.TryGetValue(nodeId, out var currentNode))
                return transform;

            var nodeTransform = GetLocalTransform(currentNode);
            var parentTransform = currentNode.Parent != null 
                ? GetParentTransform(nodeId)
                : Matrix3x2.Identity;
            
            return nodeTransform * parentTransform;
        }

        public Matrix3x2 GetParentTransform(Guid nodeId)
        {
            var transform = Matrix3x2.Identity;
            var node = Nodes[nodeId];
            var parentNode = node.Parent;

            if (parentNode == null)
                return transform;
            
            var parentTransform = GetLocalTransform(parentNode);
            var grandparentTransform = GetParentTransform(parentNode.Id);
            
            return parentTransform * grandparentTransform;
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
    }
}