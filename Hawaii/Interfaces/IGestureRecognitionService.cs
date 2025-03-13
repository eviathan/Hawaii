namespace Hawaii.Interfaces;

public interface IGestureRecognitionService
{
    void AddFrame(PointF pointA, PointF pointB);

    bool TryDetectPan(out PointF delta);

    bool TryDetectPinch(out float scaleFactor);

    bool TryDetectRotation(out float angleChange);
}