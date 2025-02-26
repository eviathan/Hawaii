using System.Numerics;
using Hawaii.EventData;
using Hawaii.Extensions;

namespace Hawaii;

public class EventDispatcher
{
    private const float CLICK_THRESHOLD_IN_MS = 200f;
    
    private const float DRAG_THRESHHOLD_IN_PIXELS = 5f;
    
    private readonly Scene _scene;
    
    private PointF? _lastSinglePoint;
    
    private (PointF A, PointF B)? _lastTwoPoints;
    
    private DateTime _singleTouchStartTime;
    
    private Node _draggedNode;
    
    private Node _twoFingerDraggedNode;

    public bool IsDragging() => _draggedNode != null;
    
    public EventDispatcher(Scene scene)
    {
        _scene = scene;
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
                var touchData = new TouchEventData(worldPoint, TransformToLocal(_draggedNode, worldPoint));
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

    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = (pointA, pointB);
        PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerClicked(gestureData));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerPan(PointF pointA, PointF pointB, PointF delta)
    {
        PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerDrag(gestureData));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerPinch(PointF pointA, PointF pointB, float scaleFactor)
    {
        PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnPinch(gestureData));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerRotate(PointF pointA, PointF pointB, float angle)
    {
        PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnRotate(gestureData));
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
            var worldBounds = _scene.GetWorldBounds(node.Id);
            
            Console.WriteLine($"Hit Test: Node = {node.GetType().Name} (ID: {node.Id}), Bounds = ({worldBounds.X}, {worldBounds.Y}, {worldBounds.Width}, {worldBounds.Height}), Point = ({worldPoint.X}, {worldPoint.Y})");
            
            if (worldBounds.Contains(worldPoint))
            {
                var localPoint = TransformToLocal(node, worldPoint);
                
                if (node.GetLocalBounds().Contains(localPoint))
                {
                    var touchData = new TouchEventData(worldPoint, localPoint);
                    if (handler(node, touchData))
                        break;
                }
            }
        }
    }

    private void PropagateTwoFingerEvent(PointF pointA, PointF pointB, Func<Node, GestureEventData, bool> handler)
    {
        var orderedNodes = GetNodesInHitTestOrder(_scene.RootNode);
        
        foreach (var node in orderedNodes)
        {
            var worldBounds = _scene.GetWorldBounds(node.Id);
            
            if (worldBounds.Contains(pointA) || worldBounds.Contains(pointB))
            {
                var localA = TransformToLocal(node, pointA);
                var localB = TransformToLocal(node, pointB);
                
                if (node.GetLocalBounds().Contains(localA) || node.GetLocalBounds().Contains(localB))
                {
                    var gestureData = new GestureEventData(
                        new TouchEventData(pointA, localA),
                        new TouchEventData(pointB, localB));
                    
                    if (handler(node, gestureData))
                        break;
                }
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
                
                var childIds = _scene.HierarchyMap
                    .Where(kvp => kvp.Value == node.Id)
                    .Select(kvp => kvp.Key)
                    .Reverse();
                
                foreach (var childId in childIds)
                {
                    if (_scene.Nodes.TryGetValue(childId, out var child))
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
        if (Matrix3x2.Invert(_scene.GetWorldTransform(node.Id), out var inverse))
        {
            var localPoint = Vector2.Transform(new Vector2(worldPoint.X, worldPoint.Y), inverse);
            return new PointF(localPoint.X, localPoint.Y);
        }
        
        return worldPoint;
    }
    
    private PointF TransformDeltaToLocal(Node node, PointF worldDelta)
    {
        if (Matrix3x2.Invert(_scene.GetWorldTransform(node.Id), out var inverse))
        {
            var deltaVec = Vector2.Transform(new Vector2(worldDelta.X, worldDelta.Y), inverse);
            var originVec = Vector2.Transform(Vector2.Zero, inverse);
            return new PointF(deltaVec.X - originVec.X, deltaVec.Y - originVec.Y);
        }
        return worldDelta;
    }
}