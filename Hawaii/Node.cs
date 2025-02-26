using Hawaii.Enums;
using Hawaii.Interfaces;

namespace Hawaii;

public class Node
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public INodeState State { get; set; }
    
    public bool PropagateScale { get; set; }
    
    public SizeF Size { get; set; }
    
    public Transform Transform { get; set; } = new();

    public Anchor Center { get; set; } = Anchor.TopLeft;

    public Alignment Alignment { get; set; } = Alignment.None;

    public PositionMode Position { get; set; } = PositionMode.Relative;

    public List<Node> Children { get; set; } = [];

    public INodeRenderer Renderer { get; set; }
    
    public virtual RectF GetLocalBounds() => new RectF(0f, 0f, Size.Width, Size.Height);

    public virtual bool OnClicked(PointF worldPoint) => false;

    public virtual bool OnDrag(PointF worldPoint, PointF delta) => false;
    
    public virtual bool OnTwoFingerClicked(PointF pointA, PointF pointB) => false;

    public virtual bool OnTwoFingerDrag(PointF pointA, PointF pointB, PointF delta) => false;

    public virtual bool OnPinch(PointF pointA, PointF pointB, float scaleFactor) => false;

    public virtual bool OnRotate(PointF pointA, PointF pointB, float angle) => false;

    public void AddChild(Node child) =>
        Children.Add(child);
} 