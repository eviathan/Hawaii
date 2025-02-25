using System.Numerics;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureNode : Node
{
    public FeatureNode()
    {
        Renderer = new NodeRenderer();
        Size = new SizeF(100, 100);
        Children =
        [
            new FeatureHandleNode
            {
                Transform = new Transform
                {
                    Position = new Vector2(0f, 40f),
                }
            },
            new FeatureHandleNode
            {
                Transform = new Transform
                {
                    Position = new Vector2(0f, -40f),
                }
            },
        ];
    }
    
    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Red;
            canvas.FillCircle(PointF.Zero, 50f);
        }
    }
}