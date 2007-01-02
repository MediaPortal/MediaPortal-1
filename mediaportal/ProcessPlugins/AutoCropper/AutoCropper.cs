#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MediaPortal.GUI.Library;
using System.Drawing.Imaging;
using MediaPortal.Player;
using System.Threading;
using MediaPortal.TV.Recording;
using WindowPlugins.AutoCropper;
using System.Collections;
using MediaPortal;

namespace ProcessPlugins.AutoCropper
{
  /// <summary>
  /// Provides autocropping functionality by using FrameAnalyzer to analyze grabbes frames.
  /// </summary>
  public class AutoCropper : ProcessPlugins.AutoCropper.PlugInBase, MediaPortal.Player.IAutoCrop
  {
    private Mode mode = Mode.OFF;
    private int sampleInterval = 500; // in milliseconds
    private AutoResetEvent workerEvent = new AutoResetEvent(false);
    private static AutoCropper instance;
    private MovingAverage topCropAvg = null;
    private MovingAverage bottomCropAvg = null;
    private CropSettings lastSettings = null;
    private FrameAnalyzer analyzer = null;
    private bool firstDynamicCrop = true;
    private bool autoEnabled = false;
    private bool manualEnabled = false;
    private int bottomMemLength = 100; // in seconds
    private int topMemLength = 50; // in seconds
    private bool useForMyVideos = false;
    private bool verboseLog = false;
    private bool stopWorkerThread = false;
    private MediaPortal.FrameGrabber grabber = FrameGrabber.GetInstance();

    enum Mode // Enum for cropping modes
    {
      MANUAL = 1, // "Manual", only on user request
      OFF = 2,    // disabled
      DYNAMIC = 3 // poll at specific interval
    }

    private List<Mode> allowedModes = null; // holds allowed crop modes

    /// <summary>
    /// Implements IAutoCrop.ToggleMode, toggling the mode
    /// of the autocropper (ie from Off-> Dynamic or similar)
    /// </summary>
    /// <returns>A string to display to the user</returns>
    public string ToggleMode()
    {

      Log.Debug(allowedModes.ToString());
      for (int i = 0; i < allowedModes.Count; i++)
      {
        if (allowedModes[i] == mode)
        {
          mode = allowedModes[(i + 1) % allowedModes.Count];
          break;
        }
      }
      firstDynamicCrop = true; // will cause reset of dynamic settings
      switch (mode)
      {
        case Mode.MANUAL:
          mode = Mode.MANUAL;
          workerEvent.Set();
          return "Manual";
          break;
        case Mode.DYNAMIC:
          mode = Mode.DYNAMIC;
          workerEvent.Set();
          return "Auto";
          break;
        case Mode.OFF:
          mode = Mode.OFF;
          workerEvent.Set();
          return "Off";
          break;
      }
      return "Error";
    }

    /// <summary>
    /// Implements IAutoCrop.Crop, executing a manual crop
    /// </summary>
    /// <returns>A string to display to the user</returns>
    public string Crop()
    {
      if (verboseLog) Log.Debug("AutoCropper: Performing manual crop");
      if (mode == Mode.MANUAL)
      {
        workerEvent.Set();
        return "Cropping";
      }
      else return "N/A";
    }

    /// <summary>
    ///  Event handler for TV playback start
    /// </summary>
    /// <param name="j"></param>
    /// <param name="dev"></param>
    public void OnTVStarted(int j, TVCaptureDevice dev)
    {
      if (verboseLog) Log.Debug("AutoCropper: On TV Playback Started");
      if (mode == Mode.DYNAMIC)
      {
        workerEvent.Set();
      }
    }

    /// <summary>
    ///  Event handler for video playback start
    /// </summary>
    /// <param name="type"></param>
    /// <param name="s"></param>

    public void OnVideoStarted(g_Player.MediaType type, string s)
    {
      // do not handle e.g. visualization window, last.fm player, etc
      if (type == g_Player.MediaType.Music)
        return;

      if (verboseLog) Log.Debug("AutoCropper: On Video Started");
      if (mode == Mode.DYNAMIC)
      {
        workerEvent.Set();
      }
    }

