using System;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using GUIPrograms;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appMyFileIni.
	/// </summary>
	public class appItemMyFileINI: ProgramsDatabase.AppItem
	{

		GUIDialogProgress pDlgProgress = null;
		
		public appItemMyFileINI(SQLiteClient paramDB): base(paramDB)
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private void ShowProgressDialog()
		{
			pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			pDlgProgress.SetHeading(13004);
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, "");
			pDlgProgress.StartModal(GetID);
			pDlgProgress.Progress();
		}

		private void DoMyFileIniImport()
		{
			if (m_db==null) return;
			if (this.AppID < 0) return;
			if ((this.SourceType != myProgSourceType.MYFILEINI) || (Source == "") || (!System.IO.File.Exists(Source))) return;
			ShowProgressDialog();
			try
			{
				MyFileIniImporter objImporter = new MyFileIniImporter(this, m_db);
				objImporter.OnReadNewFile += new MyFileIniImporter.MyEventHandler(ReadNewFile);
				try
				{
					objImporter.Start();
				}
				finally
				{
					objImporter.OnReadNewFile -= new MyFileIniImporter.MyEventHandler(ReadNewFile);
				}
			}
			finally
			{
				pDlgProgress.Close();
			}

		}

		private void ReadNewFile(string strFileName)
		{
			if (pDlgProgress != null)
			{
				pDlgProgress.SetLine(2, String.Format("{0} {1}", GUILocalizeStrings.Get(13005), strFileName)); // "last imported file {0}"
				pDlgProgress.Progress();
			}
		}

		override public bool RefreshButtonVisible()
		{
			return true;
		}

		override public void Refresh()
		{
			base.Refresh();
			DeleteFiles();
			DoMyFileIniImport();
			LoadFiles();
		}

	}
}
