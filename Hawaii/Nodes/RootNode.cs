using Hawaii.Enums;
using Hawaii.Interfaces;
using Microsoft.Maui.Graphics;
using System.Numerics;

namespace Hawaii.Nodes
{
    public class RootNode : Node
    {
        public RootNode(Scene scene) : base(scene)
        {
            Size = new SizeF(float.MaxValue, float.MaxValue);
            IgnoreAncestorScale = false;
        }
    }
}
