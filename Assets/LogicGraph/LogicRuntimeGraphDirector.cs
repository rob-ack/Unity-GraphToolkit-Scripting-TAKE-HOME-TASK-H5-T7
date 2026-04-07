#nullable enable

using System;
using UnityEngine;

namespace LogicGraph.Runtime
{
    /// <summary>
    /// The director component responsible for executing the logic graph at runtime from a given <see cref="LogicRuntimeGraph"/>.
    /// </summary>
    public partial class LogicRuntimeGraphDirector : MonoBehaviour, IExposedPropertyTable, IRuntimeContext
    {
        [Header("Graph")]
        [SerializeField]
        internal LogicRuntimeGraph RuntimeGraph;

        [NonSerialized]
        private LogicRuntimeGraph? runtimeCopyOfGraph;

        IExposedPropertyTable IRuntimeContext.ExposedPropertyTable => this;
        LogicRuntimeGraphDirector IRuntimeContext.GraphDirector => this;

        private async void Start()
        {
            //Do not operate on the original asset. It will persist changed to the runtime nodes when graph executes.
            runtimeCopyOfGraph = Instantiate(RuntimeGraph);

            foreach (var execution in runtimeCopyOfGraph.Executions)
            {
                if (execution is IRuntimeLinkStageOperation lop)
                {
                    lop.RunLinkOperation(this);
                }
            }

            await runtimeCopyOfGraph.StartNode.ExecuteAsync(this);
        }
    }
}