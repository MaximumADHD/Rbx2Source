#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rbx2Source.Reflection
{
    /// <summary>
    /// Lazy implementation of Roblox's Instance functionality.
    /// </summary>

    class Instance
    {
        private Instance _parent;
        private List<Instance> _children = new List<Instance>();
        public string Name;

        public Instance[] GetChildren()
        {
            return _children.ToArray();
        }

        public T[] GetChildrenOfClass<T>() where T : Instance
        {
            T[] result = _children.OfType<T>().ToArray();
            return result;
        }

        protected void AddChild(Instance child)
        {
            if (!_children.Contains(child))
                _children.Add(child);
        }

        protected void RemoveChild(Instance child)
        {
            if (_children.Contains(child))
                _children.Remove(child);
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
            string fullName = Name;
            Instance current = Parent;

            while (current != null)
            {
                fullName += '.' + current.Name;
                current = current.Parent;
            }

            return fullName;
        }

        public void Destroy()
        {
            Parent = null;
            foreach (Instance child in GetChildren())
                child.Destroy();
        }

        public T FindFirstChildOfClass<T>() where T : Instance
        {
            T result = null;

            foreach (Instance child in _children)
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

            foreach (Instance child in _children)
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

        public Instance FindFirstChild(string name, bool recursive = false) => FindFirstChild<Instance>(name, recursive);

        public Instance Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value != null)
                {
                    if (value == this)
                        throw new Exception("Attempt to set " + GetFullName() + " as its own parent");
                    else if (value.IsDescendantOf(this))
                        throw new Exception("Attempt to set parent of " + GetFullName() + " to " + value.GetFullName() + " would result in circular reference");
                }

                if (_parent != null)
                    _parent.RemoveChild(this);

                _parent = value;

                if (_parent != null)
                    _parent.AddChild(this);
            }
        }

        public string ClassName
        {
            get { return GetType().Name; }
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