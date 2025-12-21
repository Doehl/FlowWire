namespace FlowWire.SourceGenerators;

/// <summary>
/// Represents a parameter of an activity method.
/// </summary>
internal sealed record ActivityParameterModel(
    string Name,
    string Type
);
