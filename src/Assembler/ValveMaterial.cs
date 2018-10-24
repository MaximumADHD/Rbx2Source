using System.Collections.Generic;
using System.IO;

using Rbx2Source.Coordinates;

namespace Rbx2Source.Assembler
{
    class ValveMaterial
    {
        private Dictionary<string, object> Fields = new Dictionary<string, object>();
        public string Shader = "VertexLitGeneric";

        private static Vector3 defVertexColor = new Vector3(1, 1, 1);

        public void SetField(string name, object value)
        {
            if (!Fields.ContainsKey(name))
                Fields.Add(name, value);
        }

        public override string ToString()
        {
            StringWriter buffer = new StringWriter();
            buffer.WriteLine(Shader);
            buffer.WriteLine("{");
            foreach (string fieldName in Fields.Keys)
            {
                object value = Fields[fieldName];
                string valueStr = value.ToString();
                buffer.WriteLine("\t$" + fieldName.ToLower() + " \"" + valueStr + '"');
            }
            buffer.WriteLine("}");
            return buffer.ToString();
        }

        public ValveMaterial(Material mat)
        {
            if (mat.Transparency != 0.0)
            {
                SetField("alpha", 1 - mat.Transparency);
                SetField("translucent", 1);
                SetField("allowfencerenderstatehack", 1); // Still not sure what this is, but it works like MAGIC.
            }

            if (mat.UseReflectance)
            {
                double r = mat.Reflectance;
                string tint = "[" + string.Join(" ", r, r, r) + "]";

                SetField("envmap", "env_cubemap");
                SetField("envmaptint", tint);
            }

            if (mat.VertexColor != defVertexColor)
            {
                Vector3 vc = mat.VertexColor;

                byte r = (byte)(vc.X * 255);
                byte g = (byte)(vc.Y * 255);
                byte b = (byte)(vc.Z * 255);

                string rgb = string.Join(" ", r, g, b);
                SetField("color2", "{" + rgb + "}");
            }
        }

    }
}
