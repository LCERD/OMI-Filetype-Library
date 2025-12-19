using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * all known Model/Material information is the direct product of MattNL's work! check em out! 
 * https://github.com/MattN-L
*/
namespace OMI.Formats.Material
{
    public class MaterialContainer : List<MaterialContainer.Material>
    {
        public static readonly string[] SupportedEntities = new string[]
        {
            "bat",
            "drowned",
            "phantom",
            "shulker",
            "wither_boss",
            "wither_skeleton",
            "ghast",
            "zombie_pigman",
            "phantom_invisible",
            "sheep",
            "stray",
            "ender_dragon",
            "blaze_head",
            "enderman",
            "enderman_invisible",
            "guardian",
            "magma_cube",
            "skeleton",
            "spider",
            "spider_invisible",
            "iron_golem",
            "wolf"
        };

        public static readonly string[] ValidMaterialTypes = new string[]
        {
            "entity",
            "entity_alphatest",
            "entity_alphatest_change_color",
            "entity_change_color",
            "entity_emissive_alpha",
            "entity_emissive_alpha_only",
        };

        public class Material
        {
            private string _type;
            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    if (SupportedEntities.Contains(value))
                        _name = value;
                }
            }

            public string Type
            {
                get => _type;
                set
                {
                    if (ValidMaterialTypes.Contains(value))
                        _type = value;
                }
            }

            public Material(string name, string type)
            {
                _name = name;
                _type = type;
            }
        }

        public int Version;
    }
}
