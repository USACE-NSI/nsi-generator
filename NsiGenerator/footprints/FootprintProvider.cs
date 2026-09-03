using Nsi.Geospatial.Io;

namespace NsiGenerator.Footprints;

/// <summary>
/// Reads raw building-footprint geometries from one source on the local file
/// system and yields them as a standard <see cref="InputFootprintDataset"/>.
/// Each implementation supplies only its <see cref="Name"/>, <see cref="PriorityOrder"/>
/// and <see cref="SourcePaths"/>; the shared <see cref="Read()"/> default method
/// does the reading through the geospatial.io <see cref="IFeatureSource"/> (GDAL/OGR).
/// </summary>
public interface FootprintProvider
{
    /// <summary>The footprint dataset name, e.g. "USAStructures".</summary>
    string Name { get; }

    /// <summary>Lower value = higher priority when datasets overlap (see InputFootprintDataset).</summary>
    int PriorityOrder { get; }

    /// <summary>
    /// Path(s) to the raw local file(s) for this provider. Kept abstract so each
    /// source can express its own layout (per-county shapefile, per-state GeoJSON, ...).
    /// </summary>
    IEnumerable<string> SourcePaths { get; }

    /// <summary>
    /// The spatial reader used to decode the source files. Defaults to
    /// <see cref="SpatialReader"/>, but is injectable for tests.
    /// </summary>
    IFeatureSource Reader { get; }

    /// <summary>
    /// Reads every footprint from <see cref="SourcePaths"/> into a standard
    /// <see cref="InputFootprintDataset"/>. Shared by all providers.
    /// </summary>
    InputFootprintDataset Read()
    {
        return new InputFootprintDataset
        {
            Name = Name,
            PriorityOrder = PriorityOrder,
            Footprints = SourcePaths
                .SelectMany(path => Reader.Read(path).Features)
                .Select(f => new InputFootprint { Geometry = f })
                .ToList(),
        };
    }
}