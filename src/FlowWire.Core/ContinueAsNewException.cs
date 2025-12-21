namespace FlowWire;

public sealed class ContinueAsNewException : Exception
{
    public object? NewInput { get; }

    public ContinueAsNewException(object? newInput = null) : base("Workflow continuing as new.")
    {
        NewInput = newInput;
    }
}
