using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LOGGER;

namespace FILEHANDLER
{
    public static class Extensions
    {
        private static Logger _logger = new Logger(typeof(Extensions));

        public static void SaveToFile(this Stream stream, string file)
        {
            using (var fileStream = File.Create(file))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            _logger.Debug("Stream saved {0}", file);
        }

        public static void MoveLastEdited(this DirectoryInfo dirInfo, string newDestination)
        {
            FileInfo file = GetLastEdited(dirInfo);
            _logger.Debug("Last modified file {0}", file.FullName);

            file.MoveTo(newDestination);
            _logger.Debug("Moved last edited file in {0}", dirInfo.Name);
        }

        public static void CopyLastEdited(this DirectoryInfo dirInfo, string newFile)
        {
            FileInfo file = GetLastEdited(dirInfo);
            _logger.Debug("Last modified file {0}", file.FullName);

            file.CopyTo(newFile);
            _logger.Debug("Copied last edited file in {0}", dirInfo.Name);
        }

        public static FileInfo GetLastEdited(this DirectoryInfo dirInfo)
        {
            FileInfo file = dirInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            _logger.Debug("Get last edited {0}", file.FullName);
            return file;
        }

        public static void DeleteDirectoryFiles(this DirectoryInfo dirInfo, bool recursive = false)
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
                        _logger.Debug("Deleted file {0}", file.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug("File {0} not deleted {1}'{2}'", file.FullName, ex.GetType(), ex.Message);
                        deletedAll = false;
                    }
                }
            }

            if (deletedAll)
                _logger.Debug("Deleted directory contents {0}", dirInfo.Name);
        }
    }
}
