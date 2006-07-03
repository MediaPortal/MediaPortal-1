/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appMyFileMeedio.
  /// </summary>
  public class appItemMyFileMLF: AppItem
  {
    GUIDialogProgress pDlgProgress = null;

    public appItemMyFileMLF(SQLiteClient initSqlDB): base(initSqlDB)
    {
      // nothing to create here...
    }

    private void ShowProgressDialog()
    {
      pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowWaitCursor = true;
      pDlgProgress.SetHeading(13003);
      pDlgProgress.SetLine(0, 13003); //"importing *.mlf file
      pDlgProgress.SetLine(1, "");
      pDlgProgress.SetLine(2, "");
      pDlgProgress.StartModal(GetID);
      pDlgProgress.Progress();
    }


    private void DoMyFileMeedioImport(bool bGUIMode)
    {
      if (sqlDB == null)
        return ;
      if (this.AppID < 0)
        return ;
      if ((this.SourceType != myProgSourceType.MYFILEMEEDIO) || (Source == "") || (!File.Exists(Source)))
        return ;
      // show progress dialog and run the import...
      if (bGUIMode)
      {
        ShowProgressDialog();
      }
      try
      {
        MyFileMeedioImporter objImporter = new MyFileMeedioImporter(this, sqlDB);
        objImporter.OnReadNewFile += new MyFileMeedioImporter.MlfEventHandler(ReadNewFile);
        try
        {
          objImporter.Start();
        }
        finally
        {
          objImporter.OnReadNewFile -= new MyFileMeedioImporter.MlfEventHandler(ReadNewFile);
        }
      }
      finally
      {
        if (bGUIMode)
        {
          pDlgProgress.Close();
        }
      }
    }

    private void ReadNewFile(string strFileName, int curPos, int maxPos)
    {
      if (pDlgProgress != null)
      {
        pDlgProgress.SetLine(2, String.Format("{0} {1}", GUILocalizeStrings.Get(13005), strFileName)); // "last imported file {0}"
        if ((curPos > 0) && (maxPos > 0))
        {
          int perc = 100 * curPos / maxPos;
          pDlgProgress.SetPercentage(perc);
        }
        pDlgProgress.Progress();
      }
      SendRefreshInfo(String.Format("{0} {1}", GUILocalizeStrings.Get(13005), strFileName));
    }

    override public bool RefreshButtonVisible()
    {
      return true;
    }

    override public bool ProfileLoadingAllowed()
    {
      return true;
    }

    override public void Refresh(bool bGUIMode)
    {
      base.Refresh(bGUIMode);
      DeleteFiles();
      DoMyFileMeedioImport(bGUIMode);
      FixFileLinks();
      LoadFiles();
    }



  }
}
