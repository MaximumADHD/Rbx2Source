using System.Drawing;
using System.Collections.Generic;

namespace Rbx2Source.Assembler
{
    public class TextureBindings
    {
        public string MaterialDirectory;

        public Dictionary<string, Image> Images;
        public Dictionary<string, string> MatLinks;

        public TextureBindings()
        {
            Images = new Dictionary<string, Image>();
            MatLinks = new Dictionary<string, string>();
        }

        public void BindTextureAlias(string name)
        {
            MatLinks.Add(name, name);
        }

        public void BindTextureAlias(string link, string name)
        {
            MatLinks.Add(link, name);
        }

        public void BindTexture(string name, Image texture, bool bindName = true)
        {
            if (bindName)
                BindTextureAlias(name);

            Images.Add(name, texture);
        }

        public void BindTexture(string name, string link, Image texture)
        {
            BindTextureAlias(name, link);
            BindTexture(link, texture, false);
        }
    }
}
