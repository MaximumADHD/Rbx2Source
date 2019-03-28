#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rbx2Source.Reflection
{
    /// <summary>
    /// Lazy implementation of Roblox's Instance functionality.
    /// </summary>

    public class Instance
    {
        private Instance rawParent;
        private HashSet<Instance> children = new HashSet<Instance>();

        public string Name;
        public string ClassName => GetType().Name;

        public Instance()
        {
            Name = ClassName;
        }

        public override string ToString()
        {
            return Name;
        }
        
        public Instance[] GetChildren()
        {
            return children.ToArray();
        }

        public T[] GetChildrenOfClass<T>() where T : Instance
        {
            T[] result = children.OfType<T>().ToArray();
            return result;
        }

        public void ForEachChild(Action<Instance> action)
        {
            children.ToList().ForEach(action);
        }

        public void ForEachChildOfClass<T>(Action<T> action) where T : Instance
        {
            children.OfType<T>().ToList().ForEach(action);
        }

        protected bool AddChild(Instance child)
        {
            return children.Add(child);
        }

        protected bool RemoveChild(Instance child)
        {
            return children.Remove(child);
        }

        public bool IsAncestorOf(Instance desc = null)
        {
            while (desc != null)
            {
                if (desc == this)
                    return true;

                desc = desc.Parent;
            }

            return false;
        }

        public bool IsDescendantOf(Instance obj)
        {
            return obj.IsAncestorOf(this);
        }
        
        public string GetFullName()
        {
            string fullName = Name;

            if (Parent != null)
                fullName = Parent.GetFullName() + '.' + fullName;

            return fullName;
        }

        public void Destroy()
        {
            Parent = null;
            ForEachChild(child => child.Destroy());
        }

        public T FindFirstChildOfClass<T>() where T : Instance
        {
            T result = null;

            foreach (Instance child in children)
            {
                if (child is T)
                {
                    result = child as T;
                    break;
                }
            }

            return result;
        }

        public T FindFirstChild<T>(string name, bool recursive = false) where T : Instance
        {
            T firstChild = null;

            foreach (Instance child in children)
            {
                if (child.Name == name && child is T)
                {
                    firstChild = child as T;
                    break;
                }
                else if (recursive)
                {
                    T descendingChild = child.FindFirstChild<T>(name, true);

                    if (descendingChild != null)
                    {
                        firstChild = descendingChild;
                        break;
                    }
                }
            }

            return firstChild;
        }

        public Instance FindFirstChild(string name, bool recursive = false)
        {
            return FindFirstChild<Instance>(name, recursive);
        }

        public Instance Parent
        {
            get
            {
                return rawParent;
            }
            set
            {
                if (value == this)
                    throw new Exception("Attempt to set " + GetFullName() + " as its own parent");
                else if (IsAncestorOf(value))
                    throw new Exception("Attempt to set rawParent of " + GetFullName() + " to " + value.GetFullName() + " would result in circular reference");
                
                if (rawParent != null)
                    rawParent.RemoveChild(this);

                if (value != null)
                    value.AddChild(this);

                rawParent = value;
            }
        }
        
        public bool IsA(string className)
        {
            Type myType = GetType();
            Type specType = Type.GetType("Rbx2Source.Reflection." + className);

            return (specType != null && specType.IsAssignableFrom(myType));
        }

        public static bool TrySetParent(Instance child = null, Instance parent = null)
        {
            if (child != null)
            {
                child.Parent = parent;
                return true;
            }

            return false;
        }
    }
}