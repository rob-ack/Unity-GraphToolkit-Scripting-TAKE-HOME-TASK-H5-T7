#nullable enable

using LogicGraph.Editor.Nodes.Operations;
using System.Collections.Generic;
using LogicGraph.Runtime;

namespace LogicGraph.Editor.Conversion;

/// <summary>
/// A compiled execution may contain multiple operations per runtime node execution.
/// The ExecutionBlockBuilder allows to build such execution blocks by providing a fluent interface to push operations and define the scope instance for the block.
/// Executions may also require instance data to operate on. Therefore, the execution blocks can register an instance to work with. This instance is then passed to any execution in the block scope.
/// </summary>
public class ExecutionBlockBuilder : IExecutionBuilder
{
    private readonly IExecutionRootBuilder outerContext;
    private readonly List<IRuntimeNodeExecutor> executors = new();
    private IRuntimeNodeInstance? runtimeInstance;

    public RuntimeExecutionOperationFactory OperationFactory { get; }

    public ExecutionBlockBuilder(IExecutionRootBuilder outerContext, ICompilationStageContext compilationContext)
    {
        this.outerContext = outerContext;
        OperationFactory = new RuntimeExecutionOperationFactory(compilationContext, this);
    }

    public RuntimeExecutionBlock Build()
    {
        return new RuntimeExecutionBlock()
                            {
                                Executors = executors,
                                Instance = runtimeInstance
                            };
    }

    public IExecutionBuilder PushExecution(IRuntimeNodeExecutor? operation)
    {
        if (operation != null)
        {
            executors.Add(operation);
        }
        return this;
    }

    public IExecutionBuilder WithInstance(IRuntimeNodeInstance? instance)
    {
        if (instance != null)
        {
            this.runtimeInstance = instance;
            outerContext.WithInstance(instance, this);
        }
        return this;
    }
}