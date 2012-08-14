#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{

  /// <summary>
  /// Summary description for WeatherSettings.
  /// </summary>
  public class GUISettingsWeather : GUIInternalWindow
  {

    // Controls for GUISettingsWeather
    [SkinControl(21)] protected GUIButtonControl btnAddLocation = null;
    [SkinControl(22)] protected GUIButtonControl btnRemoveLocation = null;
    [SkinControl(23)] protected GUIMenuButton btnDefaultLocation = null;
    [SkinControl(24)] protected GUIMenuButton btnTemperatureSelect = null;
    [SkinControl(25)] protected GUIMenuButton btnWindSpeedSelect = null;
    [SkinControl(26)] protected GUIMenuButton btnRefreshInterval = null;

    private class LocationInfo
    {
      public string City;
      public string CityCode;
      public string UrlSattelite;
      public string UrlTemperature;
      public string UrlUvIndex;
      public string UrlWinds;
      public string UrlHumidity;
      public string UrlPrecip;
    }

    String defaultLocationCode = "";
    ArrayList availableLocations = new ArrayList();

    public GUISettingsWeather()
    {
      GetID = (int)Window.WINDOW_SETTINGS_WEATHER;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_weather.xml"));
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      WeatherSettings_OnPageLoad();

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
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      bool msgHandled = false;

      // Depending on the mode, handle the GUI_MSG_ITEM_SELECT message from the dialog menu and
      // the GUI_MSG_CLICKED message from the spin control.
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT ||
          message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
      {
        // Respond to the correct control.  The value is retrived directly from the control by the called handler.
        if (message.SenderControlId == btnAddLocation.GetID)
        {
          AddLocation();
          msgHandled = true;
        }
        else if (message.SenderControlId == btnRemoveLocation.GetID)
        {
          RemoveLocation();
          msgHandled = true;
        }
        else if (message.TargetControlId == btnDefaultLocation.GetID)
        {
          SetDefaultLocation();
          msgHandled = true;
        }
        else if (message.TargetControlId == btnTemperatureSelect.GetID)
        {
          SetTemperatureSelect();
          msgHandled = true;
        }
        else if (message.TargetControlId == btnWindSpeedSelect.GetID)
        {
          SetWindSpeedSelect();
          msgHandled = true;
        }
        else if (message.TargetControlId == btnRefreshInterval.GetID)
        {
          SetRefreshInterval();
          msgHandled = true;
        }
      }

      msgHandled = msgHandled | base.OnMessage(message);
      return msgHandled;
    }

    #region WeatherSettings

    private void AddLocation()
    {
      string city = "";
      if (!GetKeyboard(ref city) || String.IsNullOrEmpty(city))
      {
        return;
      }
      try
      {
        // Perform actual search        
        WeatherChannel weather = new WeatherChannel();
        ArrayList cities = weather.SearchCity(city);

        if (cities.Count <= 0)
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          dlg.SetHeading(8);
          dlg.SetLine(1, 412);
          dlg.SetLine(2, "");
          dlg.DoModal(GetID);
          return;
        }

        GUIDialogMenu dialogCitySelect = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dialogCitySelect != null)
        {
          dialogCitySelect.Reset();
          dialogCitySelect.ShowQuickNumbers = false;
          dialogCitySelect.SetHeading(396); // Select Location
          foreach (WeatherChannel.City _city in cities)
          {
            dialogCitySelect.Add(_city.Name + " (" + _city.Id + ")");
          }

          dialogCitySelect.DoModal(GetID);
          if (dialogCitySelect.SelectedLabel >= 0)
          {
            WeatherChannel.City newcity = (WeatherChannel.City)cities[dialogCitySelect.SelectedLabel];
            LocationInfo loc = new LocationInfo();
            loc.City = newcity.Name;
            loc.CityCode = newcity.Id;
            loc.UrlSattelite = "";
            loc.UrlTemperature = "";
            loc.UrlUvIndex = "";
            loc.UrlWinds = "";
            loc.UrlHumidity = "";
            loc.UrlPrecip = "";
            availableLocations.Add(loc);

            SaveLocations();
            SetDefaultLocation(); // Reset the default location as necessary
            InitDefaultLocation(); // Refresh default location button as necessary
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("MyWeather settings error: {0}", ex.ToString());
      }
    }

    private bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return false;
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.SetLabelAsInitialText(true);
      keyboard.Label = GUILocalizeStrings.Get(408);
      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        return true;
      }
      return false;
    }

    private void SaveLocations()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        // Clear the current set of locations
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string cityTag = String.Format("city{0}", i);
          string strCodeTag = String.Format("code{0}", i);
          string strSatUrlTag = String.Format("sat{0}", i);
          string strTempUrlTag = String.Format("temp{0}", i);
          string strUVUrlTag = String.Format("uv{0}", i);
          string strWindsUrlTag = String.Format("winds{0}", i);
          string strHumidUrlTag = String.Format("humid{0}", i);
          string strPrecipUrlTag = String.Format("precip{0}", i);
          xmlwriter.SetValue("weather", cityTag, "");
          xmlwriter.SetValue("weather", strCodeTag, "");
          xmlwriter.SetValue("weather", strSatUrlTag, "");
          xmlwriter.SetValue("weather", strTempUrlTag, "");
          xmlwriter.SetValue("weather", strUVUrlTag, "");
          xmlwriter.SetValue("weather", strWindsUrlTag, "");
          xmlwriter.SetValue("weather", strHumidUrlTag, "");
          xmlwriter.SetValue("weather", strPrecipUrlTag, "");
        }

        // Write our collection of locations
        for (int i = 0; i < availableLocations.Count; i++)
        {
          string cityTag = String.Format("city{0}", i);
          string strCodeTag = String.Format("code{0}", i);
          string strSatUrlTag = String.Format("sat{0}", i);
          string strTempUrlTag = String.Format("temp{0}", i);
          string strUVUrlTag = String.Format("uv{0}", i);
          string strWindsUrlTag = String.Format("winds{0}", i);
          string strHumidUrlTag = String.Format("humid{0}", i);
          string strPrecipUrlTag = String.Format("precip{0}", i);
          xmlwriter.SetValue("weather", cityTag, ((LocationInfo)availableLocations[i]).City);
          xmlwriter.SetValue("weather", strCodeTag, ((LocationInfo)availableLocations[i]).CityCode);
          xmlwriter.SetValue("weather", strSatUrlTag, ((LocationInfo)availableLocations[i]).UrlSattelite);
          xmlwriter.SetValue("weather", strTempUrlTag, ((LocationInfo)availableLocations[i]).UrlTemperature);
          xmlwriter.SetValue("weather", strUVUrlTag, ((LocationInfo)availableLocations[i]).UrlUvIndex);
          xmlwriter.SetValue("weather", strWindsUrlTag, ((LocationInfo)availableLocations[i]).UrlWinds);
          xmlwriter.SetValue("weather", strHumidUrlTag, ((LocationInfo)availableLocations[i]).UrlHumidity);
          xmlwriter.SetValue("weather", strPrecipUrlTag, ((LocationInfo)availableLocations[i]).UrlPrecip);
        }
      }
    }

    private void RemoveLocation()
    {
      if (availableLocations.Count > 0)
      {

        GUIDialogMenu dialogCitySelect = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (dialogCitySelect != null)
        {
          dialogCitySelect.Reset();
          dialogCitySelect.ShowQuickNumbers = false;
          dialogCitySelect.SetHeading(GUILocalizeStrings.Get(409)); // Remove Location
          foreach (LocationInfo loc in availableLocations)
          {
            dialogCitySelect.Add(loc.City + " (" + loc.CityCode + ")");
          }
          dialogCitySelect.DoModal(GetID);

          // Remove the selected city
          if (dialogCitySelect.SelectedLabel >= 0)
          {
            LocationInfo loc = (LocationInfo)availableLocations[dialogCitySelect.SelectedLabel];
            availableLocations.Remove(loc);
            SaveLocations();
            SetDefaultLocation(); // Reset the default location as necessary
            InitDefaultLocation(); // Refresh default location button as necessary
          }
        }
      }
      else
      {
        GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        dlg.SetHeading(GUILocalizeStrings.Get(409));
        dlg.SetLine(1, GUILocalizeStrings.Get(520));
        dlg.SetLine(2, "");
        dlg.DoModal(GetID);
        return;
      }
    }

    private void SetDefaultLocation()
    {
      string defaultLocationCode = "";
      for (int i = 0; i < availableLocations.Count; i++)
      {
        if (btnDefaultLocation.SelectedItemLabel == ((LocationInfo)availableLocations[i]).City)
        {
          defaultLocationCode = ((LocationInfo)availableLocations[i]).CityCode;
          break;
        }
      }

      // If no default location could be set then choose the first city in the list.
      if (defaultLocationCode == "" & availableLocations.Count > 0)
      {
        defaultLocationCode = ((LocationInfo)availableLocations[0]).CityCode;
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("weather", "location", defaultLocationCode);
      }
    }

    private void SetTemperatureSelect()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        string ts = "F"; // Farenheit
        if (btnTemperatureSelect.SelectedItemLabel.CompareTo("Celsius") == 0)
        {
          ts = "C"; // Celsius
        }

        xmlwriter.SetValue("weather", "temperature", ts);
      }
    }

    private void SetWindSpeedSelect()
    {
      int newWindUnit = 0;
      if (btnWindSpeedSelect.SelectedItemLabel == GUILocalizeStrings.Get(561))
      {
        newWindUnit = 0; // km/h
      }
      else if (btnWindSpeedSelect.SelectedItemLabel == GUILocalizeStrings.Get(562))
      {
        newWindUnit = 1; // mph
      }
      else if (btnWindSpeedSelect.SelectedItemLabel == GUILocalizeStrings.Get(564))
      {
        newWindUnit = 2; // m/s
      }
      else if (btnWindSpeedSelect.SelectedItemLabel == GUILocalizeStrings.Get(563))
      {
        newWindUnit = 3; // kn
      }
      else if (btnWindSpeedSelect.SelectedItemLabel == GUILocalizeStrings.Get(565))
      {
        newWindUnit = 4; // bft
      }

      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("weather", "speed", newWindUnit);
      }
    }

    private void SetRefreshInterval()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("weather", "refresh", btnRefreshInterval.SelectedItemValue);
      }
    }

    private void WeatherSettings_OnPageLoad()
    {
      LoadLocations();
      InitAddLocation();
      InitRemoveLocation();
      InitDefaultLocation();
      InitTemperatureSelect();
      InitWindSpeedSelect();
      InitRefreshInterval();
    }

    private void LoadLocations()
    {
      availableLocations.Clear();

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string cityTag = String.Format("city{0}", i);
          string strCodeTag = String.Format("code{0}", i);
          string strSatUrlTag = String.Format("sat{0}", i);
          string strTempUrlTag = String.Format("temp{0}", i);
          string strUVUrlTag = String.Format("uv{0}", i);
          string strWindsUrlTag = String.Format("winds{0}", i);
          string strHumidUrlTag = String.Format("humid{0}", i);
          string strPrecipUrlTag = String.Format("precip{0}", i);
          string city = xmlreader.GetValueAsString("weather", cityTag, string.Empty);
          string strCode = xmlreader.GetValueAsString("weather", strCodeTag, string.Empty);
          string strSatURL = xmlreader.GetValueAsString("weather", strSatUrlTag, string.Empty);
          string strTempURL = xmlreader.GetValueAsString("weather", strTempUrlTag, string.Empty);
          string strUVURL = xmlreader.GetValueAsString("weather", strUVUrlTag, string.Empty);
          string strWindsURL = xmlreader.GetValueAsString("weather", strWindsUrlTag, string.Empty);
          string strHumidURL = xmlreader.GetValueAsString("weather", strHumidUrlTag, string.Empty);
          string strPrecipURL = xmlreader.GetValueAsString("weather", strPrecipUrlTag, string.Empty);
          if (city.Length > 0 && strCode.Length > 0)
          {
            LocationInfo loc = new LocationInfo();
            loc.City = city;
            loc.CityCode = strCode;
            loc.UrlSattelite = strSatURL;
            loc.UrlTemperature = strTempURL;
            loc.UrlUvIndex = strUVURL;
            loc.UrlWinds = strWindsURL;
            loc.UrlHumidity = strHumidURL;
            loc.UrlPrecip = strPrecipURL;
            availableLocations.Add(loc);
          }
        }
      }
    }

    private void InitAddLocation()
    {
    }

    private void InitRemoveLocation()
    {
    }

    private void InitDefaultLocation()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        defaultLocationCode = xmlreader.GetValueAsString("weather", "location", string.Empty);
      }

      btnDefaultLocation.ClearMenu();
      for (int i = 0; i < availableLocations.Count; i++)
      {
        btnDefaultLocation.AddItem(((LocationInfo)availableLocations[i]).City, i);
        if (defaultLocationCode == ((LocationInfo)availableLocations[i]).CityCode)
        {
          btnDefaultLocation.SelectedItem = i;
        }
      }
    }

    private void InitTemperatureSelect()
    {
      string tempUnit;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        tempUnit = xmlreader.GetValueAsString("weather", "temperature", "C");
      }

      ArrayList availableUnits = new ArrayList();
      availableUnits.Add("Celsius");   // Celsius
      availableUnits.Add("Farenheit"); // Farenheit

      int index = 0;
      foreach (string au in availableUnits)
      {
        btnTemperatureSelect.AddItem(au, index);
        if (au[0] == tempUnit[0])
        {
          btnTemperatureSelect.SelectedItem = index;
        }
        index++;
      }
    }

    private void InitWindSpeedSelect()
    {
      int unit;
      string currentWindUnit;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        unit = xmlreader.GetValueAsInt("weather", "speed", 0);
      }

      switch (unit)
      {
        case 0:
          currentWindUnit = GUILocalizeStrings.Get(561); // km/h
          break;
        case 1:
          currentWindUnit = GUILocalizeStrings.Get(562); // mph
          break;
        case 2:
          currentWindUnit = GUILocalizeStrings.Get(564); // m/s
          break;
        case 3:
          currentWindUnit = GUILocalizeStrings.Get(563); // kn
          break;
        case 4:
          currentWindUnit = GUILocalizeStrings.Get(565); // bft
          break;
        default:
          currentWindUnit = GUILocalizeStrings.Get(565); // bft
          break;
      }

      ArrayList availableUnits = new ArrayList();
      availableUnits.Add(GUILocalizeStrings.Get(561)); // km/h
      availableUnits.Add(GUILocalizeStrings.Get(562)); // mph
      availableUnits.Add(GUILocalizeStrings.Get(563)); // kn
      availableUnits.Add(GUILocalizeStrings.Get(564)); // m/s
      availableUnits.Add(GUILocalizeStrings.Get(565)); // bft

      int index = 0;
      foreach (string au in availableUnits)
      {
        btnWindSpeedSelect.AddItem(au, index);
        if (au == currentWindUnit)
        {
          btnWindSpeedSelect.SelectedItem = index;
        }
        index++;
      }
    }

    private void InitRefreshInterval()
    {
      int refresh;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        refresh = xmlreader.GetValueAsInt("weather", "refresh", 60);
      }
      btnRefreshInterval.AddItemRange(1, 60); // minutes
      btnRefreshInterval.SelectedItem = refresh;
    }

    #endregion
  }
}
