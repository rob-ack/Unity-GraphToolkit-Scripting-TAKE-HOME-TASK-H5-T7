using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using System.Collections.Generic;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace LogicGraph.Editor.Nodes;

[Serializable]
class VectorAddNode : AbstractNode, IRuntimeNodeConverter
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputOutputExecutionPorts(context);
        context.AddInputPort<Vector3>(nameof(VectorAddRuntimeNode.A)).Build();
        context.AddInputPort<Vector3>(nameof(VectorAddRuntimeNode.B)).Build();

        context.AddOutputPort<Vector3>(nameof(VectorAddRuntimeNode.Result)).Build();
    }

    void IRuntimeNodeConverter.Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var inPortA = GetInputPortByName(nameof(VectorAddRuntimeNode.A));
        var inPortB = GetInputPortByName(nameof(VectorAddRuntimeNode.B));

        List<IPort> portsToBind = new();
        if (!inPortA.TryGetCompileTimeInputPortValue<Vector3>(out var a))
        {
            portsToBind.Add(inPortA);
        }

        if (!inPortB.TryGetCompileTimeInputPortValue<Vector3>(out var b))
        {
            portsToBind.Add(inPortB);
        }

        var node = new VectorAddRuntimeNode
                   {
                       A = a,
                       B = b,
                   };

        executionBuilder
           .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind))
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
           .WithInstance(node);
    }
}