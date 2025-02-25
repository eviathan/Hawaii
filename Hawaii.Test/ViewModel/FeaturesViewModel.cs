using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Test.Models;

namespace Hawaii.Test.ViewModel;

public class FeaturesViewModel : INodeState
{
    public List<Feature> Features { get; set; } = 
    [
        new()
        {
            Name = "Feature 1",
            Transform = new Transform
            {
                Position = new Vector2(12, 45)
            }
        },
        new()
        {
            Name = "Feature 2",
            Transform = new Transform
            {
                Position = new Vector2(120, 45)
            }
        },
        new()
        {
            Name = "Feature 3",
            Transform = new Transform
            {
                Position = new Vector2(0, 0)
            }
        },
    ];
}