#pragma warning disable 0649

using System;
using System.Collections.Generic;

namespace Rbx2Source.Reflection
{
    /// <summary>
    /// Lazy implementation of Roblox's Instance functionality.
    /// </summary>

    class Instance
    {
        private Instance parentInternal;
        private List<Instance> Children = new List<Instance>();
        public string Name;

        public Instance[] GetChildren()
        {
            return Children.ToArray();
        }

        protected void AddChild(Instance child)
        {
            if (!Children.Contains(child))
                Children.Add(child);
        }

        protected void RemoveChild(Instance child)
        {
            if (Children.Contains(child))
                Children.Remove(child);
        }

        public bool IsDescendantOf(Instance obj)
        {
            foreach (Instance child in obj.GetChildren())
            {
                if (child == obj)
                    return true;
                else
                    return IsDescendantOf(child);
            }

            return false;
        }

        public bool IsAncestorOf(Instance obj)
        {
            return obj.IsDescendantOf(this);
        }

        public string GetFullName()
        {
            List<string> traverse = new List<string>();
            traverse.Add(this.Name);
            Instance current = this;
            while (true)
            {
                Instance parent = current.Parent;
                if (parent != null)
                    traverse.Add(parent.Name);
                else
                    break;

                current = parent;
            }

            traverse.Reverse();

            string result = string.Join(".", traverse.ToArray());
            return result;
        }

        public void Destroy()
        {
            Parent = null;
            while (Children.Count > 0)
            {
                Instance child = Children[Children.Count - 1];
                Children.Remove(child);
                child.Destroy();
            }
        }

        public Instance FindFirstChild(string name, bool recursive = false)
        {
            Instance firstChild = null;
            foreach (Instance child in Children)
            {
                if (child.Name == name)
                {
                    firstChild = child;
                    break;
                }
                else if (recursive)
                {
                    Instance descendingChild = child.FindFirstChild(name, true);
                    if (descendingChild != null)
                    {
                        firstChild = descendingChild;
                        break;
                    }
                }
            }
            return firstChild;
        }

        public Instance FindFirstChildOfClass(string className)
        {
            Instance result = null;
            foreach (Instance child in Children)
            {
                if (child.IsA(className))
                {
                    result = child;
                    break;
                }
            }
            return result;
        }

        public Instance Parent
        {
            get
            {
                return parentInternal;
            }
            set
            {
                if (value != null)
                {
                    if (value == this)
                        throw new Exception("Attempt to set " + this.GetFullName() + " as its own parent");
                    else if (value.IsDescendantOf(this))
                        throw new Exception("Attempt to set parent of " + this.GetFullName() + " to " + value.GetFullName() + " would result in circular reference");
                }

                Instance oldParent = this.parentInternal;
                if (oldParent != null)
                    oldParent.RemoveChild(this);

                parentInternal = value;
                if (parentInternal != null)
                    parentInternal.AddChild(this);

            }
        }

        public string ClassName
        {
            get
            {
                Type myType = this.GetType();
                string typeName = myType.Name;
                return typeName;
            }
        }

        public bool IsA(string className)
        {
            Type myType = this.GetType();
            Type specType = Type.GetType("Rbx2Source.Reflection." + className);
            if (specType != null)
                return specType.IsAssignableFrom(myType);
            else
                return false;
        }

        public Instance this[string childName]
        {
            get
            {
                Instance result = this.FindFirstChild(childName);
                if (result != null)
                    return result;
                else
                    throw new Exception(childName + " is not a valid member of " + this.ClassName);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public Instance()
        {
            Name = ClassName;
        }
    }
}