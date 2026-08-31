using NetTopologySuite.Geometries;

namespace NsiGenerator.Footprints;

/// A footprint in the consolidated dataset. Height and square footage may be
/// missing until the "assign square footage and building heights" process runs.
public sealed record Footprint
{
    public required Polygon Geometry { get; init; }

    /// Winning source name after priority combination.
    public required string Source { get; init; }

    public double? BuildingHeight { get; init; }

    /// Derived from the polygon when the source does not provide it.
    public double? SquareFootage { get; init; }
}

/// Best-of-breed footprints for the geography.
public sealed record FootprintDataset
{
    public IReadOnlyList<Footprint> Footprints { get; init; } = [];
}