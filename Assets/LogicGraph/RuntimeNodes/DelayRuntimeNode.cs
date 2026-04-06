using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Nodes;

[Serializable]
public class DelayRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<DelayRuntimeNode>
{
    [field: SerializeField]
    public float Delay { get; set; }

    public async ValueTask ExecuteAsync(DelayRuntimeNode instance, IRuntimeContext context)
    {
        await Awaitable.WaitForSecondsAsync(instance.Delay);
    }
}