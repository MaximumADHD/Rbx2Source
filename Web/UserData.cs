#pragma warning disable 0649
using System.Collections.Generic;

namespace Rbx2Source.Web
{
    public enum AvatarType { R6, R15, Unknown }

    public class AvatarScale
    {
        public float Width;
        public float Height;
        public float Head;
        public float Depth;

        public float Proportion;
        public float BodyType;
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

    public class WrappedAssetType
    {
        public AssetType Id;
        public string Name;
    }

    public class AssetInfo
    {
        public long Id;
        public string Name;

        public WrappedAssetType AssetType;
        public AssetType Type => AssetType.Id;
    }

    public class UserAvatar
    {
        public bool UserExists;
        public UserInfo UserInfo;

        public AvatarScale Scales;
        public AvatarType PlayerAvatarType;

        public WebBodyColors BodyColors;
        public AssetInfo[] Assets;

        private static UserAvatar createUserAvatar(UserInfo info)
        {
            UserAvatar avatar = WebUtility.DownloadRbxApiJSON<UserAvatar>($"/v1/users/{info.Id}/avatar", "avatar");
            avatar.UserExists = true;
            avatar.UserInfo = info;

            return avatar;
        }

        public static UserAvatar FromUserId(long userId)
        {
            try
            {
                UserInfo info = WebUtility.DownloadRbxApiJSON<UserInfo>("Users/" + userId);
                return createUserAvatar(info);
            }
            catch
            {
                return new UserAvatar();
            }
        }

        public static UserAvatar FromUsername(string userName)
        {
            try
            {
                UserInfo info = WebUtility.DownloadRbxApiJSON<UserInfo>("Users/Get-By-Username?username=" + userName);
                return createUserAvatar(info);
            }
            catch
            {
                return new UserAvatar();
            }
        }
    }
}
