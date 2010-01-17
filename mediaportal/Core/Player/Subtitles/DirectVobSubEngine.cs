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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MediaPortal.Player.Subtitles
{
  public class DirectVobSubEngine : SubSettings, ISubEngine
  {
    protected IDirectVobSub vobSub = null;
    protected IAMStreamSelect embeddedSelector;

    private List<int> intSubs = new List<int>();
    private List<string> intNames = new List<string>();
    private int extCount;
    private int current;

    #region ISubEngine Members

    public bool LoadSubtitles(IGraphBuilder graphBuilder, string filename)
    {
      FreeSubtitles();
      LoadSettings();

      vobSub = (IDirectVobSub)DirectVobSubUtil.AddToGraph(graphBuilder);
      if (vobSub == null)
        return false;

      { //set style
        Log.Debug("VideoPlayerVMR9: Setting DirectVobsub parameters");
        LOGFONT logFont = new LOGFONT();
        int txtcolor;
        bool fShadow, fOutLine, fAdvancedRenderer = false;
        int size = Marshal.SizeOf(typeof(LOGFONT));
        vobSub.get_TextSettings(logFont, size, out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);
        FontStyle fontStyle = defStyle.fontIsBold ? FontStyle.Regular : FontStyle.Bold;
        Font Subfont = new Font(defStyle.fontName, defStyle.fontSize, fontStyle, GraphicsUnit.Point, (byte)defStyle.fontCharset);
        Subfont.ToLogFont(logFont);
        fShadow = defStyle.shadow > 0;
        fOutLine = defStyle.isBorderOutline;        
        vobSub.put_TextSettings(logFont, size, defStyle.fontColor, fShadow, fOutLine, fAdvancedRenderer);
        vobSub.put_FileName(filename);
      }

      { //load sub streams
        IBaseFilter hms = null;
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.HaaliGuid, out hms);
        embeddedSelector = hms as IAMStreamSelect;
        if (embeddedSelector != null)
        {
          AddStreams(embeddedSelector);
        }

        vobSub.get_LanguageCount(out extCount);
        if (intSubs.Count > 0)
        {
          //if there are embedded subtitles,
          //last stream of directvobsub is currently selected embedded subtitle
          extCount--;
        }
      }
      Current = 0;
      return true;
    }

    private void AddStreams(IAMStreamSelect pStrm)
    {
      int cStreams = 0;
      pStrm.Count(out cStreams);
      //GET STREAMS
      for (int istream = 0; istream < cStreams; istream++)
      {
        AMMediaType sType;
        AMStreamSelectInfoFlags sFlag;
        int sPDWGroup, sPLCid;
        string sName;
        object pppunk, ppobject;
        //STREAM INFO
        pStrm.Info(istream, out sType, out sFlag, out sPLCid,
                   out sPDWGroup, out sName, out pppunk, out ppobject);

        //SUBTITLE
        if (sPDWGroup == 2 && sName.LastIndexOf("No ") == -1)
        {
          intSubs.Add(istream);
          intNames.Add(sName);
        }
      }
    }

    public void FreeSubtitles()
    {
      if (vobSub != null)
        DirectShowUtil.ReleaseComObject(vobSub);
      if (embeddedSelector != null)
        DirectShowUtil.ReleaseComObject(embeddedSelector);
      vobSub = null;
      embeddedSelector = null;
      intSubs.Clear();
      intNames.Clear();
      extCount = 0;
      current = -1;
    }

    public void SaveToDisk() {}

    public bool IsModified()
    {
      return false;
    }

    public bool PosRelativeToFrame
    {
      get { return false; }
    }

    public AutoSaveTypeEnum AutoSaveType
    {
      get { return AutoSaveTypeEnum.NEVER; }
    }

    public void Render(Rectangle subsRect, Rectangle frameRect) {}

    public int GetCount()
    {
      return extCount + intSubs.Count;
    }

    public string GetLanguage(int iStream)
    {
      string ret = Strings.Unknown;
      if (vobSub == null)
        return ret;
      if (iStream < extCount)
      {
        IntPtr curNamePtr;
        vobSub.get_LanguageName(iStream, out curNamePtr);
        if (curNamePtr != IntPtr.Zero)
        {
          ret = Marshal.PtrToStringUni(curNamePtr);
          Marshal.FreeCoTaskMem(curNamePtr);
        }
        return ret;
      }

      int index = iStream - extCount;
      if (index >= intNames.Count)
        return ret;
      string streamName = intNames[index];
      return streamName;
    }

    public int Current
    {
      get { return current; }
      set
      {
        if (value < 0 || value >= GetCount())
          return;
        current = value;
        if (value < extCount)
        {
          vobSub.put_SelectedLanguage(value);
        }
        else
        {
          int i = value - extCount;
          int index = intSubs[i];
          embeddedSelector.Enable(index, AMStreamSelectEnableFlags.Enable);
          vobSub.put_SelectedLanguage(extCount);
        }
      }
    }

    public bool Enable
    {
      get
      {
        bool ret = false;
        if (this.vobSub != null)
        {
          int hr = vobSub.get_HideSubtitles(out ret);
          if (hr == 0)
          {
            ret = !ret;
          }
        }
        return ret;
      }
      set
      {
        if (this.vobSub != null)
        {
          bool hide = !value;
          vobSub.put_HideSubtitles(hide);
        }
      }
    }

    public int DelayInterval
    {
      get { return delayInterval; }
    }

    public int Delay
    {
      get
      {
        int delay, speedmul, speeddiv;
        vobSub.get_SubtitleTiming(out delay, out speedmul, out speeddiv);
        return delay;
      }
      set { vobSub.put_SubtitleTiming(value, 1, 1); }
    }

    public void DelayPlus()
    {
      Delay = Delay + delayInterval;
    }

    public void DelayMinus()
    {
      Delay = Delay - delayInterval;
    }

    public void SetTime(long nsSampleTime) {}
    #endregion
  }

  public class DirectVobSubUtil
  {
    public static IBaseFilter AddToGraph(IGraphBuilder graphBuilder)
    {
      IBaseFilter vob = null;

      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubAutoload, out vob);
      if (vob == null)
      {
        //Try the "normal" filter then.
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubNormal, out vob);
      }

      //if the directvobsub filter has not been added to the graph. (i.e. with evr)
      //we add a bit more intelligence to determine if subtitles are enabled.
      //and if subtitles are present for the video / movie then we add it if necessary to the graph.
      if (vob == null)
      {
        Log.Info("VideoPlayerVMR9: No VobSub filter in the current graph");
        //the filter has not been added lets check if it should be added or not.
        //add the filter to the graph
        vob = DirectShowUtil.AddFilterToGraph(graphBuilder, "DirectVobSub");
        if (vob == null)
        {
          Log.Warn("VideoPlayerVMR9: DirectVobSub filter not found! You need to install VSFilter");
          return null;
        }
        else
        {
          Log.Debug("VideoPlayerVMR9: VobSub filter added to graph");
        }
      }
      else // VobSub already loaded
      {
        return vob;
      }

      // Check if Haali Media Splitter is in the graph.
      IBaseFilter hms = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.HaaliGuid, out hms);
      if(hms == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.MPCMatroska, out hms);
      if (hms != null)
      {
        IPin pinSubTo = null;
        // It is. Connect it' subtitle output pin (if any) to Vobsub's subtitle input.
        pinSubTo = DirectShowUtil.FindPin(hms, PinDirection.Output, "Subtitle");
        if (pinSubTo == null)
        {
          while (true)
          {
            IPin freeSubtitle = FindFirstFreePin(hms, PinDirection.Output, "");
            IPin freeVobSub = FindFirstFreePin(vob, PinDirection.Input, "Input");
            if (freeSubtitle != null && freeVobSub != null)
            {
              Log.Debug("VideoPlayerVMR9: Connecting Matroska's subtitle output to VobSub's input.");
              graphBuilder.Connect(freeSubtitle, freeVobSub);
              DirectShowUtil.ReleaseComObject(freeSubtitle); freeSubtitle = null;
              DirectShowUtil.ReleaseComObject(freeVobSub); freeVobSub = null;
            }
            else
              break;
          } 
        }

        DirectShowUtil.ReleaseComObject(hms); hms = null;
        if (pinSubTo != null)
        {
          Log.Debug("VideoPlayerVMR9: Connecting Haali's subtitle output to VobSub's input.");
          // Try to render pins
          IPin pinVobSubSub = DirectShowUtil.FindPin(vob, PinDirection.Input, "Input");
          graphBuilder.Connect(pinSubTo, pinVobSubSub);
          DirectShowUtil.ReleaseComObject(pinSubTo); pinSubTo = null;
          DirectShowUtil.ReleaseComObject(pinVobSubSub); pinVobSubSub = null;
        }
      }

      // Now check if vobsub's video input is not connected.
      // Check only if vmr9 is connected (render was successful).
      VMR9Util Vmr9 = VMR9Util.g_vmr9;
      if (Vmr9.IsVMR9Connected)
      {
        Log.Debug("VideoPlayerVMR9: Connect VobSub's video pins");

        IPin pinVobSubVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
        IPin pinVobSubVideoOut = DsFindPin.ByDirection(vob, PinDirection.Output, 0);

        // This is the pin that we will connect to vobsub's input.
        IPin pinVideoTo = Vmr9.PinConnectedTo;
        IPin pinVideoFrom = null;
        pinVideoTo.ConnectedTo(out pinVideoFrom);
        pinVideoTo.Disconnect();
        pinVideoFrom.Disconnect();
        //Now make connection to VobSub
        int hr = graphBuilder.Connect(pinVideoTo, pinVobSubVideoIn);
        //hr = graphBuilder.Render(pinVideoFrom);
        if (hr != 0)
        {
          Log.Error("VideoPlayerVMR9: could not connect Vobsub's input video pin");
          return null;
        }
        hr = graphBuilder.Connect(pinVobSubVideoOut, pinVideoFrom);
        //hr = graphBuilder.Render(pinVobSubVideoOut);
        if (hr != 0)
        {
          Log.Error("VideoPlayerVMR9: could not connect Vobsub's output video pin");
          return null;
        }

        Log.Debug("VideoPlayerVMR9: Vobsub's video pins connected");
        DirectShowUtil.ReleaseComObject(pinVideoTo); pinVideoTo = null;
        DirectShowUtil.ReleaseComObject(pinVobSubVideoIn); pinVobSubVideoIn = null;
        DirectShowUtil.ReleaseComObject(pinVobSubVideoOut); pinVobSubVideoOut = null;
        DirectShowUtil.ReleaseComObject(pinVideoFrom); pinVideoFrom = null;
      }
      Vmr9 = null;
      return vob;
    }

    public static void RemoveFromGraph(IGraphBuilder graphBuilder)
    {
      IBaseFilter vob = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubAutoload, out vob);
      if (vob == null)
      {
        //Try the "normal" filter then.
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubNormal, out vob);
        if (vob == null)
          return;
      }

      Log.Info("VideoPlayerVMR9: DirectVobSub in graph, removing...");
      // Check where video inputs are connected
      IPin pinVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
      IPin pinVideoOut = DsFindPin.ByDirection(vob, PinDirection.Output, 0);

      //find directvobsub's video output pin source input pin
      IPin pinVideoTo = null;
      pinVideoOut.ConnectedTo(out pinVideoTo);
      //find directvobsub's video input pin source output pin
      IPin pinVideoFrom = null;
      pinVideoIn.ConnectedTo(out pinVideoFrom);

      int hr;

      if (pinVideoFrom != null)
      {
        hr = pinVideoFrom.Disconnect();
        if (hr != 0)
        {
          Log.Error("VideoPlayerVMR9: DirectVobSub failed disconnecting source pin");
        }
      }

      if (pinVideoTo != null)
      {
        hr = pinVideoTo.Disconnect();
        if (hr != 0)
        {
          Log.Error("VideoPlayerVMR9: DirectVobSub failed disconnecting destination pin");
        }
      }

      //remove the DirectVobSub filter from the graph
      graphBuilder.RemoveFilter(vob);
      DirectShowUtil.ReleaseComObject(vob); vob = null;

      //reconnect the source output pin to the vmr9/evr filter
      hr = graphBuilder.Connect(pinVideoFrom, pinVideoTo);
      //hr = graphBuilder.Render(pinVideoFrom);

      DirectShowUtil.ReleaseComObject(pinVideoFrom); pinVideoFrom = null;
      DirectShowUtil.ReleaseComObject(pinVideoTo); pinVideoTo = null;
      DirectShowUtil.ReleaseComObject(pinVideoOut); pinVideoOut = null;
      DirectShowUtil.ReleaseComObject(pinVideoIn); pinVideoIn = null;

      if (hr != 0)
        Log.Error("VideoPlayerVMR9: Could not connect video out to video renderer: {0}", hr);
      else
        Log.Debug("VideoPlayerVMR9: DirectVobSub graph rebuild finished");
    }

    private static IPin FindFirstFreePin(IBaseFilter filter, PinDirection dir, string strPinName)
    {
      int hr = 0;

      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        while(pinEnum.Next(1, pins, out f)==0)
        {
          if (pins[0] != null)
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == dir)
            {
              IPin other;
              hr = pins[0].ConnectedTo(out other);
              if (hr != 0 && other == null)
              {
                PinInfo info;
                pins[0].QueryPinInfo(out info);                
                if (!String.IsNullOrEmpty(strPinName))
                {
                  if (String.Compare(info.name, strPinName) == 0)
                  {
                    //Log.Debug("*** pin {0}", info.name);
                    DirectShowUtil.ReleaseComObject(pinEnum);
                    return pins[0];
                  }
                }
                else
                {
                  //Log.Debug("*** pin {0}", info.name);
                  DirectShowUtil.ReleaseComObject(pinEnum);
                  return pins[0];
                }
              }
            }
            DirectShowUtil.ReleaseComObject(pins[0]);
          }
        }
        DirectShowUtil.ReleaseComObject(pinEnum);
      }
      return null;
    }
  }
}
