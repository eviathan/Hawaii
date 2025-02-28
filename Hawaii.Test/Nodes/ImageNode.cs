using System.Numerics;
using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class ImageNode : Node
{
    public Scene Scene { get; set; }
    
    public ImageNode(Scene scene) : base(scene)
    {
        PropagateScale = true;
        Renderer = new NodeRenderer();
        Position = PositionMode.Absolute;
        Center = Anchor.TopLeft;
        Alignment = Alignment.Center;
        Size = new SizeF(640, 400);
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not ImageNode imageNode) return;
            
            canvas.FillColor = Colors.Coral;
            canvas.FillRectangle(0, 0, node.Size.Width, node.Size.Height);

            var scene = imageNode.Scene;
            var worldBounds = scene.GetWorldBounds(node.Id);
            var worldTransform = scene.GetWorldTransform(node.Id);

            // Invert world transform to map world-to-local
            if (Matrix3x2.Invert(worldTransform, out var localTransform))
            {
                // Transform worldBounds corners to local space
                var topLeft = Vector2.Transform(new Vector2(worldBounds.Left, worldBounds.Top), localTransform);
                var bottomRight = Vector2.Transform(new Vector2(worldBounds.Right, worldBounds.Bottom), localTransform);

                // Calculate local bounds
                float localX = topLeft.X;
                float localY = topLeft.Y;
                float localWidth = bottomRight.X - topLeft.X;
                float localHeight = bottomRight.Y - topLeft.Y;

                canvas.StrokeColor = Colors.Blue;
                canvas.StrokeSize = 2f;
                canvas.DrawRectangle(localX, localY, localWidth, localHeight);

                // Console.WriteLine($"WorldBounds={worldBounds}, LocalBounds=({localX}, {localY}, {localWidth}, {localHeight})");
            }
        }
    }
}