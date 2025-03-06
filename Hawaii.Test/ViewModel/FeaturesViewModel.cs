using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Test.Models;

namespace Hawaii.Test.ViewModel;

public class FeaturesViewModel : INodeState
{
    public List<Feature> Features { get; set; } =
    [
        // new Feature
        // {
        //     Name = "Feature 1",
        //     Transform = new Transform
        //     {
        //         Position = new Vector2(100, 100)
        //     }
        // },
        //new Feature
        //{
        //    Name = "Feature 2",
        //    Transform = new Transform
        //    {
        //        Position = new Vector2(600, 400)
        //    }
        //},
        new Feature
        {
            Name = "Feature 3",
            Transform = new Transform
            {
                Position = new Vector2(0, 0)
            }
        },
    ];
}