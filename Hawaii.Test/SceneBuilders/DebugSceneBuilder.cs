using Hawaii.Interfaces;
using Hawaii.Nodes;
using Hawaii.Test.Nodes;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test.SceneBuilders
{
    public class DebugSceneBuilder : ISceneBuilder
    {
        private readonly Scene _scene;

        private readonly CanvasNode _canvas;

        private readonly ImageNode _backgroundImage;

        private readonly IServiceProvider _serviceProvider;

        public DebugSceneBuilder(Scene scene, CanvasNode canvas, ImageNode backgroundImage, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _backgroundImage = backgroundImage ?? throw new ArgumentNullException(nameof(backgroundImage));
        }

        public void Build(INodeState state)
        {
            _scene.RootNode.AddChild(_canvas);
            _canvas.AddChild(_backgroundImage);

            if (state is FeaturesViewModel viewModel)
            {
                foreach (var feature in viewModel.Features)
                {
                    var featureNode = _serviceProvider.GetRequiredService<FeatureNode>();
                    _backgroundImage.AddChild(featureNode);
                    featureNode.InitHandles();

                }
            }

            Console.WriteLine($"Built HierarchyMap: {string.Join(", ", _scene.HierarchyMap.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
        }
    }
}
