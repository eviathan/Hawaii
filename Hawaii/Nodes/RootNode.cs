using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;
using System.Numerics;

namespace Hawaii.Nodes;

public class RootNode : Node
{
    public SceneCamera Camera { get; }

    private DateTime? _lastClicked;

    public RootNode(Scene scene, SceneCamera camera) : base(scene)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
        Size = new SizeF(float.MaxValue, float.MaxValue);
        Renderer = new DefaultRenderer();
        Origin = Origin.Center;
        Alignment = Alignment.Center;
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        var now = DateTime.UtcNow;
    
        if (_lastClicked.HasValue)
        {
            var elapsed = (now - _lastClicked.Value).TotalMilliseconds;
            
            if (elapsed < 500)
            {
                var screenFocalPoint = Camera.WorldToScreen(new Vector2(touchData.WorldPoint.X, touchData.WorldPoint.Y));
                Camera.ToggleZoom(screenFocalPoint);
                _lastClicked = null;
                Scene.InvalidateView?.Invoke();
                return true;
            }
            else if (elapsed > 1000)
            {
                _lastClicked = null;
            }
        }
    
        _lastClicked = now;
        return false;
    }
    
    public override bool OnDrag(TouchEventData touchData, PointF localDelta)
    {
        var delta = new Vector2(localDelta.X, localDelta.Y) / Camera.Transform.Scale;
        Camera.Transform.Position -= delta;
        Scene.InvalidateView?.Invoke();
        return true;
    }
    
    private class DefaultRenderer : INodeRenderer
    {
        private const float CHECKER_SIZE = 50f;

        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            //     if (node is not RootNode rootNode)
            //         return;
            //
            //     canvas.Alpha = 0.6f;
            //     canvas.FillColor = Color.FromArgb("#37424A");
            //     canvas.FillRectangle(-float.MaxValue / 2, -float.MaxValue / 2, float.MaxValue, float.MaxValue);
            //
            //     var topLeftWorld = rootNode.Camera.ScreenToWorld(new Vector2(dirtyRect.Left, dirtyRect.Top));
            //     var bottomRightWorld = rootNode.Camera.ScreenToWorld(new Vector2(dirtyRect.Right, dirtyRect.Bottom));
            //
            //     var inverseTransform = Matrix3x2.Invert(node.Scene.GetWorldTransform(node.Id), out var inv) ? inv : Matrix3x2.Identity;
            //     var topLeftLocal = Vector2.Transform(topLeftWorld, inverseTransform);
            //     var bottomRightLocal = Vector2.Transform(bottomRightWorld, inverseTransform);
            //
            //     var startX = (int)Math.Floor(topLeftLocal.X / CHECKER_SIZE);
            //     var endX = (int)Math.Ceiling(bottomRightLocal.X / CHECKER_SIZE);
            //     var startY = (int)Math.Floor(topLeftLocal.Y / CHECKER_SIZE);
            //     var endY = (int)Math.Ceiling(bottomRightLocal.Y / CHECKER_SIZE);
            //
            //     for (var x = startX; x < endX; x++)
            //     {
            //         for (var y = startY; y < endY; y++)
            //         {
            //             var isWhite = (x + y) % 2 == 0;
            //             canvas.FillColor = isWhite ? Colors.White : Colors.LightGray;
            //             var localX = x * CHECKER_SIZE;
            //             var localY = y * CHECKER_SIZE;
            //             canvas.FillRectangle(localX, localY, CHECKER_SIZE, CHECKER_SIZE);
            //         }
            //     }
        }
    }

    public override Vector2 GetOriginOffset()
    {
        return Origin == Origin.Center ? Vector2.Zero : base.GetOriginOffset();
    }
}