using Nsi.Geospatial.Geometry;
using NsiGenerator;
using NsiGenerator.Footprints;
using Xunit;

namespace NsiGenerator.Tests;

public class FootprintConsolidationTests
{
    // Builds a closed polygon Feature from an explicit ring, mirroring Program.Poly.
    private static Feature Poly(params (double X, double Y)[] ring)
    {
        var part = new Part();
        foreach (var (x, y) in ring)
            part.AddVertex(new Vertex(x, y));
        part.CloseRing();

        var feature = new Feature();
        feature.AddPart(part);
        feature.ComputeMbr();
        return feature;
    }

    private static InputFootprintDataset Dataset(string name, int priority, params Feature[] geometries) =>
        new()
        {
            Name = name,
            PriorityOrder = priority,
            Footprints = geometries
                .Select(g => new InputFootprint { Geometry = g })
                .ToList(),
        };

    private static readonly Feature UnitSquare = Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
    private static readonly Feature OffsetSquare = Poly((20, 20), (30, 20), (30, 30), (20, 30), (20, 20));

    [Fact]
    public void DistinctFootprints_AreAllPreserved()
    {
        var bing = Dataset("Bing", 1, UnitSquare);
        var usa = Dataset("USAStructures", 2, OffsetSquare);

        var result = Program.ConsolidateFootprints(new[] { bing, usa });

        Assert.Equal(2, result.Footprints.Count);
    }

    [Fact]
    public void DuplicateFootprint_HigherPriorityWins()
    {
        // Same geometry in both datasets; Bing (priority 1) must beat USAStructures (2).
        var bing = Dataset("Bing", 1, UnitSquare);
        var usa = Dataset("USAStructures", 2, UnitSquare);

        var result = Program.ConsolidateFootprints(new[] { bing, usa })
            .Footprints.Single();

        Assert.Equal("Bing", result.Source);
    }

    [Fact]
    public void DuplicateFootprint_GeometryComesFromWinner()
    {
        var bing = Dataset("Bing", 1, UnitSquare);
        var usa = Dataset("USAStructures", 2, UnitSquare);

        var result = Program.ConsolidateFootprints(new[] { bing, usa })
            .Footprints.Single();

        Assert.Equal(Program.RingKey(UnitSquare), Program.RingKey(result.Geometry));
    }

    [Fact]
    public void SquareFootage_IsComputedFromGeometryArea()
    {
        var bing = Dataset("Bing", 1, UnitSquare); // 10x10 => 100

        var result = Program.ConsolidateFootprints(new[] { bing })
            .Footprints.Single();

        Assert.Equal(100.0, result.SquareFootage, precision: 6);
    }

    [Fact]
    public void EmptyDatasets_ReturnEmptyResult()
    {
        var result = Program.ConsolidateFootprints(Array.Empty<InputFootprintDataset>());

        Assert.Empty(result.Footprints);
    }

    [Fact]
    public void DatasetsWithNoFootprints_ReturnEmptyResult()
    {
        var empty = new InputFootprintDataset { Name = "None", PriorityOrder = 1 };

        var result = Program.ConsolidateFootprints(new[] { empty });

        Assert.Empty(result.Footprints);
    }

    [Fact]
    public void SamePriority_TiesAreDeterministic()
    {
        var a = Dataset("A", 1, UnitSquare);
        var b = Dataset("B", 1, UnitSquare);

        var result = Program.ConsolidateFootprints(new[] { a, b })
            .Footprints.Single();

        // MinBy keeps the first of equal-priority candidates.
        Assert.Equal("A", result.Source);
    }
}
