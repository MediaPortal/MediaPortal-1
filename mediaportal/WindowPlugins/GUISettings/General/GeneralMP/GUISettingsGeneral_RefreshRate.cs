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
  public class RefreshRateData
  {
    public string Name = string.Empty;
    public string FrameRate = string.Empty;
    public string Refreshrate = string.Empty;
    public string Action = string.Empty;


    public RefreshRateData(string name, string framerate, string refreshrate, string action)
    {
      this.Name = name;
      this.FrameRate = framerate;
      this.Refreshrate = refreshrate;
      this.Action = action;
    }
  }

  /// <summary>
  /// Summary description for GUISettingsGeneralRefreshRate.
  /// </summary>
  public class GUISettingsGeneralRefreshRate : GUIInternalWindow
  {
    [SkinControl(2)] protected GUICheckButton btnEnableDynamicRefreshRate = null;
    [SkinControl(3)] protected GUICheckButton btnNotify= null;
    [SkinControl(4)] protected GUICheckButton btnUseDeviceReset = null;
    [SkinControl(5)] protected GUICheckButton btnForceRefreshRateChange= null;
    [SkinControl(6)] protected GUIListControl lcRefreshRatesList = null;
    [SkinControl(7)] protected GUIButtonControl btnAdd = null;
    [SkinControl(8)] protected GUIButtonControl btnRemove = null;
    [SkinControl(9)] protected GUIButtonControl btnDefault = null;
    [SkinControl(12)] protected GUIButtonControl btnEdit = null;
    [SkinControl(10)] protected GUICheckButton btnUseDefaultRefreshRate = null;
    [SkinControl(11)] protected GUIButtonControl btnSelectDefaultRefreshRate = null;

    private string _sDefaultHz;
    private ArrayList _defaultHz = new ArrayList();

    private string _name= string.Empty; 
    private string _framerate = string.Empty; 
    private string _refreshrate= string.Empty;
    private string _action = string.Empty;
    private GUIListItem _selectedRefreshRateListItem = new GUIListItem();
    private GUIListItem _newRefreshRateListItem = new GUIListItem();
    private int _defaultHzIndex = 0;
    private RefreshRateData _newRateDataInfo;
    

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

    public GUISettingsGeneralRefreshRate()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALREFRESHRATE; //1008
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_DynamicRefreshRate.xml"));
    }
    
    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        btnEnableDynamicRefreshRate.Selected = xmlreader.GetValueAsBool("general", "autochangerefreshrate", false);
        btnNotify.Selected = xmlreader.GetValueAsBool("general", "notify_on_refreshrate", false);
        btnUseDefaultRefreshRate.Selected = xmlreader.GetValueAsBool("general", "use_default_hz", false);
        btnUseDeviceReset.Selected = xmlreader.GetValueAsBool("general", "devicereset", false);
        btnForceRefreshRateChange.Selected = xmlreader.GetValueAsBool("general", "force_refresh_rate", false);
        _sDefaultHz = xmlreader.GetValueAsString("general", "default_hz", "");

        String[] p = null;
        _defaultHzIndex = -1;
        _defaultHz.Clear();

        for (int i = 1; i < 100; i++)
        {
          string extCmd = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_ext", "");
          string name = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", "");

          if (string.IsNullOrEmpty(name))
          {
            continue;
          }

          string fps = xmlreader.GetValueAsString("general", name + "_fps", "");
          string hz = xmlreader.GetValueAsString("general", name + "_hz", "");

          p = new String[4];
          p[0] = name;
          p[1] = fps; // fps
          p[2] = hz; //hz
          p[3] = extCmd; //action
          RefreshRateData refreshRateData = new RefreshRateData(name, fps, hz, extCmd);
          GUIListItem item = new GUIListItem();
          item.Label = p[0];
          item.AlbumInfoTag = refreshRateData;
          item.OnItemSelected += OnItemSelected;
          lcRefreshRatesList.Add(item);
          _defaultHz.Add(p[0]);

          if (_sDefaultHz == hz)
          {
            _defaultHzIndex = i - 1;
          }
        }

        if (lcRefreshRatesList.Count == 0)
        {
          InsertDefaultValues();
        }
        lcRefreshRatesList.SelectedListItemIndex = 0;
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "autochangerefreshrate", btnEnableDynamicRefreshRate.Selected);
        xmlwriter.SetValueAsBool("general", "notify_on_refreshrate", btnNotify.Selected);
        xmlwriter.SetValueAsBool("general", "use_default_hz", btnUseDefaultRefreshRate.Selected);
        xmlwriter.SetValueAsBool("general", "devicereset", btnUseDeviceReset.Selected);
        xmlwriter.SetValueAsBool("general", "force_refresh_rate", btnForceRefreshRateChange.Selected);
        
        if (_defaultHzIndex >= 0)
        {
          string rate = RefreshRateData(lcRefreshRatesList.ListItems[_defaultHzIndex]).Refreshrate;
          xmlwriter.SetValue("general", "default_hz", rate);
        }
        else
        {
          xmlwriter.SetValue("general", "default_hz", string.Empty);
        }

        //delete all refreshrate entries, then re-add them.
        Settings xmlreader = new MPSettings();
        for (int i = 1; i < 100; i++)
        {
          string name = xmlreader.GetValueAsString("general", "refreshrate0" + Convert.ToString(i) + "_name", "");

          if (string.IsNullOrEmpty(name))
          {
            continue;
          }

          xmlwriter.RemoveEntry("general", name + "_fps");
          xmlwriter.RemoveEntry("general", name + "_hz");
          xmlwriter.RemoveEntry("general", "refreshrate0" + Convert.ToString(i) + "_ext");
          xmlwriter.RemoveEntry("general", "refreshrate0" + Convert.ToString(i) + "_name");
        }

        int j = 1;
        foreach (GUIListItem item in lcRefreshRatesList.ListItems)
        {
          string name = RefreshRateData(item).Name;
          string fps = RefreshRateData(item).FrameRate;
          string hz = RefreshRateData(item).Refreshrate;
          string extCmd = RefreshRateData(item).Action;

          xmlwriter.SetValue("general", name + "_fps", fps);
          xmlwriter.SetValue("general", name + "_hz", hz);
          xmlwriter.SetValue("general", "refreshrate0" + Convert.ToString(j) + "_ext", extCmd);
          xmlwriter.SetValue("general", "refreshrate0" + Convert.ToString(j) + "_name", name);
          j++;
        }
      }
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      // Enable
      if (control == btnEnableDynamicRefreshRate)
      {
        EnableControls();
        SettingsChanged(true);
      }
      if (_name.ToLower().IndexOf("tv") < 1)
      {
        // Remove
        if (control == btnRemove)
        {
          int sIndex = lcRefreshRatesList.SelectedListItemIndex;
          lcRefreshRatesList.RemoveItem(sIndex);
          _defaultHz.RemoveAt(sIndex);

          if (sIndex > 0)
          {
            sIndex--;
            lcRefreshRatesList.SelectedListItemIndex = sIndex;
          }
          SettingsChanged(true);
        }
        
        // Default
        if (control == btnDefault)
        {
          InsertDefaultValues();
          SettingsChanged(true);
        }
        
        if (control == btnUseDefaultRefreshRate)
        {
          if (btnUseDefaultRefreshRate.Selected)
          {
            btnSelectDefaultRefreshRate.IsEnabled = true;
          }
          else
          {
            btnSelectDefaultRefreshRate.IsEnabled = false;
          }
          SettingsChanged(true);
        }
        
        // Add
        if (control == btnAdd)
        {
          _newRefreshRateListItem = new GUIListItem();
          _newRateDataInfo = new RefreshRateData("", "", "", "");
          OnAddItem();

          if (_newRateDataInfo.Name != string.Empty)
          {
            if (_newRateDataInfo.FrameRate != string.Empty)
            {
              if (_newRateDataInfo.Refreshrate != string.Empty)
              {
                _newRefreshRateListItem.Label = _newRateDataInfo.Name;
                _newRefreshRateListItem.AlbumInfoTag = _newRateDataInfo;
                _newRefreshRateListItem.OnItemSelected += OnItemSelected;
                lcRefreshRatesList.Add(_newRefreshRateListItem);
                _defaultHz.Add(_name);
                UpdateRefreshRateDataFields();
                SetProperties();
              }
            }
          }
          SettingsChanged(true);
        }
        // Edit
        if (control == btnEdit)
        {
          OnEditItem();
          
          if (_name != string.Empty)
          {
            if (_framerate != string.Empty)
            {
              if (_refreshrate != string.Empty)
              {
                lcRefreshRatesList.SelectedListItem.Label = _name;
                _newRateDataInfo = new RefreshRateData(_name, _framerate, _refreshrate, _action);
                lcRefreshRatesList.SelectedListItem.AlbumInfoTag = _newRateDataInfo;
                _defaultHz[_defaultHzIndex] = _name;
                UpdateRefreshRateDataFields();
                SetProperties();
                SettingsChanged(true);
              }
            }
          }
        }
      }
      
      // Use default
      if (control == btnUseDefaultRefreshRate)
      {
        EnableControls();
        SettingsChanged(true);
      }
      
      // Select default refreshrate
      if (control == btnSelectDefaultRefreshRate)
      {
        OnSelectDefaultRefreshRate();
        SettingsChanged(true);
      }

      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101008)); //Dynamic Refresh rate
      LoadSettings();
      //Update();
      SetProperties();
      EnableControls();

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

    private RefreshRateData RefreshRateData(GUIListItem item)
    {
      RefreshRateData refreshRateData = item.AlbumInfoTag as RefreshRateData;
      return refreshRateData;
    }

    private void SetProperties()
    {
      if (_defaultHzIndex >= 0)
      {
        GUIPropertyManager.SetProperty("#defaultrate", _defaultHz[_defaultHzIndex].ToString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#defaultrate", string.Empty);
      }
      if (lcRefreshRatesList.SelectedListItemIndex >= 0)
      {
        GUIPropertyManager.SetProperty("#name", _name);
        GUIPropertyManager.SetProperty("#fps", _framerate);
        GUIPropertyManager.SetProperty("#rate", _refreshrate);
        GUIPropertyManager.SetProperty("#action", _action);
      }
      else
      {
        GUIPropertyManager.SetProperty("#name", string.Empty);
        GUIPropertyManager.SetProperty("#fps", string.Empty);
        GUIPropertyManager.SetProperty("#rate", string.Empty);
        GUIPropertyManager.SetProperty("#action", string.Empty);
      }
    }

    private void EnableControls()
    {
      if (btnEnableDynamicRefreshRate.Selected)
      {
        btnNotify.IsEnabled = true;
        btnUseDeviceReset.IsEnabled = true;
        btnForceRefreshRateChange.IsEnabled = true;
        btnAdd.IsEnabled = true;
        btnRemove.IsEnabled = true;
        btnDefault.IsEnabled = true;
        lcRefreshRatesList.IsEnabled = true;
        btnSelectDefaultRefreshRate.IsEnabled = true;
        btnUseDefaultRefreshRate.IsEnabled = true;
      }
      else
      {
        btnNotify.IsEnabled = false;
        btnUseDeviceReset.IsEnabled = false;
        btnForceRefreshRateChange.IsEnabled = false;
        btnAdd.IsEnabled = false;
        btnRemove.IsEnabled = false;
        btnDefault.IsEnabled = false;
        lcRefreshRatesList.IsEnabled = false;
        btnSelectDefaultRefreshRate.IsEnabled = false;
        btnUseDefaultRefreshRate.IsEnabled = false;
      }

      if (btnUseDefaultRefreshRate.Selected)
      {
        btnSelectDefaultRefreshRate.IsEnabled = true;
      }
      else
      {
        btnSelectDefaultRefreshRate.IsEnabled = false;
      }
    }

    private void InsertDefaultValues()
    {
      lcRefreshRatesList.Clear();
      _defaultHz.Clear();
      Settings xmlreader = new MPSettings();
      //first time mp config is run, no refreshrate settings available, create the default ones.
      string[] p = new String[4];
      p[0] = "CINEMA";
      p[1] = "23.976;24"; // fps
      p[2] = "24"; //hz
      p[3] = ""; //action
      GUIListItem item = new GUIListItem();
      RefreshRateData refreshRateData = new RefreshRateData(p[0], p[1], p[2], p[3]);
      item.Label = p[0];
      item.AlbumInfoTag = refreshRateData;
      item.OnItemSelected += OnItemSelected;
      lcRefreshRatesList.Add(item);
      _defaultHz.Add(p[0]);

      p = new String[4];
      p[0] = "PAL";
      p[1] = "25"; // fps
      p[2] = "50"; //hz
      p[3] = ""; //action
      item = new GUIListItem();
      refreshRateData = new RefreshRateData(p[0], p[1], p[2], p[3]);
      item.Label = p[0];
      item.AlbumInfoTag = refreshRateData;
      item.OnItemSelected += OnItemSelected;
      lcRefreshRatesList.Add(item);
      _defaultHz.Add(p[0]);

      p = new String[4];
      p[0] = "HDTV";
      p[1] = "50"; // fps
      p[2] = "50"; //hz
      p[3] = ""; //action
      item = new GUIListItem();
      refreshRateData = new RefreshRateData(p[0], p[1], p[2], p[3]);
      item.Label = p[0];
      item.AlbumInfoTag = refreshRateData;
      item.OnItemSelected += OnItemSelected;
      lcRefreshRatesList.Add(item);
      _defaultHz.Add(p[0]);

      p = new String[4];
      p[0] = "NTSC";
      p[1] = "29.97;30"; // fps
      p[2] = "60"; //hz
      p[3] = ""; //action
      item = new GUIListItem();
      refreshRateData = new RefreshRateData(p[0], p[1], p[2], p[3]);
      item.Label = p[0];
      item.AlbumInfoTag = refreshRateData;
      item.OnItemSelected += OnItemSelected;
      lcRefreshRatesList.Add(item);
      _defaultHz.Add(p[0]);

      //tv section is not editable, it's static.
      string tvExtCmd = xmlreader.GetValueAsString("general", "refreshrateTV_ext", "");
      string tvName = xmlreader.GetValueAsString("general", "refreshrateTV_name", "PAL");
      string tvFPS = xmlreader.GetValueAsString("general", "tv_fps", "25");
      string tvHz = xmlreader.GetValueAsString("general", "tv_hz", "50");

      String[] parameters = new String[4];
      parameters = new String[4];
      parameters[0] = "TV";
      parameters[1] = tvFPS; // fps
      parameters[2] = tvHz; //hz
      parameters[3] = tvExtCmd; //action
      item = new GUIListItem();
      refreshRateData = new RefreshRateData(parameters[0], parameters[1], parameters[2], parameters[3]);
      item.Label = parameters[0];
      item.AlbumInfoTag = refreshRateData;
      item.OnItemSelected += OnItemSelected;
      lcRefreshRatesList.Add(item);
      _defaultHz.Add(parameters[0]);
    }

    private void UpdateRefreshRateDataFields()
    {
      _name = RefreshRateData(_selectedRefreshRateListItem).Name;
      _framerate = RefreshRateData(_selectedRefreshRateListItem).FrameRate;
      _refreshrate = RefreshRateData(_selectedRefreshRateListItem).Refreshrate;
      _action = RefreshRateData(_selectedRefreshRateListItem).Action;
    }

    private void SetRefreshRateData()
    {
      RefreshRateData(_selectedRefreshRateListItem).Name = _name;
      RefreshRateData(_selectedRefreshRateListItem).FrameRate = _framerate;
      RefreshRateData(_selectedRefreshRateListItem).Refreshrate = _refreshrate;
      RefreshRateData(_selectedRefreshRateListItem).Action = _action;
    }

    private void GetStringFromKeyboard(ref string strLine, int maxLenght)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      if (maxLenght > 0)
      {
        keyboard.SetMaxLength(maxLenght);
      }

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {
        _selectedRefreshRateListItem = item;
        UpdateRefreshRateDataFields();
        SetProperties();
      }
    }

    #region On Add Item

    private void OnAddItem()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Menu

      dlg.AddLocalizedString(300009); // Name
      dlg.AddLocalizedString(300010); // frame rate
      dlg.AddLocalizedString(300011); // refresh rate
      dlg.AddLocalizedString(300012);// Action
      
      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 300009:
          OnAddName();
          break;
        case 300010:
          OnAddFPS();
          break;
        case 300011:
          OnAddRefreshRate();
          break;
        case 300012:
          OnAddAction();
          break;
      }
    }

    private void OnAddName()
    {
      string name = string.Empty;
      GetStringFromKeyboard(ref name, -1);

      foreach (GUIListItem item in lcRefreshRatesList.ListItems)
      {
        if (item.Label.Equals(name, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(name))
        {
          GUIDialogNotify dlgNotify =
            (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (null != dlgNotify)
          {
            dlgNotify.SetHeading(GUILocalizeStrings.Get(257));
            dlgNotify.SetText(GUILocalizeStrings.Get(300013));
            dlgNotify.DoModal(GetID);
            OnAddItem();
            return;
          }
        }
      }
      _newRateDataInfo.Name = name;
      OnAddItem();
    }

    private void OnAddFPS()
    {
      string fps = string.Empty;
      GetStringFromKeyboard(ref fps, -1);
      _newRateDataInfo.FrameRate = fps;
      OnAddItem();
    }

    private void OnAddRefreshRate()
    {
      string rate = string.Empty;
      GetStringFromKeyboard(ref rate, -1);
      _newRateDataInfo.Refreshrate = rate;
      OnAddItem();
    }

    private void OnAddAction()
    {
      string action = string.Empty;
      GetStringFromKeyboard(ref action, -1);
      _newRateDataInfo.Action = action;
      OnAddItem();
    }

    #endregion

    #region On Edit item

    private void OnEditItem()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Menu

      dlg.AddLocalizedString(300009); // Name
      dlg.AddLocalizedString(300010); // frame rate
      dlg.AddLocalizedString(300011); // refresh rate
      dlg.AddLocalizedString(300012);// Action

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 300009:
          OnEditName();
          break;
        case 300010:
          OnEditFPS();
          break;
        case 300011:
          OnEditRefreshRate();
          break;
        case 300012:
          OnEditAction();
          break;
      }
    }

    private void OnEditName()
    {
      string name = _name;
      GetStringFromKeyboard(ref name, -1);

      foreach (GUIListItem item in lcRefreshRatesList.ListItems)
      {
        if (item.Label.Equals(name, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(name))
        {
          GUIDialogNotify dlgNotify =
            (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (null != dlgNotify)
          {
            dlgNotify.SetHeading(GUILocalizeStrings.Get(257));
            dlgNotify.SetText(GUILocalizeStrings.Get(300013));
            dlgNotify.DoModal(GetID);
            OnEditItem();
            return;
          }
        }
      }
      _name = name;
      OnEditItem();
    }

    private void OnEditFPS()
    {
      string fps = _framerate;
      GetStringFromKeyboard(ref fps, -1);

      if (!string.IsNullOrEmpty(fps))
      {
        _framerate = fps;
      }
      OnEditItem();
    }

    private void OnEditRefreshRate()
    {
      string rate = _refreshrate;
      GetStringFromKeyboard(ref rate, -1);
      if (!string.IsNullOrEmpty(rate))
      {
        _refreshrate = rate;
      }
      OnEditItem();
    }

    private void OnEditAction()
    {
      string action = _action;
      GetStringFromKeyboard(ref action, -1);
      _action = action;
      OnEditItem();
    }

    #endregion

    private void OnSelectDefaultRefreshRate()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Menu

      foreach (string rate in _defaultHz)
      {
        dlg.Add(rate);
      }

      if (_defaultHzIndex >= 0)
      {
        dlg.SelectedLabel = _defaultHzIndex;
      }

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      _defaultHzIndex = dlg.SelectedLabel;
      _sDefaultHz = dlg.SelectedLabelText;
      SetProperties();
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}