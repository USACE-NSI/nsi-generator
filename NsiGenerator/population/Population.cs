using Nsi.Geospatial.Geometry;

namespace NsiGenerator.Population;

/// Population categories that matter for exposure modeling.
public sealed record PopulationSet
{
    public int Working { get; init; }
    public int Students { get; init; }
    public int Teachers { get; init; }
}

/// A zonal space (e.g. census block/tract) with expected demographic composition.
public sealed record PopulationZone
{
    public required Feature Zone { get; init; }
    public int Over65 { get; init; }
    public int Under65 { get; init; }

    /// Population allocated to the zone when the dataset was produced.
    public PopulationSet Incoming { get; init; } = new();

    /// Population drawn down as it is assigned to structures within the zone.
    public PopulationSet Outgoing { get; init; } = new();
}

public sealed record PopulationDataset
{
    public IReadOnlyList<PopulationZone> PopulationZones { get; init; } = [];
}