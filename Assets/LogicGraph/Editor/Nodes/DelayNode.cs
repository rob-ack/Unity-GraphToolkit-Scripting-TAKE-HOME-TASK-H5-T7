using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using System.Collections.Generic;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes;

[Serializable]
class DelayNode : AbstractNode, IRuntimeNodeConverter
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputOutputExecutionPorts(context);
        context.AddInputPort<float>(nameof(DelayRuntimeNode.Delay)).Build();
        base.OnDefinePorts(context);
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var delayInputPort = GetInputPortByName(nameof(DelayRuntimeNode.Delay));
        List<IPort> portsToBind = new();
        if (!delayInputPort.TryGetCompileTimeInputPortValue<float>(out var delay))
        {
            portsToBind.Add(delayInputPort);
        }
        var node = new DelayRuntimeNode
                   {
                       Delay = delay,
                   };
        executionBuilder
           .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind))
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
           .WithInstance(node);
    }
}