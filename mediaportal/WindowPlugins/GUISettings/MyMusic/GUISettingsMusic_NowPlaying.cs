#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUISettingsMusicNowPlaying : GUIInternalWindow
  {
    [SkinControl(2)] protected GUICheckButton btnFetchlastfmcovers = null;
    [SkinControl(3)] protected GUICheckButton btnFetchlastfmtopalbums = null;
    [SkinControl(4)] protected GUICheckButton btnFetchlastfmtracktags = null;
    [SkinControl(5)] protected GUICheckButton btnSwitchArtistOnLastFMSubmit = null;
    [SkinControl(6)] protected GUIButtonControl btnVUNone= null;
    

    private ArrayList _vuMeterValues = new ArrayList();
    private string _vuMeter = string.Empty;

    public GUISettingsMusicNowPlaying()
    {
      GetID = (int)Window.WINDOW_SETTINGS_MUSICNOWPLAYING; //1013
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\Settings_MyMusic_NowPlaying.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        btnFetchlastfmcovers.Selected = !xmlreader.GetValueAsBool("musicmisc", "fetchlastfmcovers", true);
        btnFetchlastfmtopalbums.Selected = !xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtopalbums", true);
        btnFetchlastfmtracktags.Selected = !xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtracktags", true);
        btnSwitchArtistOnLastFMSubmit.Selected= xmlreader.GetValueAsBool("musicmisc", "switchArtistOnLastFMSubmit",
                                                                              false);

        _vuMeter= xmlreader.GetValueAsString("musicmisc", "vumeter", "none");
        _vuMeter = UppercaseFirst(_vuMeter);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmcovers", !btnFetchlastfmcovers.Selected);
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmtopalbums", !btnFetchlastfmtopalbums.Selected);
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmtracktags", !btnFetchlastfmtracktags.Selected);
        xmlwriter.SetValueAsBool("musicmisc", "switchArtistOnLastFMSubmit", btnSwitchArtistOnLastFMSubmit.Selected);

        xmlwriter.SetValue("musicmisc", "vumeter", _vuMeter.ToLowerInvariant());
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101013));
      _vuMeterValues.Clear();
      _vuMeterValues.AddRange(new object[]
                                      {
                                        "None",
                                        "Analog",
                                        "Led"
                                      });

      LoadSettings();
      SetProperty();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnVUNone)
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dlg != null)
        {
          dlg.Reset();
          dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

          foreach (string vuValue in _vuMeterValues)
          {
            dlg.Add(vuValue);

            if (vuValue == _vuMeter)
            {
              dlg.SelectedLabel = _vuMeterValues.IndexOf(vuValue);
            }
          }
          
          dlg.DoModal(GetID);

          if (dlg.SelectedId == -1)
          {
            return;
          }

          _vuMeter = dlg.SelectedLabelText;
          SetProperty();
          SettingsChanged(true);
        }
      }
      if (control == btnFetchlastfmcovers || control == btnFetchlastfmtopalbums || 
          control == btnFetchlastfmtracktags || control == btnSwitchArtistOnLastFMSubmit)
      {
        SettingsChanged(true);
      }
      
      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private string UppercaseFirst(string s)
    {
	    if (string.IsNullOrEmpty(s))
	    {
	       return string.Empty;
	    }
	    
      return char.ToUpper(s[0]) + s.Substring(1);
    }

    private void SetProperty()
    {
      GUIPropertyManager.SetProperty("#vumeter", UppercaseFirst(_vuMeter));
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}