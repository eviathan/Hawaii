using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;
using System.Numerics;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Hawaii.Nodes;

public class RootNode : Node
{
    public SceneCamera Camera { get; }

    private DateTime? _lastClicked;

    public RootNode(Scene scene, SceneCamera camera) : base(scene)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
        Origin = Origin.Center;
        Size = new SizeF(float.MaxValue, float.MaxValue);
        Renderer = new DefaultRenderer();
        //IgnoreAncestorScale = false;
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        var now = DateTime.UtcNow;

        if (_lastClicked.HasValue)
        {
            var elapsed = (now - _lastClicked.Value).TotalMilliseconds;
            if (elapsed < 500) // Double-click threshold
            {
                Camera.ToggleZoom();
                _lastClicked = null;
                Scene.InvalidateView?.Invoke();
                return true;
            }
            else if (elapsed > 1000) // Clear stale click
            {
                _lastClicked = null;
            }
        }

        _lastClicked = now;
        return false; // Single click, not consumed
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
        private const float CHECKER_SIZE = 25f;

        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if(node is not RootNode rootNode)
                return;

            // Fill the entire infinite plane with a background color
            canvas.Alpha = 0.6f;
            canvas.FillColor = Color.FromArgb("#37424A");
            canvas.FillRectangle(-float.MaxValue / 2, -float.MaxValue / 2, float.MaxValue, float.MaxValue);

            // Get the camera's scale and position from the scene
            var tileSize = rootNode.Camera.Transform.Scale * CHECKER_SIZE;

            // Convert viewport corners to world space
            var topLeftWorld = rootNode.Camera.ScreenToWorld(new Vector2(dirtyRect.Left, dirtyRect.Top));
            var bottomRightWorld = rootNode.Camera.ScreenToWorld(new Vector2(dirtyRect.Right, dirtyRect.Bottom));

            // Transform world coordinates to local space of the RootNode
            var inverseTransform = Matrix3x2.Invert(node.Scene.GetWorldTransform(node.Id), out var inv)
                ? inv : Matrix3x2.Identity;
            var topLeftLocal = Vector2.Transform(topLeftWorld, inverseTransform);
            var bottomRightLocal = Vector2.Transform(bottomRightWorld, inverseTransform);

            // Calculate tile grid bounds in local space
            int startX = (int)Math.Floor(topLeftLocal.X / tileSize.X);
            int endX = (int)Math.Ceiling(bottomRightLocal.X / tileSize.X);
            int startY = (int)Math.Floor(topLeftLocal.Y / tileSize.Y);
            int endY = (int)Math.Ceiling(bottomRightLocal.Y / tileSize.Y);

            // Draw checkerboard across the visible area
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    bool isWhite = (x + y) % 2 == 0;
                    canvas.FillColor = isWhite ? Colors.White : Colors.LightGray;
                    float localX = x * tileSize.X;
                    float localY = y * tileSize.Y;
                    canvas.FillRectangle(localX, localY, tileSize.X, tileSize.Y);
                }
            }
        }
    }

    public override Vector2 GetOriginOffset()
    {
        return Origin == Origin.Center ? Vector2.Zero : base.GetOriginOffset(); // Simplified for infinite size
    }
}