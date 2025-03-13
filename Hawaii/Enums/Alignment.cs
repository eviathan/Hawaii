namespace Hawaii.Enums;

public enum Alignment
{
    None,        // No automatic alignment (use Transform.Position)
    Center,      // Center in parent's space
    TopLeft,     // Align top-left with parent's top-left
    TopRight,    // Align top-right with parent's top-right
    BottomLeft,  // Align bottom-left with parent's bottom-left
    BottomRight  // Align bottom-right with parent's bottom-right
}