using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace Cs.FileHandler
{
    public class FileToolsException : Exception
    {
        public FileToolsException()
        {
        }

        public FileToolsException(string message)
            : base(message)
        {
        }

        public FileToolsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class Tools
    {
        public static string CalculateMD5(string filename, bool includeDash = false)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        if (includeDash)
                            return BitConverter.ToString(hash).ToLowerInvariant();
                        else
                            return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FileToolsException("Calculate MD5 hash failed", ex);
            }
        }

        public static void MoveLastEdited(DirectoryInfo dirInfo, string newDestination)
        {
            try
            {
                FileInfo file = GetLastEdited(dirInfo);
                file.MoveTo(newDestination);
            }
            catch (Exception ex)
            {
                throw new FileToolsException("Move last edited failed", ex);
            }
        }

        public static void CopyLastEdited(DirectoryInfo dirInfo, string newFile)
        {
            try
            {
                FileInfo file = GetLastEdited(dirInfo);
                file.CopyTo(newFile);
            }
            catch (Exception ex)
            {
                throw new FileToolsException("Copy last edited failed", ex);
            }
        }

        public static FileInfo GetLastEdited(DirectoryInfo dirInfo)
        {
            try
            {
                return dirInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            }
            catch (Exception ex)
            {
                throw new FileToolsException("Get last edited failed", ex);
            }
        }

        public static void DeleteDirectoryFiles(DirectoryInfo dirInfo, bool recursive = false)
        {
            bool deletedAll = true;
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            directories.Add(dirInfo);
            if (recursive)
                directories.AddRange(dirInfo.GetDirectories());

            foreach (DirectoryInfo di in directories)
            {
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        deletedAll = false;
                    }
                }
            }

            if (deletedAll)
                throw new FileToolsException("Delete files failed");
        }
    }
}
