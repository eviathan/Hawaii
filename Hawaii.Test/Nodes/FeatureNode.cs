using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Extensions;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureNode : Node
{
    public const float HANDLE_OFFSET = 100f;

    private readonly Scene _scene;

    private readonly FeatureHandleNode _rotationHandle;
    
    private readonly FeatureHandleNode _translationHandle;
    
    public bool WasClicked { get; set; }
    
    public Vector2 DebugCenter;
    
    public FeatureNode(Scene scene, FeatureHandleNode translationHandle, FeatureHandleNode rotationHandle) : base(scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
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
        // Console.WriteLine($"Clicked at Local: {touchData.LocalPoint}, Parent: {touchData.ParentPoint}, World: {touchData.WorldPoint}");
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
        TranslateFeature(touchData);
        
        return true;
    }
    
    private void OnTranslationHandleClicked(TouchEventData touchData)
    {
        _translationHandle.WasClicked = !_translationHandle.WasClicked;
    }

    private void OnTranslationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        TranslateFeature(e.touchData, new Vector2(0, -HANDLE_OFFSET));
    }

    private void TranslateFeature(TouchEventData touchData, Vector2 offset = default)
    {
        var transform = Scene.GetTransform(Id);
        var worldTransform = Scene.GetWorldTransform(Id);
        var handleLocalPos = offset;
        var handleWorldPos = Vector2.Transform(handleLocalPos, worldTransform);
        var cursorWorldPos = new Vector2(touchData.WorldPoint.X, touchData.WorldPoint.Y);
        var delta = cursorWorldPos - handleWorldPos;
        Translate(delta, Space.World);
    }

    private void OnRotationHandleClicked(TouchEventData touchData)
    {
        Console.WriteLine($"Rot Clicked at Local: {touchData.LocalPoint}, Parent: {touchData.ParentPoint}, World: {touchData.WorldPoint}");
        _rotationHandle.WasClicked = !_rotationHandle.WasClicked;
    }

    
    private void OnRotationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        Console.WriteLine($"Rot Dragged at Local: {e.touchData.LocalPoint}, Parent: {e.touchData.ParentPoint}, World: {e.touchData.WorldPoint}");
        
        var transform = _scene.GetTransform(Id);
        var worldTransform = _scene.GetParentTransform(Id);
        
        var cursor = new Vector2(e.touchData.WorldPoint.X, e.touchData.WorldPoint.Y);
        
        var featureCenter = Vector2.Transform(
            new Vector2(Size.Width * 0.5f, Size.Height * 0.5f),
            worldTransform);
        
        var deltaX = cursor.X - featureCenter.X;
        var deltaY = cursor.Y - featureCenter.Y;
        
        Console.WriteLine($"{deltaX},{deltaY}");
        
        var angleRadians = MathF.Atan2(deltaY, deltaX);
        var angleDegrees = angleRadians * (180f / MathF.PI) - 90f;
        
        transform.Rotation = angleDegrees;
        
        _scene.SetTransform(Id, transform);
        
        DebugCenter = new Vector2(
            e.touchData.ParentPoint.X,
            e.touchData.ParentPoint.Y + HANDLE_OFFSET
        );
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not FeatureNode featureNode) return;

            canvas.FillColor = featureNode.WasClicked ? Colors.Red : Colors.HotPink;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);

            // Draw world bounds for debugging
            var localBounds = node.GetLocalBounds(); // (0, 0, 100, 100)
            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 2f;
            canvas.DrawRectangle(localBounds.X, localBounds.Y, localBounds.Width, localBounds.Height);
        }
    }
}