namespace NsiGenerator.Footprints;

/// <summary>A standardized footprint attribute that any source can supply.</summary>
public enum StandardFootprintField
{
    /// <summary>Floor area of the structure, canonicalized to square feet.</summary>
    SquareFootage,

    /// <summary>Structure height, canonicalized to meters (NSI bldheight).</summary>
    BuildingHeight,

    /// <summary>Number of above-ground stories; used to estimate height when absent.</summary>
    BuildingLevels,
}

/// <summary>Unit the source column is expressed in. Values are converted to canonical units.</summary>
public enum ValueUnit
{
    None,
    SquareFeet,
    SquareMeters,
    Feet,
    Meters,
    StoryCount,
}

/// <summary>
/// One source column bound to a standardized field. Several bindings may target the
/// same field; the first column actually present in the file wins, which lets a
/// provider tolerate schema drift between vintages of the same dataset.
/// </summary>
public sealed record FieldBinding(
    string SourceColumn,
    StandardFootprintField Field,
    ValueUnit Unit = ValueUnit.None);

/// <summary>
/// Maps column names in an incoming spatial file to the standardized fields on a
/// Footprint. Column lookup is case-insensitive because OGR preserves the casing of
/// each source (shapefiles tend to be upper case, GeoJSON lower case).
/// </summary>
public sealed class FootprintFieldMap
{
    public static FootprintFieldMap Empty { get; } = new([]);

    private readonly Dictionary<StandardFootprintField, List<FieldBinding>> _byField;

    private FootprintFieldMap(IReadOnlyList<FieldBinding> bindings)
    {
        Bindings = bindings;
        _byField = [];
        foreach (FieldBinding b in bindings)
        {
            if (!_byField.TryGetValue(b.Field, out List<FieldBinding>? list))
            {
                list = [];
                _byField[b.Field] = list;
            }
            list.Add(b);
        }
    }

    /// <summary>All bindings, in declaration order.</summary>
    public IReadOnlyList<FieldBinding> Bindings { get; }

    /// <summary>Candidate columns for a field, in preference order.</summary>
    public IReadOnlyList<FieldBinding> Candidates(StandardFootprintField field) =>
        _byField.TryGetValue(field, out List<FieldBinding>? list) ? list : [];
/// <summary>
/// Builds a map from bindings in preference order. The first parameter is explicit
/// (rather than a bare params array) so a single binding can still use target-typed
/// new(): with params FieldBinding[] alone, one argument binds to the normal form and
/// the compiler tries to construct the array itself (CS8752).
/// </summary>
public static FootprintFieldMap Build(FieldBinding first, params FieldBinding[] rest) =>
    new([first, .. rest]);
}