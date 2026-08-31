using NetTopologySuite.Geometries;

namespace NsiGenerator.Footprints;

/// Structure counts for a zonal space, derived independently of footprint records.
public sealed record StructureZonalStats
{
    public required Polygon Zone { get; init; }

    /// Expected structures in the zone that are NOT represented by footprints —
    /// the count anticipated to fall back to parcel centroids or other placement.
    public int StructureCount { get; init; }
}

public sealed record StructureZonalStatisticsDataset
{
    public IReadOnlyList<StructureZonalStats> Zones { get; init; } = [];
}