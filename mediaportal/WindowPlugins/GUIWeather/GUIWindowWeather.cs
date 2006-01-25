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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using MediaPortal.GUI.Library;
using System.Xml;
using MediaPortal.Util;
using MediaPortal.Dialogs;

using System.ComponentModel;
using System.Threading;

namespace MediaPortal.GUI.Weather
{
  public class GUIWindowWeather : GUIWindow, ISetupForm
  {
    #region structs
    class LocationInfo
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

    struct DayForeCast
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
    };
    #endregion

    #region enums
    enum Controls
    {
      CONTROL_BTNSWITCH = 2
    ,
      CONTROL_BTNREFRESH = 3
    ,
      CONTROL_BTNVIEW = 4
    ,
      CONTROL_LOCATIONSELECT = 5
    ,
      CONTROL_LABELLOCATION = 10
    ,
      CONTROL_LABELUPDATED = 11
    ,
      CONTROL_IMAGELOGO = 101
    ,
      CONTROL_IMAGENOWICON = 21
      ,
      CONTROL_LABELNOWCOND = 22
  ,
      CONTROL_LABELNOWTEMP = 23
    ,
      CONTROL_LABELNOWFEEL = 24
    ,
      CONTROL_LABELNOWUVID = 25
    ,
      CONTROL_LABELNOWWIND = 26
    ,
      CONTROL_LABELNOWDEWP = 27
    ,
      CONTORL_LABELNOWHUMI = 28
    ,
      CONTROL_STATICTEMP = 223
    ,
      CONTROL_STATICFEEL = 224
    ,
      CONTROL_STATICUVID = 225
    ,
      CONTROL_STATICWIND = 226
    ,
      CONTROL_STATICDEWP = 227
    ,
      CONTROL_STATICHUMI = 228
    ,
      CONTROL_LABELD0DAY = 31
    ,
      CONTROL_LABELD0HI = 32
    ,
      CONTROL_LABELD0LOW = 33
    ,
      CONTROL_LABELD0GEN = 34
    ,
      CONTROL_IMAGED0IMG = 35
    ,
      CONTROL_LABELSUNR = 70
    ,
      CONTROL_STATICSUNR = 71
    ,
      CONTROL_LABELSUNS = 72
    ,
      CONTROL_STATICSUNS = 73
    ,
      CONTROL_IMAGE_SAT = 1000
    ,
      CONTROL_IMAGE_SAT_END = 1100
    , CONTROL_IMAGE_SUNCLOCK = 1200
    }

    enum Mode
    {
      Weather,
      Satellite,
      GeoClock
    }

    enum ImageView
    {
      Satellite,
      Temperature,
      UVIndex,
      Winds,
      Humidity,
      Precipitation
    }
    #endregion

    #region variables
    const int NUM_DAYS = 4;
    const char DEGREE_CHARACTER = (char)176;				//the degree 'o' character
    const string PARTNER_ID = "1004124588";			//weather.com partner id
    const string PARTNER_KEY = "079f24145f208494";		//weather.com partner key

    string _locationCode = "UKXX0085";
    ArrayList _listLocations = new ArrayList();
    string _temperatureFarenheit = "C";
    string _windSpeed = "K";
    int _refreshIntercal = 30;
    string _nowLocation = String.Empty;
    string _nowUpdated = String.Empty;
    string _nowIcon = @"weather\128x128\na.png";
    string _nowCond = String.Empty;
    string _nowTemp = String.Empty;
    string _nowFeel = String.Empty;
    string _nowUVId = String.Empty;
    string _nowWind = String.Empty;
    string _nowDewp = String.Empty;
    string _nowHumd = String.Empty;
    string _forcastUpdated = String.Empty;

    DayForeCast[] _forecast = new DayForeCast[NUM_DAYS];
    GUIImage _nowImage = null;
    string _urlSattelite = String.Empty;
    string _urlTemperature = String.Empty;
    string _urlUvIndex = String.Empty;
    string _urlWinds = String.Empty;
    string _urlHumidity = String.Empty;
    string _urlPreciptation = String.Empty;
    string _urlViewImage = String.Empty;
    DateTime _refreshTimer = DateTime.Now.AddHours(-1);		//for autorefresh
    int _dayNum = -2;
    string _selectedDayName = "All";

    Mode _currentMode = Mode.Weather;
    Geochron _geochronGenerator;
    float _lastTimeSunClockRendered;
    #endregion

    ImageView _imageView = ImageView.Satellite;

