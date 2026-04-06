using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Nodes;

[Serializable]
public class IfRuntimeNode : AbstractRuntimeNode, IRuntimeNodeExecutor<IfRuntimeNode>
{
    [SerializeField]
    public bool Condition;

    public Func<ValueTask> True;
    public Func<ValueTask> False;

    public ValueTask ExecuteAsync(IfRuntimeNode instance, IRuntimeContext context)
    {
        if (Condition)
        {
            return True.Invoke();
        }
        return False.Invoke();
    }
}