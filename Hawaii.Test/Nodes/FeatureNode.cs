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

    public Scene Scene { get; set; }
    
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
        rotationHandle.Color = Colors.Aquamarine;
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

    public Vector2 DebugCenter;
    
    private void OnRotationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        var transform = _sceneService.GetTransform(Id);
        var worldTransform = Scene.GetParentTransform(Id);

        var cursor = new Vector2(
            e.touchData.WorldPoint.X,
            e.touchData.WorldPoint.Y);

        var featureCenter = Vector2.Transform(
            new Vector2(Size.Width * 0.5f, Size.Height * 0.5f),
            worldTransform);

        var deltaX = cursor.X - featureCenter.X;
        var deltaY = cursor.Y - featureCenter.Y;

        var angleRadians = MathF.Atan2(deltaY, deltaX);
        var angleDegrees = angleRadians * (180f / MathF.PI) - 90f;

        transform.Rotation = angleDegrees;

        _sceneService.SetTransform(Id, transform);

        DebugCenter = new Vector2(
            e.touchData.LocalPoint.X + _rotationHandle.Size.Height * 0.5f,
            e.touchData.LocalPoint.Y + _rotationHandle.Size.Width * 0.5f + HANDLE_OFFSET
        );
    }


    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not FeatureNode featureNode)
                return;
            
            canvas.FillColor = featureNode.WasClicked ? Colors.Red : Colors.HotPink;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
            
            canvas.DrawLine(featureNode.DebugCenter, new Point(featureNode.Size.Width * 0.5f, featureNode.Size.Height * 0.5f));

            canvas.FillColor = Colors.Black;
            canvas.FillCircle(new PointF(node.Size.Width * 0.5f, node.Size.Height * 0.5f), 10);
            
            canvas.FillColor = Colors.Green;
            canvas.FillEllipse(featureNode.DebugCenter.X - 5, featureNode.DebugCenter.Y - 5, 10, 10);
        }
    }
}