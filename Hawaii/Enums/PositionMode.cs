namespace Hawaii.Enums;

public enum PositionMode
{
    Relative,  // Offset from parent's position, respects parent transform
    Absolute,  // World-space coordinates, ignores parent transform except root
    Static,    // Fixed in parent's layout, ignores own scale/rotation
    Fixed      // Fixed relative to canvas (root) space, ignores all parent transforms
}