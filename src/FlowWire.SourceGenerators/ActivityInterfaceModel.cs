namespace FlowWire.SourceGenerators;

/// <summary>
/// Represents the metadata of an interface decorated with [Activity].
/// </summary>
internal sealed record ActivityInterfaceModel(
    string Namespace,
    string InterfaceName,
    EquatableArray<ActivityMethodModel> Methods
);


