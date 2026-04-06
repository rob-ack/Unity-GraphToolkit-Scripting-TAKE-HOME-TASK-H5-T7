using System;
using System.Collections.Generic;
using System.Linq;
using LogicGraph.Editor.Conversion;
using LogicGraph.Runtime;
using LogicGraph.Runtime.Operations;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes.Operations;

/// <summary>
/// Factory class to create common runtime execution operations for runtime nodes.
/// Operations are used to add runtime behavior without explicitly defining them in runtime nodes or calling api calls.
/// Instead they are created at conversion stage and add as operations that are ready to execute at runtime.
/// The order in which these operations are executed, is defined by the order they are pushed to the execution block builder during compilation.
/// </summary>
public class RuntimeExecutionOperationFactory
{
    private readonly ICompilationStageContext compilationContext;
    private readonly IExecutionBuilder executionBlockBuilder;

    public RuntimeExecutionOperationFactory(ICompilationStageContext compilationContext, IExecutionBuilder executionBlockBuilder)
    {
        this.compilationContext = compilationContext;
        this.executionBlockBuilder = executionBlockBuilder;
    }

    /// <summary>
    /// <inheritdoc cref="CreateRuntimeGetMemberValuesOperation(IRuntimeNodeInstance,Unity.GraphToolkit.Editor.IPort[])" />
    /// </summary>
    public IRuntimeNodeExecutor CreateRuntimeGetMemberValuesOperation(IRuntimeNodeInstance receiver, IReadOnlyCollection<IPort> portsToBind)
    {
        return CreateRuntimeGetMemberValuesOperation(receiver, portsToBind?.ToArray());
    }

    /// <summary>
    /// Creates a variable read operation that will resolve the value of a given source and assigns it to the target member.
    /// The source is defined by the given source instance ID. The source member to read the value from is identified by given name. 
    /// The target is defined by the value receiving node guid and the member is identified by port name.
    /// </summary>
    /// <remarks>
    /// Nested member names are supported by using dot notation. For example, if you want to read the position of a transform, you can use "transform.position" as the source member name.
    /// The operation will then resolve the value of the position property of the transform at runtime and assign it to the target member.
    /// </remarks>
    public IRuntimeNodeExecutor CreateRuntimeGetMemberValuesOperation(IRuntimeNodeInstance receiver, params (IRuntimeNodeInstance provider, string memberName)[] portsToBind)
    {
        var array = new GetMemberValueBatchedOperation.GetMemberValueInfo[portsToBind.Length];

        for (var index = 0; index < portsToBind.Length; index++)
        {
            var (provider, memberName) = portsToBind[index];
            array[index] = new GetMemberValueBatchedOperation.GetMemberValueInfo
                           {
                               sourceMember = memberName,
                               sourceInstance = provider,
                               targetMember = memberName
                           };
        }

        var executor = new GetMemberValueBatchedOperation(receiver)
                                              {
                                                  VariablesToAssign = array
                                              };


        return executor;
    }

    /// <summary>
    /// Creates a variable read operation that will resolve the value of a given source and assigns it to the target member.
    /// The source is defined by the first connected port of the given ports to bind, the source node has to be of type <see cref="IGuidProvider"/>. The source member to read the value from is identified by the port name.
    /// The target is defined by the value receiving node guid and the member is identified by port name.
    /// </summary>
    public IRuntimeNodeExecutor CreateRuntimeGetMemberValuesOperation(IRuntimeNodeInstance receiver, params IPort[] portsToBind)
    {
        if (portsToBind is not { Length: > 0 })
        {
            return default;
        }

        Span<GetMemberValueBatchedOperation.GetMemberValueInfo> array = new GetMemberValueBatchedOperation.GetMemberValueInfo[portsToBind.Length];

        int couter = 0;
        foreach (var port in portsToBind)
        {
            if (port == null)
            {
                continue;
            }

            if (TryCreateGetRuntimeValuesByInputPortData(port, out var runtimeBinding))
            {
                array[couter++] = runtimeBinding;
            }
        }

        if (couter == 0)
        {
            return default;
        }

        return new GetMemberValueBatchedOperation(receiver)
                       {
                           VariablesToAssign = array[..couter].ToArray()
                       };
    }

    private bool TryCreateGetRuntimeValuesByInputPortData(IPort port, out GetMemberValueBatchedOperation.GetMemberValueInfo runtimeBinding)
    {
        runtimeBinding = default;

        if (!port.isConnected)
        {
            return false;
        }

        var sourcePort = port.firstConnectedPort;
        var sourceNode = sourcePort.GetNode();
        if (sourceNode == null)
        {
            return false;
        }

        var result = new GetMemberValueBatchedOperation.GetMemberValueInfo
                         {
                             sourceMember = sourcePort.name,
                             targetMember = port.name
                         };
        runtimeBinding = result;

        compilationContext.AddLinkStageFinalisation((linkStage) =>
        {
            result.sourceInstance = linkStage.GetExecutorInstanceByNode(sourceNode)?.Instance;
        });

        return true;

    }

    /// <summary>
    /// Creates an operation that will execute a flow call when the operation is run. This is useful to continue the execution flow automatically from the given execution port.
    /// </summary>
    public IRuntimeNodeExecutor CreateCallExecutionOperation(IPort execPort)
    {
        if (execPort is { isConnected: true } && execPort.firstConnectedPort.GetNode() != null)
        {
            var executor = new CallExecutorRuntimeBlockOperation();

            compilationContext.AddLinkStageFinalisation((linkStage) =>
            {
                executor.ExecutorToCall = linkStage.GetExecutorInstanceByNode(execPort.firstConnectedPort.GetNode());
            });

            return executor;
        }
        return default;
    }

    /// <summary>
    /// Creates an operation that will execute a flow call when the given FuncDelegate is invoked on a runtime node.
    /// </summary>
    public IRuntimeNodeExecutor CreateSignaledCallExecutionOperation(string signalName, IPort executionPort)
    {
        if (executionPort is { isConnected: true } 
            && executionPort.firstConnectedPort.GetNode() != null
            && executionPort.GetNode() != null)
        {
            var executor = new SignaledCallExecutorOperation
                                                     {
                                                         SourceFuncDelegateNameToBind = signalName,
                                                     };

            compilationContext.AddLinkStageFinalisation((linkStage) =>
            {
                executor.SourceNodeInstance = linkStage.GetExecutorInstanceByNode(executionPort.GetNode())?.Instance;
                executor.TargetNodeInstance = linkStage.GetExecutorInstanceByNode(executionPort.firstConnectedPort.GetNode());
            });

            return executor;
        }
        return default;
    }
}
