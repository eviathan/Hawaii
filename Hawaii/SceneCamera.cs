using System.Diagnostics;
using System.Numerics;

namespace Hawaii
{
    public class SceneCamera
    {
        public float Zoom { get; set; } = 1.0f;
        
        public Transform Transform { get; set; } = new();

        public SizeF ViewportSize { get; set; }

        public event Action DidZoom;

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
            
            canvas.Scale(Transform.Scale.X, Transform.Scale.Y);
            canvas.Translate(dirtyRect.Width / 2  - Transform.Position.X, dirtyRect.Height / 2 - Transform.Position.Y);
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            var centeredPoint = screenPoint - new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var unscaledPoint = centeredPoint / Transform.Scale;
            var worldPoint = unscaledPoint + Transform.Position;

            return worldPoint;
        }

        public Vector2 ScreenDistanceToWorld(Vector2 screenDistance)
        {
            return new Vector2(screenDistance.X / Transform.Scale.X, screenDistance.Y / Transform.Scale.Y);
        }

        public void ToggleZoom(Vector2 screenFocalPoint)
        {
            var worldFocal = ScreenToWorld(screenFocalPoint);

            Zoom = (Zoom + 1f) % 4f;
            Transform.Scale = new Vector2(1f + Zoom, 1f + Zoom);

            var viewportCenter = new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var screenOffset = screenFocalPoint - viewportCenter;
            Transform.Position = worldFocal - screenOffset / Transform.Scale;

            DidZoom?.Invoke();
        }
    }
}
