using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class ImageNode : Node
{    
    public ImageNode(Scene scene) : base(scene)
    {
        Size = new SizeF(1000, 1000);
        Origin = Origin.TopLeft;
        Alignment = Alignment.Center;
        Renderer = new NodeRenderer();
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        return true;
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