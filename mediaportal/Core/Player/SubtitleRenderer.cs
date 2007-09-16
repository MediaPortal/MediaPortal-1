using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using System.Drawing;
using Microsoft.DirectX;
using System.Runtime.InteropServices;
using Microsoft.DirectX.Direct3D;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using System.IO;
using System.Drawing.Imaging;

namespace MediaPortal.Player
{
  /// <summary>
  /// Structure used in communication with subtitle filter
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct SUBTITLE
  {
    // start of bitmap fields
    public Int32 bmType;
    public Int32 bmWidth;
    public Int32 bmHeight;
    public Int32 bmWidthBytes;
    public UInt16 bmPlanes;
    public UInt16 bmBitsPixel;
    public IntPtr bmBits;
    //end of bitmap fields

    // subtitle timestmap
    public UInt64 timeStamp;

    // how long to display subtitle
    public UInt64 timeOut; // in seconds
    public Int32 firstScanLine;
  }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TEXT_SUBTITLE
    {
      public int startTextLine;
      public int totalTextLines;

      public UInt64 timeStamp;
      public UInt64 timeOut; // in seconds
      public string text; // subtitle lines seperated by newline characters
    }

  public class Subtitle
  {
    public Bitmap subBitmap;
    public uint width;
    public uint height;
    public double presentTime;  // NOTE: in seconds
    public double timeOut;      // NOTE: in seconds
    public int firstScanLine;
    public long id = 0;

    public override string ToString()
    {
      return "Subtitle " + id + " meta data: Timeout=" + timeOut + " timestamp=" + presentTime;
    }
  }

