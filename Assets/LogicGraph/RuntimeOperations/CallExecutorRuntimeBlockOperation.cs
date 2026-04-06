using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Operations;

/// <summary>
/// Call operation to execute a node. As a node can have multiple of such operations it is a way to define custom logic node behaviour like loops or branches.
/// Utilizing this allows also to retrieve a clean call stack in the runtime.
/// </summary>
[Serializable]
public class CallExecutorRuntimeBlockOperation : IRuntimeNodeExecutor
{
    [SerializeReference]
    public IRuntimeBlockExecutor ExecutorToCall;
    
    public ValueTask ExecuteAsync(IRuntimeNodeInstance instance, IRuntimeContext context)
    {
        //TODO: can be improved to not always create a new stack frame on the call stack without returning the last one. easy to achieve by just using a scheduler instead of direct calls.
        return ExecutorToCall?.ExecuteAsync(context) ?? default;
    }
}