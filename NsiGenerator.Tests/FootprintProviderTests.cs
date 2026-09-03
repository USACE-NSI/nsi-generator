using Nsi.Geospatial.Geometry;
using Nsi.Geospatial.Io;
using NsiGenerator.Footprints;
using Xunit;

namespace NsiGenerator.Tests;

/// <summary>
/// Unit tests for the <see cref="FootprintProvider"/> default Read() and the three
/// concrete providers. The reader is faked so no real spatial files or GDAL are
/// required. Note: Read() is a default interface method, so the receiver must be
/// typed as <see cref="FootprintProvider"/> (the interface).
/// </summary>
public class FootprintProviderTests
{
    private sealed class FakeFeatureSource : IFeatureSource
    {
        private readonly Dictionary<string, FeatureCollection> _collections = new();

        public void Add(string path, FeatureCollection collection) => _collections[path] = collection;

        // Lenient: unknown paths yield an empty collection instead of throwing.
        public FeatureCollection Read(string path) =>
            _collections.TryGetValue(path, out var collection) ? collection : new FeatureCollection();
    }

    private static Feature Poly(params (double X, double Y)[] ring)
    {
        var part = new Part();
        foreach (var (x, y) in ring) part.AddVertex(new Vertex(x, y));
        part.CloseRing();
        var feature = new Feature();
        feature.AddPart(part);
        feature.ComputeBoundingBox();
        return feature;
    }

    private static FeatureCollection Collection(params Feature[] features)
    {
        var fc = new FeatureCollection();
        foreach (var f in features) fc.AddFeature(f);
        return fc;
    }

    [Fact]
    public void Read_ReturnsDatasetWithConfiguredNameAndPriority()
    {
        var reader = new FakeFeatureSource();
        reader.Add("county.shp", Collection(Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0))));

        FootprintProvider provider = new UsaStructuresFootprintProvider(
            "county.shp", reader: reader, name: "USAStructures", priorityOrder: 2);
        var dataset = provider.Read();

        Assert.Equal("USAStructures", dataset.Name);
        Assert.Equal(2, dataset.PriorityOrder);
    }

    [Fact]
    public void Read_ReadsAllFeaturesFromSingleSource()
    {
        var reader = new FakeFeatureSource();
        reader.Add("state.geojson", Collection(
            Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0)),
            Poly((20, 20), (30, 20), (30, 30), (20, 30), (20, 20))));

        FootprintProvider provider = new MicrosoftBingFootprintsProvider("state.geojson", reader: reader);
        var dataset = provider.Read();

        Assert.Equal(2, dataset.Footprints.Count);
        Assert.All(dataset.Footprints, f => Assert.NotNull(f.Geometry));
    }

    [Fact]
    public void Read_PreservesGeometryFromSource()
    {
        var reader = new FakeFeatureSource();
        reader.Add("county.shp", Collection(Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0))));

        FootprintProvider provider = new UsaStructuresFootprintProvider("county.shp", reader: reader);
        var footprint = provider.Read().Footprints.Single();

        var ring = footprint.Geometry.Parts[0].Vertices;
        Assert.Equal(0, ring[0].X);
        Assert.Equal(0, ring[0].Y);
        Assert.True(footprint.Geometry.Parts[0].Area > 0);
    }

    [Fact]
    public void Read_UnknownSource_ReturnsEmptyFootprints()
    {
        // A provider whose source path has no registered features yields an empty dataset.
        var reader = new FakeFeatureSource();
        FootprintProvider provider = new OsmFootprintProvider("missing.geojson", reader: reader);

        var dataset = provider.Read();

        Assert.Empty(dataset.Footprints);
        Assert.Equal("OpenStreetMap", dataset.Name);
    }

    [Fact]
    public void Providers_ExposeDefaultNamesAndPriorities()
    {
        var reader = new FakeFeatureSource();

        var usa = new UsaStructuresFootprintProvider("county.shp", reader: reader);
        var ms = new MicrosoftBingFootprintsProvider("state.geojson", reader: reader);
        var osm = new OsmFootprintProvider("region.geojson", reader: reader);

        Assert.Equal("USAStructures", usa.Name);
        Assert.Equal(2, usa.PriorityOrder);

        Assert.Equal("USbuildings", ms.Name);
        Assert.Equal(3, ms.PriorityOrder);

        Assert.Equal("OpenStreetMap", osm.Name);
        Assert.Equal(4, osm.PriorityOrder);
    }
}