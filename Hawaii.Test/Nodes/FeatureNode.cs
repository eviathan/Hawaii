using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Extensions;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureNode : Node
{
    public const float HANDLE_OFFSET = 100f;

    private readonly ISceneService _sceneService;
    
    private readonly FeatureHandleNode _rotationHandle;
    
    private readonly FeatureHandleNode _translationHandle;

    public bool WasClicked { get; set; }
    
    public FeatureNode(ISceneService sceneService, FeatureHandleNode translationHandle, FeatureHandleNode rotationHandle)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
        _translationHandle = translationHandle ?? throw new ArgumentNullException(nameof(translationHandle));
        _rotationHandle = rotationHandle ?? throw new ArgumentNullException(nameof(rotationHandle));
        
        Renderer = new NodeRenderer();
        Size = new SizeF(100, 100);
        Center = Anchor.Center;

        _translationHandle.Feature = this;
        _translationHandle.Transform = new Transform
        {
            Position = new Vector2(0f, -HANDLE_OFFSET),
        };
        _translationHandle.Clicked += OnTranslationHandleClicked;
        _translationHandle.Dragged += OnTranslationHandleDragged;

        _rotationHandle.Feature = this;
        _rotationHandle.Transform = new Transform
        {
            Position = new Vector2(0f, HANDLE_OFFSET),
        };
        _rotationHandle.Clicked += OnRotationHandleClicked;
        _rotationHandle.Dragged += OnRotationHandleDragged;
        
        Children = [ translationHandle, rotationHandle];
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        Console.WriteLine($"Clicked at Local: {touchData.LocalPoint}, World: {touchData.WorldPoint}");
        WasClicked = true;
        return true;
    }

    public override bool OnTouchUp(TouchEventData touchData)
    {
        WasClicked = false;
        return true;
    }

    public override bool OnDrag(TouchEventData touchData, PointF localDelta)
    {
        var transform = _sceneService.GetTransform(Id);
        transform.Position += new Vector2(localDelta.X, localDelta.Y);
        _sceneService.SetTransform(Id, transform);
        
        return true;
    }
    
    private void OnTranslationHandleClicked(TouchEventData touchData)
    {
        _translationHandle.WasClicked = !_translationHandle.WasClicked;
    }

    private void OnTranslationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        Transform.Position += new Vector2(e.localDelta.X, e.localDelta.Y);
        _sceneService.SetTransform(Id, Transform);
    }

    private void OnRotationHandleClicked(TouchEventData obj)
    {
        _rotationHandle.WasClicked = !_rotationHandle.WasClicked;
    }
    
    private void OnRotationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        Transform.Rotation = PointF.Zero.Angle(e.touchData.LocalPoint);
        _sceneService.SetTransform(Id, Transform);
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