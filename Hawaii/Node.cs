using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii;

public class Node
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public INodeState State { get; set; }
    
    public SizeF Size { get; set; }
    
    public Transform Transform { get; set; } = new();

    public Origin Origin { get; set; } = Origin.TopLeft;

    public Alignment Alignment { get; set; }

    public PositionMode Position { get; set; } = PositionMode.Relative;

    public Node Parent { get; set; }

    public List<Node> Children { get; set; } = [];

    public INodeRenderer Renderer { get; set; }
    
    public Scene Scene { get; }

    public Node(Scene scene)
    {
        Scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public virtual void Initialise() { }


    public virtual void Translate(Vector2 delta, Space space)
    {        
        if (space == Space.Local)
        {
            Transform.Position += delta;
        }
        else
        {
            var worldTransform = Scene.GetWorldTransform(Id);
            var currentWorldPos = Vector2.Transform(Vector2.Zero, worldTransform);
            var newWorldPos = currentWorldPos + delta;
            
            var parentTransform = Parent != null
                ? Scene.GetWorldTransform(Parent.Id)
                : Matrix3x2.Identity;
            
            if (Matrix3x2.Invert(parentTransform, out var inverseParent))
            {
                Transform.Position = Vector2.Transform(newWorldPos, inverseParent);
            }
        }
        
        Scene.InvalidateTransform(Id);
    }
    

    public void AddChild(Node child)
    {
        child.Parent = this;
        Children.Add(child);
        Scene.Nodes[child.Id] = child;
        Scene.InvalidateTransform(child.Id);
    }
    
    public virtual RectF GetLocalBounds()
    {
        return new RectF(0f, 0f, Size.Width, Size.Height);
    }

    public virtual Vector2 GetOriginOffset()
    {
        if (Size.Width == float.MaxValue || Size.Height == float.MaxValue)
            return Origin == Origin.Center ? Vector2.Zero : GetFiniteOriginOffset();

        return GetFiniteOriginOffset();
    }

    private Vector2 GetFiniteOriginOffset()
    {
        var halfWidth = Size.Width / 2;
        var halfHeight = Size.Height / 2;
        return Origin switch
        {
            Origin.TopLeft => Vector2.Zero,
            Origin.TopCenter => new Vector2(halfWidth, 0),
            Origin.TopRight => new Vector2(Size.Width, 0),
            Origin.CenterLeft => new Vector2(0, halfHeight),
            Origin.Center => new Vector2(halfWidth, halfHeight),
            Origin.CenterRight => new Vector2(Size.Width, halfHeight),
            Origin.BottomLeft => new Vector2(0, Size.Height),
            Origin.BottomCenter => new Vector2(halfWidth, Size.Height),
            Origin.BottomRight => new Vector2(Size.Width, Size.Height),
            _ => Vector2.Zero
        };
    }

    public virtual Vector2 GetAlignmentOffset()
    {
        if (Size.Width == float.MaxValue || Size.Height == float.MaxValue)
            return Vector2.Zero;

        var halfWidth = Size.Width / 2;
        var halfHeight = Size.Height / 2;
        var originPoint = GetOriginOffset();

        // Default to TopLeft with no offset unless explicitly aligning elsewhere
        if (Alignment == Alignment.TopLeft)
            return Vector2.Zero;

        Vector2 alignmentPoint = Alignment switch
        {
            Alignment.Center => new Vector2(halfWidth, halfHeight),
            Alignment.TopRight => new Vector2(Size.Width, 0),
            Alignment.BottomLeft => new Vector2(0, Size.Height),
            Alignment.BottomRight => new Vector2(Size.Width, Size.Height),
            _ => Vector2.Zero
        };

        return -(alignmentPoint - originPoint);
    }

    public virtual bool ContainsLocalPoint(PointF localPoint)
    {
        var bounds = GetLocalBounds();
        Vector2 anchorOffset = Origin switch
        {
            Origin.TopLeft => Vector2.Zero,
            Origin.TopCenter => new Vector2(Size.Width / 2, 0),
            Origin.TopRight => new Vector2(Size.Width, 0),
            Origin.CenterLeft => new Vector2(0, Size.Height / 2),
            Origin.Center => new Vector2(Size.Width / 2, Size.Height / 2),
            Origin.CenterRight => new Vector2(Size.Width, Size.Height / 2),
            Origin.BottomLeft => new Vector2(0, Size.Height),
            Origin.BottomCenter => new Vector2(Size.Width / 2, Size.Height),
            Origin.BottomRight => new Vector2(Size.Width, Size.Height),
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