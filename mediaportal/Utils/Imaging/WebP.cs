using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.Services;

namespace MediaPortal.Imaging
{
  public class WebP
  {
    private static readonly int WEBP_DECODER_ABI_VERSION = 0x0208;

    /// <summary>Enumeration of the status codes</summary>
    private enum VP8StatusCode
    {
      /// <summary>No error</summary>
      VP8_STATUS_OK = 0,
      /// <summary>Memory error allocating objects</summary>
      VP8_STATUS_OUT_OF_MEMORY,
      /// <summary>Configuration is invalid</summary>
      VP8_STATUS_INVALID_PARAM,
      VP8_STATUS_BITSTREAM_ERROR,
      /// <summary>Configuration is invalid</summary>
      VP8_STATUS_UNSUPPORTED_FEATURE,
      VP8_STATUS_SUSPENDED,
      /// <summary>Abort request by user</summary>
      VP8_STATUS_USER_ABORT,
      VP8_STATUS_NOT_ENOUGH_DATA,
    }

    /// <summary>Features gathered from the bit stream</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WebPBitstreamFeatures
    {
      /// <summary>Width in pixels, as read from the bit stream</summary>
      public int Width;
      /// <summary>Height in pixels, as read from the bit stream</summary>
      public int Height;
      /// <summary>True if the bit stream contains an alpha channel</summary>
      public int Has_alpha;
      /// <summary>True if the bit stream is an animation</summary>
      public int Has_animation;
      /// <summary>0 = undefined (/mixed), 1 = lossy, 2 = lossless</summary>
      public int Format;
      /// <summary>Padding for later use</summary>
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
      private readonly uint[] pad;
    };

    #region Native
    [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
    private static extern VP8StatusCode WebPGetFeatures([In()] IntPtr pRawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int iVersion);

    [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
    private static extern IntPtr WebPDecodeBGRInto([In()] IntPtr pData, UIntPtr data_size, IntPtr pOutputBuffer, int iOutputBufferSize, int iOutputStride);

    [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
    private static extern IntPtr WebPDecodeBGRAInto([In()] IntPtr pData, UIntPtr data_size, IntPtr pOutputBuffer, int iOutputBufferSize, int iOutputStride);
    #endregion

    private static ILog _Logger = GlobalServiceProvider.Get<ILog>();

    /// <summary>Read a WebP file</summary>
    /// <param name="strPathFileName">WebP file to load</param>
    /// <returns>Bitmap with the WebP image</returns>
    public static Bitmap Load(string strPathFileName)
    {
      try
      {
        return Decode(File.ReadAllBytes(strPathFileName));
      }
      catch (Exception ex)
      {
        _Logger.Error("[Load] Error: {0}", ex.Message);
      }

      return null;
    }

    /// <summary>Decode a WebP image</summary>
    /// <param name="rawWebP">The data to uncompress</param>
    /// <returns>Bitmap with the WebP image</returns>
    public static Bitmap Decode(byte[] rawWebP)
    {
      Bitmap bmp = null;
      BitmapData bmpData = null;
      GCHandle hWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);

      try
      {
        //Get image width and height
        GetInfo(rawWebP, out int iImgWidth, out int iImgHeight, out bool bHasAlpha, out bool bHasAnimation, out string strFormat);

        //Create a BitmapData and Lock all pixels to be written
        bmp = new Bitmap(iImgWidth, iImgHeight, bHasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
        bmpData = bmp.LockBits(new Rectangle(0, 0, iImgWidth, iImgHeight), ImageLockMode.WriteOnly, bmp.PixelFormat);

        //Uncompress the image
        int iOutputSize = bmpData.Stride * iImgHeight;
        IntPtr pData = hWebP.AddrOfPinnedObject();
        if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
          WebPDecodeBGRInto(pData, (UIntPtr)rawWebP.Length, bmpData.Scan0, iOutputSize, bmpData.Stride);
        else
          WebPDecodeBGRAInto(pData, (UIntPtr)rawWebP.Length, bmpData.Scan0, iOutputSize, bmpData.Stride);

        return bmp;
      }
      catch (Exception ex)
      {
        _Logger.Error("[Decode] Error: {0}", ex.Message);
        return null;
      }
      finally
      {
        //Unlock the pixels
        if (bmpData != null)
          bmp.UnlockBits(bmpData);

        //Free memory
        if (hWebP.IsAllocated)
          hWebP.Free();
      }
    }

    /// <summary>Get info of WEBP data</summary>
    /// <param name="rawWebP">The data of WebP</param>
    /// <param name="iWidth">width of image</param>
    /// <param name="iHeight">height of image</param>
    /// <param name="bHasAlpha">Image has alpha channel</param>
    /// <param name="bHasAnimation">Image is a animation</param>
    /// <param name="strFormat">Format of image: 0 = undefined (/mixed), 1 = lossy, 2 = lossless</param>
    public static void GetInfo(byte[] rawWebP, out int iWidth, out int iHeight, out bool bHasAlpha, out bool bHasAnimation, out string strFormat)
    {
      VP8StatusCode result;
      GCHandle hWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
      iWidth = -1;
      iHeight = -1;
      bHasAlpha = false;
      bHasAnimation = false;
      strFormat = null;
      try
      {
        IntPtr pRawWebP = hWebP.AddrOfPinnedObject();

        WebPBitstreamFeatures features = new WebPBitstreamFeatures();
        result = WebPGetFeatures(pRawWebP, (UIntPtr)rawWebP.Length, ref features, WEBP_DECODER_ABI_VERSION);

        if (result != 0)
          throw new Exception(result.ToString());

        iWidth = features.Width;
        iHeight = features.Height;
        bHasAlpha = features.Has_alpha == 1;
        bHasAnimation = features.Has_animation == 1;

        switch (features.Format)
        {
          case 1:
            strFormat = "lossy";
            break;
          case 2:
            strFormat = "lossless";
            break;
          default:
            strFormat = "undefined";
            break;
        }
      }
      catch (Exception ex)
      {
        _Logger.Error("[GetInfo] Error: {0}", ex.Message);
      }
      finally
      {
        //Free memory
        if (hWebP.IsAllocated)
          hWebP.Free();
      }
    }
  }
}
