using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;

namespace PersonalTVGuide
{
  public class PTGEditWishItem : GUIWindow
	{
		#region <skin> Variables
		[SkinControlAttribute(11)]    protected GUIButtonControl butKeyword = null;
    [SkinControlAttribute(21)]    protected GUIToggleButtonControl sbAutoRec = null;
    [SkinControlAttribute(31)]    protected GUIButtonControl butRanking = null;

    [SkinControlAttribute(41)]    protected GUIToggleButtonControl sbSearchTitle = null;
    [SkinControlAttribute(42)]    protected GUIToggleButtonControl sbSearchDescr = null;
    [SkinControlAttribute(43)]    protected GUIToggleButtonControl sbSearchGenre = null;
    [SkinControlAttribute(44)]    protected GUIToggleButtonControl sbSearchEpisode = null;
    [SkinControlAttribute(51)]    protected GUIButtonControl butDayTime = null;
    [SkinControlAttribute(61)]    protected GUIButtonControl butChannel = null;

    enum Controls
    {
      BUTTON_KEYWORD = 11,
      BUTTON_AUTOREC = 21,
      BUTTON_RANKING = 31,
      BUTTON_SEARCHTITLE = 41,
      BUTTON_SEARCHDESCR = 42,
      BUTTON_SEARCHGENRE = 43,
      BUTTON_SEARCHEPISODE = 44,

      BUTTON_DAYTIME = 51,
      BUTTON_CHANNEL = 61,

      BUTTON_OK = 90,
      BUTTON_CANCEL = 91,
    };
		#endregion

		#region Variables
		// Private Variables
		// Protected Variables
		protected static int _keywordID = -1;
		// Public Variables
    #endregion

		#region Constructors/Destructors
		public PTGEditWishItem()
    {
      GetID = 6002;
    }
    #endregion

		#region Properties
		// Public Properties
		public static int CurrentKeyWordID 
		{
			get { return _keywordID; }
			set { _keywordID = value; }
		}
		#endregion

		#region Public Methods
		public void UpdateWindow()
    {
  /*    GUIPropertyManager.SetProperty("#ptg_keyword", KeyWord);
      if (_WishItem.KeyWord == "") GUIPropertyManager.SetProperty("#ptg_keyword", " ");
      GUIPropertyManager.SetProperty("#ptg_ranking", _WishItem.Ranking.ToString());
      GUIPropertyManager.SetProperty("#ptg_daytime", "Edit Day/Time");

      string label = String.Empty;
      if (_WishItem.TVGroupNames.Count <= 0) label = "No TVGroup/All Channels";
      else
      {
        foreach (string group in _WishItem.TVGroupNames)
        {
          label += group + ", ";
        }
      }
      GUIPropertyManager.SetProperty("#ptg_channels", label);
   */
    }

    public void InitToggleButtons()
    {
     /* sbAutoRec.Selected = _WishItem.AutoRecord;
      sbSearchTitle.Selected = _WishItem.SearchTitle;
      sbSearchDescr.Selected = _WishItem.SearchDescription;
      sbSearchGenre.Selected = _WishItem.SearchGenre;
      sbSearchEpisode.Selected = _WishItem.SearchEpisode;
      */
		}
		#endregion

		#region Private Methods
		#region OnClicked routines
		private void OnClickedKeyWord()
		{
		/*
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
			if (keyboard == null) return;

			keyboard.Reset();
			keyboard.Text = _WishItem.KeyWord;
			keyboard.DoModal(GetID);
			if (keyboard.IsConfirmed == false) return;
			_WishItem.KeyWord = keyboard.Text;
			UpdateWindow();
     */
		}

		private void OnClickedRanking()
		{
			/*
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null) return;
			dlg.Reset();
			dlg.ShowQuickNumbers = false;
			dlg.SetHeading("Select Ranking");
			dlg.Add("none");
			dlg.Add("1 Star");
			dlg.Add("2 Stars");
			dlg.Add("3 Stars");
			dlg.Add("4 Stars");
			dlg.Add("5 Stars");
			dlg.SelectedLabel = _WishItem.Ranking;
			dlg.DoModal(GetID);
			_WishItem.Ranking = dlg.SelectedLabel;
			UpdateWindow();
       */
		}

		private void OnClickedDayTime()
		{
			/*
      GUIPTGEditDayTime gui = (GUIPTGEditDayTime)GUIWindowManager.GetWindow(6005);
			if (gui != null) gui.WishItem = _WishItem;
			GUIWindowManager.ActivateWindow(6005);
       */
		}

		private void OnClickedChannel()
		{
      /*
			GUIPTGGroupSelect gui = (GUIPTGGroupSelect)GUIWindowManager.GetWindow(6004);
			if (gui != null) gui.WishItem = _WishItem;
			GUIWindowManager.ActivateWindow(6004);
       */
		}
		#endregion

    #endregion


		#region <Base class> Overloads

		public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\PTG_EditWishItem.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#ptg_header", "Edit");
      ///BAV: Load Data
      if (_keywordID > 0)
      {

      }
      InitToggleButtons();
      UpdateWindow();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
     // Data.UpDate();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      /*
      switch (controlId)
      {
        case (int)Controls.BUTTON_KEYWORD:     OnClickedKeyWord();  break;
        case (int)Controls.BUTTON_AUTOREC:       _WishItem.AutoRecord = sbAutoRec.Selected;            break;
        case (int)Controls.BUTTON_SEARCHTITLE:   _WishItem.SearchTitle = sbSearchTitle.Selected;       break;
        case (int)Controls.BUTTON_SEARCHDESCR:   _WishItem.SearchDescription = sbSearchDescr.Selected; break;
        case (int)Controls.BUTTON_SEARCHGENRE:   _WishItem.SearchGenre = sbSearchGenre.Selected;       break;
        case (int)Controls.BUTTON_SEARCHEPISODE: _WishItem.SearchEpisode = sbSearchEpisode.Selected;   break;
        case (int)Controls.BUTTON_RANKING:     OnClickedRanking(); break;
        case (int)Controls.BUTTON_DAYTIME:     OnClickedDayTime();  break;
        case (int)Controls.BUTTON_CHANNEL:     OnClickedChannel();  break;
        case (int)Controls.BUTTON_OK:
          Data.WishList[_iSelectedItem] = _WishItem;
          Data.WishList.SaveSettings();
          GUIWindowManager.ShowPreviousWindow();
          break;
        case (int)Controls.BUTTON_CANCEL:
          GUIWindowManager.ShowPreviousWindow();
          break;
      }
      */
      base.OnClicked(controlId, control, actionType);
    }

    #endregion
  }
}
