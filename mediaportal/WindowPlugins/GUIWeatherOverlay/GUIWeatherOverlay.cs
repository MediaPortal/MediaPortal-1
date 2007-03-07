#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Configuration;
using MediaPortal.Threading;
using MediaPortal.Player;


namespace MediaPortal.GUI.WeatherOverlay
{
  /// <summary>
  ///  Weather Overlay
  /// </summary>
  public class GUIWeatherOverlay : GUIOverlayWindow, IRenderLayer
  {
    #region Imports
    #endregion

    #region Enums
    enum SkinControls
    {
      TEMPERATURE = 2,
      IMAGE       = 3,
      LOCATION    = 4,
      WEATHER_DESCRIPTION = 5
    }
    #endregion

    #region Delegates
    #endregion

    #region Events
    #endregion

    #region <skin> Variables
    [SkinControlAttribute(2)]    protected GUIFadeLabel _labelTemperature = null;
    [SkinControlAttribute(4)]    protected GUIFadeLabel _labelLocation    = null;
    [SkinControlAttribute(5)]    protected GUIFadeLabel _labelDescription = null;
    [SkinControlAttribute(3)]    protected GUIImage     _imageIcon = null;
    #endregion

    #region Variables
    // Private Variables
    private bool _xmlFilePresent = false;
    private bool _weatherOverlay = true;
    private bool _didRenderLastTime = false;
    private IntervalWork _weatherIntervalWork = null;
    private string _locationCode = String.Empty;
    private int _refreshInterval = 30;
    // Protected Variables
    protected string _currentLocation = String.Empty;
    protected string _currentTemperature = String.Empty;
    protected string _currentDescription = String.Empty;
    protected string _currentImage = String.Empty;
    // Public Variables
    #endregion

    #region Constructors/Destructors
    public GUIWeatherOverlay()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WEATHER_OVERLAY;
    }
    #endregion

    #region Properties
    // Public Properties
    #endregion

    #region Private Methods
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _locationCode = xmlreader.GetValueAsString("weather", "location", String.Empty);
        _refreshInterval = xmlreader.GetValueAsInt("weather", "refresh", 60);
        _weatherOverlay = xmlreader.GetValueAsBool("weather", "overlayEnabled", true);
      }
    }
    
    void OnUpdateState(bool render)
    {
      if (_didRenderLastTime != render)
      {
        _didRenderLastTime = render;
        if (render)
        {
          QueueAnimation(AnimationType.WindowOpen);
        }
        else
        {
          QueueAnimation(AnimationType.WindowClose);
        }
      }
    }

    #region Threading
    void StartWeatherThread()
    {
      _weatherIntervalWork = new IntervalWork(new DoWorkHandler(this.DownloadWeatherInfo), new TimeSpan(0,_refreshInterval,0));
      _weatherIntervalWork.Description = "Download Weather Info";
      GlobalServiceProvider.Get<IThreadPool>().AddIntervalWork(_weatherIntervalWork, true);
      Log.Info("WeatherThread - started");
    }

    void StopWeatherThread()
    {
      if (_weatherIntervalWork != null)
      {
        GlobalServiceProvider.Get<IThreadPool>().RemoveIntervalWork(_weatherIntervalWork);
        _weatherIntervalWork = null;
        Log.Info("WeatherThread - stopped");
      }
    }

    void DownloadWeatherInfo()
    {
      Log.Debug("WeatherOverlay: Download Weather Info - called");
      string weatherFile = Config.GetFile(Config.Dir.Weather, "curWeather.xml");
      LoadSettings();
      WeatherForecast weather = new WeatherForecast(_locationCode);
      if (weather.UpDate())
      {
        _currentLocation = weather.LocalInfo.City;
        _currentTemperature = weather.CurCondition.Temperature;
        _currentDescription = weather.CurCondition.Condition;
        _currentImage = weather.CurCondition.Icon;

        Log.Debug("WeatherOverlay: Location    = " + _currentLocation);
        Log.Debug("WeatherOverlay: Temperature = " + _currentTemperature);
        Log.Debug("WeatherOverlay: Description = " + _currentDescription);
        Log.Debug("WeatherOverlay: Icon = " + _currentImage);
      }
    }

    #endregion

    #endregion

    #region <Base class> Overloads
    public override bool Init()
    {
      _xmlFilePresent = false;
      if (System.IO.File.Exists(GUIGraphicsContext.Skin + @"\weatherOverlay.xml"))
      {
        _xmlFilePresent = Load(GUIGraphicsContext.Skin + @"\weatherOverlay.xml");
        GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.WeatherOverlay);
      }
      StartWeatherThread();
      return _xmlFilePresent;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
      StartWeatherThread();
    }

    public override void DeInit()
    {
      base.DeInit();
      StopWeatherThread();
    }


    public override void Render(float timePassed)
    {
      base.Render(timePassed);
    }

    public override bool DoesPostRender()
    {
      if (!_xmlFilePresent ||
          !_weatherOverlay ||
          GUIGraphicsContext.IsFullScreenVideo || 
          GUIGraphicsContext.Calibrating ||
          !GUIGraphicsContext.Overlay
        )
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
            
      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 5) return;

      // Set current Current Location
      if (_labelLocation != null)
      {
        _labelLocation.Label = _currentLocation;
        _labelLocation.Render(timePassed);
      }
      
      // Set current Temp 
      if (_labelTemperature != null)
      {
        _labelTemperature.Label = _currentTemperature;
        _labelTemperature.Render(timePassed);
      }

      // Set current Weather Description
      if (_labelDescription != null)
      {
        _labelDescription.Label = _currentDescription;
        _labelDescription.Render(timePassed);
      }
    
      // Set Image
      if (_imageIcon != null)
      {
        _imageIcon.FileName = _currentImage;
        _imageIcon.Render(timePassed);
      }
    }
    #endregion

    #region <Interface> Implementations
    // region for each interface
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }
    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 5);
    }
    #endregion

    #endregion
  }




}
