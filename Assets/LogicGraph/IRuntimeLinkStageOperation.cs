namespace LogicGraph.Runtime;

/// <summary>
/// This operation is run during load phase of runtime graph.
/// It can be assumed that all node instances exist at this phase but no instance execution has run yet.
/// This allows to perform operations that are necessary to runtime the graph, such as resolving member values and injecting them into consumers or setting up method invocation delegates to fire on event, etc.
/// This operation will never execute again after load phase.
/// </summary>
public interface IRuntimeLinkStageOperation
{
    void RunLinkOperation(IRuntimeContext context);
}