using Hawaii.Interfaces;

namespace Hawaii.Services;

public class SceneService : ISceneService
{
    private readonly Dictionary<Guid, Transform> _transforms = [];

    public Transform GetTransform(Guid id)
    {
        if (_transforms.TryGetValue(id, out var transform))
        {
            return transform;
        }
        
        var test = new Transform();
        return test;
    }

    public void SetTransform(Guid id, Transform transform)
    {
        _transforms[id] = transform ?? new Transform();
        TransformChanged?.Invoke(id);
    }

    public event Action<Guid>? TransformChanged;
}