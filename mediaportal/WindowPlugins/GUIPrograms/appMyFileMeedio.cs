using System;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;


namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appMyFileMeedio.
	/// </summary>
	public class appItemMyFileMLF: ProgramsDatabase.AppItem
	{
		GUIDialogProgress pDlgProgress = null;

		public appItemMyFileMLF(SQLiteClient paramDB): base(paramDB)
		{
			// nothing to create here...
		}

		private void ShowProgressDialog()
		{
			pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			pDlgProgress.SetHeading(13003);
			pDlgProgress.SetLine(0, 13003);							//"importing *.mlf file
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, "");
			pDlgProgress.StartModal(GetID);
			pDlgProgress.Progress();
		}

	
		private void DoMyFileMeedioImport(bool bGUIMode)
		{
			if (m_db==null) return;
			if (this.AppID < 0) return;
			if ((this.SourceType != myProgSourceType.MYFILEMEEDIO) || (Source == "") || (!System.IO.File.Exists(Source))) return;
			// show progress dialog and run the import...
			if (bGUIMode)
			{
				ShowProgressDialog();
			}
			try
			{
				MyFileMeedioImporter objImporter = new MyFileMeedioImporter(this, m_db);
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

		private void ReadNewFile(string strFileName)
		{
			if (pDlgProgress != null)
			{
				pDlgProgress.SetLine(2, String.Format("{0} {1}", GUILocalizeStrings.Get(13005), strFileName)); // "last imported file {0}"
				pDlgProgress.Progress();
			}
			SendRefreshInfo(String.Format("{0} {1}", GUILocalizeStrings.Get(13005), strFileName));
		}

		override public bool RefreshButtonVisible()
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
