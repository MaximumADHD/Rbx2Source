using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rbx2Source.Assembler
{
    class FileUtility
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
                File.SetAttributes(path, FileAttributes.Normal);
        }

        public static void LockFile(string path)
        {
            if (File.Exists(path))
                File.SetAttributes(path, FileAttributes.ReadOnly);
        }

        public static byte[] ReadFullStream(Stream stream, bool close = true)
        {
            MemoryStream streamBuffer = new MemoryStream();
            int count = 1;
            while (stream.CanRead && count > 0)
            {
                byte[] buffer = new byte[2048];
                count = stream.Read(buffer, 0, 2048);
                streamBuffer.Write(buffer, 0, count);
            }

            byte[] result = streamBuffer.ToArray();
            streamBuffer.Close();
            if (close) stream.Close();

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
            FileStream fileStream = (File.Exists(path) ? File.OpenWrite(path) : File.Create(path));
            fileStream.SetLength(data.LongLength);
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
            fileStream.Close();
            LockFile(path);
        }

        public static void InitiateEmptyDirectories(params string[] directories)
        {
            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                else
                    EmptyOutFiles(directory);
            }
        }

        public static void WriteFile(string path, string content)
        {
            WriteFile(path, Encoding.UTF8.GetBytes(content));
        }
    }
}
