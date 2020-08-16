using GamecraftModdingAPI;

namespace Pixi.Common
{
    /// <summary>
    /// Thing importer.
    /// This imports the thing by converting it to a common block format that Pixi can understand.
    /// </summary>
    public interface Importer
    {
        int Priority { get; }
        
        bool Optimisable { get; }
        
        string Name { get; }
        
        BlueprintProvider BlueprintProvider { get; }
        
        bool Qualifies(string name);

        BlockJsonInfo[] Import(string name);

        void PreProcess(string name, ref ProcessedVoxelObjectNotation[] blocks);

        void PostProcess(string name, ref Block[] blocks);
    }
}