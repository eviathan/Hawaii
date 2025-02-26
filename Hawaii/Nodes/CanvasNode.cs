using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii.Nodes;

public class CanvasNode : Node
{
    public CanvasNode()
    {
        Renderer = new NodeRenderer();
        Center = Anchor.Center;
        Size = new SizeF(1400, 1000);
    }

    public override bool OnClicked(PointF worldPoint)
    {
        return false;
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.White; 
            canvas.FillRectangle(0, 0, node.Size.Width, node.Size.Height);
        }
    }
}