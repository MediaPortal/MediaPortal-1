#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  /// <summary>
  /// Thread that tries to cleanup timeshift files
  /// </summary>
  internal class CleanTimeshiftFilesThread
  {
    /// <summary>
    /// Name of the file
    /// </summary>
    private readonly string _fileName;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public CleanTimeshiftFilesThread(string fileName)
    {
      _fileName = fileName;
    }

    /// <summary>
    /// Start method for the cleanup thread.
    /// It tries it up to 5 times with an intervall of 5 seconds
    /// </summary>
    public void CleanTimeshiftFiles()
    {
      for (int i = 0; i < 5; i++)
      {
        try
        {
          this.LogDebug(@"card: delete timeshift files {0}", _fileName);
          foreach (string fileName in Directory.GetFiles(Path.GetDirectoryName(_fileName)))
          {
            // TODO Ideally we should avoid making assumptions about the format
            // of the buffer file names, because they're not in our control.
            if (fileName.StartsWith(_fileName))
            {
              this.LogDebug("card:   trying to delete {0}", fileName);
              File.Delete(fileName);
              this.LogDebug("card:   deleted file {0}", fileName);
            }
          }
          return;
        }
        catch
        {
        }
        Thread.Sleep(5000);
      }
    }
  }
}