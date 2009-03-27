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
using System.Runtime.InteropServices;
using DirectShowLib;
using Microsoft.Win32;
using TvLibrary.Implementations.Analog.GraphComponents;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The tuner component of the graph
  /// </summary>
  internal class Tuner : IDisposable
  {
    #region variables
    /// <summary>
    /// The tuner device
    /// </summary>
    private readonly DsDevice _tunerDevice;
    /// <summary>
    /// The tuner filter
    /// </summary>
    private IBaseFilter _filterTvTuner;
    /// <summary>
    /// The current minimum channel number
    /// </summary>
    private int _minChannel;
    /// <summary>
    /// The current maximum channel number
    /// </summary>
    private int _maxChannel;
    /// <summary>
    /// Indicates if the tuner supports FM radio
    /// </summary>
    private bool _supportsFMRadio;
    /// <summary>
    /// Indicates if the tuner supports AM radio
    /// </summary>
    private bool _supportsAMRadio;
    /// <summary>
    /// The current analog channel
    /// </summary>
    private AnalogChannel _currentChannel;
    /// <summary>
    /// The tuner interface
    /// </summary>
    private IAMTVTuner _tuner;
    /// <summary>
    /// The current video frequency
    /// </summary>
    private int _videoFrequency;
    /// <summary>
    /// The current audio frequency
    /// </summary>
    private int _audioFrequency;
    /// <summary>
    /// Indicates if the tuner is locked
    /// </summary>
    private bool _tunerLocked;
    /// <summary>
    /// The current signal level
    /// </summary>
    private int _signalLevel;
    /// <summary>
    /// The current signal quality
    /// </summary>
    private int _signalQuality;
    /// <summary>
    /// The audio output pin
    /// </summary>
    private IPin _audioPin;
    #endregion

    #region properties
    /// <summary>
    /// Gets the TvTuner filter
    /// </summary>
    public IBaseFilter Filter
    {
      get { return _filterTvTuner; }
    }

    /// <summary>
    /// Gets the audio pin
    /// </summary>
    public IPin AudioPin
    {
      get { return _audioPin; }
    }

    /// <summary>
    /// Gets the tuner device Name
    /// </summary>
    public String TunerName
    {
      get { return _tunerDevice.Name; }
    }

    /// <summary>
    /// Gets if the tuner is locked
    /// </summary>
    public bool TunerLocked
    {
      get { return _tunerLocked; }
    }

    /// <summary>
    /// Gets the current video frequency
    /// </summary>
    public int VideoFrequency
    {
      get { return _videoFrequency; }
    }

    /// <summary>
    /// Gets the current audio frequency
    /// </summary>
    public int AudioFrequency
    {
      get { return _audioFrequency; }
    }

    /// <summary>
    /// Gets the current signal quality
    /// </summary>
    public int SignalQuality
    {
      get { return _signalQuality; }
    }

    /// <summary>
    /// Gets the current signal level
    /// </summary>
    public int SignalLevel
    {
      get { return _signalLevel; }
    }

    /// <summary>
    /// Gets if the tuner supports AM radio
    /// </summary>
    public bool SupportsAMRadio
    {
      get { return _supportsAMRadio; }
    }

    /// <summary>
    /// Gets if the tuner supports FM radio
    /// </summary>
    public bool SupportsFMRadio
    {
      get { return _supportsFMRadio; }
    }

    /// <summary>
    /// Gets the current maximum channel number
    /// </summary>
    public int MaxChannel
    {
      get { return _maxChannel; }
    }

    /// <summary>
    /// Gets the current minimum channel number
    /// </summary>
    public int MinChannel
    {
      get { return _minChannel; }
    }
    #endregion

    #region ctor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tunerDevice">The tuner device</param>
    public Tuner(DsDevice tunerDevice)
    {
      _tunerDevice = tunerDevice;
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Diposes the tuner component
    /// </summary>
    public void Dispose()
    {
      _tuner = null;
      if (_audioPin != null)
      {
        Release.ComObject("_audioPin", _audioPin);
      }
      if (_filterTvTuner != null)
      {
        while (Marshal.ReleaseComObject(_filterTvTuner) > 0)
        {
        }
        _filterTvTuner = null;
      }
      DevicesInUse.Instance.Remove(_tunerDevice);
    }
    #endregion

    #region CreateFilterInstance method
    /// <summary>
    /// Creates the tuner filter instance
    /// </summary>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphbuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    public bool CreateFilterInstance(Graph graph, IFilterGraph2 graphBuilder)
    {
      Log.Log.WriteFile("analog: AddTvTunerFilter {0}", _tunerDevice.Name);
      if (DevicesInUse.Instance.IsUsed(_tunerDevice))
        return false;
      IBaseFilter tmp;
      int hr;
      try
      {
        hr = graphBuilder.AddSourceFilterForMoniker(_tunerDevice.Mon, null, _tunerDevice.Name, out tmp);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: cannot add filter to graph");
        return false;
      }
      if (hr != 0)
      {
        Log.Log.Error("analog: AddTvTunerFilter failed:0x{0:X}", hr);
        throw new TvException("Unable to add tvtuner to graph");
      }
      _filterTvTuner = tmp;
      DevicesInUse.Instance.Add(_tunerDevice);
      _tuner = _filterTvTuner as IAMTVTuner;
      if (string.IsNullOrEmpty(graph.Tuner.Name) || !_tunerDevice.Name.Equals(
        graph.Tuner.Name))
      {
        Log.Log.WriteFile("analog: Detecting capabilities of the tuner");
        graph.Tuner.Name = _tunerDevice.Name;
        int index;
        _audioPin = FilterGraphTools.FindMediaPin(_filterTvTuner, MediaType.AnalogAudio, MediaSubType.Null, PinDirection.Output, out index);
        graph.Tuner.AudioPin = index;
        return CheckCapabilities(graph);
      }
      Log.Log.WriteFile("analog: Using stored capabilities of the tuner");
      _audioPin = DsFindPin.ByDirection(_filterTvTuner, PinDirection.Output, graph.Tuner.AudioPin);
      _supportsFMRadio = (graph.Tuner.RadioMode & RadioMode.FM) != 0;
      _supportsAMRadio = (graph.Tuner.RadioMode & RadioMode.AM) != 0;
      return true;
    }
    #endregion

    #region private helper methods
    /// <summary>
    /// Stores a frequency override in the registry
    /// </summary>
    /// <param name="channel">Channel with frequency override</param>
    private static void SetFrequencyOverride(AnalogChannel channel)
    {
      int countryCode = channel.Country.Id;
      string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")};
      if (channel.Frequency == 0)
      {
        //remove the frequency override in 
        for (int index = 0; index < registryLocations.Length; index++)
        {
          using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
          {
            if (registryKey != null)
              registryKey.DeleteValue(channel.ChannelNumber.ToString(), false);
          }
          using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(registryLocations[index]))
          {
            if (registryKey != null)
              registryKey.DeleteValue(channel.ChannelNumber.ToString(), false);
          }
        }
        return;
      }
      //set frequency override
      for (int index = 0; index < registryLocations.Length; index++)
      {
        using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(registryLocations[index]))
        {
          if (registryKey != null)
            registryKey.SetValue(channel.ChannelNumber.ToString(), (int)channel.Frequency);
        }
        using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(registryLocations[index]))
        {
          if (registryKey != null)
            registryKey.SetValue(channel.ChannelNumber.ToString(), (int)channel.Frequency);
        }
      }
    }

    /// <summary>
    /// Checks the capabilities of the tuner device
    /// </summary>
    /// <returns>true, if the checks were successful</returns>
    private bool CheckCapabilities(Graph graph)
    {
      if (_tuner == null)
      {
        return false;
      }
      UpdateMinMaxChannel();

      AMTunerModeType tunerModes;
      _tuner.GetAvailableModes(out tunerModes);
      _supportsFMRadio = (AMTunerModeType.FMRadio & tunerModes) != 0;
      _supportsAMRadio = (AMTunerModeType.AMRadio & tunerModes) != 0;
      if (_supportsFMRadio)
      {
        graph.Tuner.RadioMode = graph.Tuner.RadioMode | RadioMode.AM;
      }
      if (_supportsAMRadio)
      {
        graph.Tuner.RadioMode = graph.Tuner.RadioMode | RadioMode.AM;
      }

      return true;
    }

    /// <summary>
    /// Updates the min and max channels
    /// </summary>
    private void UpdateMinMaxChannel()
    {
      if (_tuner != null)
      {
        _tuner.ChannelMinMax(out _minChannel, out _maxChannel);
      } else
      {
        _minChannel = 0;
        _maxChannel = 128;
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Indicates if it is a special plextor card
    /// </summary>
    /// <returns>true, if it is a special plextor card</returns>
    public bool IsPlextorCard()
    {
      return FilterGraphTools.GetFilterName(_filterTvTuner).Contains("Plextor ConvertX");
    }

    /// <summary>
    /// Indicates if it is a special Nvidia nvtv card
    /// </summary>
    /// <returns>true, if it is a special nvidia card</returns>
    public bool IsNvidiaCard()
    {
      return FilterGraphTools.GetFilterName(_filterTvTuner).Contains("NVTV");
    }

    /// <summary>
    /// Updates the signal quality
    /// </summary>
    public void UpdateSignalQuality()
    {
      _tunerLocked = false;
      _signalLevel = 0;
      _signalQuality = 0;
      AMTunerSignalStrength signalStrength;
      _tuner.SignalPresent(out signalStrength);
      _tunerLocked = (signalStrength == AMTunerSignalStrength.SignalPresent || signalStrength == AMTunerSignalStrength.HasNoSignalStrength);

      if (_tunerLocked)
      {
        _signalLevel = 100;
        _signalQuality = 100;
      } else
      {
        _signalLevel = 0;
        _signalQuality = 0;
      }
    }

    /// <summary>
    /// Performs a tuning to the given channel
    /// </summary>
    /// <param name="analogChannel">The channel to tune to</param>
    public void PerformTune(AnalogChannel analogChannel)
    {
      if (_tuner == null || analogChannel == null)
      {
        throw new NullReferenceException();
      }
      if (analogChannel.IsTv)
      {
        SetFrequencyOverride(analogChannel);
      }
      if (_currentChannel != null)
      {
        if (analogChannel.IsRadio != _currentChannel.IsRadio)
        {
          if (analogChannel.IsRadio)
          {
            Log.Log.WriteFile("analog:  set to FM radio");
            _tuner.put_Mode(AMTunerModeType.FMRadio);
          } else
          {
            Log.Log.WriteFile("analog:  set to TV");
            _tuner.put_Mode(AMTunerModeType.TV);
          }
        }
        if (analogChannel.Country.Id != _currentChannel.Country.Id)
        {
          _tuner.put_TuningSpace(analogChannel.Country.Id);
          _tuner.put_CountryCode(analogChannel.Country.Id);
        }
        if (analogChannel.TunerSource != _currentChannel.TunerSource)
        {
          _tuner.put_InputType(0, analogChannel.TunerSource);
        }
        if (analogChannel.IsRadio)
        {
          if (analogChannel.Frequency != _currentChannel.Frequency)
          {
            _tuner.put_Channel((int)analogChannel.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
          }
        } else
        {
          if (analogChannel.ChannelNumber != _currentChannel.ChannelNumber)
          {
            _tuner.put_Channel(analogChannel.ChannelNumber, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
          }
        }
      } else
      {
        if (analogChannel.IsRadio)
        {
          Log.Log.WriteFile("analog:  set to FM radio");
          _tuner.put_Mode(AMTunerModeType.FMRadio);
        } else
        {
          Log.Log.WriteFile("analog:  set to TV");
          _tuner.put_Mode(AMTunerModeType.TV);
        }
        _tuner.put_TuningSpace(analogChannel.Country.Id);
        _tuner.put_CountryCode(analogChannel.Country.Id);
        _tuner.put_InputType(0, analogChannel.TunerSource);
        if (analogChannel.IsRadio)
        {
          _tuner.put_Channel((int)analogChannel.Frequency, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
        } else
        {
          _tuner.put_Channel(analogChannel.ChannelNumber, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
        }
      }
      _tuner.get_VideoFrequency(out _videoFrequency);
      _tuner.get_AudioFrequency(out _audioFrequency);
      _tunerLocked = false;
      _currentChannel = analogChannel;
      UpdateSignalQuality();
      UpdateMinMaxChannel();
      Log.Log.WriteFile("Analog: Tuned to country:{0} video:{1} Hz audio:{2} Hz locked:{3}", analogChannel.Country.Id, _videoFrequency, _audioFrequency, _tunerLocked);
    }
    #endregion

  }
}