    public GUIWindowWeather()
    {
      //loop here as well
      for (int i = 0; i < NUM_DAYS; i++)
      {
        _forecast[i].iconImageNameLow = @"weather\64x64\na.png";
        _forecast[i].iconImageNameHigh = @"weather\128x128\na.png";
        _forecast[i].Overview = String.Empty;
        _forecast[i].Day = String.Empty;
        _forecast[i].High = String.Empty;
        _forecast[i].Low = String.Empty;
      }
      GetID = (int)GUIWindow.Window.WINDOW_WEATHER;

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
      _nowImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGENOWICON);
      UpdateButtons();

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
          _nowLocation = loc.City;
          _urlSattelite = loc.UrlSattelite;
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
      if (ts.TotalMinutes >= _refreshIntercal && _locationCode != String.Empty)
        BackgroundUpdate(false);
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_BTNREFRESH)
            {
              OnRefresh();
            }
            if (iControl == (int)Controls.CONTROL_BTNVIEW)
            {
              OnChangeView();
            }
            if (iControl == (int)Controls.CONTROL_LOCATIONSELECT)
            {
              OnSelectLocation();
            }
            if (iControl == (int)Controls.CONTROL_BTNSWITCH)
              OnSwitchMode();
          }
          break;
      }
      return base.OnMessage(message);
    }

    private void OnSwitchMode()
    {
      if (_currentMode == Mode.Weather)
        _currentMode = Mode.Satellite;
      else if (_currentMode == Mode.Satellite)
        _currentMode = Mode.GeoClock;
      else
        _currentMode = Mode.Weather;
      GUIImage img = GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
      if (img != null)
      {
        //img.Filtering=true;
        //img.Centered=true;
        //img.KeepAspectRatio=true;
        switch (_imageView)
        {
          case ImageView.Satellite:
            {
              _urlViewImage = _urlSattelite;
              break;
            }
          case ImageView.Temperature:
            {
              _urlViewImage = _urlTemperature;
              break;
            }
          case ImageView.UVIndex:
            {
              _urlViewImage = _urlUvIndex;
              break;
            }
          case ImageView.Winds:
            {
              _urlViewImage = _urlWinds;
              break;
            }
          case ImageView.Humidity:
            {
              _urlViewImage = _urlHumidity;
              break;
            }
          case ImageView.Precipitation:
            {
              _urlViewImage = _urlPreciptation;
              break;
            }
        }
        img.SetFileName(_urlViewImage);
        //reallocate & load then new image
        img.FreeResources();
        img.AllocResources();
      }
      if (_currentMode == Mode.Weather)
      {
        _dayNum = -2;
        _selectedDayName = "All";
      }
      if (_currentMode == Mode.GeoClock)
      {
        _lastTimeSunClockRendered = 0;
        updateSunClock();
      }
      UpdateButtons();
    }

    private void OnSelectLocation()
    {
      GUIDialogMenu dialogOk = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dialogOk != null)
      {
        dialogOk.Reset();
        dialogOk.SetHeading(8);//my weather
        foreach (LocationInfo loc in _listLocations)
        {
          dialogOk.Add(loc.City);
        }
        dialogOk.DoModal(GetID);
        if (dialogOk.SelectedLabel >= 0)
        {
          LocationInfo loc = (LocationInfo)_listLocations[dialogOk.SelectedLabel];
          _locationCode = loc.CityCode;
          _nowLocation = loc.City;
          _urlSattelite = loc.UrlSattelite;
          _urlTemperature = loc.UrlTemperature;
          _urlUvIndex = loc.UrlUvIndex;
          _urlWinds = loc.UrlWinds;
          _urlHumidity = loc.UrlHumidity;
          _urlPreciptation = loc.UrlPrecip;
          GUIImage img = GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
          if (img != null)
          {
            //img.Filtering=true;
            //img.Centered=true;
            //img.KeepAspectRatio=true;
            switch (_imageView)
            {
              case ImageView.Satellite:
                {
                  _urlViewImage = _urlSattelite;
                  break;
                }
              case ImageView.Temperature:
                {
                  _urlViewImage = _urlTemperature;
                  break;
                }
              case ImageView.UVIndex:
                {
                  _urlViewImage = _urlUvIndex;
                  break;
                }
              case ImageView.Winds:
                {
                  _urlViewImage = _urlWinds;
                  break;
                }
              case ImageView.Humidity:
                {
                  _urlViewImage = _urlHumidity;
                  break;
                }
              case ImageView.Precipitation:
                {
                  _urlViewImage = _urlPreciptation;
                  break;
                }
            }
            img.SetFileName(_urlViewImage);
            //reallocate & load then new image
            img.FreeResources();
            img.AllocResources();
          }
          _dayNum = -2;
          _selectedDayName = "All";

          //refresh clicked so do a complete update (not an autoUpdate)
          BackgroundUpdate(false);
        }
      }
    }

    private void OnChangeView()
    {
      if (_currentMode == Mode.Satellite)
      {
        switch (_imageView)
        {
          case ImageView.Satellite:
            {
              _imageView = ImageView.Temperature;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
          case ImageView.Temperature:
            {
              _imageView = ImageView.UVIndex;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
          case ImageView.UVIndex:
            {
              _imageView = ImageView.Winds;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
          case ImageView.Winds:
            {
              _imageView = ImageView.Humidity;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
          case ImageView.Humidity:
            {
              _imageView = ImageView.Precipitation;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
          case ImageView.Precipitation:
            {
              _imageView = ImageView.Satellite;
              UpdateButtons();
              BackgroundUpdate(true);
              break;
            }
        }
      }
      else
      {
        switch (_dayNum)
        {
          case -2:
            {
              _selectedDayName = _forecast[0].Day;
              _dayNum = 0;
              UpdateButtons();
              _dayNum = 1;
              break;
            }
          case -1:
            {
              _selectedDayName = "All";
              UpdateButtons();
              _dayNum = 0;
              break;
            }
          case 0:
            {
              _selectedDayName = _forecast[0].Day;
              UpdateButtons();
              _dayNum = 1;
              break;
            }
          case 1:
            {
              _selectedDayName = _forecast[1].Day;
              UpdateButtons();
              _dayNum = 2;
              break;
            }
          case 2:
            {
              _selectedDayName = _forecast[2].Day;
              UpdateButtons();
              _dayNum = 3;
              break;
            }
          case 3:
            {
              _selectedDayName = _forecast[3].Day;
              UpdateButtons();
              _dayNum = -1;
              break;
            }
        }
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


    #region Serialisation
    void LoadSettings()
    {
      _listLocations.Clear();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _locationCode = xmlreader.GetValueAsString("weather", "location", String.Empty);
        _temperatureFarenheit = xmlreader.GetValueAsString("weather", "temperature", "C");
        _windSpeed = xmlreader.GetValueAsString("weather", "speed", "K");
        _refreshIntercal = xmlreader.GetValueAsInt("weather", "refresh", 30);

        bool bFound = false;
        for (int i = 0; i < 20; i++)
        {
          string cityTag = String.Format("city{0}", i);
          string strCodeTag = String.Format("code{0}", i);
          string strSatUrlTag = String.Format("sat{0}", i);
          string strTempUrlTag = String.Format("temp{0}", i);
          string strUVUrlTag = String.Format("uv{0}", i);
          string strWindsUrlTag = String.Format("winds{0}", i);
          string strHumidUrlTag = String.Format("humid{0}", i);
          string strPrecipUrlTag = String.Format("precip{0}", i);
          string city = xmlreader.GetValueAsString("weather", cityTag, String.Empty);
          string strCode = xmlreader.GetValueAsString("weather", strCodeTag, String.Empty);
          string strSatURL = xmlreader.GetValueAsString("weather", strSatUrlTag, String.Empty);
          string strTempURL = xmlreader.GetValueAsString("weather", strTempUrlTag, String.Empty);
          string strUVURL = xmlreader.GetValueAsString("weather", strUVUrlTag, String.Empty);
          string strWindsURL = xmlreader.GetValueAsString("weather", strWindsUrlTag, String.Empty);
          string strHumidURL = xmlreader.GetValueAsString("weather", strHumidUrlTag, String.Empty);
          string strPrecipURL = xmlreader.GetValueAsString("weather", strPrecipUrlTag, String.Empty);
          if (city.Length > 0 && strCode.Length > 0)
          {
            if (strSatURL.Length == 0)
              strSatURL = "http://www.zdf.de/ZDFde/wetter/showpicture/0,2236,161,00.gif";
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
            _locationCode = ((LocationInfo)_listLocations[0]).CityCode;
          }
        }
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("weather", "location", _locationCode);
        xmlwriter.SetValue("weather", "temperature", _temperatureFarenheit);
        xmlwriter.SetValue("weather", "speed", _windSpeed);
      }

    }
    #endregion

    int ConvertSpeed(int curSpeed)
    {
      //we might not need to convert at all
      if ((_temperatureFarenheit[0] == 'C' && _windSpeed[0] == 'K') ||
        (_temperatureFarenheit[0] == 'F' && _windSpeed[0] == 'M'))
        return curSpeed;

      //got through that so if temp is C, speed must be M or S
      if (_temperatureFarenheit[0] == 'C')
      {
        if (_windSpeed[0] == 'S')
          return (int)(curSpeed * (1000.0 / 3600.0) + 0.5);		//mps
        else
          return (int)(curSpeed / (8.0 / 5.0));		//mph
      }
      else
      {
        if (_windSpeed[0] == 'S')
          return (int)(curSpeed * (8.0 / 5.0) * (1000.0 / 3600.0) + 0.5);		//mps
        else
          return (int)(curSpeed * (8.0 / 5.0));		//kph
      }
    }

    void UpdateButtons()
    {
      if (_currentMode == Mode.Weather)
      {
        for (int i = 10; i < 900; ++i)
          GUIControl.ShowControl(GetID, i);
        GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNVIEW);
        GUIControl.ShowControl(GetID, (int)Controls.CONTROL_LOCATIONSELECT);


        for (int i = (int)Controls.CONTROL_IMAGE_SAT; i < (int)Controls.CONTROL_IMAGE_SAT_END; ++i)
          GUIControl.HideControl(GetID, i);
        GUIControl.HideControl(GetID, (int)Controls.CONTROL_IMAGE_SUNCLOCK);


        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(750));
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNREFRESH, GUILocalizeStrings.Get(184));			//Refresh
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELLOCATION, _nowLocation);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELUPDATED, _nowUpdated);

        //urgh, remove, create then add image each refresh to update nicely
        //Remove(_nowImage.GetID);
        int posX = _nowImage.XPosition;
        int posY = _nowImage.YPosition;
        //_nowImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGENOWICON, posX, posY, 128, 128, _nowIcon, 0);
        //Add(ref cntl);
        _nowImage.SetPosition(posX, posY);
        _nowImage.ColourDiffuse = 0xffffffff;
        _nowImage.SetFileName(_nowIcon);

        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWCOND, _nowCond);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWTEMP, _nowTemp);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWFEEL, _nowFeel);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWUVID, _nowUVId);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWWIND, _nowWind);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWDEWP, _nowDewp);
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTORL_LABELNOWHUMI, _nowHumd);

        //static labels
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(401));		//Temperature
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(402));		//Feels Like
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICUVID, GUILocalizeStrings.Get(403));		//UV Index
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICWIND, GUILocalizeStrings.Get(404));		//Wind
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(405));		//Dew Point
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICHUMI, GUILocalizeStrings.Get(406));		//Humidity

        if (_dayNum == -1 || _dayNum == -2)
        {
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_STATICSUNR);
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_STATICSUNS);
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELSUNR);
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELSUNS);
          for (int i = 0; i < NUM_DAYS; i++)
          {
            GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0DAY + (i * 10), _forecast[i].Day);
            GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0HI + (i * 10), _forecast[i].High);
            GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0LOW + (i * 10), _forecast[i].Low);
            GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELD0GEN + (i * 10), _forecast[i].Overview);

            //Seems a bit messy, but works. Remove, Create and then Add the image to update nicely
            //Remove(_forecast[i].m_pImage.GetID);
            GUIImage image = (GUIImage)GetControl((int)Controls.CONTROL_IMAGED0IMG + (i * 10));
            image.ColourDiffuse = 0xffffffff;
            image.SetFileName(_forecast[i].iconImageNameLow);
            //				_forecast[i].m_pImage = new GUIImage(GetID, (int)Controls.CONTROL_IMAGED0IMG+(i*10), posX, posY, 64, 64, _forecast[i].iconImageNameLow, 0);
            //			cntl=(GUIControl)_forecast[i].m_pImage;
            //		Add(ref cntl);
          }
        }
        else
        {
          for (int i = 0; i < NUM_DAYS; i++)
          {
            GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELD0DAY + (i * 10));
            GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELD0HI + (i * 10));
            GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELD0LOW + (i * 10));
            GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELD0GEN + (i * 10));
            GUIControl.HideControl(GetID, (int)Controls.CONTROL_IMAGED0IMG + (i * 10));
          }
          int currentDayNum = _dayNum;

          GUIControl.HideControl(GetID, (int)Controls.CONTROL_STATICUVID);
          GUIControl.HideControl(GetID, (int)Controls.CONTROL_LABELNOWUVID);

          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICSUNR, GUILocalizeStrings.Get(744));
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICSUNS, GUILocalizeStrings.Get(745));
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICTEMP, GUILocalizeStrings.Get(746));		//High
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICFEEL, GUILocalizeStrings.Get(747));		//Low
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STATICDEWP, GUILocalizeStrings.Get(748));

          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELSUNR, _forecast[_dayNum].SunRise);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELSUNS, _forecast[_dayNum].SunSet);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTORL_LABELNOWHUMI, _forecast[_dayNum].Humidity);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWTEMP, _forecast[_dayNum].High);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWFEEL, _forecast[_dayNum].Low);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWCOND, _forecast[_dayNum].Overview);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWDEWP, _forecast[_dayNum].Precipitation);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELNOWWIND, _forecast[_dayNum].Wind);
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELUPDATED, _forcastUpdated);

          GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNVIEW);
          GUIControl.ShowControl(GetID, (int)Controls.CONTROL_LOCATIONSELECT);

          //					_nowImage.SetFileName(_forecast[currentDayNum].iconImageNameLow);
          _nowImage.SetFileName(_forecast[currentDayNum].iconImageNameHigh);
        }
      }
      else if (_currentMode == Mode.Satellite)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(19100));

        for (int i = 10; i < 900; ++i)
          GUIControl.HideControl(GetID, i);
        GUIControl.HideControl(GetID, (int)Controls.CONTROL_IMAGE_SUNCLOCK);

        for (int i = (int)Controls.CONTROL_IMAGE_SAT; i < (int)Controls.CONTROL_IMAGE_SAT_END; ++i)
          GUIControl.ShowControl(GetID, i);

        GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNVIEW);
        GUIControl.ShowControl(GetID, (int)Controls.CONTROL_LOCATIONSELECT);

      }
      else if (_currentMode == Mode.GeoClock)
      {
        GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNSWITCH, GUILocalizeStrings.Get(717));
        GUIControl.HideControl(GetID, (int)Controls.CONTROL_BTNVIEW);
        GUIControl.HideControl(GetID, (int)Controls.CONTROL_LOCATIONSELECT);

        for (int i = (int)Controls.CONTROL_IMAGE_SAT; i < (int)Controls.CONTROL_IMAGE_SAT_END; ++i)
          GUIControl.HideControl(GetID, i);
        for (int i = 10; i < 900; ++i)
          GUIControl.HideControl(GetID, i);

        GUIControl.ShowControl(GetID, (int)Controls.CONTROL_IMAGE_SUNCLOCK);

      }
      if (_currentMode == Mode.Satellite)
      {
        switch (_imageView)
        {
          case ImageView.Satellite:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(737));
              break;
            }
          case ImageView.Temperature:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(738));
              break;
            }
          case ImageView.UVIndex:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(739));
              break;
            }
          case ImageView.Winds:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(740));
              break;
            }
          case ImageView.Humidity:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(741));
              break;
            }
          case ImageView.Precipitation:
            {
              GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(742));
              break;
            }
        }
      }
      else if (_currentMode == Mode.Weather)
      {

        if (_selectedDayName == "All")
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, GUILocalizeStrings.Get(743));
        else
          GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNVIEW, _selectedDayName);
      }

      // Update sattelite image
      GUIImage img = GetControl((int)Controls.CONTROL_IMAGE_SAT) as GUIImage;
      if (img != null)
      {
        switch (_imageView)
        {
          case ImageView.Satellite:
            {
              _urlViewImage = _urlSattelite;
              break;
            }
          case ImageView.Temperature:
            {
              _urlViewImage = _urlTemperature;
              break;
            }
          case ImageView.UVIndex:
            {
              _urlViewImage = _urlUvIndex;
              break;
            }
          case ImageView.Winds:
            {
              _urlViewImage = _urlWinds;
              break;
            }
          case ImageView.Humidity:
            {
              _urlViewImage = _urlHumidity;
              break;
            }
          case ImageView.Precipitation:
            {
              _urlViewImage = _urlPreciptation;
              break;
            }
        }
        img.SetFileName(_urlViewImage);
        //reallocate & load then new image
        img.FreeResources();
        img.AllocResources();
      }
    }

    bool Download(string weatherFile)
    {
      string url;

      bool skipConnectionTest = false;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        skipConnectionTest = xmlreader.GetValueAsBool("weather", "skipconnectiontest", false);

      Log.Write("MyWeather.SkipConnectionTest: {0}", skipConnectionTest);

      int code = 0;

      if (!Util.Win32API.IsConnectedToInternet(ref code))
      {
        if (System.IO.File.Exists(weatherFile)) return true;

        Log.Write("MyWeather.Download: No internet connection {0}", code);

        if (skipConnectionTest == false)
          return false;
      }

      char c_units = _temperatureFarenheit[0];	//convert from temp units to metric/standard
      if (c_units == 'F')	//we'll convert the speed later depending on what thats set to
        c_units = 's';
      else
        c_units = 'm';

      url = String.Format("http://xoap.weather.com/weather/local/{0}?cc=*&unit={1}&dayf=4&prod=xoap&par={2}&key={3}",
        _locationCode, c_units.ToString(), PARTNER_ID, PARTNER_KEY);

      using (WebClient client = new WebClient())
      {
        try
        {
          client.DownloadFile(url, weatherFile);
          return true;
        }
        catch (Exception ex)
        {
          Log.Write("Failed to download weather:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
      }
      return false;
    }

    //convert weather.com day strings into localized string id's
    string LocalizeDay(string dayName)
    {
      string localizedDay = String.Empty;

      if (dayName == "Monday")			//monday is localized string 11
        localizedDay = GUILocalizeStrings.Get(11);
      else if (dayName == "Tuesday")
        localizedDay = GUILocalizeStrings.Get(12);
      else if (dayName == "Wednesday")
        localizedDay = GUILocalizeStrings.Get(13);
      else if (dayName == "Thursday")
        localizedDay = GUILocalizeStrings.Get(14);
      else if (dayName == "Friday")
        localizedDay = GUILocalizeStrings.Get(15);
      else if (dayName == "Saturday")
        localizedDay = GUILocalizeStrings.Get(16);
      else if (dayName == "Sunday")
        localizedDay = GUILocalizeStrings.Get(17);
      else
        localizedDay = String.Empty;

      return localizedDay;
    }

    string RelocalizeTime(string usFormatTime)
    {
      string result = usFormatTime;

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

    string LocalizeOverview(string token)
    {
      string localizedLine = String.Empty;

      foreach (string tokenSplit in token.Split(' '))
      {
        string localizedWord = String.Empty;

        if (String.Compare(tokenSplit, "T-Storms", true) == 0)
          localizedWord = GUILocalizeStrings.Get(370);
        else if (String.Compare(tokenSplit, "Partly", true) == 0)
          localizedWord = GUILocalizeStrings.Get(371);
        else if (String.Compare(tokenSplit, "Mostly", true) == 0)
          localizedWord = GUILocalizeStrings.Get(372);
        else if (String.Compare(tokenSplit, "Sunny", true) == 0)
          localizedWord = GUILocalizeStrings.Get(373);
        else if (String.Compare(tokenSplit, "Cloudy", true) == 0 || String.Compare(tokenSplit, "Clouds", true) == 0)
          localizedWord = GUILocalizeStrings.Get(374);
        else if (String.Compare(tokenSplit, "Snow", true) == 0)
          localizedWord = GUILocalizeStrings.Get(375);
        else if (String.Compare(tokenSplit, "Rain", true) == 0)
          localizedWord = GUILocalizeStrings.Get(376);
        else if (String.Compare(tokenSplit, "Light", true) == 0)
          localizedWord = GUILocalizeStrings.Get(377);
        else if (String.Compare(tokenSplit, "AM", true) == 0)
          localizedWord = GUILocalizeStrings.Get(378);
        else if (String.Compare(tokenSplit, "PM", true) == 0)
          localizedWord = GUILocalizeStrings.Get(379);
        else if (String.Compare(tokenSplit, "Showers", true) == 0)
          localizedWord = GUILocalizeStrings.Get(380);
        else if (String.Compare(tokenSplit, "Few", true) == 0)
          localizedWord = GUILocalizeStrings.Get(381);
        else if (String.Compare(tokenSplit, "Scattered", true) == 0)
          localizedWord = GUILocalizeStrings.Get(382);
        else if (String.Compare(tokenSplit, "Wind", true) == 0)
          localizedWord = GUILocalizeStrings.Get(383);
        else if (String.Compare(tokenSplit, "Strong", true) == 0)
          localizedWord = GUILocalizeStrings.Get(384);
        else if (String.Compare(tokenSplit, "Fair", true) == 0)
          localizedWord = GUILocalizeStrings.Get(385);
        else if (String.Compare(tokenSplit, "Clear", true) == 0)
          localizedWord = GUILocalizeStrings.Get(386);
        else if (String.Compare(tokenSplit, "Early", true) == 0)
          localizedWord = GUILocalizeStrings.Get(387);
        else if (String.Compare(tokenSplit, "and", true) == 0)
          localizedWord = GUILocalizeStrings.Get(388);
        else if (String.Compare(tokenSplit, "Fog", true) == 0)
          localizedWord = GUILocalizeStrings.Get(389);
        else if (String.Compare(tokenSplit, "Haze", true) == 0)
          localizedWord = GUILocalizeStrings.Get(390);
        else if (String.Compare(tokenSplit, "Windy", true) == 0)
          localizedWord = GUILocalizeStrings.Get(391);
        else if (String.Compare(tokenSplit, "Drizzle", true) == 0)
          localizedWord = GUILocalizeStrings.Get(392);
        else if (String.Compare(tokenSplit, "Freezing", true) == 0)
          localizedWord = GUILocalizeStrings.Get(393);
        else if (String.Compare(tokenSplit, "N/A", true) == 0)
          localizedWord = GUILocalizeStrings.Get(394);

        if (localizedWord == String.Empty)
          localizedWord = tokenSplit;	//if not found, let fallback

        localizedLine = localizedLine + localizedWord;
        localizedLine += " ";
      }

      return localizedLine;

    }

    //splitStart + End are the chars to search between for a space to replace with a \n
    void SplitLongString(ref string lineString, int splitStart, int splitEnd)
    {
      //search chars 10 to 15 for a space
      //if we find one, replace it with a newline
      for (int i = splitStart; i < splitEnd && i < (int)lineString.Length; i++)
      {
        if (lineString[i] == ' ')
        {
          lineString = lineString.Substring(0, i) + "\n" + lineString.Substring(i + 1);
          return;
        }
      }
    }

    //Do a complete download, parse and update
    void RefreshMe(bool autoUpdate)
    {
      using (WaitCursor cursor = new WaitCursor())
        lock (this)
        {
          //message strings for refresh of images
          string weatherFile = @"weather\curWeather.xml";

          GUIDialogOK dialogOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          bool dlRes = false, ldRes = false;

          //Do The Download
          dlRes = Download(weatherFile);

          if (dlRes)	//dont load if download failed
            ldRes = LoadWeather(weatherFile);	//parse

          //if the download or load failed, display an error message
          if ((!dlRes || !ldRes)) //this will probably crash on an autoupdate as well, but not tested
          {
            // show failed dialog...
            dialogOk.SetHeading(412);	//"Unable to get weather data"
            dialogOk.SetLine(1, _nowLocation);
            dialogOk.SetLine(2, String.Empty);
            dialogOk.SetLine(3, String.Empty);
            dialogOk.DoModal(GetID);
          }
          else if (dlRes && ldRes)	//download and load went ok so update
          {
            UpdateButtons();
          }

          _refreshTimer = DateTime.Now;
          _dayNum = -2;
        }
    }

    bool LoadWeather(string weatherFile)
    {
      int tempInteger = 0;
      string tempString = String.Empty;
      string unitTemperature = String.Empty;
      string unitSpeed = String.Empty;
      DateTime time = DateTime.Now;

      // load the xml file
      XmlDocument doc = new XmlDocument();
      doc.Load(weatherFile);

      if (doc.DocumentElement == null)
        return false;

      string root = doc.DocumentElement.Name;
      XmlNode xmlElement = doc.DocumentElement;
      if (root == "error")
      {
        string szCheckError;

        GUIDialogOK dialogOk = (GUIDialogOK)GUIWindowManager.GetWindow(2002);

        GetString(xmlElement, "err", out szCheckError, "Unknown Error");	//grab the error string

        // show error dialog...
        dialogOk.SetHeading(412);	//"Unable to get weather data"
        dialogOk.SetLine(1, szCheckError);
        dialogOk.SetLine(2, _nowLocation);
        dialogOk.SetLine(3, String.Empty);
        dialogOk.DoModal(GetID);
        return true;	//we got a message so do display a second in refreshme()
      }

      // units (C or F and mph or km/h or m/s) 
      unitTemperature = _temperatureFarenheit;

      if (_windSpeed[0] == 'M')
        unitSpeed = "mph";
      else if (_windSpeed[0] == 'K')
        unitSpeed = "km/h";
      else
        unitSpeed = "m/s";

      // location
      XmlNode element = xmlElement.SelectSingleNode("loc");
      if (null != element)
      {
        GetString(element, "dnam", out _nowLocation, String.Empty);
      }

      //current weather
      element = xmlElement.SelectSingleNode("cc");
      if (null != element)
      {
        GetString(element, "lsup", out _nowUpdated, String.Empty);
        _nowUpdated = RelocalizeTime(_nowUpdated);

        GetInteger(element, "icon", out tempInteger);
        _nowIcon = String.Format(@"weather\128x128\{0}.png", tempInteger);

        GetString(element, "t", out _nowCond, String.Empty);			//current condition
        _nowCond = LocalizeOverview(_nowCond);
        SplitLongString(ref _nowCond, 8, 15);				//split to 2 lines if needed

        GetInteger(element, "tmp", out tempInteger);				//current temp
        _nowTemp = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);
        GetInteger(element, "flik", out tempInteger);				//current 'Feels Like'
        _nowFeel = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);

        XmlNode pNestElement = element.SelectSingleNode("wind");	//current wind
        if (null != pNestElement)
        {
          GetInteger(pNestElement, "s", out tempInteger);			//current wind strength
          tempInteger = ConvertSpeed(tempInteger);				//convert speed if needed
          GetString(pNestElement, "t", out  tempString, "N");		//current wind direction

          //From <dir eg NW> at <speed> km/h		 GUILocalizeStrings.Get(407)
          //This is a bit untidy, but i'm fed up with localization and string formats :)
          string windFrom = GUILocalizeStrings.Get(407);
          string windAt = GUILocalizeStrings.Get(408);

          _nowWind = String.Format("{0} {1} {2} {3} {4}",
            windFrom, tempString,
            windAt, tempInteger, unitSpeed);
        }

        GetInteger(element, "hmid", out tempInteger);				//current humidity
        _nowHumd = String.Format("{0}%", tempInteger);

        pNestElement = element.SelectSingleNode("uv");	//current UV index
        if (null != pNestElement)
        {
          GetInteger(pNestElement, "i", out tempInteger);
          GetString(pNestElement, "t", out  tempString, String.Empty);
          _nowUVId = String.Format("{0} {1}", tempInteger, tempString);
        }

        GetInteger(element, "dewp", out tempInteger);				//current dew point
        _nowDewp = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);

      }

      //future forcast
      element = xmlElement.SelectSingleNode("dayf");
      GetString(element, "lsup", out _forcastUpdated, String.Empty);
      if (null != element)
      {
        XmlNode pOneDayElement = element.SelectSingleNode("day"); ;
        for (int i = 0; i < NUM_DAYS; i++)
        {
          if (null != pOneDayElement)
          {
            _forecast[i].Day = pOneDayElement.Attributes.GetNamedItem("t").InnerText;
            _forecast[i].Day = LocalizeDay(_forecast[i].Day);

            GetString(pOneDayElement, "hi", out  tempString, String.Empty);	//string cause i've seen it return N/A
            if (tempString == "N/A")
              _forecast[i].High = String.Empty;
            else
              _forecast[i].High = String.Format("{0}{1}{2}", tempString, DEGREE_CHARACTER, unitTemperature);

            GetString(pOneDayElement, "low", out  tempString, String.Empty);
            if (tempString == "N/A")
              _forecast[i].High = String.Empty;
            else
              _forecast[i].Low = String.Format("{0}{1}{2}", tempString, DEGREE_CHARACTER, unitTemperature);

            GetString(pOneDayElement, "sunr", out  tempString, String.Empty);
            if (tempString == "N/A")
              _forecast[i].SunRise = String.Empty;
            else
              _forecast[i].SunRise = String.Format("{0}", tempString);

            GetString(pOneDayElement, "suns", out  tempString, String.Empty);
            if (tempString == "N/A")
              _forecast[i].SunSet = String.Empty;
            else
              _forecast[i].SunSet = String.Format("{0}", tempString);
            XmlNode pDayTimeElement = pOneDayElement.SelectSingleNode("part");	//grab the first day/night part (should be day)
            if (i == 0 && (time.Hour < 7 || time.Hour >= 19))	//weather.com works on a 7am to 7pm basis so grab night if its late in the day
              pDayTimeElement = pDayTimeElement.NextSibling;//.NextSiblingElement("part");

            if (null != pDayTimeElement)
            {
              GetInteger(pDayTimeElement, "icon", out tempInteger);
              _forecast[i].iconImageNameLow = String.Format("weather\\64x64\\{0}.png", tempInteger);
              _forecast[i].iconImageNameHigh = String.Format("weather\\128x128\\{0}.png", tempInteger);
              GetString(pDayTimeElement, "t", out  _forecast[i].Overview, String.Empty);
              _forecast[i].Overview = LocalizeOverview(_forecast[i].Overview);
              SplitLongString(ref _forecast[i].Overview, 6, 15);
              GetInteger(pDayTimeElement, "hmid", out tempInteger);
              _forecast[i].Humidity = String.Format("{0}%", tempInteger);
              GetInteger(pDayTimeElement, "ppcp", out tempInteger);
              _forecast[i].Precipitation = String.Format("{0}%", tempInteger);
            }
            XmlNode pWindElement = pDayTimeElement.SelectSingleNode("wind");	//current wind
            if (null != pWindElement)
            {
              GetInteger(pWindElement, "s", out tempInteger);			//current wind strength
              tempInteger = ConvertSpeed(tempInteger);				//convert speed if needed
              GetString(pWindElement, "t", out  tempString, "N");		//current wind direction

              //From <dir eg NW> at <speed> km/h		 GUILocalizeStrings.Get(407)
              //This is a bit untidy, but i'm fed up with localization and string formats :)
              string windFrom = GUILocalizeStrings.Get(407);
              string windAt = GUILocalizeStrings.Get(408);

              _forecast[i].Wind = String.Format("{0} {1} {2} {3} {4}",
                windFrom, tempString,
                windAt, tempInteger, unitSpeed);
            }
          }
          pOneDayElement = pOneDayElement.NextSibling;//Element("day");
        }
      }

      //			if (pDlgProgress!=null)
      //			{
      //				pDlgProgress.SetPercentage(70);
      //				pDlgProgress.Progress();
      //			}
      return true;
    }


    void GetString(XmlNode xmlElement, string tagName, out string stringValue, string defaultValue)
    {
      stringValue = String.Empty;

      XmlNode node = xmlElement.SelectSingleNode(tagName);
      if (node != null)
      {
        if (node.InnerText != null)
        {
          if (node.InnerText != "-")
            stringValue = node.InnerText;
        }
      }
      if (stringValue.Length == 0)
      {
        stringValue = defaultValue;
      }
    }

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _refreshTimer;
      if (ts.TotalMinutes >= _refreshIntercal && _locationCode != String.Empty)
      {
        _refreshTimer = DateTime.Now;
        _selectedDayName = "All";
        _dayNum = -2;

        //refresh clicked so do a complete update (not an autoUpdate)
        BackgroundUpdate(true);

        _refreshTimer = DateTime.Now;
      }
      base.Process();
    }

    void GetInteger(XmlNode xmlElement, string tagName, out int intValue)
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

    public override void Render(float timePassed)
    {
      if (_currentMode == Mode.GeoClock && _lastTimeSunClockRendered > 10)
      {
        updateSunClock();
        _lastTimeSunClockRendered = 0;
      }
      else
        _lastTimeSunClockRendered += timePassed;
      base.Render(timePassed);
    }


    private void updateSunClock()
    {
      GUIImage clockImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGE_SUNCLOCK);
      lock (clockImage)
      {
        Bitmap image = _geochronGenerator.update(DateTime.UtcNow);
        System.Drawing.Image img = (Image)image.Clone();
        clockImage.FileName = "";
        clockImage.FreeResources();
        clockImage.IsVisible = false;
        GUITextureManager.ReleaseTexture("#useMemoryImage");
        clockImage.FileName = "#useMemoryImage";
        clockImage.MemoryImage = img;
        clockImage.AllocResources();
        clockImage.IsVisible = true;
      }
    }


    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "My Weather";
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

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(8);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Plugin to show the current weather";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIWindowWeather.ShowPlugin implementation
    }

    #endregion

    ///////////////////////////////////////////

    void BackgroundUpdate(bool isAuto)
    {
      BackgroundWorker worker = new BackgroundWorker();

      worker.DoWork += new DoWorkEventHandler(DownloadWorker);
      worker.RunWorkerAsync(isAuto);

      while (_workerCompleted == false)
        GUIWindowManager.Process();
    }

    void DownloadWorker(object sender, DoWorkEventArgs e)
    {
      _workerCompleted = false;

      _refreshTimer = DateTime.Now;
      RefreshMe((bool)e.Argument);	//do an autoUpdate refresh
      _refreshTimer = DateTime.Now;

      _workerCompleted = true;
    }

    bool _workerCompleted = false;
  }
}

