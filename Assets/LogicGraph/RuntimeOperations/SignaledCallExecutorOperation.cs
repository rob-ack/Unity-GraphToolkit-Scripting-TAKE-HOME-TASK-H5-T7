using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LogicGraph.Runtime.Operations;

[Serializable]
public class SignaledCallExecutorOperation : IRuntimeNodeExecutor, IRuntimeLinkStageOperation
{
    [SerializeField]
    public string SourceFuncDelegateNameToBind;
    
    [SerializeReference]
    public IRuntimeBlockExecutor TargetNodeInstance;
    [SerializeReference]
    public IRuntimeNodeInstance SourceNodeInstance;

    public ValueTask ExecuteAsync(IRuntimeNodeInstance instance, IRuntimeContext context)
    {
        //does nothing
        return default;
    }

    void IRuntimeLinkStageOperation.RunLinkOperation(IRuntimeContext context)
    {
        var member = SourceNodeInstance?.GetType().GetMember(SourceFuncDelegateNameToBind).FirstOrDefault();

        if (SourceNodeInstance == null || member == null || TargetNodeInstance == null)
        {
            Debug.LogError($"Failed to bind signal for SignaledCallExecutorNodeOperation. SourceNodeInstance, TargetNodeInstance or member {SourceFuncDelegateNameToBind} not found.");
            return;
        }

        Func<ValueTask> newCallback = () => TargetNodeInstance.ExecuteAsync(context);

        switch (member)
        {
            case System.Reflection.FieldInfo fi:
                var currentFieldValue = fi.GetValue(SourceNodeInstance) as Func<ValueTask>;
                var updatedFieldDelegate = (Func<ValueTask>)System.Delegate.Combine(currentFieldValue, newCallback);
                fi.SetValue(SourceNodeInstance, updatedFieldDelegate);
                break;
            case System.Reflection.PropertyInfo pi:
                if (!pi.CanWrite)
                {
                    Debug.LogError($"Failed to bind signal for SignaledCallExecutorNodeOperation. Property {SourceFuncDelegateNameToBind} is read-only.");
                    return;
                }
                var currentPropValue = pi.GetValue(SourceNodeInstance) as Func<ValueTask>;
                var updatedPropDelegate = (Func<ValueTask>)System.Delegate.Combine(currentPropValue, newCallback);
                pi.SetValue(SourceNodeInstance, updatedPropDelegate);
                break;
            default:
                Debug.LogError($"Failed to bind signal for SignaledCallExecutorNodeOperation. Member {SourceFuncDelegateNameToBind} is not a field or property.");
                return;
        }
    }
}