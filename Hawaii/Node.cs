using System.Numerics;
using System.Xml.Linq;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Interfaces;

namespace Hawaii;

public class Node
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public INodeState State { get; set; }
    
    public bool IgnoreAncestorScale { get; set; }
    
    public SizeF Size { get; set; }
    
    public Transform Transform { get; set; } = new();

    public Anchor Center { get; set; } = Anchor.TopLeft;

    public Alignment Alignment { get; set; } = Alignment.None;

    public PositionMode Position { get; set; } = PositionMode.Relative;

    public List<Node> Children { get; set; } = [];

    public INodeRenderer Renderer { get; set; }
    
    public Scene Scene { get; }

    public Node(Scene scene)
    {
        Scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }
    
    public virtual void Translate(Vector2 delta, Space space)
    {
        var transform = Scene.GetTransform(Id);
        
        if (space == Space.Local)
        {
            transform.Position += delta;
        }
        else
        {
            var worldTransform = Scene.GetWorldTransform(Id);
            var currentWorldPos = Vector2.Transform(Vector2.Zero, worldTransform);
            var newWorldPos = currentWorldPos + delta;
            
            var parentTransform = Scene.HierarchyMap[Id].HasValue
                ? Scene.GetWorldTransform(Scene.HierarchyMap[Id].Value)
                : Matrix3x2.Identity;
            
            if (Matrix3x2.Invert(parentTransform, out var inverseParent))
            {
                transform.Position = Vector2.Transform(newWorldPos, inverseParent);
            }
        }
        
        Scene.SetTransform(Id, transform);
    }
    

    public void AddChild(Node child)
    {
        Children.Add(child);
        Scene.HierarchyMap[child.Id] = Id;
        Scene.AddNode(child, Id);
        Scene.MarkDirty(child.Id);
        Scene.SetTransform(child.Id, child.Transform);
    }
    
    public virtual RectF GetLocalBounds()
    {
        return new RectF(0f, 0f, Size.Width, Size.Height);
    }

    public Vector2 GetCenterOffset()
    {
        return Center switch
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