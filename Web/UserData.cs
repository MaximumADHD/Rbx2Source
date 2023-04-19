#pragma warning disable 0649
using System;
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
        public string description;
        public string created; // Date time
        public bool isBanned;
        public string externalAppDisplayName;
        public bool hasVerifiedBadge;
        public long id;
        public string name;
        public string displayName;
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

    public class ResultGetByUsername
    {
        public Data[] data;
    }

    public class Data
    {
        public string requestedUsername;
        public bool hasVerifiedBadge;
        public long id;
        public string name;
        public string displayName;
    }

    public class RequestGetByUsernameBody
    {
        public string[] usernames;
        public bool excludeBannedUsers;
        
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
            UserAvatar avatar = WebUtility.DownloadRbxApiJSON<UserAvatar>($"/v1/users/{info.id}/avatar", "avatar");
            avatar.UserExists = true;
            avatar.UserInfo = info;

            return avatar;
        }

        public static UserAvatar FromUserId(long userId)
        {
            try
            {
                UserInfo info = WebUtility.DownloadRbxApiJSON<UserInfo>("v1/users/" + userId, "users");
                System.Console.WriteLine(info);
                return createUserAvatar(info);
            }
            catch
            {
                return new UserAvatar();
            }
        }

        public static UserAvatar FromUsername(string userName)
        {
            // Very funky implementation 
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(new RequestGetByUsernameBody
            {
                usernames = new string[]
                {
                    userName
                },
                excludeBannedUsers = false
            });
            ResultGetByUsername res = WebUtility.DownloadRbxApiJSON<ResultGetByUsername>("v1/usernames/users", "users", body, "POST");
            return FromUserId(res.data[0].id);
        }
    }
}
