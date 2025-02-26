using System.ComponentModel;
using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Services;

namespace Hawaii;

public class SceneRenderer : BindableObject, IDrawable
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State),
        typeof(INodeState),
        typeof(SceneRenderer),
        propertyChanged: (bindable, _, _) => ((SceneRenderer)bindable).OnStateChanged());
    
    private readonly ISceneBuilder _sceneBuilder;

    private readonly ISceneService _sceneService;
    
    private readonly IGestureRecognitionService _gestureRecognitionService;

    private readonly EventDispatcher _dispatcher;

    private readonly Dictionary<Guid, Node> _nodes = new();

    private Scene _scene;

    private INodeState _state;

    public GraphicsView? GraphicsView { get; set; }

    public INodeState State
    {
        get => (INodeState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public SceneRenderer(ISceneService sceneService, ISceneBuilder sceneBuilder, IGestureRecognitionService gestureRecognitionService)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
        _sceneBuilder = sceneBuilder ?? throw new ArgumentNullException(nameof(sceneBuilder));
        _gestureRecognitionService = gestureRecognitionService ?? throw new ArgumentNullException(nameof(gestureRecognitionService));

        _scene = new Scene(_sceneService);
        _scene.InvalidateView = () => GraphicsView?.Invalidate();
        
        _dispatcher = new EventDispatcher(_scene);
    }
    
    public void HandleSingleTouchDown(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _dispatcher.HandleSingleTouchDown(worldPoint);
    }

    public void HandleSingleTouchMove(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _dispatcher.HandleSingleTouchMove(worldPoint);
    }

    public void HandleSingleTouchUp(PointF point)
    {
        var worldPoint = TransformToWorld(point);
        _dispatcher.HandleSingleTouchUp(worldPoint);
    }
    
    public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    {
        _gestureRecognitionService.AddFrame(pointA, pointB);
        _dispatcher.HandleTwoFingerDown(pointA, pointB);
    }
    
    public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    {
        _gestureRecognitionService.AddFrame(pointA, pointB);
        
        if (_gestureRecognitionService.TryDetectPan(out var delta))
            _dispatcher.HandleTwoFingerPan(pointA, pointB, delta);
        if (_gestureRecognitionService.TryDetectPinch(out var scaleFactor))
            _dispatcher.HandleTwoFingerPinch(pointA, pointB, scaleFactor);
        if (_gestureRecognitionService.TryDetectRotation(out var angle))
            _dispatcher.HandleTwoFingerRotate(pointA, pointB, angle);
    }
    
    public void HandleTwoFingerUp(PointF pointA, PointF pointB)
    {
        var worldA = TransformToWorld(pointA);
        var worldB = TransformToWorld(pointB);
        
        _dispatcher.HandleTwoFingerUp(worldA, worldB);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var combinedDirty = _scene.GetDirtyRegion();
        
        if (combinedDirty.IsEmpty || _dispatcher.IsDragging())
            combinedDirty = dirtyRect;
        
        Console.WriteLine($"Draw: combinedDirty = ({combinedDirty.X}, {combinedDirty.Y}, {combinedDirty.Width}, {combinedDirty.Height})");

        var orderedNodes = _scene.GetNodesInDrawOrder();
        
        foreach (var node in orderedNodes)
        {
            var bounds = _scene.GetWorldBounds(node.Id);
            
            if (bounds.IntersectsWith(combinedDirty))
            {
                var transform = _scene.GetWorldTransform(node.Id);
                var localScale = _sceneService.GetTransform(node.Id).Scale;
                
                canvas.SaveState();
                canvas.ConcatenateTransform(transform);
                
                if (!node.PropagateScale)
                    canvas.Scale(localScale.X, localScale.Y);
                
                node.Renderer?.Draw(canvas, node, combinedDirty);
                canvas.RestoreState();
            }
        }
    }
    
    private PointF TransformToWorld(PointF screenPoint)
    {
        if (Matrix3x2.Invert(_scene.GetWorldTransform(_scene.RootNode.Id), out var inverse))
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
        _sceneBuilder.Build(_scene, _state);
        
        GraphicsView?.Invalidate();

        if (_state is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnStatePropertyChanged;
            notify.PropertyChanged += OnStatePropertyChanged;
        }
    }

    private void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _sceneBuilder.Build(_scene, _state);
        GraphicsView?.Invalidate();
    }
}