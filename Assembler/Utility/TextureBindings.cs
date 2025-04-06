using System.Drawing;
using System.Collections.Generic;

namespace Rbx2Source.Assembler
{
    public class TextureBindings
    {
        public string MaterialDirectory;

        public Dictionary<string, Image> Images;
        public Dictionary<string, Dictionary<string, string>> MatLinks;

        public TextureBindings()
        {
            Images = new Dictionary<string, Image>();
            MatLinks = new Dictionary<string, Dictionary<string, string>>();
        }

        public void BindTextureAlias(string name, string key)
        {
            if (!MatLinks.TryGetValue(name, out Dictionary<string, string> dict))
            {
                dict = new Dictionary<string, string>();
                MatLinks.Add(name, dict);
            }

            dict.Add(key, $"{name}_{key}");
        }

        public void BindTextureAlias(string link, string name, string key)
        {
            if (!MatLinks.TryGetValue(link, out Dictionary<string, string> dict))
            {
                dict = new Dictionary<string, string>();
                MatLinks.Add(link, dict);
            }

            dict.Add(key, name);
        }

        public void BindTexture(string name, Image texture, string key = "basetexture")
        {
            if (key != null)
                BindTextureAlias(name, key);

            Images.Add($"{name}_{key}", texture);
        }

        public void BindTexture(string name, string link, Image texture)
        {
            BindTextureAlias(name, link, "basetexture");
            BindTexture(link, texture, null);
        }
    }
}
