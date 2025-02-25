using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class ImageNode : Node
{
    public ImageNode()
    {
        PropagateScale = true;
        Renderer = new NodeRenderer();
        Size = new SizeF(640, 400);
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Coral;
            canvas.FillRectangle(0, 0, node.Size.Width, node.Size.Height);
        }
    }
}