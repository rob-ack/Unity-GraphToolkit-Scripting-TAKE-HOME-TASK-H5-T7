using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LogicGraph.Runtime.Operations;

/// <summary>
/// This operation builds an expression tree for the given variables to read.
/// <remarks>
/// The assumption is that all related nodes have already been created during load of the graph.
/// This allows to pre build the expression tree that handles the value read and assignment at runtime.
/// The pre compiled delegate executed to trigger value assignment from the source members to the target members at runtime in very performant way.
/// </remarks>
/// </summary>
[Serializable]
public class GetMemberValueBatchedOperation : IRuntimeNodeExecutor, IRuntimeLinkStageOperation
{
    [Serializable]
    public class GetMemberValueInfo
    {
        [SerializeField]
        public string sourceMember;
        [SerializeField]
        public string targetMember;
        [SerializeReference]
        public IRuntimeNodeInstance sourceInstance;
    }

    [SerializeField]
    public GetMemberValueInfo[] VariablesToAssign;
    [SerializeReference]
    public IRuntimeNodeInstance ReceivingInstance;

    private Delegate assignExecutionExpression;

    public GetMemberValueBatchedOperation(IRuntimeNodeInstance receiver)
    {
        ReceivingInstance = receiver;
    }

    void IRuntimeLinkStageOperation.RunLinkOperation(IRuntimeContext context)
    {
        IRuntimeNodeInstance target = ReceivingInstance;
        if (target == null)
        {
            Debug.LogError($"Target instance not found in context. Variable bindings cannot be resolved.");
            return;
        }

        List<Expression> expressions = new();
        foreach (var def in VariablesToAssign)
        {
            IRuntimeNodeInstance source = def.sourceInstance;
            if (source == null)
            {
                Debug.LogError($"Source instance not found in context. Variable bindings cannot be resolved.");
                continue;
            }

            MemberExpression GetNestedProperty(Expression param, string memberPath)
            {
                // sample: splits "transform.position" in ["transform", "position"]
                var parts = memberPath.Split('.');
                Expression current = param;
                MemberExpression result = default;

                //looks up the nested properties one by one: source.transform.position
                foreach (var part in parts)
                {
                    current = result = Expression.PropertyOrField(current, part);
                }

                return result;
            }

            //build the expression to read the value of sourceMember from source and assign it to targetMember of target
            ConstantExpression sourceParam = Expression.Constant(source, source.GetType());
            ConstantExpression targetParam = Expression.Constant(target, target.GetType());
            MemberExpression sourceMember = GetNestedProperty(sourceParam, def.sourceMember);
            MemberExpression targetMember = GetNestedProperty(targetParam, def.targetMember);
            BinaryExpression assignExpression = Expression.Assign(targetMember, Expression.Convert(sourceMember, targetMember.Type)); //the conversion is for possible type differences between source and target. This is like calling a cast with and handles use cases like reading boxed values or implicit convertable types
            expressions.Add(assignExpression);
        }

        var lambda = Expression.Lambda(BlockExpression.Block(expressions));
        assignExecutionExpression = lambda.Compile();
    }

    public ValueTask ExecuteAsync(IRuntimeNodeInstance instance, IRuntimeContext context)
    {
        try
        {
            assignExecutionExpression.DynamicInvoke();
        }
        catch (Exception e)
        {
            throw new AggregateException($"Error resolving variable bindings for instance data of type: {instance?.GetType().Name}", e);
        }
        return default;
    }
}