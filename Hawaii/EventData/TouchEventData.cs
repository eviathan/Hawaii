namespace Hawaii.EventData;

public class TouchEventData
{
    public PointF WorldPoint { get; }
    
    public PointF LocalPoint { get; }

    public TouchEventData(PointF worldPoint, PointF localPoint)
    {
        WorldPoint = worldPoint;
        LocalPoint = localPoint;
    }
}