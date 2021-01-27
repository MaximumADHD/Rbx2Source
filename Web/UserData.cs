#pragma warning disable 0649

using System.Collections.Generic;
using System.Threading.Tasks;

using RobloxFiles.Enums;

namespace Rbx2Source.Web
{
    public class AvatarScale
    {
        public float Width;
        public float Height;
        public float Head;
        public float Depth;

        public float Proportion;
        public float BodyType;
    }
    public class WebApiError
    {
        public int Code;
        public string Message;
    }

    public class UserInfo
    {
        public long Id;
        public bool IsOnline;
        public string Username;
        public List<WebApiError> Errors;
    }

    public class WebBodyColors
    {
        public int HeadColorId;
        public int LeftArmColorId;
        public int RightArmColorId;
        public int LeftLegColorId;
        public int RightLegColorId;
        public int TorsoColorId;
    }

    public class AssetTypeInfo
    {
        public AssetType Id;
        public string Name;
    }

    public class AssetInfo
    {
        public long Id;
        public string Name;

        public AssetTypeInfo AssetType;
        public AssetType Type => AssetType.Id;
    }

    public class UserAvatar
    {
        public bool UserExists;
        public UserInfo UserInfo;

        public AvatarScale Scales;
        public HumanoidRigType PlayerAvatarType;

        public WebBodyColors BodyColors;
        public AssetInfo[] Assets;

        private static async Task<UserAvatar> Create(string userInfoUrl)
        {
            UserInfo info;
            UserAvatar avatar;

            using (var http = new RobloxWebClient())
            {
                info = await http.DownloadJson<UserInfo>(userInfoUrl);
                avatar = await http.DownloadJson<UserAvatar>($"https://avatar.roblox.com/v1/users/{info.Id}/avatar");
            }
            
            avatar.UserExists = true;
            avatar.UserInfo = info;

            return avatar;
        }

        public static async Task<UserAvatar> FromUserId(long userId)
        {
            try
            {
                var result = Create($"https://api.roblox.com/Users/{userId}");
                return await result.ConfigureAwait(false);
            }
            catch
            {
                return new UserAvatar();
            }
        }

        public static async Task<UserAvatar> FromUsername(string userName)
        {
            try
            {
                var result = Create($"https://api.roblox.com/Users/Get-By-Username?username={userName}");
                return await result.ConfigureAwait(false);
            }
            catch
            {
                return new UserAvatar();
            }
        }
    }
}
