using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Nodes;

internal class RotateObjectRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<RotateObjectRuntimeNode>
{
    public Vector3 Rotation;
    public GameObject Target;

    public ValueTask ExecuteAsync(RotateObjectRuntimeNode nodeInstance, IRuntimeContext context)
    {
        nodeInstance.Target.transform.Rotate(nodeInstance.Rotation);
        return default;
    }
}