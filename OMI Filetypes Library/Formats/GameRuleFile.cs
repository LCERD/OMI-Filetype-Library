using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using OMI.Workers.GameRule;

namespace OMI.Formats.GameRule
{
    public sealed class GameRuleFile
    {
        public static readonly string[] ValidGameRules = new string[]
            {
                "MapOptions",
                "ApplySchematic",
                "GenerateStructure",
                "GenerateBox",
                "PlaceBlock",
                "PlaceContainer",
                "PlaceSpawner",
                "BiomeOverride",
                "StartFeature",
                "AddItem",
                "AddEnchantment",
                "WeighedTreasureItem",
                "RandomItemSet",
                "DistributeItems",
                "WorldPosition",
                "LevelRules",
                "NamedArea",
                "ActiveChunkArea",
                "TargetArea",
                "ScoreRing",
                "ThermalArea",
                "PlayerBoundsVolume",
                "Killbox",
                "BlockLayer",
                "UseBlock",
                "CollectItem",
                "CompleteAll",
                "UpdatePlayer",
                "OnGameStartSpawnPositions",
                "OnInitialiseWorld",
                "SpawnPositionSet",
                "PopulateContainer",
                "DegradationSequence",
                "RandomDissolveDegrade",
                "DirectionalDegrade",
                "GrantPermissions",
                "AllowIn",
                "LayerGeneration",
                "LayerAsset",
                "AnyCombinationOf",
                "CombinationDefinition",
                "Variations",
                "BlockDef",
                "LayerSize",
                "UniformSize",
                "RandomizeSize",
                "LinearBlendSize",
                "LayerShape",
                "BasicShape",
                "StarShape",
                "PatchyShape",
                "RingShape",
                "SpiralShape",
                "LayerFill",
                "BasicLayerFill",
                "CurvedLayerFill",
                "WarpedLayerFill",
                "LayerTheme",
                "NullTheme",
                "FilterTheme",
                "ShaftsTheme",
                "BasicPatchesTheme",
                "BlockStackTheme",
                "RainbowTheme",
                "TerracottaTheme",
                "FunctionPatchesTheme",
                "SimplePatchesTheme",
                "CarpetTrapTheme",
                "MushroomBlockTheme",
                "TextureTheme",
                "SchematicTheme",
                "BlockCollisionException",
                "Powerup",
                "Checkpoint",
                "CustomBeacon",
                "ActiveViewArea",
            };

        public readonly GameRule Root = null;

        public readonly GameRuleFileHeader Header = null;

        public readonly List<FileEntry> Files = new List<FileEntry>();

        public enum CompressionLevel : byte
        {
            None             = 0,
            Compressed       = 1,
            CompressedRle    = 2,
            CompressedRleCrc = 3,
        }

        public enum CompressionType
        {
            Unknown = -1,
            /// <summary>
            /// Zlib compression is used on PS Vita, Wii U and Nintendo Switch.
            /// </summary>
            Zlib,
            /// <summary>
            /// Deflate compression is used on Play Station 3.
            /// </summary>
            Deflate,
            /// <summary>
            /// XMem compression is used on XBox 360.
            /// </summary>
            XMem,
        }

        public struct GameRuleParameter
        {
            public string Name;
            public string Value;

            public GameRuleParameter(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public static implicit operator GameRuleParameter(KeyValuePair<string, string> keyValuePair) => new GameRuleParameter(keyValuePair.Key, keyValuePair.Value);
        }

        public abstract class TParameter<T>
        {
            public readonly string Name;
            public readonly T Value;

            public TParameter(string name, T value)
            {
                Name = name;
                Value = value;
            }

            protected virtual string GetFormattedValue() => Value.ToString();

            public static implicit operator GameRuleParameter(TParameter<T> parameter) => new GameRuleParameter(parameter.Name, parameter.GetFormattedValue());
        }

        public sealed class IntParameter(string name, int value) : TParameter<int>(name, value) { }

        public sealed class BoolParameter(string name, bool value) : TParameter<bool>(name, value)
        {
            protected override string GetFormattedValue() => Value.ToString().ToLower();
        }

        public sealed class FloatParameter(string name, float value) : TParameter<float>(name, value) { }

        /// <summary>
        /// Initializes a new <see cref="GameRuleFile"/> with the compression level set to <see cref="CompressionLevel.None"/>.
        /// </summary>
        public GameRuleFile() : this(new GameRuleFileHeader(0xffffffff, CompressionLevel.None))
        { }

        public GameRuleFile(GameRuleFileHeader header)
        {
            Root = new GameRule("__ROOT__");
            Header = header;
        }

        public class FileEntry
        {
            public readonly string Name;
            public readonly byte[] Data;

            public FileEntry(string name, byte[] data)
            {
                Name = name;
                Data = data;
            }
        }

        public void AddFile(string name, byte[] data)
        {
            Files.Add(new FileEntry(name, data));
        }

