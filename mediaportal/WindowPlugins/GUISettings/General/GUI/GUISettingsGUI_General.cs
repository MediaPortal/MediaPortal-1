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

using System;
using System.Collections;
using System.Globalization;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGUIGeneral : GUIInternalWindow
  {
    [SkinControl(30)] protected GUICheckButton cmAllowRememberLastFocusedItem = null;
    [SkinControl(31)] protected GUICheckButton cmAutosize = null;
    [SkinControl(32)] protected GUICheckButton cmHideextensions = null;
    [SkinControl(33)] protected GUICheckButton cmFileexistscache = null;
    [SkinControl(34)] protected GUICheckButton cmEnableguisounds = null;
    [SkinControl(35)] protected GUICheckButton cmMousesupport = null;

    [SkinControl(40)] protected GUIButtonControl btnHomeUsage = null;

    // IMPORTANT: the enumeration depends on the correct order of items in homeComboBox.
    // The order is chosen to allow compositing SelectedIndex from bitmapped flags.
    [Flags]
    private enum HomeUsageEnum
    {
      PreferClassic = 0,
      PreferBasic = 1,
      UseBoth = 0,
      UseOnlyOne = 2,
    }

    private ArrayList _homeUsage = new ArrayList();
    private int _homeSelectedIndex = 0;
    
    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo)x;
        CultureInfo info2 = (CultureInfo)y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsGUIGeneral()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUIGENERAL; //1022
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI_General.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        // GUI settings
        cmAllowRememberLastFocusedItem.Selected = xmlreader.GetValueAsBool("gui", "allowRememberLastFocusedItem", true);
        cmAutosize.Selected = xmlreader.GetValueAsBool("gui", "autosize", true);
        cmHideextensions.Selected = xmlreader.GetValueAsBool("gui", "hideextensions", true);
        cmFileexistscache.Selected = xmlreader.GetValueAsBool("gui", "fileexistscache", false);
        cmEnableguisounds.Selected = xmlreader.GetValueAsBool("gui", "enableguisounds", true);
        cmMousesupport.Selected = xmlreader.GetValueAsBool("gui", "mousesupport", false);

        bool startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);
        bool useOnlyOneHome = xmlreader.GetValueAsBool("gui", "useonlyonehome", false);
        _homeSelectedIndex = (int)((useOnlyOneHome ? HomeUsageEnum.UseOnlyOne : HomeUsageEnum.UseBoth) |
                                           (startWithBasicHome ? HomeUsageEnum.PreferBasic : HomeUsageEnum.PreferClassic));

        GUIPropertyManager.SetProperty("#homeScreen", _homeUsage[_homeSelectedIndex].ToString());
        btnHomeUsage.Label = _homeUsage[_homeSelectedIndex].ToString();
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("gui", "allowRememberLastFocusedItem", cmAllowRememberLastFocusedItem.Selected);
        xmlwriter.SetValueAsBool("gui", "autosize", cmAutosize.Selected);
        xmlwriter.SetValueAsBool("gui", "hideextensions", cmHideextensions.Selected);
        xmlwriter.SetValueAsBool("gui", "fileexistscache", cmFileexistscache.Selected);
        xmlwriter.SetValueAsBool("gui", "enableguisounds", cmEnableguisounds.Selected);
        xmlwriter.SetValueAsBool("gui", "mousesupport", cmMousesupport.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == cmAllowRememberLastFocusedItem)
      {
        SettingsChanged(true);
      }
      if (control == cmAutosize)
      {
        SettingsChanged(true);
      }
      if (control == cmHideextensions)
      {
        SettingsChanged(true);
      }
      if (control == cmFileexistscache)
      {
        SettingsChanged(true);
      }
      if (control == cmEnableguisounds)
      {
        SettingsChanged(true);
      }
      if (control == cmMousesupport)
      {
        SettingsChanged(true);
      }
      if (control == btnHomeUsage)
      {
        OnHomeUsage();
      }
      
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101022));
      _homeUsage.Clear();
      _homeUsage.AddRange(new object[]
                                        {
                                          "Classic and Basic, prefer Classic",
                                          "Classic and Basic, prefer Basic",
                                          "only Classic Home",
                                          "only Basic Home"
                                        });

      LoadSettings();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();
      base.OnPageDestroy(newWindowId);
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
    
    private void OnHomeUsage()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // options

      foreach (string home in _homeUsage)
      {
        dlg.Add(home);
      }

      dlg.SelectedLabel = _homeSelectedIndex;

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      _homeSelectedIndex = dlg.SelectedLabel;
      btnHomeUsage.Label = dlg.SelectedLabelText;

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("gui", "useonlyonehome",
                                 (dlg.SelectedLabel & (int)HomeUsageEnum.UseOnlyOne) != 0);
        xmlwriter.SetValueAsBool("gui", "startbasichome",
                                 (dlg.SelectedLabel & (int)HomeUsageEnum.PreferBasic) != 0);
      }
      GUIPropertyManager.SetProperty("#homeScreen", _homeUsage[_homeSelectedIndex].ToString());

      SettingsChanged(true);
    }
    
    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}