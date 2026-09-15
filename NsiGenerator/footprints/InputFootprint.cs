using Nsi.Geospatial.Geometry;

namespace NsiGenerator.Footprints;

/// <summary>
/// A single structure footprint from one raw source, already normalized: geometry
/// plus the standardized attributes every source can be reduced to. Source-level
/// metadata (name, priority) is carried by the owning dataset.
///
/// An IsDefault flag marks a value that was NOT measured in the source but filled in
/// by the provider. Later steps (reconcile missing footprints, QA/QC) use the flags
/// to prefer measured values or to re-derive them.
/// </summary>
public sealed record InputFootprint
{
    public required Feature Geometry { get; init; }

    /// <summary>Floor area in square feet. Null only when neither mapped nor derivable from geometry.</summary>
    public double? SquareFootage { get; init; }

    /// <summary>True when SquareFootage came from a provider default, not the source.</summary>
    public bool SquareFootageIsDefault { get; init; }

    /// <summary>Height in meters (NSI bldheight). Always populated: mapped value or default.</summary>
    public required double BuildingHeight { get; init; }

    /// <summary>True when BuildingHeight is the provider default rather than a measured value.</summary>
    public bool BuildingHeightIsDefault { get; init; }

    /// <summary>Above-ground stories, when the source carries them. Not defaulted.</summary>
    public double? Levels { get; init; }

}

public sealed record InputFootprintDataset
{
    public required string Name { get; init; }
    public required int PriorityOrder { get; init; }
    public IReadOnlyList<InputFootprint> Footprints { get; init; } = [];
}