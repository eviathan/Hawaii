using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii.Nodes
{
    public class CanvasNode : Node
    {
        public bool IsZoomed { get; set; }

        public CanvasNode(Scene scene) : base(scene)
        {
            Alignment = Alignment.Center;
            Size = new SizeF(float.MaxValue, float.MaxValue);
            Renderer = new NodeRenderer();
            IgnoreAncestorScale = false;
        }

        public override bool OnClicked(TouchEventData touchData)
        {
            IsZoomed = !IsZoomed;
            var transform = Scene.GetTransform(Id);
            transform.Scale = IsZoomed ? new Vector2(1.0f, 1.0f) : new Vector2(2.0f, 2.0f);
            Scene.SetTransform(Id, transform);
            return true;
        }

        private class NodeRenderer : INodeRenderer
        {
            private const float CHECKER_SIZE = 25f;

            public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
            {
                canvas.Alpha = 0.6f;
                canvas.FillColor = Color.FromArgb("#37424A");
                canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

                var transform = node.Scene.GetTransform(node.Id);
                float scale = transform.Scale.X;
                float tileSize = CHECKER_SIZE * scale;

                // Use viewport (dirtyRect) bounds in screen space, transform to local
                var inverseTransform = Matrix3x2.Invert(node.Scene.GetWorldTransform(node.Id), out var inv) ? inv : Matrix3x2.Identity;
                var topLeftLocal = Vector2.Transform(new Vector2(dirtyRect.Left, dirtyRect.Top), inverseTransform);
                var bottomRightLocal = Vector2.Transform(new Vector2(dirtyRect.Right, dirtyRect.Bottom), inverseTransform);

                // Calculate tile grid in local space
                int startX = (int)Math.Floor(topLeftLocal.X / tileSize);
                int endX = (int)Math.Ceiling(bottomRightLocal.X / tileSize);
                int startY = (int)Math.Floor(topLeftLocal.Y / tileSize);
                int endY = (int)Math.Ceiling(bottomRightLocal.Y / tileSize);

                // Draw tiles covering entire viewport
                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        bool isWhite = (x + y) % 2 == 0;
                        canvas.FillColor = isWhite ? Colors.White : Colors.LightGray;
                        float localX = x * tileSize;
                        float localY = y * tileSize;
                        canvas.FillRectangle(localX, localY, tileSize, tileSize);
                    }
                }
            }
        }
    }
}