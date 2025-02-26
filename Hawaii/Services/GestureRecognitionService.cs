using Hawaii.Extensions;
using Hawaii.Interfaces;

namespace Hawaii.Services;

public class GestureRecognitionService : IGestureRecognitionService
{
    private const int BUFFER_SIZE = 6;
    
    private readonly Queue<(PointF A, PointF B)> _gestureBuffer = new Queue<(PointF A, PointF B)>();

    public void AddFrame(PointF pointA, PointF pointB)
    {
        _gestureBuffer.Enqueue((pointA, pointB));

        if (_gestureBuffer.Count > BUFFER_SIZE)
        {
            _gestureBuffer.Dequeue();
        }
    }

    public bool TryDetectPan(out PointF delta)
    {
        delta = new PointF(0, 0);

        if (_gestureBuffer.Count < BUFFER_SIZE)
            return false;

        var baselineCount = BUFFER_SIZE / 2;
        var baselineFrames = _gestureBuffer.Take(baselineCount).ToArray();

        var averageAX = baselineFrames.Average(frame => frame.A.X);
        var averageAY = baselineFrames.Average(frame => frame.A.Y);
        var averageBX = baselineFrames.Average(frame => frame.B.X);
        var averageBY = baselineFrames.Average(frame => frame.B.Y);

        var baselineA = new PointF(averageAX, averageAY);
        var baselineB = new PointF(averageBX, averageBY);

        var latest = _gestureBuffer.Last();

        var deltaA = new PointF(latest.A.X - baselineA.X, latest.A.Y - baselineA.Y);
        var deltaB = new PointF(latest.B.X - baselineB.X, latest.B.Y - baselineB.Y);

        delta = new PointF((deltaA.X + deltaB.X) / 2, (deltaA.Y + deltaB.Y) / 2);

        return Math.Abs(delta.X) > 3.0f || Math.Abs(delta.Y) > 3.0f;
    }

    public bool TryDetectPinch(out float scaleFactor)
    {
        scaleFactor = 1.0f;

        if (_gestureBuffer.Count < 2)
            return false;

        var initial = _gestureBuffer.First();
        var latest = _gestureBuffer.Last();

        var initialDistance = initial.A.Distance(initial.B);
        var latestDistance = latest.A.Distance(latest.B);

        if (initialDistance == 0) return false;

        scaleFactor = latestDistance / initialDistance;

        return Math.Abs(scaleFactor - 1.0f) > 0.05f;
    }

    public bool TryDetectRotation(out float angleChange)
    {
        angleChange = 0f;

        if (_gestureBuffer.Count < 2)
            return false;

        var initial = _gestureBuffer.First();
        var latest = _gestureBuffer.Last();

        var initialAngle = initial.A.Angle(initial.B);
        var latestAngle = latest.A.Angle(latest.B);

        angleChange = latestAngle - initialAngle;

        return Math.Abs(angleChange) > 3.0f;
    }
}