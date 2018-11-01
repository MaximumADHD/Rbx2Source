﻿#pragma warning disable 0649
using System.Collections.Generic;

namespace Rbx2Source.Web
{
    enum AvatarType { R6, R15, Unknown }

    struct AvatarScale
    {
        public float Width;
        public float Height;
        public float Head;
        public float Depth;
        public float Proportion;
        public float BodyType;
    }

    struct UserInfo
    {
        public long Id;
        public string Username;
        public bool IsOnline;
        public List<WebApiError> Errors;
    }

    struct BodyColors
    {
        public int HeadColor;
        public int LeftArmColor;
        public int RightArmColor;
        public int LeftLegColor;
        public int RightLegColor;
        public int TorsoColor;
    }

    class UserAvatar
    {
        public bool UserExists;
        public UserInfo UserInfo;

        public AvatarScale Scales;
        public BodyColors BodyColors;

        public AvatarType ResolvedAvatarType;
        public List<long> AccessoryVersionIds;

        public Dictionary<string, long> Animations;
        
        private static UserAvatar createUserAvatar(UserInfo info)
        {
            UserAvatar avatar = WebUtility.DownloadRbxApiJSON<UserAvatar>("v1.1/avatar-fetch?placeId=0&userId=" + info.Id);
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
