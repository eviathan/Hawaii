namespace Hawaii.EventData;

public class GestureEventData
{
    public TouchEventData PointA { get; }
    
    public TouchEventData PointB { get; }
    
    public PointF? Delta { get; } // Local delta for drag/pan
    
    public float? ScaleFactor { get; } // For pinch
    
    public float? Angle { get; } // For rotate

    public GestureEventData(TouchEventData pointA, TouchEventData pointB, PointF? delta = null, float? scaleFactor = null, float? angle = null)
    {
        PointA = pointA;
        PointB = pointB;
        Delta = delta;
        ScaleFactor = scaleFactor;
        Angle = angle;
    }
}