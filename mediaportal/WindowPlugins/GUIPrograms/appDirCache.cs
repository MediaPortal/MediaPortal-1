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
	/// Summary description for appItemDirCache.
	/// </summary>
	public class appItemDirCache: ProgramsDatabase.AppItem
	{

		GUIDialogProgress pDlgProgress = null;
		VirtualDirectory  m_directory = new VirtualDirectory();
		
		public appItemDirCache(SQLiteClient paramDB): base(paramDB)
		{
		}

		private void ShowProgressDialog()
		{
			pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			pDlgProgress.SetHeading("scanning directory");
			pDlgProgress.SetLine(0, "scanning directory");		//todo: localize! 13004...
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, "");
			pDlgProgress.StartModal(GetID);
			pDlgProgress.Progress();
		}


		private string GetThumbsFile(GUIListItem guiFile)
		{
			string strFolderThumb = "";
			if (ImageDirs.Length > 0)
			{
				string strMainImgDir = ImageDirs[0];
				strFolderThumb= strMainImgDir + "\\" + guiFile.Label + ".jpg";
				if (!System.IO.File.Exists(strFolderThumb))
				{
					strFolderThumb = strMainImgDir+ "\\" + guiFile.Label + ".gif";
				}
				if( !System.IO.File.Exists(strFolderThumb) )
				{
					strFolderThumb = strMainImgDir + "\\" + guiFile.Label + ".png";
				}
			}
			return strFolderThumb;
		}

		private void ImportFileItem(GUIListItem guiFile)
		{
			string strImageFile = GetThumbsFile(guiFile);
			FileItem curFile = new FileItem(m_db);
			curFile.FileID = -1; // to force an INSERT statement when writing the item
			curFile.AppID = this.AppID;
			curFile.Title = guiFile.Label;
			curFile.Filename = guiFile.Path;
			curFile.Imagefile = strImageFile;
			// not imported properties => set default values
			curFile.ManualFilename = "";
			curFile.LastTimeLaunched = DateTime.MinValue;
			curFile.LaunchCount = 0;
			curFile.Write();
			pDlgProgress.SetLine(2, String.Format("{0} {1}", GUILocalizeStrings.Get(13005), curFile.Title)); // "last imported file {0}"
			pDlgProgress.Progress();
		}

		private void DoDirCacheImport()
		{
			if (m_db==null) return;
			if (this.AppID < 0) return;
			if (this.SourceType != myProgSourceType.DIRCACHE) return;
			ShowProgressDialog();
			try
			{
				ValidExtensions = ValidExtensions.Replace(" ", "");
				ArrayList mExtensions = new ArrayList( this.ValidExtensions.Split( ',' ) );
				// allow spaces between extensions...
				m_directory.SetExtensions(mExtensions);
				ArrayList arrFiles = m_directory.GetDirectory( this.FileDirectory );

				foreach (GUIListItem file in arrFiles)
				{
					if (!file.IsFolder)
					{
						ImportFileItem(file);
					}
				}
			}
			finally
			{
				pDlgProgress.Close();
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
			DoDirCacheImport();
			LoadFiles();
		}
    }

}
