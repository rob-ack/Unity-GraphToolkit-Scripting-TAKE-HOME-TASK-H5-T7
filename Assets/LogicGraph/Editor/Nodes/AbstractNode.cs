using System;
using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Nodes;

[Serializable]
internal abstract class AbstractNode : Node, INodeBase
{
    public const string EXECUTION_PORT_DEFAULT_NAME = INodeBase.EXECUTION_PORT_DEFAULT_NAME;

    /// <inheritdoc/>
    public void AddInputOutputExecutionPorts(IPortDefinitionContext context) => ((INodeBase)this).AddInputOutputExecutionPortsImplementation(context);
}

[Serializable]
internal abstract class AbstractBlockNode : BlockNode, INodeBase
{
    public const string EXECUTION_PORT_DEFAULT_NAME = INodeBase.EXECUTION_PORT_DEFAULT_NAME;

    /// <inheritdoc/>
    public void AddInputOutputExecutionPorts(IPortDefinitionContext context) => ((INodeBase)this).AddInputOutputExecutionPortsImplementation(context);
}