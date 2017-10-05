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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal
{
  /// <summary>
  /// Provides functionality for grabbing the next video frame handed by the VMR to MediaPortal.
  /// </summary>
  public class FrameGrabber
  {
    [DllImport("DXUtil.dll", PreserveSig = false, CharSet = CharSet.Auto)]
    private static extern void VideoSurfaceToRGBSurface(IntPtr src, IntPtr dst);

    private Surface rgbSurface = null; // surface used to hold frame grabs
    private bool grabSucceeded = false; // indicates success/failure of framegrabs
    private bool grabSample = false; // flag to indicate that a frame must be grabbed
    private readonly object grabNotifier = new object(); // Wait/Notify object for waiting for the grab to complete
    private Bitmap FrameResult;

    //FrameSource enum for NewFrameHandler 
    public enum FrameSource
    {
        GUI = 0,
        Video
    }

    private static FrameGrabber instance = null;

    private FrameGrabber()
    {
    }

    // MP1-4248 :  Start* Line Code for Ambilight System Capture (Atmolight)
    public delegate void NewFrameHandler(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pSurface, FrameSource FrameSource);

    public event NewFrameHandler OnNewFrame;
    // MP1-4248 : End* Ambilight Capture

    // MP1-4248 :  Start* Line Code for Ambilight System Capture for madVR (Atmolight)
    public delegate void NewFrameHandlerMadVr(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, Bitmap pTargetmadVrBmp, FrameSource FrameSource);

    public event NewFrameHandlerMadVr OnNewFrameMadVr;
    // MP1-4248 : End* Ambilight Capture for madVR

    public static FrameGrabber GetInstance()
    {
      if (instance == null)
      {
        instance = new FrameGrabber();
      }
      return instance;
    }

    /// <summary>
    /// Grabs the next frame of video obtained
    /// from the VMR9 and return it as an RGB image
    /// </summary> 
    /// <returns>Returns null on failure or a Bitmap object</returns>
    public Bitmap GetCurrentImage()
    {
      try
      {
        //Log.Debug("GetCurrentImage called");

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            GUIGraphicsContext.Vmr9Active && !FrameGrabberD3D9Enable)
        {
          lock (grabNotifier)
          {
            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.MadVrGrabCurrentFrame();
              try
              {
                if (FrameResult != null)
                {
                  FrameResult.SafeDispose();
                  FrameResult = null;
                }

                if (GUIGraphicsContext.madVRCurrentFrameBitmap != null)
                {
                  FrameResult = new Bitmap(GUIGraphicsContext.madVRCurrentFrameBitmap);
                  return FrameResult;
                }
              }
              catch
              {
                Log.Debug("FrameGrabber: Frame grab catch failed for madVR");
                return null;
                // When Bitmap is not yet ready
              }
            }

            //////// Part of code used for D3D9 setting in madVR
            //////lock (grabNotifier)
            //////{
            //////  grabSucceeded = false;
            //////  grabSample = true;
            //////  if (!Monitor.Wait(grabNotifier, 500))
            //////  {
            //////    Log.Debug("FrameGrabber: Timed-out waiting for grabbed frame!");
            //////    return null;
            //////  }

            //////  if (grabSucceeded)
            //////  {
            //////    try
            //////    {
            //////      if (FrameResult != null)
            //////      {
            //////        FrameResult.SafeDispose();
            //////        FrameResult = null;
            //////      }

            //////      if (GUIGraphicsContext.madVRFrameBitmap != null)
            //////      {
            //////        FrameResult = new Bitmap(GUIGraphicsContext.madVRFrameBitmap);
            //////        return FrameResult;
            //////      }
            //////    }
            //////    catch
            //////    {
            //////      Log.Debug("FrameGrabber: Frame grab catch failed for madVR");
            //////      return null;
            //////      // When Bitmap is not yet ready
            //////    }
            //////  }
            //////}
          }
          // Bitmap not ready return null
          Log.Debug("FrameGrabber: Frame grab failed for madVR");
          return null;
        }

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            GUIGraphicsContext.Vmr9Active)
        {
          Surface backbuffer = null;
          Bitmap b = null;
          try
          {
            backbuffer = GUIGraphicsContext.DX9DeviceMadVr.GetBackBuffer(0, 0, BackBufferType.Mono);
            using (var stream = SurfaceLoader.SaveToStream(ImageFileFormat.Bmp, backbuffer))
            {
              b = new Bitmap(Image.FromStream(stream));

              // IMPORTANT: Closes and disposes the stream
              // If this is not done we get a memory leak!
              stream.Close();
              stream.Dispose();
              backbuffer.Dispose();
              return b;
            }
          }
          catch (Exception)
          {
            backbuffer?.Dispose();
            b?.Dispose();
            Log.Debug("FrameGrabber: Timed-out waiting for grabbed frame!");
          }
        }
        else
        {
          lock (grabNotifier)
          {
            grabSucceeded = false;
            grabSample = true;
            if (!Monitor.Wait(grabNotifier, 500))
            {
              Log.Debug("FrameGrabber: Timed-out waiting for grabbed frame!");
              return null;
            }

            if (grabSucceeded)
            {
              using (GraphicsStream stream = SurfaceLoader.SaveToStream(ImageFileFormat.Bmp, rgbSurface))
              {
                Bitmap b = new Bitmap(Image.FromStream(stream));

                // IMPORTANT: Closes and disposes the stream
                // If this is not done we get a memory leak!
                stream.Close();
                return b;
              }
            }
            Log.Debug("FrameGrabber: Frame grab failed");
            return null;
          }
        }
      }
      catch (Exception e) // Can occur for example if the video device is lost
      {
        Log.Debug(e.ToString());
        return null;
      }
      // Not image grabbed
      return null;
    }

    public bool FrameGrabberD3D9Enable { get; set; }

    /// <summary>
    /// Suggests that the FrameGrabber releases resources.
    /// </summary>
    public void Clean()
    {
      if (rgbSurface != null)
      {
        rgbSurface.SafeDispose();
        rgbSurface = null;
      }

      if (GUIGraphicsContext.madVRFrameBitmap != null)
      {
        GUIGraphicsContext.madVRFrameBitmap.SafeDispose();
        GUIGraphicsContext.madVRFrameBitmap = null;
      }

      if (FrameResult != null)
      {
        FrameResult.SafeDispose();
        FrameResult = null;
      }
    }

    /// <summary>
    /// Callback that gives the framegrabber a chance to grab a GUI frame
    /// </summary>
    public void OnFrameGUI()
    {
      if ((OnNewFrame != null) && (!GUIGraphicsContext.IsFullScreenVideo))
      {
        using (Surface surface = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono))
        {
          OnFrameGUI(surface);
        }
      }
    }

    /// <summary>
    /// Callback that gives the framegrabber a chance to grab a GUI frame
    /// </summary>
    /// <param name="surface"></param>
    public void OnFrameGUI(Surface surface)
    {
      if ((OnNewFrame != null) && (!GUIGraphicsContext.IsFullScreenVideo))
      {
        unsafe
        {
          OnFrame((Int16)surface.Description.Width, (Int16)surface.Description.Height, 0, 0, (uint)surface.UnmanagedComPointer, FrameSource.GUI);
        }
      }
    }

    /// <summary>
    /// Callback that gives the framegrabber a chance to grab the frame
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="arWidth"></param>
    /// <param name="arHeight"></param>
    /// <param name="pTargetmadVrDib"></param>
    public void OnFrame(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, IntPtr pTargetmadVrDib, FrameSource FrameSource)
    {
      FrameGrabberD3D9Enable = false;
      //Dont pass GUI frames to GetCurrentImage() -> VideoModeSwitcher is using it
      if (FrameSource == FrameGrabber.FrameSource.GUI) return;

      //Log.Debug("PlaneScene: grabSample is true");
      try
      {
        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            GUIGraphicsContext.Vmr9Active)
        {
          lock (grabNotifier)
          {
            if (GUIGraphicsContext.madVRFrameBitmap != null)
            {
              GUIGraphicsContext.madVRFrameBitmap.SafeDispose();
              GUIGraphicsContext.madVRFrameBitmap = null;
            }

            // Fill the GUIGraphicsContext.madVRFrameBitmap
            Util.Utils.GetMadVrBitmapFromDib(pTargetmadVrDib);

            // MP1-4248 :Start* Line Code for Ambilight System Capture (Atmolight)
            if (OnNewFrameMadVr != null)
            {
              try
              {
                //raise event to any subcribers for event NewFrameHandler
                OnNewFrameMadVr(width, height, arWidth, arHeight, GUIGraphicsContext.madVRFrameBitmap, FrameSource);
              }
              catch (Exception)
              {
              }
            }
            // MP1-4248 :End* Ambilight Capture code

            grabSample = false;
            grabSucceeded = true;
            Monitor.Pulse(grabNotifier);
          }
        }
      }
      // The loss of the D3DX device or similar can cause exceptions, catch any such
      // exception and report failure to GetCurrentImage
      catch (Exception e)
      {
        if (rgbSurface != null)
        {
          rgbSurface.SafeDispose(); // get rid of rgbSurface just to make sure
          rgbSurface = null;
        }
        if (GUIGraphicsContext.madVRFrameBitmap != null)
        {
          GUIGraphicsContext.madVRFrameBitmap.SafeDispose();
          GUIGraphicsContext.madVRFrameBitmap = null;
        }
        if (FrameResult != null)
        {
          FrameResult.SafeDispose();
          FrameResult = null;
        }
        lock (grabNotifier)
        {
          grabSucceeded = false;
          Monitor.Pulse(grabNotifier);
        }
        Log.Error(e.ToString());
      }
    }

    /// <summary>
    /// Callback that gives the framegrabber a chance to grab the frame
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="arWidth"></param>
    /// <param name="arHeight"></param>
    /// <param name="pSurface"></param>
    public void OnFrame(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pSurface, FrameSource FrameSource)
    {
      FrameGrabberD3D9Enable = true;
      // MP1-4248 :Start* Line Code for Ambilight System Capture (Atmolight)
      if (OnNewFrame != null)
      {
        try
        {
          //raise event to any subcribers for event NewFrameHandler
          OnNewFrame(width, height, arWidth, arHeight, pSurface, FrameSource);
        }
        catch (Exception)
        {
        }
      }
      // MP1-4248 :End* Ambilight Capture code

      //Dont pass GUI frames to GetCurrentImage() -> VideoModeSwitcher is using it
      if (FrameSource == FrameGrabber.FrameSource.GUI) return;

      //Log.Debug("PlaneScene: grabSample is true");
      try
      {
        // Is GetCurrentImage() requesting a frame grab?
        if (!grabSample || width == 0 || height == 0)
        {
          return;
        }

        // if we havent already allocated a surface or the surface dimensions dont match
        // allocate a new surface to store the grabbed frame in
        if (rgbSurface == null || rgbSurface.Disposed || rgbSurface.Description.Height != height ||
            rgbSurface.Description.Width != width)
        {
          Log.Debug("FrameGrabber: Creating new frame grabbing surface");

          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
          {
            rgbSurface = GUIGraphicsContext.DX9Device.CreateRenderTarget(width, height, Format.A8R8G8B8,
                                                             MultiSampleType.None, 0, true);
          }
          else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR && GUIGraphicsContext.Vmr9Active)
          {
            if (GUIGraphicsContext.DX9DeviceMadVr != null)
            {
              rgbSurface = GUIGraphicsContext.DX9DeviceMadVr.CreateRenderTarget(width, height, Format.A8R8G8B8,
                                                                 MultiSampleType.None, 0, true);
            }
          }
          else
          {
            rgbSurface = GUIGraphicsContext.DX9Device.CreateRenderTarget(width, height, Format.A8R8G8B8,
                                                                         MultiSampleType.None, 0, true);
          }
        }
        unsafe
        {
          // copy the YUV video surface to our managed ARGB surface
          // Log.Debug("Calling VideoSurfaceToRGBSurface");
          if (rgbSurface != null)
          {
            VideoSurfaceToRGBSurface(new IntPtr(pSurface), (IntPtr) rgbSurface.UnmanagedComPointer);
          }
          lock (grabNotifier)
          {
            grabSample = false;
            grabSucceeded = true;
            Monitor.Pulse(grabNotifier);
          }
        }
      }
        // The loss of the D3DX device or similar can cause exceptions, catch any such
        // exception and report failure to GetCurrentImage
      catch (Exception e)
      {
        if (rgbSurface != null)
        {
          rgbSurface.SafeDispose(); // get rid of rgbSurface just to make sure
          rgbSurface = null;
        }
        if (GUIGraphicsContext.madVRFrameBitmap != null)
        {
          GUIGraphicsContext.madVRFrameBitmap.SafeDispose();
          GUIGraphicsContext.madVRFrameBitmap = null;
        }
        if (FrameResult != null)
        {
          FrameResult.SafeDispose();
          FrameResult = null;
        }
        lock (grabNotifier)
        {
          grabSucceeded = false;
          Monitor.Pulse(grabNotifier);
        }
        Log.Error(e.ToString());
      }
    }
  }
}