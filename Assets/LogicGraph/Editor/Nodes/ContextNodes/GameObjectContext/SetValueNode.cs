using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

using static LogicGraph.Runtime.Nodes.SetValueRuntimeNode;

namespace LogicGraph.Editor.Nodes.ContextNodes;

[Serializable]
[UseWithContext(typeof(GameObjectContextNode))]
class SetValueNode : AbstractBlockNode, IRuntimeNodeConverter
{
    private const string OPTION_MEMBER = "Member";

    private SupportedValues memberValueToRead;
    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        if (this.contextNode?.inputPortCount <= 0)
        {
            //log error
            return;
        }

        context.AddOption<SupportedValues>(OPTION_MEMBER)
               .WithDisplayName("Member")
               .WithDefaultValue(SupportedValues.Position)
               .Delayed();
    }

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        AddInputOutputExecutionPorts(context);

        GetNodeOptionByName(OPTION_MEMBER).TryGetValue<SupportedValues>(out var member);

        memberValueToRead = member;

        switch (member)
        {
            case SupportedValues.Position:
                context.AddInputPort<Vector3>(nameof(SetValueRuntimeNode.Value)).Build();
                break;
            case SupportedValues.Rotation:
                context.AddInputPort<Quaternion>(nameof(SetValueRuntimeNode.Value)).Build();
                break;
            case SupportedValues.Scale:
                context.AddInputPort<Vector3>(nameof(SetValueRuntimeNode.Value)).Build();
                break;
            default:
                //TODO: log error
                break;
        }
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var inputPortSource = contextNode.GetInputPortByName(nameof(GameObjectContextNode.Source));
        var inputPortValue = GetInputPortByName(nameof(SetValueRuntimeNode.Value));
        object value = null; //TODO: issue boxing

        List<IPort> portsToBind = new();
        if (!inputPortSource.TryGetCompileTimeInputPortValue<GameObject>(out var source))
        {
            portsToBind.Add(inputPortSource);
        }

        switch (memberValueToRead)
        {
            case SupportedValues.Position:
                if (inputPortValue.TryGetCompileTimeInputPortValue<Vector3>(out var position))
                {
                    value = position;
                }
                else
                {
                    portsToBind.Add(inputPortValue);
                }
                break;
            case SupportedValues.Rotation:
                if (inputPortValue.TryGetCompileTimeInputPortValue<Quaternion>(out var rotation))
                {
                    value = rotation;
                }
                else
                {
                    portsToBind.Add(inputPortValue);
                }
                break;
            case SupportedValues.Scale:
                if (inputPortValue.TryGetCompileTimeInputPortValue<Vector3>(out var scale))
                {
                    value = scale;
                }
                else
                {
                    portsToBind.Add(inputPortValue);
                }
                break;
        }

        var node = new SetValueRuntimeNode
                   {
                       Source = source,
                       Member = memberValueToRead,
                       Value = value
                   };

        if (portsToBind.Any())
        {
            executionBuilder
               .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, portsToBind.ToArray()));
        }

        executionBuilder
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
           .WithInstance(node);
    }
}