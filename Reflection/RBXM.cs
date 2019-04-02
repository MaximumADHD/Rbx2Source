#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Xml;

using Rbx2Source.DataTypes;
using Rbx2Source.Reflection.BinaryFormat;
using Rbx2Source.Web;

namespace Rbx2Source.Reflection
{
    static class RBXM
    {
        private static BindingFlags fieldInfoFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;

        private static XmlNode LoadRobloxNode_XML(string content)
        {
            
            XmlDocument root = new XmlDocument();

            try
            {
                root.LoadXml(content);
            }
            catch
            {
                Console.WriteLine("FAILED TO LOAD XML");
            }

            return root.FirstChild;
        }

        private static Type GetClassType(string className)
        {
            Type rbxm = typeof(RBXM);
            return Type.GetType(rbxm.Namespace + '.' + className);
        }

        private static Instance Reflect_XML(XmlNode node, Type objType)
        {
            if (!typeof(Instance).IsAssignableFrom(objType))
                throw new Exception("'T' must be an Instance.");

            Instance obj = (Instance)Activator.CreateInstance(objType);

            List<XmlNode> children = new List<XmlNode>();
            XmlNode properties = null;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Properties")
                {
                    properties = child;
                }
                else if (child.Name == "Item")
                {
                    children.Add(child);
                }
            }

            if (properties != null)
            {
                foreach (XmlNode property in properties.ChildNodes)
                {
                    string propertyName = property.Attributes.GetNamedItem("name").Value;
                    propertyName = propertyName.Replace(" ", "_");

                    FieldInfo field = objType.GetField(propertyName, fieldInfoFlags);

                    if (field != null && property.FirstChild != null)
                    {
                        string propertyType = property.Name;
                        object value = null;

                        string sValue = property.FirstChild.Value;

                        if (propertyType == "string")
                            value = sValue;
                        else if (propertyType == "float")
                            value = Format.ParseFloat(sValue);
                        else if (propertyType == "double")
                            value = Format.ParseDouble(sValue);
                        else if (propertyType == "int")
                            value = Format.ParseInt(sValue);
                        else if (propertyType == "CoordinateFrame")
                            value = CFrame.FromXml(property);
                        else if (propertyType == "Vector3")
                            value = new Vector3(property);
                        else if (propertyType == "Content")
                            value = property.InnerText;

                        if (propertyType == "token")
                        {
                            Type fieldType = field.FieldType;

                            if (fieldType.IsEnum)
                            {
                                int index = int.Parse(sValue);
                                value = Enum.ToObject(fieldType, index);
                            }
                        }
                        else if (propertyType == "Color3uint8")
                        {
                            uint rgb = uint.Parse(sValue);

                            int r = (int)(rgb / 65536) % 256;
                            int g = (int)(rgb / 256) % 256;
                            int b = (int)(rgb % 256);

                            value = Color.FromArgb(r, g, b);
                        }

                        if (value != null)
                        {
                            if (field.FieldType == typeof(BrickColor))
                            {
                                int brickColorId = (int)value;
                                value = BrickColor.FromNumber(brickColorId);
                            }

                            field.SetValue(obj, value);
                        }
                    }
                }
            }

            foreach (XmlNode child in children)
            {
                XmlNode classNameNode = child.Attributes.GetNamedItem("class");
                if (classNameNode != null)
                {
                    Type classType = GetClassType(classNameNode.Value);
                    if (classType != null)
                    {
                        Instance childObj = Reflect_XML(child, classType);
                        childObj.Parent = obj;
                    }  
                }
            }

            return obj;
        }

        private static Instance Reflect_BIN(FileInstance fileInst, Type objType)
        {
            if (!typeof(Instance).IsAssignableFrom(objType))
                throw new Exception("'T' must be an Instance.");

            Instance obj = Activator.CreateInstance(objType) as Instance;

            foreach (FileProperty prop in fileInst.Properties)
            {
                string propertyName = prop.Name.Replace(" ", "_");
                FieldInfo field = objType.GetField(propertyName, fieldInfoFlags);

                if (field != null)
                {
                    PropertyType propType = prop.Type;
                    object value = prop.Value;

                    if (propType == PropertyType.Enum)
                    {
                        Type fieldType = field.FieldType;

                        if (fieldType.IsEnum)
                        {
                            uint enumIndex = (uint)prop.Value;
                            int index = (int)enumIndex;
                            value = Enum.ToObject(fieldType, index);
                        }
                    }

                    if (value != null)
                    {
                        field.SetValue(obj, value);
                    }
                }
            }

            foreach (FileInstance child in fileInst.GetChildren())
            {
                Type classType = GetClassType(child.ClassName);

                if (classType != null)
                {
                    Instance childObj = Reflect_BIN(child, classType);
                    childObj.Parent = obj;
                }
            }

            return obj;
        }

        private static Folder AssembleModel_XML(XmlNode roblox)
        {
            Folder folder = new Folder();

            foreach (XmlNode child in roblox.ChildNodes)
            {
                if (child.Name == "Item")
                {
                    XmlNode classNameNode = child.Attributes.GetNamedItem("class");
                    if (classNameNode != null)
                    {
                        Type classType = GetClassType(classNameNode.Value);

                        if (classType != null)
                        {
                            Instance childObj = Reflect_XML(child, classType);
                            childObj.Parent = folder;
                        }
                    }
                }
            }

            return folder;
        }

        private static Folder AssembleModel_BIN(RobloxBinaryFile file)
        {
            Folder folder = new Folder();

            foreach (FileInstance fileInst in file.Contents.GetChildren())
            {
                Type classType = GetClassType(fileInst.ClassName);

                if (classType != null)
                {
                    Instance childObj = Reflect_BIN(fileInst, classType);
                    childObj.Parent = folder;
                }
            }

            return folder;
        }

        public static Folder LoadFromBuffer(byte[] buffer)
        {
            Folder result = null;
            string contents = Encoding.Default.GetString(buffer);

            if (contents.StartsWith("<roblox!"))
            {
                RobloxBinaryFile bin = new RobloxBinaryFile(buffer);
                result = AssembleModel_BIN(bin);
            }
            else
            {
                XmlNode xml = LoadRobloxNode_XML(contents);
                result = AssembleModel_XML(xml);
            }

            return result;
        }

        public static Folder LoadFromAsset(Asset asset)
        {
            byte[] rawContent = asset.GetContent();

            Folder result = LoadFromBuffer(rawContent);
            result.Name = "AssetImport_" + asset.Id;

            return result;
        }
    }
}
