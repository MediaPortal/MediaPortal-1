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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Font = System.Drawing.Font;

namespace MediaPortal.Player.Subtitles
{
  /// <summary>
  /// Structure used in communication with subtitle filter
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct NATIVE_SUBTITLE
  {
    // start of bitmap fields
    public Int32 bmType;
    public Int32 bmWidth;
    public Int32 bmHeight;
    public Int32 bmWidthBytes;
    public UInt16 bmPlanes;
    public UInt16 bmBitsPixel;
    public IntPtr bmBits;

    // start of screen size definition
    public Int32 screenWidth;
    public Int32 screenHeight;

    // subtitle timestmap
    public UInt64 timeStamp;

    // how long to display subtitle
    public UInt64 timeOut; // in seconds
    public Int32 firstScanLine;
  }

  /*
   * int character_table;
  LPCSTR language;
  int page;
  LPCSTR text;
  int firstLine;  // can be 0 to (totalLines - 1)
  int totalLines; // for teletext this is 25 lines

  unsigned    __int64 timestamp;
  unsigned    __int64 timeOut;

  */

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TEXT_SUBTITLE
  {
    public int encoding;
    public string language;

    public int page;
    public string text; // subtitle lines seperated by newline characters
    public LineContent[] lc;
    public UInt64 timeStamp;
    public UInt64 timeOut; // in seconds
  }

  public enum TeletextCharTable
  {
    English = 1,
    Swedish = 2,
    Third = 3,
    Fourth = 4,
    Fifth = 5,
    Sixth = 6 //,
  }

  public class TeletextPageEntry
  {
    public TeletextPageEntry() {}

    public TeletextPageEntry(TeletextPageEntry e)
    {
      page = e.page;
      encoding = e.encoding;
      language = String.Copy(e.language);
    }

    public int page = -1; // indicates not valid
    public TeletextCharTable encoding;
    public string language;
  }

  public class Subtitle
  {
    public static int idCount = 0;

    public Subtitle()
    {
      id = idCount++;
    }

    public Bitmap subBitmap;
    public uint width;
    public uint height;
    public uint screenWidth;
    public uint screenHeight;

    public double presentTime; // NOTE: in seconds
    public double timeOut; // NOTE: in seconds
    public int firstScanLine;
    public long id = 0;
    public Texture texture;

    public void Dispose()
    {
      if (subBitmap != null)
      {
        subBitmap.Dispose();
      }
    }

    public override string ToString()
    {
      return "Subtitle " + id + " meta data: Timeout=" + timeOut + " timestamp=" + presentTime;
    }
  }

