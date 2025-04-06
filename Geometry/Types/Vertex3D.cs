using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using RobloxFiles;
using RobloxFiles.DataTypes;
using Rbx2Source.StudioMdl;

namespace Rbx2Source.Geometry
{
    // 3D Geometry components of the Vertex class
    // The 2D Texture components are defined in Textures/Vertex2D.cs

    public partial class Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public Color? Color;
        public MeshSkinning Skinning;
        public Dictionary<int, float> Weights = new Dictionary<int, float>();

        public string WriteStudioMdl(StudioMdlWriter writer, BasePart identity, Mesh mesh)
        {
            var scale = Rbx2Source.MODEL_SCALE;
            var numWeights = Weights.Keys.Count;

            var values = new List<float>()
            {
                Position.X * scale,
                Position.Y * scale,
                Position.Z * scale,

                Normal.X,
                Normal.Y,
                Normal.Z,

                UV.X,
                1 - UV.Y,
            };

            if (numWeights > 0)
            {
                foreach (var pair in Weights)
                {
                    int boneIndex = pair.Key;
                    var bone = mesh.Bones[boneIndex];

                    if (bone.Name == identity.Name)
                    {
                        numWeights -= 1;
                        continue;
                    }

                    var targetNodeQuery = writer.Nodes
                        .Where(node => node.Name == bone.Name)
                        .Select(node => node.NodeIndex);

                    if (targetNodeQuery.Any())
                    {
                        var targetNode = targetNodeQuery.First();
                        float weight = pair.Value;

                        values.Add(targetNode);
                        values.Add(weight);
                    }
                }

                var insertAt = values.Count - (numWeights * 2);
                values.Insert(insertAt, numWeights);
            }

            return Format.FormatFloats(values.ToArray()).Replace(".0000000", "");
        }
    }
}
