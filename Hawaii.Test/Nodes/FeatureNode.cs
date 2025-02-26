using System.Numerics;
using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureNode : Node
{
    public const float HANDLE_OFFSET = 100f;
    
    public FeatureNode()
    {
        Renderer = new NodeRenderer();
        Size = new SizeF(100, 100);
        Center = Anchor.Center;
        Children =
        [
            new FeatureHandleNode
            {
                Transform = new Transform
                {
                    Position = new Vector2(0f, -HANDLE_OFFSET),
                }
            },
            new FeatureHandleNode
            {
                Transform = new Transform
                {
                    Position = new Vector2(0f, HANDLE_OFFSET),
                }
            },
        ];
    }

    public override bool OnClicked(PointF worldPoint)
    {
        WasClicked = !WasClicked;
        return true;
    }

    public bool WasClicked { get; set; }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not FeatureNode featureNode)
                return;
            
            canvas.FillColor = featureNode.WasClicked ? Colors.Red : Colors.HotPink;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
        }
    }
}