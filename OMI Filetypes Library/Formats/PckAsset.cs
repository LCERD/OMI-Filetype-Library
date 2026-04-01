using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OMI.Formats.Pck
{
    public class PckAsset : IEquatable<PckAsset>
    {
        public string Filename
        {
            get => filename;
            set
            {
                string newFilename = value.Replace('\\', '/');
                OnFilenameChanging?.Invoke(this, newFilename);
                filename = newFilename;
            }
        }
        public PckAssetType Type
        {
            get => type;
            set
            {
                var newValue = value;
                OnAssetTypeChanging?.Invoke(this, newValue);
                type = newValue;
            }
        }

        public byte[] Data => _data;
        public int Size => _data?.Length ?? 0;

        public int PropertyCount => Properties.Count;

        public PckAsset(string filename, PckAssetType type)
        {
            Type = type;
            Filename = filename;
        }

        public void AddProperty(KeyValuePair<string, string> property) => Properties.Add(property);

        public void AddProperty(string propertyName, string value) => Properties.Add(propertyName, value);

        public void AddProperty<T>(string propertyName, T value) => Properties.Add(propertyName, value);

        public void RemoveProperty(string propertyName) => Properties.Remove(propertyName);

        public bool RemoveProperty(KeyValuePair<string, string> property) => Properties.Remove(property);

        public void RemoveProperties(string propertyName) => Properties.RemoveAll(p => p.Key == propertyName);

        public void ClearProperties() => Properties.Clear();

        public bool HasProperty(string propertyName) => Properties.Contains(propertyName);

        public int GetPropertyIndex(KeyValuePair<string, string> property) => Properties.IndexOf(property);

        public string GetProperty(string propertyName) => Properties.GetPropertyValue(propertyName);

        public T GetProperty<T>(string name, Func<string, T> func) => Properties.GetPropertyValue(name, func);

        public bool TryGetProperty(string propertyName, out string value) => Properties.TryGetProperty(propertyName, out value);

        public KeyValuePair<string, string>[] GetMultipleProperties(string propertyName) => Properties.GetProperties(propertyName);
        public string[] GetPropertyValues(string propertyName) => Properties.GetProperties(propertyName).Select(kv => kv.Value).ToArray();

        public IReadOnlyList<KeyValuePair<string, string>> GetProperties() => Properties.AsReadOnly();

        public void SetProperty(int index, KeyValuePair<string, string> property) => Properties[index] = property;

        public void SetProperty(string propertyName, string value) => Properties.SetProperty(propertyName, value);

        public override bool Equals(object obj)
        {
            return obj is PckAsset other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 953938382;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Filename);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(Data);
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<PckFileProperties>.Default.GetHashCode(Properties);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(filename);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(_data);
            return hashCode;
        }

        public void SetData(byte[] data)
        {
            _data = data;
        }

        internal delegate void OnFilenameChangingDelegate(PckAsset _this, string newFilename);
        internal delegate void OnFiletypeChangingDelegate(PckAsset _this, PckAssetType newFiletype);
        internal delegate void OnMoveDelegate(PckAsset _this);
        internal PckFileProperties Properties = new PckFileProperties();

        private string filename;
        private PckAssetType type;
        private OnFilenameChangingDelegate OnFilenameChanging;
        private OnFiletypeChangingDelegate OnAssetTypeChanging;
        private OnMoveDelegate OnMove;
        private byte[] _data = new byte[0];

        internal PckAsset(string filename, PckAssetType filetype,
            OnFilenameChangingDelegate onFilenameChanging, OnFiletypeChangingDelegate onFiletypeChanging,
            OnMoveDelegate onMove)
            : this(filename, filetype)
        {
            SetEvents(onFilenameChanging, onFiletypeChanging, onMove);
        }

        internal PckAsset(string filename, PckAssetType filetype, int dataSize) : this(filename, filetype)
        {
            _data = new byte[dataSize];
        }

        internal bool HasEventsSet()
        {
            return OnFilenameChanging != null && OnAssetTypeChanging != null && OnMove != null;
        }

        internal void SetEvents(OnFilenameChangingDelegate onFilenameChanging, OnFiletypeChangingDelegate onFiletypeChanging, OnMoveDelegate onMove)
        {
            OnFilenameChanging = onFilenameChanging;
            OnAssetTypeChanging = onFiletypeChanging;
            OnMove = onMove;
        }

        public bool Equals(PckAsset other)
        {
            var hasher = MD5.Create();
            var thisHash = BitConverter.ToString(hasher.ComputeHash(Data));
            var otherHash = BitConverter.ToString(hasher.ComputeHash(other.Data));
            return Filename.Equals(other.Filename) &&
                Type.Equals(other.Type) &&
                Size.Equals(other.Size) &&
                thisHash.Equals(otherHash);
        }

        internal void Move()
        {
            OnMove?.Invoke(this);
        }
    }
}
