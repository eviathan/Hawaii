using System.ComponentModel;
using Hawaii.Interfaces;
using Hawaii.Services;

namespace Hawaii;

public class SceneRenderer : BindableObject, IDrawable
{
    private readonly ISceneBuilder _sceneBuilder;

    private readonly ISceneService _sceneService;

    private readonly EventDispatcher _dispatcher;

    private readonly Dictionary<Guid, Node> _nodes = new();

    private Scene _scene;

    private INodeState _state;

    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State),
        typeof(INodeState),
        typeof(SceneRenderer),
        propertyChanged: (bindable, _, _) => ((SceneRenderer)bindable).OnStateChanged());

    public INodeState State
    {
        get => (INodeState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public GraphicsView? GraphicsView { get; set; }

    public void HandleSingleTouchDown(PointF worldPoint) => _dispatcher.HandleSingleTouchDown(worldPoint);
    public void HandleSingleTouchMove(PointF worldPoint) => _dispatcher.HandleSingleTouchMove(worldPoint);
    public void HandleSingleTouchUp(PointF worldPoint) => _dispatcher.HandleSingleTouchUp(worldPoint);

    public SceneRenderer(ISceneService sceneService, ISceneBuilder sceneBuilder)
    {
        _sceneService = sceneService ?? throw new ArgumentNullException(nameof(sceneService));
        _sceneBuilder = sceneBuilder ?? throw new ArgumentNullException(nameof(sceneBuilder));
        
        _scene = new Scene(_sceneService);
        _dispatcher = new EventDispatcher(_scene);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var combinedDirty = _scene.GetDirtyRegion();
        if (combinedDirty.IsEmpty)
            combinedDirty = dirtyRect;

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

    private void OnStateChanged()
    {
        if (State == null)
            return;

        _state = State;
        _scene = _sceneBuilder.Build(_state);
        GraphicsView?.Invalidate();

        if (_state is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged -= OnStatePropertyChanged;
            notify.PropertyChanged += OnStatePropertyChanged;
        }
    }

    private void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _scene = _sceneBuilder.Build(_state);
        GraphicsView?.Invalidate();
    }
}