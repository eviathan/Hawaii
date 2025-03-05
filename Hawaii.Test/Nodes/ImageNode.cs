using System.Numerics;
using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class ImageNode : Node
{    
    public ImageNode(Scene scene) : base(scene)
    {
        Renderer = new NodeRenderer();
        Size = new SizeF(600, 400);
        Transform.Position = new Vector2(100, 100);
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            if (node is not ImageNode imageNode)
                return;

            canvas.Alpha = .6f;
            canvas.FillColor = Colors.Coral;
            canvas.FillRectangle(0, 0, node.Size.Width, node.Size.Height);

            canvas.StrokeDashPattern = [4f, 2f]; 
            canvas.StrokeSize = 4;
            canvas.StrokeColor = Colors.DarkBlue;
            canvas.DrawRectangle(0, 0, node.Size.Width, node.Size.Height);
        }
    }
}