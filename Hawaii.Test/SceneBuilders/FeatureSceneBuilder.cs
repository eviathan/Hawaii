using System.Numerics;
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
    
    public Scene Build(INodeState state)
    {
        if (state is not FeaturesViewModel viewModel)
            throw new ArgumentException("Expected FeaturesViewModel", nameof(state));
        
        var scene = new Scene(_sceneService);
        var background = new ImageNode();
        scene.AddNode(background, scene.RootNode.Id);
        
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

        return scene;
    }
}