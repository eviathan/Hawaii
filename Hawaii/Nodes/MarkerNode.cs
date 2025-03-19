using System.Numerics;
using Hawaii.Enums;

namespace Hawaii.Nodes
{
    public abstract class MarkerNode : Node
    {
        protected MarkerNode(Scene scene) : base(scene) { }

        public override bool ContainsLocalPoint(PointF localPoint)
        {
            // Convert local touch point to world space
            var worldTransform = Scene.GetWorldTransform(Id);
            var worldTouchPoint = Vector2.Transform(new Vector2(localPoint.X, localPoint.Y), worldTransform);

            // Convert touch point to screen space
            var screenTouchPoint = Scene.Camera.WorldToScreen(worldTouchPoint);

            // Get the node's world position (local origin in world space)
            var nodeWorldPos = Vector2.Transform(Vector2.Zero, worldTransform);

            // Convert node position to screen space
            var nodeScreenPos = Scene.Camera.WorldToScreen(nodeWorldPos);

            // Define fixed screen-space bounds (e.g., 100x100 pixels)
            var screenSize = new SizeF(Size.Width, Size.Height);
            var screenBounds = new RectF(0, 0, screenSize.Width, screenSize.Height);

            // Adjust for Origin in screen space
            Vector2 screenOriginOffset = Origin switch
            {
                Origin.TopLeft => Vector2.Zero,
                Origin.Center => new Vector2(screenSize.Width / 2, screenSize.Height / 2),
                Origin.TopCenter => new Vector2(screenSize.Width / 2, 0),
                Origin.TopRight => new Vector2(screenSize.Width, 0),
                Origin.CenterLeft => new Vector2(0, screenSize.Height / 2),
                Origin.CenterRight => new Vector2(screenSize.Width, screenSize.Height / 2),
                Origin.BottomLeft => new Vector2(0, screenSize.Height),
                Origin.BottomCenter => new Vector2(screenSize.Width / 2, screenSize.Height),
                Origin.BottomRight => new Vector2(screenSize.Width, screenSize.Height),
                _ => Vector2.Zero
            };

            // Compute the node's effective screen-space origin (top-left of bounds)
            var screenBoundsOrigin = nodeScreenPos - screenOriginOffset;

            // Convert touch point to be relative to the node's screen-space bounds
            var relativeScreenPoint = new PointF(
                screenTouchPoint.X - screenBoundsOrigin.X,
                screenTouchPoint.Y - screenBoundsOrigin.Y
            );

            // Test if the relative point is within the fixed screen-space bounds
            return screenBounds.Contains(relativeScreenPoint);
        }
    }
}