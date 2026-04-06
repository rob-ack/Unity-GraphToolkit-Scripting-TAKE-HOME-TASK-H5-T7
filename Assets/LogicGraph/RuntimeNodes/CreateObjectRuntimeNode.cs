using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Nodes;

public class CreatePrimitiveObjectRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<CreatePrimitiveObjectRuntimeNode>
{
    [field:SerializeField]
    public PrimitiveType PrimitiveType { get; set; }
    public GameObject CreatedObject { get; set; }

    public ValueTask ExecuteAsync(CreatePrimitiveObjectRuntimeNode nodeInstance, IRuntimeContext context)
    {
        var primitive = GameObject.CreatePrimitive(PrimitiveType);
        nodeInstance.CreatedObject = primitive;
        return default;
    }
}

public class CreatePrefabeRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<CreatePrefabeRuntimeNode>
{
    [field: SerializeField]
    public GameObject Prefab { get; set; }
    public GameObject CreatedObject { get; set; }

    public ValueTask ExecuteAsync(CreatePrefabeRuntimeNode nodeInstance, IRuntimeContext context)
    {
        var instance = GameObject.Instantiate(Prefab);
        nodeInstance.CreatedObject = instance;
        return default;
    }

}