    /// <summary>
    /// Used for checking whether there is any purpose to cropping
    /// </summary>
    /// <returns></returns>
    private bool IsPlaying()
    {
      return g_Player.Playing || MediaPortal.TV.Recording.Recorder.IsViewing();
    }

    /// <summary>
    /// This method runs in a thread and calls the actual cropping methods
    ///  DynamicCrop and SingleCrop.
    /// </summary>
    private void Worker()
    {
      while (true)
      {
        if (verboseLog) Log.Debug("AutoCropper: Mode : " + mode + " IsPlaying : " + IsPlaying());
        if (stopWorkerThread)
        {
          stopWorkerThread = false;
          return;
        }
        if (mode == Mode.DYNAMIC && IsPlaying())
        {
          // do processing for dynamic cropping and sleep
          // a short while
          DynamicCrop();
          //Bitmap b = PlaneScene.GetCurrentImage();
          Thread.Sleep(sampleInterval);
        }
        else if (mode == Mode.MANUAL && IsPlaying())
        {
          // crop once and wait to be woken up again
          SingleCrop();
          workerEvent.WaitOne(); // reset automatically
        }
        else if (mode == Mode.OFF && IsPlaying())
        {
          CropSettings noCrop = new CropSettings(0, 0, 0, 0);
          RequestCrop(noCrop);
          workerEvent.WaitOne();
        }
        else
        {
          if (verboseLog) Log.Debug("AutoCropper: Worker halting, waiting for worker event");
          ResetDynamic();
          workerEvent.WaitOne(); // reset automatically
        }
      }
    }

