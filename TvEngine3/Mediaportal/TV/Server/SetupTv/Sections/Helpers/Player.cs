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
using System.Drawing;
using System.Windows.Forms;
using DirectShowLib;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class Player
  {
    private static readonly Guid CLSID_TS_READER = new Guid(0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f);

    private IFilterGraph2 _graph = null;
    private DsROTEntry _rotEntry = null;  // ROT = running object table
    private IBaseFilter _tsReader = null;
    private IBaseFilter _decoderVideo = null;
    private IBaseFilter _decoderAudio = null;
    private IBaseFilter _rendererVideo = null;
    private IBaseFilter _rendererAudio = null;
    private Form _form = null;

    public bool Play(string fileName, Form form)
    {
      this.LogInfo("player: play, file = {0}", fileName);
      _form = form;
      _form.ClientSize = new Size(480, 270);  // 16:9

      try
      {
        this.LogDebug("player: create graph");
        _graph = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graph);

        this.LogDebug("player: add TS reader");
        _tsReader = ComHelper.LoadComObjectFromFile("TsReader.ax", CLSID_TS_READER, typeof(IBaseFilter).GUID, true) as IBaseFilter;
        int hr = _graph.AddFilter(_tsReader, "MediaPortal TS Reader");
        TvExceptionDirectShowError.Throw(hr, "Failed to add TS reader to graph.");

        this.LogDebug("player: load file");
        IFileSourceFilter fileSource = _tsReader as IFileSourceFilter;
        if (fileSource == null)
        {
          this.LogError("player: failed to find file source interface on TS reader");
          return false;
        }
        hr = fileSource.Load(fileName, null);
        TvExceptionDirectShowError.Throw(hr, "Failed to load file \"{0}\" with TS reader.", fileName);

        // add the preferred decoders if they're installed
        this.LogDebug("player: add preferred decoders");
        try
        {
          Codec c = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecVideo", Codec.DEFAULT_VIDEO.Serialise()));
          if (c != null && c.ClassId != Guid.Empty)
          {
            _decoderVideo = AddFilterFromRegisteredClsid(_graph, c.ClassId, c.Name);
          }

          c = Codec.Deserialise(ServiceAgents.Instance.SettingServiceAgent.GetValue("previewCodecAudio", Codec.DEFAULT_AUDIO.Serialise()));
          if (c != null && c.ClassId != Guid.Empty)
          {
            _decoderAudio = AddFilterFromRegisteredClsid(_graph, c.ClassId, c.Name);
          }
        }
        catch
        {
          this.LogWarn("player: failed to add preferred decoders to graph, render will use decoders with highest merit");
        }

        this.LogDebug("player: add audio renderer");
        try
        {
          _rendererAudio = AddFilterFromRegisteredClsid(_graph, typeof(DSoundRender).GUID, "Default DirectSound Audio Renderer");
        }
        catch
        {
          this.LogWarn("player: failed to add default DirectSound audio renderer, render will use the audio renderer with highest merit");
        }

        this.LogDebug("player: add video renderer");
        try
        {
          if (Environment.OSVersion.Version.Major >= 6) // Vista or later
          {
            try
            {
              _rendererVideo = AddFilterFromRegisteredClsid(_graph, typeof(EnhancedVideoRenderer).GUID, "Enhanced Video Renderer");
              this.LogDebug("player: using enhanced video renderer");
            }
            catch
            {
              // It *should* be possible to use the EVR on Vista or newer.
              this.LogWarn("player: failed to use enhanced video render");
            }
          }

          if (_rendererVideo == null)
          {
            _rendererVideo = AddFilterFromRegisteredClsid(_graph, typeof(VideoMixingRenderer9).GUID, "Video Mixing Renderer 9");
            this.LogDebug("player: using video mixing renderer 9");
          }
        }
        catch
        {
          this.LogWarn("player: failed to add video renderer, render will use the video renderer with highest merit");
        }

        this.LogDebug("player: render TS reader outputs");
        IEnumPins pinEnum;
        hr = _tsReader.EnumPins(out pinEnum);
        TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for TS reader.");
        try
        {
          IPin[] pins = new IPin[2];
          int pinCount = 0;
          while (pinEnum.Next(1, pins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
          {
            IPin pin = pins[0];
            try
            {
              string pinName = GetPinName(pin);
              PinDirection direction;
              hr = pin.QueryDirection(out direction);
              TvExceptionDirectShowError.Throw(hr, "Failed to query direction for TS reader pin \"{0}\".", pinName);
              if (direction == PinDirection.Output && !string.Equals(pinName, "Subtitle"))
              {
                hr = _graph.Render(pin);
                if (hr != (int)NativeMethods.HResult.S_OK)
                {
                  this.LogWarn("player: failed to render TS reader pin \"{0}\", hr = 0x{1:x}", pinName, hr);
                }
              }
            }
            finally
            {
              Release.ComObject("player TS reader pin", ref pin);
            }
          }
        }
        finally
        {
          Release.ComObject("player TS reader pin enumerator", ref pinEnum);
        }

        DebugGraphStructure(_graph);

        this.LogDebug("player: configure video window");
        IVideoWindow videoWindow = _graph as IVideoWindow;
        if (videoWindow == null)
        {
          this.LogError("player: failed to find video window interface on graph");
          return false;
        }

        // These calls return VFW_E_NOT_CONNECTED when the video renderer is
        // not connected. Don't bother error checking.
        int x = videoWindow.put_Visible(OABool.True);
        x = videoWindow.put_Owner(form.Handle);
        x = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
        x = videoWindow.put_MessageDrain(form.Handle);
        Rectangle r = form.ClientRectangle;
        x = videoWindow.SetWindowPosition(r.X, r.Y, r.Width, r.Height);

        this.LogDebug("player: run graph");
        IMediaControl mediaControl = _graph as IMediaControl;
        if (mediaControl == null)
        {
          this.LogError("player: failed to find media control interface on graph");
          return false;
        }
        hr = mediaControl.Run();
        // Run() can return S_FALSE in certain non-error conditions.
        if (hr != (int)NativeMethods.HResult.S_OK && hr != (int)NativeMethods.HResult.S_FALSE)
        {
          TvExceptionDirectShowError.Throw(hr, "Failed to start graph.");
        }
        this.LogInfo("player: playing successfully");
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "player: failed to play file \"{0}\"", fileName);
      }
      return false;
    }

    public void Stop()
    {
      this.LogInfo("player: stop");

      if (_graph == null)
      {
        return;
      }

      IVideoWindow videoWindow = _graph as IVideoWindow;
      if (videoWindow != null)
      {
        videoWindow.put_Visible(OABool.False);
      }

      int hr = (int)NativeMethods.HResult.E_FAIL;
      IMediaControl mediaControl = _graph as IMediaControl;
      if (mediaControl != null)
      {
        hr = mediaControl.Stop();
      }
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogWarn("player: failed to stop graph, hr = 0x{0:x}", hr);
      }

      if (_rendererVideo != null)
      {
        _graph.RemoveFilter(_rendererVideo);
        Release.ComObject("player video renderer", ref _rendererVideo);
      }
      if (_rendererAudio != null)
      {
        _graph.RemoveFilter(_rendererAudio);
        Release.ComObject("player audio renderer", ref _rendererAudio);
      }
      if (_decoderVideo != null)
      {
        _graph.RemoveFilter(_decoderVideo);
        Release.ComObject("player video decoder", ref _decoderVideo);
      }
      if (_decoderAudio != null)
      {
        _graph.RemoveFilter(_decoderAudio);
        Release.ComObject("player audio decoder", ref _decoderAudio);
      }
      if (_tsReader != null)
      {
        _graph.RemoveFilter(_tsReader);
        Release.ComObject("player TS reader", ref _tsReader);
      }

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }

      Release.ComObject("player graph", ref _graph);
    }

    public void ResizeToParent()
    {
      IVideoWindow videoWindow = _graph as IVideoWindow;
      if (videoWindow != null && _form != null)
      {
        Rectangle r = _form.ClientRectangle;
        videoWindow.SetWindowPosition(r.X, r.Y, r.Width, r.Height);
      }
    }

    #region utility functions

    /// <summary>
    /// Add a filter to a DirectShow graph using its CLSID.
    /// </summary>
    /// <remarks>
    /// The filter class must be registered with Windows using regsvr32.
    /// You can use <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> to check if the CLSID is valid before calling this method.
    /// </remarks>
    /// <param name="graph">The graph.</param>
    /// <param name="clsid">The class ID (CLSID) for the filter class. The class must expose the IBaseFilter interface.</param>
    /// <param name="name">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    private static IBaseFilter AddFilterFromRegisteredClsid(IFilterGraph2 graph, Guid clsid, string name)
    {
      IBaseFilter filter = null;
      try
      {
        Type type = Type.GetTypeFromCLSID(clsid);
        filter = Activator.CreateInstance(type) as IBaseFilter;

        int hr = graph.AddFilter(filter, name);
        TvExceptionDirectShowError.Throw(hr, "Failed to add the filter to the graph.");
      }
      catch
      {
        Release.ComObject("registered filter", ref filter);
        throw;
      }

      return filter;
    }

    /// <summary>
    /// Get the name of a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>the name of the filter</returns>
    private static string GetFilterName(IBaseFilter filter)
    {
      FilterInfo filterInfo;
      int hr = filter.QueryFilterInfo(out filterInfo);
      TvExceptionDirectShowError.Throw(hr, "Failed to query filter information.");
      Release.FilterInfo(ref filterInfo);
      return filterInfo.achName;
    }

    /// <summary>
    /// Get the name of a pin.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns>the name of the pin</returns>
    private static string GetPinName(IPin pin)
    {
      PinInfo pinInfo;
      int hr = pin.QueryPinInfo(out pinInfo);
      TvExceptionDirectShowError.Throw(hr, "Failed to query pin information.");
      Release.ComObject("player pin name filter", ref pinInfo.filter);
      return pinInfo.name;
    }

    private static void DebugGraphStructure(IFilterGraph2 graph)
    {
      Log.Debug("player: graph structure...");
      IEnumFilters filterEnum;
      int hr = graph.EnumFilters(out filterEnum);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain filter enumerator for graph.");
      try
      {
        IBaseFilter[] filters = new IBaseFilter[2];
        int filterCount = 0;
        while (filterEnum.Next(1, filters, out filterCount) == (int)NativeMethods.HResult.S_OK && filterCount == 1)
        {
          IBaseFilter filter = filters[0];
          try
          {
            string filterName = GetFilterName(filter);
            Log.Debug("  filter {0}...", filterName);

            IEnumPins pinEnum;
            hr = filter.EnumPins(out pinEnum);
            TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for filter \"{0}\".", filterName);
            try
            {
              IPin[] pins = new IPin[2];
              int pinCount = 0;
              while (pinEnum.Next(1, pins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
              {
                IPin pin = pins[0];
                try
                {
                  string pinName = GetPinName(pin);
                  PinDirection direction;
                  hr = pin.QueryDirection(out direction);
                  TvExceptionDirectShowError.Throw(hr, "Failed to query direction for filter \"{0}\" pin \"{1}\".", filterName, pinName);
                  if (direction == PinDirection.Input)
                  {
                    Log.Debug("    input pin {0}", pinName);
                    continue;
                  }

                  IPin connectedPin;
                  hr = pin.ConnectedTo(out connectedPin);
                  if (hr != (int)NativeMethods.HResult.S_OK || connectedPin == null)
                  {
                    Log.Debug("    output pin {0}, not connected", pinName);
                    continue;
                  }

                  try
                  {
                    PinInfo pinInfo;
                    hr = connectedPin.QueryPinInfo(out pinInfo);
                    TvExceptionDirectShowError.Throw(hr, "Failed to query pin information for filter \"{0}\" pin \"{1}\" connected pin.", filterName, pinName);
                    try
                    {
                      Log.Debug("    output pin {0}, connected to filter \"{1}\" pin \"{2}\"", pinName, GetFilterName(pinInfo.filter), GetPinName(connectedPin));
                    }
                    finally
                    {
                      Release.PinInfo(ref pinInfo);
                    }
                  }
                  finally
                  {
                    Release.ComObject("player graph filter connected pin", ref connectedPin);
                  }
                }
                finally
                {
                  Release.ComObject("player graph filter pin", ref pin);
                }
              }
            }
            finally
            {
              Release.ComObject("player graph filter pin enumerator", ref pinEnum);
            }
          }
          finally
          {
            Release.ComObject("player graph filter", ref filter);
          }
        }
      }
      finally
      {
        Release.ComObject("player graph filter enumerator", ref filterEnum);
      }
    }

    #endregion
  }
}