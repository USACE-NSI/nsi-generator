using Nsi.Geospatial.Geometry;
using NsiGenerator.Footprints;
using NsiGenerator.Parcels;
using NsiGenerator.Population;

namespace NsiGenerator;

public static class Program
{
    public static void Main()
    {
        // Build a polygon Feature from an explicit ring of (X, Y) coordinates.
        Feature Poly(params (double X, double Y)[] ring)
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

        // ---- 1. Footprints: combine input datasets by priority ----
        var bing = new InputFootprintDataset
        {
            Name = "Bing",
            PriorityOrder = 1,
            Footprints =
            [
                new InputFootprint { Geometry = Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0)) },
                new InputFootprint { Geometry = Poly((20, 20), (30, 20), (30, 30), (20, 30), (20, 20)) },
            ],
        };
        var usaStructures = new InputFootprintDataset
        {
            Name = "USAStructures",
            PriorityOrder = 2,
            Footprints =
            [
                // Same footprint as Bing's first; Bing (priority 1) wins.
                new InputFootprint { Geometry = Poly((0, 0), (10, 0), (10, 10), (0, 10), (0, 0)) },
                new InputFootprint { Geometry = Poly((40, 0), (50, 0), (50, 10), (40, 10), (40, 0)) },
            ],
        };

        var consolidated = ConsolidateFootprints([bing, usaStructures]);
        Console.WriteLine($"Footprint dataset: {consolidated.Footprints.Count} footprints");
        foreach (var f in consolidated.Footprints)
            Console.WriteLine($" source={f.Source,-12} squareFootage={f.SquareFootage:F0}");

        // ---- 2. Fortified parcel dataset ----
        var fortified = new FortifiedParcelDataset
        {
            Parcels =
            [
                new FortifiedParcel
                {
                    ParcelGeometry = Poly((0, 0), (100, 0), (100, 100), (0, 100), (0, 0)),
                    Buildings =
                    [
                        new FortifiedBuilding
                        {
                            OccupancyType = OccupancyTypeEnum.EDU1,
                            Attributes = new FortifiedStructureAttributes
                            {
                                YearBuilt = 1998,
                                BuildingType = BuildingTypeEnum.Wood,
                                SquareFootage = 42_000,
                                NumStories = 2,
                                Students = 600,
                                Teachers = 40,
                            },
                        },
                    ],
                },
            ],
        };
        var school = fortified.Parcels[0].Buildings[0];
        Console.WriteLine($"\nFortified parcels: {fortified.Parcels.Count}, " +
            $"buildings: {fortified.Parcels.Sum(p => p.Buildings.Count)}");
        Console.WriteLine($" {school.OccupancyType} yearBuilt={school.Attributes.YearBuilt} " +
            $"students={school.Attributes.Students} teachers={school.Attributes.Teachers}");

        // ---- 3. Population dataset ----
        var population = new PopulationDataset
        {
            PopulationZones =
            [
                new PopulationZone
                {
                    Zone = Poly((0, 0), (100, 0), (100, 100), (0, 100), (0, 0)),
                    Over65 = 120,
                    Under65 = 1_480,
                    Incoming = new PopulationSet { Working = 800, Students = 600, Teachers = 40 },
                },
            ],
        };
        var zone = population.PopulationZones[0];
        Console.WriteLine($"\nPopulation zones: {population.PopulationZones.Count}");
        Console.WriteLine($" over65={zone.Over65} under65={zone.Under65} " +
            $"incoming working={zone.Incoming.Working}");

        // ---- 4. Zonal structure statistics ----
        var zonal = new StructureZonalStatisticsDataset
        {
            Zones =
            [
                new StructureZonalStats
                {
                    Zone = Poly((0, 0), (100, 0), (100, 100), (0, 100), (0, 0)),
                    StructureCount = 3, // expected structures not covered by footprints
                },
            ],
        };
        Console.WriteLine($"Zonal structure stats: {zonal.Zones.Sum(z => z.StructureCount)} " +
            "structures to place by fallback");

        Console.WriteLine("\nNext: DropPoints -> AssignPopulation -> floodplain/year-built " +
            "adjustments -> QA/QC (see workflow.md).");
    }

    /// Combines input footprint datasets: the same geometry in multiple datasets
    /// resolves to the dataset with the best (lowest) PriorityOrder.
    static FootprintDataset ConsolidateFootprints(IReadOnlyList<InputFootprintDataset> datasets)
    {
        return new FootprintDataset
        {
            Footprints = datasets
                .SelectMany(d => d.Footprints.Select(f => (d, f)))
                .GroupBy(t => RingKey(t.f.Geometry))
                .Select(g =>
                {
                    var winner = g.MinBy(t => t.d.PriorityOrder);
                    return new Footprint
                    {
                        Geometry = winner!.f.Geometry,
                        Source = winner.d.Name,
                        // Square footage from the exterior ring's area; building
                        // height is assigned by a later process when the source lacks it.
                        SquareFootage = winner.f.Geometry.Parts.Count > 0
                            ? winner.f.Geometry.Parts[0].Area
                            : 0,
                    };
                })
                .ToList(),
        };
    }

    /// A stable dedupe key for a polygon Feature: the exterior ring's coordinates.
    static string RingKey(Feature feature)
    {
        var ring = feature.Parts.Count > 0 ? feature.Parts[0].Vertices : [];
        return string.Join(";", ring.Select(v => $"{v.X},{v.Y}"));
    }
}