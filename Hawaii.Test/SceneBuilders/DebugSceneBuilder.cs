using Hawaii.Interfaces;
using Hawaii.Nodes;
using Hawaii.Test.Nodes;
using Hawaii.Test.ViewModel;

namespace Hawaii.Test.SceneBuilders
{
    public class DebugSceneBuilder : ISceneBuilder
    {
        private readonly Scene _scene;

        private readonly ImageNode _backgroundImage;

        public DebugSceneBuilder(Scene scene, ImageNode backgroundImage)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _backgroundImage = backgroundImage ?? throw new ArgumentNullException(nameof(backgroundImage));
        }

        public void Build(INodeState state)
        {
            _scene.RootNode.AddChild(_backgroundImage);

            // if (state is FeaturesViewModel viewModel)
            // {
            //     foreach (var feature in viewModel.Features)
            //     {
            //         var featureNode = new FeatureNode(_scene, feature);
            //         //featureNode.Initialise();
            //
            //         //var translationHandle = new FeatureHandleNode(_scene);
            //         //translationHandle.State = state;
            //         //translationHandle.Feature = featureNode;
            //         //translationHandle.Transform = new Transform
            //         //{
            //         //    Position = new Vector2(0f, -HANDLE_OFFSET),
            //         //};
            //         //translationHandle.Clicked += OnTranslationHandleClicked;
            //         //translationHandle.Dragged += OnTranslationHandleDragged;
            //         //featureNode.AddChild(translationHandle);
            //
            //         _backgroundImage.AddChild(featureNode);
            //     }
            // }
        }
    }
}