  /*
  STDMETHOD(SetBitmapCallback) ( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub) ) PURE;
  STDMETHOD(SetTeletextCallback) ( int (CALLBACK *pSTextSubtitleObserver)(TEXT_SUBTITLE* sub) ) PURE;
  STDMETHOD(SetResetCallback)( int (CALLBACK *pResetObserver)() ) PURE;
  STDMETHOD(SetUpdateTimeoutCallback)( int (CALLBACK *pUpdateTimeoutObserver)(__int64* pTimeout) ) PURE; 
  STDMETHOD(StatusTest)( int testval ) PURE;
 */
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
    void SetTeletextCallback(IntPtr callBack);
    void SetResetCallback(IntPtr callBack);
    void SetUpdateTimeoutCallback(IntPtr callBack);
    void StatusTest(int status);
  }

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int SubtitleCallback(ref SUBTITLE sub);
  public delegate int TextSubtitleCallback(/*ref TEXT_SUBTITLE sub*/IntPtr textsub);
  public delegate int ResetCallback();
  public delegate int UpdateTimeoutCallback(ref Int64 timeOut);

  public class SubtitleRenderer
  {
    private static SubtitleRenderer instance = null;
    private IDVBSubtitleSource subFilter = null;
    private long subCounter = 0;
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
    private TextSubtitleCallback textCallBack;
    private ResetCallback resetCallBack;
    private UpdateTimeoutCallback updateTimeoutCallBack;

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
      get
      {
        return renderSubtitles;
      }
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
        instance.textCallBack = new TextSubtitleCallback(instance.OnTextSubtitle);
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
      Log.Debug("SubtitleRenderer: UpdateTimeout" );
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


    /// <summary>
    /// Callback from subtitle filter, alerting us that a new subtitle is available
    /// It receives the neew subtitle as the argument sub, which data is only valid 
    /// for the duration of OnSubtitle.
    /// </summary>
    /// <returns></returns>
    public int OnSubtitle(ref SUBTITLE sub)
    {
      Log.Debug("OnSubtitle - stream position " + player.StreamPosition);
      lock (alert)
      {
        try
        {
          Log.Debug("SubtitleRenderer:  Bitmap: bpp=" + sub.bmBitsPixel + " planes " + sub.bmPlanes + " dim = " + sub.bmWidth + " x " + sub.bmHeight + " stride : " + sub.bmWidthBytes);
          Log.Debug("SubtitleRenderer: to = " + sub.timeOut + " ts=" + sub.timeStamp + " fsl=" + sub.firstScanLine + " (startPos = " + startPos +")" );

          Subtitle subtitle = new Subtitle();
          subtitle.subBitmap = new Bitmap(sub.bmWidth, sub.bmHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
          subtitle.timeOut = sub.timeOut; 
          subtitle.presentTime = ((double)sub.timeStamp / 1000.0f) + startPos; // compute present time in SECONDS
          subtitle.height = (uint)sub.bmHeight;
          subtitle.width = (uint)sub.bmWidth;
          subtitle.firstScanLine = sub.firstScanLine;
          subtitle.id = subCounter++;
          //Log.Debug("Received Subtitle : " + subtitle.ToString());

          // get bits of allocated image
          BitmapData bmData = subtitle.subBitmap.LockBits(new Rectangle(0, 0, sub.bmWidth, sub.bmHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
          int newSize = bmData.Stride * sub.bmHeight;
          int size = sub.bmWidthBytes * sub.bmHeight;

          if (newSize != size)
          {
            Log.Error("SubtitleRenderer: newSize != size : {0} != {1}", newSize, size);
          }
          // Copy to new bitmap
          //Marshal.Copy(sub.bmBits,bmData.Scan0, 0, newSize);
          byte[] srcData = new byte[size];

          // could be done in one copy, but no IntPtr -> IntPtr Marshal.Copy method exists?
          Marshal.Copy(sub.bmBits, srcData, 0, size);
          Marshal.Copy(srcData, 0, bmData.Scan0, newSize);

          subtitle.subBitmap.UnlockBits(bmData);

          // subtitle.subBitmap.Save("C:\\users\\petert\\sub" + subtitle.id + ".bmp"); // debug

          lock (subtitles)
          {
            subtitles.AddLast(subtitle);
            Log.Debug("SubtitleRenderer: Subtitle added, now have " + subtitles.Count + " subtitles in cache");
          }
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
      }
      return 0;
    }

      public int OnTextSubtitle(IntPtr p /*ref TEXT_SUBTITLE sub*/)
      {

          Log.Debug("On\nText\nSubtitle\ncalled");
          try
          {
              TEXT_SUBTITLE sub = (TEXT_SUBTITLE)Marshal.PtrToStructure(p, typeof(TEXT_SUBTITLE));
              Log.Debug("Start line: " + sub.startTextLine + " total lines " + sub.totalTextLines);
              Log.Debug("Timeout: " + sub.timeOut);
              Log.Debug(sub.text);

              Subtitle subtitle = new Subtitle();
              subtitle.subBitmap = RenderText(sub.text, sub.startTextLine, sub.totalTextLines);
              subtitle.timeOut = sub.timeOut;
              subtitle.presentTime = player.CurrentPosition; // compute present time in SECONDS, text subs are show immediatly
              subtitle.height = 576;
              subtitle.width = 720;
              subtitle.firstScanLine = (int)(sub.startTextLine / (float)sub.totalTextLines) * 576; //sub.firstScanLine;

              lock (subtitles)
              {
                  subtitles.AddLast(subtitle);
                  Log.Debug("SubtitleRenderer: Text subtitle added, now have " + subtitles.Count + " subtitles in cache " + subtitle.ToString());
              }
          }
          catch (Exception e)
          {
              Log.Error("Problem marshalling TEXT_SUBTITLE");
              Log.Error(e);
          }
          return 0;
      }


      public Bitmap RenderText(string lines, int startLine, int totalLines)
      {
          int w = 720;
          int h = 576;

          Bitmap bmp = new Bitmap(w, h);

          int hOffset = (int)(h * (startLine / (float)totalLines));

          Console.WriteLine(hOffset);

          using (Graphics gBmp = Graphics.FromImage(bmp))
          using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255)))
          using (SolidBrush blackBrush = new SolidBrush(Color.FromArgb(0, 0, 0)))
          {
              gBmp.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
              using (System.Drawing.Font fnt = new System.Drawing.Font("Courier", 14, FontStyle.Bold)) // fixed width font!
              {

                  SizeF size = gBmp.MeasureString(lines, fnt);
                  //gBmp.FillRectangle(new SolidBrush(Color.Pink), new Rectangle(0, 0, w, h));
                  int vOffset = (int)((w - size.Width) / 2); // center based on actual text width
                  gBmp.DrawString(lines, fnt, blackBrush, new PointF(vOffset + 1, hOffset + 0));
                  gBmp.DrawString(lines, fnt, blackBrush, new PointF(vOffset + 0, hOffset + 1));
                  gBmp.DrawString(lines, fnt, blackBrush, new PointF(vOffset - 1, hOffset + 0));
                  gBmp.DrawString(lines, fnt, blackBrush, new PointF(vOffset + 0, hOffset - 1));
                  gBmp.DrawString(lines, fnt, brush, new PointF(vOffset, hOffset));
              }
          }
          return bmp;
      }


    /// <summary>
    /// Update the subtitle texture from a Bitmap
    /// </summary>
    /// <param name="bitmap"></param>
    private void SetSubtitle(Subtitle subtitle)
    {
      Log.Debug("SubtitleRenderer: SetSubtitle : " + subtitle.ToString());
      Texture texture = null;
      try
      {
        Bitmap bitmap = subtitle.subBitmap;
        // allocate new texture
        texture = new Texture(GUIGraphicsContext.DX9Device, bitmap.Width, bitmap.Height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
        int pitch;
        Microsoft.DirectX.GraphicsStream a = texture.LockRectangle(0, LockFlags.None, out pitch);
        System.Drawing.Imaging.BitmapData bd = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
        bitmap.UnlockBits(bd);
        bitmap.Dispose();
        bitmap = null;
      }
      catch (Exception e)
      {
        Log.Debug("SubtitleRenderer: Failed to create subtitle surface!!!");
        Log.Error(e);
        return;
      }
      // dispose of old subtitle
      if (subTexture != null)
      {
        subTexture.Dispose();
        subTexture = null;
      }

      // set new subtitle
      subTexture = texture;
      currentSubtitle = subtitle;
      currentSubtitle.subBitmap.Dispose();
      currentSubtitle.subBitmap = null;
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
      IntPtr pTextCallback = Marshal.GetFunctionPointerForDelegate(textCallBack);
      subFilter.SetBitmapCallback(pCallback);
      subFilter.SetTeletextCallback(pTextCallback);

      subFilter.StatusTest(222);

      IntPtr pResetCallBack = Marshal.GetFunctionPointerForDelegate(resetCallBack);
      subFilter.SetResetCallback(pResetCallBack);

      IntPtr pUpdateTimeoutCallBack = Marshal.GetFunctionPointerForDelegate(updateTimeoutCallBack);
      subFilter.SetUpdateTimeoutCallback(pUpdateTimeoutCallBack);

      //IntPtr pTextCallback = Marshal.GetFunctionPointerForDelegate(textCallBack); // needed for when teletext stuff is added
      //subFilter.SetTextCallback(pTextCallback);

      return filter;
    }

    public void Render()
    {
      if (player == null)
      {
        return;
      }
      //Log.Debug("\n\n***** SubtitleRenderer: Subtitle render *********");
      //Log.Debug(" Stream pos: "+player.StreamPosition); 
      //if (!GUIGraphicsContext.IsFullScreenVideo) return;

      if (clearOnNextRender)
      {
        //Log.Debug("SubtitleRenderer: clearOnNextRender");
        clearOnNextRender = false;
        if(subTexture != null) subTexture.Dispose();
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
          if (next.presentTime <= player.StreamPosition) timeForNext = true;
          else
          {
            //Log.Debug("-NEXT subtitle is in the future");
          }
        }
      }

      // Check for subtitle if we dont have one currently or if the current one is beyond its timeout
      if (currentSubtitle == null || currentSubtitle.presentTime + currentSubtitle.timeOut <= player.StreamPosition || timeForNext)
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
        if (currentSubtitle != null) SetSubtitle(currentSubtitle);
        else return;
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
          rationH = GUIGraphicsContext.Height / 576.0f;
          rationW = rationH;
          
          // Get the location to render the subtitle to
          wx = GUIGraphicsContext.OverScanLeft +
             (int)(((float)(GUIGraphicsContext.Width - currentSubtitle.width * rationW  )) / 2);
          wy = GUIGraphicsContext.OverScanTop + (int)(rationH * (float)currentSubtitle.firstScanLine );
        }
        else // Video overlay
        {
          rationH = GUIGraphicsContext.VideoWindow.Height / 576.0f;
          rationW = rationH;

          wx = GUIGraphicsContext.VideoWindow.Right - ( GUIGraphicsContext.VideoWindow.Width / 2 ) - 
            (int)(((float)currentSubtitle.width * rationW) / 2);
          wy = GUIGraphicsContext.VideoWindow.Top + (int)(rationH * (float)currentSubtitle.firstScanLine );
        }

        wwidth = (int)((float)currentSubtitle.width * rationW );
        wheight = (int)((float)currentSubtitle.height * rationH );
        
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
        vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedTextured),
                        4, GUIGraphicsContext.DX9Device,
                        0, CustomVertex.TransformedTextured.Format,
                        Pool.Managed);
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
    }
  }
}
