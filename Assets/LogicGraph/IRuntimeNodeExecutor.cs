#nullable enable

using System;
using System.Threading.Tasks;

namespace LogicGraph.Runtime;

/// <summary>
/// Defines a contract for executors which rely on runtime node instance data.
/// They define the runtime execution logic for a given node and may rely on the runtime node instance data to execute its logic.
/// </summary>
public interface IRuntimeNodeExecutor
{
    ValueTask ExecuteAsync(IRuntimeNodeInstance? instance, IRuntimeContext context);
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public interface IRuntimeNodeExecutor<in T> : IRuntimeNodeExecutor where T : IRuntimeNodeInstance
{
    ValueTask ExecuteAsync(T instance, IRuntimeContext context);

    ValueTask IRuntimeNodeExecutor.ExecuteAsync(IRuntimeNodeInstance? instance, IRuntimeContext context)
    {
        if (instance is T typedInstance)
        {
            return ExecuteAsync(typedInstance, context);
        }
        throw new InvalidCastException($"Expected instance of type {typeof(T)}, but got {instance?.GetType()}");
    }
}