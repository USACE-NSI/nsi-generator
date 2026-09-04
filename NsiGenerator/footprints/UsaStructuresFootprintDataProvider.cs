using Nsi.Geospatial.Io;

namespace NsiGenerator.Footprints;

/// <summary>
/// ORNL "USA Structures" footprints, delivered as an Esri shapefile per county.
/// </summary>
public sealed class UsaStructuresFootprintProvider : FootprintProvider
{
    public UsaStructuresFootprintProvider(
        string shapefilePath,
        IFeatureSource? reader = null,
        string name = "USAStructures",
        int priorityOrder = 2)
    {
        SourcePaths = [shapefilePath];
        Reader = reader ?? new SpatialReader();
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
    public IFeatureSource Reader { get; }
    public FootprintFieldMap FieldMap { get; } = FootprintFieldMap.Build(
        new("HEIGHT", StandardFootprintField.BuildingHeight, ValueUnit.Meters));
}