using Hawaii.Enums;
using Hawaii.EventData;
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

    public virtual bool OnClicked(TouchEventData touchData) => false;
    public virtual bool OnDrag(TouchEventData touchData, PointF localDelta) => false;
    public virtual bool OnTwoFingerClicked(GestureEventData gestureData) => false;
    public virtual bool OnTwoFingerDrag(GestureEventData gestureData) => false;
    public virtual bool OnPinch(GestureEventData gestureData) => false;
    public virtual bool OnRotate(GestureEventData gestureData) => false;

    public void AddChild(Node child) =>
        Children.Add(child);
} 