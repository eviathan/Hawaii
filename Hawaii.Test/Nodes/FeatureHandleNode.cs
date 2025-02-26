using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii.Test.Nodes;

public class FeatureHandleNode : Node
{
    public FeatureHandleNode()
    {
        Renderer = new NodeRenderer();
        Center = Anchor.Center;
        Size = new SizeF(50, 50);
    }

    public override bool OnClicked(TouchEventData touchData)
    {
        Console.WriteLine($"Clicked at Local: {touchData.LocalPoint}, World: {touchData.WorldPoint}");
        return true;
    }

    private class NodeRenderer : INodeRenderer
    {
        public void Draw(ICanvas canvas, Node node, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Blue;
            canvas.FillEllipse(0f, 0f, node.Size.Width, node.Size.Height);
        }
    }
}