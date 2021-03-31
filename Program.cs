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
using SE.MDH.Logging;
using System;
using Topshelf;

namespace FileDirectorySystemWatcher
{
  internal class Program
  {
    public static void Main()
    {
      Logger.CurrentHostLoggerConfigurator.UseLog4Net(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
      var rc = HostFactory.Run(x =>
      {
        x.Service<FileWatcherService>(s =>
        {
          s.ConstructUsing(name => new FileWatcherService());
          s.WhenStarted(tc => tc.Start());
          s.WhenStopped(tc => tc.Stop());
          s.WhenShutdown(service => service.Stop());
        });
        x.SetDescription("Filesystemwatcher som lyssnar på update-händelse");
        x.SetDisplayName("FileDirectoryWatcher");
        x.SetServiceName("FileSystemWatcher");
        x.EnableShutdown();
        x.RunAsLocalSystem();
        x.StartAutomatically();
        x.EnableServiceRecovery(recov =>
        {
          recov.OnCrashOnly();
          // Startar om tjänsten efter 2 min.
          recov.RestartService(2);
          // Startar om tjänsten efter 5 min.
          recov.RestartService(5);
          // Startar om tjänsten efter 10 min.
          recov.RestartService(10);
          // Återställer perioden efter en dag.
          recov.SetResetPeriod(1);
        });
      });

      var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
      Environment.ExitCode = exitCode;
    }
  }
}
