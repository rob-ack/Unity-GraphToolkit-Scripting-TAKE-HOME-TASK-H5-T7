using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace LogicGraph.Runtime.Nodes;

[Serializable]
public class SetValueRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<SetValueRuntimeNode>
{
    [Serializable]
    [MovedFrom("")]
    public enum SupportedValues
    {
        Position,
        Rotation,
        Scale
    }

    [SerializeField]
    public SupportedValues Member;
    [SerializeReference]
    public object Value; //TODO: boxing. can be optimized by using a generic version of the node, but this is simpler for now since we still use the unity serialization system. More easy to fix when moving to custom serialisation

    public GameObject Source { get; set; }

    public ValueTask ExecuteAsync(SetValueRuntimeNode instance, IRuntimeContext context)
    {
        switch (Member)
        {
            case SupportedValues.Position:
                Source.transform.position = (Vector3)Value;
                break;
            case SupportedValues.Rotation:
                Source.transform.rotation = (Quaternion)Value;
                break;
            case SupportedValues.Scale:
                Source.transform.localScale = (Vector3)Value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return default;
    }
}