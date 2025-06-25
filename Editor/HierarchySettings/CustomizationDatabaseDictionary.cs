using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Settings
{
    [Serializable]
    public class CustomizationDatabaseDictionary : Dictionary<GlobalObjectId, CustomizationData>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<GlobalObjectId> keys = new List<GlobalObjectId>();

        [SerializeField]
        private List<CustomizationData> values = new List<CustomizationData>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
            {
                throw new Exception(string.Format("There are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
            }

            for (int i = 0; i < keys.Count; i++)
            {
                this.Add(keys[i], values[i]);
            }
        }

        public CustomizationData GetOrFetchPersistentCustomization(GlobalObjectId fileGuidAndLocalId)
        {
            if (TryGetValue(fileGuidAndLocalId, out CustomizationData currentCustomization))
            {
                return currentCustomization;
            }

            var newCustomization = new CustomizationData();
            TryAdd(fileGuidAndLocalId, newCustomization);
            return newCustomization;
        }
    }
}