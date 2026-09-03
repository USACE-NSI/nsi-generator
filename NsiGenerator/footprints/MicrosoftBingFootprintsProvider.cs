namespace NsiGenerator.Footprints;

/// <summary>
/// Microsoft Bing footprints, delivered as a
/// GeoJSON file per state.
/// </summary>
public sealed class MicrosoftBingFootprintsProvider : FootprintProvider
{
    public MicrosoftBingFootprintsProvider(
        string geojsonPath,
        string name = "USbuildings",
        int priorityOrder = 3)
    {
        SourcePaths = [geojsonPath];
        Name = name;
        PriorityOrder = priorityOrder;
    }

    public string Name { get; }
    public int PriorityOrder { get; }
    public IEnumerable<string> SourcePaths { get; }
}