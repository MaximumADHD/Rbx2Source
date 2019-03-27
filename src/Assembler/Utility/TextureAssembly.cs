using System.Drawing;
using System.Collections.Generic;

namespace Rbx2Source.Assembler
{
    public class TextureAssembly
    {
        public string MaterialDirectory;

        public Dictionary<string, Image> Images;
        public Dictionary<string, string> MatLinks;

        public TextureAssembly()
        {
            Images = new Dictionary<string, Image>();
            MatLinks = new Dictionary<string, string>();
        }

        public void BindName(string name)
        {
            MatLinks.Add(name, name);
        }

        public void BindName(string link, string name)
        {
            MatLinks.Add(link, name);
        }

        public void AddTexture(string name, Image texture)
        {
            Images.Add(name, texture);
        }

        public void BindTexture(string name, Image texture)
        {
            BindName(name);
            AddTexture(name, texture);
        }

        public void BindTexture(string name, string link, Image texture)
        {
            BindName(name, link);
            AddTexture(link, texture);
        }
    }
}
