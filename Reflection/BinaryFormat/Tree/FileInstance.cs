using System;
using System.Collections.Generic;
using System.Linq;

namespace Rbx2Source.Reflection.BinaryFormat
{
    public class FileInstance
    {
        public readonly string ClassName;

        public List<FileProperty> Properties = new List<FileProperty>();
        private List<FileInstance> Children = new List<FileInstance>();

        private FileInstance rawParent;
        
        public string Name => ReadProperty("Name", ClassName);
        public override string ToString() => Name;
        
        public FileInstance(string className = "Instance")
        {
            ClassName = className;
        }

        public FileInstance(string className = "Instance", string name = "Instance")
        {
            FileProperty propName = new FileProperty();
            propName.Type = PropertyType.String;
            propName.Instance = this;
            propName.Name = "Name";
            propName.Value = name;
            
            ClassName = className;
            Properties.Add(propName);
        }
        
        public bool IsAncestorOf(FileInstance descendant)
        {
            while (descendant != null)
            {
                if (descendant == this)
                    return true;

                descendant = descendant.Parent;
            }

            return false;
        }
        
        public bool IsDescendantOf(FileInstance ancestor)
        {
            return ancestor.IsAncestorOf(this);
        }
        
        public FileInstance Parent
        {
            get
            {
                return rawParent;
            }
            set
            {
                if (IsAncestorOf(value))
                    throw new Exception("Parent would result in circular reference.");

                if (Parent == this)
                    throw new Exception("Attempt to set parent to self.");

                if (rawParent != null)
                    rawParent.Children.Remove(this);

                value.Children.Add(this);
                rawParent = value;
            }
        }
        
        public FileInstance[] GetChildren()
        {
            return Children.ToArray();
        }
        
        public FileInstance[] GetDescendants()
        {
            List<FileInstance> results = new List<FileInstance>();

            foreach (FileInstance child in Children)
            {
                // Add this child to the results.
                results.Add(child);

                // Add its descendants to the results.
                FileInstance[] descendants = child.GetDescendants();
                results.AddRange(descendants);
            }

            return results.ToArray();
        }
        
        public object ReadProperty(string propertyName)
        {
            FileProperty property = null;

            var query = Properties.Where((prop) => prop.Name.ToLower() == propertyName.ToLower());
            if (query.Count() > 0)
                property = query.First();

            return (property != null ? property.Value : null);
        }
        
        public T ReadProperty<T>(string propertyName, T nullFallback)
        {
            try
            {
                object result = ReadProperty(propertyName);
                return (T)result;
            }
            catch
            {
                return nullFallback;
            }
        }
        
        public bool TryReadProperty<T>(string propertyName, ref T outValue)
        {
            try
            {
                object result = ReadProperty(propertyName);
                outValue = (T)result;

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public void AddProperty(ref FileProperty prop)
        {
            Properties.Add(prop);
        }
    }
}