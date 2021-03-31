// Copyright 2020 Mälardalens högskola
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
// sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
// IN THE SOFTWARE.
//

using SE.MDH.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FileDirectorySystemWatcher
{
  /// <summary>
  /// https://jira.mdh.se/browse/DAGLIGDRIFT-173
  /// FileWatcher Service with TopShelf and log4net

  /// </summary>
  ///
  public class FileWatcherService
  {
    private static readonly ILogWriter log = Logger.Get(MethodBase.GetCurrentMethod().DeclaringType);
    private static string sourceFolder;
    private static string destinationFolder;
    private static string filterFileType;

    public static string DestinationFolder { get => destinationFolder; set => destinationFolder = value; }
    public static string SourceFolder { get => sourceFolder; set => sourceFolder = value; }
    public static string FilterFileType { get => filterFileType; set => filterFileType = value; }

    /// <summary>
    /// FileWatcherService Constructor initilaisation with Config
    /// </summary>
    public FileWatcherService()
    {
      SourceFolder = ConfigurationManager.AppSettings["sourceFolder"]; ;
      DestinationFolder = ConfigurationManager.AppSettings["destinationFolder"]; ;
      FilterFileType = ConfigurationManager.AppSettings["filterFileType"]; ;

      log.Info($"sourceFolder:{ SourceFolder}, destinationFolder:{ DestinationFolder} : filterFileType:{FilterFileType} ");

      if (string.IsNullOrEmpty(SourceFolder) || string.IsNullOrEmpty(DestinationFolder) || string.IsNullOrEmpty(FilterFileType))
      {
        throw new ArgumentNullException("Det saknas några parametrar som (sourceFolder , destinationFolder, filterFileType");
      }
      log.Info("Klart att läsa config");
    }
    public void Start()
    {
      var watcher = new FileSystemWatcher(SourceFolder);

      watcher.NotifyFilter = NotifyFilters.Attributes
                           | NotifyFilters.CreationTime
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.FileName
                           | NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Security
                           | NotifyFilters.Size;


      watcher.Changed += OnChanged;
      watcher.Created += OnCreated;
      watcher.Deleted += OnDeleted;
      watcher.Renamed += OnRenamed;
      watcher.Error += OnError;

      watcher.Filter = FilterFileType;
      watcher.IncludeSubdirectories = true;
      watcher.EnableRaisingEvents = true;
      log.Info("File watcher Started");

    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
      if (e.ChangeType != WatcherChangeTypes.Changed)
      {
        DirectoryCopy(SourceFolder, DestinationFolder, false);
      }
      log.Info($"Changed: {e.FullPath}");
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
      string value = $"Created: {e.FullPath}";
      DirectoryCopy(SourceFolder, DestinationFolder, true);
      log.Info(value);
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e) =>
       log.Info($"Deleted: {e.FullPath}");

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
      log.Info($"Renamed:");
      DirectoryCopy(SourceFolder, DestinationFolder, true);
      log.Info($"    Old: {e.OldFullPath}");
      log.Info($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e) =>
        PrintException(e.GetException());

    private static void PrintException(Exception ex)
    {
      if (ex != null)
      {
        log.Info($"Message: {ex.Message}");
        log.Info("Stacktrace:");
        log.Info(ex.StackTrace);
        PrintException(ex.InnerException);
      }
    }


    public void Stop()
    {
      log.Info("File watcher stopped");
    }

    /// <summary>
    ///  Copy file between directories
    /// </summary>
    /// <param name="sourceDirName"></param>
    /// <param name="destDirName"></param>
    /// <param name="copySubDirs"></param>
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
      // Get the subdirectories for the specified directory.
      DirectoryInfo dir = new DirectoryInfo(sourceDirName);

      if (!dir.Exists)
      {
        throw new DirectoryNotFoundException(
            "Source directory does not exist or could not be found: "
            + sourceDirName);
      }

      DirectoryInfo[] dirs = dir.GetDirectories();

      // If the destination directory doesn't exist, create it.       
      Directory.CreateDirectory(destDirName);

      // Get the files in the directory and copy them to the new location.
      FileInfo[] files = dir.GetFiles();

      var file = files.OrderByDescending(f => f.LastWriteTime).First();

      string tempPath = Path.Combine(destDirName, file.Name);

      file.CopyTo(tempPath, true);
    }

    /// <summary>
    /// Adjust fileName
    /// </summary>
    /// <param name="nameOfFile"></param>
    /// <returns></returns>
    private static string SpecialFileName(string nameOfFile)
    {
      // Write file containing the date with an extension.
      string fileName = string.Format("nameOfFile" + "-{0:yyyy-MM-dd_hh-mm-ss-tt}.xml",
          DateTime.Now);
      return fileName;
    }

  }
}
