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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service
{
  internal class ServiceEncoder : ServiceBase
  {
    private CpAction _getEncoderCapabilitiesAction = null;
    private CpAction _setEncoderParametersAction = null;
    private CpAction _getEncoderParametersAction = null;

    public ServiceEncoder(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Encoder")
    {
      _service.Actions.TryGetValue("GetEncoderCapabilities", out _getEncoderCapabilitiesAction);
      _service.Actions.TryGetValue("SetEncoderParameters", out _setEncoderParametersAction);
      _service.Actions.TryGetValue("GetEncoderParameters", out _getEncoderParametersAction);
    }

    /// <summary>
    /// Upon receipt of the GetEncoderCapabilities action, the DRIT SHALL provide a detailed description of its audio and
    /// video encoder characteristics in less than 1s.
    /// </summary>
    /// <param name="audioProfile">This argument provides the value of the AudioEncoderMethodList state variable when the action response is created.</param>
    /// <param name="videoProfile">This argument provides the value of the VideoEncoderMethodList state variable when the action response is created.</param>
    public void GetEncoderCapabilities(out IList<EncoderAudioProfile> audioProfile, out IList<EncoderVideoProfile> videoProfile)
    {
      IList<object> outParams = _getEncoderCapabilitiesAction.InvokeAction(null);
      audioProfile = new List<EncoderAudioProfile>();
      byte[] bytes = (byte[])outParams[0];
      if (bytes != null && bytes.Length > 0)
      {
        byte numberAudioCompressionFormat = bytes[0];
        int expectedByteCount = (Marshal.SizeOf(typeof(EncoderAudioProfile)) * numberAudioCompressionFormat) + 1;
        if (bytes.Length != expectedByteCount)
        {
          throw new TvException("GetEncoderCapabilities audioProfile has {0} profile(s), but the byte count is {1} (expected {2}).", numberAudioCompressionFormat, bytes.Length, expectedByteCount);
        }

        int i = 1;
        while (i < expectedByteCount)
        {
          EncoderAudioProfile profile = new EncoderAudioProfile();
          profile.AudioAlgorithmCode = (EncoderAudioAlgorithm)bytes[i++];
          profile.SamplingRate = (uint)((bytes[i] << 24) | (bytes[i + 1] << 16) | (bytes[i + 2] << 8) | bytes[i]);  // note the endianness
          i += 4;
          profile.BitDepth = bytes[i++];
          profile.NumberChannel = bytes[i++];
          audioProfile.Add(profile);
        }
      }
      videoProfile = new List<EncoderVideoProfile>();
      bytes = (byte[])outParams[1];
      if (bytes != null && bytes.Length > 0)
      {
        byte numberVideoCompressionFormat = bytes[0];
        int expectedByteCount = (Marshal.SizeOf(typeof(EncoderVideoProfile)) * numberVideoCompressionFormat) + 1;
        if (bytes.Length != expectedByteCount)
        {
          throw new TvException("GetEncoderCapabilities videoProfile has {0} profile(s), but the byte count is {1} (expected {2}).", numberVideoCompressionFormat, bytes.Length, expectedByteCount);
        }

        int i = 1;
        while (i < expectedByteCount)
        {
          EncoderVideoProfile profile = new EncoderVideoProfile();
          profile.VerticalSize = (ushort)((bytes[i] << 8) | bytes[i + 1]);    // note the endianness
          i += 2;
          profile.HorizontalSize = (ushort)((bytes[i] << 8) | bytes[i + 1]);  // note the endianness
          i += 2;
          profile.AspectRatioInformation = (EncoderVideoAspectRatio)bytes[i++];
          profile.FrameRateCode = (EncoderVideoFrameRate)bytes[i++];
          profile.ProgressiveSequence = (EncoderVideoProgressiveSequence)bytes[i++];
          videoProfile.Add(profile);
        }
      }
    }

    /// <summary>
    /// Upon receipt of the SetEncoderParameters action, the DRIT SHALL configure the audio and video encoder based
    /// on the input parameters in less than 2s.
    /// </summary>
    /// <param name="audioMode">This argument sets the AudioBitrateMode state variable.</param>
    /// <param name="audioBitrate">This argument sets the AudioBitrateTarget state variable.</param>
    /// <param name="audioMethod">This argument defines the AudioEncoderMethodNumber state variable.</param>
    /// <param name="mute">This argument sets the AudioMute state variable.</param>
    /// <param name="fieldToggle">This argument sets the FieldOrdering state variable.</param>
    /// <param name="signalSource">This argument sets the InputSelection state variable.</param>
    /// <param name="noiseFilter">This argument sets the NoiseReduction state variable.</param>
    /// <param name="pulldown">This argument sets the PulldownSelection state variable.</param>
    /// <param name="sap">This argument set the SAPSelection state variable.</param>
    /// <param name="videoMode">This argument sets the VideoBitrateMode state variable.</param>
    /// <param name="videoBitrate">This argument defines the VideoBitrateTarget state variable.</param>
    /// <param name="videoMethod">This argument defines the VideoEncoderMethodNumber state variable.</param>
    public void SetEncoderParameters(EncoderMode audioMode, uint audioBitrate, byte audioMethod, bool mute,
                                      EncoderFieldOrder fieldToggle, EncoderInputSelection signalSource,
                                      bool noiseFilter, bool pulldown, bool sap, EncoderMode videoMode,
                                      uint videoBitrate, byte videoMethod)
    {
      _setEncoderParametersAction.InvokeAction(new List<object> {
        audioMode.ToString(), audioBitrate, audioMethod, mute, fieldToggle.ToString(), signalSource.ToString(),
        noiseFilter, pulldown, sap, videoMode.ToString(), videoBitrate, videoMethod
      });
    }

    /// <summary>
    /// Upon receipt of the GetEncoderParameters action, the DRIT SHALL report the current configuration of the audio
    /// and video encoder in less than 1s.
    /// </summary>
    /// <param name="currentAudioMax">This argument provides the value of the AudioBitrateMax state variable when the action response is created.</param>
    /// <param name="currentAudioMin">This argument provides the value of the AudioBitrateMin state variable when the action response is created.</param>
    /// <param name="currentAudioMode">This argument provides the value of the AudioBitrateMode state variable when the action response is created.</param>
    /// <param name="currentAudioStepping">This argument provides the value of the AudioBitrateStepping state variable when the action response is created.</param>
    /// <param name="currentAudioBitrate">This argument provides the value of the AudioBitrateTarget state variable when the action response is created.</param>
    /// <param name="currentAudioMethod">This argument provides the value of the AudioEncoderMethodNumber state variable when the action response is created.</param>
    /// <param name="currentMuteStatus">This argument provides the value of the AudioMute state variable when the action response is created.</param>
    /// <param name="currentFieldOrder">This argument provides the value of the FieldOrdering state variable when the action response is created.</param>
    /// <param name="currentSignalSource">This argument provides the value of the InputSelection state variable when the action response is created.</param>
    /// <param name="currentNoiseFilter">This argument provides the value of the NoiseReduction state variable when the action response is created.</param>
    /// <param name="currentPulldownStatus">This argument provides the value of the PulldownDetection state variable when the action response is created.</param>
    /// <param name="currentPulldownSetting">This argument provides the value of the PulldownSelection state variable when the action response is created.</param>
    /// <param name="currentSapStatus">This argument provides the value of the SAPDetection state variable when the action response is created.</param>
    /// <param name="currentSapSetting">This argument provides the value of the SAPSelection state variable when the action response is created.</param>
    /// <param name="currentVideoMax">This argument provides the value of the VideoBitrateMax state variable when the action response is created.</param>
    /// <param name="currentVideoMin">This argument provides the value of the VideoBitrateMin state variable when the action response is created.</param>
    /// <param name="currentVideoMode">This argument provides the value of the VideoBitrateMode state variable when the action response is created.</param>
    /// <param name="currentVideoBitrate">This argument provides the value of the VideoBitrateStepping state variable when the action response is created.</param>
    /// <param name="currentVideoStepping">This argument provides the value of the VideoBitrateTarget state variable when the action response is created.</param>
    /// <param name="currentVideoMethod">This argument provides the value of the VideoEncoderMethodNumber state variable when the action response is created.</param>
    public void GetEncoderParameters(out uint currentAudioMax, out uint currentAudioMin, out EncoderMode currentAudioMode,
                                      out uint currentAudioStepping, out uint currentAudioBitrate, out byte currentAudioMethod,
                                      out bool currentMuteStatus, out EncoderFieldOrder currentFieldOrder,
                                      out EncoderInputSelection currentSignalSource, out bool currentNoiseFilter,
                                      out bool currentPulldownStatus, out bool currentPulldownSetting, out bool currentSapStatus,
                                      out bool currentSapSetting, out uint currentVideoMax, out uint currentVideoMin,
                                      out EncoderMode currentVideoMode, out uint currentVideoBitrate,
                                      out uint currentVideoStepping, out byte currentVideoMethod)
    {
      IList<object> outParams = _getEncoderParametersAction.InvokeAction(null);
      currentAudioMax = (uint)outParams[0];
      currentAudioMin = (uint)outParams[1];
      currentAudioMode = (EncoderMode)(string)outParams[2];
      currentAudioStepping = (uint)outParams[3];
      currentAudioBitrate = (uint)outParams[4];
      currentAudioMethod = (byte)outParams[5];
      currentMuteStatus = (bool)outParams[6];
      currentFieldOrder = (EncoderFieldOrder)(string)outParams[7];
      currentSignalSource = (EncoderInputSelection)(string)outParams[8];
      currentNoiseFilter = (bool)outParams[9];
      currentPulldownStatus = (bool)outParams[10];
      currentPulldownSetting = (bool)outParams[11];
      currentSapStatus = (bool)outParams[12];
      currentSapSetting = (bool)outParams[13];
      currentVideoMax = (uint)outParams[14];
      currentVideoMin = (uint)outParams[15];
      currentVideoMode = (EncoderMode)(string)outParams[16];
      currentVideoBitrate = (uint)outParams[17];
      currentVideoStepping = (uint)outParams[18];
      currentVideoMethod = (byte)outParams[19];
    }

    /// <remarks>
    /// NOT A DRI ACTION.
    /// </remarks>
    public void GetEncoderModeDetails(out IList<EncoderMode> supportedModesVideo, out EncoderMode defaultModeVideo, out IList<EncoderMode> supportedModesAudio, out EncoderMode defaultModeAudio)
    {
      GetEncoderModeDetails("VideoBitrateMode", out supportedModesVideo, out defaultModeVideo);
      GetEncoderModeDetails("AudioBitrateMode", out supportedModesAudio, out defaultModeAudio);
    }

    private void GetEncoderModeDetails(string variableName, out IList<EncoderMode> supportedModes, out EncoderMode defaultMode)
    {
      supportedModes = new List<EncoderMode>(EncoderMode.Values.Count);
      defaultMode = null;
      CpStateVariable variable;
      if (_service.StateVariables.TryGetValue(variableName, out variable) && variable != null)
      {
        foreach (string value in variable.AllowedValueList)
        {
          EncoderMode mode = (EncoderMode)value;
          if (mode != null)
          {
            supportedModes.Add(mode);
          }
        }

        defaultMode = (EncoderMode)((string)variable.DefaultValue);
        if (defaultMode == null)
        {
          defaultMode = variableName.ToLowerInvariant().Contains("audio") ? EncoderMode.ConstantBitRate : EncoderMode.AverageBitRate;
        }
      }
    }
  }
}