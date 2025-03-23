using System.ComponentModel;
using System.Diagnostics;
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

        // canvas.FillColor = Colors.Green;
        // canvas.FillCircle(0, 0, 15); // Where is this?

        foreach (var node in _scene.GetNodesInDrawOrder())
        {
            Matrix3x2 nodeWorld = _scene.GetWorldTransform(node.Id);
            Debug.WriteLine($"Node {node.Id} World: {nodeWorld}");
            
            canvas.SaveState();
            canvas.ConcatenateTransform(nodeWorld);
            
            // canvas.FillColor = Colors.Red;
            // canvas.FillCircle(0, 0, 10);
            
            node.Renderer?.Draw(canvas, node, dirtyRect);
            
            canvas.RestoreState();
        }
        canvas.RestoreState();
    }

    private PointF ScreenToWorld(PointF screenPoint)
    {
        var worldPoint = _camera.ScreenToWorld(new Vector2(screenPoint.X, screenPoint.Y));
        return new PointF(worldPoint.X, worldPoint.Y);
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