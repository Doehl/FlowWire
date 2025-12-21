namespace FlowWire.SourceGenerators;

/// <summary>
/// Represents a method within an activity interface.
/// </summary>
internal sealed record ActivityMethodModel(
    string Name,
    string ReturnType,
    EquatableArray<ActivityParameterModel> Parameters,
    string? ActivityNameAttributeValue,
    bool HasReturnValue,
    string UnwrappedReturnType
);
