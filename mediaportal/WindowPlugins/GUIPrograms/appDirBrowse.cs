using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using SQLite.NET;

using Programs.Utils;
using MediaPortal.Ripper;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for appDirBrowse.
	/// </summary>
	public class appItemDirBrowse: ProgramsDatabase.AppItem
	{
		VirtualDirectory  m_directory = new VirtualDirectory();
		ProgramComparer pc = new ProgramComparer(); // slightly hacky: pc replaces the base.dbPc object....

		public appItemDirBrowse(SQLiteClient paramDB): base(paramDB)
		{
		}

		override public void LoadFiles()
		{
			// nothing to load, because directory is directly displayed
			// no FileItems!
		}


		override public bool FileEditorAllowed()
		{
			return false;  // no editor allowed!
		}

		override public bool FileAddAllowed()
		{
			return false;  // and of course, no file adding allowed!
		}

		override public bool FilesCanBeFavourites()
		{
			return false;  // no files, no links!
		}

		override public bool ProfileLoadingAllowed()
		{
			return true;
		}

		String GetFolderThumb( String fileName )
		{
			string strFolderThumb = "";
			if (ImageDirs.Length > 0)
			{
				string strMainImgDir = ImageDirs[0];
				strFolderThumb = strMainImgDir + "\\" + fileName;
				strFolderThumb = Path.ChangeExtension(strFolderThumb, ".jpg");
				if (!System.IO.File.Exists(strFolderThumb))
				{
					strFolderThumb = Path.ChangeExtension(strFolderThumb, ".gif");
				}
				if( !System.IO.File.Exists(strFolderThumb) )
				{
					strFolderThumb = Path.ChangeExtension(strFolderThumb, ".png");
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


		int LoadDirectory(string strNewDirectory)
		{
			ValidExtensions = ValidExtensions.Replace(" ", "");
			ArrayList mExtensions = new ArrayList( this.ValidExtensions.Split( ',' ) );
			// allow spaces between extensions...
			m_directory.SetExtensions(mExtensions);
			ArrayList arrFiles = m_directory.GetDirectory( strNewDirectory );

			int  iTotalItems=0;
			foreach (GUIListItem file in arrFiles)
			{
				Utils.SetDefaultIcons( file );
				if (file.IsFolder)
				{
					file.ThumbnailImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
					file.IconImageBig = GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png";
					file.IconImage = GUIGraphicsContext.Skin+@"\media\DefaultFolderNF.png";
				}
				else
				{
					string strFolderThumb= GetFolderThumb( file.Label );
					file.ThumbnailImage=strFolderThumb;
					file.IconImageBig=strFolderThumb;
					file.IconImage=strFolderThumb;
				}
				
				if (file.Label != ProgramUtils.cBackLabel)
				{
					file.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(file_OnItemSelected);
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_VIEW,file);
					iTotalItems++;
				}
			}

			return iTotalItems;
		}

		private void file_OnItemSelected(GUIListItem item, GUIControl parent)
		{
			GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
			if (filmstrip==null) return;
			string thumbName = "";
			if ((item.ThumbnailImage != GUIGraphicsContext.Skin+@"\media\DefaultFolderBig.png")
				&& (item.ThumbnailImage != ""))
			{
				// only show big thumb if there is really one....
				thumbName = item.ThumbnailImage;
			}
			filmstrip.InfoImageFileName= thumbName;
		}


		override public int DisplayFiles(string Filepath)
		{
			int Total = 0;
			if (Filepath == "")
			{
				// normal: load the main filelist of the application
				Total = LoadDirectory(this.FileDirectory);
			}
			else
			{
				// subfolder is activated: load the filelist of the subfolder
				Total = LoadDirectory(Filepath);
			}
			return Total;
		}


		override public string DefaultFilepath()
		{
			return this.FileDirectory ; 
		}

		override public void LaunchFile(GUIListItem item)
		{
			string curFilename = item.Path;
			Process proc = new Process();
			if (Filename != "")
			{
				proc.StartInfo.FileName = this.Filename; // application
				proc.StartInfo.Arguments = this.Arguments;
				if (UseQuotes) 
				{
					curFilename = " \"" + item.Path + "\"";
				}
				if (proc.StartInfo.Arguments.IndexOf("%FILE%") == -1)
				{
					// no placeholder found => default handling: add the fileitem as the last argument
					proc.StartInfo.Arguments = proc.StartInfo.Arguments + curFilename;
				}
				else
				{
					// placeholder found => replace the placeholder by the correct filename
					proc.StartInfo.Arguments = proc.StartInfo.Arguments.Replace("%FILE%", curFilename);
				}
			}
			else 
			{
				// application has no filename given => simply ShellExecute the item....
				proc.StartInfo.FileName = item.Path;
			}
			proc.StartInfo.WorkingDirectory  = Startupdir;
			if (proc.StartInfo.WorkingDirectory.IndexOf("%FILEDIR%") != -1)
			{
				proc.StartInfo.WorkingDirectory = proc.StartInfo.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(item.Path));
			}
			proc.StartInfo.UseShellExecute = UseShellExecute;
			proc.StartInfo.WindowStyle = this.WindowStyle;
			try
			{
				AutoPlay.StopListening();
				proc.Start(); // start the app
				if (WaitForExit)
				{
					proc.WaitForExit(); // stop MP
				}
				GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters); // and restore the DirectX screen (in case the app was a DirectX application itself!)
				AutoPlay.StartListening();

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

		override public void OnSort(GUIFacadeControl view, bool bDoSwitch)
		{
			// todo: polymorph it! pc => dbPc
			if (bDoSwitch)
			{
				pc.updateState();
			}
			view.Sort(pc);
		}

		override public void OnSortToggle(GUIFacadeControl view)
		{
			pc.bAsc = (!pc.bAsc);
			view.Sort(pc);
		}

		override public string CurrentSortTitle()
		{
			return pc.currentSortMethodAsText;
		}

		override public bool GetCurrentSortIsAscending()
		{
			return pc.bAsc;
		}

		override public void SetCurrentSortIndex(int newValue)
		{
			pc.currentSortMethodIndex = newValue;
		}

		override public void SetCurrentSortIsAscending(bool newValue)
		{
			pc.bAsc = newValue;
		}

		override public int GetCurrentSortIndex()
		{
			return pc.currentSortMethodIndex;
		}



	}
}
