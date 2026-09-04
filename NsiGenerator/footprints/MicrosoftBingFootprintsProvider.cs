using Nsi.Geospatial.Io;

namespace NsiGenerator.Footprints;

/// <summary>
/// Microsoft Bing footprints, delivered as a
/// GeoJSON file per state.
/// </summary>
public sealed class MicrosoftBingFootprintsProvider : FootprintProvider
{
    public MicrosoftBingFootprintsProvider(
        string geojsonPath,
        IFeatureSource? reader = null,
        string name = "USbuildings",
        int priorityOrder = 3)
    {
        SourcePaths = [geojsonPath];
        Reader = reader ?? new SpatialReader();
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
    public IFeatureSource Reader { get; }
    /// <summary>No usable attributes in the source; everything is defaulted.</summary>
    public FootprintFieldMap FieldMap { get; } = FootprintFieldMap.Empty;
}