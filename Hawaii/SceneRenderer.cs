using System.ComponentModel;
using System.Numerics;
using Hawaii.Interfaces;
using Hawaii.Nodes;

namespace Hawaii;

public class SceneRenderer : BindableObject, IDrawable
{
    private Scene _scene;

    private readonly SceneCamera _camera;

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
        _camera.UpdateViewportSize(new SizeF(dirtyRect.Width, dirtyRect.Height));
        canvas.SaveState();
        canvas.ConcatenateTransform(_camera.GetViewMatrix());

        var orderedNodes = _scene.GetNodesInDrawOrder();
        foreach (var node in orderedNodes)
        {
            var transform = _scene.GetParentTransform(node.Id);
            var localScale = node.Transform.Scale;

            canvas.SaveState();
            canvas.ConcatenateTransform(transform);

            if (!node.IgnoreAncestorScale)
                canvas.Scale(localScale.X, localScale.Y);

            // Only apply originOffset for RootNode if needed; child nodes handle it in transform
            if (node == _scene.RootNode)
            {
                var originOffset = node.GetOriginOffset();
                canvas.Translate(-originOffset.X, -originOffset.Y);
            }

            node.Renderer?.Draw(canvas, node, dirtyRect);
            canvas.RestoreState();
        }

        canvas.RestoreState();
    }

    private PointF TransformToWorld(PointF screenPoint)
    {
        return new PointF(_camera.ScreenToWorld(new Vector2(screenPoint.X, screenPoint.Y)).X,
                         _camera.ScreenToWorld(new Vector2(screenPoint.X, screenPoint.Y)).Y);
    }

    public void HandleSingleTouchDown(PointF point) => _eventDispatcher.HandleSingleTouchDown(TransformToWorld(point));
    public void HandleSingleTouchMove(PointF point) => _eventDispatcher.HandleSingleTouchMove(TransformToWorld(point));
    public void HandleSingleTouchUp(PointF point) => _eventDispatcher.HandleSingleTouchUp(TransformToWorld(point));
    public void HandleTwoFingerDown(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerDown(TransformToWorld(pointA), TransformToWorld(pointB));
    public void HandleTwoFingerMove(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerMove(TransformToWorld(pointA), TransformToWorld(pointB));
    public void HandleTwoFingerUp(PointF pointA, PointF pointB) =>
        _eventDispatcher.HandleTwoFingerUp(TransformToWorld(pointA), TransformToWorld(pointB));

    //public void HandleSingleTouchDown(PointF point)
    //{
    //    var worldPoint = TransformToWorld(point);
    //    _eventDispatcher.HandleSingleTouchDown(worldPoint);
    //}

    //public void HandleSingleTouchMove(PointF point)
    //{
    //    var worldPoint = TransformToWorld(point);
    //    _eventDispatcher.HandleSingleTouchMove(worldPoint);
    //}

    //public void HandleSingleTouchUp(PointF point)
    //{
    //    var worldPoint = TransformToWorld(point);
    //    _eventDispatcher.HandleSingleTouchUp(worldPoint);
    //}

    //public void HandleTwoFingerDown(PointF pointA, PointF pointB)
    //{
    //    _eventDispatcher.HandleTwoFingerDown(pointA, pointB);
    //}

    //public void HandleTwoFingerMove(PointF pointA, PointF pointB)
    //{
    //    _eventDispatcher.HandleTwoFingerMove(pointA, pointB);
    //}

    //public void HandleTwoFingerUp(PointF pointA, PointF pointB)
    //{
    //    var worldA = TransformToWorld(pointA);
    //    var worldB = TransformToWorld(pointB);

    //    _eventDispatcher.HandleTwoFingerUp(worldA, worldB);
    //}

    //public void Draw(ICanvas canvas, RectF dirtyRect)
    //{
    //    var orderedNodes = _scene.GetNodesInDrawOrder();

    //    // Root node's origin is at top-left, no initial translation needed
    //    canvas.SaveState();

    //    foreach (var node in orderedNodes)
    //    {
    //        var transform = _scene.GetParentTransform(node.Id);
    //        var localScale = node.Transform.Scale;

    //        canvas.SaveState();
    //        canvas.ConcatenateTransform(transform);

    //        if (!node.IgnoreAncestorScale)
    //            canvas.Scale(localScale.X, localScale.Y);

    //        // Special handling for CanvasNode to center its origin
    //        if (node is CanvasNode)
    //        {
    //            var viewportCenter = new Vector2(dirtyRect.Width / 2, dirtyRect.Height / 2);
    //            canvas.Translate(viewportCenter.X, viewportCenter.Y); // Center the canvas origin
    //        }

    //        var originOffset = node.GetOriginOffset();
    //        canvas.Translate(-originOffset.X, -originOffset.Y);

    //        node.Renderer?.Draw(canvas, node, dirtyRect);
    //        canvas.RestoreState();
    //    }

    //    canvas.RestoreState();
    //}

    //private PointF TransformToWorld(PointF screenPoint)
    //{
    //    var worldTransform = _scene.GetWorldTransform(_scene.RootNode.Id);
    //    if (Matrix3x2.Invert(worldTransform, out var inverse))
    //    {
    //        var worldVec = Vector2.Transform(new Vector2(screenPoint.X, screenPoint.Y), inverse);
    //        return new PointF(worldVec.X, worldVec.Y);
    //    }
    //    return screenPoint;
    //}

    //public void Draw(ICanvas canvas, RectF dirtyRect)
    //{
    //    var orderedNodes = _scene.GetNodesInDrawOrder();

    //    foreach (var node in orderedNodes)
    //    {
    //        var transform = _scene.GetParentTransform(node.Id);
    //        var localScale = node.Transform.Scale;

    //        canvas.SaveState();
    //        canvas.ConcatenateTransform(transform);

    //        if (!node.IgnoreAncestorScale)
    //            canvas.Scale(localScale.X, localScale.Y);

    //        node.Renderer?.Draw(canvas, node, dirtyRect);
    //        canvas.RestoreState();
    //    }
    //}


    //private PointF TransformToWorld(PointF screenPoint)
    //{
    //    if (Matrix3x2.Invert(_scene.GetParentTransform(_scene.RootNode.Id), out var inverse))
    //    {
    //        var worldVec = Vector2.Transform(new Vector2(screenPoint.X, screenPoint.Y), inverse);
    //        return new PointF(worldVec.X, worldVec.Y);
    //    }

    //    return screenPoint;
    //}

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