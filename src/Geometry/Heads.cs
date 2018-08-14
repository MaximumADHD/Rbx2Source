using System;
using System.Collections.Generic;
using System.Linq;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection;
using Rbx2Source.Web;

namespace Rbx2Source.Geometry
{
    enum BevelType { Unknown, Block, Cylinder }

    class Head
    {

        public BevelType BevelType;
        public double Bevel;
        public double Roundness;
        public double Buldge;

        public Head(BevelType bevelType, double bevel, double roundness, double buldge)
        {
            BevelType = bevelType;
            Bevel = bevel;
            Roundness = roundness;
            Buldge = buldge;
        }

        private bool fuzzyEq(double a, double b)
        {
            double diff = Math.Abs(b - a);
            return (diff < 0.001);
        }

        public bool paramsMatchWith(BevelMesh mesh)
        {
            return fuzzyEq(mesh.Bevel,           Bevel) &&
                   fuzzyEq(mesh.Bevel_Roundness, Roundness) &&
                   fuzzyEq(mesh.Buldge,          Buldge);
        }

        public static Dictionary<Head,string> Lookup = new Dictionary<Head,string>()
        {
            { new Head(BevelType.Block,    0.0,   0.0,   0.0), "Blockhead"        },
            { new Head(BevelType.Block,    0.5,   0.0,   0.0), "Hex"              },
            { new Head(BevelType.Block,    0.3,   0.0,   0.0), "Octoblox"         },
            { new Head(BevelType.Block,    0.05,  0.0,   0.0), "Roll"             },

            { new Head(BevelType.Cylinder, 0.0,   0.0,   0.5), "Barrel"           },
            { new Head(BevelType.Cylinder, 0.1,   0.0,   0.5), "Cool Thing"       },
            { new Head(BevelType.Cylinder, 0.4,   0.0,   0.0), "Cylinder Madness" },
            { new Head(BevelType.Cylinder, 0.66,  0.0,   0.5), "Diamond"          },
            { new Head(BevelType.Cylinder, 0.0,   0.0,   0.0), "Eraser Head"      },
            { new Head(BevelType.Cylinder, 0.0,   0.0,   1.0), "Fat Head"         },
            { new Head(BevelType.Cylinder, 0.2,   0.0,   1.0), "Flat Top"         },
            { new Head(BevelType.Cylinder, 0.4,   1.0,   0.0), "Roundy"           },
            { new Head(BevelType.Cylinder, 0.2,   0.0,   0.5), "ROX BOX"          },
            { new Head(BevelType.Cylinder, 0.1,   0.0,   0.0), "Trim"             },
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

                Head match = Lookup.Keys.Where(head => head.BevelType == bevelType && head.paramsMatchWith(bevelMesh)).First();
                if (match != null)
                    result = Lookup[match];
            }

            else if (mesh.IsA("SpecialMesh"))
            {
                SpecialMesh specialMesh = mesh as SpecialMesh;
                if (specialMesh.MeshType == MeshType.Sphere)
                    result = "Perfection";
                else
                    specialMesh.Scale = new Vector3(0.8f, 0.8f, 0.8f);
            }

            return Asset.FromResource("Meshes/Heads/" + result + ".mesh");
        }
    }
}
