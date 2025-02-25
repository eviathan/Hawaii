using Hawaii.Interfaces;
using Hawaii.Services;

namespace Hawaii;

public class SceneGraphicsView<TBuilder> : GraphicsView
    where TBuilder : class, ISceneBuilder
    {
        private readonly SceneRenderer _renderer;

        private readonly ISceneService _sceneService;

        public INodeState State
        {
            get => (INodeState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly BindableProperty StateProperty = BindableProperty.Create(
            nameof(State),
            typeof(INodeState),
            typeof(SceneGraphicsView<TBuilder>),
            propertyChanged: (bindableObject, oldObject, newObject) =>
                ((SceneGraphicsView<TBuilder>)bindableObject)._renderer.State = (INodeState)newObject
        );

        public SceneGraphicsView()
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;

            if (services == null)
                throw new InvalidOperationException("DI container not available.");

            var sceneBuilder = services.GetRequiredService<TBuilder>();
            
            _sceneService = services.GetRequiredService<ISceneService>();
            _renderer = new SceneRenderer(_sceneService, sceneBuilder);
            
            _renderer.GraphicsView = this;
            Drawable = _renderer;

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += (s, e) =>
            {
                var point = new PointF((float)e.TotalX, (float)e.TotalY);
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        _renderer.HandleSingleTouchDown(point);
                        break;
                    case GestureStatus.Running:
                        _renderer.HandleSingleTouchMove(point);
                        Invalidate();
                        break;
                    case GestureStatus.Completed:
                        _renderer.HandleSingleTouchUp(point);
                        break;
                }
            };

            GestureRecognizers.Add(pan);

            SetBinding(StateProperty, new Binding(nameof(State)));
        }     
    }