using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureHandleNode : Node
{
    public FeatureHandleNode()
    {
        Renderer = new NodeRenderer();
        Center = Anchor.Center;
        Size = new SizeF(50, 50);
    }

    public override bool OnClicked(PointF worldPoint)
    {
        return base.OnClicked(worldPoint);
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
        }
    }
}