    /// <summary>
    /// Loads settings from the configuration file, and sets up allowed modes.
    /// </summary>
    /// <returns>False if the autocropper has no allowed modes and true otherwise</returns>
    private bool LoadSettings()
    {
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
      {
        //bool enabled = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.enableAutoCropSetting, false);
        verboseLog = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmVerboseLog, false);
        useForMyVideos = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmUseForMyVideos, false);
        autoEnabled = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.enableAutoModeSetting, false);
        manualEnabled = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.enableManualModeSetting, true);
        bottomMemLength = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmBottomMemoryLength, 100);
        topMemLength = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmTopMemoryLength, 50);
        sampleInterval = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmSampleInterval, 500);

        allowedModes = new List<Mode>();
        allowedModes.Add(Mode.OFF);

        if (manualEnabled)
        {
          allowedModes.Add(Mode.MANUAL);
        }
        if (autoEnabled)
        {
          allowedModes.Add(Mode.DYNAMIC);
        }

        if (reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmDefaultModeIsManual, true))
        {
          mode = Mode.MANUAL;
        }
        else if (autoEnabled)
        {
          mode = Mode.DYNAMIC;
        }
        else
        {
          mode = Mode.OFF;
        }
        return autoEnabled || manualEnabled;
      }
    }

    /// <summary>
    /// Implements PlugInBase.Start
    /// Sets up the autocropper
    /// and sets GUIGraphicsContext.autoCropper
    /// to point to this object.
    /// </summary>
    public override void Start()
    {
      Log.Debug("AutoCropper: Start()");
      AutoCropper.instance = this;

      analyzer = new FrameAnalyzer();

      // Load settings, if returns false,
      // none of autocropper modes are allowed
      // so we dont do anything
      if (!LoadSettings()) return;

      GUIGraphicsContext.autoCropper = this;

      lastSettings = new CropSettings(0, 0, 0, 0);
      int topMemLengthInFrames = (int)(topMemLength / (sampleInterval / 1000.0f));
      int bottomMemLengthInFrames = (int)(bottomMemLength / (sampleInterval / 1000.0f));
      Log.Debug("AutoCropper: Top memory is " + topMemLengthInFrames + " sampleinterval " + sampleInterval + " mem length " + topMemLength);
      Log.Debug("AutoCropper: Bottom memory is " + bottomMemLengthInFrames + " sampleinterval " + sampleInterval + " mem length " + bottomMemLength);
      topCropAvg = new MovingAverage(topMemLengthInFrames, 0);
      bottomCropAvg = new MovingAverage(bottomMemLengthInFrames, 0);

      // start the thread that will execute the actual cropping
      Thread t = new Thread(new ThreadStart(instance.Worker));
      t.IsBackground = true;
      t.Priority = ThreadPriority.BelowNormal;
      t.Name = "AutoCropThread";
      t.Start();

      // register to handle playback events so we can wake
      // the above thread when playback starts
      Recorder.OnTvViewingStarted += new Recorder.OnTvViewHandler(OnTVStarted);
      if (useForMyVideos)
      {
        g_Player.PlayBackStarted += new g_Player.StartedHandler(OnVideoStarted);
      }
    }

    /// <summary>
    /// Implements PlugInBase.Stop
    /// Stops the worker thread, and removes the reference to this autocropper
    /// from GUIGraphicsContext
    /// </summary>
    public override void Stop()
    {
      Log.Debug("AutoCropper: Stop()");
      stopWorkerThread = true;
      GUIGraphicsContext.autoCropper = null;
    }


    /// <summary>
    /// Uses the static cropping system to execute the cropping
    /// </summary>
    private void RequestCrop(MediaPortal.Player.CropSettings cropSettings)
    {
      if (verboseLog) Log.Debug("AutoCropper: RequestCrop");
      // Send message to planescene with crop
      MediaPortal.GUI.Library.GUIMessage msg = new GUIMessage();
      msg.Message = GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP;
      msg.Object = cropSettings;
      GUIWindowManager.SendMessage(msg);
      lastSettings.Bottom = cropSettings.Bottom;
      lastSettings.Top = cropSettings.Top;
    }


    /// <summary>
    /// Get the last frame of video from the Video Mixing Renderer
    ///  (only works for WMR9 Renderless)
    /// </summary>
    /// <returns></returns>
    private Bitmap GetFrame()
    {
      //Log.Debug("GetFrame");
      return grabber.GetCurrentImage();
    }




    /// <summary>
    ///      Crops dynamically based on previous history
    /// </summary>
    private void DynamicCrop()
    {
      if (verboseLog) Log.Debug("DynamicCrop");
      Bitmap frame = GetFrame();
      if (frame == null) return;
      Rectangle bounds = new Rectangle();

      if (!analyzer.FindBounds(frame, ref bounds)) return;

      int topCrop = bounds.Top;
      int bottomCrop = frame.Height - bounds.Height - topCrop;

      if (bottomCrop < 0) Log.Error("bottomCrop <0");
      //MinDynamicCrop(topCrop, bottomCrop);
      AvgDynamicCrop(topCrop, bottomCrop);

      /*long id = DateTime.Now.Ticks;

      Log.Debug("{0} : top {1}, bot {2}", id, topCrop, bottomCrop);
      frame.Save("C:\\" +  id +".bmp");
      Log.Debug("Current top" + lastSettings.Top);
      Log.Debug("Current bottom" + lastSettings.Bottom);*/
      frame.Dispose();
      frame = null;
    }

    /// <summary>
    /// Performs dynamic cropping based on the largest bounding box
    /// encountered (ie smallest cropping) within the 'memory' of the 
    /// autocropper
    /// </summary>
    /// <param name="topCrop">Cropping found in current frame</param>
    /// <param name="bottomCrop">Cropping found in current frame</param>
    private void MinDynamicCrop(int topCrop, int bottomCrop)
    {
      if (firstDynamicCrop)
      {
        if (verboseLog) Log.Debug("First dynamic crop, resetting to top {0}, bottom {1}", topCrop, bottomCrop);
        topCropAvg.Reset(topCrop);
        bottomCropAvg.Reset(bottomCrop);
        firstDynamicCrop = false;
      }
      else
      {
        topCropAvg.Add(topCrop);
        bottomCropAvg.Add(bottomCrop);
      }

      int topMin = (int)topCropAvg.GetMin();
      int bottomMin = (int)bottomCropAvg.GetMin();

      if (verboseLog)
      {
        Log.Debug("Current topMin {0}, bottomMin {1}", topMin, bottomMin);
      }

      if (Math.Abs(topMin - lastSettings.Top) > 2 || Math.Abs(bottomMin - lastSettings.Bottom) > 2)
      {
        CropSettings newSettings = new CropSettings();
        newSettings.Top = topMin;
        newSettings.Bottom = bottomMin;
        RequestCrop(newSettings);
      }
    }

    /// <summary>
    /// Performs dynamic cropping based on a moving average or the
    /// cropping the analyzer has produced so far within the memory.
    /// </summary>
    /// <param name="topCrop">Cropping found in current frame</param>
    /// <param name="bottomCrop">Cropping found in current frame</param>
    private void AvgDynamicCrop(int topCrop, int bottomCrop)
    {
      CropSettings newSettings = new CropSettings();
      newSettings.Top = lastSettings.Top;
      newSettings.Bottom = lastSettings.Bottom;
      bool update = false;

      // debug

      //newSettings.Top = topCrop;
      //newSettings.Bottom = bottomCrop;
      //RequestCrop(newSettings);
      //return;
      // end debug

      // If the image area has increase immediatly reset to this larger size
      if (topCrop < lastSettings.Top || firstDynamicCrop)
      {
        if (verboseLog) Log.Debug("Top of image has increased");
        update = true;
        topCropAvg.Reset(topCrop);
        newSettings.Top = topCrop;
      }
      else
      {
        topCropAvg.Add(topCrop);
      }

      // If the image area has increase immediatly reset to this larger size
      if (bottomCrop < lastSettings.Bottom || firstDynamicCrop)
      {
        if (verboseLog) Log.Debug("Bottom of image has increased");
        bottomCropAvg.Reset(bottomCrop);
        newSettings.Bottom = bottomCrop;
        update = true;
      }
      else
      {
        bottomCropAvg.Add(bottomCrop);
      }

      firstDynamicCrop = false;

      if (verboseLog) Log.Debug("This Frame: {0} / {1}, Avg: {4}/{5} Current Crop: {2}/{3}", topCrop, bottomCrop, lastSettings.Top, lastSettings.Bottom, topCropAvg.Average, bottomCropAvg.Average);

      if (topCropAvg.Average - lastSettings.Top > 4 && Math.Abs(topCropAvg.Average - topCrop) < 2)
      {
        newSettings.Top = (int)topCropAvg.Average;
        update = true;
      }
      if (topCropAvg.Average - lastSettings.Top > 4 && Math.Abs(topCropAvg.Average - topCrop) < 2)
      {
        newSettings.Bottom = (int)bottomCropAvg.Average;
        update = true;
      }

      if (update && (newSettings.Top != lastSettings.Top || newSettings.Bottom != lastSettings.Bottom))
      {
        RequestCrop(newSettings);
      }
    }

    /// <summary>
    /// Reset 'memory' of dynamic mode (for example when changing channels etc
    /// </summary>
    private void ResetDynamic()
    {
      bottomCropAvg.Reset(0);
      topCropAvg.Reset(0);
      firstDynamicCrop = true;
    }

    /// <summary>
    /// Finds the bounds an crops accordingly immediatly
    /// </summary>
    private void SingleCrop()
    {
      // are we viewing video?
      if (!IsPlaying()) return;

      Bitmap frame = GetFrame();

      if (frame == null)
      {
        Log.Warn("AutoCropper Failed to get frame (==null), aborting");
        return;
      }

      Rectangle bounds = new Rectangle();

      if (!analyzer.FindBounds(frame, ref bounds))
      {
        return;
      }

      CropSettings cropSettings = new CropSettings();
      cropSettings.Top = bounds.Top;
      cropSettings.Bottom = GUIGraphicsContext.VideoSize.Height - (bounds.Bottom + 1);

      RequestCrop(cropSettings);
      frame.Dispose();
      frame = null;
    }

  }
}