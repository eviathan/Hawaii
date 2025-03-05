using System.Numerics;
using Hawaii.Enums;
using Hawaii.EventData;
using Hawaii.Extensions;
using Hawaii.Interfaces;

namespace Hawaii;

public class EventDispatcher
{
    private const float CLICK_THRESHOLD_IN_MS = 200f;
    
    private const float DRAG_THRESHHOLD_IN_PIXELS = 5f;
    
    private readonly Scene _scene;
    
    private IGestureRecognitionService _gestureRecognitionService;
    
    private PointF? _lastSinglePoint;
    
    private (PointF A, PointF B)? _lastTwoPoints;
    
    private DateTime _singleTouchStartTime;
    
    private Node _draggedNode;
    
    private Node _twoFingerDraggedNode;

    public bool IsDragging() => _draggedNode != null;
    
    public EventDispatcher(Scene scene, IGestureRecognitionService gestureRecognitionService)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _gestureRecognitionService = gestureRecognitionService ?? throw new ArgumentNullException(nameof(gestureRecognitionService));
    }
    
    public void HandleSingleTouchDown(PointF worldPoint)
    {
        _lastSinglePoint = worldPoint;
        _singleTouchStartTime = DateTime.Now;
        PropagateEvent(worldPoint, (node, touchData) => node.OnClicked(touchData));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleSingleTouchMove(PointF worldPoint)
    {
        if (!_lastSinglePoint.HasValue)
            return;
    
        var delta = new PointF(worldPoint.X - _lastSinglePoint.Value.X, worldPoint.Y - _lastSinglePoint.Value.Y);
        var timeElapsed = (DateTime.Now - _singleTouchStartTime).TotalMilliseconds;
    
        if (timeElapsed > CLICK_THRESHOLD_IN_MS || delta.Length() > DRAG_THRESHHOLD_IN_PIXELS)
        {
            if (_draggedNode == null)
            {
                PropagateEvent(worldPoint, (node, touchData) =>
                {
                    // Console.WriteLine($"WorldPoint: ({worldPoint.X}, {worldPoint.Y})");
                    var localDelta = TransformDeltaToLocal(node, delta);
                    if (node.OnDrag(touchData, localDelta))
                    {
                        _draggedNode = node;
                        return true;
                    }
                    return false;
                });
            }
            else
            {
                PointF parentPoint = worldPoint;
                if (_draggedNode.Parent != null)
                {
                    parentPoint = TransformToLocal(_draggedNode.Parent, worldPoint);
                }
                var touchData = new TouchEventData(worldPoint, parentPoint, TransformToLocal(_draggedNode, worldPoint));
                var localDelta = TransformDeltaToLocal(_draggedNode, delta);
            
                _draggedNode.OnDrag(touchData, localDelta);
            }
        
            _lastSinglePoint = worldPoint;
            _scene.InvalidateView?.Invoke();
        }
    }

    public void HandleSingleTouchUp(PointF worldPoint)
    {
        if (_lastSinglePoint.HasValue)
        {
            var timeElapsed = (DateTime.Now - _singleTouchStartTime).TotalMilliseconds;
            var delta = new PointF(worldPoint.X - _lastSinglePoint.Value.X, worldPoint.Y - _lastSinglePoint.Value.Y);

            if (timeElapsed <= CLICK_THRESHOLD_IN_MS && delta.Length() <= DRAG_THRESHHOLD_IN_PIXELS)
                PropagateEvent(worldPoint, (node, touchData) => node.OnClicked(touchData));
            
            PropagateEvent(worldPoint, (node, touchData) =>
                node.OnTouchUp(touchData));
        }
        
        _lastSinglePoint = null;
        _draggedNode = null;
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    {
        _gestureRecognitionService.AddFrame(pointA, pointB);
        
        if (_gestureRecognitionService.TryDetectPan(out var delta))
            PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerDrag(gestureData));
        if (_gestureRecognitionService.TryDetectPinch(out var scaleFactor))
            PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnPinch(gestureData));
        if (_gestureRecognitionService.TryDetectRotation(out var angle))
            PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnRotate(gestureData));
        
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = (pointA, pointB);
        _gestureRecognitionService.AddFrame(pointA, pointB);
        
        PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerClicked(gestureData));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerUp(PointF pointA, PointF pointB)
    {
        if (_lastTwoPoints.HasValue)
        {
            PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerTouchUp(gestureData));
        }
        
        _lastTwoPoints = null;
        _scene.InvalidateView?.Invoke();
    }

    private void PropagateEvent(PointF worldPoint, Func<Node, TouchEventData, bool> handler)
    {
        var orderedNodes = GetNodesInHitTestOrder(_scene.RootNode);

        foreach (var node in orderedNodes)
        {
            var localPoint = TransformToLocal(node, worldPoint);
            if (node.ContainsLocalPoint(localPoint))
            {
                PointF parentPoint = worldPoint;

                if (node.Parent != null)
                {
                    parentPoint = TransformToLocal(node.Parent, worldPoint);
                }

                var touchData = new TouchEventData(worldPoint, parentPoint, localPoint);
                if (handler(node, touchData))
                    break;
            }
        }
    }

    private void PropagateTwoFingerEvent(PointF pointA, PointF pointB, Func<Node, GestureEventData, bool> handler)
    {
        var orderedNodes = GetNodesInHitTestOrder(_scene.RootNode);
    
        foreach (var node in orderedNodes)
        {
            var localA = TransformToLocal(node, pointA);
            var localB = TransformToLocal(node, pointB);
        
            if (node.ContainsLocalPoint(localA) || node.ContainsLocalPoint(localB))
            {
                PointF parentPointA = pointA;
                PointF parentPointB = pointB;
                if (node.Parent != null)
                {
                    var parent = node.Parent;
                    parentPointA = TransformToLocal(parent, pointA);
                    parentPointB = TransformToLocal(parent, pointB);
                }
                var gestureData = new GestureEventData(
                    new TouchEventData(pointA, parentPointA, localA),
                    new TouchEventData(pointB, parentPointB, localB));
            
                if (handler(node, gestureData))
                    break;
            }
        }
    }

    private IEnumerable<Node> GetNodesInHitTestOrder(Node root)
    {
        var stack = new Stack<(Node node, bool processedChildren)>();
        stack.Push((root, false));

        while (stack.Count > 0)
        {
            var (node, processedChildren) = stack.Pop();

            if (!processedChildren)
            {
                stack.Push((node, true));
                
                var children = node.Children;
                children.Reverse();
                
                foreach (var child in children)
                {
                    stack.Push((child, false));
                }
            }
            else
            {
                yield return node;
            }
        }
    }

    // TODO: Maybe move these outside of this class
    private PointF TransformToLocal(Node node, PointF worldPoint)
    {
        // Get the node's world transform
        var worldTransform = _scene.GetParentTransform(node.Id);
    
        // Invert it to map world -> local
        if (!Matrix3x2.Invert(worldTransform, out var inverse))
            return worldPoint;

        // Transform the world point to the node's local space
        var localPoint = Vector2.Transform(new Vector2(worldPoint.X, worldPoint.Y), inverse);

        // Adjust for the node's anchor (Center) - shift local origin to match anchor
        Vector2 anchorOffset = node.Center switch
        {
            Anchor.TopLeft => Vector2.Zero,
            Anchor.TopCenter => new Vector2(node.Size.Width / 2, 0),
            Anchor.TopRight => new Vector2(node.Size.Width, 0),
            Anchor.CenterLeft => new Vector2(0, node.Size.Height / 2),
            Anchor.Center => new Vector2(node.Size.Width / 2, node.Size.Height / 2),
            Anchor.CenterRight => new Vector2(node.Size.Width, node.Size.Height / 2),
            Anchor.BottomLeft => new Vector2(0, node.Size.Height),
            Anchor.BottomCenter => new Vector2(node.Size.Width / 2, node.Size.Height),
            Anchor.BottomRight => new Vector2(node.Size.Width, node.Size.Height),
            _ => Vector2.Zero
        };

        // Subtract anchor offset to align local (0,0) with the anchor point
        localPoint -= anchorOffset;

        return new PointF(localPoint.X, localPoint.Y);
    }
    
    private PointF TransformDeltaToLocal(Node node, PointF worldDelta)
    {
        if (Matrix3x2.Invert(_scene.GetParentTransform(node.Id), out var inverse))
        {
            var deltaVec = Vector2.Transform(new Vector2(worldDelta.X, worldDelta.Y), inverse);
            var originVec = Vector2.Transform(Vector2.Zero, inverse);
            return new PointF(deltaVec.X - originVec.X, deltaVec.Y - originVec.Y);
        }
        return worldDelta;
    }
}