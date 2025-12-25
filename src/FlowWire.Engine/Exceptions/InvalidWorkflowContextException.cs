namespace FlowWire.Engine.Exceptions;

public class InvalidWorkflowContextException : FlowWireException
{
    public InvalidWorkflowContextException()
        : base("Access to this WorkflowContext is invalid. It may have been reused for a different execution. Do not store IWorkflowContext in static fields or across executions.") { }
}
