using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using System.Drawing;
using Microsoft.DirectX;
using System;
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

    // how long to display subtitle
    public UInt64 timeOut;
  }

  /// <summary>
  /// Interface to the subtitle filter, which
  /// allows us to get notified of subtitles and
  /// retrieve them
  /// </summary>
  [Guid("C19647D5-A861-4845-97A6-EBD0A135D0BF"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IDVBSubtitle
  {
    void GetSubtitle(int place, ref SUBTITLE subtitle);
    void GetSubtitleCount(out int count);
    void SetCallback(IntPtr callBack);
    void DiscardOldestSubtitle();
    void Test(int status);
  }

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int SubtitleCallback();

  class SubtitleRenderer
  {
    private static SubtitleRenderer instance = null;
    private IDVBSubtitle subFilter = null;

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int wx0, wy0, wwidth0, wheight0 = 0;

    /// <summary>
    /// Vertex buffer for rendering subtitles
    /// </summary>
    private VertexBuffer vertexBuffer = null;

    // important, this delegate must NOT be garbage collected
    // or horrible things will happen when the native code tries to call it!
    private SubtitleCallback callBack;
    private int count = 0;

    /// <summary>
    /// Lock to prevent concurent updates to subTexture
    /// </summary>
    private object subtitleLock = new object();

    /// <summary>
    /// Texture storing the current/last subtitle
    /// </summary>
    private Texture subTexture;

    /// <summary>
    /// Reference to the DirectShow DVBSub filter, which 
    /// is the source of our subtitle bitmaps
    /// </summary>
    private IBaseFilter filter = null;
    private object alert = new object();

    private SubtitleRenderer() { }

    public static SubtitleRenderer GetInstance(){
      if(instance == null){
        instance = new SubtitleRenderer();
        instance.callBack = new SubtitleCallback(instance.OnSubtitle);
      }
      return instance;
    }

    private SUBTITLE sub = new SUBTITLE();
    byte[] srcData = null;

    /// <summary>
    /// Callback from subtitle filter, alerting us that a new subtitle is available
    /// </summary>
    /// <returns></returns>
    public int OnSubtitle()
    {
      lock (alert)
      {
        try
        {
          sub = new SUBTITLE();
          subFilter.GetSubtitle(0, ref sub);
          Log.Debug("Subtitle Bitmap: bpp=" + sub.bmBitsPixel + " planes " + sub.bmPlanes + " dim = " + sub.bmWidth + " x " + sub.bmHeight + " stride : " + sub.bmWidthBytes);
          int size = sub.bmWidthBytes * sub.bmHeight;
          srcData = new byte[size];
          Marshal.Copy(sub.bmBits, srcData, 0, size);

          // the subfilter caches subtitles, so ask it to remove the sub it just gave us
          subFilter.DiscardOldestSubtitle();
          Monitor.Pulse(alert);
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
        
      }
      return 0;
    }

    public void WorkerThread()
    {
      while (true)
      {
        lock (alert)
        {
          Monitor.Wait(alert);
        try{
            int srcbpp = sub.bmBitsPixel / 8;
            int dstbpp = srcbpp + 1; // an extra byte for alpha channel
            int size = sub.bmWidthBytes * sub.bmHeight;
            // allocate a new image with an alpha channel
            Bitmap bitmap = new Bitmap(sub.bmWidth, sub.bmHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // get bits of allocated image
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, sub.bmWidth, sub.bmHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int newSize = bmData.Stride * sub.bmHeight;
            byte[] dstData = new byte[newSize];

            // add alpha channel data
            for (int x = 0; x < sub.bmWidth; x++)
            {
              for (int y = 0; y < sub.bmHeight; y++)
              {
                int dstLineOffset = y * bmData.Stride;
                int srcLineOffset = y * sub.bmWidthBytes;

                byte Y = srcData[srcbpp * x + srcLineOffset];
                byte U = srcData[srcbpp * x + 1 + srcLineOffset];
                byte V = srcData[srcbpp * x + 2 + srcLineOffset];

                // convert YUV -> RGB
                byte R = (byte)(Y + 1.402 * (V - 128));
                byte G = (byte)(Y - 0.34414 * (U - 128) - 0.71414 * (V - 128));
                byte B = (byte)(Y + 1.772 * (U - 128));

                if ((Y | U | V) == 0)
                {
                  dstData[dstbpp * x + 0 + dstLineOffset] = 0; // Blue
                  dstData[dstbpp * x + 1 + dstLineOffset] = 0; // Green
                  dstData[dstbpp * x + 2 + dstLineOffset] = 0; // Red
                  dstData[dstbpp * x + 3 + dstLineOffset] = 0; // Alpha
                }
                else
                {
                  dstData[dstbpp * x + 0 + dstLineOffset] = R;// B;
                  dstData[dstbpp * x + 1 + dstLineOffset] = G; //G;
                  dstData[dstbpp * x + 2 + dstLineOffset] = B; //R;
                  dstData[dstbpp * x + 3 + dstLineOffset] = 255;
                }
              }
            }

            // copy image data
            Marshal.Copy(dstData, 0, bmData.Scan0, newSize);
            bitmap.UnlockBits(bmData);

            //bitmap.Save("C:\\sub" + count + ".bmp");
            //count++;

            // replace the current subtitle with the new one
            SetSubtitle(bitmap);

            // the texture copies the bitmap, so get rid of it
            bitmap.Dispose();

          }
          catch (Exception e)
          {
            Log.Error(e);
          }
        }
      }
    }
    /// <summary>
    /// Cleans up resources
    /// </summary>
    public void Clear() {
      lock (subtitleLock)
      {
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

    /// <summary>
    /// Update the subtitle texture from a Bitmap
    /// </summary>
    /// <param name="bitmap"></param>
    private void SetSubtitle(Bitmap bitmap)
    {
      Log.Debug("Subtitle: SetSubtitle");
      Texture newTexture = null;
      try
      {
        // allocate new texture
        newTexture = Texture.FromBitmap(GUIGraphicsContext.DX9Device, bitmap, Usage.None, Pool.Managed);
      }
      catch (Exception e)
      {
        Log.Debug("Failed to create subtitle surface!!!");
        Log.Error(e);
        return;
      }
      lock (subtitleLock)
      {
        // dispose of old subtitle
        if (subTexture != null)
        {
          subTexture.Dispose();
          subTexture = null;
        }
        // set new subtitle
        subTexture = newTexture;
      }

    }

    /// <summary>
    /// Adds the subtitle filter to the graph.
    /// Done here, because of MTA vs. STA issues
    /// </summary>
    /// <param name="_graphBuilder"></param>
    /// <returns></returns>
    public IBaseFilter AddSubtitleFilter(IGraphBuilder _graphBuilder)
    {
      /*ParameterizedThreadStart ts = new ParameterizedThreadStart(CreateFilter);
      Thread t = new Thread(ts);
      t.IsBackground = true;
      t.SetApartmentState(ApartmentState.MTA);
      lock (alert)
      {
        t.Start(_graphBuilder);
        Monitor.Wait(alert);
      }*/
      CreateFilter(_graphBuilder);
      IntPtr pCallback = Marshal.GetFunctionPointerForDelegate(callBack);
      subFilter.SetCallback(pCallback);

      ThreadStart ts = new ThreadStart(WorkerThread);
      Thread t = new Thread(ts);
      t.IsBackground = true;
      t.Start();
      return filter;
    }

    private void CreateFilter(object o)
    {
      try
      {
        //lock (alert)
        //{
          IGraphBuilder _graphBuilder = o as IGraphBuilder;
          filter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "MediaPortal DVBSub");
          subFilter = filter as IDVBSubtitle;

          subFilter.Test(600);
       //   Monitor.Pulse(alert);
          Log.Debug("CreateFilter success: " + (filter != null) + " & " + (subFilter != null));
       // }
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

      if (wx0 != wx || wy0 != wy || wwidth0 != wwidth || wheight0 != wheight0)
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

    public void Render()
    {
      if (!GUIGraphicsContext.IsFullScreenVideo || subTexture == null) return;

      bool alphaTest = false;
      bool alphaBlend = false;
      VertexFormats vertexFormat = CustomVertex.TransformedColoredTextured.Format;
      
      try
      {
        // store current settings so they can be restored when we are done
        alphaTest = GUIGraphicsContext.DX9Device.GetRenderStateBoolean(RenderStates.AlphaTestEnable);
        alphaBlend = GUIGraphicsContext.DX9Device.GetRenderStateBoolean(RenderStates.AlphaBlendEnable);
        vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;
        
        lock (subtitleLock)
        {
          // Get the location to render the subtitle to
          int wx = GUIGraphicsContext.OverScanLeft;
          int wy = GUIGraphicsContext.OverScanTop;
          int wwidth = GUIGraphicsContext.OverScanWidth;
          int wheight = GUIGraphicsContext.OverScanHeight;

          // make sure the vertex buffer is ready and correct for the coordinates
          CreateVertexBuffer(wx, wy, wwidth, wheight);

         // Log.Debug("Subtitle render target: wx = {0} wy = {1} ww = {2} wh = {3}", wx, wy, wwidth, wheight);

          // enable alpha testing so that the subtitle is rendered with transparent background
          GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable, false);
          GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaTestEnable, true);

          GUIGraphicsContext.DX9Device.SetStreamSource(0, vertexBuffer, 0);
          GUIGraphicsContext.DX9Device.SetTexture(0, subTexture);
          GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
          GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
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
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable, alphaBlend);
        GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaTestEnable, alphaTest);
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
    }
  }
}
