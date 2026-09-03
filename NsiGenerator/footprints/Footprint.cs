using Nsi.Geospatial.Geometry;

namespace NsiGenerator.Footprints;

public sealed record Footprint
{
    public required Feature Geometry { get; init; }
    public required string Source { get; init; }
    public double? BuildingHeight { get; init; }
    public double? SquareFootage { get; init; }
}

public sealed record FootprintDataset
{
    public IReadOnlyList<Footprint> Footprints { get; init; } = [];
}