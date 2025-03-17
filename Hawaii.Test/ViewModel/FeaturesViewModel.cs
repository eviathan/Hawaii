using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Test.Models;

namespace Hawaii.Test.ViewModel;

public class FeaturesViewModel : INodeState
{
    public List<Feature> Features { get; set; } =
    [
        new Feature
        {
            Name = "Middle",
            Transform = new Transform
            {
                Position = new Vector2(500, 500)
            }
        },
        new Feature
        {
            Name = "Bottom Right",
            Transform = new Transform
            {
                Position = new Vector2(1000, 1000)
            }
        },
        new Feature
        {
            Name = "Top Left",
            Transform = new Transform
            {
                Position = new Vector2(0, 0)
            }
        },
    ];
}