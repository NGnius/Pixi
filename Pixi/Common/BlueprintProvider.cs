namespace Pixi.Common
{
    public interface BlueprintProvider
    {
        string Name { get; }
        
        BlockJsonInfo[] Blueprint(string name, BlockJsonInfo root);
    }
}