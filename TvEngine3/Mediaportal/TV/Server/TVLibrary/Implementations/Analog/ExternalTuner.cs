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
using System.Diagnostics;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog
{
  internal class ExternalTuner
  {
    #region variables

    private CaptureSourceVideo _externalInputSourceVideo;
    private CaptureSourceAudio _externalInputSourceAudio;
    private int _externalInputCountryId;
    private short _externalInputPhysicalChannelNumber;
    private string _externalTunerProgram = string.Empty;
    private string _externalTunerProgramArguments = string.Empty;

    #endregion

    public void ReloadConfiguration(AnalogTunerSettings configuration)
    {
      this.LogDebug("external tuner: reload configuration");

      _externalInputSourceVideo = (CaptureSourceVideo)configuration.ExternalInputSourceVideo;
      _externalInputSourceAudio = (CaptureSourceAudio)configuration.ExternalInputSourceAudio;
      _externalInputCountryId = configuration.ExternalInputCountryId;
      _externalInputPhysicalChannelNumber = (short)configuration.ExternalInputPhysicalChannelNumber;
      _externalTunerProgram = configuration.ExternalTunerProgram;
      _externalTunerProgramArguments = configuration.ExternalTunerProgramArguments;

      this.LogDebug("  external input...");
      this.LogDebug("    video source  = {0} [{1}]", _externalInputSourceVideo, (CaptureSourceVideo)configuration.SupportedVideoSources);
      this.LogDebug("    audio source  = {0} [{1}]", _externalInputSourceAudio, (CaptureSourceAudio)configuration.SupportedAudioSources);
      this.LogDebug("    country       = {0}", _externalInputCountryId);
      this.LogDebug("    phys. channel = {0}", _externalInputPhysicalChannelNumber);
      this.LogDebug("  external tuner...");
      this.LogDebug("    program       = {0}", _externalTunerProgram);
      this.LogDebug("    program args  = {0}", _externalTunerProgramArguments);
    }

    public IChannel Tune(IChannel channel)
    {
      // Channel or external input settings?
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (
        captureChannel == null ||
        (
          captureChannel.VideoSource != CaptureSourceVideo.TunerDefault &&
          captureChannel.AudioSource != CaptureSourceAudio.TunerDefault
        )
      )
      {
        return channel;
      }

      this.LogDebug("external tuner: using external input");
      IChannel tuneChannel;
      if (captureChannel.VideoSource == CaptureSourceVideo.TunerDefault && _externalInputSourceVideo == CaptureSourceVideo.Tuner)
      {
        ChannelAnalogTv externalAnalogTvChannel = new ChannelAnalogTv();
        externalAnalogTvChannel.MediaType = MediaType.Television;
        externalAnalogTvChannel.TunerSource = AnalogTunerSource.Cable;
        externalAnalogTvChannel.Country = CountryCollection.Instance.GetCountryById(_externalInputCountryId);
        externalAnalogTvChannel.PhysicalChannelNumber = _externalInputPhysicalChannelNumber;
        tuneChannel = externalAnalogTvChannel;
      }
      else
      {
        ChannelCapture externalCaptureChannel = new ChannelCapture();
        externalCaptureChannel.MediaType = MediaType.Television;
        if (_externalInputSourceVideo == CaptureSourceVideo.None)
        {
          externalCaptureChannel.MediaType = MediaType.Radio;
        }
        if (captureChannel.VideoSource == CaptureSourceVideo.TunerDefault)
        {
          externalCaptureChannel.VideoSource = _externalInputSourceVideo;
        }
        else
        {
          externalCaptureChannel.VideoSource = captureChannel.VideoSource;
        }
        if (captureChannel.AudioSource == CaptureSourceAudio.TunerDefault)
        {
          externalCaptureChannel.AudioSource = _externalInputSourceAudio;
        }
        else
        {
          externalCaptureChannel.AudioSource = captureChannel.AudioSource;
        }
        externalCaptureChannel.IsVcrSignal = captureChannel.IsVcrSignal;
        tuneChannel = externalCaptureChannel;
      }

      tuneChannel.Name = channel.Name;
      tuneChannel.Provider = channel.Provider;
      tuneChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
      tuneChannel.IsEncrypted = channel.IsEncrypted;
      tuneChannel.IsHighDefinition = channel.IsHighDefinition;
      tuneChannel.IsThreeDimensional = channel.IsThreeDimensional;

      // External tuner?
      if (!string.IsNullOrEmpty(_externalTunerProgram))
      {
        this.LogDebug("external tuner: using external tuner");
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = true;
        startInfo.ErrorDialog = false;
        startInfo.LoadUserProfile = false;
        startInfo.UseShellExecute = true;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = _externalTunerProgram;
        startInfo.Arguments = _externalTunerProgramArguments.Replace("%channel number%", channel.LogicalChannelNumber.ToString());
        try
        {
          Process p = Process.Start(startInfo);
          if (p.WaitForExit(20000))
          {
            this.LogDebug("external tuner: process exited with code {0}", p.ExitCode);
          }
          else
          {
            this.LogWarn("external tuner: process failed to exit within 10 seconds");
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "external tuner: process threw exception");
        }
      }
      return tuneChannel;
    }
  }
}