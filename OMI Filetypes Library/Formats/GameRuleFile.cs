using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using Newtonsoft.Json;
using OMI.Workers.GameRule;

namespace OMI.Formats.GameRule
{
    public class GameRuleFile
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

        /// <summary>
        /// Initializes a new <see cref="GameRuleFile"/> with the compression level set to <see cref="CompressionLevel.None"/>.
        /// </summary>
        public GameRuleFile() : this(new GameRuleFileHeader(0xffffffff, CompressionLevel.None))
        { }

        public GameRuleFile(GameRuleFileHeader header)
        {
            Root = new GameRule("__ROOT__", null);
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

        public class GameRule
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

            [JsonIgnore]
            public GameRule Parent { get; } = null;
            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();
            public List<GameRule> ChildRules { get; } = new List<GameRule>();

            public GameRule(string name, GameRule parent)
            {
                Name = name;
                Parent = parent;
            }

            public GameRule AddRule(string gameRuleName) => AddRule(gameRuleName, false);

            /// <summary>Adds a new gamerule</summary>
            /// <param name="gameRuleName">Name of the game rule</param>
            /// <param name="validate">Wether to check the given <paramref name="gameRuleName"/></param>
            /// <returns>Added <see cref="GameRule"/></returns>
            public GameRule AddRule(string gameRuleName, bool validate)
            {
                if (validate && !ValidGameRules.Contains(gameRuleName))
                    throw new ArgumentException(gameRuleName + " is not a valid rule name.");
                var rule = new GameRule(gameRuleName, this);
                ChildRules.Add(rule);
                return rule;
            }

            public GameRule AddRule(string gameRuleName, params KeyValuePair<string,string>[] parameters)
            {
                var rule = AddRule(gameRuleName);
                if (rule is null)
                    throw new InvalidOperationException($"Game rule name '{gameRuleName}' is not valid.");
                foreach(var param in parameters)
                { 
                    rule.Parameters[param.Key] = param.Value;
                }
                return rule;
            }
        }

        public void AddGameRules(IEnumerable<GameRule> gameRules) => Root.ChildRules.AddRange(gameRules);
        
        public GameRule AddRule(string gameRuleName)
            => AddRule(gameRuleName, false);

        public GameRule AddRule(string gameRuleName, bool validate)
            => Root.AddRule(gameRuleName, validate);

        public GameRule AddRule(string gameRuleName, params KeyValuePair<string, string>[] parameters)
            => Root.AddRule(gameRuleName, parameters);
    }
}
