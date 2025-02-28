using Hawaii.Interfaces;
using Hawaii.Services;

namespace Hawaii;

public class SceneGraphicsView<TBuilder> : GraphicsView
    where TBuilder : class, ISceneBuilder
    {
        private readonly SceneRenderer _renderer;
        
        private readonly IServiceScope _scope;

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

            _scope = services.CreateScope();
            _renderer = _scope.ServiceProvider.GetRequiredService<SceneRenderer>();
            
            _renderer.GraphicsView = this;
            Drawable = _renderer;
            
            StartInteraction += (sender, e) => HandleStartInteraction(e);
            DragInteraction += (sender, e) => HandleDragInteraction(e);
            EndInteraction += (sender, e) => HandleEndInteraction(e);

            SetBinding(StateProperty, new Binding(nameof(State)));
        }
        
        private void HandleStartInteraction(TouchEventArgs e)
        {
            if (e.Touches.Length == 1)
                _renderer.HandleSingleTouchDown(e.Touches[0]);
            else if (e.Touches.Length == 2)
                _renderer.HandleTwoFingerDown(e.Touches[0], e.Touches[1]);
        }

        private void HandleDragInteraction(TouchEventArgs e)
        {
            if (e.Touches.Length == 1)
                _renderer.HandleSingleTouchMove(e.Touches[0]);
            else if (e.Touches.Length == 2)
                _renderer.HandleTwoFingerMove(e.Touches[0], e.Touches[1]);
            
            Invalidate();
        }

        private void HandleEndInteraction(TouchEventArgs e)
        {
            if (e.Touches.Length == 1)
                _renderer.HandleSingleTouchUp(e.Touches[0]);
            else if (e.Touches.Length == 2)
                _renderer.HandleTwoFingerUp(e.Touches[0], e.Touches[1]);
            
            Invalidate();
        }
    }