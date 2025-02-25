namespace Hawaii.Interfaces;

public interface INodeRenderer
{
    void Draw(ICanvas canvas, Node node, RectF dirtyRect);
}