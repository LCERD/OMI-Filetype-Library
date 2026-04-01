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

        public int ParameterCount => Parameters.Count;

        public PckAsset(string filename, PckAssetType type)
        {
            Type = type;
            Filename = filename;
        }

        public void AddParameter(KeyValuePair<string, string> parameter) => Parameters.Add(parameter);

        public void AddParameter(string parameter, string value) => Parameters.Add(parameter, value);

        public void AddParameter<T>(string parameterName, T value) => Parameters.Add(parameterName, value);

        public void RemoveParameter(string parameterName) => Parameters.Remove(parameterName);

        public bool RemoveParameter(KeyValuePair<string, string> parameter) => Parameters.Remove(parameter);

        public void RemoveParameters(string parameterName) => Parameters.RemoveAll(p => p.Key == parameterName);

        public void ClearParameters() => Parameters.Clear();

        public bool HasParameter(string parameterName) => Parameters.Contains(parameterName);

        public int GetParameterIndex(KeyValuePair<string, string> parameter) => Parameters.IndexOf(parameter);

        public string GetParameter(string parameterName) => Parameters.GetParameterValue(parameterName);

        public T GetParameter<T>(string name, Func<string, T> func) => Parameters.GetParameterValue(name, func);

        public bool TryGetParameter(string parameterName, out string value) => Parameters.TryGetParameter(parameterName, out value);

        public KeyValuePair<string, string>[] GetMultipleParameters(string parameterName) => Parameters.GetParameters(parameterName);
        public string[] GetParameterValues(string parameterName) => Parameters.GetParameters(parameterName).Select(kv => kv.Value).ToArray();

        public IReadOnlyList<KeyValuePair<string, string>> GetParameters() => Parameters.AsReadOnly();

        public void SetParameter(int index, KeyValuePair<string, string> parameter) => Parameters[index] = parameter;

        public void SetParameter(string parameterName, string value) => Parameters.SetParameter(parameterName, value);

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
            hashCode = hashCode * -1521134295 + EqualityComparer<PckFileParameters>.Default.GetHashCode(Parameters);
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
        internal PckFileParameters Parameters = new PckFileParameters();

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
