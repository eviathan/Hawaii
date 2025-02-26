using System.Numerics;
using Hawaii.EventData;
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
                PropagateEvent(worldPoint, (node, touchData) =>
                {
                    var localDelta = TransformDeltaToLocal(node, delta);
                    return node.OnDrag(touchData, localDelta);
                });
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
                    PropagateEvent(worldPoint, (node, touchData) => node.OnClicked(touchData));
            }
            _lastSinglePoint = null;
        }

        public void HandleTwoFingerDown(PointF pointA, PointF pointB)
        {
            _lastTwoPoints = (pointA, pointB);
            PropagateTwoFingerEvent(pointA, pointB, (node, gestureData) => node.OnTwoFingerClicked(gestureData));
        }

        public void HandleTwoFingerMove(PointF pointA, PointF pointB)
        {
            _lastTwoPoints = (pointA, pointB);
            // Gesture recognition in SceneRenderer
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
            _lastTwoPoints = null;
        }

        private void PropagateEvent(PointF worldPoint, Func<Node, TouchEventData, bool> handler)
        {
            var orderedNodes = GetNodesInHitTestOrder(_scene.RootNode);
            
            foreach (var node in orderedNodes)
            {
                var worldBounds = _scene.GetWorldBounds(node.Id);
                
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

        // Overload for gestures with additional data
        private void PropagateTwoFingerEvent(PointF pointA, PointF pointB, PointF? delta, float? scaleFactor, float? angle, Func<Node, GestureEventData, bool> handler)
        {
            foreach (var node in GetNodesInHitTestOrder(_scene.RootNode))
            {
                var worldBounds = _scene.GetWorldBounds(node.Id);
                
                if (worldBounds.Contains(pointA) || worldBounds.Contains(pointB))
                {
                    var localA = TransformToLocal(node, pointA);
                    var localB = TransformToLocal(node, pointB);
                    
                    if (node.GetLocalBounds().Contains(localA) || node.GetLocalBounds().Contains(localB))
                    {
                        PointF? localDelta = delta.HasValue 
                            ? TransformDeltaToLocal(node, delta.Value) 
                            : null;
                        
                        var gestureData = new GestureEventData(
                            new TouchEventData(pointA, localA),
                            new TouchEventData(pointB, localB),
                            localDelta,
                            scaleFactor,
                            angle
                        );
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