using System.Collections.Generic;
using System.Linq;

namespace Rbx2Source.QuakeC
{
    public class QuakeCWriter
    {
        private List<QuakeCItem> elements = new List<QuakeCItem>();
        
        public void Add(QuakeCItem item)
        {
            elements.Add(item);
        }

        public QuakeCItem Add(string type, params object[] attributes)
        {
            QuakeCItem item = new QuakeCItem(type, attributes);
            Add(item);

            return item;
        }

        public override string ToString()
        {
            string[] items = elements
                .Select((item) => item.ToString())
                .ToArray();

            return string.Join("\n", items);
        }
    }
}
