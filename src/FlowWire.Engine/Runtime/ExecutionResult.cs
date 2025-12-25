using FlowWire.Engine.Internals;

namespace FlowWire.Engine.Runtime;

public record ExecutionResult(ExecutionStatus Status, CommandQueue Commands, object? Result);
