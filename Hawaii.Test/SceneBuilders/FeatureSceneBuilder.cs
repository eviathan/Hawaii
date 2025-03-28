using Hawaii.Interfaces;
using Hawaii.Test.Nodes;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test.SceneBuilders;

public class FeatureSceneBuilder : ISceneBuilder
{
    private readonly Scene _scene;
    
    private readonly IServiceProvider _serviceProvider;

    public FeatureSceneBuilder(Scene scene, IServiceProvider serviceProvider)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public void Build(INodeState state)
    {
        _scene.ClearNodes();
        _scene.AddNode(_scene.RootNode);
        
        var background = _serviceProvider.GetRequiredService<ImageNode>();
        background.Scene = _scene;
        _scene.AddNode(background, _scene.RootNode.Id);

        if (state is FeaturesViewModel viewModel)
        {
            foreach (var feature in viewModel.Features)
            {
                var featureNode = _serviceProvider.GetRequiredService<FeatureNode>();
                
                _scene.AddNode(featureNode, background.Id);
                 _scene.SetTransform(featureNode.Id, feature.Transform);
                
                foreach (var child in featureNode.Children)
                {
                    _scene.AddNode(child, featureNode.Id);
                     _scene.SetTransform(child.Id, child.Transform);
                }
            }
        }
        
        Console.WriteLine($"Built HierarchyMap: {string.Join(", ", _scene.HierarchyMap.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
    }
}