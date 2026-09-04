using System.Globalization;
using Nsi.Geospatial.Geometry;
using Nsi.Geospatial.Io;

namespace NsiGenerator.Footprints;

/// <summary>
/// Reads raw building-footprint geometries from one source on the local file system
/// and yields them as a standard <see cref="InputFootprintDataset"/>.
///
/// Providers tailor themselves to their source with a single declaration,
/// <see cref="FieldMap"/>: which source columns carry square footage / height /
/// levels, and in what units. Anything the source does not supply is filled with a
/// provider default and flagged (see <see cref="InputFootprint.SquareFootageIsDefault"/>),
/// so every provider emits the same normalized shape without per-source logic.
///
/// Read() is a default interface method: call it through <see cref="FootprintProvider"/>,
/// not through the concrete class.
/// </summary>
public interface FootprintProvider
{
    /// <summary>The footprint dataset name, e.g. "USAStructures".</summary>
    string Name { get; }

    /// <summary>Lower value = higher priority where datasets overlap (see InputFootprintDataset).</summary>
    int PriorityOrder { get; }

    /// <summary>Path(s) to the raw local file(s): per-county shapefile, per-state GeoJSON, ...</summary>
    IEnumerable<string> SourcePaths { get; }

    /// <summary>Source column -> standardized field, with the units each column is in.</summary>
    FootprintFieldMap FieldMap { get; }

    /// <summary>Reader used to decode the files; <see cref="SpatialReader"/> in production.</summary>
    IFeatureSource Reader { get; }

    /// <summary>
    /// Height in meters applied when the source carries none (NSI bldheight is
    /// meters). Roughly a single story. Overridable per provider.
    /// </summary>
    double DefaultBuildingHeight => 3.0;

    /// <summary>
    /// Reads every footprint from <see cref="SourcePaths"/>, normalizing each feature
    /// through <see cref="Normalize"/>. Shared by all providers.
    /// </summary>
    InputFootprintDataset Read()
    {
        List<InputFootprint> footprints = [];
        foreach (string path in SourcePaths)
        {
            foreach (Feature feature in Reader.Read(path).Features)
            {
                footprints.Add(Normalize(feature));
            }
        }

        return new InputFootprintDataset
        {
            Name = Name,
            PriorityOrder = PriorityOrder,
            Footprints = footprints,
        };
    }

    /// <summary>
    /// Turns one raw feature into a normalized <see cref="InputFootprint"/>. Square
    /// footage falls back to the footprint's own area; height falls back to
    /// <see cref="DefaultBuildingHeight"/>. Both fall back with the IsDefault flag set.
    /// </summary>
    InputFootprint Normalize(Feature feature)
    {
        double? squareFootage = ReadMapped(StandardFootprintField.SquareFootage, feature);
        bool squareFootageIsDefault = squareFootage is null;
        squareFootage ??= ExteriorAreaSquareFeet(feature);

        double? buildingHeight = ReadMapped(StandardFootprintField.BuildingHeight, feature);
        bool buildingHeightIsDefault = buildingHeight is null;

        return new InputFootprint
        {
            Geometry = feature,
            SquareFootage = squareFootage,
            SquareFootageIsDefault = squareFootageIsDefault,
            BuildingHeight = buildingHeight ?? DefaultBuildingHeight,
            BuildingHeightIsDefault = buildingHeightIsDefault,
            Levels = ReadMapped(StandardFootprintField.BuildingLevels, feature),
        };
    }

    /// <summary>
    /// First candidate column present in the file, canonicalized to square feet /
    /// meters / story count. Absent, unparseable and non-positive values all read as
    /// null so the fallback gets a chance (OGR nulls and 0 sentinels both mean unknown).
    /// </summary>
    double? ReadMapped(StandardFootprintField field, Feature feature)
    {
        const double SquareMetersToSquareFeet = 10.763910416709722;
        const double FeetToMeters = 0.3048;

        foreach (FieldBinding binding in FieldMap.Candidates(field))
        {
            if (!feature.Attributes.TryGetValue(binding.SourceColumn, out object? raw) || raw is null)
            {
                continue;
            }

            double? value = raw switch
            {
                double d => d,
                float f => f,
                int i => i,
                long l => l,
                // Shapefile date fields and GeoJSON tags both arrive as text.
                string s when double.TryParse(
                    s, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) => parsed,
                _ => null,
            };

            if (value is null || double.IsNaN(value.Value) || value.Value <= 0)
            {
                continue;
            }

            return binding.Unit switch
            {
                ValueUnit.SquareMeters => value.Value * SquareMetersToSquareFeet,
                ValueUnit.Feet => value.Value * FeetToMeters,
                _ => value.Value, // SquareFeet, Meters, StoryCount, None
            };
        }

        return null;
    }

    /// <summary>
    /// Exterior ring area minus interior rings (holes). Only meaningful when the
    /// geometry is in a linear CRS whose unit is feet — see the EPSG:4326 caveat in
    /// the notes. Returns null for degenerate rings.
    /// </summary>
    static double? ExteriorAreaSquareFeet(Feature feature)
    {
        if (feature.Parts.Count == 0 || feature.Parts[0].Vertices.Count < 3) return null;

        double area = feature.Parts[0].Area;
        for (int i = 1; i < feature.Parts.Count; i++)
        {
            area -= feature.Parts[i].Area;
        }

        return area > 0 ? area : null;
    }
}