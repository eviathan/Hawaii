using System.Diagnostics;
using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;
using Hawaii.Nodes;

namespace Hawaii.Test.Nodes;

public class FeatureHandleNode : MarkerNode
{
    public FeatureNode Feature { get; set; }

    public Color Color { get; set; } = Colors.Black;

    public bool WasClicked { get; set; }

    public event Action<TouchEventData> Clicked;
    
    public event Action<(TouchEventData touchData, PointF localDelta)> Dragged;

    public FeatureHandleNode(Scene scene) : base(scene)
    {
        Renderer = new NodeRenderer();
        Origin = Origin.Center;
        Size = new SizeF(50, 50);
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        Clicked?.Invoke(touchData);
        return true;
    }
    
    public override bool OnDrag(TouchEventData touchData, PointF localDelta)
    {
        Dragged?.Invoke((touchData, localDelta));
        return true;
    }
    
    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not FeatureHandleNode featureHandleNode)
                return;

            Debug.WriteLine($"Drawing {featureHandleNode}: Pos {node.Transform.Position}, World Pos {Vector2.Transform(Vector2.Zero, node.Scene.GetWorldTransform(node.Id))}");

            canvas.FillColor = featureHandleNode.Color;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
        }
    }
}