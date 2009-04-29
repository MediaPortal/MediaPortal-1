#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#region Usings
using System;
using System.Collections;
using System.Globalization;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using TvPlugin;
using TvDatabase;
using TvControl;
using Gentle.Common;
using Gentle.Framework;

#endregion

namespace PersonalTVGuide
{
	public class PersonalTVGuideMain : GUIWindow, ISetupForm
	{
		#region <skin> Variables
		[SkinControlAttribute(10)]		protected GUIListControl lcProgramList = null;
		[SkinControlAttribute(15)]		protected GUIButtonControl btWishList = null;
		[SkinControlAttribute(18)]		protected GUIImage imStars = null;

		[SkinControlAttribute(20)]		protected GUICheckMarkControl cmToday = null;
		[SkinControlAttribute(22)]		protected GUICheckMarkControl cmTomorrow = null;
		[SkinControlAttribute(24)]		protected GUICheckMarkControl cmOneWeek = null;
		[SkinControlAttribute(26)]		protected GUICheckMarkControl cmAll = null;

		enum Controls
		{
			LISTCONTROL_WISHLIST = 10,
			BUTTON_WISHLIST = 15,
			IMAGE_STARS = 18,
			CHECKMARK_TODAY = 20,
			CHECKMARK_TOMORROW = 22,
			CHECKMARK_OneWeek = 24,
			CHECKMARK_ALL = 26,
		};
		#endregion

		#region Variables
		// Private Variables
		// Protected Variables
		protected DateTime _startTime;
		protected DateTime _stopTime;
		// Public Variables
		#endregion

		#region Constructors/Destructors
		public PersonalTVGuideMain()
		{
			GetID = 6000;
      //if (!GlobalServiceProvider.Instance.IsRegistered<IWishList>())
		  GlobalServiceProvider.Instance.Add<IWishList>(new TVServerWishList());
      GlobalServiceProvider.Instance.Get<IWishList>().UpDate();
		}
		#endregion

		#region Properties
		// Public Properties
		#endregion

		#region Public Methods
		#endregion

		#region Private Methods
		#region Settings
		private void LoadSettings()
		{
		}

		private void SaveSettings()
		{
		}
		
		#endregion

		#region OnClicked routines
		private void OnWishListClicked()
		{
			GUIListItem item = lcProgramList.SelectedListItem;
			if (item == null) return;
			GUIDialogSelect2 dlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT2);
			if (dlg == null) return;
			dlg.Reset();
			dlg.SetHeading(GUILocalizeStrings.Get(498));  // 498 = Actions
			if (item.PinImage == String.Empty) dlg.Add(GUILocalizeStrings.Get(264)); // 264 = Record
			else dlg.Add(GUILocalizeStrings.Get(610));   // 610 = Don't Record
			dlg.Add(GUILocalizeStrings.Get(2076));       // 2076 = Edit
			dlg.Add(GUILocalizeStrings.Get(4517));       // 4517 = Close
			dlg.DoModal(GetID);
			switch (dlg.SelectedLabel)
			{
				case 0: OnRecord(); break;
				case 1: OnEditWishItem(); break;
				default: break;
			}
		}

