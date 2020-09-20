using System.Collections.Generic;

using Rbx2Source.Animating;
using RobloxFiles;
using Rbx2Source.StudioMdl;
using Rbx2Source.Textures;
using Rbx2Source.Web;

namespace Rbx2Source.Assembler
{
    public interface ICharacterAssembler
    {
        StudioMdlWriter AssembleModel(Folder characterAssets, AvatarScale scale, bool collisionModel = false);
        Dictionary<string, AnimationId> CollectAnimationIds(UserAvatar avatar);

        TextureCompositor ComposeTextureMap(Folder characterAssets, WebBodyColors bodyColors);
        TextureBindings BindTextures(TextureCompositor compositor, Dictionary<string, ValveMaterial> materials);
        
        byte[] CollisionModelScript { get; }
    }
}
