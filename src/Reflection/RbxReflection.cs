#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

using Rbx2Source.Coordinates;
using Rbx2Source.Web;

namespace Rbx2Source.Reflection
{
    static class RbxReflection
    {
        private static XmlNode LoadRobloxNode(string content)
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

        public static Instance Reflect(XmlNode node, Type objType)
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
                    FieldInfo field = objType.GetField(propertyName, 
                                                       BindingFlags.Instance|
                                                       BindingFlags.Public| 
                                                       BindingFlags.FlattenHierarchy);

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
                        Instance childObj = Reflect(child, classType);
                        childObj.Parent = obj;
                    }  
                }
            }

            return obj;
        }

        private static Folder AssembleModel(XmlNode roblox)
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
                            Instance childObj = Reflect(child, classType);
                            childObj.Parent = folder;
                        }
                    }
                }
            }
            return folder;
        }

        public static Folder LoadFromAsset(Asset asset)
        {
            byte[] rawContent = asset.GetContent();
            string content = Encoding.UTF8.GetString(rawContent);
            XmlNode roblox = LoadRobloxNode(content);
            Folder result;
            if (roblox != null)
                result = AssembleModel(roblox);
            else
                result = new Folder();

            result.Name = "AssetImport_" + asset.Id;
            return result;
        }
    }
}
