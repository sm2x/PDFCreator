﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace pdfforge.PDFCreator.Utilities.IO
{
    /// <summary>
    ///     Helper class to clean a folder with files older then a specific date
    /// </summary>
    public class FolderCleaner
    {
        /// <summary>
        ///     Build a new FolderCleander with the given path. The folder itself will NOT be deleted during cleanup.
        /// </summary>
        /// <param name="cleanupFolder">The path to be cleaned</param>
        public FolderCleaner(string cleanupFolder)
        {
            CleanupFolder = cleanupFolder;
        }

        public string CleanupFolder { get; }

        public Dictionary<string, Exception> CleanupExceptions { get; } = new Dictionary<string, Exception>();

        /// <summary>
        ///     Clean all files in the configured folder. The folder itself will NOT be deleted during cleanup.
        ///     If exceptions occur while cleaning up, they will be stored in CleanupExceptions.
        /// </summary>
        public void Clean()
        {
            Clean(TimeSpan.Zero);
        }

        /// <summary>
        ///     Clean all files in the configured folder. The folder itself will NOT be deleted during cleanup.
        ///     If exceptions occur while cleaning up, they will be stored in CleanupExceptions.
        ///     <param name="minAge">The minimum TimeSpan between file creation date and current time.</param>
        /// </summary>
        public void Clean(TimeSpan minAge)
        {
            try
            {
                if (!Directory.Exists(CleanupFolder))
                    return;

                Clean(CleanupFolder, minAge);
            }
            catch (Exception ex)
            {
                HandleException(CleanupFolder, ex);
            }
        }

        private void Clean(string folder, TimeSpan minAge)
        {
            var folders = new DirectoryInfo(folder).GetDirectories();
            foreach (var item in folders)
            {
                if (Directory.Exists(item.FullName))
                {
                    Clean(item.FullName, minAge);
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(item.FullName).Any())
                            DeleteFolder(minAge, item.FullName);
                    }
                    catch (Exception ex)
                    {
                        HandleException(item.FullName, ex);
                    }
                }
            }

            DeleteFiles(folder, minAge);
        }

        private void DeleteFolder(TimeSpan minAge, string subFolder)
        {
            if (Directory.Exists(subFolder))
            {
                try
                {
                    var directory = new DirectoryInfo(subFolder);
                    var folderAge = DateTime.UtcNow - directory.CreationTimeUtc;

                    if (folderAge >= minAge)
                        Directory.Delete(subFolder);
                }
                catch (Exception ex)
                {
                    HandleException(subFolder, ex);
                }
            }
        }

        private void DeleteFiles(string folder, TimeSpan minAge)
        {
            if (Directory.Exists(folder))
            {
                var files = new DirectoryInfo(folder).GetFiles();
                foreach (var item in files)
                {
                    try
                    {
                        var fileAge = DateTime.UtcNow - item.CreationTimeUtc;
                        if (File.Exists(item.FullName) && fileAge >= minAge)
                            File.Delete(item.FullName);
                    }
                    catch (Exception ex)
                    {
                        HandleException(item.FullName, ex);
                    }
                }
            }
        }

        private void HandleException(string path, Exception ex)
        {
            CleanupExceptions[path] = ex;
        }
    }
}
