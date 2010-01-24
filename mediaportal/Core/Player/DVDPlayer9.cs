#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;
using DirectShowLib.Dvd;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class DVDPlayer9 : DVDPlayer
  {
    private const uint VFW_E_DVD_DECNOTENOUGH = 0x8004027B;
    private const uint VFW_E_DVD_RENDERFAIL = 0x8004027A;

    private VMR9Util _vmr9 = null;

    private void RenderOutPins(IBaseFilter baseFilter)
    {
      if (baseFilter == null)
        return;

      int fetched;
      IEnumPins pinEnum;
      int hr = baseFilter.EnumPins(out pinEnum);
      if (hr == 0 && pinEnum != null)
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        while (pinEnum.Next(1, pins, out fetched) == 0)
        {
          if (fetched > 0)
          {
            try
            {
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                IPin pOutPin;
                hr = pins[0].ConnectedTo(out pOutPin);
                if (pOutPin != null)
                  DirectShowUtil.ReleaseComObject(pOutPin);
                if (hr == DsResults.E_NotConnected)
                {
                  FilterInfo i;
                  PinInfo pinInfo = new PinInfo();
                  if (baseFilter.QueryFilterInfo(out i) == 0 && pins[0].QueryPinInfo(out pinInfo) == 0)
                  {
                    Log.Debug("Filter: {0} - pin connect: {1}", i.achName, pinInfo.name);

                  }
                  DirectShowUtil.ReleaseComObject(i.pGraph);
                  DirectShowUtil.ReleaseComObject(pinInfo.filter);
                  hr = _graphBuilder.Render(pins[0]);
                  DsError.ThrowExceptionForHR(hr);
                }
              }
            }
            finally { }
          }
          else
          {
            break;
          }
        }
        DirectShowUtil.ReleaseComObject(pinEnum);
      }      
    }

    private void DoGraph(IGraphBuilder graphBuilder, IBaseFilter baseFilter)
    {
      if (graphBuilder == null)
        return;
      if (baseFilter != null)
        RenderOutPins(baseFilter);
      
      IEnumFilters enumFilters;
      graphBuilder.EnumFilters(out enumFilters);
      int fetched;
      IBaseFilter[] filters = new IBaseFilter[1];
      while (enumFilters.Next(1, filters, out fetched) == 0)
      {
        if (fetched > 0 && filters[0] != baseFilter)
        {
          RenderOutPins(filters[0]);
        }
        else
        {
          break;
        }
      }
      DirectShowUtil.ReleaseComObject(enumFilters);      
    }

    /// <summary> create the used COM components and get the interfaces. </summary>    
    protected override bool GetInterfaces(string path)
    {
      int hr;
      object comobj = null;
      _freeNavigator = true;
      _dvdInfo = null;
      _dvdCtrl = null;
      _videoWin = null;
      string dvdDNavigator = "DVD Navigator";
      string aspectRatio = "";
      string displayMode = "";
      bool turnoffDXVA = false;
      int codecValue = 0;
      string codecType = "";

      using (Settings xmlreader = new MPSettings())
      {
        dvdDNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");

        if (dvdDNavigator.ToLower().Contains("cyberlink dvd navigator"))
        {
          _cyberlinkDVDNavigator = true;
        }

        aspectRatio = xmlreader.GetValueAsString("dvdplayer", "armode", "").ToLower();
        if (aspectRatio == "crop")
        {
          arMode = AspectRatioMode.Crop;
        }
        else if (aspectRatio == "letterbox")
        {
          arMode = AspectRatioMode.LetterBox;
        }
        else if (aspectRatio == "stretch")
        {
          arMode = AspectRatioMode.Stretched;
        }
        else if (aspectRatio == "follow stream")
        {
          arMode = AspectRatioMode.StretchedAsPrimary;
        }

        displayMode = xmlreader.GetValueAsString("dvdplayer", "displaymode", "").ToLower();
        if (displayMode == "default")
        {
          _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
        }
        else if (displayMode == "16:9")
        {
          _videoPref = DvdPreferredDisplayMode.Display16x9;
        }
        else if (displayMode == "4:3 pan scan")
        {
          _videoPref = DvdPreferredDisplayMode.Display4x3PanScanPreferred;
        }
        else if (displayMode == "4:3 letterbox")
        {
          _videoPref = DvdPreferredDisplayMode.Display4x3LetterBoxPreferred;
        }

        turnoffDXVA = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", true);
        Log.Info("DVDPlayer9:Turn off DXVA value = {0}", turnoffDXVA);
        if (turnoffDXVA == true)
        {
          codecType = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
          Log.Info("DVDPlayer9:Video Decoder = {0}", codecType);
          if (codecType == "InterVideo Video Decoder")
          {
            codecValue = xmlreader.GetValueAsInt("videocodec", "intervideo", 1);
            if (codecValue == 1)
            {
              Log.Info("DVDPlayer9:Turning InterVideo DXVA off");
              using (
                RegistryKey subkey =
                  Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\VideoDec\MediaPortal"))
              {
                subkey.SetValue("DXVA", 0);
              }
            }
            if (codecValue == 0)
            {
              Log.Info("DVDPlayer9:InterVideo DXVA already off");
            }
          }
          if (codecType.StartsWith("CyberLink Video/SP Decoder"))
          {
            codecValue = xmlreader.GetValueAsInt("videocodec", "cyberlink", 1);
            if (codecValue == 1)
            {
              Log.Info("DVDPlayer9:Turning CyberLink DXVA off");
              using (
                RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLVSD\MediaPortal"))
              {
                subkey.SetValue("UIUseHVA", 0);
              }
            }
            if (codecValue == 0)
            {
              Log.Info("DVDPlayer9:CyberLink DXVA already off");
            }
          }
          if (codecType == "NVIDIA Video Decoder")
          {
            codecValue = xmlreader.GetValueAsInt("videocodec", "nvidia", 1);
            if (codecValue == 1)
            {
              Log.Info("DVDPlayer9:Turning NVIDIA DXVA off");
              using (
                RegistryKey subkey = Registry.LocalMachine.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Video"))
              {
                subkey.SetValue("EnableDXVA", 0);
              }
            }
            if (codecValue == 0)
            {
              Log.Info("DVDPlayer9:NVIDIA DXVA already off");
            }
          }
        }
      }

      Log.Info("DVDPlayer9:Enabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      GUIWindowManager.SendMessage(msg);

      try
      {        
        _dvdGraph = (IDvdGraphBuilder)new DvdGraphBuilder();

        hr = _dvdGraph.GetFiltergraph(out _graphBuilder);
        DsError.ThrowExceptionForHR(hr);

        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);
        
        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);

        try
        {
          Log.Info("DVDPlayer9:Add {0}", dvdDNavigator);
          _dvdbasefilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, dvdDNavigator);
          if (_dvdbasefilter != null)
          {
            AddPreferedCodecs(_graphBuilder);
            _dvdCtrl = (IDvdControl2)_dvdbasefilter;
            if (_dvdCtrl != null)
            {
              _dvdInfo = (IDvdInfo2)_dvdbasefilter;
              if (!String.IsNullOrEmpty(path))
              {
                  hr = _dvdCtrl.SetDVDDirectory(path);
                  DsError.ThrowExceptionForHR(hr);
              }
              _dvdCtrl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true); // use new HMSF timecode format
              _dvdCtrl.SetOption(DvdOptionFlag.ResetOnStop, false);

              DoGraph(_graphBuilder, _dvdbasefilter);
    
              _freeNavigator = false;
            }        
          }
        }
        catch (Exception ex)
        {
          Log.Error("DVDPlayer9:Add {0} as navigator failed: {1}", dvdDNavigator, ex.Message);
        }
        
        if (_dvdInfo == null)
        {
          Log.Info("Dvdplayer9:Volume rendered, get interfaces");
          hr = _dvdGraph.GetDvdInterface(typeof(IDvdInfo2).GUID, out comobj);
          DsError.ThrowExceptionForHR(hr);
          _dvdInfo = (IDvdInfo2)comobj;
          comobj = null;
        }

        if (_dvdCtrl == null)
        {
          Log.Info("Dvdplayer9:Get IDvdControl2");
          hr = _dvdGraph.GetDvdInterface(typeof(IDvdControl2).GUID, out comobj);
          DsError.ThrowExceptionForHR(hr);
          _dvdCtrl = (IDvdControl2)comobj;
          comobj = null;
          if (_dvdCtrl != null)
          {
            Log.Info("Dvdplayer9:Get IDvdControl2");
          }
          else
          {
            Log.Error("Dvdplayer9:Failed to get IDvdControl2");
          }
        }        

        // disable Closed Captions!
        IBaseFilter basefilter;
        _graphBuilder.FindFilterByName("Line 21 Decoder", out basefilter);
        if (basefilter == null)
        {
          _graphBuilder.FindFilterByName("Line21 Decoder", out basefilter);
        }
        if (basefilter == null)
        {
          _graphBuilder.FindFilterByName("Line 21 Decoder 2", out basefilter);
        }
        if (basefilter != null)
        {
          Log.Info("Dvdplayer9:Disabling Line21 Decoder (Closed Captions)");        
          _line21Decoder = (IAMLine21Decoder)basefilter;
          if (_line21Decoder != null)
          {
            AMLine21CCState state = AMLine21CCState.Off;
            hr = _line21Decoder.SetServiceState(state);
            if (hr == 0)
            {
              Log.Info("DVDPlayer9:Closed Captions disabled");
            }
            else
            {
              Log.Info("DVDPlayer9:Failed to disable Closed Captions");
            }
          }
        }

        if (!_vmr9.IsVMR9Connected)
        {
          Log.Info("DVDPlayer9:Failed vmr9 not connected");
          _mediaCtrl = null;
          Cleanup();
          return false;
        }
        
        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _basicAudio = (IBasicAudio)_graphBuilder;
        _mediaPos = (IMediaPosition)_graphBuilder;
        _basicVideo = (IBasicVideo2)_graphBuilder;

        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;

        DirectShowUtil.SetARMode(_graphBuilder, arMode);        
        _vmr9.SetDeinterlaceMode();
        _vmr9.Enable(true);



        Log.Info("Dvdplayer9:Graph created");
        _started = true;
        return true;
      }
      catch (Exception)
      {
        CloseInterfaces();
        return false;
      }
      finally {}
    }
    
    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    private void Cleanup()
    {
      if (_graphBuilder == null)
      {
        return;
      }
      int hr;
      using (Settings xmlreader = new MPSettings())
      {
        int codecValue = 0;
        string codecType = "";
        codecType = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
        if (codecType == "InterVideo Video Decoder")
        {
          codecValue = xmlreader.GetValueAsInt("videocodec", "intervideo", 1);
          Log.Info("DVDPlayer9:Resetting InterVideo DXVA to {0}", codecValue);
          using (
            RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\VideoDec\MediaPortal"))
          {
            subkey.SetValue("DXVA", codecValue);
          }
        }
        if (codecType.StartsWith("CyberLink Video/SP Decoder"))
        {
          codecValue = xmlreader.GetValueAsInt("videocodec", "cyberlink", 1);
          Log.Info("DVDPlayer9:Resetting CyberLink DXVA to {0}", codecValue);
          using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLVSD\MediaPortal"))
          {
            subkey.SetValue("UIUseHVA", codecValue);
          }
        }
        if (codecType == "NVIDIA Video Decoder")
        {
          codecValue = xmlreader.GetValueAsInt("videocodec", "nvidia", 1);
          Log.Info("DVDPlayer9:Resetting NVIDIA DXVA to {0}", codecValue);
          using (RegistryKey subkey = Registry.LocalMachine.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Video"))
          {
            subkey.SetValue("EnableDXVA", codecValue);
          }
        }
      }
      try
      {
        Log.Info("DVDPlayer9: cleanup DShow graph");

        if (_mediaCtrl != null)
        {
          int counter = 0;
          FilterState state;
          hr = _mediaCtrl.Stop();
          hr = _mediaCtrl.GetState(10, out state);
          while (state != FilterState.Stopped || GUIGraphicsContext.InVmr9Render)
          {
            System.Threading.Thread.Sleep(100);
            hr = _mediaCtrl.GetState(10, out state);
            counter++;
            if (counter >= 30)
            {
              if (state != FilterState.Stopped)
                Log.Debug("DVDPlayer9: graph still running");
              if (GUIGraphicsContext.InVmr9Render)
                Log.Debug("DVDPlayer9: in renderer");
              break;
            }
          }
          _mediaCtrl = null;
        }

        _state = PlayState.Stopped;
        VMR9Util.g_vmr9.EVRSetDVDMenuState(false);

        _visible = false;
        _mediaEvt = null;
        _dvdCtrl = null;
        _dvdInfo = null;
        _basicVideo = null;
        _basicAudio = null;
        _mediaPos = null;
        _videoWin = null;
        _pendingCmd = false;

        if (_cmdOption != null)
        {
          DirectShowUtil.ReleaseComObject(_cmdOption);
          _cmdOption = null;
        }

        if (_dvdbasefilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_dvdbasefilter)) > 0) ;
          _dvdbasefilter = null;
        }

        if (_dvdGraph != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_dvdGraph)) > 0) ;
          _dvdGraph = null;
        }

        if (_line21Decoder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_line21Decoder)) > 0) ;
          _line21Decoder = null;
        }

        if (_vmr9 != null)
        {
          _vmr9.Enable(false);
          _vmr9.Dispose();
          _vmr9 = null;
        }

        if (_graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(_graphBuilder);
          // _rotEntry has a reference to _graphBuilder (see _rotEntry ctor)
          // so, release of _rotEntry must be before _graphBuilder is released
          if (_rotEntry != null)
          {
            _rotEntry.Dispose();
            _rotEntry = null;
          }
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          _graphBuilder = null;
        }

        _state = PlayState.Init;
        GUIGraphicsContext.form.Invalidate(true);
        //GUIGraphicsContext.form.Activate();
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer9: Exception while cleanuping DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      Log.Info("DVDPlayer9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);
    }

    protected override void OnProcess()
    {
      //_sourceRectangle=m_scene.SourceRect;
      //_videoRectangle=m_scene.DestRect;
    }

    protected override void Repaint()
    {
      if (_vmr9 == null)
      {
        return;
      }
      _vmr9.Repaint();
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }
      if (!_started)
      {
        return;
      }
      // BAV, 02.03.08: checking GUIGraphicsContext.InVmr9Render makes no sense here, there are no changes in render items
      //                removing this should solve 1 min delays in skip steps
      //if (GUIGraphicsContext.InVmr9Render) return;
      HandleMouseMessages();
      OnProcess();
    }

    private void HandleMouseMessages()
    {
      if (!GUIGraphicsContext.IsFullScreenVideo)
      {
        return;
      }
      //if (GUIGraphicsContext.Vmr9Active) return;
      try
      {
        Point pt;
        foreach (Message m in _mouseMsg)
        {
          long lParam = m.LParam.ToInt32();
          double x = (double)(lParam & 0xffff);
          double y = (double)(lParam >> 16);
          double arx, ary;

          Rectangle src, dst;

          // Transform back to original window position / aspect ratio
          // in order to know the intended position
          _vmr9.GetVideoWindows(out src, out dst);

          x -= dst.X;
          y -= dst.Y;
          arx = (double)dst.Width / (double)src.Width;
          ary = (double)dst.Height / (double)src.Height;
          x /= arx;
          y /= ary;

          pt = new Point((int)x, (int)y);

          if (m.Msg == WM_MOUSEMOVE)
          {
            // Select the button at the current position, if it exists
            _dvdCtrl.SelectAtPosition(pt);
          }

          if (m.Msg == WM_LBUTTONUP)
          {
            // Highlight the button at the current position, if it exists
            _dvdCtrl.ActivateAtPosition(pt);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer9:HandleMouseMessages() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      _mouseMsg.Clear();
    }
  }
}