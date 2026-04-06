using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace LogicGraph.Runtime.Nodes;

[Serializable]
public class GetValueRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<GetValueRuntimeNode>
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

    public GameObject Source { get; set; }
    public object Value { get; set; } //TODO: boxing. can be optimized by using a generic version of the node, but this is simpler for now since we still use the unity serialization system. More easy to fix when moving to custom serialisation

    public ValueTask ExecuteAsync(GetValueRuntimeNode instance, IRuntimeContext context)
    {
        switch (Member)
        {
            case SupportedValues.Position:
                Value = Source.transform.position;
                break;
            case SupportedValues.Rotation:
                Value = Source.transform.rotation;
                break;
            case SupportedValues.Scale:
                Value = Source.transform.localScale;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return default;
    }
}