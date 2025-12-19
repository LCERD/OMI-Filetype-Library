using System;
using System.Drawing;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

/*
 * all known Model/Material information is the direct product of MattNL's work! check em out! 
 * https://github.com/MattN-L
*/
namespace OMI.Formats.Model
{
    public sealed class ModelContainer : ICollection<Model>
    {
        public int Version;

        private Dictionary<string, Model> Models { get; } = new Dictionary<string, Model>();

        public int ModelCount => Models.Count;

        public int Count => Models.Count;

        public bool IsReadOnly => false;

        /// <exception cref="ModelNotFoundException"></exception>
        public Model GetModelByName(string name)
        {
            if (!Models.ContainsKey(name))
                throw new ModelNotFoundException(nameof(name));
            return Models[name];
        }

        public IEnumerable<string> GetModelNames() => Models.Keys;
        
        public IEnumerable<Model> GetModels() => Models.Values;

        public void Add(Model item) => Models.Add(item.Name, item);

        public void Clear() => Models.Clear();

        public bool ContainsModel(string name) => Models.ContainsKey(name);

        public bool Contains(Model item) => ContainsModel(item.Name);

        public void CopyTo(Model[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void SetModel(Model model) => Models[model.Name] = model;

        public bool Remove(Model item) => Models.Remove(item.Name);

        public IEnumerator<Model> GetEnumerator() => Models.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class Model
    {
        public string Name { get; }
        public Size TextureSize { get; }
        public int PartCount => _parts.Count;

        private Dictionary<string, ModelPart> _parts;

        public Model(string name, Size textureSize)
        {
            Name = name;
            TextureSize = textureSize;
            _parts = new Dictionary<string, ModelPart>();
        }

        public void AddPart(ModelPart part)
        {
            _parts.Add(part.Name, part);
        }

        public bool ContainsPart(string partName) => _parts.ContainsKey(partName);
        
        public bool TryGetPart(string partName, out ModelPart modelPart) => _parts.TryGetValue(partName, out modelPart);

        public IEnumerable<ModelPart> GetParts() => _parts.Values;
    }

    public sealed class ModelPart
    {
        public string Name { get; }
        public string ParentName { get; }
        public Vector3 Translation { get; }
        public Vector3 Rotation { get; }
        public Vector3 AdditionalRotation { get; }
        public int BoxCount => _boxes.Count;

        private List<ModelBox> _boxes = new List<ModelBox>();
        public ModelPart(string name, string parentName, Vector3 translation, Vector3 rotation, Vector3 additionalRotation)
        {
            Name = name;
            ParentName = parentName;
            Translation = translation;
            Rotation = rotation;
            AdditionalRotation = additionalRotation;
        }

        public void AddBox(ModelBox modelBox) => _boxes.Add(modelBox);
        public void AddBoxes(IEnumerable<ModelBox> modelBoxes) => _boxes.AddRange(modelBoxes);

        public IEnumerable<ModelBox> GetBoxes() => _boxes;
    }

    public sealed class ModelBox
    {
        public Vector3 Position { get; }
        public Vector3 Size { get; }
        public Vector2 Uv { get; }
        public float Inflate { get; }
        public bool Mirror { get; }

        public ModelBox(Vector3 position, Vector3 size, Vector2 uv, float inflate, bool mirror)
        {
            Position = position;
            Size = size;
            Uv = uv;
            Inflate = inflate;
            Mirror = mirror;
        }
    }
}
