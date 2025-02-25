using System.Numerics;

namespace Hawaii;

public class EventDispatcher
{
    private readonly Scene _scene;

    private PointF? _lastSinglePoint;

    private (PointF A, PointF B)? _lastTwoPoints;

    public EventDispatcher(Scene scene)
    {
        _scene = scene;
    }

    public void HandleSingleTouchDown(PointF worldPoint)
    {
        _lastSinglePoint = worldPoint;
        PropagateEvent(worldPoint, n => n.OnClicked(worldPoint));
    }

    public void HandleSingleTouchMove(PointF worldPoint)
    {
        if (_lastSinglePoint.HasValue)
        {
            var delta = new PointF(worldPoint.X - _lastSinglePoint.Value.X, worldPoint.Y - _lastSinglePoint.Value.Y);
            PropagateEvent(worldPoint, n => n.OnDrag(worldPoint, delta));
            _lastSinglePoint = worldPoint;
        }
    }

    public void HandleSingleTouchUp(PointF worldPoint)
    {
        _lastSinglePoint = null;
    }

    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _lastTwoPoints = (pointA, pointB);
    }

    public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    {
        if (_lastTwoPoints.HasValue)
        {
            var (prevA, prevB) = _lastTwoPoints.Value;
            var deltaA = pointA - prevA;
            var deltaB = pointB - prevB;
            var avgDelta = new PointF((deltaA.Width + deltaB.Width) / 2, (deltaA.Height + deltaB.Height) / 2);

            foreach (var node in _scene.GetNodesInDrawOrder())
            {
                var transform = node.Transform;
                transform.Position += new Vector2(avgDelta.X, avgDelta.Y);
                _scene.SetTransform(node.Id, transform);
            }

            var scaleFactor = pointA.Distance(pointB) / prevA.Distance(prevB);
            var angle = AngleBetween(prevA, prevB, pointA, pointB);
            PropagateEvent(pointA, n => n.OnPinch(pointA, pointB, scaleFactor));
            PropagateEvent(pointA, n => n.OnRotate(pointA, pointB, angle));

            _lastTwoPoints = (pointA, pointB);
        }
    }

    public void HandleTwoFingerUp()
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
                if (node.GetLocalBounds().Contains(localPoint))
                {
                    if (handler(node))
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
                
                var childIds = _scene.HierarchyMap.Where(kvp => kvp.Value == node.Id)
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
        var worldTransform = _scene.GetWorldTransform(node.Id);
        if (Matrix3x2.Invert(worldTransform, out var inverse))
        {
            var localVec = Vector2.Transform(new Vector2(worldPoint.X, worldPoint.Y), inverse);
            return new PointF(localVec.X, localVec.Y);
        }
        return worldPoint;
    }

    private float AngleBetween(PointF a1, PointF b1, PointF a2, PointF b2)
    {
        var v1 = new Vector2(b1.X - a1.X, b1.Y - a1.Y);
        var v2 = new Vector2(b2.X - a2.X, b2.Y - a2.Y);
        var dot = Vector2.Dot(v1, v2);
        var det = v1.X * v2.Y - v1.Y * v2.X;
        return MathF.Atan2(det, dot) * 180f / MathF.PI;
    }
}