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
using System.Linq;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEncoder
{
  /// <summary>
  /// A class that implements encoder control for newer Hauppauge capture devices based on ViXS
  /// encoder chips. At present this includes the Colossus and HD-PVR 2 variants. Note HD-PVR 1
  /// supports standard Microsoft encoder interfaces.
  /// </summary>
  public class HauppaugeEncoder : BaseTunerExtension, IDisposable, IEncoder
  {
    #region enums

    // Note: not all hardware supports all properties and/or values.
    private enum PropertyVideo
    {
      AspectRatio = 0,
      TbcMode,
      Scaler,
      Filter,
      BitRate,
      SourceInfo,
      H264Settings,
      Latency         // Not supported by Colossus (or at least not <= D3).
    }

    private enum PropertyAudio
    {
      AacSettings = 0,
      AnalogBoost
    }

    private enum PropertyInfo
    {
      Model = 0,
      SerialNumber,
      Revision
    }

    private enum AspectRatio : int
    {
      Ratio4_3 = 0,
      Ratio16_9
    }

    private enum DownscalerResolution : int
    {
      Source = 0,
      H960V540,
      H720V480,
      H640V480,
      H640V360,
      H480V360,
      H480V270,

      // HD-PVR 2 = supported
      // Colossus = not supported
      H1280V720
    }

    private enum FrameRate : int
    {
      Source = 0,
      Fps30,
      Fps25,
      Fps15
    }

    private enum EncoderMode : int
    {
      ConstantBitRate = 0,
      VariableBitRate
    }

    private enum H264Profile : int
    {
      Default = 0,
      Base,
      Main,
      High
    }

    private enum H264Level : int
    {
      Default = 0,
      L1,
      L1b,
      L1_1,
      L1_2,
      L2_3,
      L2,
      L2_1,
      L2_2,
      L3,
      L3_1,
      L3_2,
      L4,
      L4_1,
      L4_2
    }

    private enum VideoLatency : int
    {
      Low = 0,
      Medium,
      High
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VideoAspectRatioSettings
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsActive;
      public AspectRatio AspectRatioSdi;    // standard definition interlaced
      public AspectRatio AspectRatioSdp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VideoScalerSettings
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsActive;
      public DownscalerResolution Resolution1080i;
      public DownscalerResolution Resolution720p;
      public DownscalerResolution ResolutionSdi;
      public DownscalerResolution ResolutionSdp;
      public FrameRate FrameRate1080i;
      public FrameRate FrameRate720p;
      public FrameRate FrameRateSdp;
      public FrameRate FrameRateSdi;

      // HD-PVR 2 = supported
      // Colossus = not supported
      public DownscalerResolution Resolution1080p;
      public FrameRate FrameRate1080p;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VideoBitRateSettings
    {
      public int BitRateSdi;
      public int PeakBitRateSdi;
      public EncoderMode ModeSdi;
      public int BitRateSdp;
      public int PeakBitRateSdp;
      public EncoderMode ModeSdp;
      public int BitRate720p;
      public int PeakBitRate720p;
      public EncoderMode Mode720p;
      public int BitRate1080i;
      public int PeakBitRate1080i;
      public EncoderMode Mode1080i;

      // HD-PVR 2 = supported
      // Colossus = not supported
      public int BitRate1080p;
      public int PeakBitRate1080p;
      public EncoderMode Mode1080p;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VideoSourceInfo
    {
      public int Width;
      public int Height;
      public int FrameRate;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsProgressive;
      public int Unknown;
      public int VideoId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VideoH264Settings
    {
      public H264Profile Profile;
      public H264Level Level;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct AudioAacSettings
    {
      public int BitRate;       // unit = b/s
      // [Downscaled] sample rates, per input sample rate.
      public int SampleRate48k; // unit = s/s
      public int SampleRate44k; // unit = s/s
    }

    #endregion

    #region constants

    private static readonly Guid PROPERTY_SET_VIDEO = new Guid(0x33438e86, 0x2ca0, 0x4428, 0x82, 0x14, 0x60, 0x1e, 0x34, 0x96, 0xff, 0x8d);
    private static readonly Guid PROPERTY_SET_AUDIO = new Guid(0x6337a7d7, 0x9f2f, 0x4ff7, 0x8e, 0xce, 0x14, 0xa5, 0x49, 0xd4, 0x8c, 0x2e);
    private static readonly Guid PROPERTY_SET_INFO = new Guid(0x9ba6c1a9, 0xd872, 0x4e89, 0x81, 0xf2, 0xb3, 0xf9, 0xda, 0x1f, 0x3e, 0x32);

    private const uint BIT_RATE_VIDEO_MINIMUM = 1000000;
    private const uint BIT_RATE_VIDEO_MAXIMUM = 13500000;
    private const uint BIT_RATE_VIDEO_MAXIMUM_COLOSSUS = 20000000;
    private const uint BIT_RATE_VIDEO_RESOLUTION = 500000;
    private const uint BIT_RATE_VIDEO_DEFAULT = 10000000;
    private const uint BIT_RATE_VIDEO_DEFAULT_PEAK = BIT_RATE_VIDEO_MAXIMUM;
    private const uint BIT_RATE_VIDEO_DEFAULT_PEAK_COLOSSUS = 15000000;

    private const uint BIT_RATE_AUDIO_MINIMUM = 64000;
    private const uint BIT_RATE_AUDIO_MAXIMUM = 256000;
    private const uint BIT_RATE_AUDIO_RESOLUTION = 32000;
    private const uint BIT_RATE_AUDIO_DEFAULT = 192000;

    private static readonly int VIDEO_ASPECT_RATIO_SETTINGS_SIZE = Marshal.SizeOf(typeof(VideoAspectRatioSettings));  // 12
    private static readonly int VIDEO_SCALER_SETTINGS_SIZE = Marshal.SizeOf(typeof(VideoScalerSettings));             // 44
    private static readonly int VIDEO_BIT_RATE_SETTINGS_SIZE = Marshal.SizeOf(typeof(VideoBitRateSettings));          // 60
    private static readonly int VIDEO_SOURCE_INFO_SIZE = Marshal.SizeOf(typeof(VideoSourceInfo));                     // 24
    private static readonly int VIDEO_H264_SETTINGS_SIZE = Marshal.SizeOf(typeof(VideoH264Settings));                 // 8
    private const int VIDEO_LATENCY_SIZE = 4;

    private static readonly int AUDIO_AAC_SETTINGS_SIZE = Marshal.SizeOf(typeof(AudioAacSettings));                   // 12
    private const int AUDIO_ANALOG_BOOST_SIZE = 4;

    private const int INFO_MODEL_SIZE = 4;
    private const int INFO_SERIAL_NUMBER_SIZE = 4;
    private const int INFO_REVISION_SIZE = 5;

    private static readonly int GENERAL_BUFFER_SIZE = new int[]
      {
        VIDEO_ASPECT_RATIO_SETTINGS_SIZE, VIDEO_SCALER_SETTINGS_SIZE, VIDEO_BIT_RATE_SETTINGS_SIZE,
        VIDEO_SOURCE_INFO_SIZE, VIDEO_H264_SETTINGS_SIZE, VIDEO_LATENCY_SIZE, AUDIO_AAC_SETTINGS_SIZE,
        AUDIO_ANALOG_BOOST_SIZE, INFO_MODEL_SIZE, INFO_SERIAL_NUMBER_SIZE, INFO_REVISION_SIZE
      }.Max();


    #endregion

    #region variables

    private bool _isHauppaugeEncoder = false;
    private bool _isColossus = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _generalBuffer = IntPtr.Zero;

    #endregion

    private void ReadDeviceInfo()
    {
      this.LogDebug("Hauppauge encoder: read device information");
      for (int i = 0; i < INFO_MODEL_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(PROPERTY_SET_INFO, (int)PropertyInfo.Model, _generalBuffer, INFO_MODEL_SIZE, _generalBuffer, INFO_MODEL_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != INFO_MODEL_SIZE)
      {
        this.LogWarn("Hauppauge encoder: failed to read model number, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        this.LogDebug("  model number  = {0}", Marshal.ReadInt32(_generalBuffer, 0));
      }

      for (int i = 0; i < INFO_SERIAL_NUMBER_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(PROPERTY_SET_INFO, (int)PropertyInfo.SerialNumber, _generalBuffer, INFO_SERIAL_NUMBER_SIZE, _generalBuffer, INFO_SERIAL_NUMBER_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != INFO_SERIAL_NUMBER_SIZE)
      {
        this.LogWarn("Hauppauge encoder: failed to read serial number, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        this.LogDebug("  serial number = {0}", Marshal.ReadInt32(_generalBuffer, 0));
      }

      for (int i = 0; i < INFO_REVISION_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(PROPERTY_SET_INFO, (int)PropertyInfo.Revision, _generalBuffer, INFO_REVISION_SIZE, _generalBuffer, INFO_REVISION_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != INFO_REVISION_SIZE)
      {
        this.LogWarn("Hauppauge encoder: failed to read revision, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        this.LogDebug("  revision      = {0}", Marshal.PtrToStringAnsi(_generalBuffer, INFO_REVISION_SIZE));
      }
    }

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 60;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge encoder";
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
      this.LogDebug("Hauppauge encoder: initialising");

      if (_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: extension already initialised");
        return true;
      }

      IBaseFilter mainFilter = context as IBaseFilter;
      if (mainFilter == null)
      {
        this.LogDebug("Hauppauge encoder: context is not a filter");
        return false;
      }

      // We need a reference to the graph.
      FilterInfo filterInfo;
      int hr = mainFilter.QueryFilterInfo(out filterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Hauppauge encoder: failed to get filter info, hr = 0x{0:x}", hr);
        return false;
      }
      IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
      if (graph == null)
      {
        this.LogError("Hauppauge encoder: failed to get graph reference");
        return false;
      }

      try
      {
        IEnumFilters enumFilters;
        hr = graph.EnumFilters(out enumFilters);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Hauppauge encoder: failed to get graph filter enumerator, hr = 0x{0:x}", hr);
          return false;
        }

        try
        {
          IBaseFilter[] filters = new IBaseFilter[2];
          int countFilters = 1;
          while (enumFilters.Next(1, filters, out countFilters) == (int)NativeMethods.HResult.S_OK && countFilters == 1)
          {
            IBaseFilter filter = filters[0];
            string filterName = "Unknown";
            try
            {
              FilterInfo infoFilter;
              if (filter.QueryFilterInfo(out infoFilter) == 0)
              {
                filterName = infoFilter.achName;
                Release.FilterInfo(ref infoFilter);
              }
              this.LogDebug("Hauppauge encoder: filter {0}", filterName);
              _propertySet = filter as IKsPropertySet;
              if (_propertySet == null)
              {
                this.LogDebug("Hauppauge encoder:   filter is not a property set");
                continue;
              }

              KSPropertySupport support;
              hr = _propertySet.QuerySupported(PROPERTY_SET_INFO, (int)PropertyInfo.Model, out support);
              if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Get))
              {
                this.LogInfo("Hauppauge encoder: extension supported");
                _isHauppaugeEncoder = true;
                _isColossus = filterName.Contains("Colossus");
                _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
                ReadDeviceInfo();
                return true;
              }
              this.LogDebug("Hauppauge encoder:   property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
            }
            finally
            {
              if (!_isHauppaugeEncoder)
              {
                Release.ComObject(string.Format("Hauppauge encoder graph filter {0}", filterName), ref filter);
                _propertySet = null;
              }
            }
          }
        }
        finally
        {
          Release.ComObject("Hauppauge encoder filter enumerator", ref enumFilters);
        }
      }
      finally
      {
        Release.ComObject("Hauppauge encoder graph", ref filterInfo.pGraph);
      }

      return false;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Hauppauge encoder: on before tune call back");
      action = TunerAction.Default;

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return;
      }

      for (int i = 0; i < VIDEO_SOURCE_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(PROPERTY_SET_VIDEO, (int)PropertyVideo.SourceInfo, _generalBuffer, VIDEO_SOURCE_INFO_SIZE, _generalBuffer, VIDEO_SOURCE_INFO_SIZE, out returnedByteCount);
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != VIDEO_SOURCE_INFO_SIZE)
      {
        this.LogError("Hauppauge encoder: failed to get source information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return;
      }

      VideoSourceInfo info = (VideoSourceInfo)Marshal.PtrToStructure(_generalBuffer, typeof(VideoSourceInfo));
      this.LogDebug("  resolution     = {0}x{1}", info.Width, info.Height);
      this.LogDebug("  frame rate     = {0} fps", info.FrameRate);
      this.LogDebug("  is progressive = {0}", info.IsProgressive);
      this.LogDebug("  video ID       = {0}", info.VideoId);
      this.LogDebug("  unknown        = {0}", info.Unknown);
    }

    #endregion

    #endregion

    #region IEncoder members

    /// <summary>
    /// Determine whether the encoder can manipulate a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <returns><c>true</c> if the parameter can be manipulated, otherwise <c>false</c></returns>
    public bool IsParameterSupported(Guid parameterId)
    {
      this.LogDebug("Hauppauge encoder: is parameter supported, parameter = {0}", parameterId);
      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_PeakBitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRateMode ||
        parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE ||
        parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        this.LogDebug("Hauppauge encoder: supported");
        return true;
      }
      this.LogDebug("Hauppauge encoder: not supported");
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
      this.LogDebug("Hauppauge encoder: get parameter range, parameter = {0}", parameterId);
      minimum = null;
      maximum = null;
      resolution = null;

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return false;
      }

      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_PeakBitRate)
      {
        minimum = BIT_RATE_VIDEO_MINIMUM;
        if (_isColossus)
        {
          maximum = BIT_RATE_VIDEO_MAXIMUM_COLOSSUS;
        }
        else
        {
          maximum = BIT_RATE_VIDEO_MAXIMUM;
        }
        resolution = BIT_RATE_VIDEO_RESOLUTION;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        minimum = (uint)eAVEncCommonRateControlMode.CBR;
        maximum = (uint)eAVEncCommonRateControlMode.PeakConstrainedVBR;
        resolution = (uint)1;
      }
      else if (parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        minimum = (int)VideoEncoderBitrateMode.ConstantBitRate;
        maximum = (int)VideoEncoderBitrateMode.VariableBitRatePeak;
        resolution = (int)1;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        minimum = BIT_RATE_AUDIO_MINIMUM;
        maximum = BIT_RATE_AUDIO_MAXIMUM;
        resolution = BIT_RATE_AUDIO_RESOLUTION;
      }
      else
      {
        this.LogDebug("Hauppauge encoder: not supported");
        return false;
      }

      this.LogDebug("Hauppauge encoder: result = success, minimum = {0}, maximum = {1}, resolution = {2}", minimum, maximum, resolution);
      return true;
    }

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="values">The possible values that the parameter may take.</param>
    /// <returns><c>true</c> if the parameter values are successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValues(Guid parameterId, out object[] values)
    {
      this.LogDebug("Hauppauge encoder: get parameter values, parameter = {0}", parameterId);
      values = null;

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return false;
      }

      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_PeakBitRate)
      {
        uint maxBitRate = BIT_RATE_VIDEO_MAXIMUM;
        if (_isColossus)
        {
          maxBitRate = BIT_RATE_VIDEO_MAXIMUM_COLOSSUS;
        }
        uint valueCount = 1 + ((maxBitRate - BIT_RATE_VIDEO_MINIMUM) / BIT_RATE_VIDEO_RESOLUTION);
        values = new object[valueCount];
        uint bitRate = BIT_RATE_VIDEO_MINIMUM;
        for (uint i = 0; i < valueCount; i++)
        {
          values[i] = bitRate;
          bitRate += BIT_RATE_VIDEO_RESOLUTION;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        values = new object[2];
        values[0] = (uint)eAVEncCommonRateControlMode.CBR;
        values[1] = (uint)eAVEncCommonRateControlMode.PeakConstrainedVBR;
      }
      else if (parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        values = new object[2];
        values[0] = (int)VideoEncoderBitrateMode.ConstantBitRate;
        values[1] = (int)VideoEncoderBitrateMode.VariableBitRatePeak;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        uint valueCount = 1 + ((BIT_RATE_AUDIO_MAXIMUM - BIT_RATE_AUDIO_MINIMUM) / BIT_RATE_AUDIO_RESOLUTION);
        values = new object[valueCount];
        uint bitRate = BIT_RATE_AUDIO_MINIMUM;
        for (int i = 0; i < valueCount; i++)
        {
          values[i] = bitRate;
          bitRate += BIT_RATE_AUDIO_RESOLUTION;
        }
      }
      else if (parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        values = new object[7];
        values[0] = (uint)48000;
        values[1] = (uint)32000;
        values[2] = (uint)24000;
        values[3] = (uint)16000;
        values[4] = (uint)8000;
        values[5] = (uint)22050;
        values[6] = (uint)11025;
      }
      else
      {
        this.LogDebug("Hauppauge encoder: not supported");
        return false;
      }

      this.LogDebug("Hauppauge encoder: result = success, values = {0}", string.Join(", ", values));
      return true;
    }

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns><c>true</c> if the default parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterDefaultValue(Guid parameterId, out object value)
    {
      this.LogDebug("Hauppauge encoder: get default value, parameter = {0}", parameterId);
      value = null;

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return false;
      }

      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
      {
        value = BIT_RATE_VIDEO_DEFAULT;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_PeakBitRate)
      {
        value = BIT_RATE_VIDEO_DEFAULT_PEAK;
        if (_isColossus)
        {
          value = BIT_RATE_VIDEO_DEFAULT_PEAK_COLOSSUS;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
      {
        value = (uint)eAVEncCommonRateControlMode.PeakConstrainedVBR;
      }
      else if (parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        value = (int)VideoEncoderBitrateMode.VariableBitRatePeak;
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
      {
        value = BIT_RATE_AUDIO_DEFAULT;
      }
      else if (parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        value = (uint)48000;
      }
      else
      {
        this.LogDebug("Hauppauge encoder: not supported");
        return false;
      }

      this.LogDebug("Hauppauge encoder: result = success, value = {0}", value);
      return true;
    }

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns><c>true</c> if the current parameter value is successfully retrieved, otherwise <c>false</c></returns>
    public bool GetParameterValue(Guid parameterId, out object value)
    {
      this.LogDebug("Hauppauge encoder: get value, parameter = {0}", parameterId);
      value = null;

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return false;
      }

      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_PeakBitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        for (int i = 0; i < VIDEO_BIT_RATE_SETTINGS_SIZE; i++)
        {
          Marshal.WriteByte(_generalBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = _propertySet.Get(PROPERTY_SET_VIDEO, (int)PropertyVideo.BitRate, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != VIDEO_BIT_RATE_SETTINGS_SIZE)
        {
          this.LogError("Hauppauge encoder: failed to get video bit rate, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        VideoBitRateSettings settings = (VideoBitRateSettings)Marshal.PtrToStructure(_generalBuffer, typeof(VideoBitRateSettings));
        if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
        {
          value = (uint)settings.BitRateSdi * 10;
        }
        else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_PeakBitRate)
        {
          value = (uint)settings.PeakBitRateSdi * 10;
        }
        else if (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE)
        {
          if (settings.ModeSdi == EncoderMode.ConstantBitRate)
          {
            value = (uint)eAVEncCommonRateControlMode.CBR;
          }
          else
          {
            value = (uint)eAVEncCommonRateControlMode.PeakConstrainedVBR;
          }
        }
        else
        {
          if (settings.ModeSdi == EncoderMode.ConstantBitRate)
          {
            value = (int)VideoEncoderBitrateMode.ConstantBitRate;
          }
          else
          {
            value = (int)VideoEncoderBitrateMode.VariableBitRatePeak;
          }
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE || parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        for (int i = 0; i < AUDIO_AAC_SETTINGS_SIZE; i++)
        {
          Marshal.WriteByte(_generalBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = _propertySet.Get(PROPERTY_SET_AUDIO, (int)PropertyAudio.AacSettings, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != AUDIO_AAC_SETTINGS_SIZE)
        {
          this.LogError("Hauppauge encoder: failed to get AAC audio settings, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        AudioAacSettings settings = (AudioAacSettings)Marshal.PtrToStructure(_generalBuffer, typeof(AudioAacSettings));
        if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
        {
          value = (uint)settings.BitRate;
        }
        else
        {
          value = (uint)settings.SampleRate48k;
        }
      }
      else
      {
        this.LogDebug("Hauppauge encoder: not supported");
        return false;
      }

      this.LogDebug("Hauppauge encoder: result = success, value = {0}", value);
      return true;
    }

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns><c>true</c> if the parameter value is successfully set, otherwise <c>false</c></returns>
    public bool SetParameterValue(Guid parameterId, object value)
    {
      this.LogDebug("Hauppauge encoder: set value, parameter = {0}, value = {1}", parameterId, value);

      if (!_isHauppaugeEncoder)
      {
        this.LogWarn("Hauppauge encoder: not initialised or interface not supported");
        return false;
      }

      if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE ||
        parameterId == PropSetID.ENCAPIPARAM_PeakBitRate ||
        parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE ||
        parameterId == PropSetID.ENCAPIPARAM_BitRateMode)
      {
        for (int i = 0; i < VIDEO_BIT_RATE_SETTINGS_SIZE; i++)
        {
          Marshal.WriteByte(_generalBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = _propertySet.Get(PROPERTY_SET_VIDEO, (int)PropertyVideo.BitRate, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != VIDEO_BIT_RATE_SETTINGS_SIZE)
        {
          this.LogError("Hauppauge encoder: failed to get video bit rate, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        int intValue = Convert.ToInt32(value);
        VideoBitRateSettings settings = (VideoBitRateSettings)Marshal.PtrToStructure(_generalBuffer, typeof(VideoBitRateSettings));
        if (parameterId == CodecApiParameter.AV_ENC_COMMON_MEAN_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_BitRate)
        {
          int bitRate = intValue / 10;
          settings.BitRateSdi = bitRate;
          settings.BitRateSdp = bitRate;
          settings.BitRate720p = bitRate;
          settings.BitRate1080i = bitRate;
          settings.BitRate1080p = bitRate;
        }
        else if (parameterId == CodecApiParameter.AV_ENC_COMMON_MAX_BIT_RATE || parameterId == PropSetID.ENCAPIPARAM_PeakBitRate)
        {
          int bitRate = intValue / 10;
          settings.PeakBitRateSdi = bitRate;
          settings.PeakBitRateSdp = bitRate;
          settings.PeakBitRate720p = bitRate;
          settings.PeakBitRate1080i = bitRate;
          settings.PeakBitRate1080p = bitRate;
        }
        else
        {
          EncoderMode mode = EncoderMode.VariableBitRate;
          if (
            (parameterId == CodecApiParameter.AV_ENC_COMMON_RATE_CONTROL_MODE && intValue == (int)eAVEncCommonRateControlMode.CBR) ||
            (parameterId == PropSetID.ENCAPIPARAM_PeakBitRate && intValue == (int)VideoEncoderBitrateMode.ConstantBitRate)
          )
          {
            mode = EncoderMode.ConstantBitRate;
          }
          settings.ModeSdi = mode;
          settings.ModeSdp = mode;
          settings.Mode720p = mode;
          settings.Mode1080i = mode;
          settings.Mode1080p = mode;
        }
        Marshal.StructureToPtr(settings, _generalBuffer, false);
        hr = _propertySet.Set(PROPERTY_SET_VIDEO, (int)PropertyVideo.BitRate, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE, _generalBuffer, VIDEO_BIT_RATE_SETTINGS_SIZE);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Hauppauge encoder: failed to set video bit rate, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }
      }
      else if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE || parameterId == CodecApiParameter.AV_AUDIO_SAMPLE_RATE)
      {
        for (int i = 0; i < AUDIO_AAC_SETTINGS_SIZE; i++)
        {
          Marshal.WriteByte(_generalBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = _propertySet.Get(PROPERTY_SET_AUDIO, (int)PropertyAudio.AacSettings, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != AUDIO_AAC_SETTINGS_SIZE)
        {
          this.LogError("Hauppauge encoder: failed to get AAC audio settings, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        AudioAacSettings settings = (AudioAacSettings)Marshal.PtrToStructure(_generalBuffer, typeof(AudioAacSettings));
        if (parameterId == CodecApiParameter.AV_ENC_AUDIO_MEAN_BIT_RATE)
        {
          settings.BitRate = Convert.ToInt32(value);
        }
        else
        {
          int sampleRate = Convert.ToInt32(value);
          if (sampleRate % 1000 == 0)
          {
            settings.SampleRate48k = sampleRate;
          }
          else
          {
            settings.SampleRate44k = sampleRate;
          }
        }
        Marshal.StructureToPtr(settings, _generalBuffer, false);
        hr = _propertySet.Set(PROPERTY_SET_AUDIO, (int)PropertyAudio.AacSettings, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE, _generalBuffer, AUDIO_AAC_SETTINGS_SIZE);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Hauppauge encoder: failed to set AAC audio settings, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }
      }
      else
      {
        this.LogDebug("Hauppauge encoder: not supported");
      }

      this.LogDebug("Hauppauge encoder: result = success");
      return true;
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

    ~HauppaugeEncoder()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Release.ComObject("Hauppauge encoder property set", ref _propertySet);
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isHauppaugeEncoder = false;
    }

    #endregion
  }
}
