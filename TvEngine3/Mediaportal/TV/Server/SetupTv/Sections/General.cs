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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class General : SectionSettings
  {
    private enum ServicePriority
    {
      [Description("Real-time")]
      RealTime = (int)ProcessPriorityClass.RealTime,
      [Description("High")]
      High = (int)ProcessPriorityClass.High,
      [Description("Above Normal")]
      AboveNormal = (int)ProcessPriorityClass.AboveNormal,
      [Description("Normal")]
      Normal = (int)ProcessPriorityClass.Normal,
      [Description("Below Normal")]
      BelowNormal = (int)ProcessPriorityClass.BelowNormal,
      [Description("Idle")]
      Idle = (int)ProcessPriorityClass.Idle
    }

    #region constants

    private const int MINIMUM_STREAM_TUNER_PORT_COUNT = 4;

    private static readonly Guid MEDIA_SUB_TYPE_AVC = new Guid(0x31435641, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    private static readonly Guid MEDIA_SUB_TYPE_LATM_AAC = new Guid(0x00001ff, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    private static readonly Guid MEDIA_SUB_TYPE_DOLBY_DIGITAL = new Guid(0xe06d802c, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
    private static readonly Guid MEDIA_SUB_TYPE_DOLBY_DIGITAL_PLUS = new Guid(0xa7fb87af, 0x2d02, 0x42fb, 0xa4, 0xd4, 0x05, 0xcd, 0x93, 0x84, 0x3b, 0xdd);

    #endregion

    public General()
      : base("General")
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("general: activating");

      // First activation.
      if (comboBoxServicePriority.Items.Count == 0)
      {
        comboBoxServicePriority.Items.AddRange(typeof(ServicePriority).GetDescriptions());

        comboBoxPreviewCodecVideo.Items.Add(new Codec("Automatic", Guid.Empty));
        comboBoxPreviewCodecVideo.Items.AddRange(GetCodecs(MediaType.Video, new Guid[3] { MediaSubType.Mpeg2Video, MEDIA_SUB_TYPE_AVC, MediaSubType.H264 }));

        comboBoxPreviewCodecAudio.Items.Add(new Codec("Automatic", Guid.Empty));
        comboBoxPreviewCodecAudio.Items.AddRange(GetCodecs(MediaType.Audio, new Guid[4] { MediaSubType.Mpeg2Audio, MEDIA_SUB_TYPE_LATM_AAC, MEDIA_SUB_TYPE_DOLBY_DIGITAL, MEDIA_SUB_TYPE_DOLBY_DIGITAL_PLUS }));
      }

      comboBoxServicePriority.SelectedItem = ((ServicePriority)ServiceAgents.Instance.SettingServiceAgent.GetValue("servicePriority", (int)ServicePriority.Normal)).GetDescription();
      numericUpDownStreamTunerPortMinimum.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("streamTunersPortMinimum", 49152);
      numericUpDownStreamTunerPortMaximum.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("streamTunersPortMaximum", 65535);

      Codec codecVideo = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecVideo", Codec.DEFAULT_VIDEO.Serialise()));
      if (codecVideo != null)
      {
        comboBoxPreviewCodecVideo.SelectedItem = codecVideo;
      }
      if (comboBoxPreviewCodecVideo.SelectedItem == null)
      {
        comboBoxPreviewCodecVideo.SelectedIndex = 0;
      }

      Codec codecAudio = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecAudio", Codec.DEFAULT_AUDIO.Serialise()));
      if (codecAudio != null)
      {
        comboBoxPreviewCodecAudio.SelectedItem = codecAudio;
      }
      if (comboBoxPreviewCodecAudio.SelectedItem == null)
      {
        comboBoxPreviewCodecAudio.SelectedIndex = 0;
      }

      numericUpDownTimeLimitSignalLock.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitSignalLock", 2500);
      numericUpDownTimeLimitReceiveStreamInfo.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitReceiveStreamInfo", 5000);
      numericUpDownTimeLimitReceiveVideoAudio.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("timeLimitReceiveVideoAudio", 5000);

      DebugSettings();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("general: deactivating");

      int servicePriority = Convert.ToInt32(typeof(ServicePriority).GetEnumFromDescription((string)comboBoxServicePriority.SelectedItem));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("servicePriority", servicePriority);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("streamTunersPortMinimum", (int)numericUpDownStreamTunerPortMinimum.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("streamTunersPortMaximum", (int)numericUpDownStreamTunerPortMaximum.Value);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("previewCodecVideo", ((Codec)comboBoxPreviewCodecVideo.SelectedItem).Serialise());
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("previewCodecAudio", ((Codec)comboBoxPreviewCodecAudio.SelectedItem).Serialise());

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitSignalLock", (int)numericUpDownTimeLimitSignalLock.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitReceiveStreamInfo", (int)numericUpDownTimeLimitReceiveStreamInfo.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("timeLimitReceiveVideoAudio", (int)numericUpDownTimeLimitReceiveVideoAudio.Value);

      DebugSettings();

      // TODO trigger server-side config reloading for service priority and time limits (and stream tuner port range???)

      base.OnSectionDeActivated();
    }

    private void DebugSettings()
    {
      this.LogDebug("general: settings...");
      this.LogDebug("  service priority      = {0}", comboBoxServicePriority.SelectedItem);
      this.LogDebug("  stream tuner ports...");
      this.LogDebug("    minimum             = {0}", numericUpDownStreamTunerPortMinimum.Value);
      this.LogDebug("    maximum             = {0}", numericUpDownStreamTunerPortMaximum.Value);
      this.LogDebug("  preview codecs...");
      Codec c = (Codec)comboBoxPreviewCodecVideo.SelectedItem;
      this.LogDebug("    video               = {0} ({1})", c.Name, c.ClassId);
      c = (Codec)comboBoxPreviewCodecAudio.SelectedItem;
      this.LogDebug("    audio               = {0} ({1})", c.Name, c.ClassId);
      this.LogDebug("  time limits...");
      this.LogDebug("    signal lock         = {0} ms", numericUpDownTimeLimitSignalLock.Value);
      this.LogDebug("    receive stream info = {0} ms", numericUpDownTimeLimitReceiveStreamInfo.Value);
      this.LogDebug("    receive video/audio = {0} ms", numericUpDownTimeLimitReceiveVideoAudio.Value);
    }

    private void numericUpDownStreamTunerPortMinimum_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownStreamTunerPortMinimum.Value + MINIMUM_STREAM_TUNER_PORT_COUNT > numericUpDownStreamTunerPortMaximum.Value)
      {
        if (numericUpDownStreamTunerPortMinimum.Value + MINIMUM_STREAM_TUNER_PORT_COUNT - 1 > numericUpDownStreamTunerPortMaximum.Maximum)
        {
          numericUpDownStreamTunerPortMinimum.Value = numericUpDownStreamTunerPortMaximum.Value - MINIMUM_STREAM_TUNER_PORT_COUNT + 1;
          numericUpDownStreamTunerPortMaximum.Value = numericUpDownStreamTunerPortMaximum.Maximum;
        }
        else
        {
          numericUpDownStreamTunerPortMaximum.Value = numericUpDownStreamTunerPortMinimum.Value + MINIMUM_STREAM_TUNER_PORT_COUNT - 1;
        }
      }
    }

    private void numericUpDownStreamTunerPortMaximum_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownStreamTunerPortMinimum.Value + MINIMUM_STREAM_TUNER_PORT_COUNT > numericUpDownStreamTunerPortMaximum.Value)
      {
        if (numericUpDownStreamTunerPortMaximum.Value - MINIMUM_STREAM_TUNER_PORT_COUNT + 1 < numericUpDownStreamTunerPortMinimum.Minimum)
        {
          numericUpDownStreamTunerPortMinimum.Value = numericUpDownStreamTunerPortMinimum.Minimum;
          numericUpDownStreamTunerPortMaximum.Value = numericUpDownStreamTunerPortMinimum.Value + MINIMUM_STREAM_TUNER_PORT_COUNT - 1;
        }
        else
        {
          numericUpDownStreamTunerPortMinimum.Value = numericUpDownStreamTunerPortMaximum.Value - MINIMUM_STREAM_TUNER_PORT_COUNT + 1;
        }
      }
    }

    private static Codec[] GetCodecs(Guid mediaType, Guid[] mediaSubTypes)
    {
      List<Codec> codecs = new List<Codec>();
      Guid[] types = new Guid[mediaSubTypes.Length * 2];
      for (int i = 0; i < mediaSubTypes.Length; i++)
      {
        types[i * 2] = mediaType;
        types[(i * 2) + 1] = mediaSubTypes[i];
      }

      HashSet<Guid> seenClsids = new HashSet<Guid>();
      IFilterMapper2 mapper = null;
      try
      {
        mapper = (IFilterMapper2)new FilterMapper2();
        if (mapper == null)
        {
          return codecs.ToArray();
        }

        IEnumMoniker enumMoniker = null;
        int hr = mapper.EnumMatchingFilters(
          out enumMoniker,
          0,                // flags - must be 0
          true,             // exact match
          (Merit)0x080001,  // merit - match MediaPortal
          true,             // input needed
          mediaSubTypes.Length,
          types,
          null,             // input pin medium
          null,             // input pin category
          false,            // render
          true,             // output needed
          0,                // output pin media type count
          new Guid[0],      // output pin media types
          null,             // output pin medium
          null);            // output pin category

        TvExceptionDirectShowError.Throw(hr, "Failed to enumerate filters.");
        try
        {
          while (true)
          {
            IMoniker[] monikers = new IMoniker[1];
            hr = enumMoniker.Next(1, monikers, IntPtr.Zero);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              break;
            }

            if (monikers[0] == null)
            {
              continue;
            }
            using (DsDevice d = new DsDevice(monikers[0]))
            {
              string name = d.Name;
              if (!string.IsNullOrEmpty(name))
              {
                string lowerName = name.ToLowerInvariant();
                if (
                  !lowerName.Contains("adjust") &&
                  !lowerName.Contains("encoder") &&
                  !lowerName.Contains("multiplexer") &&
                  !lowerName.Contains("muxer") &&
                  !lowerName.Contains("splicer") &&
                  !lowerName.Contains("transformer")
                )
                {
                  Guid clsid = Guid.Empty;
                  object o = d.GetPropBagValue("CLSID");  // Note: CLSID property bag value is different to ClassId property
                  if (o != null)
                  {
                    try
                    {
                      clsid = new Guid(o.ToString());
                    }
                    catch (Exception ex)
                    {
                      Log.Warn(ex, "general: failed to get CLSID for codec, name = {0}", name);
                      clsid = Guid.Empty;
                    }
                  }
                  if (clsid != Guid.Empty && !seenClsids.Contains(clsid))
                  {
                    codecs.Add(new Codec(name, clsid));
                    seenClsids.Add(clsid);
                  }
                }
              }
              d.Dispose();
            }
          }
        }
        finally
        {
          Release.ComObject("moniker enumerator", ref enumMoniker);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "general: failed to get codec list, media type = {0}", mediaType);
      }
      finally
      {
        Release.ComObject("filter mapper", ref mapper);
      }

      codecs.Sort();
      return codecs.ToArray();
    }
  }
}