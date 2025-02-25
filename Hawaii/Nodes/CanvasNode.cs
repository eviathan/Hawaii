using Hawaii.Interfaces;

namespace Hawaii.Nodes;

public class CanvasNode : Node
{
    public CanvasNode()
    {
        Renderer = new NodeRenderer();
        Size = new SizeF(400, 600);
    }
    
    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Yellow; 
            canvas.FillRectangle(0, 0, node.Size.Width, node.Size.Height);
        }
    }
}