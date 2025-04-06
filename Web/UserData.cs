#pragma warning disable 0649

using System;
using System.Collections.Generic;
using RobloxFiles.DataTypes;

using Newtonsoft.Json;

namespace Rbx2Source.Web
{
    public enum AvatarType { R6, R15, Unknown }

    public struct AvatarScale
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
        public string Name;
        public string DisplayName;
        public bool HasVerifiedBadge;
        public List<WebApiError> Errors;
    }
    
    public struct UserInfos
    {
        public UserInfo[] Data;
    }

    public struct MultiGetByUsernameRequest
    {
        public string[] Usernames;
        public bool ExcludeBannedUsers;

        public MultiGetByUsernameRequest(bool excludeBannedUsers, params string[] usernames)
        {
            ExcludeBannedUsers = excludeBannedUsers;
            Usernames = usernames;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public struct AvatarBodyColors
    {
        public string HeadColor3;
        public string LeftArmColor3;
        public string RightArmColor3;
        public string LeftLegColor3;
        public string RightLegColor3;
        public string TorsoColor3;
    }

    public struct AssetVector3
    {
        public float X;
        public float Y;
        public float Z;

        public AssetVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector3(AssetVector3 vec) => new Vector3(vec.X, vec.Y, vec.Z);
        public static implicit operator AssetVector3(Vector3 vec) => new AssetVector3(vec.X, vec.Y, vec.Z);
    }

    public struct WebAssetType
    {
        public AssetType Id;
        public string Name => Enum.GetName(typeof(AssetType), Id);

        public static implicit operator AssetType(WebAssetType assetType) => assetType.Id;
        public static implicit operator WebAssetType(AssetType id) => new WebAssetType() { Id = id };
    }

    public struct AssetMeta
    {
        public int Version;
        public int? Order;
        public float? Puffiness;

        public AssetVector3? Position;
        public AssetVector3? Rotation;
        public AssetVector3? Scale;
    }

    public class AssetInfo
    {
        public long Id;
        public string Name;

        public WebAssetType AssetType;

        public AssetMeta? Meta;
    }

    public class ThumbnailConfig
    {
        public int ThumbnailId = 3;
        public string ThumbnailType = "3d";
        public string Size = "420x420";
    }
    
    public class RenderAvatarRequest
    {
        public UserAvatar AvatarDefinition;
        public ThumbnailConfig ThumbnailConfig = new ThumbnailConfig();

        public RenderAvatarRequest(UserAvatar avatar)
        {
            AvatarDefinition = avatar;
        }
    }

    public class UserAvatar
    {
        public bool UserExists;
        public UserInfo UserInfo;

        public AvatarScale Scales;
        public AvatarType PlayerAvatarType;

        public AvatarBodyColors BodyColor3s;
        public AssetInfo[] Assets;

        private static UserAvatar CreateUserAvatar(UserInfo info)
        {
            var avatar = WebUtil.DownloadJSON<UserAvatar>($"https://avatar.roblox.com/v2/avatar/users/{info.Id}/avatar");
            avatar.UserExists = true;
            avatar.UserInfo = info;

            return avatar;
        }

        public static UserAvatar FromUserId(long userId)
        {
            var info = WebUtil.DownloadJSON<UserInfo>($"https://users.roblox.com/v1/users/{userId}");
            return CreateUserAvatar(info);
        }

        public static UserAvatar FromUsername(string userName)
        {
            var request = new MultiGetByUsernameRequest(false, userName);
            var requestBody = request.ToString();

            var userInfos = WebUtil.DownloadJSON<UserInfos>("https://users.roblox.com/v1/usernames/users", "POST", requestBody);
            var userInfo = userInfos.Data[0];

            return CreateUserAvatar(userInfo);
        }
    }
}
