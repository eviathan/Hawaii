using System.Diagnostics;
using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;
using Hawaii.Nodes;
using Hawaii.Test.Models;

namespace Hawaii.Test.Nodes;

public class FeatureNode : MarkerNode
{
    public const float HANDLE_OFFSET = 100f;

    public FeatureHandleNode RotationHandle;
    
    public FeatureHandleNode TranslationHandle;
    
    public bool WasClicked { get; set; }
    
    public Vector2 DebugCenter;
    
    public FeatureNode(Scene scene, Feature state) : base(scene)
    {
        Size = new SizeF(100, 100);
        Origin = Origin.Center;
        Transform = state.Transform;
        //IgnoreAncestorScale = true;

        Renderer = new NodeRenderer();
    }

    #region Hide
    public override void Initialise()
    {
        var parentOffset = Parent?.GetAlignmentOffset() ?? Vector2.Zero;

        //// TODO: Encapsulate this into its own method and call twice or move to featurehandle constructor
        TranslationHandle = new FeatureHandleNode(Scene);
        TranslationHandle.State = State;
        TranslationHandle.Feature = this;
        TranslationHandle.Transform = new Transform
        {
            Position = new Vector2(0f, -HANDLE_OFFSET) - parentOffset,
        };
        TranslationHandle.Clicked += OnTranslationHandleClicked;
        TranslationHandle.Dragged += OnTranslationHandleDragged;
        AddChild(TranslationHandle);

        RotationHandle = new FeatureHandleNode(Scene);
        RotationHandle.State = State;
        RotationHandle.Feature = this;
        RotationHandle.Color = Colors.Aquamarine;
        RotationHandle.Transform = new Transform
        {
            Position = new Vector2(0f, HANDLE_OFFSET) - parentOffset,
        };
        RotationHandle.Clicked += OnRotationHandleClicked;
        RotationHandle.Dragged += OnRotationHandleDragged;

        AddChild(RotationHandle);

        Debug.WriteLine($"ImageNode World Pos: {Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(Parent.Id))}");
        Debug.WriteLine($"FeatureNode World Pos: {Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(Id))}");
        Debug.WriteLine($"TranslationHandle World Pos: {Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(TranslationHandle.Id))}");
        Debug.WriteLine($"RotationHandle World Pos: {Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(RotationHandle.Id))}");
        Debug.WriteLine($"ImageNode Alignment Offset: {Parent.GetAlignmentOffset()}");
        Debug.WriteLine($"TranslationHandle Local Pos: {TranslationHandle.Transform.Position}");
        Debug.WriteLine($"RotationHandle Local Pos: {RotationHandle.Transform.Position}");
        Debug.WriteLine($"FeatureNode Screen Pos: {Scene.Camera.WorldToScreen(Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(Id)))}");
        Debug.WriteLine($"TranslationHandle Screen Pos: {Scene.Camera.WorldToScreen(Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(TranslationHandle.Id)))}");
        Debug.WriteLine($"RotationHandle Screen Pos: {Scene.Camera.WorldToScreen(Vector2.Transform(Vector2.Zero, Scene.GetWorldTransform(RotationHandle.Id)))}");
    }

    public override bool OnClicked(TouchEventData touchData)
    {
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
        var worldTransform = Scene.GetWorldTransform(Id);
        var handleWorldPos = Vector2.Transform(Vector2.Zero, worldTransform);
        var cursorWorldPos = new Vector2(touchData.WorldPoint.X, touchData.WorldPoint.Y);
        var delta = cursorWorldPos - handleWorldPos; // Move center so handle hits cursor
        Translate(delta, Space.World);

        return true;
    }

    private void OnTranslationHandleClicked(TouchEventData touchData)
    {
        TranslationHandle.WasClicked = !TranslationHandle.WasClicked;
    }

    private void OnTranslationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        var parentSpaceDelta = new Vector2(e.localDelta.X, e.localDelta.Y);
        var rotationRadians = Transform.Rotation * MathF.PI / 180f;
        var rotationMatrix = Matrix3x2.CreateRotation(rotationRadians);
        var localSpaceDelta = Vector2.Transform(parentSpaceDelta, rotationMatrix);

        Translate(localSpaceDelta, Space.Local);
    }

    private void OnRotationHandleClicked(TouchEventData touchData)
    {
        Console.WriteLine($"Rot Clicked at Local: {touchData.LocalPoint}, Parent: {touchData.ParentPoint}, World: {touchData.WorldPoint}");
        RotationHandle.WasClicked = !RotationHandle.WasClicked;
    }


    private void OnRotationHandleDragged((TouchEventData touchData, PointF localDelta) e)
    {
        Console.WriteLine($"Rot Dragged at Local: {e.touchData.LocalPoint}, Parent: {e.touchData.ParentPoint}, World: {e.touchData.WorldPoint}");

        var worldTransform = Scene.GetParentTransform(Id);

        var cursor = new Vector2(e.touchData.WorldPoint.X, e.touchData.WorldPoint.Y);

        var featureCenter = Vector2.Transform(
            new Vector2(Size.Width * 0.5f, Size.Height * 0.5f),
            worldTransform);

        var deltaX = cursor.X - featureCenter.X;
        var deltaY = cursor.Y - featureCenter.Y;

        Console.WriteLine($"{deltaX},{deltaY}");

        var angleRadians = MathF.Atan2(deltaY, deltaX);
        var angleDegrees = angleRadians * (180f / MathF.PI) - 90f;

        Transform.Rotation = angleDegrees;

        Scene.InvalidateTransform(Id);

        DebugCenter = new Vector2(
            e.touchData.ParentPoint.X,
            e.touchData.ParentPoint.Y + HANDLE_OFFSET
        );
    }
    #endregion

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not FeatureNode featureNode) return;

            canvas.FillColor = featureNode.WasClicked ? Colors.Red : Colors.HotPink;
            canvas.FillEllipse(0, 0, node.Size.Width, node.Size.Height);
        }
    }
}