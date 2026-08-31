namespace NsiGenerator.Inventory;

/// <summary>
/// A single structure record in the NSI inventory — the final output schema of the
/// generator (workflow step 8, QA/QC → NSI). Properties are the PascalCase form of
/// the snake_case field names in the NSI 2026 "Public Fields" table
/// (Technical Documentation, https://www.hec.usace.army.mil/confluence/nsi/technicalreferences/latest/technical-documentation).
/// "Single" types in the schema map to float; "Double" to double; "Integer" to int.
/// Nullable fields are ones the schema can legitimately leave empty
/// (e.g. firmzone is null when no FIRM zone applies).
/// </summary>
public sealed record Structure
{
    // ── Identity and location ─────────────────────────────────────────────
    /// <summary>fd_id — unique for all structures.</summary>
    public required int FdId { get; init; }

    /// <summary>bid — building ID: NAD-rotated bounding box of the footprint (centroid + four cardinal extents).</summary>
    public required string Bid { get; init; }

    /// <summary>x — X coordinate of the structure, GCS WGS84.</summary>
    public required double X { get; init; }

    /// <summary>y — Y coordinate of the structure, GCS WGS84.</summary>
    public required double Y { get; init; }

    /// <summary>cbfips — Census Block containing the structure (2020 census blocks).</summary>
    public required string CbFips { get; init; }

    // ── Classification ────────────────────────────────────────────────────
    /// <summary>st_damcat — damage category, a larger aggregation than occupancy type (Residential, Commercial, Industrial, Public).</summary>
    public required string StDamCat { get; init; }

    /// <summary>occtype — damage function / occupancy type (e.g. RES1-2SNB, COM1, EDU1).</summary>
    public required string OccType { get; init; }

    /// <summary>bldgtype — building type / exterior wall (M = Masonry, W = Wood, H = Manufactured, S = Steel).</summary>
    public required string BldgType { get; init; }

    /// <summary>source — source of the initial iteration of the structure (P = Parcel, H = HIFLD).</summary>
    public required string Source { get; init; }

    // ── Footprint linkage ─────────────────────────────────────────────────
    /// <summary>ftprntid — identifier of the footprint record used to estimate sqft/num_story; stacked structures share one.</summary>
    public string? FtPrntId { get; init; }

    /// <summary>ftprntsrc — source of the utilized footprint (Bing, Oak Ridge National Labs, National Geospatial-Intelligence Agency).</summary>
    public string? FtPrntSrc { get; init; }

    /// <summary>ftprntsqft — square footage of the footprint polygon used during NSI generation (moved from private to public).</summary>
    public float? FtPrntSqft { get; init; }

    /// <summary>bldheight — reported building height in meters from the footprint source (moved from private to public).</summary>
    public float? BldHeight { get; init; }

    /// <summary>usastrucid — ID from USA Structures, when a USA Structures footprint was used.</summary>
    public int? UsaStructId { get; init; }

    // ── Physical characteristics ──────────────────────────────────────────
    /// <summary>sqft — estimated square footage of the structure; drives the depreciated replacement value.</summary>
    public float? Sqft { get; init; }

    /// <summary>num_story — number of stories.</summary>
    public float? NumStory { get; init; }

    /// <summary>found_type — foundation type (C = Crawl, B = Basement, S = Slab, P = Pier, I = Pile, F = Fill, W = Solid Wall).</summary>
    public string? FoundType { get; init; }

    /// <summary>found_ht — foundation height in feet from ground elevation.</summary>
    public float? FoundHt { get; init; }

    /// <summary>resunits — estimated number of housing units at the structure (moved from private to public).</summary>
    public int? ResUnits { get; init; }

    /// <summary>med_yr_blt — median year built of structures within the Census tract.</summary>
    public int? MedYrBlt { get; init; }

    // ── Economic value (August 2025 price levels) ─────────────────────────
    /// <summary>val_struct — depreciated replacement value of the structure, in dollars.</summary>
    public float? ValStruct { get; init; }

    /// <summary>val_cont — depreciated value of the contents, in dollars.</summary>
    public float? ValCont { get; init; }

    /// <summary>val_vehic — depreciated value of the vehicles at the structure, in dollars.</summary>
    public float? ValVehic { get; init; }

    /// <summary>fullrep — full (not depreciated) replacement value of the structure record.</summary>
    public float? FullRep { get; init; }

    /// <summary>depindex — Census tract depreciation index percentile; factor when depreciating from full replacement value.</summary>
    public float? DepIndex { get; init; }

    // ── Population ────────────────────────────────────────────────────────
    /// <summary>pop2amu65 — night population of the structure, under 65.</summary>
    public int Pop2AmU65 { get; init; }

    /// <summary>pop2amo65 — night population of the structure, over 65.</summary>
    public int Pop2AmO65 { get; init; }

    /// <summary>pop2pmu65 — day population of the structure, under 65.</summary>
    public int Pop2PmU65 { get; init; }

    /// <summary>pop2pmo65 — day population of the structure, over 65.</summary>
    public int Pop2PmO65 { get; init; }

    /// <summary>students — students attending the school per NCES data; 0 for non-schools.</summary>
    public int Students { get; init; }

    /// <summary>ornl_med — ORNL lab median population estimate, when available (from USA Structures footprints).</summary>
    public int? OrnlMed { get; init; }

    /// <summary>ornl_low — ORNL lab 5th percentile population estimate, when available.</summary>
    public int? OrnlLow { get; init; }

    /// <summary>ornl_hgh — ORNL lab 95th percentile population estimate, when available.</summary>
    public int? OrnlHgh { get; init; }

    // ── Tract/state context (accessibility, vehicles, vulnerability) ──────
    /// <summary>o65disable — percent of the county population over 65 expected to have an ambulatory disability.</summary>
    public float? O65Disable { get; init; }

    /// <summary>u65disable — percent of the county population under 65 expected to have an ambulatory disability.</summary>
    public float? U65Disable { get; init; }

    /// <summary>novehprob — percent of households in the Census tract without access to a vehicle.</summary>
    public float? NoVehProb { get; init; }

    /// <summary>vehperunit — average number of vehicles per household within the Census block.</summary>
    public float? VehPerUnit { get; init; }

    /// <summary>pctlowclr — estimated percent of vehicles in the state that are low clearance.</summary>
    public float? PctLowClr { get; init; }

    /// <summary>creprcnt — percent of tract households with 3+ indicators of vulnerability (Census CRE).</summary>
    public float? CrePrcnt { get; init; }

    /// <summary>crerank — percentile of the tract nationwide for the CRE 3-indicator estimate.</summary>
    public float? CreRank { get; init; }

    // ── Flood mapping and elevation ───────────────────────────────────────
    /// <summary>firmzone — estimated 2025 flood zone for the structure.</summary>
    public string? FirmZone { get; init; }

    /// <summary>zone_sub — FIRM flood zone sub type (last spatially joined in 2025).</summary>
    public string? ZoneSub { get; init; }

    /// <summary>static_bfe — FIRM Base Flood Elevation (last spatially joined in 2025).</summary>
    public int? StaticBfe { get; init; }

    /// <summary>grnd_elv_m — ground elevation in meters, NAVD88, at the structure.</summary>
    public float? GrndElvM { get; init; }

    /// <summary>ground_elv — ground elevation in feet, NAVD88, at the structure.</summary>
    public float? GroundElv { get; init; }
}