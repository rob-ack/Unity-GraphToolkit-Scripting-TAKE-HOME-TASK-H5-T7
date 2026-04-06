using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes;

public interface INodeBase : INode
{
    public const string EXECUTION_PORT_DEFAULT_NAME = "ExecutionPort";

    /// <summary>
    /// Defines common input and output execution ports for all nodes.
    /// </summary>
    void AddInputOutputExecutionPorts(Node.IPortDefinitionContext context);

    /// <inheritdoc cref="AddInputOutputExecutionPorts"/>
    public void AddInputOutputExecutionPortsImplementation(Node.IPortDefinitionContext context)
    {
        context.AddInputPort(EXECUTION_PORT_DEFAULT_NAME)
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();

        context.AddOutputPort(EXECUTION_PORT_DEFAULT_NAME)
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
    }
}