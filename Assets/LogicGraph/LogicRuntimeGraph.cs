using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicGraph.Runtime
{
    /// <summary>
    /// The runtime representation of a logic graph.
    /// </summary>
    /// <remarks>
    /// For the sake of this sample, the logic graph is represented simply as a linear list of nodes and a start node.
    /// </remarks>
    public class LogicRuntimeGraph : ScriptableObject
    {
        [SerializeReference]
        public List<IRuntimeBlockExecutor> Executions = new();
        [SerializeReference]
        public IRuntimeBlockExecutor StartNode;
    }
}
