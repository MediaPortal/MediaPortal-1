#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TvLibrary.Log;

namespace TvEngine
{
  public class TvMovieDatabaseConnection : IDisposable
  {
    private const char DELIMITER = '\0';

    private Process _Process = null;

    /// <summary>
    /// Open proccess
    /// </summary>
    /// <param name="strDbPath">Path to the datavse file</param>
    /// <returns>True if succesfull.</returns>
    public bool Open(string strDbPath)
    {
      try
      {
        ProcessStartInfo psi = new ProcessStartInfo()
        {
          Arguments = "\"" + strDbPath + '\"',
          FileName = "\"Plugins\\TvMovieDbReader.exe\"",
          UseShellExecute = false,
          ErrorDialog = false,
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          CreateNoWindow = true
        };

        Process pr = new Process
        {
          StartInfo = psi
        };

        //Start
        pr.Start();

        //Read response from the process
        string strLine = pr.StandardOutput.ReadLine();
        if (strLine == "OPEN")
        {
          this._Process = pr;
          Log.Debug("TvMovieDatabaseConnection: Open() Openned.");
          return true;
        }

        Log.Error("TvMovieDatabaseConnection: Open() Error: {0}", strLine);

        if (!pr.WaitForExit(5000))
          pr.Kill();
      }
      catch (Exception ex)
      {
        Log.Error("TvMovieDatabaseConnection: Open() Error: {0}", ex.Message);
      }

      return false;
    }

    /// <summary>
    /// Close openned process
    /// </summary>
    public void Close()
    {
      if (this._Process != null)
      {
        if (!this._Process.HasExited)
          this._Process.StandardInput.WriteLine("Close");

        if (!this._Process.WaitForExit(5000))
          this._Process.Kill();

        this._Process = null;

        Log.Debug("TvMovieDatabaseConnection: Close() Closed.");
      }
    }

    /// <summary>
    /// Get channel list
    /// </summary>
    /// <returns>List of channels</returns>
    public List<TVMChannel> GetChannels()
    {
      if (this._Process != null)
      {
        List<TVMChannel> result = new List<TVMChannel>();
        this._Process.StandardInput.WriteLine("GetChannels");
        while (true)
        {
          string strLine = this._Process.StandardOutput.ReadLine();

          if (strLine == "ROWS_END")
            break;

          if (strLine.StartsWith("ERROR: "))
          {
            Log.Error("TvMovieDatabaseConnection: GetChannels() Error: {0}", strLine);
            return null;
          }

          if (strLine.StartsWith("ROW: "))
          {
            string[] fields = Encoding.UTF8.GetString(Convert.FromBase64String(strLine.Substring(5))).Split(DELIMITER);
            if (fields.Length == 6)  //for details look in TvMovieDbReader project
            {
              result.Add(new TVMChannel(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]));
            }
            else
            {
              Log.Error("TvMovieDatabaseConnection: GetChannels() Invalid fields: {0}", strLine);
              return null;
            }
          }
          else
          {
            Log.Error("TvMovieDatabaseConnection: GetChannels() Invalid response: {0}", strLine);
            return null;
          }
        }
        return result;
      }

      return null;
    }

    /// <summary>
    /// Get EPG data from given channel
    /// </summary>
    /// <param name="strChannelName">Name of the channel</param>
    /// <returns>List of EPG data</returns>
    public List<string[]> GetChannelData(string strChannelName)
    {
      if (this._Process != null)
      {
        List<string[]> result = new List<string[]>();
        this._Process.StandardInput.WriteLine("GetChannelData \"" + strChannelName + '\"');
        while (true)
        {
          string strLine = this._Process.StandardOutput.ReadLine();

          if (strLine == "ROWS_END")
            break;

          if (strLine.StartsWith("ERROR: "))
          {
            Log.Error("TvMovieDatabaseConnection: GetChannelData() Error: {0}", strLine);
            return null;
          }

          if (strLine.StartsWith("ROW: "))
          {
            string[] fields = Encoding.UTF8.GetString(Convert.FromBase64String(strLine.Substring(5))).Split(DELIMITER);
            if (fields.Length == 25) //for details look in TvMovieDbReader project
            {
              result.Add(fields);
            }
            else
            {
              Log.Error("TvMovieDatabaseConnection: GetChannelData() Invalid fields: {0}", strLine);
              return null;
            }
          }
          else
          {
            Log.Error("TvMovieDatabaseConnection: GetChannelData() Invalid response: {0}", strLine);
            return null;
          }
        }
        return result;
      }

      return null;
    }

    public void Dispose()
    {
      this.Close();
    }
  }
}
