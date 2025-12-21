namespace FlowWire;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowUpdateAttribute : Attribute
{
    public string? Name { get; init; }
}
