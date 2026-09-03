using Nsi.Geospatial.Geometry;
using NsiGenerator.Footprints;
using Xunit;

namespace NsiGenerator.Tests;

/// <summary>
/// File-backed tests for the footprint providers using real spatial fixtures under
/// ../data (relative to this test project). These exercise the actual GDAL/OGR
/// read path via Nsi.Geospatial.Io, so they are tagged "Gdal" and can be
/// excluded on remote/CI runs that lack the native GDAL runtime.
/// </summary>
[Trait("Category", "Gdal")]
public class FootprintProviderFileTests
{
    // Resolve ../data relative to the test assembly output directory.
    private static readonly string DataRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data"));

    [Fact]
    public void Read_MicrosoftVermont_ReturnsFootprints()
    {
        var path = Path.Combine(DataRoot, "bing", "Vermont.geojson");
        Assert.True(File.Exists(path), $"Missing fixture: {path}");

        FootprintProvider provider = new MicrosoftBingFootprintsProvider(path);
        var dataset = provider.Read();

        Assert.Equal("USbuildings", dataset.Name);
        Assert.NotEmpty(dataset.Footprints);
        Assert.All(dataset.Footprints, f =>
        {
            Assert.NotNull(f.Geometry);
            Assert.NotEmpty(f.Geometry.Parts);
        });
    }

    [Fact]
    public void Read_OrnlVermontCounty_ReturnsFootprints()
    {
        // All county FIPS for Vermont.
        string[] countyFips =
        {
            "50001", "50003", "50005", "50007", "50009",
            "50011", "50013", "50015", "50017", "50019",
            "50021", "50023", "50025", "50027",
        };

        int total = 0;
        foreach (var fips in countyFips)
        {
            var path = Path.Combine(DataRoot, "ornl", "vt", $"{fips}.shp");
            if (!File.Exists(path))
            {
                // Allow a partially-present dataset; skip missing counties rather than fail.
                continue;
            }

            FootprintProvider provider = new UsaStructuresFootprintProvider(path);
            var dataset = provider.Read();

            Assert.Equal("USAStructures", dataset.Name);
            Assert.NotEmpty(dataset.Footprints);
            total += dataset.Footprints.Count;
        }

        Assert.True(total > 0, "No ORNL Vermont county shapefiles were found under ../data/ornl/vt/");
    }

    [Fact]
    public void Read_OrnlSingleCounty_ReturnsFootprints()
    {
        var path = Path.Combine(DataRoot, "ornl", "vt", "50001.shp");
        Assert.True(File.Exists(path), $"Missing fixture: {path}");

        FootprintProvider provider = new UsaStructuresFootprintProvider(path);
        var dataset = provider.Read();

        Assert.Equal("USAStructures", dataset.Name);
        Assert.Equal(2, dataset.PriorityOrder);
        Assert.NotEmpty(dataset.Footprints);
    }

    [Fact]
    public void Read_OsmVermont_ReturnsFootprints()
    {
        // OSM fixtures commonly live at ../data/osm/... — adjust the name to your fixture.
        var path = Path.Combine(DataRoot, "osm", "vermont.geojson");
        Assert.True(File.Exists(path), $"Missing fixture: {path}");

        FootprintProvider provider = new OsmFootprintProvider(path);
        var dataset = provider.Read();

        Assert.Equal("OpenStreetMap", dataset.Name);
        Assert.NotEmpty(dataset.Footprints);
    }

    [Fact]
    public void Read_AllProviders_ProduceDatasetsForConsolidation()
    {
        var providers = new FootprintProvider[]
        {
            new UsaStructuresFootprintProvider(Path.Combine(DataRoot, "ornl", "vt", "50001.shp")),
            new MicrosoftBingFootprintsProvider(Path.Combine(DataRoot, "bing", "Vermont.geojson")),
        };

        var datasets = providers.Select(p => p.Read()).ToList();

        Assert.NotEmpty(datasets);
        Assert.All(datasets, d => Assert.NotNull(d.Name));
        Assert.True(datasets.Sum(d => d.Footprints.Count) > 0);
    }
}