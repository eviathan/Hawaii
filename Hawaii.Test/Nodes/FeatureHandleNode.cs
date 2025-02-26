using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureHandleNode : Node
{
    private readonly ISceneService _sceneService;
    
    public FeatureNode Feature { get; set; }

    public Color Color { get; set; } = Colors.Black;

    public bool WasClicked { get; set; }

    public event Action<TouchEventData> Clicked;
    
    public event Action<(TouchEventData touchData, PointF localDelta)> Dragged;

    public FeatureHandleNode(ISceneService sceneService)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
        Renderer = new NodeRenderer();
        Center = Anchor.Center;
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

            canvas.FillColor = featureHandleNode.Color;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
        }
    }
}