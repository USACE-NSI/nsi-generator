namespace NsiGenerator.Footprints;

/// <summary>
/// ORNL "USA Structures" footprints, delivered as an Esri shapefile per county.
/// </summary>
public sealed class UsaStructuresFootprintProvider : FootprintProvider
{
    public UsaStructuresFootprintProvider(
        string shapefilePath,
        string name = "USAStructures",
        int priorityOrder = 2)
    {
        SourcePaths = [shapefilePath];
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
}