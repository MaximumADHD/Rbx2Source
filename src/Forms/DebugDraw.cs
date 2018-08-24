using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rbx2Source.Assembler;
using Rbx2Source.Reflection;
using Rbx2Source.StudioMdl;
using Rbx2Source.Web;

namespace Rbx2Source.Forms
{
    public partial class DebugDraw : Form
    {
        public DebugDraw()
        {
            InitializeComponent();
        }

        private void DebugDraw_Load(object sender, EventArgs e)
        {
            UserAvatar avatar = UserAvatar.FromUserId(2032622);
            string avatarType = Rbx2Source.GetEnumName(avatar.ResolvedAvatarType);
            Folder characterAssets = CharacterAssembler.AppendCharacterAssets(avatar, avatarType);

            R15CharacterAssembler assembler = new R15CharacterAssembler();
            assembler.AssembleModel(characterAssets, avatar.Scales);

            TextureCompositor compositor = assembler.ComposeTextureMap(characterAssets, avatar.BodyColors);
            Bitmap baked = compositor.BakeTextureMap();

            image.Image = baked;
        }
    }
}
