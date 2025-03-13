using System.Numerics;

namespace Hawaii.Extensions;

public static class PointFExtensions
{
    public static float Length(this PointF point)
    {
        return PointF.Zero.Distance(point);
    }
    
    public static float Distance(this PointF a, PointF b)
    {
        var deltaX = b.X - a.X;
        var deltaY = b.Y - a.Y;

        return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public static float Angle(this PointF a, PointF b)
    {
        var deltaX = b.X - a.X;
        var deltaY = b.Y - a.Y;
        var radians = MathF.Atan2(deltaY, deltaX);
        var degrees = radians * (180f / MathF.PI);

        return degrees;
    }

    public static PointF Midpoint(this PointF a, PointF b)
    {
        return new PointF((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
    }

    public static PointF ForwardTransform(this PointF worldPoint, float translateX, float translateY, float scale, float rotationDeg)
    {
        var scaleX = worldPoint.X * scale;
        var scaleY = worldPoint.Y * scale;

        var radians = rotationDeg * (MathF.PI / 180f);
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);

        var rotationX = (scaleX * cos) - (scaleY * sin);
        var rotationY = (scaleX * sin) + (scaleY * cos);

        var finalX = rotationX + translateX;
        var finalY = rotationY + translateY;

        return new PointF(finalX, finalY);
    }

    public static PointF InverseTransform(this PointF devicePoint, float translateX, float translateY, float scale, float rotationDeg)
    {
        float x = devicePoint.X - translateX;
        float y = devicePoint.Y - translateY;

        float radians = rotationDeg * (MathF.PI / 180f);
        float cos = MathF.Cos(-radians);
        float sin = MathF.Sin(-radians);

        float rotationX = (x * cos) - (y * sin);
        float rotationY = (x * sin) + (y * cos);

        float finalX = rotationX / scale;
        float finalY = rotationY / scale;

        return new PointF(finalX, finalY);
    }

    public static List<PointF> FindNearbyPoints(this List<PointF> points, PointF target, float radius)
    {
        float r2 = radius * radius;

        return points
            .Where(p => (p.X - target.X) * (p.X - target.X) + (p.Y - target.Y) * (p.Y - target.Y) <= r2)
            .ToList();
    }

    public static PointF LocalToWorld(this PointF localPoint, Matrix3x2 worldTransform)
    {
        var localVector = new Vector2(localPoint.X, localPoint.Y);
        var worldVector = Vector2.Transform(localVector, worldTransform);

        return new PointF(worldVector.X, worldVector.Y);
    }

    public static PointF WorldToLocal(this PointF worldPoint, Matrix3x2 worldTransform)
    {
        _ = Matrix3x2.Invert(worldTransform, out var inverse);
        var worldVector = new Vector2(worldPoint.X, worldPoint.Y);
        var localVector = Vector2.Transform(worldVector, inverse);

        return new PointF(localVector.X, localVector.Y);
    }

    public static float LookAt(this PointF from, PointF to, float offsetDegrees = 0f)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var radians = (float)Math.Atan2(dy, dx);
        var angle = radians * (180f / (float)Math.PI);

        angle += offsetDegrees;
        angle %= 360f;

        if (angle < 0)
            angle += 360f;

        return angle;
    }
}