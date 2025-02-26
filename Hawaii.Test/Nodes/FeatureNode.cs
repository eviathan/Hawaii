using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureNode : Node
{
    public const float HANDLE_OFFSET = 100f;

    private readonly ISceneService _sceneService;
    
    public bool WasClicked { get; set; }
    
    public FeatureNode(ISceneService sceneService)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
        
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

    public override bool OnClicked(TouchEventData touchData)
    {
        Console.WriteLine($"Clicked at Local: {touchData.LocalPoint}, World: {touchData.WorldPoint}");
        WasClicked = true;
        return true;
    }

    public override bool OnDrag(TouchEventData touchData, PointF localDelta)
    {
        var transform = _sceneService.GetTransform(Id);
        transform.Position += new Vector2(localDelta.X, localDelta.Y);
        _sceneService.SetTransform(Id, transform);
        
        return true;
    }

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