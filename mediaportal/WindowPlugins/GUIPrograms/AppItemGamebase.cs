using System;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Programs.Utils;
using ProgramsDatabase;
using SQLite.NET;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for AppItemGamebase.
	/// </summary>
	public class AppItemGamebase: AppItem
	{
    GUIDialogProgress pDlgProgress = null;

    public AppItemGamebase(SQLiteClient initSqlDB): base(initSqlDB)
    {
      // nothing to create here...
    }

    private void ShowProgressDialog()
    {
      pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetHeading("importing gamebase file");
      pDlgProgress.SetLine(0, "importing gamebase file"); //todo: localize it!
      pDlgProgress.SetLine(1, "");
      pDlgProgress.SetLine(2, "");
      pDlgProgress.StartModal(GetID);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.Progress();
    }

    private void DoGamebaseImport(bool bGUIMode)
    {
      if (sqlDB == null)
        return ;
      if (this.AppID < 0)
        return ;
      if ((this.SourceType != myProgSourceType.GAMEBASE) || (Source == "") || (!File.Exists(Source)))
        return ;
      // show progress dialog and run the import...
      if (bGUIMode)
      {
        ShowProgressDialog();
      }
      try
      {
        MyGamebaseImporter objImporter = new MyGamebaseImporter(this, sqlDB);
        objImporter.OnReadNewFile += new MyGamebaseImporter.GamebaseEventHandler(ReadNewFile);
        try
        {
          objImporter.Start();
        }
        finally
        {
          objImporter.OnReadNewFile -= new MyGamebaseImporter.GamebaseEventHandler(ReadNewFile);
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
      DoGamebaseImport(bGUIMode);
      FixFileLinks();
      LoadFiles();
    }



	}
}
