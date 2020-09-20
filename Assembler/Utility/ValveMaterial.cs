using System;
using System.Collections.Generic;
using System.IO;

using RobloxFiles.DataTypes;
using RobloxFiles;
using Rbx2Source.Web;


#pragma warning disable CA1308 // Normalize strings to uppercase

namespace Rbx2Source.Assembler
{
    public class ValveMaterial
    {
        private static readonly Vector3 DEFAULT_VERTEX_COLOR = new Vector3(1, 1, 1);

        public BasePart LinkedTo;
        public Asset TextureAsset;

        public bool UseAvatarMap = false;
        public bool UseEnvMap = false;

        public double Transparency = 0.0;
        public double Reflectance = 0.0;

        private readonly Dictionary<string, object> VmtFields = new Dictionary<string, object>();
        public string Shader = "VertexLitGeneric";
        
        private Vector3 vertexColor = null;
        private bool usingDefaultColor = true;

        public Vector3 VertexColor
        {
            get
            {
                Vector3 result;

                if (DEFAULT_VERTEX_COLOR == vertexColor)
                    usingDefaultColor = true;

                if (usingDefaultColor)
                    result = DEFAULT_VERTEX_COLOR;
                else
                    result = vertexColor;

                return result;
            }
            set
            {
                if (value != null)
                {
                    if (DEFAULT_VERTEX_COLOR != value)
                        usingDefaultColor = false;

                    vertexColor = value;
                }
            }
        }

        public void SetVmtField(string name, object value)
        {
            VmtFields[name] = value;
        }

        private void UpdateVmtFields()
        {
            if (Transparency != 0.0 && Transparency != 1.0)
            {
                SetVmtField("alpha", 1 - Transparency);
                SetVmtField("translucent", 1);
            }

            if (UseEnvMap)
            {
                double r = Reflectance;
                string tint = '[' + string.Join(" ", r, r, r) + ']';

                SetVmtField("envmap", "env_cubemap");
                SetVmtField("envmaptint", tint);
            }

            if (!usingDefaultColor)
            {
                float x = Math.Min(1f, Math.Max(0f, VertexColor.X)),
                      y = Math.Min(1f, Math.Max(0f, VertexColor.Y)),
                      z = Math.Min(1f, Math.Max(0f, VertexColor.Z));

                byte r = (byte)(x * 255),
                     g = (byte)(y * 255),
                     b = (byte)(z * 255);

                string rgb = string.Join(" ", r, g, b);
                SetVmtField("color2", $"{{{rgb}}}");
            }
        }

        public string WriteVmtFile()
        {
            string vmtFile;

            using (StringWriter buffer = new StringWriter())
            {
                UpdateVmtFields();

                buffer.WriteLine(Shader);
                buffer.WriteLine("{");

                foreach (string fieldName in VmtFields.Keys)
                {
                    object value = VmtFields[fieldName];
                    buffer.WriteLine("\t$" + fieldName.ToLowerInvariant() + " \"" + value.ToInvariantString() + '"');
                }

                buffer.WriteLine("}");
                vmtFile = buffer.ToString();
            }

            return vmtFile;
        }

        public void WriteVmtFile(string vmtPath)
        {
            string vmtFile = WriteVmtFile();
            FileUtility.WriteFile(vmtPath, vmtFile);
        }
    }
}
