using System.Numerics;

namespace Hawaii
{
    public class SceneCamera
    {
        public Transform Transform { get; set; } = new Transform();

        public SizeF ViewportSize { get; set; }

        // World to Screen transformation
        public Vector2 WorldToScreen(Vector2 worldPoint)
        {
            // 1. Relative to camera position
            var relative = worldPoint - Transform.Position;
            // 2. Apply scale
            var scaled = relative * Transform.Scale;
            // 3. Offset by viewport center
            var screen = scaled + new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);

            return screen;
        }

        // Screen to World transformation
        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            // 1. Remove viewport center offset
            var centered = screenPoint - new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            // 2. Reverse scale
            var unscaled = centered / Transform.Scale;
            // 3. Add camera position
            var world = unscaled + Transform.Position;

            return world;
        }

        // Get the transformation matrix for rendering
        public Matrix3x2 GetViewMatrix()
        {
            return Matrix3x2.CreateTranslation(-Transform.Position) *
                   Matrix3x2.CreateScale(Transform.Scale) *
                   Matrix3x2.CreateTranslation(new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2));
        }

        // Update viewport size if it changes
        public void UpdateViewportSize(SizeF newSize)
        {
            ViewportSize = newSize;
        }

        private bool _isZoomed;

        public void ToggleZoom()
        {
            Transform.Scale = _isZoomed ? Vector2.One: new Vector2(2.0f, 2.0f);
            _isZoomed = !_isZoomed;
        }
    }
}
