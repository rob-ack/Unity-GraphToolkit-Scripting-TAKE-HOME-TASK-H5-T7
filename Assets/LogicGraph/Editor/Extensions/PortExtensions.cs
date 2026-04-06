using Unity.GraphToolkit.Editor;

namespace LogicGraph.Editor.Extensions;

public static class PortExtensions
{
    /// <summary>
    /// Gets the value of an input port on a node.
    /// <br/><br/>
    /// The value is obtained from (in priority order):<br/>
    /// 1. Connections to the port (variable nodes, constant nodes, wire portals)<br/>
    /// 2. Embedded value on the port<br/>
    /// 3. Default value of the port<br/>
    /// </summary>
    public static bool TryGetCompileTimeInputPortValue<T>(this IPort port, out T value)
    {
        // If port is connected to another node, get value from connection
        if (port.isConnected)
        {
            switch (port.firstConnectedPort.GetNode())
            {
                case IVariableNode variableNode:
                    return variableNode.variable.TryGetDefaultValue(out value);
                case IConstantNode constantNode:
                    return constantNode.TryGetValue(out value);
                default:
                    break;
            }
        }

        // If port has embedded value, return it.
        // Otherwise, return the default value of the port
        return port.TryGetValue(out value);
    }
}