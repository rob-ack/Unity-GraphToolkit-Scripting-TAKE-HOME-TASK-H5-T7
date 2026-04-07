using LogicGraph.Editor.Conversion;
using LogicGraph.Editor.Extensions;
using System;
using LogicGraph.Runtime.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEngine;

using static LogicGraph.Runtime.Nodes.GetValueRuntimeNode;

namespace LogicGraph.Editor.Nodes.ContextNodes;

/// <summary>
/// A very simple and limited node for accessing properties on game objects.
/// </summary>
/// <remarks>
/// Note: this is probably the weakest most unflexible code part so far however unitys graph toolkit does not yet support the needed dynamic for node views to support reflection based node option choices or adding custom runtime generated nodes.
/// A solution is to run source generators to create compile time generated node definitions for objects. It would also be a performant way of supporting all the nodes for a real scripting graph tool.
/// but this is beyond the scope now.
/// </remarks>
[Serializable]
[UseWithContext(typeof(GameObjectContextNode))]
class GetValueNode : AbstractBlockNode, IRuntimeNodeConverter
{
    private const string OPTION_MEMBER = "Member";

    private SupportedValues memberValueToRead;
    
    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        if (contextNode?.inputPortCount <= 0)
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
                context.AddOutputPort<Vector3>(nameof(GetValueRuntimeNode.Value)).Build();
                break;
            case SupportedValues.Rotation:
                context.AddOutputPort<Quaternion>(nameof(GetValueRuntimeNode.Value)).Build();
                break;
            case SupportedValues.Scale:
                context.AddOutputPort<Vector3>(nameof(GetValueRuntimeNode.Value)).Build();
                break;
            default:
                //TODO: log error
                break;
        }
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        var inputPort = contextNode.GetInputPortByName(nameof(GameObjectContextNode.Source));
        //try to evaluate the input port value at compile time, if possible
        bool isInputSet = inputPort.TryGetConstantInputPortValue<GameObject>(out var source);

        var node = new GetValueRuntimeNode
                   {
                       Source = source,
                       Member = memberValueToRead,
                   };

        if (!isInputSet)
        {
            executionBuilder
               .PushExecution(executionBuilder.OperationFactory.CreateRuntimeGetMemberValuesOperation(node, inputPort));
        }

        executionBuilder
           .PushExecution(node)
           .PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)))
           .WithInstance(node);
    }
}