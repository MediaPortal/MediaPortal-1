using System;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using GUIPrograms;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appDirBrowse.
	/// </summary>
	public class appItemDirBrowse: ProgramsDatabase.AppItem
	{
		Stack mDirectories = new Stack();
		VirtualDirectory  m_directory = new VirtualDirectory();
		DirBrowseComparer pc = new DirBrowseComparer(); // slightly hacky: pc replaces the base.dbPc object....

		public appItemDirBrowse(SQLiteClient paramDB): base(paramDB)
		{
			mDirectories.Clear();
		}

		override protected void LoadFiles()
		{
			// nothing to load, because directory is directly displayed
			// no FileItems!
		}


		override public bool FileEditorAllowed()
		{
			return false;  // no editor allowed!
		}

		String GetFolderThumb( String fileName )
		{
			string strFolderThumb = "";
			if (ImageDirs.Length > 0)
			{
				string strMainImgDir = ImageDirs[0];
				strFolderThumb= strMainImgDir + "\\" + fileName + ".jpg";
				if (!System.IO.File.Exists(strFolderThumb))
				{
					strFolderThumb = strMainImgDir+ "\\" + fileName + ".gif";
				}
				if( !System.IO.File.Exists(strFolderThumb) )
				{
					strFolderThumb = strMainImgDir + "\\" + fileName + ".png";
				}
				if( !System.IO.File.Exists( strFolderThumb ) )
				{
					strFolderThumb = strMainImgDir + "\\default.png";
				}
				if( !System.IO.File.Exists( strFolderThumb ) )
				{
					strFolderThumb = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
				}
			}
			else
			{
				strFolderThumb = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
			}
			return strFolderThumb;
		}


		void LoadDirectory(string strNewDirectory)
		{
			ValidExtensions = ValidExtensions.Replace(" ", "");
			ArrayList mExtensions = new ArrayList( this.ValidExtensions.Split( ',' ) );
			// allow spaces between extensions...
			m_directory.SetExtensions(mExtensions);
			mDirectories.Push( strNewDirectory );
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST ); 
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_THUMBS );
			ArrayList arrFiles = m_directory.GetDirectory( strNewDirectory );

			int  iTotalItems=0;
			foreach (GUIListItem file in arrFiles)
			{
				Utils.SetDefaultIcons( file );
				
				if (file.Label == ProgramUtils.cBackLabel)
				{
					file.ThumbnailImage=GUIGraphicsContext.Skin+@"\media\DefaultFolderBackBig.png";
					file.IconImageBig=GUIGraphicsContext.Skin+@"\media\DefaultFolderBack.png";
					file.IconImage=GUIGraphicsContext.Skin+@"\media\DefaultFolderBack.png";
				}
				else
				{
					string strFolderThumb= GetFolderThumb( file.Label );
					file.ThumbnailImage=strFolderThumb;
					file.IconImageBig=strFolderThumb;
					file.IconImage=strFolderThumb;
				}
				

				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,file);
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,file);
				iTotalItems++;
			}


//			GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS );
//			if( pControl != null )
//				pControl.ShowBigIcons( true );

			string strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}


		override public void DisplayFiles(GUIListItem itemParent)
		{
			if (itemParent == null) 
			{
				// display the "root" filelist of the application
				LoadDirectory(this.FileDirectory);
			}
			else 
			{
				// display the filelist of a subfolder
				LoadDirectory(itemParent.Path); 
			}
		}


		override public bool BackItemClick(GUIListItem itemBack)
		{
			if (mDirectories.Count > 1)
			{
				mDirectories.Pop();
				LoadDirectory( (string) mDirectories.Pop() );
				return false; // display parent directory
			}
			else
			{
				mDirectories.Clear();
				return true; // return to toplevel (=> applicationlist)
			}
		}
		

		override public void LaunchFile(GUIListItem item)
		{
			string strFilename = item.Path;
			Process proc = new Process();
			if (Filename != "")
			{
				proc.StartInfo.FileName = this.Filename; // application
				proc.StartInfo.Arguments = this.Arguments;
				if (UseQuotes) 
				{
					strFilename = " \"" + item.Path + "\"";
				}
				if (proc.StartInfo.Arguments.IndexOf("%FILE%") == -1)
				{
					// no placeholder found => default handling: add the fileitem as the last argument
					proc.StartInfo.Arguments = proc.StartInfo.Arguments + strFilename;
				}
				else
				{
					// placeholder found => replace the placeholder by the correct filename
					proc.StartInfo.Arguments = proc.StartInfo.Arguments.Replace("%FILE%", strFilename);
				}
			}
			else 
			{
				// application has no filename given => simply ShellExecute the item....
				proc.StartInfo.FileName = item.Path;
			}
			proc.StartInfo.WorkingDirectory  = Startupdir;
			proc.StartInfo.UseShellExecute = UseShellExecute;
			proc.StartInfo.WindowStyle = this.WindowStyle;
			try
			{
				proc.Start();
//				Log.Write("myPrograms: DEBUG LOG program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n",
//					proc.StartInfo.FileName, 
//					proc.StartInfo.Arguments, 
//					proc.StartInfo.WorkingDirectory);
			}
			catch (Exception ex)
			{
				Log.Write("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
					proc.StartInfo.FileName, 
					proc.StartInfo.Arguments, 
					proc.StartInfo.WorkingDirectory, 
					ex.Message, 
					ex.Source, 
					ex.StackTrace);
			}   

		}

		override public void OnInfo(GUIListItem item)
		{
			// no info screen for directory items
		}

		override public void OnSort(GUIListControl list, GUIThumbnailPanel panel)
		{
			// todo: polymorph it! pc => dbPc
			pc.updateState();
			list.Sort(pc);
			panel.Sort(pc);
		}

		override public void OnSortToggle(GUIListControl list, GUIThumbnailPanel panel)
		{
			pc.bAsc = (!pc.bAsc);
			list.Sort(pc);
			panel.Sort(pc);
		}

		override public string CurrentSortTitle()
		{
			return pc.currentSortMethodAsText;
		}

		override public bool CurrentSortIsAscending()
		{
			return pc.bAsc;
		}



	}
}
