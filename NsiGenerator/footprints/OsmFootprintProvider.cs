namespace NsiGenerator.Footprints;

/// <summary>
/// OpenStreetMap building footprints. OSM is commonly exported to a local
/// shapefile or GeoJSON.
/// </summary>
public sealed class OsmFootprintProvider : FootprintProvider
{
    public OsmFootprintProvider(
        string path,
        string name = "OpenStreetMap",
        int priorityOrder = 4)
    {
        SourcePaths = [path];
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
}