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
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public enum DriEncoderAudioAlgorithm : byte
  {
    Mpeg1Layer2 = 0,
    DobyAc3 = 1
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(false)]
  public struct DriEncoderAudioProfile
  {
    public DriEncoderAudioAlgorithm AudioAlgorithmCode;
    public UInt32 SamplingRate;   // unit = Hz, 48 kHz guaranteed supported
    public byte BitDepth;         // 16 bit per sample guaranteed supported
    public byte NumberChannel;    // stereo (2 channel) guaranteed supported
  }

  public enum DriEncoderVideoAspectRatio : byte
  {
    SquareSamples = 1,
    Ar4_3 = 2,
    Ar16_9 = 3
  }

  public enum DriEncoderVideoFrameRate : byte
  {
    Fr23_976 = 1,
    Fr24 = 2,
    Fr29_97 = 4,
    Fr30 = 5,
    Fr59_94 = 7,
    Fr60 = 8
  }

  public enum DriEncoderVideoProgressiveSequence : byte
  {
    Interlaced = 0,
    Progressive = 1
  }

  // See SCTE 43 section 5.1.2 for valid combinations
  [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(false)]
  public struct DriEncoderVideoProfile
  {
    public UInt16 VerticalSize;   // unit = pixels
    public UInt16 HorizontalSize; // unit = pixels
    public DriEncoderVideoAspectRatio AspectRatioInformation;
    public DriEncoderVideoFrameRate FrameRateCode;
    public DriEncoderVideoProgressiveSequence ProgressiveSequence;
  }

  public enum DriEncoderAudioMode
  {
    CBR,
    AVR,
    VBR
  }

  public enum DriEncoderFieldOrder
  {
    Lower,
    Higher
  }

  public enum DriEncoderInputSelection
  {
    Tuner,
    Aux
  }

  public enum DriEncoderVideoMode
  {
    CBR,
    AVR,
    VBR
  }

  public class EncoderService : BaseService
  {
    private CpAction _getEncoderCapabilitiesAction = null;
    private CpAction _setEncoderParametersAction = null;
    private CpAction _getEncoderParametersAction = null;

    public EncoderService(CpDevice device)
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
    public void GetEncoderCapabilities(out IList<DriEncoderAudioProfile> audioProfile, out IList<DriEncoderVideoProfile> videoProfile)
    {
      IList<object> outParams = _getEncoderCapabilitiesAction.InvokeAction(null);
      audioProfile = new List<DriEncoderAudioProfile>();
      byte[] bytes = (byte[])outParams[0];
      if (bytes != null && bytes.Length > 0)
      {
        byte numberAudioCompressionFormat = bytes[0];
        int audioProfileSize = Marshal.SizeOf(typeof(DriEncoderAudioProfile));
        int expectedByteCount = (audioProfileSize * numberAudioCompressionFormat) + 1;
        if (bytes.Length != expectedByteCount)
        {
          throw new Exception(string.Format("DRI: GetEncoderCapabilities audioProfile has {0} profile(s), but the byte count is {1} (we expect {2})", numberAudioCompressionFormat, bytes.Length, expectedByteCount));
        }
        else
        {
          for (int i = 1; i < expectedByteCount; i += audioProfileSize)
          {
            GCHandle handle = GCHandle.Alloc(bytes[i], GCHandleType.Pinned);
            audioProfile.Add((DriEncoderAudioProfile)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DriEncoderAudioProfile)));
            handle.Free();
          }
        }
      }
      videoProfile = new List<DriEncoderVideoProfile>();
      bytes = (byte[])outParams[1];
      if (bytes != null && bytes.Length > 0)
      {
        byte numberVideoCompressionFormat = bytes[0];
        int videoProfileSize = Marshal.SizeOf(typeof(DriEncoderVideoProfile));
        int expectedByteCount = (videoProfileSize * numberVideoCompressionFormat) + 1;
        if (bytes.Length != expectedByteCount)
        {
          throw new TvException(string.Format("DRI: GetEncoderCapabilities videoProfile has {0} profile(s), but the byte count is {1} (we expect {2})", numberVideoCompressionFormat, bytes.Length, expectedByteCount));
        }
        else
        {
          for (int i = 1; i < expectedByteCount; i += videoProfileSize)
          {
            GCHandle handle = GCHandle.Alloc(bytes[i], GCHandleType.Pinned);
            videoProfile.Add((DriEncoderVideoProfile)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DriEncoderVideoProfile)));
            handle.Free();
          }
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
    public void SetEncoderParameters(DriEncoderAudioMode audioMode, UInt32 audioBitrate, byte audioMethod, bool mute,
                                      DriEncoderFieldOrder fieldToggle, DriEncoderInputSelection signalSource,
                                      bool noiseFilter, bool pulldown, bool sap, DriEncoderVideoMode videoMode,
                                      UInt32 videoBitrate, byte videoMethod)
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
    public void GetEncoderParameters(out UInt32 currentAudioMax, out UInt32 currentAudioMin, out DriEncoderAudioMode currentAudioMode,
                                    out UInt32 currentAudioStepping, out UInt32 currentAudioBitrate, out byte currentAudioMethod,
                                    out bool currentMuteStatus, out DriEncoderFieldOrder currentFieldOrder,
                                    out DriEncoderInputSelection currentSignalSource, out bool currentNoiseFilter,
                                    out bool currentPulldownStatus, out bool currentPulldownSetting, out bool currentSapStatus,
                                    out bool currentSapSetting, out UInt32 currentVideoMax, out UInt32 currentVideoMin,
                                    out DriEncoderVideoMode currentVideoMode, out UInt32 currentVideoBitrate,
                                    out UInt32 currentVideoStepping, out byte currentVideoMethod)
    {
      IList<object> outParams = _getEncoderParametersAction.InvokeAction(null);
      currentAudioMax = (uint)outParams[0];
      currentAudioMin = (uint)outParams[1];
      currentAudioMode = (DriEncoderAudioMode)Enum.Parse(typeof(DriEncoderAudioMode), (string)outParams[2]);
      currentAudioStepping = (uint)outParams[3];
      currentAudioBitrate = (uint)outParams[4];
      currentAudioMethod = (byte)outParams[5];
      currentMuteStatus = (bool)outParams[6];
      currentFieldOrder = (DriEncoderFieldOrder)Enum.Parse(typeof(DriEncoderFieldOrder), (string)outParams[7]);
      currentSignalSource = (DriEncoderInputSelection)Enum.Parse(typeof(DriEncoderInputSelection), (string)outParams[8]);
      currentNoiseFilter = (bool)outParams[9];
      currentPulldownStatus = (bool)outParams[10];
      currentPulldownSetting = (bool)outParams[11];
      currentSapStatus = (bool)outParams[12];
      currentSapSetting = (bool)outParams[13];
      currentVideoMax = (uint)outParams[14];
      currentVideoMin = (uint)outParams[15];
      currentVideoMode = (DriEncoderVideoMode)Enum.Parse(typeof(DriEncoderVideoMode), (string)outParams[16]);
      currentVideoBitrate = (uint)outParams[17];
      currentVideoStepping = (uint)outParams[18];
      currentVideoMethod = (byte)outParams[19];
    }
  }
}