		private void OnRecord()
		{
			GUIListItem item = lcProgramList.SelectedListItem;
			if (item == null) return;
			if (item.TVTag == null) return;
			TVProgramInfo.CurrentProgram = (Program)item.TVTag;
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO);
		}
				
		private void OnEditWishItem()
		{
			GUIListItem item = lcProgramList.SelectedListItem;
			if (item == null) return;
			if (item.MusicTag == null) return;
			PTGEditWishItem.CurrentKeyWordID = ((PersonalTVGuideMap)item.MusicTag).IdKeyword;
			GUIWindowManager.ActivateWindow(6002);  // 6002 = PTGEditWishItem
		}

		private void OnButtonWishListClicked()
		{
			GUIWindowManager.ActivateWindow(6001);
		}

		#endregion

		#region CheckMark routines
		public void ResetCheckMarks()
		{
			cmToday.Selected = false;
			cmTomorrow.Selected = false;
			cmOneWeek.Selected = false;
			cmAll.Selected = false;
		}

		public int GetSelectedCheckMark()
		{
			if (cmToday.Selected) return (int)Controls.CHECKMARK_TODAY;
			if (cmTomorrow.Selected) return (int)Controls.CHECKMARK_TOMORROW;
			if (cmOneWeek.Selected) return (int)Controls.CHECKMARK_OneWeek;
			if (cmAll.Selected) return (int)Controls.CHECKMARK_ALL;

			return (int)Controls.CHECKMARK_TODAY; // default
		}

		private void SelectCheckMark(int Selection)
		{
			ResetCheckMarks();
			switch (Selection)
			{
				case (int)Controls.CHECKMARK_TODAY: OnViewToday(); break;
				case (int)Controls.CHECKMARK_TOMORROW: OnViewTomorrow(); break;
				case (int)Controls.CHECKMARK_OneWeek: OnViewOneWeek(); break;
				case (int)Controls.CHECKMARK_ALL: OnViewAll(); break;
			}
		}

		public void OnViewToday()
		{
			cmToday.Selected = true;
			SetStartStop(0, 1);
		}

		public void OnViewTomorrow()
		{
			cmTomorrow.Selected = true;
			SetStartStop(1, 1);
		}

		public void OnViewOneWeek()
		{
			cmOneWeek.Selected = true;
			SetStartStop(0, 7);
		}

		public void OnViewAll()
		{
			cmAll.Selected = true;
			SetStartStop(0, 30);
		}
		
    #endregion

		public void SetStartStop(int StartOffset, int StopOffset)
		{
			DateTime dt = DateTime.Now.AddDays(StartOffset);
			_startTime = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);

			dt = _startTime.AddDays(StopOffset);
			_stopTime = dt.AddSeconds(-1);
			UpDateProgramlist();
		}

		protected void UpDateProgramlist()
		{
      if (lcProgramList != null)
      {
        lcProgramList.Clear();
        GlobalServiceProvider.Instance.Get<IWishList>().InsertTVProgs(ref lcProgramList, _startTime, _stopTime);
        lcProgramList.Disabled = (lcProgramList.Count <= 0);
      }
		}

		#endregion

		#region <Base class> Overloads
		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\PTG_Main.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			GUIPropertyManager.SetProperty("#ptg_header", "Upcoming Broadcasts");
			GUIPropertyManager.SetProperty("#description", " ");
			if (imStars != null) imStars.SetFileName(String.Empty);
			LoadSettings();
		}

		protected override void OnPageDestroy(int new_windowId)
		{
			base.OnPageDestroy(new_windowId);
			SaveSettings();
		}

		protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
		{
			switch (controlId)
			{
				case (int)Controls.LISTCONTROL_WISHLIST:
					OnWishListClicked();
					break;
				case (int)Controls.BUTTON_WISHLIST:
					OnButtonWishListClicked();
					break;
				case (int)Controls.CHECKMARK_TODAY:
				case (int)Controls.CHECKMARK_TOMORROW:
				case (int)Controls.CHECKMARK_OneWeek:
				case (int)Controls.CHECKMARK_ALL:
					SelectCheckMark(controlId);
					break;
			}

			base.OnClicked(controlId, control, actionType);
		}

		public override void OnAction(Action action)
		{
			base.OnAction(action);
			if (GetFocusControlId() == (int)Controls.LISTCONTROL_WISHLIST)
			{
				if ((lcProgramList.SelectedListItem != null) && (imStars != null))
				{
					GUIListItem item = lcProgramList.SelectedListItem;
					if (item.Rating > 0)
					{
						int i = (int)item.Rating;
						imStars.SetFileName("PTG_Star" + i.ToString() + ".png");
					}
					else
					{
						imStars.SetFileName(String.Empty);
					}
				}
			}
		}

		#endregion

		#region ISetupForm implementation
		public string Author()
		{
			return "Bavarian";
		}
		public bool CanEnable()
		{
			return true;
		}
		public bool DefaultEnabled()
		{
			return false;
		}
		public string Description()
		{
			return "Self learning EPG";
		}
		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = null;
			strButtonImage = null;
			strButtonImageFocus = null;
			strPictureImage = null;
			return false;
		}
		public int GetWindowId()
		{
			return 0;
		}
		public bool HasSetup()
		{
			return false;
		}
		public string PluginName()
		{
			return "Personal TV Guide - client plugin";
		}
		public void ShowPlugin()
		{
			//System.Windows.Forms.Form f = new SetupForm();
			//f.Show();
		}
		#endregion
	}
}
