using System.ComponentModel;
using System.Numerics;
using Hawaii.Interfaces;

namespace Hawaii;

public class SceneRenderer : BindableObject, IDrawable
{
    private Scene _scene;
    
    private readonly ISceneBuilder _sceneBuilder;

    private readonly EventDispatcher _eventDispatcher;

    private readonly Dictionary<Guid, Node> _nodes = [];

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

    public SceneRenderer(Scene scene, EventDispatcher eventDispatcher, ISceneBuilder sceneBuilder)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _sceneBuilder = sceneBuilder ?? throw new ArgumentNullException(nameof(sceneBuilder));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        
        _scene.InvalidateView = () => GraphicsView?.Invalidate();
    }
    
    public void HandleSingleTouchDown(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _eventDispatcher.HandleSingleTouchDown(worldPoint);
    }

    public void HandleSingleTouchMove(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _eventDispatcher.HandleSingleTouchMove(worldPoint);
    }

    public void HandleSingleTouchUp(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _eventDispatcher.HandleSingleTouchUp(worldPoint);
    }
    
    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _eventDispatcher.HandleTwoFingerDown(pointA, pointB);
    }
    
    public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    {
        _eventDispatcher.HandleTwoFingerMove(pointA, pointB);
    }
    
    public void HandleTwoFingerUp(PointF pointA, PointF pointB)
    {
        var worldA = TransformToWorld(pointA);
        var worldB = TransformToWorld(pointB);
        
        _eventDispatcher.HandleTwoFingerUp(worldA, worldB);
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var orderedNodes = _scene.GetNodesInDrawOrder();
    
        foreach (var node in orderedNodes)
        {
            var transform = _scene.GetParentTransform(node.Id);
            var localScale = _scene.GetTransform(node.Id).Scale;
        
            canvas.SaveState();
            canvas.ConcatenateTransform(transform);
        
            if (!node.IgnoreAncestorScale)
                canvas.Scale(localScale.X, localScale.Y);
        
            node.Renderer?.Draw(canvas, node, dirtyRect);
            canvas.RestoreState();
        }
    }
    
    private PointF TransformToWorld(PointF screenPoint)
    {
        if (Matrix3x2.Invert(_scene.GetParentTransform(_scene.RootNode.Id), out var inverse))
        {
            var worldVec = Vector2.Transform(new Vector2(screenPoint.X, screenPoint.Y), inverse);
            return new PointF(worldVec.X, worldVec.Y);
        }

        return screenPoint;
    }

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
}