  /// <summary>
  /// Interface to the subtitle filter, which
  /// allows us to get notified of subtitles and
  /// retrieve them
  /// </summary>
  [Guid("4A4fAE7C-6095-11DC-8314-0800200C9A66"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVBSubtitleSource
  {
    void SetBitmapCallback(IntPtr callBack);
    void SetResetCallback(IntPtr callBack);
    void SetUpdateTimeoutCallback(IntPtr callBack);
    void StatusTest(int status);
  }

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int SubtitleCallback(ref NATIVE_SUBTITLE sub);

  public delegate int ResetCallback();

  public delegate int UpdateTimeoutCallback(ref Int64 timeOut);

  public class SubtitleRenderer
  {
    private bool useBitmap = false; // if false use teletext
    private int activeSubPage = -1; // if use teletext, what page
    private static SubtitleRenderer instance = null;
    private IDVBSubtitleSource subFilter = null;
    private long subCounter = 0;
    private const int MAX_SUBTITLES_IN_QUEUE = 20;

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int wx0, wy0, wwidth0, wheight0 = 0;

    /// <summary>
    /// Vertex buffer for rendering subtitles
    /// </summary>
    private VertexBuffer vertexBuffer = null;

    // important, these delegates must NOT be garbage collected
    // or horrible things will happen when the native code tries to call those!
    private SubtitleCallback callBack;
    //private TextSubtitleCallback textCallBack;
    private ResetCallback resetCallBack;
    private UpdateTimeoutCallback updateTimeoutCallBack;

    private double posOnLastRender; //file position on last render

    /// <summary>
    /// Texture storing the current/last subtitle
    /// </summary>
    private Texture subTexture;

    /// <summary>
    /// Reference to the DirectShow DVBSub filter, which 
    /// is the source of our subtitle bitmaps
    /// </summary>
    private IBaseFilter filter = null;

    // timestampt offset in MILLISECONDS
    private double startPos = 0;

    private Subtitle currentSubtitle = null;
    private IPlayer player = null;
    private LinkedList<Subtitle> subtitles;
    private object alert = new object();

    private bool clearOnNextRender = false;
    private bool renderSubtitles = true;

    public bool RenderSubtitles
    {
      get { return renderSubtitles; }
      set
      {
        renderSubtitles = value;
        if (value == false)
        {
          clearOnNextRender = true;
        }
      }
    }

    private SubtitleRenderer()
    {
      subtitles = new LinkedList<Subtitle>();
    }

    public static SubtitleRenderer GetInstance()
    {
      if (instance == null)
      {
        instance = new SubtitleRenderer();
        instance.callBack = new SubtitleCallback(instance.OnSubtitle);
        //instance.textCallBack = new TextSubtitleCallback(instance.OnTextSubtitle);
        instance.resetCallBack = new ResetCallback(instance.Reset);
        instance.updateTimeoutCallBack = new UpdateTimeoutCallback(instance.UpdateTimeout);
      }
      return instance;
    }

    public void SetPlayer(IPlayer p)
    {
      lock (subtitles)
      {
        subtitles.Clear();
      }
      clearOnNextRender = true;
      player = p;
    }

    public void SetSubtitleOption(SubtitleOption option)
    {
      if (option.type == SubtitleType.None)
      {
        useBitmap = false;
        activeSubPage = 0;
      }
      else if (option.type == SubtitleType.Teletext)
      {
        useBitmap = false;
        activeSubPage = option.entry.page;
        Log.Debug("SubtitleRender: Now rendering {0} teletext subtitle page {1}", option.language, activeSubPage);
      }
      else if (option.type == SubtitleType.Bitmap)
      {
        useBitmap = true;
        Log.Debug("SubtitleRender: Now rendering bitmap subtitles in language {0}", option.language);
      }
      else
      {
        Log.Error("Unknown subtitle option " + option);
      }
    }

    /// <summary>
    /// Alerts the subtitle render that a seek has just been performed.
    /// Stops displaying the current subtitle and removes any cached subtitles.
    /// Furthermore updates the time that playback starts after the seek.
    /// </summary>
    /// <returns></returns>
    public int OnSeek(double startPos)
    {
      Log.Debug("SubtitleRenderer: OnSeek - clear subtitles");
      // Remove all previously received subtitles
      lock (subtitles)
      {
        subtitles.Clear();
      }
      // Fixed seeking, currently TsPlayer & TsReader is not reseting the base time when seeking
      //this.startPos = startPos;
      clearOnNextRender = true;
      //posOnLastTextSub = -1;
      Log.Debug("New StartPos is " + startPos);
      return 0;
    }

    /// <summary>
    /// Alerts the subtitle render that a reset has just been performed.
    /// Stops displaying the current subtitle and removes any cached subtitles.
    /// </summary>
    /// <returns></returns>
    public int Reset()
    {
      Log.Debug("SubtitleRenderer: RESET");
      // Remove all previously received subtitles
      lock (subtitles)
      {
        subtitles.Clear();
      }
      clearOnNextRender = true;
      return 0;
    }

    /// <summary>
    /// Callback from subtitle filter
    /// Updates the latest subtitle timeout 
    /// </summary>
    /// <returns></returns>
    public int UpdateTimeout(ref Int64 timeOut)
    {
      Log.Debug("SubtitleRenderer: UpdateTimeout");
      Subtitle latest;
      if (subtitles.Count > 0)
      {
        latest = subtitles.Last.Value;
      }
      else
      {
        latest = currentSubtitle;
      }

      if (latest != null)
      {
        latest.timeOut = (double)timeOut / 1000.0f;
        Log.Debug("  new timeOut = {0}", latest.timeOut);
      }
      return 0;
    }

    private void AddSubtitle(Subtitle sub)
    {
      lock (subtitles)
      {
        while (subtitles.Count >= MAX_SUBTITLES_IN_QUEUE)
        {
          Log.Debug("SubtitleRenderer: Subtitle queue too big, discarding first element");
          subtitles.First.Value.Dispose();
          subtitles.RemoveFirst();
        }
        subtitles.AddLast(sub);
        Log.Debug("SubtitleRenderer: Subtitle added, now have " + subtitles.Count + " subtitles in cache");
      }
    }

    /// <summary>
    /// Callback from subtitle filter, alerting us that a new subtitle is available
    /// It receives the new subtitle as the argument sub, which data is only valid 
    /// for the duration of OnSubtitle.
    /// </summary>
    /// <returns></returns>
    public int OnSubtitle(ref NATIVE_SUBTITLE sub)
    {
      if (!useBitmap || !renderSubtitles)
      {
        return 0;
        // TODO: Might be good to let this cache and then check in Render method because bitmap subs arrive a while before display
      }
      Log.Debug("OnSubtitle - stream position " + player.StreamPosition);
      lock (alert)
      {
        try
        {
          Log.Debug("SubtitleRenderer:  Bitmap: bpp=" + sub.bmBitsPixel + " planes " + sub.bmPlanes + " dim = " +
                    sub.bmWidth + " x " + sub.bmHeight + " stride : " + sub.bmWidthBytes);
          Log.Debug("SubtitleRenderer: to = " + sub.timeOut + " ts=" + sub.timeStamp + " fsl=" + sub.firstScanLine +
                    " (startPos = " + startPos + ")");

          Subtitle subtitle = new Subtitle();
          subtitle.subBitmap = new Bitmap(sub.bmWidth, sub.bmHeight, PixelFormat.Format32bppArgb);
          subtitle.timeOut = sub.timeOut;
          subtitle.presentTime = ((double)sub.timeStamp / 1000.0f) + startPos; // compute present time in SECONDS
          subtitle.height = (uint)sub.bmHeight;
          subtitle.width = (uint)sub.bmWidth;
          subtitle.screenHeight = (uint)sub.screenHeight;
          subtitle.screenWidth = (uint)sub.screenWidth;
          subtitle.firstScanLine = sub.firstScanLine;
          subtitle.id = subCounter++;
          //Log.Debug("Received Subtitle : " + subtitle.ToString());

          Texture texture = null;
          try
          {
            // allocate new texture
            texture = new Texture(GUIGraphicsContext.DX9Device, (int)subtitle.width, (int)subtitle.height, 1, Usage.Dynamic,
                                  Format.A8R8G8B8, Pool.Default);

            if (texture == null)
            {
              Log.Debug("OnSubtitle: Failed to create new texture!");
              return 0;
            }

            int pitch;
            GraphicsStream a = texture.LockRectangle(0, LockFlags.None, out pitch);

            // Quick copy of content
            unsafe
            {
              byte* to = (byte*)a.InternalDataPointer;
              byte* from = (byte*)sub.bmBits;
              for (int y = 0; y < sub.bmHeight; ++y)
              {
                for (int x = 0; x < sub.bmWidth * 4; ++x)
                {
                  to[pitch * y + x] = from[y * sub.bmWidthBytes + x];
                }
              }
            }

            texture.UnlockRectangle(0);
            subtitle.texture = texture;
          }
          catch (Exception)
          {
            Log.Debug("OnSubtitle: Failed to copy bitmap data!");
            return 0;
          }

          AddSubtitle(subtitle);
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
      }
      return 0;
    }

    /* private double posOnLastTextSub = -1;
    private bool lastTextSubBlank = false;
    private bool useMinSeperation = false;*/

    public void OnTextSubtitle(ref TEXT_SUBTITLE sub)
    {
      //bool blank = false;

      try
      {
        if (sub.page == activeSubPage)
        {
          Log.Debug("Page: " + sub.page);
          Log.Debug("Character table: " + sub.encoding);
          Log.Debug("Timeout: " + sub.timeOut);
          Log.Debug("Timestamp: " + sub.timeStamp);
          Log.Debug("Language: " + sub.language);

          String content = sub.text;
          if (content == null)
          {
            Log.Error("OnTextSubtitle: sub.txt == null!");
            return;
          }
          Log.Debug("Content: ");
          if (content.Trim().Length > 0) // debug log subtitles
          {
            StringTokenizer st = new StringTokenizer(content, new char[] {'\n'});
            while (st.HasMore)
            {
              Log.Debug(st.NextToken());
            }
          }
          else
          {
            //blank = true;
            Log.Debug("Page: <BLANK PAGE>");
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("Problem with TEXT_SUBTITLE");
        Log.Error(e);
      }

      try
      {
        // if we dont need the subtitle
        if (!renderSubtitles || useBitmap || (activeSubPage != sub.page))
        {
          //
          //chemelli: too much logging. You can check if logs have:
          //          Log.Debug("Page: " + sub.page);  or Log.Debug("Page: <BLANK PAGE>");
          //          and
          //          Log.Debug("Text subtitle (page {0}) ACCEPTED: [...]
          //          to know the evaluation of this if block
          //
          //Log.Debug("Text subtitle (page {0}) discarded: useBitmap is {1} and activeSubPage is {2}", sub.page, useBitmap,
          //          activeSubPage);

          return;
        }
        Log.Debug("Text subtitle (page {0}) ACCEPTED: useBitmap is {1} and activeSubPage is {2}", sub.page, useBitmap,
                  activeSubPage);

        Subtitle subtitle = new Subtitle();
        
        // TODO - RenderText should directly draw to a D3D texture
        subtitle.subBitmap = RenderText(sub.lc);
        subtitle.timeOut = sub.timeOut;
        subtitle.presentTime = sub.timeStamp / 90000.0f + startPos;

        subtitle.height = 576;
        subtitle.width = 720;
        subtitle.screenHeight = 576;
        subtitle.screenWidth = 720;
        subtitle.firstScanLine = 0;

        Texture texture = null;
        try
        {
          // allocate new texture
          texture = new Texture(GUIGraphicsContext.DX9Device, subtitle.subBitmap.Width, 
            subtitle.subBitmap.Height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
          int pitch;
          GraphicsStream a = texture.LockRectangle(0, LockFlags.None, out pitch);
          BitmapData bd = subtitle.subBitmap.LockBits(new Rectangle(0, 0, subtitle.subBitmap.Width, 
            subtitle.subBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

          // Quick copy of content
          unsafe
          {
            byte* to = (byte*)a.InternalDataPointer;
            byte* from = (byte*)bd.Scan0.ToPointer();
            for (int y = 0; y < bd.Height; ++y)
            {
              for (int x = 0; x < bd.Width * 4; ++x)
              {
                to[pitch * y + x] = from[y * bd.Stride + x];
              }
            }
          }
          texture.UnlockRectangle(0);
          subtitle.subBitmap.UnlockBits(bd);
          subtitle.subBitmap.Dispose();
          subtitle.subBitmap = null;
          subtitle.texture = texture;
        }
        catch (Exception e)
        {
          Log.Debug("SubtitleRenderer: Failed to create subtitle surface!");
          Log.Error(e);
          return;
        }

        AddSubtitle(subtitle);
      }
      catch (Exception e)
      {
        Log.Error("Problem processing text subtitle");
        Log.Error(e);
      }

      return;
    }

    public static Bitmap RenderText(LineContent[] lc)
    {
      int w = 720;
      int h = 576;

      Bitmap bmp = new Bitmap(w, h);

      using (Graphics gBmp = Graphics.FromImage(bmp))
      {
        using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255)))
        {
          using (SolidBrush blackBrush = new SolidBrush(Color.FromArgb(0, 0, 0)))
          {
            gBmp.TextRenderingHint = TextRenderingHint.AntiAlias;
            for (int i = 0; i < lc.Length; i++)
            {
              using (Font fnt = new Font("Courier", (lc[i].doubleHeight ? 22 : 15), FontStyle.Bold))
                // fixed width font!
              {
                int vertOffset = (h / lc.Length) * i;

                SizeF size = gBmp.MeasureString(lc[i].line, fnt);
                //gBmp.FillRectangle(new SolidBrush(Color.Pink), new Rectangle(0, 0, w, h));
                int horzOffset = (int)((w - size.Width) / 2); // center based on actual text width
                gBmp.DrawString(lc[i].line, fnt, blackBrush, new PointF(horzOffset + 1, vertOffset + 0));
                gBmp.DrawString(lc[i].line, fnt, blackBrush, new PointF(horzOffset + 0, vertOffset + 1));
                gBmp.DrawString(lc[i].line, fnt, blackBrush, new PointF(horzOffset - 1, vertOffset + 0));
                gBmp.DrawString(lc[i].line, fnt, blackBrush, new PointF(horzOffset + 0, vertOffset - 1));
                gBmp.DrawString(lc[i].line, fnt, brush, new PointF(horzOffset, vertOffset));
              }
            }
          }
        }
      }
      return bmp;
    }


    /// <summary>
    /// Update the subtitle texture from a Bitmap
    /// </summary>
    /// <param name="subtitle"></param>
    private void SetSubtitle(Subtitle subtitle)
    {
      try
      {
        Log.Debug("SubtitleRenderer: SetSubtitle : " + subtitle.ToString());

        // dispose of old subtitle
        if (subTexture != null)
        {
          subTexture.Dispose();
          subTexture = null;
        }

        // set new subtitle
        if (subtitle != null)
        {          
          subTexture = subtitle.texture;
          currentSubtitle = subtitle;

          if (currentSubtitle.subBitmap != null)
          {
            currentSubtitle.subBitmap.Dispose();
            currentSubtitle.subBitmap = null;
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("SubtitleRenderer: SetSubtitle exception: {0} {1}", e.Message, e.StackTrace);         
      }      
    }

    /// <summary>
    /// Adds the subtitle filter to the graph.
    /// </summary>
    /// <param name="_graphBuilder"></param>
    /// <returns></returns>
    public IBaseFilter AddSubtitleFilter(IGraphBuilder _graphBuilder)
    {
      try
      {
        filter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal DVBSub2");
        subFilter = filter as IDVBSubtitleSource;
        Log.Debug("SubtitleRenderer: CreateFilter success: " + (filter != null) + " & " + (subFilter != null));
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      subFilter.StatusTest(111);
      IntPtr pCallback = Marshal.GetFunctionPointerForDelegate(callBack);
      subFilter.SetBitmapCallback(pCallback);

      subFilter.StatusTest(222);

      IntPtr pResetCallBack = Marshal.GetFunctionPointerForDelegate(resetCallBack);
      subFilter.SetResetCallback(pResetCallBack);

      IntPtr pUpdateTimeoutCallBack = Marshal.GetFunctionPointerForDelegate(updateTimeoutCallBack);
      subFilter.SetUpdateTimeoutCallback(pUpdateTimeoutCallBack);

      return filter;
    }

    public void Render()
    {
      if (player == null)
      {
        return;
      }
      //Log.Debug("\n\n***** SubtitleRenderer: Subtitle render *********");
      // Log.Debug(" Stream pos: "+player.StreamPosition); 
      //if (!GUIGraphicsContext.IsFullScreenVideo) return;

      if (clearOnNextRender)
      {
        //Log.Debug("SubtitleRenderer: clearOnNextRender");
        clearOnNextRender = false;
        if (subTexture != null)
        {
          subTexture.Dispose();
        }
        subTexture = null;
        currentSubtitle = null;
      }

      if (renderSubtitles == false)
      {
        return;
      }

      // ugly temp!
      bool timeForNext = false;
      lock (subtitles)
      {
        if (subtitles.Count > 0)
        {
          Subtitle next = subtitles.First.Value;
          if (next.presentTime <= player.StreamPosition)
          {
            timeForNext = true;
          }
        }
      }

      posOnLastRender = player.StreamPosition;

      // Check for subtitle if we dont have one currently or if the current one is beyond its timeout
      if (currentSubtitle == null || currentSubtitle.presentTime + currentSubtitle.timeOut <= player.StreamPosition ||
          timeForNext)
      {
        //Log.Debug("-Current position: ");
        if (currentSubtitle != null && !timeForNext)
        {
          //Log.Debug("-Current subtitle : " + currentSubtitle.ToString() + " time out expired");
          currentSubtitle = null;
        }
        if (timeForNext)
        {
          //if (currentSubtitle != null) Log.Debug("-Current subtitle : " + currentSubtitle.ToString() + " TIME FOR NEXT!");
        }

        Subtitle next = null;
        lock (subtitles)
        {
          while (subtitles.Count > 0)
          {
            next = subtitles.First.Value;

            //Log.Debug("-next from queue: " + next.ToString());
            // if the next should be displayed now or previously
            if (next.presentTime <= player.StreamPosition)
            {
              // remove from queue
              subtitles.RemoveFirst();

              // if it is not too late for this sub to be displayed, break
              // otherwise continue
              if (next.presentTime + next.timeOut >= player.StreamPosition)
              {
                currentSubtitle = next;
                break;
              }
            }
              // next wants to be displayed in the future so break
            else
            {
              //Log.Debug("-next is in the future");
              break;
            }
          }
        }
        // if currentSubtitle is non-null we have a new subtitle
        if (currentSubtitle != null)
        {
          SetSubtitle(currentSubtitle);
        }
        else
        {
          return;
        }
      }
      bool alphaTest = false;
      bool alphaBlend = false;
      VertexFormats vertexFormat = CustomVertex.TransformedColoredTextured.Format;

      try
      {
        // store current settings so they can be restored when we are done
        alphaTest = GUIGraphicsContext.DX9Device.GetRenderStateBoolean(RenderStates.AlphaTestEnable);
        alphaBlend = GUIGraphicsContext.DX9Device.GetRenderStateBoolean(RenderStates.AlphaBlendEnable);
        vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;

        int wx = 0, wy = 0, wwidth = 0, wheight = 0;
        float rationW = 1, rationH = 1;

        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          rationH = GUIGraphicsContext.Height / (float)currentSubtitle.screenHeight;
          rationW = rationH;

          // Get the location to render the subtitle to
          wx = GUIGraphicsContext.OverScanLeft +
               (int)(((float)(GUIGraphicsContext.Width - currentSubtitle.width * rationW)) / 2);
          wy = GUIGraphicsContext.OverScanTop + (int)(rationH * (float)currentSubtitle.firstScanLine);
        }
        else // Video overlay
        {
          rationH = GUIGraphicsContext.VideoWindow.Height / (float)currentSubtitle.screenHeight;
          rationW = rationH;

          wx = GUIGraphicsContext.VideoWindow.Right - (GUIGraphicsContext.VideoWindow.Width / 2) -
               (int)(((float)currentSubtitle.width * rationW) / 2);
          wy = GUIGraphicsContext.VideoWindow.Top + (int)(rationH * (float)currentSubtitle.firstScanLine);
        }

        wwidth = (int)((float)currentSubtitle.width * rationW);
        wheight = (int)((float)currentSubtitle.height * rationH);

        // make sure the vertex buffer is ready and correct for the coordinates
        CreateVertexBuffer(wx, wy, wwidth, wheight);

        // Log.Debug("Subtitle render target: wx = {0} wy = {1} ww = {2} wh = {3}", wx, wy, wwidth, wheight);

        // enable alpha testing so that the subtitle is rendered with transparent background
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable, true);
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaTestEnable, false);

        GUIGraphicsContext.DX9Device.SetStreamSource(0, vertexBuffer, 0);
        GUIGraphicsContext.DX9Device.SetTexture(0, subTexture);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      try
      {
        // Restore device settings
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
        GUIGraphicsContext.DX9Device.VertexFormat = vertexFormat;
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable, alphaBlend);
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaTestEnable, alphaTest);
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
    }

    /// <summary>
    /// Creates a vertex buffer for a transformed textured quad matching
    /// the given rectangle and stores it in vertexBuffer
    /// </summary>
    /// <param name="wx"></param>
    /// <param name="wy"></param>
    /// <param name="wwidth"></param>
    /// <param name="wheight"></param>
    private void CreateVertexBuffer(int wx, int wy, int wwidth, int wheight)
    {
      if (vertexBuffer == null)
      {
        Log.Debug("Subtitle: Creating vertex buffer");
        vertexBuffer = new VertexBuffer(typeof (CustomVertex.TransformedTextured),
                                        4, GUIGraphicsContext.DX9Device,
                                        0, CustomVertex.TransformedTextured.Format,
                                        GUIGraphicsContext.GetTexturePoolType());
      }

      if (wx0 != wx || wy0 != wy || wwidth0 != wwidth || wheight0 != wheight)
      {
        Log.Debug("Subtitle: Setting vertices");
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])vertexBuffer.Lock(0, 0);

        // upper left
        verts[0] = new CustomVertex.TransformedTextured(wx, wy, 0, 1, 0, 0);

        // upper right
        verts[1] = new CustomVertex.TransformedTextured(wx + wwidth, wy, 0, 1, 1, 0);

        // lower left
        verts[2] = new CustomVertex.TransformedTextured(wx, wy + wheight, 0, 1, 0, 1);

        // lower right
        verts[3] = new CustomVertex.TransformedTextured(wx + wwidth, wy + wheight, 0, 1, 1, 1);

        vertexBuffer.Unlock();

        // remember what the vertexBuffer is set to
        wy0 = wy;
        wx0 = wx;
        wheight0 = wheight;
        wwidth0 = wwidth;
      }
    }

    /// <summary>
    /// Cleans up resources
    /// </summary>
    public void Clear()
    {
      Log.Debug("SubtitleRenderer: starting cleanup");
      startPos = 0;
      lock (subtitles)
      {
        subtitles.Clear();
      }

      // swap
      if (subTexture != null)
      {
        subTexture.Dispose();
        subTexture = null;
        lock (alert)
        {
          subFilter = null;
        }
      }

      if (vertexBuffer != null)
      {
        vertexBuffer.Dispose();
        vertexBuffer = null;
      }
      Log.Debug("SubtitleRenderer: cleanup done");
    }
  }
}