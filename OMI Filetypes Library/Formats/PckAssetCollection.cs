using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace OMI.Formats.Pck
{
    public class PckAssetCollection : IList<PckAsset>
    {
        private OrderedDictionary _files = new OrderedDictionary();
        private ArrayList duplicates = new ArrayList();

        public int Count => _files.Count;

        public bool IsReadOnly => false;

        public PckAsset this[string filename, PckAssetType assetType]
        {
            get
            {
                var storeKey = GetStorageKey(filename, assetType);
                if (!_files.Contains(storeKey))
                {
                    throw new KeyNotFoundException(storeKey.ToString());
                }
                return (PckAsset)_files[storeKey];
            }
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));
                _files[GetStorageKey(filename, assetType)] = value;
            }
        }

        public PckAsset this[int index]
        {
            get => _files[index] as PckAsset;
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));
                _files[index] = value;
            }
        }

        public void Add(PckAsset value)
        {
            var key = GetStorageKey(value.Filename, value.Type);
            if (_files.Contains(key))
            {
                if (this[value.Filename, value.Type].Equals(value))
                {
                    Debug.WriteLine($"Duplicate asset: '{value.Filename}'", category: $"{nameof(PckAssetCollection)}.{nameof(Add)}");
                    Debug.WriteLine($"Merging '{value.Filename}' Properties", category: $"{nameof(PckAssetCollection)}.{nameof(Add)}");
                    PckAsset first = GetAsset(value.Filename, value.Type);
                    first.Properties.Merge(value.Properties);
                    return;
                }
                var markedKey = key + value.GetHashCode().ToString();
                Debug.WriteLine($"'{key}' is already present! Adding it as '{markedKey}'",
                    category: $"{nameof(PckAssetCollection)}.{nameof(Add)}");
                duplicates.Add(markedKey);

                _files.Add(markedKey, value);
                return;
            }
            _files.Add(key, value);
        }

        internal PckAsset GetAsset(string filename, PckAssetType assetType)
        {
            return _files[GetStorageKey(filename, assetType)] as PckAsset;
        }

        public void Clear()
        {
            _files.Clear();
        }

        public bool Contains(PckAsset item)
        {
            return Contains(item.Filename, item.Type);
        }

        public bool Contains(string filename, PckAssetType assetType)
        {
            return _files.Contains(GetStorageKey(filename, assetType));
        }

        public bool Contains(PckAssetType assetType)
        {
            return _files.Values.Cast<PckAsset>().FirstOrDefault(asset => asset.Type == assetType) is not null;
        }

        public IEnumerable<PckAsset> GetByType(PckAssetType assetType)
        {
            return _files.Values.Cast<PckAsset>().Where(asset => asset.Type == assetType);
        }

        private object GetStorageKey(string key, PckAssetType assetType)
        {
            return $"{key}_{assetType}";
        }

        private object GetStorageKey(PckAsset item)
        {
            return GetStorageKey(item.Filename, item.Type);
        }

        public void CopyTo(PckAsset[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<PckAsset> GetEnumerator()
        {
            return _files.Values.Cast<PckAsset>().GetEnumerator();
        }

        public int IndexOf(PckAsset key)
        {
            for (int i = 0; i < _files.Count; i++)
            {
                object key2 = _files[i];
                if (key2.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, PckAsset item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));
            _files.Insert(index, GetStorageKey(item), item);
        }
        public void Insert(int index, PckAsset item, string filename, PckAssetType type)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));
            _files.Insert(index, GetStorageKey(filename, type), item);
        }

        internal bool Remove(string filename, PckAssetType assetType)
        {
            if (Contains(filename, assetType))
            {
                _files.Remove(GetStorageKey(filename, assetType));
                return true;
            }
            return false;
        }

        internal bool RemoveKeyFromCollection(PckAsset item)
        {
            return item is not null && InternalRemoveKeyFromCollection(item.Filename, item.Type);
        }

        internal bool InternalRemoveKeyFromCollection(string filename, PckAssetType assetType)
        {
            return Remove(filename,assetType);
        }

        public bool Remove(PckAsset item)
        {
            item?.SetEvents(null, null, null);
            return RemoveKeyFromCollection(item);
        }

        public void RemoveDuplicates()
        {
            foreach (var key in duplicates)
            {
                _files.Remove(key);
            }
            duplicates.Clear();
        }

        public void RemoveAll(Predicate<PckAsset> predicate)
        {
            var valuesToRemove = new List<PckAsset>();
            foreach (PckAsset item in _files.Values)
            {
                if (predicate(item))
                    valuesToRemove.Add(item);
            }
            valuesToRemove.ForEach(v => Remove(v));
        }

        public void RemoveAt(int index)
        {
            _files.RemoveAt(index);
        }

        internal bool TryGet(string key, PckAssetType assetType, out PckAsset value)
        {
            if (Contains(key, assetType))
            {
                value = GetAsset(key, assetType);
                return true;
            }
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _files.GetEnumerator();
        }
    }
}
