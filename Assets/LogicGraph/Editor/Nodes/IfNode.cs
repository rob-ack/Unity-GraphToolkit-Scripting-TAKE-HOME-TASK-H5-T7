using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using System.Collections.Generic;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes;

/// <summary>
/// Represents a conditional node that executes different paths based on a boolean condition.
/// </summary>
[Serializable]
class IfNode : AbstractNode, IRuntimeNodeConverter
{
    private const string EXECUTION_PORT_TRUE_NAME = "True";
    private const string EXECUTION_PORT_FALSE_NAME = "False";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();

        context.AddInputPort<bool>(nameof(IfRuntimeNode.Condition)).Build();
        context.AddOutputPort(EXECUTION_PORT_TRUE_NAME)
               .WithDisplayName(EXECUTION_PORT_TRUE_NAME)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
        context.AddOutputPort(EXECUTION_PORT_FALSE_NAME)
               .WithDisplayName(EXECUTION_PORT_FALSE_NAME)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var conditionInputPort = GetInputPortByName(nameof(IfRuntimeNode.Condition));
        bool isInputSet = conditionInputPort.TryGetConstantInputPortValue<bool>(out var condition);

        List<IPort> portsToBind = new();
        if (!isInputSet)
        {
            portsToBind.Add(conditionInputPort);
        }

        var node = new IfRuntimeNode
        {
            Condition = condition,
        };

        executionBuilder
           .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind.ToArray()))
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateSignaledCallExecutionOperation(nameof(IfRuntimeNode.True), GetOutputPortByName(EXECUTION_PORT_TRUE_NAME)))
           .PushExecution(executionBuilder.OperationFactory.CreateSignaledCallExecutionOperation(nameof(IfRuntimeNode.False), GetOutputPortByName(EXECUTION_PORT_FALSE_NAME)))
           .WithInstance(node);
    }
}