#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.IO;
using MediaPortal.Support;
using NUnit.Framework;

namespace MediaPortal.Tests.Support
{
  [TestFixture]
  public class MediaPortalLogsTests
  {
    private string outputDir = "Support\\TestData\\TestOutput";
    private string logFile = "Support\\TestData\\TestOutput\\MediaPortal.log";

    [SetUp]
    public void Init()
    {
      Directory.CreateDirectory(outputDir);
      foreach (string file in Directory.GetFiles(outputDir))
      {
        File.Delete(file);
      }
    }

    [Test]
    public void CreateLogsWithNoErrors()
    {
      MediaPortalLogs mplogs = new MediaPortalLogs("Support\\TestData");
      mplogs.CreateLogs(outputDir);
      Assert.IsTrue(File.Exists(outputDir + "\\MediaPortal.log"), "Log file not copied!");
    }

    [Test]
    public void CopyOverExistingFiles()
    {
      MediaPortalLogs mplogs = new MediaPortalLogs("Support\\TestData");
      FileHelper.Touch(logFile);
      mplogs.CreateLogs(outputDir);
    }
  }
}