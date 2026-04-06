using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using LogicGraph.Runtime;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace LogicGraph.Editor.Nodes;

[Serializable]
class CreateObjectNode : AbstractNode, IRuntimeNodeConverter
{
    public enum CreationType
    {
        Primitive,
        Prefab
    }

    public static readonly string OPTION_CREATIONTYPE = "CreationType";
    
    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<CreationType>(OPTION_CREATIONTYPE).WithDisplayName("Create from").WithDefaultValue(CreationType.Prefab).Delayed();
        base.OnDefineOptions(context);
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputOutputExecutionPorts(context);

        if (GetNodeOptionByName(OPTION_CREATIONTYPE).TryGetValue<CreationType>(out var creationType))
        {
            switch (creationType)
            {
                case CreationType.Primitive:
                    context.AddInputPort<PrimitiveType>(nameof(CreatePrimitiveObjectRuntimeNode.PrimitiveType)).WithDisplayName("Primitive Type").WithDefaultValue(PrimitiveType.Cube).Build();
                    context.AddOutputPort<GameObject>(nameof(CreatePrimitiveObjectRuntimeNode.CreatedObject)).Build();
                    break;
                case CreationType.Prefab:
                    context.AddInputPort<GameObject>(nameof(CreatePrefabeRuntimeNode.Prefab)).WithDisplayName("Prefab").Build();
                    context.AddOutputPort<GameObject>(nameof(CreatePrefabeRuntimeNode.CreatedObject)).Build();
                    break;
            }
        }
    }

    CreatePrimitiveObjectRuntimeNode CompileForPrimitiveCreateion(out IPort dynamicEvaluatedPort)
    {
        dynamicEvaluatedPort = null;
        var inputPort = GetInputPortByName(nameof(CreatePrimitiveObjectRuntimeNode.PrimitiveType));
        //try to evaluate the input port value at compile time, if possible
        bool isInputSet = inputPort.TryGetCompileTimeInputPortValue<PrimitiveType>(out var primitiveTypeGetInputPortByName);

        var node = new CreatePrimitiveObjectRuntimeNode
                                               {
                                                   PrimitiveType = primitiveTypeGetInputPortByName,
                                               };

        if (!isInputSet && inputPort.isConnected)
        {
            dynamicEvaluatedPort = inputPort;
        }
        return node;
    }

    CreatePrefabeRuntimeNode CompileForPrefabCreation(out IPort dynamicEvaluatedPort)
    {
        dynamicEvaluatedPort = null;
        var inputPort = GetInputPortByName(nameof(CreatePrefabeRuntimeNode.Prefab));
        //try to evaluate the input port value at compile time, if possible
        bool isInputSet = inputPort.TryGetCompileTimeInputPortValue<GameObject>(out var prefab);

        var node = new CreatePrefabeRuntimeNode
                   {
                       Prefab = prefab,
                   };

        if (!isInputSet && inputPort.isConnected)
        {
            dynamicEvaluatedPort = inputPort;
        }

        return node;
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        if (!GetNodeOptionByName(OPTION_CREATIONTYPE).TryGetValue<CreationType>(out var creationType))
        {
            context.Logger.LogFormat(LogType.Error, context.AssetContext, "Failed to compile CreateObjectNode: CreationType option is invalid.");
            return;
        }

        AbstractRuntimeNode node;
        IRuntimeNodeExecutor executor;
        IPort dynamicPort = null;

        switch (creationType)
        {
            case CreationType.Primitive:
            {
                CreatePrimitiveObjectRuntimeNode runtimeNode =  CompileForPrimitiveCreateion(out dynamicPort);
                node = runtimeNode;
                executor = runtimeNode;
                break;
            }
            case CreationType.Prefab:
            {
                CreatePrefabeRuntimeNode runtimeNode = CompileForPrefabCreation(out dynamicPort);
                node = runtimeNode;
                executor = runtimeNode;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        executionBuilder
            .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, dynamicPort))
            .PushExecution(executor)
            .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
            .WithInstance(node);
    }
}