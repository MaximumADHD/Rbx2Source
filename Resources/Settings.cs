using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Rbx2Source.Resources
{
    static class Settings
    {
        private static readonly Dictionary<string, object> cache;
        private static readonly RegistryKey rbx2Source;

        public static object GetSetting(string key)
        {
            if (!cache.ContainsKey(key))
                cache[key] = rbx2Source.GetValue(key);

            return cache[key];
        }

        public static string GetString(string key)
        {
            object value = GetSetting(key);
            string result = "";

            if (value != null)
                result = value.ToInvariantString();

            return result;
        }

        public static void Save()
        {
            foreach (string key in cache.Keys)
            {
                object value = cache[key];

                if (value != null)
                {
                    rbx2Source.SetValue(key, cache[key]);
                }
            }
        }

        public static void SetSetting(string key, object value)
        {
            cache[key] = value;
        }

        public static void SaveSetting(string key, object value)
        {
            SetSetting(key, value);
            Save();
        }
        
        private static RegistryKey Open(RegistryKey current, string target)
        {
            return current.CreateSubKey(target, RegistryKeyPermissionCheck.ReadWriteSubTree);
        }


        static Settings()
        {
            RegistryKey currentUser = Registry.CurrentUser;
            RegistryKey software = Open(currentUser, "SOFTWARE");
            rbx2Source = Open(software, "Rbx2Source");
            cache = new Dictionary<string, object>();

            foreach (string key in rbx2Source.GetValueNames())
                SetSetting(key, rbx2Source.GetValue(key));

            if (GetSetting("InitializedV2") == null)
            {
                SetSetting("Username", "qfoxb");
                SetSetting("AssetId", "44113968");
                SetSetting("CompilerType", "Avatar");
                SetSetting("InitializedV2", true);
            }

            software.Dispose();
        }
    }
}