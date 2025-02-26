using System.Numerics;
using Hawaii.Extensions;

namespace Hawaii;

public class EventDispatcher
{
    private const float ClickThresholdInMs = 200f;
    
    private const float DragThresholdInPixels = 5f;
    
    private readonly Scene _scene;
    
    private PointF? _lastSinglePoint;
    
    private (PointF A, PointF B)? _lastTwoPoints;
    
    private DateTime _singleTouchStartTime;

    public EventDispatcher(Scene scene)
    {
        _scene = scene;
    }
    
    public void HandleSingleTouchDown(PointF worldPoint)
    {
        _lastSinglePoint = worldPoint;
        _singleTouchStartTime = DateTime.Now;
    }

    public void HandleSingleTouchMove(PointF worldPoint)
    {
        if (!_lastSinglePoint.HasValue) return;

        var delta = new PointF(worldPoint.X - _lastSinglePoint.Value.X, worldPoint.Y - _lastSinglePoint.Value.Y);
        var timeElapsed = (DateTime.Now - _singleTouchStartTime).TotalMilliseconds;

        if (timeElapsed > ClickThresholdInMs || delta.Length() > DragThresholdInPixels)
        {
            PropagateEvent(worldPoint, node => node.OnDrag(worldPoint, delta));
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

            if (timeElapsed <= ClickThresholdInMs && delta.Length() <= DragThresholdInPixels)
                PropagateEvent(worldPoint, node => node.OnClicked(worldPoint));
        }
        
        _lastSinglePoint = null;
    }

    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = (pointA, pointB);
        PropagateTwoFingerEvent(pointA, pointB, node => node.OnTwoFingerClicked(pointA, pointB));
    }

    public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = (pointA, pointB);
        // Gesture recognition handled in SceneRenderer
    }

    public void HandleTwoFingerPan(PointF pointA, PointF pointB, PointF delta)
    {
        PropagateTwoFingerEvent(pointA, pointB, node => node.OnTwoFingerDrag(pointA, pointB, delta));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerPinch(PointF pointA, PointF pointB, float scaleFactor)
    {
        PropagateTwoFingerEvent(pointA, pointB, node => node.OnPinch(pointA, pointB, scaleFactor));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerRotate(PointF pointA, PointF pointB, float angle)
    {
        PropagateTwoFingerEvent(pointA, pointB, node => node.OnRotate(pointA, pointB, angle));
        _scene.InvalidateView?.Invoke();
    }

    public void HandleTwoFingerUp(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = null;
    }

    private void PropagateEvent(PointF worldPoint, Func<Node, bool> handler)
    {
        foreach (var node in GetNodesInHitTestOrder(_scene.RootNode))
        {
            var worldBounds = _scene.GetWorldBounds(node.Id);
            if (worldBounds.Contains(worldPoint))
            {
                var localPoint = TransformToLocal(node, worldPoint);
                
                if (node.GetLocalBounds().Contains(localPoint) && handler(node))
                   break;
            }
        }
    }

    private void PropagateTwoFingerEvent(PointF pointA, PointF pointB, Func<Node, bool> handler)
    {
        foreach (var node in GetNodesInHitTestOrder(_scene.RootNode))
        {
            var worldBounds = _scene.GetWorldBounds(node.Id);
            if (worldBounds.Contains(pointA) || worldBounds.Contains(pointB))
            {
                var localA = TransformToLocal(node, pointA);
                var localB = TransformToLocal(node, pointB);
                
                if (node.GetLocalBounds().Contains(localA) || node.GetLocalBounds().Contains(localB) && handler(node))
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

    private PointF TransformToLocal(Node node, PointF worldPoint)
    {
        if (Matrix3x2.Invert(_scene.GetWorldTransform(node.Id), out var inverse))
        {
            var localPoint = Vector2.Transform(new Vector2(worldPoint.X, worldPoint.Y), inverse);
            return new PointF(localPoint.X, localPoint.Y);
        }
        
        return worldPoint;
    }
}