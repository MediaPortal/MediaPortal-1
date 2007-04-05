#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using SQLite.NET;
using MediaPortal.Ripper;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;
using Programs.Utils;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appItemAppExec.
  /// </summary>
  public class appItemAppExec : AppItem
  {
    public appItemAppExec(SQLiteClient initSqlDB)
      : base(initSqlDB)
    {
      // some nice defaults...
      UseShellExecute = true;
      UseQuotes = true;
      Startupdir = "%FILEDIR%";
    }

    public override bool FileEditorAllowed()
    {
      return false; // no editor allowed!
    }

    public override bool FileAddAllowed()
    {
      return false;
    }

    public override bool FilesCanBeFavourites()
    {
      return false;
    }

    public override void LaunchFile(MediaPortal.GUI.Library.GUIListItem item)
    {
      // Launch application
      ProcessStartInfo procStart = new ProcessStartInfo();
      // use the APPLICATION launcher and add current file information
      procStart.FileName = Filename; // filename of the application
      // set the arguments: one of the arguments is the fileitem-filename
      procStart.Arguments = " " + this.Arguments + " ";

      procStart.WorkingDirectory = Startupdir;
      if (procStart.WorkingDirectory.IndexOf("%FILEDIR%") != -1)
      {
        procStart.WorkingDirectory = procStart.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(Filename));
      }
      procStart.WindowStyle = this.WindowStyle;

      this.LaunchErrorMsg = "";
      try
      {
        DoPreLaunch();
        AutoPlay.StopListening();
        if (g_Player.Playing)
        {
          g_Player.Stop();
        }
        MediaPortal.Util.Utils.StartProcess(procStart, WaitForExit);
        GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
        AutoPlay.StartListening();
      }
      catch (Exception ex)
      {
        string ErrorString = String.Format("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
                                         procStart.FileName,
                                         procStart.Arguments,
                                         procStart.WorkingDirectory,
                                         ex.Message,
                                         ex.Source,
                                         ex.StackTrace);
        Log.Info(ErrorString);
        this.LaunchErrorMsg = ErrorString;
      }
      finally
      {
        DoPostLaunch();
      }
    }
  }
}
