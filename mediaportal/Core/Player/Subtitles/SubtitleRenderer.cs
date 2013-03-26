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
using MediaPortal.ExtensionMethods;


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
    public Int32 horizontalPosition;
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
    Sixth = 6
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

  public class Subtitle : IDisposable
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
    public int horizontalPosition;
    public long id = 0;
    public Texture texture;

    public void Dispose()
    {
      if (subBitmap != null)
      {
        subBitmap.SafeDispose();
        subBitmap = null;
        unsafe
        {
          texture.UpdateUnmanagedPointer(null);
        }
      }

      if (texture != null && !texture.Disposed)
      {
        texture.SafeDispose();
        texture = null;
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
    private bool _useBitmap = false; // if false use teletext
    private int _activeSubPage = -1; // if use teletext, what page
    private static SubtitleRenderer _instance = null;
    private IDVBSubtitleSource _subFilter = null;
    private long _subCounter = 0;
    private const int MAX_SUBTITLES_IN_QUEUE = 20;

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int _wx, _wy, _wwidth, _wheight = 0;

    /// <summary>
    /// Vertex buffer for rendering subtitles
    /// </summary>
    private VertexBuffer _vertexBuffer = null;

    // important, these delegates must NOT be garbage collected
    // or horrible things will happen when the native code tries to call those!
    private SubtitleCallback _callBack;
    private ResetCallback _resetCallBack;
    private UpdateTimeoutCallback _updateTimeoutCallBack;

    private double _posOnLastRender; //file position on last render

    /// <summary>
    /// Texture storing the current/last subtitle
    /// </summary>
    private Texture _subTexture;

    /// <summary>
    /// Reference to the DirectShow DVBSub filter, which 
    /// is the source of our subtitle bitmaps
    /// </summary>
    private IBaseFilter _filter = null;

    // timestampt offset in milliseconds
    private double _startPos = 0;

    private Subtitle _currentSubtitle = null;
    private IPlayer _player = null;
    private LinkedList<Subtitle> _subtitles;
    private object _alert = new object();
    private object _subtitleLock = new object();

    private bool _clearOnNextRender = false;
    private bool _renderSubtitles = true;

    public bool RenderSubtitles
    {
      get { return _renderSubtitles; }
      set
      {
        _renderSubtitles = value;
        if (value == false)
        {
          _clearOnNextRender = true;
        }
      }
    }

    private SubtitleRenderer()
    {
      _subtitles = new LinkedList<Subtitle>();
    }

    public static SubtitleRenderer GetInstance()
    {
      if (_instance == null)
      {
        _instance = new SubtitleRenderer();
        _instance._callBack = new SubtitleCallback(_instance.OnSubtitle);
        _instance._resetCallBack = new ResetCallback(_instance.Reset);
        _instance._updateTimeoutCallBack = new UpdateTimeoutCallback(_instance.UpdateTimeout);
      }
      return _instance;
    }

    public void SetPlayer(IPlayer p)
    {
      ClearSubtitles();
      _clearOnNextRender = true;
      _player = p;
    }

    public void SetSubtitleOption(SubtitleOption option)
    {
      if (option.type == SubtitleType.None)
      {
        _useBitmap = false;
        _activeSubPage = 0;
      }
      else if (option.type == SubtitleType.Teletext)
      {
        _useBitmap = false;
        _activeSubPage = option.entry.page;
        Log.Debug("SubtitleRender: Now rendering {0} teletext subtitle page {1}", option.language, _activeSubPage);
      }
      else if (option.type == SubtitleType.Bitmap)
      {
        _useBitmap = true;
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
      ClearSubtitles();
      _clearOnNextRender = true;
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
      ClearSubtitles();
      _clearOnNextRender = true;
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
      if (_subtitles.Count > 0)
      {
        latest = _subtitles.Last.Value;
      }
      else
      {
        latest = _currentSubtitle;
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
      lock (_subtitleLock)
      {
        while (_subtitles.Count >= MAX_SUBTITLES_IN_QUEUE)
        {
          Log.Debug("SubtitleRenderer: Subtitle queue too big, discarding first element");
          _subtitles.First.Value.SafeDispose();
          _subtitles.RemoveFirst();
        }
        _subtitles.AddLast(sub);
        Log.Debug("SubtitleRenderer: Subtitle added, now have " + _subtitles.Count + " subtitles in cache");
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
      if (!_useBitmap || !_renderSubtitles)
      {
        return 0;
        // TODO: Might be good to let this cache and then check in Render method because bitmap subs arrive a while before display
      }
      Log.Debug("OnSubtitle - stream position " + _player.StreamPosition);
      lock (_alert)
      {
        try
        {
          Log.Debug("SubtitleRenderer:  Bitmap: bpp=" + sub.bmBitsPixel + " planes " + sub.bmPlanes + " dim = " +
                    sub.bmWidth + " x " + sub.bmHeight + " stride : " + sub.bmWidthBytes);
          Log.Debug("SubtitleRenderer: to = " + sub.timeOut + " ts=" + sub.timeStamp + " fsl=" + sub.firstScanLine + 
            " h pos=" + sub.horizontalPosition + " (startPos = " + _startPos + ")");

          Subtitle subtitle = new Subtitle();
          subtitle.subBitmap = new Bitmap(sub.bmWidth, sub.bmHeight, PixelFormat.Format32bppArgb);
          subtitle.timeOut = sub.timeOut;
          subtitle.presentTime = ((double)sub.timeStamp / 1000.0f) + _startPos; // compute present time in SECONDS
          subtitle.height = (uint)sub.bmHeight;
          subtitle.width = (uint)sub.bmWidth;
          subtitle.screenHeight = (uint)sub.screenHeight;
          subtitle.screenWidth = (uint)sub.screenWidth;
          subtitle.firstScanLine = sub.firstScanLine;
          subtitle.horizontalPosition = sub.horizontalPosition;
          subtitle.id = _subCounter++;
          //Log.Debug("Received Subtitle : " + subtitle.ToString());

          Texture texture = null;
          try
          {
            // allocate new texture
            texture = new Texture(GUIGraphicsContext.DX9Device, (int)subtitle.width, (int)subtitle.height, 1,
                                  Usage.Dynamic,
                                  Format.A8R8G8B8, Pool.Default);

            if (texture == null)
            {
              Log.Debug("OnSubtitle: Failed to create new texture!");
              return 0;
            }

            int pitch;
            using (GraphicsStream a = texture.LockRectangle(0, LockFlags.Discard, out pitch))
            {
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
              a.Close();
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

    public void OnTextSubtitle(ref TEXT_SUBTITLE sub)
    {
      try
      {
        if (sub.page == _activeSubPage)
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
        if (!_renderSubtitles || _useBitmap || (_activeSubPage != sub.page))
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
        Log.Debug("Text subtitle (page {0}) ACCEPTED: useBitmap is {1} and activeSubPage is {2}", sub.page, _useBitmap,
                  _activeSubPage);

        Subtitle subtitle = new Subtitle();

        // TODO - RenderText should directly draw to a D3D texture
        subtitle.subBitmap = RenderText(sub.lc);
        subtitle.timeOut = sub.timeOut;
        subtitle.presentTime = sub.timeStamp / 90000.0f + _startPos;

        subtitle.height = 576;
        subtitle.width = 720;
        subtitle.screenHeight = 576;
        subtitle.screenWidth = 720;
        subtitle.firstScanLine = 0;
        subtitle.horizontalPosition = 0;

        Texture texture = null;
        try
        {
          // allocate new texture
          texture = new Texture(GUIGraphicsContext.DX9Device, subtitle.subBitmap.Width,
                                subtitle.subBitmap.Height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
          int pitch;
          using (GraphicsStream a = texture.LockRectangle(0, LockFlags.Discard, out pitch))
          {
            BitmapData bd = subtitle.subBitmap.LockBits(new Rectangle(0, 0, subtitle.subBitmap.Width,
                                                                      subtitle.subBitmap.Height), ImageLockMode.ReadOnly,
                                                        PixelFormat.Format32bppArgb);

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
            subtitle.subBitmap.SafeDispose();
            subtitle.subBitmap = null;
            subtitle.texture = texture;
            a.Close();
          }
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


    private void SetSubtitle(Subtitle subtitle)
    {
      try
      {
        lock (_subtitleLock)
        {
          Log.Debug("SubtitleRenderer: SetSubtitle : " + subtitle.ToString());

          // dispose of old subtitle
          _subTexture.SafeDispose();
          _subTexture = null;

          // set new subtitle
          if (subtitle != null)
          {
            _subTexture = subtitle.texture;
            _currentSubtitle = subtitle;

            _currentSubtitle.subBitmap.SafeDispose();
            _currentSubtitle.subBitmap = null;
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
        _filter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal DVBSub3");
        _subFilter = _filter as IDVBSubtitleSource;
        Log.Debug("SubtitleRenderer: CreateFilter success: " + (_filter != null) + " & " + (_subFilter != null));
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      _subFilter.StatusTest(111);
      IntPtr pCallback = Marshal.GetFunctionPointerForDelegate(_callBack);
      _subFilter.SetBitmapCallback(pCallback);

      _subFilter.StatusTest(222);

      IntPtr pResetCallBack = Marshal.GetFunctionPointerForDelegate(_resetCallBack);
      _subFilter.SetResetCallback(pResetCallBack);

      IntPtr pUpdateTimeoutCallBack = Marshal.GetFunctionPointerForDelegate(_updateTimeoutCallBack);
      _subFilter.SetUpdateTimeoutCallback(pUpdateTimeoutCallBack);

      return _filter;
    }

    public void Render()
    {
      if (_player == null)
      {
        return;
      }

      lock (_subtitleLock)
      {
        if (_clearOnNextRender)
        {
          //Log.Debug("SubtitleRenderer: clearOnNextRender");
          _clearOnNextRender = false;
          if (_subTexture != null)
          {
            _subTexture.SafeDispose();
          }
          _subTexture = null;
          _currentSubtitle = null;
        }

        if (_renderSubtitles == false)
        {
          return;
        }

        // ugly temp!
        bool timeForNext = false;
        if (_subtitles.Count > 0)
        {
          Subtitle next = _subtitles.First.Value;
          if (next.presentTime <= _player.StreamPosition)
          {
            timeForNext = true;
          }
        }

        _posOnLastRender = _player.StreamPosition;

        // Check for subtitle if we dont have one currently or if the current one is beyond its timeout
        if (_currentSubtitle == null ||
            _currentSubtitle.presentTime + _currentSubtitle.timeOut <= _player.StreamPosition ||
            timeForNext)
        {
          //Log.Debug("-Current position: ");
          if (_currentSubtitle != null && !timeForNext)
          {
            //Log.Debug("-Current subtitle : " + currentSubtitle.ToString() + " time out expired");
            _currentSubtitle = null;
          }
          if (timeForNext)
          {
            //if (currentSubtitle != null) Log.Debug("-Current subtitle : " + currentSubtitle.ToString() + " TIME FOR NEXT!");
          }

          Subtitle next = null;
          while (_subtitles.Count > 0)
          {
            next = _subtitles.First.Value;

            //Log.Debug("-next from queue: " + next.ToString());
            // if the next should be displayed now or previously
            if (next.presentTime <= _player.StreamPosition)
            {
              // remove from queue
              _subtitles.RemoveFirst();

              // if it is not too late for this sub to be displayed, break
              // otherwise continue
              if (next.presentTime + next.timeOut >= _player.StreamPosition)
              {
                _currentSubtitle = next;
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
          // if currentSubtitle is non-null we have a new subtitle
          if (_currentSubtitle != null)
          {
            SetSubtitle(_currentSubtitle);
          }
          else
          {
            return;
          }
        }
        
        VertexFormats vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;

        try
        {
          int wx = 0, wy = 0, wwidth = 0, wheight = 0;
          float rationW = 1, rationH = 1;

          Rectangle src, dst;
          VMR9Util.g_vmr9.GetVideoWindows(out src, out dst);

          rationH = dst.Height / (float)_currentSubtitle.screenHeight;
          rationW = dst.Width / (float)_currentSubtitle.screenWidth;
          wx = dst.X + (int)(rationW * (float)_currentSubtitle.horizontalPosition);
          wy = dst.Y + (int)(rationH * (float)_currentSubtitle.firstScanLine);          
          wwidth = (int)((float)_currentSubtitle.width * rationW);
          wheight = (int)((float)_currentSubtitle.height * rationH);
          
          // make sure the vertex buffer is ready and correct for the coordinates
          CreateVertexBuffer(wx, wy, wwidth, wheight);

          // Log.Debug("Subtitle render target: wx = {0} wy = {1} ww = {2} wh = {3}", wx, wy, wwidth, wheight);

          // enable alpha blending so that the subtitle is rendered with transparent background
          DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHABLENDENABLE, 1);

          // Make sure D3D objects haven't been disposed for some reason. This would  cause
          // an access violation on native side, causing Skin Engine to halt rendering
          if (!_subTexture.Disposed && !_vertexBuffer.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetStreamSource(0, _vertexBuffer, 0);
            GUIGraphicsContext.DX9Device.SetTexture(0, _subTexture);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
          }
          else
          {
            Log.Debug("Subtitle renderer: D3D resource was disposed! Not trying to render the texture");
          }
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
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
      } // end of lock (subtitle)
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
      if (_vertexBuffer == null)
      {
        Log.Debug("Subtitle: Creating vertex buffer");
        _vertexBuffer = new VertexBuffer(typeof (CustomVertex.TransformedTextured),
                                        4, GUIGraphicsContext.DX9Device,
                                        Usage.Dynamic | Usage.WriteOnly,
                                        CustomVertex.TransformedTextured.Format,
                                        GUIGraphicsContext.GetTexturePoolType());
        _wx = _wy = _wwidth = _wheight = 0;
      }

      if (_wx != wx || _wy != wy || _wwidth != wwidth || _wheight != wheight)
      {
        Log.Debug("Subtitle: Setting vertices");
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])_vertexBuffer.Lock(0, 0);

        // upper left
        verts[0] = new CustomVertex.TransformedTextured(wx, wy, 0, 1, 0, 0);

        // upper right
        verts[1] = new CustomVertex.TransformedTextured(wx + wwidth, wy, 0, 1, 1, 0);

        // lower left
        verts[2] = new CustomVertex.TransformedTextured(wx, wy + wheight, 0, 1, 0, 1);

        // lower right
        verts[3] = new CustomVertex.TransformedTextured(wx + wwidth, wy + wheight, 0, 1, 1, 1);

        _vertexBuffer.SetData(verts, 0, LockFlags.None);

        // remember what the vertexBuffer is set to
        _wy = wy;
        _wx = wx;
        _wheight = wheight;
        _wwidth = wwidth;
      }
    }

    private void ClearSubtitles()
    {
      lock (_subtitleLock)
      {
        _subtitles.DisposeAndClearCollection();
      }
    }

    /// <summary>
    /// Cleans up resources
    /// </summary>
    public void Clear()
    {
      Log.Debug("SubtitleRenderer: starting cleanup");
      _startPos = 0;

      ClearSubtitles();

      lock (_subtitleLock)
      {
        // swap
        if (_subTexture != null)
        {
          _subTexture.SafeDispose();
          _subTexture = null;
        }

        if (_vertexBuffer != null)
        {
          _vertexBuffer.SafeDispose();
          _vertexBuffer = null;
        }
      }

      lock (_alert)
      {
        _subFilter = null;
      }

      _instance = null;

      Log.Debug("SubtitleRenderer: cleanup done");
    }
  }
}