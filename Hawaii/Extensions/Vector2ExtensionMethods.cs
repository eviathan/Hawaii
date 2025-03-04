using System.Numerics;

namespace Hawaii.Extensions
{
    public static class Vector2ExtensionMethods
    {
        public static Vector2 GetScale(this Matrix3x2 matrix)
        {
            return new Vector2(
                MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M21 * matrix.M21),
                MathF.Sqrt(matrix.M12 * matrix.M12 + matrix.M22 * matrix.M22)
            );
        }
    }
}
