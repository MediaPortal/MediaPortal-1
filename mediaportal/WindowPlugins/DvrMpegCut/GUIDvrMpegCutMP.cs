/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using DirectShowLib.SBE;

namespace DvrMpegCutMP
{
	public class GUIDvrMpegCutMP : GUIWindow
	{
		readonly int windowID = 170601;
		

		#region GUIControls
		[SkinControlAttribute(24)]
		protected GUIButtonControl backBtn = null;
		//[SkinControlAttribute(25)]
		//protected GUIButtonControl cancelBtn = null;
		[SkinControlAttribute(32)]
		protected GUILabelControl startTime = null;
		[SkinControlAttribute(34)]
		protected GUILabelControl oldDurationLbl = null;
		[SkinControlAttribute(101)]
		protected GUIListControl videoListLct = null;
		[SkinControlAttribute(99)]
		protected GUIVideoControl videoWindow = null;
		#endregion

		#region Own Variables
		const int maxDrives = 50;
		int cntDrives = 0;
		string[] drives = new string[maxDrives];
		string currentFolder = "";
		ArrayList extensions;
		DvrMpegCutPreview cutScr;
		#endregion

		public GUIDvrMpegCutMP()
		{ }

		#region Overrides
		public override int GetID
		{
			get
			{
				return windowID;
			}
			set
			{
			}
		}
		public override bool Init()
		{
			try
			{

				bool init = Load(GUIGraphicsContext.Skin + @"\DvrCutMP.xml");
				if (init)
				{
					GetDrives();
				}
				return init;
			}
			catch
			{
				//MessageBox.Show("Fehler","Fehler",MessageBoxButtons.OKCancel);
				return false;
			}
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			try 
			{
				extensions = new ArrayList();
				extensions.Add(".dvr-ms");
				extensions.Add(".mpeg");
				extensions.Add(".mpg");
				videoListLct.Clear();
				videoListLct.UpdateLayout();
				LoadDrives();
			}
			catch (Exception e)
			{
				Log.WriteFile(Log.LogType.Error, "DvrMpegCut: (OnPageLoad) " + e.StackTrace);
			}

		}
		protected override void OnPageDestroy(int new_windowId)
		{
			g_Player.Release();
			base.OnPageDestroy(new_windowId);
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == backBtn)
			{
				//Abbrechen();
				GUIWindowManager.ShowPreviousWindow();
			}
			/*if (control == abbrechenBtn)
			{
				Abbrechen();
			}*/
			if (control == videoListLct)
			{
				GUIListItem item = videoListLct.SelectedListItem;
				//System.Windows.Forms.MessageBox.Show(item.Path);
				if (!item.IsFolder)
				{
					ToCutScreen(item.Path);
				}

				else if (item.Label.Substring(1, 1) == ":")  // is a drive
				{
					currentFolder = item.Label;
					if (currentFolder != String.Empty)
						LoadListControl(currentFolder, extensions);
					else
						LoadDrives();
				}
				else
					LoadListControl(item.Path, extensions);
				if (item.Path == "")
				{
					LoadDrives();
				}
			}
			base.OnClicked(controlId, control, actionType);
		}

		#endregion

	
		enum DriveType
		{
			Removable = 2,
			Fixed = 3,
			RemoteDisk = 4,
			CD = 5,
			DVD = 5,
			RamDisk = 6
		}

		/// <summary>
		/// get the number of drives
		/// </summary>
		private void GetDrives()
		{
			cntDrives = 0;
			foreach (string drive in Environment.GetLogicalDrives())
			{
				switch ((DriveType)MediaPortal.Util.Utils.getDriveType(drive))
				{
					case DriveType.Removable:
					case DriveType.CD:
					//case DriveType.DVD:
					case DriveType.Fixed:
					case DriveType.RemoteDisk:
					case DriveType.RamDisk:
						drives[cntDrives] = drive;
						cntDrives++;
						break;
				}
			}
		}

		/// <summary>
		/// Add the drives to the listcontrol with the matching icons
		/// </summary>
		private void LoadDrives()
		{
			try
			{
				currentFolder = "";
				for (int i = 0; i < cntDrives; i++)
				{
					GUIListItem item = new GUIListItem(drives[i]);
					item.IsFolder = true;
					item.Path = drives[i];
					MediaPortal.Util.Utils.SetDefaultIcons(item);
					videoListLct.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Error, "DvrMpegCut: (LoadDrives) " + ex.StackTrace);
			}
		}

		#region Eventhandler

		/// <summary>
		/// Load the list control with the items of the specified directory
		/// </summary>
		/// <param name="folder">Path of the director to load</param>
		/// <param name="exts">the extensions to show</param>
		private void LoadListControl(string folder, ArrayList exts)
		{
			try
			{
				if (folder != null && folder != "")
					folder = Utils.RemoveTrailingSlash(folder);
				
				VirtualDirectory Directory;
				ArrayList itemlist;
				Directory = new VirtualDirectory();
				Directory.SetExtensions(exts);
				itemlist = Directory.GetDirectory(folder);
				videoListLct.Clear();
				foreach (GUIListItem item in itemlist)
				{
					if (!item.IsFolder) // if item a folder
					{
						GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
						pItem.FileInfo = item.FileInfo;
						pItem.IsFolder = false;
						pItem.Path = String.Format(@"{0}\{1}", folder, item.FileInfo.Name);
						videoListLct.Add(pItem);
					}
					else
					{
						GUIListItem pItem = new GUIListItem(item.Label);
						pItem.IsFolder = true;
						pItem.Path = String.Format(@"{0}\{1}", folder, item.Label);
						if (item.Label == "..")
						{
							string prevFolder = "";
							int pos = folder.LastIndexOf(@"\");
							if (pos >= 0) prevFolder = folder.Substring(0, pos);
							pItem.Path = prevFolder;
						}
						Utils.SetDefaultIcons(pItem);
						videoListLct.Add(pItem);
					}
				}
				currentFolder = folder;
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Error,"DvrMpegCut: (LoadListControl) "+ ex.Message);
			}

		}

		protected void ToCutScreen(string filepath)
		{
			try
			{
				if (filepath == null)
					System.Windows.Forms.MessageBox.Show("No path");
				if (cutScr == null)
				{
					cutScr = new DvrMpegCutPreview(filepath);
					cutScr.Init();
					if (GUIWindowManager.GetWindow(cutScr.GetID) == null)
					{
						GUIWindow win = (GUIWindow)cutScr;
						GUIWindowManager.Add(ref win);
					}
				}
				else
					cutScr.CutFileName = filepath;

				GUIWindowManager.ActivateWindow(cutScr.GetID);
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Error, "DvrMpegCut: (ToCutScreen) " + ex.Message);
			}
		}
		#endregion
	}
}
