using LogicGraph.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace LogicGraph.Editor.Conversion;

public interface IExecutionRootBuilder : ILogHandler
{
    public IExecutionRootBuilder WithStartNode(IRuntimeBlockExecutor blockExecutor);
    public IExecutionRootBuilder WithInstance(IRuntimeNodeInstance instance, IExecutionBuilder executionBlockBuilder);
    public IExecutionRootBuilder AddExecutionOperation(IRuntimeBlockExecutor operation, INode node);
}