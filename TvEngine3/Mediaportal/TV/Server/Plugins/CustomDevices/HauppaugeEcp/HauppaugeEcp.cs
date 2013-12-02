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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEcp
{
  /// <summary>
  /// A class that implements encoder control for older Hauppauge analog tuners which expose their
  /// encode configuration properties interface.
  /// </summary>
  public class HauppaugeEcp : BaseCustomDevice, IEncoder
  {
    /// <summary>
    /// MediaPortal's wrapper class for the Hauppauge ECP interface.
    /// </summary>
    [ComImport, Guid("7f4a6ccd-3f79-444a-bf65-02bd5bff80d5")]
    private class MediaPortalHauppaugeEncoderConfigurationProperties
    {
    }

    /// <summary>
    /// The main interface on the Hauppauge ECP wrapper class.
    /// </summary>
    [ComVisible(false), ComImport,
      Guid("0530ee38-eb91-49ea-aaf5-f85402ff0ca5"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMpHcwEcp
    {
      [PreserveSig]
      int Initialise(IBaseFilter captureFilter);

      [PreserveSig]
      int Dispose();

      [PreserveSig]
      int GetModelNumber(ref uint modelNumber);

      [PreserveSig]
      int GetDriverVersion(ref uint versionMajor, ref uint versionMinor, ref uint revision, ref uint build);

      [PreserveSig]
      int GetGeneralInfo([MarshalAs(UnmanagedType.LPStr)] out string info);
    }

    private bool _isHauppaugeEcp = false;
    private IMpHcwEcp _interfaceEcp = null;
    private ICodecAPI _interfaceCodecApi = null;
    private IBaseFilter _filter = null;

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("Hauppauge ECP: read device information");

      this.LogDebug("Hauppauge ECP: reading model number");
      uint modelNumber = 0;
      int hr = _interfaceEcp.GetModelNumber(ref modelNumber);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  model number = {0}", modelNumber);
      }
      else
      {
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      this.LogDebug("Hauppauge ECP: reading version number");
      uint versionMajor = 0;
      uint versionMinor = 0;
      uint revision = 0;
      uint build = 0;
      hr = _interfaceEcp.GetDriverVersion(ref versionMajor, ref versionMinor, ref revision, ref build);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  version = {0}.{1}.{2}b{3}", versionMajor, versionMinor, revision, build);
      }
      else
      {
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }

      this.LogDebug("Hauppauge ECP: reading general info");
      string generalInfo;
      hr = _interfaceEcp.GetGeneralInfo(out generalInfo);
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("  info = {0}", generalInfo);
      }
      else
      {
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 60;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge ECP";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
    {
      this.LogDebug("Hauppauge ECP: initialising device");

      IBaseFilter mainFilter = context as IBaseFilter;
      if (mainFilter == null)
      {
        this.LogDebug("Hauppauge ECP: main filter is null");
        return false;
      }
      if (_isHauppaugeEcp)
      {
        this.LogDebug("Hauppauge ECP: device is already initialised");
        return true;
      }

      _interfaceEcp = ComHelper.LoadComObjectFromFile("HauppaugeEcp.dll", typeof(MediaPortalHauppaugeEncoderConfigurationProperties).GUID, typeof(IMpHcwEcp).GUID, true) as IMpHcwEcp;

      // We need a reference to the graph.
      FilterInfo filterInfo;
      int hr = mainFilter.QueryFilterInfo(out filterInfo);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge ECP: failed to get filter info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
      if (graph == null)
      {
        this.LogDebug("Hauppauge ECP: failed to get graph reference");
        return false;
      }

      try
      {
        IEnumFilters enumFilters;
        hr = graph.EnumFilters(out enumFilters);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogDebug("Hauppauge ECP: failed to get graph filter enumerator, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        try
        {
          IBaseFilter[] filters = new IBaseFilter[2];
          int countFilters = 1;
          while (enumFilters.Next(1, filters, out countFilters) == (int)HResult.Severity.Success && countFilters == 1)
          {
            IBaseFilter filter = filters[0];
            string nameFilter = "Unknown";
            try
            {
              FilterInfo infoFilter;
              if (filter.QueryFilterInfo(out infoFilter) == 0)
              {
                nameFilter = infoFilter.achName;
                Release.FilterInfo(ref infoFilter);
              }
              this.LogDebug("Hauppauge ECP: filter {0}", nameFilter);
              if (_interfaceEcp.Initialise(filter) == (int)HResult.Severity.Success)
              {
                this.LogDebug("Hauppauge ECP: supported device detected");
                _isHauppaugeEcp = true;
                _interfaceCodecApi = _interfaceEcp as ICodecAPI;
                _filter = filter;
                ReadDeviceInfo();
                return true;
              }
              else
              {
                _interfaceEcp.Dispose();
                Release.ComObject("Hauppauge ECP interface", ref _interfaceEcp);
              }
            }
            finally
            {
              if (_filter == null)
              {
                Release.ComObject(string.Format("Hauppauge ECP graph filter {0}", nameFilter), ref filter);
              }
            }
          }
        }
        finally
        {
          Release.ComObject("Hauppauge ECP filter enumerator", ref enumFilters);
        }
      }
      finally
      {
        Release.ComObject("Hauppauge ECP graph", ref filterInfo.pGraph);
      }

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
      this.LogDebug("Hauppauge ECP: is parameter supported, parameter = {0}", parameterId);
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge ECP: supported");
        return true;
      }
      this.LogDebug("Hauppauge ECP: not supported");
      return false;
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
      this.LogDebug("Hauppauge ECP: get parameter range, parameter = {0}", parameterId);
      minimum = null;
      maximum = null;
      resolution = null;
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        int hr = _interfaceCodecApi.GetParameterRange(parameterId, out minimum, out maximum, out resolution);
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("Hauppauge ECP: result = success, minimum = {0}, maximum = {1}, resolution = {2}", minimum, maximum, resolution);
          return true;
        }
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        this.LogDebug("Hauppauge ECP: not supported");
      }
      return false;
    }

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="values">The possible values that the parameter may take.</param>
    /// <returns><c>true</c> if the parameter values are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValues(Guid parameterId, out object[] values)
    {
      this.LogDebug("Hauppauge ECP: get parameter values, parameter = {0}", parameterId);
      values = null;
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        IntPtr valuesPtr;
        int valuesCount;
        int hr = _interfaceCodecApi.GetParameterValues(parameterId, out valuesPtr, out valuesCount);
        if (hr == (int)HResult.Severity.Success)
        {
          values = Marshal.GetObjectsForNativeVariants(valuesPtr, valuesCount);
          this.LogDebug("Hauppauge ECP: result = success, values = {0}", string.Join(", ", values));
          return true;
        }
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        this.LogDebug("Hauppauge ECP: not supported");
      }
      return false;
    }

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns><c>true</c> if the default parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterDefaultValue(Guid parameterId, out object value)
    {
      this.LogDebug("Hauppauge ECP: get default value, parameter = {0}", parameterId);
      value = null;
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        int hr = _interfaceCodecApi.GetDefaultValue(parameterId, out value);
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("Hauppauge ECP: result = success, value = {0}", value);
          return true;
        }
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        this.LogDebug("Hauppauge ECP: not supported");
      }
      return false;
    }

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns><c>true</c> if the current parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValue(Guid parameterId, out object value)
    {
      this.LogDebug("Hauppauge ECP: get value, parameter = {0}", parameterId);
      value = null;
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        int hr = _interfaceCodecApi.GetValue(parameterId, out value);
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("Hauppauge ECP: result = success, value = {0}", value);
          return true;
        }
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        this.LogDebug("Hauppauge ECP: not supported");
      }
      return false;
    }

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns><c>true</c> if the parameter value is successfully set, otherwise <c>false</c></returns>
    public bool SetParameterValue(Guid parameterId, object value)
    {
      this.LogDebug("Hauppauge ECP: set value, parameter = {0}, value = {1}", parameterId, value);
      if (_interfaceCodecApi.IsSupported(parameterId) == (int)HResult.Severity.Success)
      {
        int hr = _interfaceCodecApi.SetValue(parameterId, ref value);
        if (hr == (int)HResult.Severity.Success)
        {
          this.LogDebug("Hauppauge ECP: result = success");
          return true;
        }
        this.LogDebug("Hauppauge ECP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        this.LogDebug("Hauppauge ECP: not supported");
      }
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      _interfaceCodecApi = null;
      if (_interfaceEcp != null)
      {
        _interfaceEcp.Dispose();
        Release.ComObject("Hauppauge ECP interface", ref _interfaceEcp);
      }
      Release.ComObject("Hauppauge ECP filter", ref _filter);      
      _isHauppaugeEcp = false;
    }

    #endregion
  }
}