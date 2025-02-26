using Hawaii.Interfaces;
using Hawaii.Test.Nodes;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test.SceneBuilders;

public class FeatureSceneBuilder : ISceneBuilder
{
    private readonly ISceneService _sceneService;

    public FeatureSceneBuilder(ISceneService sceneService)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
    }
    
    public void Build(Scene scene, INodeState state)
    {
        // scene.ClearNodes();
        
        var background = new ImageNode();
        scene.AddNode(background, scene.RootNode.Id);

        if (state is FeaturesViewModel viewModel)
        {
            foreach (var feature in viewModel.Features)
            {
                var marker = new FeatureNode();
                scene.AddNode(marker, background.Id);
                
                _sceneService.SetTransform(marker.Id, feature.Transform ?? new Transform());
                
                foreach (var child in marker.Children)
                {
                    scene.AddNode(child, marker.Id);
                    _sceneService.SetTransform(child.Id, child.Transform ?? new Transform());
                }
            }
        }
        
        Console.WriteLine($"Built HierarchyMap: {string.Join(", ", scene.HierarchyMap.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}