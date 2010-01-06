#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - diehard2
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.Analog.Components
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
    /// The mode of the TvAudio component in the graph
    /// </summary>
    private TvAudioVariant mode;

    /// <summary>
    /// List of available streams
    /// </summary>
    private List<IAudioStream> streams;

    #endregion

    #region properties

    public String TvAudioName
    {
      get { return _audioDevice.Name; }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Disposes the TvAudio component
    /// </summary>
    public void Dispose()
    {
      if (mode == TvAudioVariant.Normal)
      {
        if (_filterTvAudioTuner != null)
        {
          while (Marshal.ReleaseComObject(_filterTvAudioTuner) > 0) {}
          _filterTvAudioTuner = null;
        }
        if (_audioDevice != null)
        {
          DevicesInUse.Instance.Remove(_audioDevice);
          _audioDevice = null;
        }
      }
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
        Log.Log.WriteFile("analog: Using TvAudio configuration from stored graph");
        if (CreateConfigurationBasedFilterInstance(graph, tuner, crossbar, graphBuilder))
        {
          Log.Log.WriteFile("analog: Using TvAudio configuration from stored graph succeeded");
          return true;
        }
      }
      if (tuner.AudioPin == null)
      {
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio device needed!");
        mode = TvAudioVariant.Unavailable;
        return true;
      }
      Log.Log.WriteFile("analog: No stored graph for TvAudio component - Trying to detect");
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
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
        int hr = graphBuilder.Connect(tuner.AudioPin, crossbar.AudioTunerIn);
        if (hr < 0)
        {
          Log.Log.Error("analog: unable to add TvAudioTuner to graph - even TvTuner as TvAudio fails");
          return false;
        }
        if (graph.TvAudio.Mode == TvAudioVariant.TvTuner)
        {
          Log.Log.WriteFile("analog: AddTvAudioFilter connected TvTuner with Crossbar directly succeeded!");
          _tvAudioTunerInterface = tuner.Filter as IAMTVAudio;
          if (_tvAudioTunerInterface != null)
          {
            Log.Log.WriteFile("analog: AddTvAudioFilter succeeded - TvTuner is also TvAudio");
            _filterTvAudioTuner = tuner.Filter;
            mode = TvAudioVariant.TvTuner;
            streams = new List<IAudioStream>();
            CheckCapabilities(graph);
            return true;
          }
        }
        return false;
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
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
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
          Log.Log.WriteFile("analog: AddTvAudioFilter use:{0} {1}", devices[i].Name, i);
          int hr;
          try
          {
            //add tv audio tuner to graph
            hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            Log.Log.WriteFile("analog: cannot add filter to graph");
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
              Log.Log.WriteFile("analog: AddTvAudioFilter succeeded:{0}", devices[i].Name);
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
      mode = TvAudioVariant.Normal;
      streams = new List<IAudioStream>();
      CheckCapabilities(graph);
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
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
      }
      if (devices != null && devices.Length > 0)
      {
        // try each tv audio tuner
        for (int i = 0; i < devices.Length; i++)
        {
          IBaseFilter tmp;
          Log.Log.WriteFile("analog: AddTvAudioFilter try:{0} {1}", devices[i].Name, i);
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
            Log.Log.WriteFile("analog: cannot add filter to graph");
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
              Log.Log.WriteFile("analog: AddTvAudioFilter succeeded:{0}", devices[i].Name);
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
        Log.Log.WriteFile("analog: AddTvAudioFilter no tv audio devices found - Trying TvTuner filter");
        int hr = graphBuilder.Connect(tuner.AudioPin, crossbar.AudioTunerIn);
        if (hr < 0)
        {
          Log.Log.Error("analog: unable to add TvAudioTuner to graph - even TvTuner as TvAudio fails");
          return false;
        }
        Log.Log.WriteFile("analog: AddTvAudioFilter connected TvTuner with Crossbar directly succeeded!");
        _tvAudioTunerInterface = tuner.Filter as IAMTVAudio;
        mode = TvAudioVariant.TvTunerConnection;
        if (_tvAudioTunerInterface != null)
        {
          Log.Log.WriteFile("analog: AddTvAudioFilter succeeded - TvTuner is also TvAudio");
          _filterTvAudioTuner = tuner.Filter;
          mode = TvAudioVariant.TvTuner;
        }
        else
        {
          mode = TvAudioVariant.Unavailable;
        }
        graph.TvAudio.Mode = mode;
      }
      else
      {
        mode = TvAudioVariant.Normal;
        graph.TvAudio.Name = _audioDevice.Name;
      }
      if (mode != TvAudioVariant.Unavailable && mode != TvAudioVariant.TvTunerConnection &&
          _tvAudioTunerInterface != null)
      {
        streams = new List<IAudioStream>();
        CheckCapabilities(graph);
      }
      return true;
    }

    /// <summary>
    /// Detects the capabilities of the tv audio device
    /// </summary>
    private void CheckCapabilities(Graph graph)
    {
      TVAudioMode availableAudioModes;
      _tvAudioTunerInterface.GetHardwareSupportedTVAudioModes(out availableAudioModes);
      graph.TvAudio.AudioModes = availableAudioModes;
      if ((availableAudioModes & (TVAudioMode.Stereo)) != 0)
      {
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.Stereo;
        stream.Language = "Stereo";
        streams.Add(stream);
      }
      if ((availableAudioModes & (TVAudioMode.Mono)) != 0)
      {
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.Mono;
        stream.Language = "Mono";
        streams.Add(stream);
      }
      if ((availableAudioModes & (TVAudioMode.LangA)) != 0)
      {
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.LangA;
        stream.Language = "LangA";
        streams.Add(stream);
      }
      if ((availableAudioModes & (TVAudioMode.LangB)) != 0)
      {
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.LangB;
        stream.Language = "LangB";
        streams.Add(stream);
      }
      if ((availableAudioModes & (TVAudioMode.LangC)) != 0)
      {
        AnalogAudioStream stream = new AnalogAudioStream();
        stream.AudioMode = TVAudioMode.LangC;
        stream.Language = "LangC";
        streams.Add(stream);
      }
    }

    #endregion

    #region public methods

    /// <summary>
    /// Gets the available audio streams
    /// </summary>
    /// <returns>List of available audio streams</returns>
    public List<IAudioStream> GetAvailableAudioStreams()
    {
      List<IAudioStream> availableStreams = new List<IAudioStream>();
      TVAudioMode availableAudioModes;
      _tvAudioTunerInterface.GetHardwareSupportedTVAudioModes(out availableAudioModes);
      foreach (AnalogAudioStream stream in streams)
      {
        if ((stream.AudioMode & availableAudioModes) != 0)
        {
          availableStreams.Add(stream);
        }
      }
      return availableStreams;
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get
      {
        if (_filterTvAudioTuner == null)
          return null;
        TVAudioMode currentMode;
        _tvAudioTunerInterface.get_TVAudioMode(out currentMode);
        List<IAudioStream> availableStreams = GetAvailableAudioStreams();
        foreach (AnalogAudioStream stream in availableStreams)
        {
          if (stream.AudioMode == currentMode)
            return stream;
        }
        return null;
      }
      set
      {
        AnalogAudioStream stream = value as AnalogAudioStream;
        if (stream != null && _filterTvAudioTuner != null)
        {
          IAMTVAudio tvAudioTunerInterface = _filterTvAudioTuner as IAMTVAudio;
          if (tvAudioTunerInterface != null)
            tvAudioTunerInterface.put_TVAudioMode(stream.AudioMode);
        }
      }
    }

    #endregion
  }
}