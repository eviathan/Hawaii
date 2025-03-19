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
            var relativePoint = worldPoint - Transform.Position;
            var scaledPoint = relativePoint * Transform.Scale;
            var screenPoint = scaledPoint + new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);

            return screenPoint;
        }

        public void ApplyTransform(ICanvas canvas, RectF dirtyRect)
        {
            ViewportSize = new SizeF(dirtyRect.Width, dirtyRect.Height);
            canvas.ConcatenateTransform(GetViewMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            var centeredPoint = screenPoint - new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var unscaledPoint = centeredPoint / Transform.Scale;
            var worldPoint = unscaledPoint + Transform.Position;

            return worldPoint;
        }

        public Matrix3x2 GetViewMatrix()
        {
            return Matrix3x2.CreateTranslation(-Transform.Position) *
                   Matrix3x2.CreateScale(Transform.Scale) *
                   Matrix3x2.CreateTranslation(new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2));
        }
        
        public void ToggleZoom(Vector2 screenFocalPoint)
        {
            var worldFocal = ScreenToWorld(screenFocalPoint);

            _zoom = (_zoom + 1f) % 4f;
            Transform.Scale = new Vector2(1f + _zoom, 1f + _zoom);

            var viewportCenter = new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var screenOffset = screenFocalPoint - viewportCenter;
            Transform.Position = worldFocal - screenOffset / Transform.Scale;
        }
    }
}
