using System.Numerics;

namespace Hawaii
{
    public class SceneCamera
    {
        private bool _isZoomed;

        private float _zoom;
        
        public Transform Transform { get; set; } = new Transform();

        public SizeF ViewportSize { get; set; }

        public Vector2 WorldToScreen(Vector2 worldPoint)
        {
            var relative = worldPoint - Transform.Position;
            var scaled = relative * Transform.Scale;
            var screen = scaled + new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);

            return screen;
        }

        public void ApplyTransform(ICanvas canvas, RectF dirtyRect)
        {
            ViewportSize = new SizeF(dirtyRect.Width, dirtyRect.Height);
            canvas.ConcatenateTransform(GetViewMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            var centered = screenPoint - new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var unscaled = centered / Transform.Scale;
            var world = unscaled + Transform.Position;

            return world;
        }

        public Matrix3x2 GetViewMatrix()
        {
            return Matrix3x2.CreateTranslation(-Transform.Position) *
                   Matrix3x2.CreateScale(Transform.Scale) *
                   Matrix3x2.CreateTranslation(new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2));
        }
        
        public void ToggleZoom(Vector2 screenFocalPoint)
        {
            // Step 1: Calculate the world-space point under the click before zooming
            Vector2 worldFocal = ScreenToWorld(screenFocalPoint);

            // Step 2: Update the zoom level (increment by 0.1, wrap at 4)
            _zoom = (_zoom + 0.1f) % 4f;
            Transform.Scale = new Vector2(1f + _zoom, 1f + _zoom); // Uniform scaling

            // Step 3: Adjust Position to keep the focal point stationary
            // - After scaling, the world point should map back to the same screen point
            // - New position = worldFocal - (screen offset from center / new scale)
            Vector2 viewportCenter = new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            Vector2 screenOffset = screenFocalPoint - viewportCenter;
            Transform.Position = worldFocal - screenOffset / Transform.Scale;
        }
    }
}
