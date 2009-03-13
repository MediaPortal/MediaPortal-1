#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.GUI.Weather
{
  [PluginIcons("WindowPlugins.GUIWeather.Weather.gif", "WindowPlugins.GUIWeather.WeatherDisabled.gif")]
  public class GUIWindowWeather : GUIWindow, ISetupForm, IShowPlugin
  {
    #region structs

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

    private struct DayForeCast
    {
      public string iconImageNameLow;
      public string iconImageNameHigh;
      public string Overview;
      public string Day;
      public string High;
      public string Low;
      public string SunRise;
      public string SunSet;
      public string Precipitation;
      public string Humidity;
      public string Wind;
    } ;

    #endregion

    #region enums

    private enum Controls
    {
      CONTROL_BTNSWITCH = 2,
      CONTROL_BTNREFRESH = 3,
      CONTROL_BTNVIEW = 4,
      CONTROL_LOCATIONSELECT = 5,
      CONTROL_LABELLOCATION = 10,
      CONTROL_LABELUPDATED = 11,
      CONTROL_IMAGELOGO = 101,
      CONTROL_IMAGENOWICON = 21,
      CONTROL_LABELNOWCOND = 22,
      CONTROL_LABELNOWTEMP = 23,
      CONTROL_LABELNOWFEEL = 24,
      CONTROL_LABELNOWUVID = 25,
      CONTROL_LABELNOWWIND = 26,
      CONTROL_LABELNOWDEWP = 27,
      CONTORL_LABELNOWHUMI = 28,
      CONTROL_STATICTEMP = 223,
      CONTROL_STATICFEEL = 224,
      CONTROL_STATICUVID = 225,
      CONTROL_STATICWIND = 226,
      CONTROL_STATICDEWP = 227,
      CONTROL_STATICHUMI = 228,
      CONTROL_LABELD0DAY = 31,
      CONTROL_LABELD0HI = 32,
      CONTROL_LABELD0LOW = 33,
      CONTROL_LABELD0GEN = 34,
      CONTROL_IMAGED0IMG = 35,
      // Controls 31..35 - 71..75 are reserved for day 0-4 !!!
      CONTROL_LABELSUNR = 200,
      CONTROL_STATICSUNR = 201,
      CONTROL_LABELSUNS = 202,
      CONTROL_STATICSUNS = 203,
      CONTROL_IMAGE_SAT = 1000,
      CONTROL_IMAGE_SAT_END = 1100,
      CONTROL_IMAGE_SUNCLOCK = 1200,
    }

    private enum WindUnit : int
    {
      Kmh = 0,
      mph = 1,
      ms = 2,
      Kn = 3,
      Bft = 4,
    }

    private enum Mode
    {
      Weather,
      DetailImages,
      GeoClock
    }

    private enum ImageView
    {
      Satellite = 0,
      Temperature = 1,
      UVIndex = 2,
      Winds = 3,
      Humidity = 4,
      Precipitation = 5,
    }

    #endregion

    #region variables

    private const int NUM_DAYS = 5;
    private const char DEGREE_CHARACTER = (char) 176; //the degree 'o' character
    private const string PARTNER_ID = "1004124588"; //weather.com partner id
    private const string PARTNER_KEY = "079f24145f208494"; //weather.com partner key

    private string _locationCode = "UKXX0085";
    private ArrayList _listLocations = new ArrayList();
    private string _temperatureFarenheit = "C";
    private WindUnit _currentWindUnit = WindUnit.Bft;
    private int _refreshIntercal = 30;
    private string _nowLabelLocation = string.Empty;
    private string _nowLabelUpdateTime = string.Empty;
    private string _nowIcon = @"weather\128x128\na.png";
    private string _nowLabelCondition = string.Empty;
    private string _nowLabelTemp = string.Empty;
    private string _nowLabelFeelLike = string.Empty;
    private string _nowLabelUVId = string.Empty;
    private string _nowLabelWind = string.Empty;
    private string _nowLabelDewPoint = string.Empty;
    private string _nowLabelHumidity = string.Empty;
    private string _forcastLabelUpdated = string.Empty;

    private DayForeCast[] _forecast = new DayForeCast[NUM_DAYS];
    private GUIImage _nowImage = null;
    private string _urlSatellite = string.Empty;
    private string _urlTemperature = string.Empty;
    private string _urlUvIndex = string.Empty;
    private string _urlWinds = string.Empty;
    private string _urlHumidity = string.Empty;
    private string _urlPreciptation = string.Empty;
    private string _urlViewImage = string.Empty;
    private DateTime _refreshTimer = DateTime.Now.AddHours(-1); //for autorefresh
    private int _dayNum = -2;
    private string _selectedDayName = "All";

    private Mode _currentMode = Mode.Weather;
    private Geochron _geochronGenerator;
    private float _lastTimeSunClockRendered;
    private bool _skipConnectionTest = false;
    private bool _workerCompleted = false;
    private object _downloadLock = null;

    private ImageView _imageView = ImageView.Satellite;

    #endregion

    #region Constructor

    public GUIWindowWeather()
    {
      //loop here as well
      for (int i = 0; i < NUM_DAYS; i++)
      {
        _forecast[i].iconImageNameLow = Config.GetFile(Config.Dir.Weather, @"64x64\na.png");
        _forecast[i].iconImageNameHigh = Config.GetFile(Config.Dir.Weather, @"128x128\na.png");
        _forecast[i].Overview = string.Empty;
        _forecast[i].Day = string.Empty;
        _forecast[i].High = string.Empty;
        _forecast[i].Low = string.Empty;
      }

      GetID = (int) Window.WINDOW_WEATHER;
      _downloadLock = new object();
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      _listLocations.Clear();
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _locationCode = xmlreader.GetValueAsString("weather", "location", string.Empty);
        _temperatureFarenheit = xmlreader.GetValueAsString("weather", "temperature", "C");
        int loadWind = xmlreader.GetValueAsInt("weather", "speed", 0);
        switch (loadWind)
        {
          case 0:
            _currentWindUnit = WindUnit.Kmh;
            break;
          case 1:
            _currentWindUnit = WindUnit.mph;
            break;
          case 2:
            _currentWindUnit = WindUnit.ms;
            break;
          case 3:
            _currentWindUnit = WindUnit.Kn;
            break;
          case 4:
            _currentWindUnit = WindUnit.Bft;
            break;
          default:
            _currentWindUnit = WindUnit.Bft;
            break;
        }

        _refreshIntercal = xmlreader.GetValueAsInt("weather", "refresh", 60);
        _refreshTimer = DateTime.Now.AddMinutes(-(_refreshIntercal + 1));
        _skipConnectionTest = xmlreader.GetValueAsBool("weather", "skipconnectiontest", false);
        Log.Info("GUIWindowWeather: SkipConnectionTest: {0}", _skipConnectionTest);

        bool bFound = false;
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
            if (strSatURL.Length == 0)
            {
              //strSatURL = "http://www.heute.de/CMO/frontend/subsystem_we/WeShowPicture/0,6008,161,00.gif";
              strSatURL = @"http://images.intellicast.com/WeatherImg/SatelliteLoop/hieusat_None_anim.gif";
            }

            LocationInfo loc = new LocationInfo();
            loc.City = city;
            loc.CityCode = strCode;
            loc.UrlSattelite = strSatURL;
            loc.UrlTemperature = strTempURL;
            loc.UrlUvIndex = strUVURL;
            loc.UrlWinds = strWindsURL;
            loc.UrlHumidity = strHumidURL;
            loc.UrlPrecip = strPrecipURL;
            _listLocations.Add(loc);
            if (String.Compare(_locationCode, strCode, true) == 0)
            {
              bFound = true;
            }
          }
        }
        if (!bFound)
        {
          if (_listLocations.Count > 0)
          {
            _locationCode = ((LocationInfo) _listLocations[0]).CityCode;
          }
        }
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("weather", "location", _locationCode);
        xmlwriter.SetValue("weather", "temperature", _temperatureFarenheit);
        xmlwriter.SetValue("weather", "speed", (int) _currentWindUnit);
      }
    }

    #endregion

    #region Overrides

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _refreshTimer;
      if (ts.TotalMinutes >= _refreshIntercal && _locationCode != string.Empty)
      {
        Log.Debug("GUIWindowWeather: autoupdating data");
        _refreshTimer = DateTime.Now;
        _selectedDayName = "All";
        _dayNum = -2;

        //refresh clicked so do a complete update (not an autoUpdate)
        BackgroundUpdate(true);

        _refreshTimer = DateTime.Now;
      }
      base.Process();
    }

    public override void Render(float timePassed)
    {
      if (_currentMode == Mode.GeoClock && _lastTimeSunClockRendered > 10)
      {
        updateSunClock();
        _lastTimeSunClockRendered = 0;
      }
      else
      {
        _lastTimeSunClockRendered += timePassed;
      }
      base.Render(timePassed);
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myweather.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      _currentMode = Mode.Weather;
      _selectedDayName = "All";
      _dayNum = -2;
      LoadSettings();

      //do image id to control stuff so we can use them later
      //do image id to control stuff so we can use them later
      _nowImage = (GUIImage) GetControl((int) Controls.CONTROL_IMAGENOWICON);
      RefreshNewMode();

      int i = 0;
      int selected = 0;
      //					GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT);
      foreach (LocationInfo loc in _listLocations)
      {
        string city = loc.City;
        int pos = city.IndexOf(",");
        //						if (pos>0) city=city.Substring(0,pos);
        //							GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT,city);
        if (_locationCode == loc.CityCode)
        {
          _nowLabelLocation = loc.City;
          _urlSatellite = loc.UrlSattelite;
          _urlTemperature = loc.UrlTemperature;
          _urlUvIndex = loc.UrlUvIndex;
          _urlWinds = loc.UrlWinds;
          _urlHumidity = loc.UrlHumidity;
          _urlPreciptation = loc.UrlPrecip;
          selected = i;
        }
        i++;
      }
      //GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LOCATIONSELECT,selected);

      // Init Daylight clock _geochronGenerator
      _geochronGenerator = new Geochron(GUIGraphicsContext.Skin + @"\Media");
      TimeSpan ts = DateTime.Now - _refreshTimer;
      if (ts.TotalMinutes >= _refreshIntercal && _locationCode != string.Empty)
      {
        BackgroundUpdate(false);
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
      _geochronGenerator = null;
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int) Controls.CONTROL_BTNREFRESH)
            {
              if (CityAvailable())
              {
                OnRefresh();
              }
            }
            if (iControl == (int) Controls.CONTROL_BTNVIEW)
            {
              if (CityAvailable())
              {
                OnChangeView();
              }
            }
            if (iControl == (int) Controls.CONTROL_LOCATIONSELECT)
            {
              if (CityAvailable())
              {
                OnSelectLocation();
              }
            }
            if (iControl == (int) Controls.CONTROL_BTNSWITCH)
            {
              OnSwitchMode();
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    private bool CityAvailable()
    {
      if (_listLocations.Count > 0)
      {
        return true;
      }

      GUIDialogOK dialogOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      if (dialogOK != null)
      {
        dialogOK.Reset();
        dialogOK.SetHeading(8); // myWeather
        dialogOK.SetLine(1, 412); // Unable to get weather data
        dialogOK.SetLine(2, 1028); // City
        dialogOK.SetLine(3, 416); // not available
        dialogOK.DoModal(GetID);
      }
      return false;
    }

    #endregion

    #region Control events

    private void OnSwitchMode()
    {
      switch (_currentMode)
      {
        case Mode.Weather:
          _currentMode = Mode.DetailImages;
          break;
        case Mode.DetailImages:
          _currentMode = Mode.GeoClock;
          _lastTimeSunClockRendered = 0;
          updateSunClock();
          break;
        case Mode.GeoClock:
          _currentMode = Mode.Weather;
          _dayNum = -2;
          _selectedDayName = "All";
          break;
      }

      RefreshNewMode();
    }

    private void OnSelectLocation()
    {
      GUIDialogMenu dialogCitySelect = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dialogCitySelect != null)
      {
        dialogCitySelect.Reset();
        dialogCitySelect.SetHeading(8); //my weather
        foreach (LocationInfo loc in _listLocations)
        {
          dialogCitySelect.Add(loc.City);
        }
        dialogCitySelect.DoModal(GetID);
        if (dialogCitySelect.SelectedLabel >= 0)
        {
          LocationInfo loc = (LocationInfo) _listLocations[dialogCitySelect.SelectedLabel];
          _locationCode = loc.CityCode;
          _nowLabelLocation = loc.City;
          _urlSatellite = loc.UrlSattelite;
          _urlTemperature = loc.UrlTemperature;
          _urlUvIndex = loc.UrlUvIndex;
          _urlWinds = loc.UrlWinds;
          _urlHumidity = loc.UrlHumidity;
          _urlPreciptation = loc.UrlPrecip;

          UpdateDetailImages();

          _dayNum = -2;
          _selectedDayName = "All";

          //refresh clicked so do a complete update (not an autoUpdate)
          BackgroundUpdate(false);
        }
      }
    }

    private bool ViewTypeHasUrl(ImageView aViewTypeToCheck)
    {
      switch (aViewTypeToCheck)
      {
        case ImageView.Satellite:
          return !string.IsNullOrEmpty(_urlSatellite);
        case ImageView.Temperature:
          return !string.IsNullOrEmpty(_urlTemperature);
        case ImageView.UVIndex:
          return !string.IsNullOrEmpty(_urlUvIndex);
        case ImageView.Winds:
          return !string.IsNullOrEmpty(_urlWinds);
        case ImageView.Humidity:
          return !string.IsNullOrEmpty(_urlHumidity);
        case ImageView.Precipitation:
          return !string.IsNullOrEmpty(_urlPreciptation);
        default:
          return false;
      }
    }

    private void SetNextViewType()
    {
      // How many URLs do we have?
      int enumValues = Enum.GetValues(typeof (ImageView)).Length;
      // position on current Enum member
      int enumIndex = (int) _imageView;
      for (int i = 0; i < enumValues; i++)
      {
        // try the next ImageView
        enumIndex++;
        // start from the beginning
        if (enumIndex >= enumValues)
        {
          enumIndex = 0;
        }

        try
        {
          ImageView checkMember = (ImageView) Enum.ToObject(typeof (ImageView), enumIndex);
          if (ViewTypeHasUrl(checkMember))
          {
            _imageView = checkMember;
            return;
          }
        }
        catch (ArgumentException)
        {
        }
      }
    }

    private void OnChangeView()
    {
      if (_currentMode == Mode.DetailImages)
      {
        SetNextViewType();

        RefreshNewMode();
        BackgroundUpdate(false);
      }
      else
      {
        switch (_dayNum)
        {
          case -2:
            _selectedDayName = _forecast[0].Day;
            _dayNum = 0;
            break;
          case -1:
            _selectedDayName = "All";
            break;
          default:
            _selectedDayName = _forecast[_dayNum].Day;
            break;
        }
        RefreshNewMode();
        _dayNum = GetNextDayNum(_dayNum);
      }
    }

    private int GetNextDayNum(int aDayNum)
    {
      if (aDayNum == -2)
      {
        return 1;
      }
      if (aDayNum >= (NUM_DAYS - 1))
      {
        return -1;
      }
      else
      {
        return aDayNum += 1;
      }
    }

    private void OnRefresh()
    {
      if (_currentMode == Mode.GeoClock)
      {
        updateSunClock();
      }
      else
      {
        _dayNum = -2;
        _selectedDayName = "All";
        BackgroundUpdate(false);
      }
    }

    #endregion

    #region Skin control methods

    private void RefreshNewMode()
    {
      switch (_currentMode)
      {
        case Mode.Weather:
          SetWeatherModeControls();
          break;
        case Mode.DetailImages:
          SetDetailImagesModeControls();
          UpdateDetailImages();
          break;
        case Mode.GeoClock:
          SetGeoClockControls();
          break;
      }
    }

    private void UpdateDetailImages()
    {
      GUIImage img = GetControl((int) Controls.CONTROL_IMAGE_SAT) as GUIImage;
      if (img != null)
      {
        switch (_imageView)
        {
          case ImageView.Satellite:
            _urlViewImage = _urlSatellite;
            break;
          case ImageView.Temperature:
            _urlViewImage = _urlTemperature;
            break;
          case ImageView.UVIndex:
            _urlViewImage = _urlUvIndex;
            break;
          case ImageView.Winds:
            _urlViewImage = _urlWinds;
            break;
          case ImageView.Humidity:
            _urlViewImage = _urlHumidity;
            break;
          case ImageView.Precipitation:
            _urlViewImage = _urlPreciptation;
            break;
        }
        img.SetFileName(_urlViewImage);
        //reallocate & load then new image
        img.FreeResources();
        img.AllocResources();
      }
    }

    private void updateSunClock()
    {
      GUIImage clockImage = (GUIImage) GetControl((int) Controls.CONTROL_IMAGE_SUNCLOCK);
      lock (clockImage)
      {
        Bitmap image = _geochronGenerator.update(DateTime.UtcNow);
        Image img = (Image) image.Clone();
        clockImage.IsVisible = false;
        clockImage.FileName = "";
        GUITextureManager.ReleaseTexture("[weatherImage]");
        clockImage.MemoryImage = img;
        clockImage.FileName = "[weatherImage]";
        clockImage.IsVisible = true;
      }
    }

    private void SetGeoClockControls()
    {
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(717));
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_BTNVIEW);
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_LOCATIONSELECT);

      for (int i = (int) Controls.CONTROL_IMAGE_SAT; i < (int) Controls.CONTROL_IMAGE_SAT_END; ++i)
      {
        GUIControl.HideControl(GetID, i);
      }
      for (int i = 10; i < 900; ++i)
      {
        GUIControl.HideControl(GetID, i);
      }

      GUIControl.ShowControl(GetID, (int) Controls.CONTROL_IMAGE_SUNCLOCK);
    }

    private void SetDetailImagesModeControls()
    {
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(19100));

      for (int i = 10; i < 900; ++i)
      {
        GUIControl.HideControl(GetID, i);
      }
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_IMAGE_SUNCLOCK);

      for (int i = (int) Controls.CONTROL_IMAGE_SAT; i < (int) Controls.CONTROL_IMAGE_SAT_END; ++i)
      {
        GUIControl.ShowControl(GetID, i);
      }

      GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BTNVIEW);
      GUIControl.ShowControl(GetID, (int) Controls.CONTROL_LOCATIONSELECT);

      switch (_imageView)
      {
        case ImageView.Satellite:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(737));
          break;
        case ImageView.Temperature:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(738));
          break;
        case ImageView.UVIndex:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(739));
          break;
        case ImageView.Winds:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(740));
          break;
        case ImageView.Humidity:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(741));
          break;
        case ImageView.Precipitation:
          GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(742));
          break;
      }
    }

    private void SetWeatherModeControls()
    {
      for (int i = 10; i < 900; ++i)
      {
        GUIControl.ShowControl(GetID, i);
      }
      GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BTNVIEW);
      GUIControl.ShowControl(GetID, (int) Controls.CONTROL_LOCATIONSELECT);


      for (int i = (int) Controls.CONTROL_IMAGE_SAT; i < (int) Controls.CONTROL_IMAGE_SAT_END; ++i)
      {
        GUIControl.HideControl(GetID, i);
      }
      GUIControl.HideControl(GetID, (int) Controls.CONTROL_IMAGE_SUNCLOCK);


      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(750));
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNREFRESH, GUILocalizeStrings.Get(184)); //Refresh
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELLOCATION, _nowLabelLocation);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELUPDATED, _nowLabelUpdateTime);

      //urgh, remove, create then add image each refresh to update nicely
      //Remove(_nowImage.GetID);
      int posX = _nowImage.XPosition;
      int posY = _nowImage.YPosition;
      //_nowImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGENOWICON, posX, posY, 128, 128, _nowIcon, 0);
      //Add(ref cntl);
      _nowImage.SetPosition(posX, posY);
      _nowImage.ColourDiffuse = 0xffffffff;
      _nowImage.SetFileName(_nowIcon);

      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWCOND, _nowLabelCondition);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWTEMP, _nowLabelTemp);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWFEEL, _nowLabelFeelLike);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWUVID, _nowLabelUVId);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWWIND, _nowLabelWind);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWDEWP, _nowLabelDewPoint);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTORL_LABELNOWHUMI, _nowLabelHumidity);

      //static labels
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(401)); //Temperature
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(402)); //Feels Like
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICUVID, GUILocalizeStrings.Get(403)); //UV Index
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICWIND, GUILocalizeStrings.Get(404)); //Wind
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(405)); //Dew Point
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICHUMI, GUILocalizeStrings.Get(406)); //Humidity

      if (_dayNum == -1 || _dayNum == -2)
      {
        GUIControl.HideControl(GetID, (int) Controls.CONTROL_STATICSUNR);
        GUIControl.HideControl(GetID, (int) Controls.CONTROL_STATICSUNS);
        GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELSUNR);
        GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELSUNS);
        for (int i = 0; i < NUM_DAYS; i++)
        {
          try
          {
            GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELD0DAY + (i*10), _forecast[i].Day);
            GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELD0HI + (i*10), _forecast[i].High);
            GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELD0LOW + (i*10), _forecast[i].Low);
            GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELD0GEN + (i*10), _forecast[i].Overview);

            //Seems a bit messy, but works. Remove, Create and then Add the image to update nicely
            //Remove(_forecast[i].m_pImage.GetID);
            GUIImage image = (GUIImage) GetControl((int) Controls.CONTROL_IMAGED0IMG + (i*10));
            image.ColourDiffuse = 0xffffffff;
            image.SetFileName(_forecast[i].iconImageNameLow);
            //				_forecast[i].m_pImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGED0IMG+(i*10), posX, posY, 64, 64, _forecast[i].iconImageNameLow, 0);
            //			cntl=(GUIControl)_forecast[i].m_pImage;
            //		Add(ref cntl);
          }
          catch (NullReferenceException)
          {
            Log.Warn("GUIWindowWeather: Error assigning controls for day {0}/{1} - your skin might be outdated!", i,
                     NUM_DAYS);
          }
        }
      }
      else
      {
        for (int i = 0; i < NUM_DAYS; i++)
        {
          GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELD0DAY + (i*10));
          GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELD0HI + (i*10));
          GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELD0LOW + (i*10));
          GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELD0GEN + (i*10));
          GUIControl.HideControl(GetID, (int) Controls.CONTROL_IMAGED0IMG + (i*10));
        }
        int currentDayNum = _dayNum;

        GUIControl.HideControl(GetID, (int) Controls.CONTROL_STATICUVID);
        GUIControl.HideControl(GetID, (int) Controls.CONTROL_LABELNOWUVID);

        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICSUNR, GUILocalizeStrings.Get(744));
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICSUNS, GUILocalizeStrings.Get(745));
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(746)); //High
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(747)); //Low
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(748));

        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELSUNR, _forecast[_dayNum].SunRise);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELSUNS, _forecast[_dayNum].SunSet);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTORL_LABELNOWHUMI, _forecast[_dayNum].Humidity);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWTEMP, _forecast[_dayNum].High);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWFEEL, _forecast[_dayNum].Low);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWCOND, _forecast[_dayNum].Overview);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWDEWP, _forecast[_dayNum].Precipitation);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELNOWWIND, _forecast[_dayNum].Wind);
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_LABELUPDATED, _forcastLabelUpdated);

        GUIControl.ShowControl(GetID, (int) Controls.CONTROL_BTNVIEW);
        GUIControl.ShowControl(GetID, (int) Controls.CONTROL_LOCATIONSELECT);

        _nowImage.SetFileName(_forecast[currentDayNum].iconImageNameHigh);
      }

      if (_selectedDayName == "All")
      {
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(743));
      }
      else
      {
        GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_BTNVIEW, _selectedDayName);
      }
    }

    #endregion

    #region Main refresh method

    //Do a complete download, parse and update
    private void RefreshMe(bool autoUpdate)
    {
      lock (_downloadLock)
      {
        using (WaitCursor cursor = new WaitCursor())
        {
          //message strings for refresh of images
          if (!Directory.Exists(Config.GetFolder(Config.Dir.Cache)))
          {
            Directory.CreateDirectory(Config.GetFolder(Config.Dir.Cache));
          }

          string weatherFile = Config.GetFile(Config.Dir.Cache, "curWeather.xml");

          bool dlRes = false, ldRes = false;
          //Do The Download
          dlRes = Download(weatherFile);

          if (dlRes) //dont load if download failed
          {
            ldRes = ParseWeatherInfoFromFile(weatherFile); //parse
          }

          //if the download or load failed, display an error message
          if ((!dlRes || !ldRes) && !autoUpdate) //this will probably crash on an autoupdate as well, but not tested
          {
            // show failed dialog...
            GUIDialogOK errorDialog = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
            errorDialog.SetHeading(412); //"Unable to get weather data"
            errorDialog.SetLine(1, _nowLabelLocation);
            errorDialog.SetLine(2, string.Empty);
            errorDialog.SetLine(3, string.Empty);
            errorDialog.DoModal(GetID);
          }
          else if (dlRes && ldRes) //download and load went ok so update
          {
            RefreshNewMode();
          }

          _refreshTimer = DateTime.Now;
          _dayNum = -2;
        }
      }
    }

    #endregion

    #region Web download

    private bool Download(string weatherFile)
    {
      int code = 0;
      if (!Win32API.IsConnectedToInternet(ref code))
      {
        if (File.Exists(weatherFile))
        {
          return true;
        }

        Log.Info("MyWeather.Download: No internet connection {0}", code);
        if (_skipConnectionTest == false)
        {
          return false;
        }
      }

      char c_units = _temperatureFarenheit[0]; //convert from temp units to metric/standard
      //we'll convert the speed later depending on what thats set to
      c_units = (c_units == 'F') ? 's' : 'm';

      string url =
        String.Format("http://xoap.weather.com/weather/local/{0}?cc=*&dayf=5&link=xoap∏=xoap&par={1}&key={2}&unit={3}",
                      _locationCode, PARTNER_ID, PARTNER_KEY, c_units);

      using (WebClient client = new WebClient())
      {
        try
        {
          client.Proxy.Credentials = CredentialCache.DefaultCredentials;
          client.DownloadFile(url, weatherFile);
          return true;
        }
        catch (Exception ex)
        {
          Log.Info("Failed to download weather: {0} {1}", ex.Message, ex.Source);
        }
      }
      return false;
    }

    #endregion

    #region Background updater

    private void BackgroundUpdate(bool isAuto)
    {
      Thread updateThread = new Thread(new ParameterizedThreadStart(DownloadWorker));
      updateThread.IsBackground = true;
      updateThread.Name = "Weather updater";

      updateThread.Start(isAuto);

      while (_workerCompleted == false)
      {
        GUIWindowManager.Process();
      }
    }

    private void DownloadWorker(object data)
    {
      _workerCompleted = false;

      _refreshTimer = DateTime.Now;
      RefreshMe((bool) data); //do an autoUpdate refresh
      _refreshTimer = DateTime.Now;

      _workerCompleted = true;
    }

    #endregion

    #region XML Parsing

    private void ParseAndBuildWindString(XmlNode node, string unitSpeed, out string wind)
    {
      int tempInteger = 0;
      string tempString = string.Empty;

      if (node == null)
      {
        wind = string.Empty;
        return;
      }

      GetInteger(node, "s", out tempInteger); //current wind strength
      tempInteger = ConvertSpeed(tempInteger); //convert speed if needed
      GetString(node, "t", out tempString, "N"); //current wind direction
      tempString = LocalizeOverview(tempString);

      if (tempInteger != 0) // Have wind
      {
        //From <dir eg NW> at <speed> km/h	
        string format = GUILocalizeStrings.Get(555);
        if (format == "")
        {
          format = "From {0} at {1} {2}";
        }
        wind = String.Format(format, tempString, tempInteger, unitSpeed);
      }
      else // Calm
      {
        wind = GUILocalizeStrings.Get(558);
        if (wind == "")
        {
          wind = "No wind";
        }
      }
    }

    private bool ParseWeatherInfoFromFile(string weatherFile)
    {
      int tempInteger = 0;
      string tempString = string.Empty;
      string unitTemperature = string.Empty;
      string unitSpeed = string.Empty;

      // load the xml file
      XmlDocument doc = new XmlDocument();
      doc.Load(weatherFile);

      if (doc.DocumentElement == null)
      {
        return false;
      }

      string root = doc.DocumentElement.Name;
      XmlNode xmlElement = doc.DocumentElement;
      if (root == "error")
      {
        string szCheckError;

        GUIDialogOK dialogOk = (GUIDialogOK) GUIWindowManager.GetWindow(2002);

        GetString(xmlElement, "err", out szCheckError, "Unknown Error"); //grab the error string

        // show error dialog...
        dialogOk.SetHeading(412); //"Unable to get weather data"
        dialogOk.SetLine(1, szCheckError);
        dialogOk.SetLine(2, _nowLabelLocation);
        dialogOk.SetLine(3, string.Empty);
        dialogOk.DoModal(GetID);
        return true; //we got a message so do display a second in refreshme()
      }

      // units (C or F and mph or km/h or m/s) 
      unitTemperature = _temperatureFarenheit;

      if (_currentWindUnit == WindUnit.Kmh)
      {
        unitSpeed = GUILocalizeStrings.Get(561); // "km/h";
      }
      else if (_currentWindUnit == WindUnit.mph)
      {
        unitSpeed = GUILocalizeStrings.Get(562); // "mph";
      }
      else if (_currentWindUnit == WindUnit.Kn)
      {
        unitSpeed = GUILocalizeStrings.Get(563); // "kn";
      }
      else if (_currentWindUnit == WindUnit.ms)
      {
        unitSpeed = GUILocalizeStrings.Get(564); // "m/s";
      }
      else
      {
        unitSpeed = GUILocalizeStrings.Get(565); // "bft";
      }

      // location
      XmlNode element = xmlElement.SelectSingleNode("loc");
      if (null != element)
      {
        GetString(element, "dnam", out _nowLabelLocation, string.Empty);
      }

      //current weather
      element = xmlElement.SelectSingleNode("cc");
      if (null != element)
      {
        GetString(element, "lsup", out _nowLabelUpdateTime, string.Empty);
        _nowLabelUpdateTime = RelocalizeDateTime(_nowLabelUpdateTime);

        GetInteger(element, "icon", out tempInteger);
        _nowIcon = Config.GetFile(Config.Dir.Weather, String.Format(@"128x128\{0}.png", tempInteger));

        GetString(element, "t", out _nowLabelCondition, string.Empty); //current condition
        _nowLabelCondition = LocalizeOverview(_nowLabelCondition);
        SplitLongString(ref _nowLabelCondition, 8, 15); //split to 2 lines if needed

        GetInteger(element, "tmp", out tempInteger); //current temp
        _nowLabelTemp = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);
        GetInteger(element, "flik", out tempInteger); //current 'Feels Like'
        _nowLabelFeelLike = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);

        XmlNode pNestElement = element.SelectSingleNode("wind"); //current wind
        ParseAndBuildWindString(pNestElement, unitSpeed, out _nowLabelWind);

        GetInteger(element, "hmid", out tempInteger); //current humidity
        _nowLabelHumidity = String.Format("{0}%", tempInteger);

        pNestElement = element.SelectSingleNode("uv"); //current UV index
        if (null != pNestElement)
        {
          GetInteger(pNestElement, "i", out tempInteger);
          GetString(pNestElement, "t", out tempString, string.Empty);
          _nowLabelUVId = String.Format("{0} {1}", tempInteger, LocalizeOverview(tempString));
        }

        GetInteger(element, "dewp", out tempInteger); //current dew point
        _nowLabelDewPoint = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);
      }

      //future forcast
      element = xmlElement.SelectSingleNode("dayf");
      GetString(element, "lsup", out _forcastLabelUpdated, string.Empty);
      _forcastLabelUpdated = RelocalizeDateTime(_forcastLabelUpdated);
      if (null != element)
      {
        XmlNode pOneDayElement = element.SelectSingleNode("day");
        ;
        for (int i = 0; i < NUM_DAYS; i++)
        {
          if (null != pOneDayElement)
          {
            _forecast[i].Day = pOneDayElement.Attributes.GetNamedItem("t").InnerText;
            _forecast[i].Day = LocalizeDay(_forecast[i].Day);

            GetString(pOneDayElement, "hi", out tempString, string.Empty); //string cause i've seen it return N/A
            if (tempString == "N/A")
            {
              _forecast[i].High = string.Empty;
            }
            else
            {
              _forecast[i].High = String.Format("{0}{1}{2}", tempString, DEGREE_CHARACTER, unitTemperature);
            }

            GetString(pOneDayElement, "low", out tempString, string.Empty);
            if (tempString == "N/A")
            {
              _forecast[i].Low = string.Empty;
            }
            else
            {
              _forecast[i].Low = String.Format("{0}{1}{2}", tempString, DEGREE_CHARACTER, unitTemperature);
            }

            GetString(pOneDayElement, "sunr", out tempString, string.Empty);
            if (tempString == "N/A")
            {
              _forecast[i].SunRise = string.Empty;
            }
            else
            {
              tempString = RelocalizeTime(tempString);
              _forecast[i].SunRise = String.Format("{0}", tempString);
            }
            GetString(pOneDayElement, "suns", out tempString, string.Empty);
            if (tempString == "N/A")
            {
              _forecast[i].SunSet = string.Empty;
            }
            else
            {
              tempString = RelocalizeTime(tempString);
              _forecast[i].SunSet = String.Format("{0}", tempString);
            }
            XmlNode pDayTimeElement = pOneDayElement.SelectSingleNode("part");
              //grab the first day/night part (should be day)
            if (null != pDayTimeElement && i == 0)
            {
              GetString(pDayTimeElement, "t", out tempString, string.Empty);
              // If day forecast is not available (at the end of the day), show night forecast
              if (tempString == "N/A")
              {
                pDayTimeElement = pDayTimeElement.NextSibling;
              }
            }

            if (null != pDayTimeElement)
            {
              string finalString;
              GetInteger(pDayTimeElement, "icon", out tempInteger);
              _forecast[i].iconImageNameLow = Config.GetFile(Config.Dir.Weather,
                                                             String.Format("64x64\\{0}.png", tempInteger));
              _forecast[i].iconImageNameHigh = Config.GetFile(Config.Dir.Weather,
                                                              String.Format("128x128\\{0}.png", tempInteger));
              GetString(pDayTimeElement, "t", out _forecast[i].Overview, string.Empty);
              _forecast[i].Overview = LocalizeOverview(_forecast[i].Overview);
              finalString = string.Empty;
              foreach (string tokenSplit in _forecast[i].Overview.Split('/'))
              {
                string workstring;
                workstring = tokenSplit.Trim();
                SplitLongString(ref workstring, 6, 15);
                finalString += workstring + '\n';
              }
              _forecast[i].Overview = finalString;
              GetInteger(pDayTimeElement, "hmid", out tempInteger);
              _forecast[i].Humidity = String.Format("{0}%", tempInteger);
              GetInteger(pDayTimeElement, "ppcp", out tempInteger);
              _forecast[i].Precipitation = String.Format("{0}%", tempInteger);
            }
            XmlNode pWindElement = pDayTimeElement.SelectSingleNode("wind"); //current wind
            ParseAndBuildWindString(pWindElement, unitSpeed, out _forecast[i].Wind);
          }
          pOneDayElement = pOneDayElement.NextSibling; //Element("day");
        }
      }

      return true;
    }

    //splitStart + End are the chars to search between for a space to replace with a \n
    private void SplitLongString(ref string lineString, int splitStart, int splitEnd)
    {
      //search chars 10 to 15 for a space
      //if we find one, replace it with a newline
      for (int i = splitStart; i < splitEnd && i < (int) lineString.Length; i++)
      {
        if (lineString[i] == ' ')
        {
          lineString = lineString.Substring(0, i) + "\n" + lineString.Substring(i + 1);
          return;
        }
      }
    }

    private void GetString(XmlNode xmlElement, string tagName, out string stringValue, string defaultValue)
    {
      stringValue = string.Empty;

      XmlNode node = xmlElement.SelectSingleNode(tagName);
      if (node != null)
      {
        if (node.InnerText != null)
        {
          if (node.InnerText != "-")
          {
            stringValue = node.InnerText;
          }
        }
      }
      if (stringValue.Length == 0)
      {
        stringValue = defaultValue;
      }
    }

    private void GetInteger(XmlNode xmlElement, string tagName, out int intValue)
    {
      intValue = 0;
      XmlNode node = xmlElement.SelectSingleNode(tagName);
      if (node != null)
      {
        if (node.InnerText != null)
        {
          try
          {
            intValue = Int32.Parse(node.InnerText);
          }
          catch (Exception)
          {
          }
        }
      }
    }

    #endregion

    #region Unit conversion

    private int ConvertSpeed(int curSpeed)
    {
      // only calculate in the metric system
      if (_temperatureFarenheit[0] == 'F')
      {
        curSpeed = Convert.ToInt32(curSpeed/1.6093);
      }

      switch (_currentWindUnit)
      {
        case WindUnit.Kmh:
          return curSpeed;

        case WindUnit.mph:
          return (int) (curSpeed*1.6093);

        case WindUnit.ms:
          return (int) (curSpeed*(1000.0/3600.0) + 0.5);

        case WindUnit.Kn:
          return (int) (curSpeed/1.852);

        case WindUnit.Bft:
          return (int) ((curSpeed + 10)/6);
      }

      return curSpeed;
    }

    #endregion

    #region Localisation

    private string LocalizeDay(string dayName)
    {
      string localizedDay = string.Empty;

      if (dayName == "Monday") //monday is localized string 11
      {
        localizedDay = GUILocalizeStrings.Get(11);
      }
      else if (dayName == "Tuesday")
      {
        localizedDay = GUILocalizeStrings.Get(12);
      }
      else if (dayName == "Wednesday")
      {
        localizedDay = GUILocalizeStrings.Get(13);
      }
      else if (dayName == "Thursday")
      {
        localizedDay = GUILocalizeStrings.Get(14);
      }
      else if (dayName == "Friday")
      {
        localizedDay = GUILocalizeStrings.Get(15);
      }
      else if (dayName == "Saturday")
      {
        localizedDay = GUILocalizeStrings.Get(16);
      }
      else if (dayName == "Sunday")
      {
        localizedDay = GUILocalizeStrings.Get(17);
      }
      else
      {
        localizedDay = string.Empty;
      }

      return localizedDay;
    }

    private string RelocalizeTime(string usFormatTime)
    {
      string result = usFormatTime;

      string[] tokens = result.Split(' ');

      if (tokens.Length == 2)
      {
        try
        {
          string[] timePart = tokens[0].Split(':');
          DateTime now = DateTime.Now;
          DateTime time = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            Int32.Parse(timePart[0]) + (String.Compare(tokens[1], "PM", true) == 0 ? 12 : 0),
            Int32.Parse(timePart[1]),
            0
            );

          result = time.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
        catch (Exception)
        {
          // default value is ok
        }
      }
      return result;
    }

    private string RelocalizeDateTime(string usFormatDateTime)
    {
      string result = usFormatDateTime;

      string[] tokens = result.Split(' ');

      // A safety check
      if ((tokens.Length == 5) &&
          (String.Compare(tokens[3], "Local", true) == 0) && (String.Compare(tokens[4], "Time", true) == 0))
      {
        try
        {
          string[] datePart = tokens[0].Split('/');
          string[] timePart = tokens[1].Split(':');
          DateTime time = new DateTime(
            2000 + Int32.Parse(datePart[2]),
            Int32.Parse(datePart[0]),
            Int32.Parse(datePart[1]),
            Int32.Parse(timePart[0]) + (String.Compare(tokens[2], "PM", true) == 0 ? 12 : 0),
            Int32.Parse(timePart[1]),
            0
            );
          result = time.ToString("f", CultureInfo.CurrentCulture.DateTimeFormat);
        }
        catch (Exception)
        {
          // default value is ok
        }
      }
      return result;
    }

    private string LocalizeOverview(string token)
    {
      string localizedLine = string.Empty;

      foreach (string tokenSplit in token.Split(' '))
      {
        string localizedWord = string.Empty;

        if (String.Compare(tokenSplit, "T-Storms", true) == 0 || String.Compare(tokenSplit, "T-Storm", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(370);
        }
        else if (String.Compare(tokenSplit, "Partly", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(371);
        }
        else if (String.Compare(tokenSplit, "Mostly", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(372);
        }
        else if (String.Compare(tokenSplit, "Sunny", true) == 0 || String.Compare(tokenSplit, "Sun", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(373);
        }
        else if (String.Compare(tokenSplit, "Cloudy", true) == 0 || String.Compare(tokenSplit, "Clouds", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(374);
        }
        else if (String.Compare(tokenSplit, "Snow", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(375);
        }
        else if (String.Compare(tokenSplit, "Rain", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(376);
        }
        else if (String.Compare(tokenSplit, "Light", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(377);
        }
        else if (String.Compare(tokenSplit, "AM", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(378);
        }
        else if (String.Compare(tokenSplit, "PM", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(379);
        }
        else if (String.Compare(tokenSplit, "Showers", true) == 0 || String.Compare(tokenSplit, "Shower", true) == 0 ||
                 String.Compare(tokenSplit, "T-Showers", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(380);
        }
        else if (String.Compare(tokenSplit, "Few", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(381);
        }
        else if (String.Compare(tokenSplit, "Scattered", true) == 0 || String.Compare(tokenSplit, "Isolated", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(382);
        }
        else if (String.Compare(tokenSplit, "Wind", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(383);
        }
        else if (String.Compare(tokenSplit, "Strong", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(384);
        }
        else if (String.Compare(tokenSplit, "Fair", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(385);
        }
        else if (String.Compare(tokenSplit, "Clear", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(386);
        }
        else if (String.Compare(tokenSplit, "Early", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(387);
        }
        else if (String.Compare(tokenSplit, "and", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(388);
        }
        else if (String.Compare(tokenSplit, "Fog", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(389);
        }
        else if (String.Compare(tokenSplit, "Haze", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(390);
        }
        else if (String.Compare(tokenSplit, "Windy", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(391);
        }
        else if (String.Compare(tokenSplit, "Drizzle", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(392);
        }
        else if (String.Compare(tokenSplit, "Freezing", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(393);
        }
        else if (String.Compare(tokenSplit, "N/A", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(394);
        }
        else if (String.Compare(tokenSplit, "Mist", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(395);
        }
        else if (String.Compare(tokenSplit, "High", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(799);
        }
        else if (String.Compare(tokenSplit, "Low", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(798);
        }
        else if (String.Compare(tokenSplit, "Moderate", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(534);
        }
        else if (String.Compare(tokenSplit, "Late", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(553);
        }
        else if (String.Compare(tokenSplit, "Very", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(554);
        }
        else if (String.Compare(tokenSplit, "Heavy", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(407);
        }
          // wind directions
        else if (String.Compare(tokenSplit, "N", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(535);
        }
        else if (String.Compare(tokenSplit, "E", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(536);
        }
        else if (String.Compare(tokenSplit, "S", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(537);
        }
        else if (String.Compare(tokenSplit, "W", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(538);
        }
        else if (String.Compare(tokenSplit, "NE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(539);
        }
        else if (String.Compare(tokenSplit, "SE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(540);
        }
        else if (String.Compare(tokenSplit, "SW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(541);
        }
        else if (String.Compare(tokenSplit, "NW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(542);
        }
        else if (String.Compare(tokenSplit, "Thunder", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(543);
        }
        else if (String.Compare(tokenSplit, "NNE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(544);
        }
        else if (String.Compare(tokenSplit, "ENE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(545);
        }
        else if (String.Compare(tokenSplit, "ESE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(546);
        }
        else if (String.Compare(tokenSplit, "SSE", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(547);
        }
        else if (String.Compare(tokenSplit, "SSW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(548);
        }
        else if (String.Compare(tokenSplit, "WSW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(549);
        }
        else if (String.Compare(tokenSplit, "WNW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(551);
        }
        else if (String.Compare(tokenSplit, "NNW", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(552);
        }
        else if (String.Compare(tokenSplit, "VAR", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(556);
        }
        else if (String.Compare(tokenSplit, "CALM", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(557);
        }
        else if (String.Compare(tokenSplit, "Storm", true) == 0 || String.Compare(tokenSplit, "Gale", true) == 0 ||
                 String.Compare(tokenSplit, "Tempest", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(599);
        }
        else if (String.Compare(tokenSplit, "in the Vicinity", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(559);
        }
        else if (String.Compare(tokenSplit, "Clearing", true) == 0)
        {
          localizedWord = GUILocalizeStrings.Get(560);
        }

        if (localizedWord == string.Empty)
        {
          localizedWord = tokenSplit; //if not found, let fallback
        }

        localizedLine = localizedLine + localizedWord;
        localizedLine += " ";
      }

      return localizedLine;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "Weather";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(8);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my weather.png";
      return true;
    }

    public string Author()
    {
      return "rtv";
    }

    public string Description()
    {
      return "Shows the current weather (incl. forecast) in your region or anywhere in the world";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIWindowWeather.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}