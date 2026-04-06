using JetBrains.Annotations;
using LogicGraph.Editor.Nodes.Operations;
using LogicGraph.Runtime;

namespace LogicGraph.Editor.Conversion;

public interface IExecutionBuilder
{
    public IExecutionBuilder PushExecution([CanBeNull] IRuntimeNodeExecutor operation);
    public IExecutionBuilder WithInstance([CanBeNull] IRuntimeNodeInstance operation);
    public RuntimeExecutionOperationFactory OperationFactory { get; }
}