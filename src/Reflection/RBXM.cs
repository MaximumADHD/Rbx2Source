#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

using Rbx2Source.Coordinates;
using Rbx2Source.Reflection.BinaryFormat;
using Rbx2Source.Web;

namespace Rbx2Source.Reflection
{
    static class RBXM
    {
        private static XmlNode LoadRobloxNodeXML(string content)
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

            XmlNode roblox = root.FirstChild;
            return roblox;
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
                    properties = child;
                else if (child.Name == "Item")
                    children.Add(child);

            }

            if (properties != null)
            {
                foreach (XmlNode property in properties.ChildNodes)
                {
                    string propertyName = property.Attributes.GetNamedItem("name").Value;
                    propertyName = propertyName.Replace(" ", "_");

                    FieldInfo field = objType.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                    if (field != null)
                    {
                        string propertyType = property.Name;
                        object value = null;

                        string sValue = property.FirstChild.Value;

                        if (propertyType == "string")
                            value = sValue;
                        else if (propertyType == "float")
                            value = float.Parse(sValue, Rbx2Source.NormalParse);
                        else if (propertyType == "double")
                            value = double.Parse(sValue, Rbx2Source.NormalParse);
                        else if (propertyType == "int")
                            value = int.Parse(sValue, Rbx2Source.NormalParse);
                        else if (propertyType == "CoordinateFrame")
                            value = CFrame.FromXml(property);
                        else if (propertyType == "Vector3")
                            value = new Vector3(property);
                        else if (propertyType == "Content")
                        {
                            if (property.FirstChild != null && property.FirstChild.FirstChild != null)
                                value = property.FirstChild.FirstChild.Value;
                        }
                        else if (propertyType == "token")
                        {
                            Type fieldType = field.FieldType;
                            if (fieldType.IsEnum)
                            {
                                int index = int.Parse(sValue);
                                value = Enum.ToObject(fieldType, index);
                            }
                        }

                        if (value != null)
                            field.SetValue(obj, value);
                    }
                }
            }

            foreach (XmlNode child in children)
            {
                XmlNode classNameNode = child.Attributes.GetNamedItem("class");
                if (classNameNode != null)
                {
                    string className = classNameNode.Value;
                    Type classType = Type.GetType("Rbx2Source.Reflection." + className);
                    if (classType != null)
                    {
                        Instance childObj = Reflect_XML(child, classType);
                        childObj.Parent = obj;
                    }  
                }
            }

            return obj;
        }

        private static Instance Reflect_BIN(ClassDescriptor classDesc, Type objType)
        {
            if (!typeof(Instance).IsAssignableFrom(objType))
                throw new Exception("'T' must be an Instance.");

            Instance obj = (Instance)Activator.CreateInstance(objType);
            foreach (PropertyDescriptor prop in classDesc.Properties)
            {
                string propertyName = prop.Name.Replace(" ", "_");
                FieldInfo field = objType.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                if (field != null)
                {
                    BinaryPropertyFormat format = prop.Format;
                    object value = null;

                    switch (format)
                    {
                        // In most use cases, we can just set the value directly.
                        case BinaryPropertyFormat.Float:
                        case BinaryPropertyFormat.Double:
                        case BinaryPropertyFormat.Bool:
                        case BinaryPropertyFormat.Int:
                        case BinaryPropertyFormat.Int64:
                            value = prop.Value;
                            break;

                        // In some cases however, we need to handle it directly.
                        case BinaryPropertyFormat.Vector3:
                            float[] xyz = prop.Value as float[];
                            value = new Vector3(xyz);
                            break;
                        case BinaryPropertyFormat.CFrame:
                            float[] components = prop.Value as float[];
                            value = CFrame.FromComponents(components);
                            break;
                        case BinaryPropertyFormat.Enum:
                            Type fieldType = field.FieldType;
                            if (fieldType.IsEnum)
                            {
                                uint enumIndex = (uint)prop.Value;
                                int index = (int)enumIndex;
                                value = Enum.ToObject(fieldType, index);
                            }
                            break;
                        case BinaryPropertyFormat.String:
                            value = prop.Value.ToString();
                            break;
                        default:
                            break;
                    }

                    if (value != null)
                        field.SetValue(obj, value);
                    
                }
            }

            foreach (ClassDescriptor child in classDesc.Children)
            {
                string className = child.ClassName;
                Type classType = Type.GetType("Rbx2Source.Reflection." + className);
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
                        string className = classNameNode.Value;
                        Type classType = Type.GetType("Rbx2Source.Reflection." + className);
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

        private static Folder AssembleModel_BIN(BinaryFile file)
        {
            Folder folder = new Folder();

            foreach (ClassDescriptor classDesc in file.TreeRoot)
            {
                string className = classDesc.ClassName;
                Type classType = Type.GetType("Rbx2Source.Reflection." + className);
                if (classType != null)
                {
                    Instance childObj = Reflect_BIN(classDesc, classType);
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
                BinaryFile roblox = new BinaryFile(buffer);
                result = AssembleModel_BIN(roblox);
            }
            else
            {
                XmlNode roblox = LoadRobloxNodeXML(contents);
                if (roblox != null)
                    result = AssembleModel_XML(roblox);
                else
                    result = new Folder();
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
