using System;
using System.Collections.Generic;
using System.Linq;

using Rbx2Source.DataTypes;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Geometry
{
    public enum BevelType { Unknown, Block, Cylinder }

    public class Head
    {
        public BevelType BevelType;
        
        public double Bevel;
        public double Buldge;
        public double Roundness;
        
        public Head(BevelType bevelType, double bevel, double roundness, double buldge)
        {
            Bevel = bevel;
            Buldge = buldge;
            BevelType = bevelType;
            Roundness = roundness;
        }

        private bool fuzzyEq(double a, double b)
        {
            double diff = Math.Abs(b - a);
            return (diff < 0.001);
        }

        public bool FieldsMatchWith(BevelMesh mesh)
        {
            bool bevelEq = fuzzyEq(Bevel, mesh.Bevel);
            bool buldgeEq = fuzzyEq(Buldge, mesh.Buldge);
            bool roundnessEq = fuzzyEq(Roundness, mesh.Bevel_Roundness);

            return (bevelEq && buldgeEq && roundnessEq);
        }

        public static Dictionary<Head,string> Lookup = new Dictionary<Head,string>()
        {
            { new Head(BevelType.Block,    0.00, 0.00, 0.00), "Blockhead"        },
            { new Head(BevelType.Block,    0.50, 0.00, 0.00), "Hex"              },
            { new Head(BevelType.Block,    0.30, 0.00, 0.00), "Octoblox"         },
            { new Head(BevelType.Block,    0.05, 0.00, 0.00), "Roll"             },

            { new Head(BevelType.Cylinder, 0.00, 0.00, 0.50), "Barrel"           },
            { new Head(BevelType.Cylinder, 0.10, 0.00, 0.50), "Cool Thing"       },
            { new Head(BevelType.Cylinder, 0.40, 0.00, 0.00), "Cylinder Madness" },
            { new Head(BevelType.Cylinder, 0.66, 0.00, 0.50), "Diamond"          },
            { new Head(BevelType.Cylinder, 0.00, 0.00, 0.00), "Eraser Head"      },
            { new Head(BevelType.Cylinder, 0.00, 0.00, 1.00), "Fat Head"         },
            { new Head(BevelType.Cylinder, 0.20, 0.00, 1.00), "Flat Top"         },
            { new Head(BevelType.Cylinder, 0.40, 1.00, 0.00), "Roundy"           },
            { new Head(BevelType.Cylinder, 0.20, 0.00, 0.50), "ROX BOX"          },
            { new Head(BevelType.Cylinder, 0.10, 0.00, 0.00), "Trim"             },
        };

        public static Asset ResolveHeadMeshAsset(DataModelMesh mesh)
        {
            string result = "Default";

            if (mesh.IsA("BevelMesh"))
            {
                BevelMesh bevelMesh = mesh as BevelMesh;
                BevelType bevelType = BevelType.Unknown;
                
                if (mesh.IsA("BlockMesh"))
                    bevelType = BevelType.Block;
                else if (mesh.IsA("CylinderMesh"))
                    bevelType = BevelType.Cylinder;

                Head match = Lookup.Keys
                    .Where((head) => bevelType == head.BevelType)
                    .Where((head) => head.FieldsMatchWith(bevelMesh))
                    .First();
                
                if (match != null)
                    result = Lookup[match];

                mesh.Scale = new Vector3(1, 1, 1);
            }

            else if (mesh.IsA("SpecialMesh"))
            {
                SpecialMesh specialMesh = mesh as SpecialMesh;
                
                if (specialMesh.MeshType == MeshType.Sphere)
                {
                    result = "Perfection";
                    specialMesh.Scale = new Vector3(1, 1, 1);
                }
            }

            if (result == "Default")
                mesh.Scale /= new Vector3(1.25f, 1.25f, 1.25f);

            return Asset.FromResource("Meshes/Heads/" + result + ".mesh");
        }
    }
}
