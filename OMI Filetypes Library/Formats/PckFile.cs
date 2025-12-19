using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMI.Formats.Languages;

namespace OMI.Formats.Pck
{
    public class PckFile
    {
        public readonly int Type;
        public const string XML_VERSION_STRING = "XMLVERSION";
        public bool HasVerionString => _hasVerionString;
        public int AssetCount => Assets.Count;

        private PckAssetCollection Assets { get; } = new PckAssetCollection();
        private bool _hasVerionString = false;

        public PckFile(int type)
        {
            Type = type;
        }

        public PckFile(int type, bool hasVersionStr)
            : this(type)
        {
            _hasVerionString = hasVersionStr;
        }

        public PckFile() : this(3) { }

        public List<string> GetPropertyList()
        {
            var lut = new List<string>();
            foreach (PckAsset asset in Assets)
            {
                asset.Properties.ForEach(pair =>
                {
                    if (!lut.Contains(pair.Key))
                        lut.Add(pair.Key);
                });
            }
            return lut;
        }

        /// <summary>
        /// Create and add new <see cref="PckAsset"/> object.
        /// </summary>
        /// <param name="assetName">Filename</param>
        /// <param name="assetType">Filetype</param>
        /// <returns>Added <see cref="PckAsset"/> object</returns>
        public PckAsset CreateNewAsset(string assetName, PckAssetType assetType)
        {
            var file = new PckAsset(assetName, assetType);
            AddAsset(file);
            return file;
        }

        /// <summary>
        /// Create, add and initialize new <see cref="PckAsset"/> object.
        /// </summary>
        /// <param name="assetName">Filename</param>
        /// <param name="assetType">Filetype</param>
        /// <returns>Initialized <see cref="PckAsset"/> object</returns>
        public PckAsset CreateNewAsset(string assetName, PckAssetType assetType, Func<byte[]> dataInitializier)
        {
            PckAsset asset = CreateNewAsset(assetName, assetType);
            asset.SetData(dataInitializier?.Invoke());
            return asset;
        }

        /// <summary>
        /// Checks wether a file with <paramref name="assetName"/> and <paramref name="assetType"/> exists
        /// </summary>
        /// <param name="assetName">Path to the file in the pck</param>
        /// <param name="assetType">Type of the file <see cref="PckAsset.FileType"/></param>
        /// <returns>True when file exists, otherwise false </returns>
        public bool HasAsset(string assetName, PckAssetType assetType)
        {
            return Assets.Contains(assetName, assetType);
        }

        /// <summary>
        /// Gets the first file that Equals <paramref name="assetName"/> and <paramref name="assetType"/>
        /// </summary>
        /// <param name="assetName">Path to the file in the pck</param>
        /// <param name="assetType">Type of the file <see cref="PckAsset.FileType"/></param>
        /// <returns>FileData if found, otherwise null</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public PckAsset GetAsset(string assetName, PckAssetType assetType)
        {
            return Assets.GetAsset(assetName, assetType);
        }

        /// <summary>
        /// Tries to get a file with <paramref name="assetPath"/> and <paramref name="assetType"/>.
        /// </summary>
        /// <param name="assetPath">Path to the file in the pck</param>
        /// <param name="assetType">Type of the file <see cref="PckAsset.FileType"/></param>
        /// <param name="asset">If succeeded <paramref name="asset"/> will be non-null, otherwise null</param>
        /// <returns>True if succeeded, otherwise false</returns>
        public bool TryGetAsset(string assetPath, PckAssetType assetType, out PckAsset asset)
        {
            return Assets.TryGet(assetPath, assetType, out asset);
        }

        private void OnPckAssetNameChanging(PckAsset value, string newAssetName)
        {
            if (value.Filename.Equals(newAssetName))
                return;
            int index = Assets.IndexOf(value);
            Assets.RemoveKeyFromCollection(value);
            Assets.Insert(index, value, newAssetName, value.Type);
        }

        private void OnPckAssetTypeChanging(PckAsset value, PckAssetType newAssetType)
        {
            if (value.Type == newAssetType)
                return;
            int index = Assets.IndexOf(value);
            Assets.RemoveKeyFromCollection(value);
            Assets.Insert(index, value, value.Filename, newAssetType);
        }

        private void OnMoveFile(PckAsset value)
        {
            if (Assets.Contains(value.Filename, value.Type))
            {
                Assets.Remove(value);
            }
        }

        public PckAsset GetOrCreate(string assetName, PckAssetType assetType)
        {
            if (Assets.Contains(assetName, assetType))
            {
                return Assets.GetAsset(assetName, assetType);
            }
            return CreateNewAsset(assetName, assetType);
        }

        public bool Contains(string assetName, PckAssetType assetType)
        {
            return Assets.Contains(assetName, assetType);
        }

        public bool Contains(PckAssetType assetType)
        {
            return Assets.Contains(assetType);
        }

        public IEnumerable<PckAsset> GetAssetsByType(PckAssetType assetType)
        {
            return Assets.GetByType(assetType);
        }

        public void AddAsset(PckAsset asset)
        {
            asset.Move();
            asset.SetEvents(OnPckAssetNameChanging, OnPckAssetTypeChanging, OnMoveFile);
            Assets.Add(asset);
        }

        public IReadOnlyCollection<PckAsset> GetAssets()
        {
            return new ReadOnlyCollection<PckAsset>(Assets);
        }

        public bool RemoveAsset(PckAsset asset)
        {
            return Assets.Remove(asset);
        }

        public void RemoveAll(Predicate<PckAsset> value)
        {
            Assets.RemoveAll(value);
        }

        public void InsertAsset(int index, PckAsset asset)
        {
            Assets.Insert(index, asset);
        }

        public int IndexOfAsset(PckAsset asset)
        {
            return Assets.IndexOf(asset);
        }
    }
}
