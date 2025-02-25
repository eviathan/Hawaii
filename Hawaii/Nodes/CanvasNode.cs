using Hawaii.Interfaces;

namespace Hawaii.Nodes;

public class CanvasNode : Node
{
    public CanvasNode()
    {
        Renderer = new NodeRenderer();
    }
    
    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            throw new NotImplementedException();
        }
    }
}