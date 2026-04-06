using LogicGraph.Editor.Conversion;
using System;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes;

/// <summary>
/// Represents the Start Node.
/// </summary>
/// <remarks>
/// The start node serves as the entry point to the graph.
/// </remarks>
[Serializable]
internal class StartNode : AbstractNode, IRuntimeNodeConverter
{
    /// <summary>
    /// Defines the output for the node.
    /// </summary>
    /// <param name="context">The scope to define the node.</param>
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        // Start is a special node that has no input, so we don't call DefineCommonPorts
        context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
    }

    public void Convert(ICompilationStageContext context, IExecutionBuilder executionBuilder)
    {
        executionBuilder.PushExecution(executionBuilder.OperationFactory.CreateCallExecutionOperation(GetOutputPortByName(EXECUTION_PORT_DEFAULT_NAME)));
    }
}