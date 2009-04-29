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
using DirectShowLib;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The crossbar component of the graph
  /// </summary>
  internal class Crossbar : IDisposable
  {
    #region variables
    /// <summary>
    /// The crossbar filter
    /// </summary>
    private IBaseFilter _filterCrossBar;
    /// <summary>
    /// The crossbar device
    /// </summary>
    private DsDevice _crossBarDevice;
    /// <summary>
    /// The crossbar interface
    /// </summary>
    private IAMCrossbar _crossBarFilter;
    /// <summary>
    /// The mapping of the video input sources to their pin index
    /// </summary>
    private Dictionary<AnalogChannel.VideoInputType, int> _videoPinMap;
    /// <summary>
    /// The mapping of the video input sources to their related audio pin index
    /// </summary>
    private Dictionary<AnalogChannel.VideoInputType, int> _videoPinRelatedAudioMap;
    /// <summary>
    /// The mapping of the audio input sources to their pin index
    /// </summary>
    private Dictionary<AnalogChannel.AudioInputType, int> _audioPinMap;
    /// <summary>
    /// The audio tuner in pin
    /// </summary>
    private IPin _audioTunerIn;
    /// <summary>
    /// The video output pin index 
    /// </summary>
    private int _videoOutPinIndex;
    /// <summary>
    /// The audio output pin index
    /// </summary>
    private int _audioOutPinIndex;
    /// <summary>
    /// The video output pin
    /// </summary>
    private IPin _videoOut;
    /// <summary>
    /// The audio output pin
    /// </summary>
    private IPin _audioOut;
    /// <summary>
    /// The current analog channel
    /// </summary>
    private AnalogChannel _currentChannel;
    #endregion

    #region properties
    /// <summary>
    /// Gets the video output pin
    /// </summary>
    public IPin VideoOut
    {
      get { return _videoOut; }
    }

    /// <summary>
    /// Gets the audio output pin
    /// </summary>
    public IPin AudioOut
    {
      get { return _audioOut; }
    }
    
    /// <summary>
    /// Gets the name of the crossbar device
    /// </summary>
    public String CrossBarName
    {
      get { return _crossBarDevice.Name; }
    }

    /// <summary>
    /// Gets the crossbar filter
    /// </summary>
    public IBaseFilter Filter
    {
      get { return _filterCrossBar; }
    }

    /// <summary>
    /// Gets the audio tuner input pin
    /// </summary>
    public IPin AudioTunerIn
    {
      get { return _audioTunerIn; }
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Disposes the crossbar component
    /// </summary>
    public void Dispose()
    {
      if (_audioTunerIn != null)
      {
        Release.ComObject("_audioTunerIn", _audioTunerIn);
      }
      if (_videoOut != null)
      {
        Release.ComObject("_videoOut", _videoOut);
      }
      if (_audioOut != null)
      {
        Release.ComObject("_audioOut", _audioOut);
      }
      if (_filterCrossBar != null)
      {
        Release.ComObject("crossbar filter", _filterCrossBar);
        _filterCrossBar = null;
      }
      if (_crossBarDevice != null)
      {
        DevicesInUse.Instance.Remove(_crossBarDevice);
        _crossBarDevice = null;
      }
    }
    #endregion

    #region CreateFilterInstance method
    /// <summary>
    /// Adds the cross bar filter to the graph and connects the tv tuner to the crossbar.
    /// at the end of this method the graph looks like:
    /// [tv tuner]----->[crossbar]
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    public bool CreateFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      if (!string.IsNullOrEmpty(graph.Crossbar.Name))
      {
        Log.Log.WriteFile("analog: Using Crossbar configuration from stored graph");
        if (CreateConfigurationBasedFilterInstance(graph, graphBuilder, tuner))
        {
          Log.Log.WriteFile("analog: Using Crossbar configuration from stored graph succeeded");
          return true;
        }
      }
      Log.Log.WriteFile("analog: No stored or invalid graph for Crossbar component - Trying to detect");
      return CreateAutomaticFilterInstance(graph, graphBuilder, tuner);
    }
    #endregion

    #region private helper methods
    /// <summary>
    /// Creates the filter based on the configuration file
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateConfigurationBasedFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      string deviceName = graph.Crossbar.Name;
      _audioTunerIn = null;
      DsDevice[] devices;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, graph.Tuner.Name);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      if (devices == null || devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        if (!deviceName.Equals(devices[i].Name))
          continue;
        Log.Log.WriteFile("analog: AddCrossBarFilter use:{0} {1}", devices[i].Name, i);
        int hr;
        try
        {
          //add the crossbar to the graph
          hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //failed. try next crossbar
          if (tmp != null)
          {
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }
        _crossBarFilter = (IAMCrossbar)tmp;
        _videoPinMap = graph.Crossbar.VideoPinMap;
        _audioPinMap = graph.Crossbar.AudioPinMap;
        _videoPinRelatedAudioMap = graph.Crossbar.VideoPinRelatedAudioMap;
        _videoOutPinIndex = graph.Crossbar.VideoOut;
        _audioOutPinIndex = graph.Crossbar.AudioOut;
        if (_videoOutPinIndex == -1)
        {
          Log.Log.WriteFile("analog: AddCrossbarFilter no video output found");
          graphBuilder.RemoveFilter(tmp);
          _crossBarFilter = null;
          Release.ComObject("CrossBarFilter", tmp);
          continue;
        }
        //connect tv tuner->crossbar
        IPin tunerOut = DsFindPin.ByDirection(tuner.Filter, PinDirection.Output,
                                                  graph.Tuner.VideoPin);
        if (tunerOut != null && FilterGraphTools.ConnectPin(graphBuilder,tunerOut,tmp,_videoPinMap[AnalogChannel.VideoInputType.Tuner]))
        {
          // Got it, we're done
          _filterCrossBar = tmp;
          _crossBarDevice = devices[i];
          DevicesInUse.Instance.Add(_crossBarDevice);
          if (_audioTunerIn == null)
          {
            _audioTunerIn = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Input,
                                                  _audioPinMap[AnalogChannel.AudioInputType.Tuner]);
          }
          Release.ComObject("tuner video out", tunerOut);
          _videoOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _videoOutPinIndex);
          if (_audioOutPinIndex != -1)
          {
            _audioOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _audioOutPinIndex);
          }
          Log.Log.WriteFile("analog: AddCrossBarFilter succeeded");
          break;
        }
        // cannot connect tv tuner to crossbar, try next crossbar device
        if (tmp != null)
        {
          graphBuilder.RemoveFilter(tmp);
          Release.ComObject("crossbarFilter filter", tmp);
        }
        if (tunerOut != null)
        {
          Release.ComObject("tuner video out", tunerOut);
        }
      }
      return _filterCrossBar != null;
    }

    /// <summary>
    /// Creates the filter by trying to detect it
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateAutomaticFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      _audioTunerIn = null;
      DsDevice[] devices;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, graph.Tuner.Name);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      if (devices == null || devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        Log.Log.WriteFile("analog: AddCrossBarFilter try:{0} {1}", devices[i].Name, i);
        //if crossbar is already in use then we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          //add the crossbar to the graph
          hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //failed. try next crossbar
          if (tmp != null)
          {
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }
        _crossBarFilter = (IAMCrossbar)tmp;
        CheckCapabilities();
        if (_videoOutPinIndex == -1)
        {
          Log.Log.WriteFile("analog: AddCrossbarFilter no video output found");
          graphBuilder.RemoveFilter(tmp);
          _crossBarFilter = null;
          Release.ComObject("CrossBarFilter", tmp);
          continue;
        }
        IPin pinIn = DsFindPin.ByDirection(tmp, PinDirection.Input, _videoPinMap[AnalogChannel.VideoInputType.Tuner]);
        if (pinIn == null)
        {
          // no pin found, continue with next crossbar
          Log.Log.WriteFile("analog: AddCrossBarFilter no video tuner input pin detected");
          if (tmp != null)
          {
            graphBuilder.RemoveFilter(tmp);
            _crossBarFilter = null;
            Release.ComObject("CrossBarFilter", tmp);
          }
          continue;
        }
        //connect tv tuner->crossbar
        int tempVideoPinIndex;
        if (FilterGraphTools.ConnectFilter(graphBuilder, tuner.Filter, pinIn, out tempVideoPinIndex))
        {
          // Got it, we're done
          _filterCrossBar = tmp;
          _crossBarDevice = devices[i];
          DevicesInUse.Instance.Add(_crossBarDevice);
          if (_audioTunerIn == null)
          {
            _audioTunerIn = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Input,
                                                  _audioPinMap[AnalogChannel.AudioInputType.Tuner]);
          }
          Release.ComObject("crossbar videotuner pin", pinIn);
          _videoOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _videoOutPinIndex);
          if (_audioOutPinIndex != -1)
          {
            _audioOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _audioOutPinIndex);
          }
          Log.Log.WriteFile("analog: AddCrossBarFilter succeeded");
          graph.Crossbar.AudioOut = _audioOutPinIndex;
          graph.Crossbar.AudioPinMap = _audioPinMap;
          graph.Crossbar.Name = _crossBarDevice.Name;
          graph.Crossbar.VideoOut = _videoOutPinIndex;
          graph.Crossbar.VideoPinMap = _videoPinMap;
          graph.Crossbar.VideoPinRelatedAudioMap = _videoPinRelatedAudioMap;
          graph.Tuner.VideoPin = tempVideoPinIndex;
          break;
        }
        // cannot connect tv tuner to crossbar, try next crossbar device
        graphBuilder.RemoveFilter(tmp);
        Release.ComObject("crossbar videotuner pin", pinIn);
        Release.ComObject("crossbar filter", tmp);
      }
      return _filterCrossBar != null;
    }


    /// <summary>
    /// Checks the capabilities
    /// </summary>
    private void CheckCapabilities()
    {
      int outputs, inputs;
      _crossBarFilter.get_PinCounts(out outputs, out inputs);
      _videoOutPinIndex = -1;
      _audioOutPinIndex = -1;
      _videoPinMap = new Dictionary<AnalogChannel.VideoInputType, int>();
      _audioPinMap = new Dictionary<AnalogChannel.AudioInputType, int>();
      _videoPinRelatedAudioMap = new Dictionary<AnalogChannel.VideoInputType, int>();
      int relatedPinIndex;
      PhysicalConnectorType connectorType;
      for (int i = 0; i < outputs; ++i)
      {
        _crossBarFilter.get_CrossbarPinInfo(false, i, out relatedPinIndex, out connectorType);
        if (connectorType == PhysicalConnectorType.Video_VideoDecoder)
        {
          _videoOutPinIndex = i;
        }
        if (connectorType == PhysicalConnectorType.Audio_AudioDecoder)
        {
          _audioOutPinIndex = i;
        }
      }

      int audioLine = 0;
      int audioSPDIF = 0;
      int audioAux = 0;
      int videoCvbsNr = 0;
      int videoSvhsNr = 0;
      int videoYrYbYNr = 0;
      int videoRgbNr = 0;
      for (int i = 0; i < inputs; ++i)
      {
        _crossBarFilter.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
        Log.Log.Write(" crossbar pin:{0} type:{1}", i, connectorType);
        switch (connectorType)
        {
          case PhysicalConnectorType.Audio_Tuner:
            _audioPinMap.Add(AnalogChannel.AudioInputType.Tuner, i);
            break;
          case PhysicalConnectorType.Video_Tuner:
            _videoPinMap.Add(AnalogChannel.VideoInputType.Tuner, i);
            _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.Tuner, relatedPinIndex);
            break;
          case PhysicalConnectorType.Audio_Line:
            audioLine++;
            switch (audioLine)
            {
              case 1:
                _audioPinMap.Add(AnalogChannel.AudioInputType.LineInput1, i);
                break;
              case 2:
                _audioPinMap.Add(AnalogChannel.AudioInputType.LineInput2, i);
                break;
              case 3:
                _audioPinMap.Add(AnalogChannel.AudioInputType.LineInput3, i);
                break;
            }
            break;
          case PhysicalConnectorType.Audio_SPDIFDigital:
            audioSPDIF++;
            switch (audioSPDIF)
            {
              case 1:
                _audioPinMap.Add(AnalogChannel.AudioInputType.SPDIFInput1, i);
                break;
              case 2:
                _audioPinMap.Add(AnalogChannel.AudioInputType.SPDIFInput2, i);
                break;
              case 3:
                _audioPinMap.Add(AnalogChannel.AudioInputType.SPDIFInput3, i);
                break;
            }
            break;
          case PhysicalConnectorType.Audio_AUX:
            audioAux++;
            switch (audioAux)
            {
              case 1:
                _audioPinMap.Add(AnalogChannel.AudioInputType.AUXInput1, i);
                break;
              case 2:
                _audioPinMap.Add(AnalogChannel.AudioInputType.AUXInput2, i);
                break;
              case 3:
                _audioPinMap.Add(AnalogChannel.AudioInputType.AUXInput3, i);
                break;
            }
            break;
          case PhysicalConnectorType.Video_Composite:
            videoCvbsNr++;
            switch (videoCvbsNr)
            {
              case 1:
                _videoPinMap.Add(AnalogChannel.VideoInputType.VideoInput1, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.VideoInput1, relatedPinIndex);
                break;
              case 2:
                _videoPinMap.Add(AnalogChannel.VideoInputType.VideoInput2, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.VideoInput2, relatedPinIndex);
                break;
              case 3:
                _videoPinMap.Add(AnalogChannel.VideoInputType.VideoInput3, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.VideoInput3, relatedPinIndex);
                break;
            }
            break;
          case PhysicalConnectorType.Video_SVideo:
            videoSvhsNr++;
            switch (videoSvhsNr)
            {
              case 1:
                _videoPinMap.Add(AnalogChannel.VideoInputType.SvhsInput1, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.SvhsInput1, relatedPinIndex);
                break;
              case 2:
                _videoPinMap.Add(AnalogChannel.VideoInputType.SvhsInput2, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.SvhsInput2, relatedPinIndex);
                break;
              case 3:
                _videoPinMap.Add(AnalogChannel.VideoInputType.VideoInput3, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.VideoInput3, relatedPinIndex);
                break;
            }
            break;
          case PhysicalConnectorType.Video_RGB:
            videoRgbNr++;
            switch (videoRgbNr)
            {
              case 1:
                _videoPinMap.Add(AnalogChannel.VideoInputType.RgbInput1, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.RgbInput1, relatedPinIndex);
                break;
              case 2:
                _videoPinMap.Add(AnalogChannel.VideoInputType.RgbInput2, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.RgbInput2, relatedPinIndex);
                break;
              case 3:
                _videoPinMap.Add(AnalogChannel.VideoInputType.SvhsInput3, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.SvhsInput3, relatedPinIndex);
                break;
            }
            break;
          case PhysicalConnectorType.Video_YRYBY:
            videoYrYbYNr++;
            switch (videoYrYbYNr)
            {
              case 1:
                _videoPinMap.Add(AnalogChannel.VideoInputType.YRYBYInput1, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.YRYBYInput1, relatedPinIndex);
                break;
              case 2:
                _videoPinMap.Add(AnalogChannel.VideoInputType.YRYBYInput2, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.YRYBYInput2, relatedPinIndex);
                break;
              case 3:
                _videoPinMap.Add(AnalogChannel.VideoInputType.YRYBYInput3, i);
                _videoPinRelatedAudioMap.Add(AnalogChannel.VideoInputType.YRYBYInput3, relatedPinIndex);
                break;
            }
            break;
        }
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Indicates if it is a special plextor card
    /// </summary>
    /// <returns>true, if it is a special plextor card</returns>
    public void PerformTune(AnalogChannel channel)
    {
      if (_currentChannel != null)
      {
        if(_currentChannel.VideoSource != channel.VideoSource && _videoPinMap.ContainsKey(channel.VideoSource))
        {
          _crossBarFilter.Route(_videoOutPinIndex, _videoPinMap[channel.VideoSource]);
        }
        if(_currentChannel.AudioSource!= channel.AudioSource)
        {
          if (channel.AudioSource == AnalogChannel.AudioInputType.Automatic && _videoPinRelatedAudioMap.ContainsKey(channel.VideoSource))
          {
            _crossBarFilter.Route(_audioOutPinIndex, _videoPinRelatedAudioMap[channel.VideoSource]);
          } else if (_audioPinMap.ContainsKey(channel.AudioSource))
          {
            _crossBarFilter.Route(_audioOutPinIndex, _audioPinMap[channel.AudioSource]);
          }
        }
      } else
      {
        _crossBarFilter.Route(_videoOutPinIndex, _videoPinMap[channel.VideoSource]);
        if (_audioOutPinIndex == -1)
        {
          return;
        }
        if (channel.AudioSource == AnalogChannel.AudioInputType.Automatic)
        {
          _crossBarFilter.Route(_audioOutPinIndex, _videoPinRelatedAudioMap[channel.VideoSource]);
        }
        else
        {
          _crossBarFilter.Route(_audioOutPinIndex, _audioPinMap[channel.AudioSource]);
        }
      }
      _currentChannel = channel;
    }
    #endregion
  }
}
