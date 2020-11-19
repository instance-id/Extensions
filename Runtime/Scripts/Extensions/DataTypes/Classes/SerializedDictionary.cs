using System;
using System.Collections.Generic;
using UnityEngine;

namespace instance.id.Extensions
{
    // -- SerializedDictionaryDrawer.cs
    [Serializable] // @formatter:off
    public class SerialDict<TKey, TValue> : SerializedDictionary<TKey, TValue>
    {
        public SerialDict(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerialDict() : base() { } // @formatter:on
    }

    [Serializable]
    public class SerializedDictionary<KeyType, TValue> : Dictionary<KeyType, TValue>, ISerializationCallbackReceiver
    {
        public const string KeyProperty = nameof(_keys);
        public const string ValueProperty = nameof(_values);

        // These are protected so they can be found by the editor.
        [SerializeField] protected List<KeyType> _keys = new List<KeyType>();
        [SerializeField] protected List<TValue> _values = new List<TValue>(); // @formatter:off

        public SerializedDictionary() { } 
        public SerializedDictionary(IDictionary<KeyType, TValue> dictionary) // @formatter:on
        {
            if (dictionary != null)
                foreach (KeyValuePair<KeyType, TValue> keyValuePair in (IEnumerable<KeyValuePair<KeyType, TValue>>) dictionary)
                    this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        public SerializedDictionary(IDictionary<KeyType, TValue> dictionary, IEqualityComparer<KeyType> comparer)
        {
            if (dictionary != null)
                foreach (KeyValuePair<KeyType, TValue> keyValuePair in (IEnumerable<KeyValuePair<KeyType, TValue>>) dictionary)
                    this.Add(keyValuePair.Key, keyValuePair.Value);
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            ConvertToLists();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            ConvertFromLists();
        }

        private void ConvertToLists()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var entry in this)
            {
                _keys.Add(entry.Key);
                _values.Add(entry.Value);
            }
        }

        private void ConvertFromLists()
        {
            Clear();

            var count = Math.Min(_keys.Count, _values.Count);

            for (var i = 0; i < count; i++)
                Add(_keys[i], _values[i]);
        }
    }
}
