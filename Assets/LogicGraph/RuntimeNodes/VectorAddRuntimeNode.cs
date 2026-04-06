using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Nodes;

[Serializable]
public class VectorAddRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor
{
    [SerializeField]
    public Vector3 A;
    [SerializeField]
    public Vector3 B;

    public Vector3 Result { get; private set; }

    public ValueTask ExecuteAsync(IRuntimeNodeInstance instance, IRuntimeContext context)
    {
        Result = A + B;
        return default;
    }
}