namespace FlowWire.Engine.Exceptions;

public class FlowWireException : Exception
{
    public FlowWireException(string message) : base(message) { }
    public FlowWireException(string message, Exception inner) : base(message, inner) { }
}
