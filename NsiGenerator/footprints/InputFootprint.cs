using Nsi.Geospatial.Geometry;

namespace NsiGenerator.Footprints;

/// A single structure footprint from one raw source. Inputs are geometry-only;
/// source-level metadata is carried by the owning dataset.
public sealed record InputFootprint
{
    public required Feature Geometry { get; init; }
}

/// One raw source of footprints. PriorityOrder decides which dataset wins when
/// several datasets carry the same footprint in the same geography.
/// Lower value = higher priority.
public sealed record InputFootprintDataset
{
    public required string Name { get; init; }
    public required int PriorityOrder { get; init; }
    public IReadOnlyList<InputFootprint> Footprints { get; init; } = [];
}