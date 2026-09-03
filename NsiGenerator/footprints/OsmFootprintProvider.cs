using Nsi.Geospatial.Io;

namespace NsiGenerator.Footprints;

/// <summary>
/// OpenStreetMap building footprints. OSM is commonly exported to a local
/// shapefile or GeoJSON.
/// </summary>
public sealed class OsmFootprintProvider : FootprintProvider
{
    public OsmFootprintProvider(
        string path,
        IFeatureSource? reader = null,
        string name = "OpenStreetMap",
        int priorityOrder = 4)
    {
        SourcePaths = [path];
        Reader = reader ?? new SpatialReader();
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
    public IFeatureSource Reader { get; }
}