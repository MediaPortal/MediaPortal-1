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
      LoadSettings();
      FreeSubtitles();

      vobSub = DirectVobSubUtil.AddToGraph(graphBuilder, filename);
      if (vobSub == null)
        return false;

      { //set style
        Log.Info("VideoPlayerVMR9: Setting DirectVobsub parameters");
        //string strTmp = "";
        //string strFont = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
        //int iFontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);
        //bool bBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
        //strTmp = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
        //long iColor = Convert.ToInt64(strTmp, 16);
        //int iShadow = xmlreader.GetValueAsInt("subtitles", "shadow", 5);
        LOGFONT logFont = new LOGFONT();
        int txtcolor;
        bool fShadow, fOutLine, fAdvancedRenderer = false;
        int size = Marshal.SizeOf(typeof(LOGFONT));
        vobSub.get_TextSettings(logFont, size, out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);
        FontStyle fontStyle = defStyle.fontIsBold ? FontStyle.Regular : FontStyle.Bold;
        Font Subfont = new Font(defStyle.fontName, defStyle.fontSize, fontStyle, GraphicsUnit.Point, (byte)defStyle.fontCharset);
        Subfont.ToLogFont(logFont);
        //int R = (int)((iColor >> 16) & 0xff);
        //int G = (int)((iColor >> 8) & 0xff);
        //int B = (int)((iColor) & 0xff);
        //txtcolor = (B << 16) + (G << 8) + R;
        fShadow = defStyle.shadow > 0;
        fOutLine = defStyle.isBorderOutline;
        vobSub.put_TextSettings(logFont, size, defStyle.fontColor, fShadow, fOutLine, fAdvancedRenderer);
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

    public void SaveToDisk()
    {
    }

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

    public void Render(Rectangle subsRect, Rectangle frameRect)
    {
    }

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
      get
      {
        return current;
      }
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

    public int Delay
    {
      get
      {
        int delay, speedmul, speeddiv;
        vobSub.get_SubtitleTiming(out delay, out speedmul, out speeddiv);
        return delay;
      }
      set
      {
        vobSub.put_SubtitleTiming(value, 1, 1);
      }
    }

    public void DelayPlus()
    {
      Delay = Delay + delayInterval;
    }

    public void DelayMinus()
    {
      Delay = Delay - delayInterval;
    }

    public void SetTime(long nsSampleTime)
    {
    }
    #endregion
  }

  public class DirectVobSubUtil
  {
    public static IDirectVobSub AddToGraph(IGraphBuilder graphBuilder, string filename)
    {
      IBaseFilter vob = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubAutoload, out vob);
      if (vob == null)
      {
        //Try the "normal" filter then.
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubNormal, out vob);
      }
      IDirectVobSub vobSub = (IDirectVobSub)vob;

      //if the directvobsub filter has not been added to the graph. (i.e. with evr)
      //we add a bit more intelligence to determine if subtitles are enabled.
      //and if subtitles are present for the video / movie then we add it if necessary to the graph.
      if (vobSub == null)
      {
        Log.Info("VideoPlayerVMR9: no vob sub filter in the current graph");
        //the filter has not been added lets check if it should be added or not.
        //add the filter to the graph
        Log.Info("VideoPlayerVMR9: subtitles present adding DirectVobSub filter to the current graph");
        vob = DirectShowUtil.AddFilterToGraph(graphBuilder, "DirectVobSub");
        if (vob == null)
        {
          Log.Info("VideoPlayerVMR9: DirectVobSub filter not found! You need to install DirectVobSub v2.39");
        }
        else
        {
          Log.Info("VideoPlayerVMR9: add normal vob sub filter");
          vobSub = (IDirectVobSub)vob;
        }
      }

      if (vobSub == null)
        return null;

      {
        // Now check if vobsub's video input is not connected.
        // Check only if vmr9 is connected (render was successful).
        VMR9Util Vmr9 = VMR9Util.g_vmr9;
        if (Vmr9.IsVMR9Connected)
        {
          IPin pinVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
          // Check if video input pin is connected
          IPin pinVideoTo = null;
          int hr = pinVideoIn.ConnectedTo(out pinVideoTo);
          if (hr != 0 || pinVideoTo == null)
          {
            // Pin is not connected. Connect it.
            Log.Info("VideoPlayerVMR9: Connect vobsub's video pins!");
            // This is the pin that we will connect to vobsub's input.
            pinVideoTo = Vmr9.PinConnectedTo;
            // We have to re-add and re-initialize vmr9 as we cannot connect to it once it has been connected to
            Vmr9.Dispose();
            // Just in any case...
            pinVideoTo.Disconnect();
            //Now force connection to vobsub
            hr = graphBuilder.Connect(pinVideoTo, pinVideoIn);
            if (hr != 0)
            {
              Log.Info("VideoPlayerVMR9: could not connect Vobsub's input video pin...");
              return null;
            }
            Log.Info("VideoPlayerVMR9: Vobsub's video input pin connected...");
            DirectShowUtil.ReleaseComObject(pinVideoTo);
            //Add vmr9 again
            Vmr9.AddVMR9(graphBuilder);
            Vmr9.Enable(false);
            // Now render vobsub's video output pin.
            pinVideoTo = DirectShowUtil.FindPin(vob, PinDirection.Output, "Output");
            if (pinVideoTo == null)
            {
              Log.Info("VideoPlayerVMR9: Vobsub output pin NOT FOUND!");
              return null;
            }
            hr = graphBuilder.Render(pinVideoTo);
            if (hr != 0)
            {
              Log.Info("VideoPlayerVMR9: could not connect Vobsub to Vmr9 Renderer");
              return null;
            }
            Log.Info("VideoPlayerVMR9: Vobsub connected to Vmr9 Renderer...");
          }
          else
          {
            DirectShowUtil.ReleaseComObject(pinVideoTo);
          }
          DirectShowUtil.ReleaseComObject(pinVideoIn);
          // Query VobSub's subtitle input pin (first one).
          IPin pinSubIn = DirectShowUtil.FindPin(vob, PinDirection.Input, "Input");
          if (pinSubIn != null)
          {
            // Check if subtitle input pin is connected
            IPin pinSubTo = null;
            pinSubIn.ConnectedTo(out pinSubTo);
            if (hr != 0 || pinSubTo == null)
            {
              // Not connected.
              // Check if Haali Media Splitter is in the graph.
              IBaseFilter hms = null;
              DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.HaaliGuid, out hms);
              if (hms != null)
              {
                // It is. Connect it' subtitle output pin (if any) to Vobsub's subtitle input.
                Log.Info("VideoPlayerVMR9: Connecting Haali's subtitle output to Vobsub's input.");
                pinSubTo = DirectShowUtil.FindPin(hms, PinDirection.Output, "Subtitle");
                if (pinSubTo != null)
                {
                  // Disconnect Haali's output if connected.
                  IPin pinSubToConnectedTo = null;
                  pinSubTo.ConnectedTo(out pinSubToConnectedTo);
                  if (pinSubToConnectedTo != null)
                  {
                    pinSubTo.Disconnect();
                    DirectShowUtil.ReleaseComObject(pinSubToConnectedTo);
                  }
                  // Now, connect Haali and Vobsub.
                  hr = graphBuilder.ConnectDirect(pinSubTo, pinSubIn, null);
                  if (hr != 0)
                  {
                    Log.Info("VideoPlayerVMR9: Haali - Vobsub connect failed: {0}", hr);
                  }
                  DirectShowUtil.ReleaseComObject(pinSubTo);
                }
                DirectShowUtil.ReleaseComObject(hms);
              }
            }
            else
            {
              DirectShowUtil.ReleaseComObject(pinSubTo);
            }
            DirectShowUtil.ReleaseComObject(pinSubIn);
          }
          // Force vobsub to reload available subtitles.
          // This is needed if added as postprocessing filter.
          vobSub.put_FileName(filename);
        }
      }
      return vobSub;
    }

    public static void RemoveFromGraph(IGraphBuilder graphBuilder)
    {
      IBaseFilter vob = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubAutoload, out vob);
      if (vob == null)
      {
        //Try the "normal" filter then.
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubNormal, out vob);
      }

      if (vob != null)
      {//remove directvobsub from graph 

        // Check if video input pin is connected
        // If not just remove the DirectVobSub filter.
        IPin pinVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
        IPin pinInputIn = DsFindPin.ByDirection(vob, PinDirection.Input, 1);
        //find directvobsub's video input pin source output pin
        IPin pinVideoFrom = null;
        pinVideoIn.ConnectedTo(out pinVideoFrom);
        //find DirectVobSub's subtitle input source output pin
        IPin pinSubtitleFrom = null;
        pinInputIn.ConnectedTo(out pinSubtitleFrom);
        PinInfo pininfo;
        int hr;
        if (pinVideoFrom == null)
        {
          //video input pin is not connected
          Log.Info("VideoPlayerVMR9: DirectVobSub not connected, removing...");
          //first check if the subtitle pin is connected (i.e. mkv's), if so disconnect
          if (pinSubtitleFrom != null)
          {
            pinSubtitleFrom.QueryPinInfo(out pininfo);
            hr = pinSubtitleFrom.Disconnect();
            if (hr != 0)
            {
              Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source subtitle output pin {0}",
                       pininfo.name);
            }
          }
          graphBuilder.RemoveFilter(vob);
          while ((hr = DirectShowUtil.ReleaseComObject(vob)) > 0)
          {
            ;
          }
          vob = null;
        }
        else
        {
          //video pin connected, disconnect it.
          //also disconnect the subtitle input pin & output pin.
          pinVideoFrom.QueryPinInfo(out pininfo);
          Log.Info("VideoPlayerVMR9: DirectVobSub connected, removing...");
          hr = pinVideoFrom.Disconnect();
          if (hr != 0)
          {
            Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source video output pin: {0}",
                     pininfo.name);
          }
          //check if the subtitle pin is connected also (mkv's), if so disconnect
          if (pinSubtitleFrom != null)
          {
            pinSubtitleFrom.QueryPinInfo(out pininfo);
            hr = pinSubtitleFrom.Disconnect();
            if (hr != 0)
            {
              Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source subtitle output pin {0}",
                       pininfo.name);
            }
            DirectShowUtil.ReleaseComObject(pinInputIn);
            DirectShowUtil.ReleaseComObject(pinSubtitleFrom);
          }
          DirectShowUtil.ReleaseComObject(pinVideoIn);

          VMR9Util Vmr9 = VMR9Util.g_vmr9;
          //remove vmr9 filter so it can be re-initialized later
          Vmr9.Dispose();
          //remove the DirectVobSub filter from the graph
          graphBuilder.RemoveFilter(vob);
          while ((hr = DirectShowUtil.ReleaseComObject(vob)) > 0)
          {
            ;
          }
          vob = null;
          //Add vmr9 again
          Vmr9.AddVMR9(graphBuilder);
          Vmr9.Enable(false);
          if (pinVideoFrom == null)
          {
            Log.Info("VideoPlayerVMR9: Source output pin NOT FOUND!");
            return;
          }
          //reconnect the source output pin to the vmr9/evr filter
          hr = graphBuilder.Render(pinVideoFrom);
          if (hr != 0)
          {
            Log.Info("VideoPlayerVMR9: Could not connect video out to video renderer: {0}", hr);
            return;
          }
          Log.Info("VideoPlayerVMR9: Video out connected to video renderer...");
          DirectShowUtil.ReleaseComObject(pinVideoFrom);
        }
      }
    }
  }
}
