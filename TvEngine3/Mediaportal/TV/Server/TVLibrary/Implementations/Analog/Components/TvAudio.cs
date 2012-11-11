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
using System.Collections.Generic;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Analog.GraphComponents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The TvAudio component of the graph
  /// </summary>
  internal class TvAudio : IDisposable
  {
    #region variables

    /// <summary>
    /// The TvAudio filter
    /// </summary>
    private IBaseFilter _filterTvAudioTuner;

    /// <summary>
    /// The TvAudio interface for changing the audio stream
    /// </summary>
    private IAMTVAudio _tvAudioTunerInterface;

    /// <summary>
    /// The TvAudio device
    /// </summary>
    private DsDevice _audioDevice;

    /// <summary>
    /// The variant or mode of the audio source in the graph.
    /// </summary>
    private TvAudioVariant _variant;

    /// <summary>
    /// The set of audio modes that the hardware supports.
    /// </summary>
    private TVAudioMode _hardwareSupportedModes;

    #endregion

    #region properties

    public String TvAudioName
    {
      get
      {
        if (_audioDevice != null)
        {
          return _audioDevice.Name;
        }
        return String.Empty;
      }
    }

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // get rid of managed resources
        if (_audioDevice != null)
        {
          DevicesInUse.Instance.Remove(_audioDevice);
          _audioDevice.Dispose();
          _audioDevice = null;
        }
      }

      // get rid of unmanaged resources
      if (_variant == TvAudioVariant.Normal)
      {
        if (_filterTvAudioTuner != null)
        {
          while (Release.ComObject(_filterTvAudioTuner) > 0) { }
          _filterTvAudioTuner = null;          
        }
      }
    }

    /// <summary>
    /// Disposes the TvAudio component
    /// </summary>    
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~TvAudio()
    {
      Dispose(false);
    }

    #endregion

    #region CreateFlterInstance method

    /// <summary>
    /// Adds the tv audio tuner to the graph and connects it to the crossbar.
    /// At the end of this method the graph looks like:
    /// [          ] ------------------------->[           ]
    /// [ tvtuner  ]                           [ crossbar  ]
    /// [          ]----[            ]-------->[           ]
    ///                 [ tvaudio    ]
    ///                 [   tuner    ]
    /// </summary>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    public bool CreateFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner, Crossbar crossbar)
    {
      if (!string.IsNullOrEmpty(graph.TvAudio.Name) && graph.TvAudio.Mode != TvAudioVariant.Unavailable)
      {
        this.LogDebug("analog: Using TvAudio configuration from stored graph");

        if (CreateConfigurationBasedFilterInstance(graph, tuner, crossbar, graphBuilder))
        {
          this.LogDebug("analog: Using TvAudio configuration from stored graph succeeded");
          return true;
        }
      }
      if (tuner.AudioPin == null)
      {
        this.LogDebug("analog: AddTvAudioFilter no tv audio device needed!");
        _variant = TvAudioVariant.Unavailable;
        return true;
      }
      this.LogDebug("analog: No stored graph for TvAudio component - Trying to detect");
      return CreateAutomaticFilterInstance(graph, tuner, crossbar, graphBuilder);
    }

    #endregion

    #region private helper methods

    /// <summary>
    /// Creates the filter based on the configuration file
    /// </summary>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateConfigurationBasedFilterInstance(Graph graph, Tuner tuner, Crossbar crossbar,
                                                        IFilterGraph2 graphBuilder)
    {
      if (graph.TvAudio.Mode == TvAudioVariant.TvTuner || graph.TvAudio.Mode == TvAudioVariant.TvTunerConnection)
      {
        this.LogDebug("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
        int hr = graphBuilder.Connect(tuner.AudioPin, crossbar.AudioTunerIn);
        if (hr != 0)
        {
          this.LogError("analog: unable to add TvAudioTuner to graph - even TvTuner as TvAudio fails");
          return false;
        }
        if (graph.TvAudio.Mode == TvAudioVariant.TvTuner)
        {
          this.LogDebug("analog: AddTvAudioFilter connected TvTuner with Crossbar directly succeeded!");
          _tvAudioTunerInterface = tuner.Filter as IAMTVAudio;
          if (_tvAudioTunerInterface != null)
          {
            this.LogDebug("analog: AddTvAudioFilter succeeded - TvTuner is also TvAudio");
            _filterTvAudioTuner = tuner.Filter;
            _variant = TvAudioVariant.TvTuner;
            _hardwareSupportedModes = graph.TvAudio.AudioModes;
            return true;
          }
          return false;
        }
        return true;
      }
      string deviceName = graph.TvAudio.Name;
      //get all tv audio tuner devices on this system
      DsDevice[] devices = null;
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVAudio);
        devices = DeviceSorter.Sort(devices, tuner.TunerName, crossbar.CrossBarName);
      }
      catch (Exception)
      {
        this.LogDebug("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
      }
      if (devices != null && devices.Length > 0)
      {
        // try each tv audio tuner
        for (int i = 0; i < devices.Length; i++)
        {
          IBaseFilter tmp;
          //if tv audio tuner is currently in use we can skip it
          if (DevicesInUse.Instance.IsUsed(devices[i]))
            continue;
          if (!deviceName.Equals(devices[i].Name))
            continue;
          this.LogDebug("analog: AddTvAudioFilter use:{0} {1}", devices[i].Name, i);
          int hr;
          try
          {
            //add tv audio tuner to graph
            hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("analog: cannot add filter to graph");
            continue;
          }
          if (hr != 0)
          {
            //failed to add tv audio tuner to graph, continue with the next one
            if (tmp != null)
            {
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("tvAudioFilter filter", tmp);
            }
            continue;
          }
          // try connecting the tv tuner-> tv audio tuner
          if (FilterGraphTools.ConnectPin(graphBuilder, tuner.AudioPin, tmp, 0))
          {
            // Got it !
            // Connect tv audio tuner to the crossbar
            IPin pin = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
            hr = graphBuilder.Connect(pin, crossbar.AudioTunerIn);
            if (hr < 0)
            {
              //failed
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("audiotuner pinin", pin);
              Release.ComObject("audiotuner filter", tmp);
            }
            else
            {
              //succeeded. we're done
              this.LogDebug("analog: AddTvAudioFilter succeeded:{0}", devices[i].Name);
              Release.ComObject("audiotuner pinin", pin);
              _filterTvAudioTuner = tmp;
              _audioDevice = devices[i];
              DevicesInUse.Instance.Add(_audioDevice);
              _tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
              break;
            }
          }
          else
          {
            // cannot connect tv tuner-> tv audio tuner, try next one...
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiotuner filter", tmp);
          }
        }
      }
      if (_filterTvAudioTuner == null)
      {
        return false;
      }
      _variant = TvAudioVariant.Normal;
      _hardwareSupportedModes = graph.TvAudio.AudioModes;
      return true;
    }

    /// <summary>
    /// Creates the filter by trying to detect it
    /// </summary>
    /// <param name="crossbar">The crossbar componen</param>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateAutomaticFilterInstance(Graph graph, Tuner tuner, Crossbar crossbar, IFilterGraph2 graphBuilder)
    {
      //get all tv audio tuner devices on this system
      DsDevice[] devices = null;
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVAudio);
        devices = DeviceSorter.Sort(devices, tuner.TunerName, crossbar.CrossBarName);
      }
      catch (Exception)
      {
        this.LogDebug("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
      }
      if (devices != null && devices.Length > 0)
      {
        // try each tv audio tuner
        for (int i = 0; i < devices.Length; i++)
        {
          IBaseFilter tmp;
          this.LogDebug("analog: AddTvAudioFilter try:{0} {1}", devices[i].Name, i);
          //if tv audio tuner is currently in use we can skip it
          if (DevicesInUse.Instance.IsUsed(devices[i]))
            continue;
          int hr;
          try
          {
            //add tv audio tuner to graph
            hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("analog: cannot add filter to graph");
            continue;
          }
          if (hr != 0)
          {
            //failed to add tv audio tuner to graph, continue with the next one
            if (tmp != null)
            {
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("tvAudioFilter filter", tmp);
            }
            continue;
          }
          // try connecting the tv tuner-> tv audio tuner
          if (FilterGraphTools.ConnectPin(graphBuilder, tuner.AudioPin, tmp, 0))
          {
            // Got it !
            // Connect tv audio tuner to the crossbar
            IPin pin = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
            hr = graphBuilder.Connect(pin, crossbar.AudioTunerIn);
            if (hr < 0)
            {
              //failed
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("audiotuner pinin", pin);
              Release.ComObject("audiotuner filter", tmp);
            }
            else
            {
              //succeeded. we're done
              this.LogDebug("analog: AddTvAudioFilter succeeded:{0}", devices[i].Name);
              Release.ComObject("audiotuner pinin", pin);
              _filterTvAudioTuner = tmp;
              _audioDevice = devices[i];
              DevicesInUse.Instance.Add(_audioDevice);
              _tvAudioTunerInterface = tuner.Filter as IAMTVAudio;
              break;
            }
          }
          else
          {
            // cannot connect tv tuner-> tv audio tuner, try next one...
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiotuner filter", tmp);
          }
        }
      }
      if (_filterTvAudioTuner == null)
      {
        this.LogDebug("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
        int hr = graphBuilder.Connect(tuner.AudioPin, crossbar.AudioTunerIn);
        if (hr != 0)
        {
          this.LogError("analog: unable to add TvAudioTuner to graph - even TvTuner as TvAudio fails");
          _variant = TvAudioVariant.Unavailable;
        }
        else
        {
          this.LogDebug("analog: AddTvAudioFilter connected TvTuner with Crossbar directly succeeded!");
          _variant = TvAudioVariant.TvTunerConnection;
          _tvAudioTunerInterface = tuner.Filter as IAMTVAudio;
          if (_tvAudioTunerInterface != null)
          {
            this.LogDebug("analog: AddTvAudioFilter succeeded - TvTuner is also TvAudio");
            _filterTvAudioTuner = tuner.Filter;
            _variant = TvAudioVariant.TvTuner;
          }
        }
        graph.TvAudio.Mode = _variant;
      }
      else
      {
        _variant = TvAudioVariant.Normal;
        graph.TvAudio.Name = _audioDevice.Name;
      }
      if (_variant != TvAudioVariant.Unavailable && _variant != TvAudioVariant.TvTunerConnection &&
          _tvAudioTunerInterface != null)
      {
        _tvAudioTunerInterface.GetHardwareSupportedTVAudioModes(out _hardwareSupportedModes);
        graph.TvAudio.AudioModes = _hardwareSupportedModes;
      }
      return true;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Gets the available audio modes.
    /// </summary>
    /// <returns>List of available audio modes.</returns>
    public List<TVAudioMode> GetAvailableAudioModes()
    {
      List<TVAudioMode> availableStreams = new List<TVAudioMode>();
      if (_filterTvAudioTuner != null)
      {
        TVAudioMode availableAudioModes;
        _tvAudioTunerInterface.GetAvailableTVAudioModes(out availableAudioModes);
        availableAudioModes = availableAudioModes & _hardwareSupportedModes;

        if ((availableAudioModes & TVAudioMode.Stereo) != 0)
        {
          availableStreams.Add(TVAudioMode.Stereo);
        }
        if ((availableAudioModes & TVAudioMode.Mono) != 0)
        {
          availableStreams.Add(TVAudioMode.Mono);
        }
        if ((availableAudioModes & TVAudioMode.LangA) != 0)
        {
          availableStreams.Add(TVAudioMode.LangA);
        }
        if ((availableAudioModes & TVAudioMode.LangB) != 0)
        {
          availableStreams.Add(TVAudioMode.LangB);
        }
        if ((availableAudioModes & TVAudioMode.LangC) != 0)
        {
          availableStreams.Add(TVAudioMode.LangC);
        }
      }
      return availableStreams;
    }

    /// <summary>
    /// get/set the audio mode
    /// </summary>
    public TVAudioMode CurrentAudioMode
    {
      get
      {
        if (_filterTvAudioTuner == null)
        {
          return TVAudioMode.None;
        }
        TVAudioMode currentMode;
        _tvAudioTunerInterface.get_TVAudioMode(out currentMode);
        return currentMode;
      }
      set
      {
        if (_filterTvAudioTuner != null && (value & _hardwareSupportedModes) != 0)
        {
          IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
          if (tvAudioTunerInterface != null)
          {
            tvAudioTunerInterface.put_TVAudioMode(value);
          }
        }
      }
    }

    #endregion
  }
}