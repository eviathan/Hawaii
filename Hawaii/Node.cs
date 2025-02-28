using System.Numerics;
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
    

    public void AddChild(Node child)
    {
        Children.Add(child);
    }
    
    public virtual RectF GetLocalBounds()
    {
        return new RectF(0f, 0f, Size.Width, Size.Height);
    }
    
    public virtual bool ContainsLocalPoint(PointF localPoint)
    {
        var bounds = GetLocalBounds();
        Vector2 anchorOffset = Center switch
        {
            Anchor.TopLeft => Vector2.Zero,
            Anchor.TopCenter => new Vector2(Size.Width / 2, 0),
            Anchor.TopRight => new Vector2(Size.Width, 0),
            Anchor.CenterLeft => new Vector2(0, Size.Height / 2),
            Anchor.Center => new Vector2(Size.Width / 2, Size.Height / 2),
            Anchor.CenterRight => new Vector2(Size.Width, Size.Height / 2),
            Anchor.BottomLeft => new Vector2(0, Size.Height),
            Anchor.BottomCenter => new Vector2(Size.Width / 2, Size.Height),
            Anchor.BottomRight => new Vector2(Size.Width, Size.Height),
            _ => Vector2.Zero
        };
        
        var adjustedPoint = new PointF(localPoint.X + anchorOffset.X, localPoint.Y + anchorOffset.Y);
        
        return bounds.Contains(adjustedPoint);
    }
    
    #region Events
    public virtual bool OnClicked(TouchEventData touchData) => false;
    public virtual bool OnDrag(TouchEventData touchData, PointF localDelta) => false;
    public virtual bool OnTouchUp(TouchEventData touchData) => false;
    public virtual bool OnTwoFingerClicked(GestureEventData gestureData) => false;
    public virtual bool OnTwoFingerDrag(GestureEventData gestureData) => false;
    public virtual bool OnPinch(GestureEventData gestureData) => false;
    public virtual bool OnRotate(GestureEventData gestureData) => false;
    public virtual bool OnTwoFingerTouchUp(GestureEventData gestureData) => false;
    #endregion
} 