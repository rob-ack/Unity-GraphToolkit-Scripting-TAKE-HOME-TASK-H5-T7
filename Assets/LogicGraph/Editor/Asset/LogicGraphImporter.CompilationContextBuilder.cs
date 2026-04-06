using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using LogicGraph.Editor.Conversion;
using LogicGraph.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LogicGraph.Editor.Asset;

internal sealed partial class LogicGraphImporter
{
    private class CompilationContextBuilder : ICompilationStageContext, IExecutionRootBuilder, ILinkStageContext
    {
        public Graph Graph { get; init; }
        public ILogHandler Logger => this;
        public Object AssetContext => runtimeAsset;

        private readonly AssetImportContext assetImportContext;
        private readonly LogicRuntimeGraph runtimeAsset;
        private readonly HashSet<Action<ILinkStageContext>> finalisationActions = new();
        private readonly Dictionary<INode, IRuntimeBlockExecutor> nodeToExecutorMap = new();

        public CompilationContextBuilder(Graph graph, AssetImportContext assetImportContext, LogicRuntimeGraph runtimeAsset)
        {
            this.runtimeAsset = runtimeAsset;
            Graph = graph;
            this.assetImportContext = assetImportContext;
        }

        void ILogHandler.LogFormat(LogType logType, [CanBeNull] Object context, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Exception:
                case LogType.Error:
                case LogType.Assert:
                    assetImportContext.LogImportError(string.Format(format, args), context);
                    break;
                case LogType.Warning:
                case LogType.Log:
                default:
                    assetImportContext.LogImportWarning(string.Format(format, args), context);
                    break;
            }
        }

        void ILogHandler.LogException(Exception exception, Object context) => assetImportContext.LogImportError(exception.Message, context);

        public void FinishCompilation()
        {
            foreach (var action in finalisationActions)
            {
                action.Invoke(this);
            }
        }

        void ICompilationStageContext.AddLinkStageFinalisation(Action<ILinkStageContext> action)
        {
            finalisationActions.Add(action);
        }

        IRuntimeBlockExecutor ILinkStageContext.GetExecutorInstanceByNode(INode node)
        {
            if (node == null)
            {
                Debug.LogError("Invalid node provided to GetExecutorInstanceById. Cannot retrieve executor instance.");
                return null;
            }

            if (!nodeToExecutorMap.TryGetValue(node, out var executor))
            {
                Debug.LogError($"No executor instance found for node: {node}");
                return null;
            }

            return executor;
        }

        public IExecutionRootBuilder WithStartNode(IRuntimeBlockExecutor blockExecutor)
        {
            runtimeAsset.StartNode = blockExecutor;
            return this;
        }

        public IExecutionRootBuilder WithInstance(IRuntimeNodeInstance instance, IExecutionBuilder executionBlockBuilder)
        {
            return this;
        }

        public IExecutionRootBuilder AddExecutionOperation(IRuntimeBlockExecutor operation, INode node)
        {
            if (operation != null)
            {
                runtimeAsset.Executions.Add(operation);
                if (operation.Instance != null && node != null)
                {
                    nodeToExecutorMap[node] = operation;
                }

                return this;
            }
            throw new ArgumentNullException(nameof(operation), "Cannot push a null operation to the execution context.");
        }
    }
}