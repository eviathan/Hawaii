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
        
        public SceneCamera()
        {
            Zoom = 1.0f;
            Transform = new Transform
            {
                Position = new Vector2(0.0f, 240.0f),
                Scale = Vector2.One
            };
        }

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
            Debug.WriteLine($"ViewportSize: {ViewportSize}, dirtyRect: {dirtyRect}, Transform.Position: {Transform.Position}");
            // canvas.ConcatenateTransform(GetViewMatrix());
            canvas.Translate(dirtyRect.Width / 2, dirtyRect.Height / 2);
        }
        
        public Matrix3x2 GetViewMatrix()
        {
            var center = new Vector2(ViewportSize.Width / 2, ViewportSize.Height / 2);
            var expectedCenter = new Vector2(300, 468);
            var observedOffset = new Vector2(60, 40);
            var correction = expectedCenter - observedOffset;
            
            return Matrix3x2.CreateTranslation(-Transform.Position)
                   * Matrix3x2.CreateScale(Transform.Scale)
                   * Matrix3x2.CreateTranslation(center + correction);
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
