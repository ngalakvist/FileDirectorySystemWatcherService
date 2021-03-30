using SE.MDH.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
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
using System.Security.Principal;

namespace FileDirectorySystemWatcher
{
  public static class InstallerConfig
  {

    private static readonly ILogWriter log = Logger.Get(MethodBase.GetCurrentMethod().DeclaringType);
    private static string SourceFolder = ConfigurationManager.AppSettings["sourceFolder"];
    private static string DestinationFolder = ConfigurationManager.AppSettings["destinationFolder"];

    public static void Install()
    {

      SetEveryoneAccess(SourceFolder);
      SetEveryoneAccess(DestinationFolder);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirName"></param>
    /// <returns></returns>
    private static bool SetEveryoneAccess(string dirName)
    {
      try
      {
        // Make sure directory exists
        if (Directory.Exists(dirName) == false)
          throw new Exception(string.Format("Directory {0} does not exist, so permissions cannot be set.", dirName));

        // Get directory access info
        DirectoryInfo dinfo = new DirectoryInfo(dirName);
        DirectorySecurity dSecurity = dinfo.GetAccessControl();

        // Add the FileSystemAccessRule to the security settings. 
        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

        // Set the access control
        dinfo.SetAccessControl(dSecurity);

        log.Info($"Everyone FullControl Permissions were set for directory{ dirName}");

        return true;

      }
      catch (Exception ex)
      {
        log.Error(ex.Message);
        return false;
      }
    }
  }
}