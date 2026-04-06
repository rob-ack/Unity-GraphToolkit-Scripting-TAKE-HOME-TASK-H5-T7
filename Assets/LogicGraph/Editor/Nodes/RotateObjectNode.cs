using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using System.Collections.Generic;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace LogicGraph.Editor.Nodes;

[Serializable]
class RotateObjectNode : AbstractNode, IRuntimeNodeConverter
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputOutputExecutionPorts(context);
        context.AddInputPort<GameObject>(nameof(RotateObjectRuntimeNode.Target)).Build();
        context.AddInputPort<Vector3>(nameof(RotateObjectRuntimeNode.Rotation)).Build();
    }

    void IRuntimeNodeConverter.Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var targetInputPort = GetInputPortByName(nameof(RotateObjectRuntimeNode.Target));
        bool isTargetInputSet = targetInputPort.TryGetCompileTimeInputPortValue<GameObject>(out var target);

        var rotationInputPort = GetInputPortByName(nameof(RotateObjectRuntimeNode.Rotation));
        bool isRotationInputSet = rotationInputPort.TryGetCompileTimeInputPortValue<Vector3>(out var rotation);

        List<IPort> portsToBind = new ();
        if (!isTargetInputSet)
        {
            portsToBind.Add(targetInputPort);
        }

        if (!isRotationInputSet)
        {
            portsToBind.Add(rotationInputPort);
        }

        var node = new RotateObjectRuntimeNode
                   {
                       Target = target,
                       Rotation = rotation,
                   };

        executionBuilder
           .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind.ToArray()))
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
           .WithInstance(node);
    }
}