namespace Hawaii.EventData;

public class TouchEventData
{
    public PointF WorldPoint { get; }
    
    public PointF ParentPoint { get; }
    
    public PointF LocalPoint { get; }

    public TouchEventData(PointF worldPoint, PointF parentPoint, PointF localPoint)
    {
        WorldPoint = worldPoint;
        ParentPoint = parentPoint;
        LocalPoint = localPoint;
    }
}