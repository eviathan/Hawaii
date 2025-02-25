using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureHandleNode : Node
{
    public FeatureHandleNode()
    {
        Renderer = new NodeRenderer();
        Size = new SizeF(50, 50);
    }
    
    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillCircle(PointF.Zero, 50f);
        }
    }
}