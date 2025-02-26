namespace Hawaii.Extensions;

public static class FloatExtensions
{
    public static float DegreesToRadians(this float degrees)
    {
        return (float)(degrees * (Math.PI / 180.0));
    }

    public static float Clamp(this float value, float amount)
    {
        if (value < 0f)
            return 0f;
        else if (value > amount)
            return amount;
        else
            return value;
    }
}
