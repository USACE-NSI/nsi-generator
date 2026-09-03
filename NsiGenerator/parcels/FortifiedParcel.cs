using Nsi.Geospatial.Geometry;

namespace NsiGenerator.Parcels;

/// Attributes for specialty buildings (schools, hospitals, prisons, ...).
/// Location/population fields are optional — many fortified parcels will not
/// have them at this stage.
public sealed record FortifiedStructureAttributes
{
    public int YearBuilt { get; init; }
    public BuildingTypeEnum BuildingType { get; init; }
    public double? X { get; init; }
    public double? Y { get; init; }
    public int? Students { get; init; }
    public int? Teachers { get; init; }
    public double SquareFootage { get; init; }
    public bool Basement { get; init; }
    public double NumStories { get; init; }
}

/// A specialty building joined onto a parcel.
public sealed record FortifiedBuilding
{
    public required OccupancyTypeEnum OccupancyType { get; init; }
    public required FortifiedStructureAttributes Attributes { get; init; }
}

/// A parcel joined with specialty building data.
public sealed record FortifiedParcel
{
    public required Feature ParcelGeometry { get; init; }
    public IReadOnlyList<FortifiedBuilding> Buildings { get; init; } = [];
}

public sealed record FortifiedParcelDataset
{
    public IReadOnlyList<FortifiedParcel> Parcels { get; init; } = [];
}