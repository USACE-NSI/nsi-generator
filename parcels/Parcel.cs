using NetTopologySuite.Geometries;

namespace NsiGenerator.Parcels;

/// Minimum parcel attributes necessary for a parcel to be viable in
/// generation of a fortified parcel dataset.
public sealed record ParcelAttributes
{
    public int YearBuilt { get; init; }
    public BuildingTypeEnum UseType { get; init; }
    public double SquareFootage { get; init; }
    public bool Basement { get; init; }
    public double NumStories { get; init; }
}

/// A parcel in the raw input.
public sealed record InputParcel
{
    public required Polygon ParcelGeometry { get; init; }
    public required ParcelAttributes Attributes { get; init; }
}

public sealed record InputParcelDataset
{
    public IReadOnlyList<InputParcel> Parcels { get; init; } = [];
}