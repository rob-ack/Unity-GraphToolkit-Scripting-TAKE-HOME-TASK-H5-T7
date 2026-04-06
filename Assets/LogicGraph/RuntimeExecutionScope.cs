#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime;

/// <summary>
/// <inheritdoc cref="IRuntimeBlockExecutor"/>
/// Represents a block of related runtime executions.
/// The executions share a runtime node instance to operate on.
/// Since not all operations require instance data this may be null.
/// </summary>
[Serializable]
public class RuntimeExecutionBlock : IRuntimeLinkStageOperation, IRuntimeBlockExecutor
{
    [SerializeReference]
    private List<IRuntimeNodeExecutor> executors = new();
    [SerializeReference]
    private IRuntimeNodeInstance? instance;

    public List<IRuntimeNodeExecutor> Executors { get => executors; init => executors = value; }
    public IRuntimeNodeInstance? Instance { get => instance; init => instance = value; }

    public async ValueTask ExecuteAsync(IRuntimeContext context)
    {
        foreach (var executor in Executors)
        {
            await executor.ExecuteAsync(this.Instance, context);
        }
    }

    public void RunLinkOperation(IRuntimeContext context)
    {
        foreach (var executor in Executors)
        {
            if (executor is IRuntimeLinkStageOperation lop)
            {
                lop.RunLinkOperation(context);
            }
        }
    }
}