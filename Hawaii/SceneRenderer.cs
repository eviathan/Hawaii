using System.ComponentModel;
using System.Numerics;
using Hawaii.Extensions;
using Hawaii.Interfaces;
using Hawaii.Nodes;

namespace Hawaii;

public class SceneRenderer : BindableObject, IDrawable
{
    private Scene _scene;

    private readonly SceneCamera _camera;

    private readonly ISceneBuilder _sceneBuilder;

    private readonly EventDispatcher _eventDispatcher;

    public GraphicsView? GraphicsView { get; set; }
    
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State),
        typeof(INodeState),
        typeof(SceneRenderer),
        propertyChanged: (bindable, _, _) => ((SceneRenderer)bindable).OnStateChanged());
    
    private INodeState _state;
    
    public INodeState State
    {
        get => (INodeState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public SceneRenderer(Scene scene, SceneCamera camera, EventDispatcher eventDispatcher, ISceneBuilder sceneBuilder)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _sceneBuilder = sceneBuilder ?? throw new ArgumentNullException(nameof(sceneBuilder));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        
        _scene.InvalidateView = () => GraphicsView?.Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        _camera.ApplyTransform(canvas, dirtyRect);

        var orderedNodes = _scene.GetNodesInDrawOrder();
        foreach (var node in orderedNodes)
        {
            var transform = _scene.GetParentTransform(node.Id);
            var localScale = node.Transform.Scale;

            canvas.SaveState();
            canvas.ConcatenateTransform(transform);

            // if (node is MarkerNode)
            // {
            //     var inheritedScale = _camera.Transform.Scale * transform.GetScale();
            //     canvas.Scale(1f / inheritedScale.X, 1f / inheritedScale.Y);
            // }
            
            if (node is MarkerNode)
            {
                var inheritedScale = _camera.Transform.Scale * transform.GetScale();
                canvas.Scale(1f / inheritedScale.X, 1f / inheritedScale.Y);
                // Offset to correct drift: scale the origin offset by the inverse scale
                var originOffset = node.GetOriginOffset(); // (50, 50) for Center
                canvas.Translate(originOffset.X * (inheritedScale.X - 1), originOffset.Y * (inheritedScale.Y - 1));
            }

            if (node == _scene.RootNode)
            {
                var originOffset = node.GetOriginOffset();
                canvas.Translate(-originOffset.X, -originOffset.Y);
            }

            // Render Node
            node.Renderer?.Draw(canvas, node, dirtyRect);

            canvas.RestoreState();
        }

        canvas.RestoreState();
    }

    private PointF ScreenToWorld(PointF screenPoint)
    {
        return new PointF(
            _camera.ScreenToWorld(new Vector2(screenPoint.X, screenPoint.Y)).X,
            _camera.ScreenToWorld(new Vector2(screenPoint.X, screenPoint.Y)).Y
        );
    }

    #region Event Handling
    public void HandleSingleTouchDown(PointF point) =>
        _eventDispatcher.HandleSingleTouchDown(ScreenToWorld(point));

    public void HandleSingleTouchMove(PointF point) =>
        _eventDispatcher.HandleSingleTouchMove(ScreenToWorld(point));

    public void HandleSingleTouchUp(PointF point) =>
        _eventDispatcher.HandleSingleTouchUp(ScreenToWorld(point));

    public void HandleTwoFingerDown(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerDown(ScreenToWorld(pointA), ScreenToWorld(pointB));

    public void HandleTwoFingerMove(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerMove(ScreenToWorld(pointA), ScreenToWorld(pointB));

    public void HandleTwoFingerUp(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerUp(ScreenToWorld(pointA), ScreenToWorld(pointB));

    private void OnStateChanged()
    {
        if (State == null)
            return;

        _state = State;
        _scene.ClearNodes();
        _sceneBuilder.Build(_state);
        GraphicsView?.Invalidate();

        if (_state is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnStatePropertyChanged;
            notify.PropertyChanged += OnStatePropertyChanged;
        }
    }

    private void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _scene.ClearNodes();
        _sceneBuilder.Build(_state);

        GraphicsView?.Invalidate();
    }
    #endregion
}