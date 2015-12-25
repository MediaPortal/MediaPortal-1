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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftEncoder
{
  /// <summary>
  /// A class that implements encoder control using the Microsoft ICodecAPI, IVideoEncoder and
  /// IEncoderAPI interfaces.
  /// </summary>
  public class MicrosoftEncoder : BaseTunerExtension, IDisposable, IEncoder
  {
    #region delegates

    /// <summary>
    /// Determine whether the encoder can manipulate a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <returns>an HRESULT indicating whether the parameter can be manipulated</returns>
    private delegate int IsParameterSupportedDelegate(Guid parameterId);

    /// <summary>
    /// Get the extents and resolution for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="minimum">The minimum value that the parameter may take.</param>
    /// <param name="maximum">The maximum value that the parameter may take.</param>
    /// <param name="resolution">The magnitude of the smallest adjustment that can be applied to
    ///   the parameter. In most cases the value of the parameter should be a multiple of th.</param>
    /// <returns>an HRESULT indicating whether the parameter extents and resolution were successfully retrieved</returns>
    private delegate int GetParameterRangeDelegate(Guid parameterId, out object minimum, out object maximum, out object resolution);

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="valuesPtr">A pointer to a buffer containing the possible values that the parameter may take.</param>
    /// <param name="valuesCount">The number of values in the buffer.</param>
    /// <returns>an HRESULT indicating whether the parameter values were successfully retrieved</returns>
    private delegate int GetParameterValuesDelegate(Guid parameterId, out IntPtr valuesPtr, out int valuesCount);

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns>an HRESULT indicating whether the default parameter value was successfully retrieved</returns>
    private delegate int GetParameterDefaultValueDelegate(Guid parameterId, out object value);

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns>an HRESULT indicating whether the current parameter value was successfully retrieved</returns>
    private delegate int GetParameterValueDelegate(Guid parameterId, out object value);

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns>an HRESULT indicating whether the parameter value was successfully set</returns>
    private delegate int SetParameterValueDelegate(Guid parameterId, ref object value);

    #endregion

    #region variables

    private bool _isMicrosoftEncoder = false;
    private List<ICodecAPI> _interfacesCodecApi = new List<ICodecAPI>(4);
    private List<IVideoEncoder> _interfacesVideoEncoder = new List<IVideoEncoder>(4);
    // Disable obsolete interface warning. Some implementations of
    // IEncoderAPI that we want to support do not implement ICodecAPI.
    #pragma warning disable 618
    private List<IEncoderAPI> _interfacesEncoderApi = new List<IEncoderAPI>(4);
    #pragma warning restore 618

    private List<IsParameterSupportedDelegate> _delegatesIsSupported = new List<IsParameterSupportedDelegate>(4);
    private List<GetParameterRangeDelegate> _delegatesGetRange = new List<GetParameterRangeDelegate>(4);
    private List<GetParameterValuesDelegate> _delegatesGetValues = new List<GetParameterValuesDelegate>(4);
    private List<GetParameterDefaultValueDelegate> _delegatesGetDefaultValue = new List<GetParameterDefaultValueDelegate>(4);
    private List<GetParameterValueDelegate> _delegatesGetValue = new List<GetParameterValueDelegate>(4);
    private List<SetParameterValueDelegate> _delegatesSetValue = new List<SetParameterValueDelegate>(4);

    #endregion

    private bool CheckInterfaces(object obj)
    {
      ICodecAPI codecApi = obj as ICodecAPI;
      if (codecApi != null)
      {
        this.LogDebug("Microsoft encoder:     found ICodecAPI interface");
        _interfacesCodecApi.Add(codecApi);
        _delegatesIsSupported.Add(codecApi.IsSupported);
        _delegatesGetRange.Add(codecApi.GetParameterRange);
        _delegatesGetValues.Add(codecApi.GetParameterValues);
        _delegatesGetDefaultValue.Add(codecApi.GetDefaultValue);
        _delegatesGetValue.Add(codecApi.GetValue);
        _delegatesSetValue.Add(codecApi.SetValue);
        return true;
      }

      // Order is important. IVideoEncoder before IEncoderAPI due to the inheritence.
      IVideoEncoder videoEncoder = obj as IVideoEncoder;
      if (videoEncoder != null)
      {
        this.LogDebug("Microsoft encoder:     found IVideoEncoder interface");
        _interfacesVideoEncoder.Add(videoEncoder);
        _delegatesIsSupported.Add(videoEncoder.IsSupported);
        _delegatesGetRange.Add(videoEncoder.GetParameterRange);
        _delegatesGetValues.Add(videoEncoder.GetParameterValues);
        _delegatesGetDefaultValue.Add(videoEncoder.GetDefaultValue);
        _delegatesGetValue.Add(videoEncoder.GetValue);
        _delegatesSetValue.Add(videoEncoder.SetValue);
        return true;
      }

      // Disable obsolete interface warning. Some implementations of
      // IEncoderAPI that we want to support do not implement ICodecAPI.
      #pragma warning disable 618
      IEncoderAPI encoderApi = obj as IEncoderAPI;
      #pragma warning restore 618
      if (encoderApi != null)
      {
        this.LogDebug("Microsoft encoder:     found IEncoderAPI interface");
        _interfacesEncoderApi.Add(encoderApi);
        _delegatesIsSupported.Add(encoderApi.IsSupported);
        _delegatesGetRange.Add(encoderApi.GetParameterRange);
        _delegatesGetValues.Add(encoderApi.GetParameterValues);
        _delegatesGetDefaultValue.Add(encoderApi.GetDefaultValue);
        _delegatesGetValue.Add(encoderApi.GetValue);
        _delegatesSetValue.Add(encoderApi.SetValue);
        return true;
      }
      return false;
    }

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft encoder";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Microsoft encoder: initialising");

      if (_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: extension already initialised");
        return true;
      }

      IBaseFilter mainFilter = context as IBaseFilter;
      if (mainFilter == null)
      {
        this.LogDebug("Microsoft encoder: context is not a filter");
        return false;
      }

      // We need a reference to the graph.
      FilterInfo filterInfo;
      int hr = mainFilter.QueryFilterInfo(out filterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft encoder: failed to get filter info, hr = 0x{0:x}", hr);
        return false;
      }
      IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
      if (graph == null)
      {
        this.LogError("Microsoft encoder: failed to get graph reference");
        return false;
      }

      this.LogDebug("Microsoft encoder: searching for supported interfaces...");
      try
      {
        IEnumFilters enumFilters;
        hr = graph.EnumFilters(out enumFilters);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft encoder: failed to get graph filter enumerator, hr = 0x{0:x}", hr);
          return false;
        }

        try
        {
          IBaseFilter[] filters = new IBaseFilter[2];
          int countFilters = 1;
          while (enumFilters.Next(1, filters, out countFilters) == (int)NativeMethods.HResult.S_OK && countFilters == 1)
          {
            IBaseFilter filter = filters[0];
            bool canReleaseFilter = true;
            string nameFilter = "Unknown";
            try
            {
              FilterInfo infoFilter;
              if (filter.QueryFilterInfo(out infoFilter) == 0)
              {
                nameFilter = infoFilter.achName;
                Release.FilterInfo(ref infoFilter);
              }
              this.LogDebug("Microsoft encoder: filter {0}", nameFilter);
              canReleaseFilter = !CheckInterfaces(filter);

              IEnumPins enumPins;
              hr = filter.EnumPins(out enumPins);
              if (hr != (int)NativeMethods.HResult.S_OK)
              {
                this.LogError("Microsoft encoder: failed to get filter pin enumerator");
                return false;
              }

              try
              {
                IPin[] pins = new IPin[2];
                int countPins = 1;
                while (enumPins.Next(1, pins, out countPins) == (int)NativeMethods.HResult.S_OK && countPins == 1)
                {
                  IPin pin = pins[0];
                  bool canReleasePin = true;
                  string namePin = "Unknown";
                  try
                  {
                    PinInfo infoPin;
                    if (pin.QueryPinInfo(out infoPin) == (int)NativeMethods.HResult.S_OK)
                    {
                      namePin = infoPin.name;
                      Release.PinInfo(ref infoPin);
                    }
                    this.LogDebug("Microsoft encoder:   pin {0}", namePin);
                    canReleasePin = !CheckInterfaces(pin);
                  }
                  finally
                  {
                    if (canReleasePin)
                    {
                      Release.ComObject(string.Format("Microsoft encoder graph filter {0} pin {1}", nameFilter, namePin), ref pin);
                    }
                  }
                }
              }
              finally
              {
                Release.ComObject(string.Format("Microsoft encoder graph filter {0} pin enumerator", nameFilter), ref enumPins);
              }
            }
            finally
            {
              if (canReleaseFilter)
              {
                Release.ComObject(string.Format("Microsoft encoder graph filter {0}", nameFilter), ref filter);
              }
            }
          }
        }
        finally
        {
          Release.ComObject("Microsoft encoder filter enumerator", ref enumFilters);
        }
      }
      finally
      {
        Release.ComObject("Microsoft encoder graph", ref filterInfo.pGraph);
      }

      if (_interfacesCodecApi.Count > 0 || _interfacesVideoEncoder.Count > 0 || _interfacesEncoderApi.Count > 0)
      {
        this.LogInfo("Microsoft encoder: extension supported");
        _isMicrosoftEncoder = true;
        return true;
      }

      this.LogDebug("Microsoft encoder: no supported interfaces found on any filters or pins");
      return false;
    }

    #endregion

    #region IEncoder members

    /// <summary>
    /// Determine whether the encoder can manipulate a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <returns><c>true</c> if the parameter can be manipulated, otherwise <c>false</c></returns>
    public bool IsParameterSupported(Guid parameterId)
    {
      this.LogDebug("Microsoft encoder: is parameter supported, parameter = {0}", parameterId);

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Microsoft encoder: interface {0} supports", i);
          isSupported = true;
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return isSupported;
    }

    /// <summary>
    /// Get the extents and resolution for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="minimum">The minimum value that the parameter may take.</param>
    /// <param name="maximum">The maximum value that the parameter may take.</param>
    /// <param name="resolution">The magnitude of the smallest adjustment that can be applied to
    ///   the parameter. In most cases the value of the parameter should be a multiple of th.</param>
    /// <returns><c>true</c> if the parameter extents and resolution are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterRange(Guid parameterId, out object minimum, out object maximum, out object resolution)
    {
      this.LogDebug("Microsoft encoder: get parameter range, parameter = {0}", parameterId);
      minimum = null;
      maximum = null;
      resolution = null;

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      bool success = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          isSupported = true;
          int hr = _delegatesGetRange[i](parameterId, out minimum, out maximum, out resolution);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("Microsoft encoder: interface {0}, minimum = {1}, maximum = {2}, resolution = {3}", i, minimum, maximum, resolution);
            success = true;
          }
          else
          {
            this.LogError("Microsoft encoder: failed to get parameter range for interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return success;
    }

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="values">The possible values that the parameter may take.</param>
    /// <returns><c>true</c> if the parameter values are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValues(Guid parameterId, out object[] values)
    {
      this.LogDebug("Microsoft encoder: get parameter values, parameter = {0}", parameterId);
      values = null;

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      bool success = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          isSupported = true;
          IntPtr valuesPtr;
          int valuesCount;
          int hr = _delegatesGetValues[i](parameterId, out valuesPtr, out valuesCount);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            values = Marshal.GetObjectsForNativeVariants(valuesPtr, valuesCount);
            this.LogDebug("Microsoft encoder: interface {0}, values = {1}", i, string.Join(", ", values));
            success = true;

            // Free memory. Each variant in the array must be VariantClear()'d
            // and then the variant array itself must be freed.
            IntPtr valuePtr = valuesPtr;
            for (int v = 0; v < valuesCount; v++)
            {
              NativeMethods.VariantClear(valuePtr);
              valuePtr = IntPtr.Add(valuePtr, 16);    // 16 = sizeof(VARIANT)
            }
            Marshal.FreeCoTaskMem(valuesPtr);
          }
          else
          {
            this.LogError("Microsoft encoder: failed to get parameter values for interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return success;
    }

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns><c>true</c> if the default parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterDefaultValue(Guid parameterId, out object value)
    {
      this.LogDebug("Microsoft encoder: get default value, parameter = {0}", parameterId);
      value = null;

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      bool success = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          isSupported = true;
          int hr = _delegatesGetDefaultValue[i](parameterId, out value);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("Microsoft encoder: interface {0}, value = {1}", i, value);
            success = true;
          }
          else
          {
            this.LogError("Microsoft encoder: failed to get parameter default value for interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return success;
    }

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns><c>true</c> if the current parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValue(Guid parameterId, out object value)
    {
      this.LogDebug("Microsoft encoder: get value, parameter = {0}", parameterId);
      value = null;

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      bool success = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          isSupported = true;
          int hr = _delegatesGetValue[i](parameterId, out value);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("Microsoft encoder: interface {0}, value = {1}", i, value);
            success = true;
          }
          else
          {
            this.LogError("Microsoft encoder: failed to get parameter value for interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return success;
    }

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns><c>true</c> if the parameter value is successfully set, otherwise <c>false</c></returns>
    public bool SetParameterValue(Guid parameterId, object value)
    {
      this.LogDebug("Microsoft encoder: set value, parameter = {0}, value = {1}", parameterId, value);

      if (!_isMicrosoftEncoder)
      {
        this.LogWarn("Microsoft encoder: not initialised or interface not supported");
        return false;
      }

      bool isSupported = false;
      bool success = false;
      for (int i = 0; i < _delegatesIsSupported.Count; i++)
      {
        if (_delegatesIsSupported[i](parameterId) == (int)NativeMethods.HResult.S_OK)
        {
          isSupported = true;
          int hr = _delegatesSetValue[i](parameterId, ref value);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("Microsoft encoder: interface {0}, set", i);
            success = true;
          }
          else
          {
            this.LogError("Microsoft encoder: failed to set parameter value for interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }
      if (!isSupported)
      {
        this.LogDebug("Microsoft encoder: not supported");
      }
      return success;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~MicrosoftEncoder()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (!isDisposing)
      {
        return;
      }

      _delegatesIsSupported.Clear();
      _delegatesGetRange.Clear();
      _delegatesGetValues.Clear();
      _delegatesGetDefaultValue.Clear();
      _delegatesGetValue.Clear();
      _delegatesSetValue.Clear();

      for (int i = 0; i < _interfacesCodecApi.Count; i++)
      {
        ICodecAPI codecApi = _interfacesCodecApi[i];
        Release.ComObject(string.Format("Microsoft encoder codec API interface {0}", i), ref codecApi);
      }
      _interfacesCodecApi.Clear();

      for (int i = 0; i < _interfacesVideoEncoder.Count; i++)
      {
        IVideoEncoder videoEncoder = _interfacesVideoEncoder[i];
        Release.ComObject(string.Format("Microsoft encoder video encoder interface {0}", i), ref videoEncoder);
      }
      _interfacesVideoEncoder.Clear();

      for (int i = 0; i < _interfacesEncoderApi.Count; i++)
      {
        // Disable obsolete interface warning. Some implementations of
        // IEncoderAPI that we want to support do not implement ICodecAPI.
        #pragma warning disable 618
        IEncoderAPI encoderApi = _interfacesEncoderApi[i];
        #pragma warning restore 618
        Release.ComObject(string.Format("Microsoft encoder encoder API interface {0}", i), ref encoderApi);
      }
      _interfacesEncoderApi.Clear();

      _isMicrosoftEncoder = false;
    }

    #endregion
  }
}