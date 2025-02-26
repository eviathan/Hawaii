using Hawaii.Interfaces;
using Hawaii.Test.Nodes;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test.SceneBuilders;

public class FeatureSceneBuilder : ISceneBuilder
{
    private readonly IServiceProvider _serviceProvider;
    
    private readonly ISceneService _sceneService;

    public FeatureSceneBuilder(IServiceProvider serviceProvider, ISceneService sceneService)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
    }
    
    public void Build(Scene scene, INodeState state)
    {
        scene.ClearNodes();
        scene.AddNode(scene.RootNode);
        
        var background = _serviceProvider.GetRequiredService<ImageNode>();
        scene.AddNode(background, scene.RootNode.Id);

        if (state is FeaturesViewModel viewModel)
        {
            foreach (var feature in viewModel.Features)
            {
                var marker = _serviceProvider.GetRequiredService<FeatureNode>();
                scene.AddNode(marker, background.Id);
                _sceneService.SetTransform(marker.Id, feature.Transform);
                
                foreach (var child in marker.Children)
                {
                    scene.AddNode(child, marker.Id);
                    _sceneService.SetTransform(child.Id, child.Transform);
                }
            }
        }
        
        Console.WriteLine($"Built HierarchyMap: {string.Join(", ", scene.HierarchyMap.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}