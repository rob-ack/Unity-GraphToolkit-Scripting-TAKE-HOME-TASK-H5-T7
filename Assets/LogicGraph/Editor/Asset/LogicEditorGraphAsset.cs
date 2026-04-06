using LogicGraph.Editor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace LogicGraph.Editor.Asset
{
    [Graph(AssetExtension)]
    [Serializable]
    class LogicEditorGraphAsset : Graph
    {
        public const string AssetExtension = "logicg";

        [MenuItem("Assets/Create/Graph/Logic Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<LogicEditorGraphAsset>();
        }

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);
            CheckGraphErrors(graphLogger);
        }

        /// <summary>
        /// Checks the graph for errors and warnings and adds them to the result object.
        /// </summary>
        /// <param name="infos">Object implementing <see cref="GraphLogger"/> interface and containing
        /// collected errors and warnings</param>
        /// <remarks>Errors and warnings are reported by adding them to the GraphLogger object,
        /// which is the default reporting mechanism for a Graph Toolkit tool. </remarks>
        void CheckGraphErrors(GraphLogger infos)
        {
            List<StartNode> startNodes = GetNodes().OfType<StartNode>().ToList();

            switch (startNodes.Count)
            {
                case 0:
                    infos.LogError("Add a StartNode in your graph.", this);
                    break;
                case >= 1:
                {
                    foreach (var startNode in startNodes.Skip(1))
                    {
                        infos.LogWarning($"Only one StartNode is supported per graph. Only the first created one will be used.", startNode);
                    }
                    break;
                }
            }
        }
    }
}