namespace Hawaii.Interfaces;

public interface ISceneService
{
    Transform GetTransform(Guid id);

    void SetTransform(Guid id, Transform transform);
    
    event Action<Guid>? TransformChanged;
}