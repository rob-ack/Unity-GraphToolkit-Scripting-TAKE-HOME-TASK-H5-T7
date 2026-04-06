using System.Collections.Generic;
using UnityEngine;

namespace LogicGraph.Runtime;

public partial class LogicRuntimeGraphDirector : ISerializationCallbackReceiver
{
    [System.Serializable]
    internal struct ReferenceEntry
    {
        public string key;
        public Object value;
    }

    // Serialize information for the reference map as a list of entries, since Unity cannot serialize dictionaries directly.
    [SerializeField]
    private List<ReferenceEntry> serializedEntries = new();
    // Hold the reference map in memory for quick access. This is not serialized directly, but built from the serialized entries on Awake.
    private Dictionary<PropertyName, Object> referenceMap = new();

    public Object GetReferenceValue(PropertyName id, out bool idValid)
    {
        idValid = referenceMap.TryGetValue(id, out Object value);
        return value;
    }

    void IExposedPropertyTable.SetReferenceValue(PropertyName id, Object value) => referenceMap[id] = value;
    void IExposedPropertyTable.ClearReferenceValue(PropertyName id) => referenceMap.Remove(id);

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        serializedEntries.Clear();
        foreach (var kvp in referenceMap)
        {
            serializedEntries.Add(new ReferenceEntry { key = kvp.Key.ToString(), value = kvp.Value });
        }

    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        foreach (var entry in serializedEntries)
        {
            referenceMap[new PropertyName(entry.key)] = entry.value;
        }
    }
}