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
using System.Threading;
using MediaPortal.GUI.Library;
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

    private static FrameGrabber instance = null;

    private FrameGrabber() {}

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
            GraphicsStream stream = SurfaceLoader.SaveToStream(ImageFileFormat.Bmp, rgbSurface);
            Bitmap b = new Bitmap(Image.FromStream(stream));

            // IMPORTANT: Closes and disposes the stream
            // If this is not done we get a memory leak!
            stream.Close();
            stream.Dispose();
            return b;
          }
          Log.Debug("FrameGrabber: Frame grab failed");
          return null;
        }
      }
      catch (Exception e) // Can occur for example if the video device is lost
      {
        Log.Debug(e.ToString());
        return null;
      }
    }

    /// <summary>
    /// Suggests that the FrameGrabber releases resources.
    /// </summary>
    public void Clean()
    {
      if (rgbSurface != null)
      {
        rgbSurface.Dispose();
        rgbSurface = null;
      }
    }

    /// <summary>
    /// Callback that gives the framegrabber a chance to grab the frame, 
    /// returns immediatly if no one is requesting a frame grab
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="arWidth"></param>
    /// <param name="arHeight"></param>
    /// <param name="pSurface"></param>
    public void OnFrame(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pSurface)
    {
      // Is GetCurrentImage() requesting a frame grab?
      if (!grabSample || width == 0 || height == 0)
      {
        return;
      }

      //Log.Debug("PlaneScene: grabSample is true");
      try
      {
        // if we havent already allocated a surface or the surface dimensions dont match
        // allocate a new surface to store the grabbed frame in
        if (rgbSurface == null || rgbSurface.Disposed || rgbSurface.Description.Height != height ||
            rgbSurface.Description.Width != width)
        {
          Log.Debug("FrameGrabber: Creating new frame grabbing surface");

          rgbSurface = GUIGraphicsContext.DX9Device.CreateRenderTarget(width, height, Format.A8R8G8B8,
                                                                         MultiSampleType.None, 0, true);
        }
        unsafe
        {
          // copy the YUV video surface to our managed ARGB surface
          // Log.Debug("Calling VideoSurfaceToRGBSurface");
          VideoSurfaceToRGBSurface(new IntPtr(pSurface), (IntPtr)rgbSurface.UnmanagedComPointer);
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
          rgbSurface.Dispose(); // get rid of rgbSurface just to make sure
          rgbSurface = null;
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