        public sealed class GameRule
        {
            /// <summary> Contains all valid Parameter names </summary>
            public static readonly string[] ValidParameters = new string[]
            {
                "plus_x",
                "minus_x",
                "plus_z",
                "minus_z",
                "omni_plus_x",
                "omni_minus_x",
                "omni_plus_z",
                "omni_minus_z",
                "none",
                "plus_y",
                "minus_y",
                "plus_x",
                "minus_x",
                "plus_z",
                "minus_z",
                "descriptionName",
                "promptName",
                "dataTag",
                "enchantmentId",
                "enchantmentLevel",
                "itemId",
                "quantity",
                "auxValue",
                "slot",
                "name",
                "food",
                "health",
                "blockId",
                "useCoords",
                "seed",
                "flatworld",
                "filename",
                "rot",
                "data",
                "block",
                "entity",
                "facing",
                "edgeBlock",
                "fillBlock",
                "skipAir",
                "x",
                "x0",
                "x1",
                "y",
                "y0",
                "y1",
                "z",
                "z0",
                "z1",
                "chunkX",
                "chunkZ",
                "yRot",
                "xRot",
                "spawnX",
                "spawnY",
                "spawnZ",
                "orientation",
                "dimension",
                "topblockId",
                "biomeId",
                "feature",
                "minCount",
                "maxCount",
                "weight",
                "id",
                "probability",
                "method",
                "hasBeenInCreative",
                "cloudHeight",
                "fogDistance",
                "dayTime",
                "target",
                "speed",
                "dir",
                "type",
                "pass",
                "for",
                "random",
                "blockAux",
                "size",
                "scale",
                "freq",
                "func",
                "upper",
                "lower",
                "dY",
                "thickness",
                "points",
                "holeSize",
                "variant",
                "startHeight",
                "pattern",
                "colour",
                "primary",
                "laps",
                "liftForceModifier",
                "staticLift",
                "targetHeight",
                "speedBoost",
                "boostDirection",
                "condition_type",
                "condition_value_0",
                "condition_value_1",
                "beam_length",
            };

            public string Name { get; set; } = string.Empty;

            public GameRule Parent => _parent;
            private GameRule _parent = null;
            private Dictionary<string, string> _parameters { get; } = new Dictionary<string, string>();
            private List<GameRule> _childRules { get; } = new List<GameRule>();

            public GameRule(string name, GameRule parent)
            {
                Name = name;
                _parent = parent;
            }

            public GameRule(string name) : this(name, null)
            {
            }

            public GameRule AddRule(string gameRuleName) => AddRule(gameRuleName, false);

            public void AddRule(GameRule gameRule)
            {
                gameRule._parent = this;
                _childRules.Add(gameRule);
            }

            /// <summary>Adds a new gamerule</summary>
            /// <param name="gameRuleName">Name of the game rule</param>
            /// <param name="validate">Wether to check the given <paramref name="gameRuleName"/></param>
            /// <returns>Added <see cref="GameRule"/></returns>
            public GameRule AddRule(string gameRuleName, bool validate)
            {
                if (validate && !ValidGameRules.Contains(gameRuleName))
                    throw new ArgumentException(gameRuleName + " is not a valid rule name.");
                var rule = new GameRule(gameRuleName, this);
                _childRules.Add(rule);
                return rule;
            }

            public GameRule AddRule(string gameRuleName, params GameRuleParameter[] parameters)
            {
                GameRule rule = AddRule(gameRuleName) ?? throw new InvalidOperationException($"Game rule name '{gameRuleName}' is not valid.");
                rule.AddParameters(parameters);
                return rule;
            }

            public void AddRules(IEnumerable<GameRule> gameRules)
            {
                foreach (GameRule gameRule in gameRules)
                {
                    AddRule(gameRule);
                }
            }

            public void AddParameter(string name, string value) => _parameters.Add(name, value);

            public void AddParameter(GameRuleParameter parameter) => AddParameter(parameter.Name, parameter.Value);

            public void AddParameters(params GameRuleParameter[] parameters)
            {
                foreach (GameRuleParameter parameter in parameters)
                {
                    AddParameter(parameter);
                }
            }

            public IReadOnlyCollection<GameRule> GetRules() => _childRules;

            public bool RemoveRule(GameRule rule) => _childRules.Remove(rule);

            public IReadOnlyCollection<KeyValuePair<string, string>> GetParameters() => _parameters.ToArray();

            public bool ContainsParameter(string parameterName) => _parameters.ContainsKey(parameterName);

            public bool RemoveParameter(string parameterName) => _parameters.Remove(parameterName);

            public void SetParameter(string parameterName, string parameterValue) => _parameters[parameterName] = parameterValue;
        }

        public void AddGameRules(IEnumerable<GameRule> gameRules) => Root.AddRules(gameRules);

        public GameRule AddRule(string gameRuleName)
            => AddRule(gameRuleName, false);

        public GameRule AddRule(string gameRuleName, bool validate)
            => Root.AddRule(gameRuleName, validate);

        public GameRule AddRule(string gameRuleName, params GameRuleParameter[] parameters)
            => Root.AddRule(gameRuleName, parameters);
    }
}
