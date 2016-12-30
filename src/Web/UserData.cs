#pragma warning disable 0649
using System.Collections.Generic;

namespace Rbx2Source.Web
{
    enum AvatarType { R6, R15, Unknown }

    class AvatarScale
    {
        public float Width;
        public float Height;
        public float Head;
    }

    struct UserInfo
    {
        public int Id;
        public string Username;
        public bool IsOnline;
        public List<RbxWebApiError> Errors;
    }

    class UserAvatar
    {
        public AvatarType ResolvedAvatarType;
        public List<int> AccessoryVersionIds;
        public AvatarScale Scales;
        public UserInfo UserInfo;

        public bool UserExists = false;

        public static UserAvatar FromUserId(int userId)
        {
            try
            {
                UserInfo info = RbxWebUtility.DownloadRbxApiJSON<UserInfo>("Users/" + userId);
                UserAvatar avatar = RbxWebUtility.DownloadRbxApiJSON<UserAvatar>("v1.1/avatar-fetch?placeId=0&userId=" + userId);
                avatar.UserExists = true;
                avatar.UserInfo = info;
                return avatar;
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
                UserInfo info = RbxWebUtility.DownloadRbxApiJSON<UserInfo>("Users/Get-By-Username?username=" + userName);
                UserAvatar avatar = RbxWebUtility.DownloadRbxApiJSON<UserAvatar>("v1.1/avatar-fetch?placeId=0&userId=" + info.Id);
                avatar.UserExists = true;
                avatar.UserInfo = info;
                return avatar;
            }
            catch
            {
                return new UserAvatar();
            }
        }
    }
}
