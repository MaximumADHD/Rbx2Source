using System.Collections.Generic;
using System.Linq;

namespace Rbx2Source.QuakeC
{
    public class QuakeCItem
    {
        public string Type = "";
        protected bool IsChild;

        public List<string> Attributes = new List<string>();
        public HashSet<QuakeCItem> Children = new HashSet<QuakeCItem>();

        public QuakeCItem(string type, params object[] attributes)
        {
            var strings = attributes.Select((attr) => attr.ToInvariantString());
            Attributes.AddRange(strings);
            Type = type;
        }

        public void AddSubItem(QuakeCItem item)
        {
            if (item != null)
            {
                item.IsChild = true;
                Children.Add(item);
            }
        }

        public void AddSubItem(string type, params object[] attributes)
        {
            QuakeCItem item = new QuakeCItem(type, attributes);
            AddSubItem(item);
        }

        public void AddAttribute(object value)
        {
            Attributes.Add(value.ToInvariantString());
        }

        public override string ToString()
        {
            string result = Type;

            if (!IsChild)
                result = '$' + result;
            
            foreach (string attribute in Attributes)
            {
                string value = attribute;

                if (Attributes.Count > 1 || Children.Count > 0)
                    value = '"' + value + '"';

                result += ' ' + value;
            }

            if (Children.Count > 0)
            {
                result = '\n' + result + " {\n";

                foreach (QuakeCItem child in Children)
                    result += '\t' + child.ToString() + '\n';

                result += '}';
            }

            return result;
        }
    }
}
