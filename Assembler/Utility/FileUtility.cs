using System.IO;
using System.Text.RegularExpressions;

namespace Rbx2Source.Assembler
{
    static class FileUtility
    {
        public static string MakeNameWindowsSafe(string name, string replaceWith = "", bool doExtraStuff = true)
        {
            string result = Regex.Replace(name, @"[^A-Za-z0-9 _]", replaceWith).Trim();
            if (doExtraStuff)
                result = result.Replace(" ","_").ToLower();

            return result;
        }

        public static void UnlockFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
        }

        public static void LockFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.ReadOnly);
            }
        }

        public static byte[] ReadFullStream(Stream stream, bool close = true)
        {
            byte[] result;

            using (MemoryStream streamBuffer = new MemoryStream())
            {
                stream.CopyTo(streamBuffer);
                result = streamBuffer.ToArray();
            }

            if (close)
            {
                stream.Close();
                stream.Dispose();
            }

            return result;
        }

        public static void EmptyOutFiles(string folder, bool recursive = true)
        {
            DirectoryInfo info = new DirectoryInfo(folder);
            info.Attributes = FileAttributes.Normal;

            foreach (FileInfo file in info.GetFiles())
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    file.Delete();
                }
                catch
                {
                    Rbx2Source.Print("{0} is locked.", file.Name);
                }
            }

            if (recursive)
            {
                foreach (DirectoryInfo directory in info.GetDirectories())
                {
                    directory.Attributes = FileAttributes.Normal;
                    EmptyOutFiles(directory.FullName, true);
                }
            }
        }

        public static void WriteFile(string path, byte[] data)
        {
            UnlockFile(path);
            File.WriteAllBytes(path, data);
            LockFile(path);
        }

        public static void InitiateEmptyDirectories(params string[] directories)
        {
            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                EmptyOutFiles(directory);
            }
        }

        public static void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
            Rbx2Source.Print("Wrote file: {0}", path);
        }
    }
}
