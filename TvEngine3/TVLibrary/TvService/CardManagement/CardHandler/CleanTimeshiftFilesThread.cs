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

using System;
using System.IO;
using System.Threading;
using MediaPortal.CoreServices;

namespace TvService
{
  /// <summary>
  /// Thread that tries to cleanup timeshift files
  /// </summary>
  internal class CleanTimeshiftFilesThread
  {
    /// <summary>
    /// The folder
    /// </summary>
    private readonly string _folder;

    /// <summary>
    /// Name of the file
    /// </summary>
    private readonly string _fileName;

    /// <summary>
    /// Sleep intervall
    /// </summary>
    private const int _sleepIntervall = 5000;

    /// <summary>
    /// Number of retries
    /// </summary>
    private const int _numOfRetries = 5;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    public CleanTimeshiftFilesThread(string folder, string fileName)
    {
      _folder = folder;
      _fileName = fileName;
    }

    /// <summary>
    /// Start method for the cleanup thread.
    /// It tries it up to 5 times with an intervall of 5 seconds
    /// </summary>
    public void CleanTimeshiftFiles()
    {
      for (int i = 0; i < _numOfRetries; i++)
      {
        if (CleanTimeShiftFiles())
        {
          return;
        }
        Thread.Sleep(_sleepIntervall);
      }
    }

    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    private bool CleanTimeShiftFiles()
    {
      try
      {
        GlobalServiceProvider.Instance.Get<ILogger>().Debug(@"card: delete timeshift files {0}\{1}", _folder, _fileName);
        string[] files = Directory.GetFiles(_folder);
        for (int i = 0; i < files.Length; ++i)
        {
          if (files[i].IndexOf(_fileName) >= 0)
          {
            GlobalServiceProvider.Instance.Get<ILogger>().Debug("card:   trying to delete {0}", files[i]);
            File.Delete(files[i]);
            GlobalServiceProvider.Instance.Get<ILogger>().Debug("card:   deleted file {0}", files[i]);
          }
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
  }
}