/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Diagnostics;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appItemMameDirect.
	/// </summary>
	public class appItemMameDirect: AppItem
	{

    GUIDialogProgress pDlgProgress = null;
    bool importOriginalsOnly = true; 

    public appItemMameDirect(SQLiteClient initSqlDB): base(initSqlDB)
    {
      // some nice working mame defaults...
      Filename = "<yourpath>\\mame32.exe";
      UseShellExecute = true;
      UseQuotes = true;
      ImportValidImagesOnly = true;
      ValidExtensions = ".zip";
      Arguments = "-joy -skip_disclaimer -skip_gameinfo";
      WindowStyle = ProcessWindowStyle.Minimized;
    }

    public bool ImportOriginalsOnly
    {
      get { return importOriginalsOnly; }
      set { importOriginalsOnly = value; }
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
      DoMameImport(bGUIMode);
      FixFileLinks();
      LoadFiles();
    }

    void DoMameImport(bool bGUIMode)
    {
      if (sqlDB == null)
        return ;
      if (this.AppID < 0)
        return ;
      if (this.SourceType != myProgSourceType.MAMEDIRECT)
        return ;
      if (!File.Exists(this.Filename)) // no "mame.exe"
        return ;
      if (bGUIMode)
      {
        ShowProgressDialog();
      }
      try
      {
        MyMameImporter objImporter = new MyMameImporter(this, sqlDB);
        objImporter.OnReadNewFile += new MyMameImporter.MyEventHandler(ReadNewFile);
        objImporter.OnSendMessage += new MyMameImporter.MyEventHandler(DisplayText);
        try
        {
          objImporter.Start();
        }
        finally
        {
          objImporter.OnReadNewFile -= new MyMameImporter.MyEventHandler(ReadNewFile);
          objImporter.OnSendMessage -= new MyMameImporter.MyEventHandler(DisplayText);
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

    void ReadNewFile(string strFileName, int curPos, int maxPos)
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

    void DisplayText(string msg, int curPos, int maxPos)
    {
      SendRefreshInfo(msg);
    }

    void ShowProgressDialog()
    {
      pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetHeading("importing MAME roms");
      pDlgProgress.SetLine(0, "importing MAME roms"); //"importing *.my file
      pDlgProgress.SetLine(1, "");
      pDlgProgress.SetLine(2, "");
      pDlgProgress.StartModal(GetID);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.Progress();
    }



	}
}
