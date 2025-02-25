namespace Hawaii.Interfaces;

public interface ISceneBuilder
{
    Scene Build(INodeState state);
}