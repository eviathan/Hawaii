using System.Numerics;

namespace Hawaii
{
    public class Transform
    {
        public Vector2 Position { get; set; } = Vector2.Zero;

        public Vector2 Scale { get; set; } = Vector2.One;

        public float Rotation { get; set; } = 0f;
    }
}
