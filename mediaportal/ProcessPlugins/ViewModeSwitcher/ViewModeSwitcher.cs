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
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using MediaPortal;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace ProcessPlugins.ViewModeSwitcher
{
  [PluginIcons("ProcessPlugins.ViewModeSwitcher.ViewModeSwitcherIconEnabled.png",
    "ProcessPlugins.ViewModeSwitcher.ViewModeSwitcherIconDisabled.png")]
  public class ViewModeSwitcher : PlugInBase, IAutoCrop
  {
    public static readonly ViewModeswitcherSettings currentSettings = new ViewModeswitcherSettings();
    private float LastSwitchedAspectRatio; // stores the last automatically set ratio. 
    private readonly FrameGrabber grabber = FrameGrabber.GetInstance();
    private FrameAnalyzer analyzer = null;
    private bool LastDetectionResult = true;
    private bool ViewModeSwitcherEnabled = true;
    private ViewModeSwitcher instance;
    private AutoResetEvent workerEvent = new AutoResetEvent(false);
    private bool stopWorkerThread = false;
    private bool isPlaying = false;
    private bool enableLB = false;
    private CropSettings cropSettings = new CropSettings();
    private int overScan = 0;
    private float PixelAspectRatio; // Video pixel aspect ratio 
    private bool isVideoReceived = false;
    private AutoResetEvent videoRecvEvent = new AutoResetEvent(false);
    private bool isAutoCrop = false;
    private Geometry.Type LastGeometry = Geometry.Type.Normal;


    /// <summary>
    /// Implements IAutoCrop.Crop, executing a manual crop
    /// </summary>
    /// <returns>A string to display to the user</returns>
    public string Crop()
    {
      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: Performing manual crop");
      }
      enableLB = true;
      LastDetectionResult = false;
      workerEvent.Set();
      return "BB detect...";
    }

    /// <summary>
    /// Implements IAutoCrop.ToggleMode, toggling the mode
    /// of the autocropper (ie from Off-> Dynamic or similar)
    /// </summary>
    /// <returns>A string to display to the user</returns>
    public string ToggleMode()
    {
      if (!isAutoCrop)
      {
        Log.Debug("ViewModeSwitcher: auto crop ON");
        enableLB = true;
        isAutoCrop = true;
        LastDetectionResult = false;
        workerEvent.Set();
        return "BB detect > Auto";
      }
      else
      {
        Log.Debug("ViewModeSwitcher: auto crop OFF");
        enableLB = true;
        isAutoCrop = false;
        LastDetectionResult = false;
        workerEvent.Set();
        return "BB detect > Manual";
      }      
    }

    /// <summary>
    /// Handles the g_Player.PlayBackEnded event
    /// </summary>
    /// <param name="type"></param>
    /// <param name="s"></param>
    public void OnVideoEnded(g_Player.MediaType type, string s)
    {
      // do not handle e.g. visualization window, last.fm player, etc
      if (type == g_Player.MediaType.Music)
      {
        return;
      }

      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: On Video Ended");
      }
      LastSwitchedAspectRatio = 0f;
      PixelAspectRatio = 0f;
      LastGeometry = Geometry.Type.Normal;
      LastDetectionResult = true;
      isPlaying = false;
      isVideoReceived = false;
    }

    /// <summary>
    /// Handles the g_Player.PlayBackStopped event
    /// </summary>
    /// <param name="type"></param>
    /// <param name="i"></param>
    /// <param name="s"></param>
    public void OnVideoStopped(g_Player.MediaType type, int i, string s)
    {
      // do not handle e.g. visualization window, last.fm player, etc
      if (type == g_Player.MediaType.Music)
      {
        return;
      }

      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: On Video Stopped");
      }
      LastSwitchedAspectRatio = 0f;
      PixelAspectRatio = 0f;
      LastGeometry = Geometry.Type.Normal;
      LastDetectionResult = true;
      isPlaying = false;
      isVideoReceived = false;
    }

    /// <summary>
    /// Handles the g_Player.PlayBackStarted event
    /// </summary>
    public void OnVideoStarted(g_Player.MediaType type, string s)
    {
      // do not handle e.g. visualization window, last.fm player, etc
      if (type == g_Player.MediaType.Music)
      {
        return;
      }

      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: On Video Started");
      }
      LastDetectionResult = true;
      isPlaying = true;
    }

    /// <summary>
    /// Handles the g_Player.OnTvChannelChangeHandler event
    /// </summary>
    public void OnTVChannelChanged()
    {
      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: OnTVChannelChange... Reset rule processing!");
      }
      LastSwitchedAspectRatio = 0f;
      PixelAspectRatio = 0f;
      LastGeometry = Geometry.Type.Normal;
      LastDetectionResult = true;
      isVideoReceived = false;
    }

    /// <summary>
    /// starts the process plugin
    /// </summary>
    public override void Start()
    {
      Log.Debug("ViewModeSwitcher: Start()");

      if (!currentSettings.LoadSettings())
      {
        Log.Info("ViewModeSwitcher: No enabled rule found. Process stopped!");
        return;
      }
      instance = this;
      analyzer = new FrameAnalyzer();
      GUIGraphicsContext.autoCropper = this;
      GUIGraphicsContext.OnVideoReceived += new VideoReceivedHandler(OnVideoReceived);
      g_Player.PlayBackEnded += OnVideoEnded;
      g_Player.PlayBackStopped += OnVideoStopped;
      g_Player.PlayBackStarted += OnVideoStarted;
      g_Player.TVChannelChanged += OnTVChannelChanged;
      
      isAutoCrop = currentSettings.UseAutoLBDetection;

      // start the thread that will execute the actual cropping
      Thread t = new Thread(new ThreadStart(instance.Worker));
      t.IsBackground = true;
      t.Priority = ThreadPriority.BelowNormal;
      t.Name = "ViewModeSwitcher";
      t.Start();
    }

    /// <summary>
    /// stops the process plugin
    /// </summary>
    public override void Stop()
    {
      stopWorkerThread = true;
      workerEvent.Set();
      Log.Debug("ViewModeSwitcher: Stop()");
    }

    private void Worker()
    {
      bool debugToggle = false;
      int loopCount = 0;
      
      while (true)
      {
        if (stopWorkerThread)
        {
          stopWorkerThread = false;
          return;
        }
        if (isVideoReceived)
        {
          if (debugToggle)  
          {
            Log.Debug("ViewModeSwitcher: Reset AR to original");
            GUIGraphicsContext.ARType = Geometry.Type.Zoom14to9;
            SetCropMode(); //force AR type update
            LastSwitchedAspectRatio = 0f;
            LastDetectionResult = false;
          }     
          else
          {     
            CheckAspectRatios();
            if ((loopCount%4) == 0)
            {
              if (isAutoCrop)
              {
                LastDetectionResult = false;
              }
              if (!currentSettings.DisableLBGlobaly && !LastDetectionResult && enableLB)
              {
                if (currentSettings.verboseLog)
                {
                  Log.Debug("ViewModeSwitcher: Black Bar detect");
                }
                LastDetectionResult = true;
                SingleCrop();
              }
            }
          }
          //debugToggle = !debugToggle;   
          loopCount++;
          videoRecvEvent.Set();
          workerEvent.WaitOne(247); // reset automatically - timeout after 247ms wait           
        }   
        else
        {
          workerEvent.WaitOne(); // reset automatically - sleep until triggered      
        }   
      }
    }

    /// <summary>
    /// checks if a rule is fitting and executes it
    /// </summary>    
    private void ProcessRules()
    {
      bool updateCrop = false;
      int width = VMR9Util.g_vmr9.VideoWidth;
      int height = VMR9Util.g_vmr9.VideoHeight;
      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: VideoAspectRatioX " + VMR9Util.g_vmr9.VideoAspectRatioX);
        Log.Debug("ViewModeSwitcher: VideoAspectRatioY " + VMR9Util.g_vmr9.VideoAspectRatioY);
        Log.Debug("ViewModeSwitcher: VideoWidth " + width);
        Log.Debug("ViewModeSwitcher: VideoHeight " + height);
        Log.Debug("ViewModeSwitcher: AR type:" + GUIGraphicsContext.ARType + " AR Calc: " + LastSwitchedAspectRatio);
      }

      int currentRule = -1;
      overScan = 0;
      enableLB = false;
      for (int i = 0; i < currentSettings.ViewModeRules.Count; i++)
      {
        Rule tmpRule = currentSettings.ViewModeRules[i];

        if (tmpRule.Enabled
            && LastSwitchedAspectRatio >= tmpRule.ARFrom
            && LastSwitchedAspectRatio <= tmpRule.ARTo
            && width >= tmpRule.MinWidth
            && width <= tmpRule.MaxWidth
            && height >= tmpRule.MinHeight
            && height <= tmpRule.MaxHeight)
        {
          currentRule = i;

          Log.Info("ViewModeSwitcher: Rule \"" + tmpRule.Name + "\" fits conditions.");
          if (tmpRule.EnableLBDetection)
          {
            enableLB = true;
          }
          if (tmpRule.ChangeOs || !enableLB)
          {
            overScan = tmpRule.OverScan;
            Crop(overScan);
            updateCrop = true;
          }
          if (tmpRule.ChangeAR)
          {
            SetAspectRatio(tmpRule.Name, tmpRule.ViewMode);
            updateCrop = true;
          }
          break; // do not process any additional rule after a rule fits (better for offset function)
        }
      }
      //process the fallback rule if no other rule has fitted
      if (currentSettings.UseFallbackRule && currentRule == -1 && height != 100 && width != 100)
      {
        Log.Info("ViewModeSwitcher: Processing the fallback rule!");
        Crop(currentSettings.fboverScan);
        SetAspectRatio("Fallback rule", currentSettings.FallBackViewMode);
        updateCrop = true;
      }
      if (updateCrop)
      {
        SetCropMode();
      }
    }

    private void Crop(int crop)
    {
      if (crop > 0)
      {
        cropSettings.Top = (int)((float)crop / LastSwitchedAspectRatio);
        cropSettings.Bottom = (int)((float)crop / LastSwitchedAspectRatio);
        cropSettings.Left = crop;
        cropSettings.Right = crop;
      }
      else
      {
        cropSettings.Top = currentSettings.CropTop;
        cropSettings.Bottom = currentSettings.CropBottom;
        cropSettings.Left = currentSettings.CropLeft;
        cropSettings.Right = currentSettings.CropRight;
      }
    }

    /// <summary>
    /// This method handles the GUIGraphicsContext.OnVideoReceived() calls
    /// </summary>
    private void OnVideoReceived()
    {
      if (!isVideoReceived)
      {
        Log.Debug("ViewModeSwitcher: OnVideoReceived()");
        isVideoReceived = true;
        videoRecvEvent.Reset();
        workerEvent.Set();
        videoRecvEvent.WaitOne();
      }
    }

    /// <summary>
    /// Changes the aspect ratio of MediaPortal
    /// </summary>
    /// <param name="MessageString">Message text of the switch message</param>
    /// <param name="AR">the aspect ratio to switch to</param>
    private bool SetAspectRatio(string MessageString, Geometry.Type AR)
    {
      if (LastGeometry == AR)
      {
        return false;
      }
        
      Log.Info("ViewModeSwitcher: Switching to viewmode: " + AR);
      GUIGraphicsContext.ARType = AR;
      LastGeometry = AR;

      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        GUIMessage guiMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED, 0, 0, 0, 0, 0, null);
        guiMsg.Label = "ViewModeSwitcher";
        
        if (!enableLB || currentSettings.DisableLBGlobaly)
        {
          guiMsg.Label2 = (MessageString + " > " + AR + " | BB detect > Off");
        }
        else if (!isAutoCrop)
        {
          guiMsg.Label2 = (MessageString + " > " + AR + " | BB detect > Manual");
        }
        else
        {
          guiMsg.Label2 = (MessageString + " > " + AR + " | BB detect > Auto");
        }  
            
        guiMsg.Param1 = 5;

        GUIGraphicsContext.SendMessage(guiMsg);
      }   
      
      return true;   
    }

    /// <summary>
    /// set the oversan of MediaPortal by setting crop parameters
    /// </summary>
    private void SetCropMode()
    {
      CropSettings tmpCropSettings = new CropSettings();
      
      tmpCropSettings.Left   = cropSettings.Left;
      tmpCropSettings.Right  = cropSettings.Right;
      tmpCropSettings.Top    = cropSettings.Top;
      tmpCropSettings.Bottom = cropSettings.Bottom;

      if (PixelAspectRatio > 0f && LastSwitchedAspectRatio > 0f)
      {
        //Correction to crop settings for anamorphic video i.e. when pixel aspect ratio != video aspect ratio
        Log.Debug("ViewModeSwitcher: SetCropMode() Anamorph factor: {0}", (LastSwitchedAspectRatio/PixelAspectRatio));
        tmpCropSettings.Left  = (int)((float)tmpCropSettings.Left  / (LastSwitchedAspectRatio/PixelAspectRatio));
        tmpCropSettings.Right = (int)((float)tmpCropSettings.Right / (LastSwitchedAspectRatio/PixelAspectRatio));
      }
      GUIMessage msg = new GUIMessage();
      msg.Message = GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP;
      msg.Object = tmpCropSettings;
      GUIWindowManager.SendMessage(msg);
    }

    /// <summary>
    /// Finds the bounds and crops accordingly immediately
    /// </summary>
    private void SingleCrop()
    {
      bool updateCrop = false;
      
      if (GUIGraphicsContext.RenderBlackImage)
      {
        LastDetectionResult = false;
        return;
      }

      Bitmap frame = grabber.GetCurrentImage();

      if (frame == null || frame.Height == 0 || frame.Width == 0)
      {
        LastDetectionResult = false;
        return;
      }

      Rectangle bounds = new Rectangle();

      if (!analyzer.FindBounds(frame, ref bounds))
      {
        LastDetectionResult = false;
        return;
      }

      int cropH = Math.Min(GUIGraphicsContext.VideoSize.Width - bounds.Right, bounds.Left);
      if (cropH < overScan)
        cropH = overScan;

      int cropV = Math.Min(GUIGraphicsContext.VideoSize.Height - bounds.Bottom, bounds.Top);
      if (cropV < overScan / LastSwitchedAspectRatio)
        cropV = (int)((float)overScan / LastSwitchedAspectRatio);

      if (cropV > cropH / LastSwitchedAspectRatio)
        cropV = (int)((float)cropH / LastSwitchedAspectRatio);

      float newasp = (float)(frame.Width - cropH * 2) / (float)(frame.Height - cropV * 2);
      if (newasp < 1) // faulty crop
      {
        cropH = overScan;
        cropV = (int)((float)overScan / LastSwitchedAspectRatio);
      }
      else // (newasp >= 1)
      {
        float asp = (float)(frame.Width) / (float)(frame.Height);
        //Log.Debug("asp: {0}, newasp: {1}", asp, newasp);      
        if (Math.Abs(asp - newasp) > 0.2 && LastSwitchedAspectRatio > 1.5)
        {
          if (SetAspectRatio("4:3 inside 16:9", Geometry.Type.NonLinearStretch))
          {
            updateCrop = true;
          }
          cropH = overScan;
          cropV = (int)((float)overScan / LastSwitchedAspectRatio);
        }
      } 

      if ((Math.Abs(cropH - cropSettings.Left) > 1) || (Math.Abs(cropV - cropSettings.Top) > 1))
      {
        cropSettings.Top = cropV; //bounds.Top;
        cropSettings.Bottom = cropV; // GUIGraphicsContext.VideoSize.Height - (bounds.Bottom + 1);
        cropSettings.Left = cropH; // bounds.Left;
        cropSettings.Right = cropH; // GUIGraphicsContext.VideoSize.Width - (bounds.Right + 1);
        updateCrop = true;
      }      
          
      if (updateCrop)
      {
        SetCropMode();
      }

      frame.Dispose();
      frame = null;
    }
       
        /// <summary>
    /// This method runs in a thread and calculates the aspect ratio 
    /// checks if a rule is affected
    /// </summary>
    private void CheckAspectRatios()
    {
      if (ViewModeSwitcherEnabled && VMR9Util.g_vmr9 != null && VMR9Util.g_vmr9.VideoAspectRatioX != 0 &&
          VMR9Util.g_vmr9.VideoAspectRatioY != 0)
      {
        if (VMR9Util.g_vmr9.VideoWidth != 0 && VMR9Util.g_vmr9.VideoHeight != 0 )
        {
          PixelAspectRatio = (float)VMR9Util.g_vmr9.VideoWidth / (float)VMR9Util.g_vmr9.VideoHeight;
        }
        // calculate the current aspect ratio
        float aspectRatio = (float)VMR9Util.g_vmr9.VideoAspectRatioX / (float)VMR9Util.g_vmr9.VideoAspectRatioY;
        if (aspectRatio != LastSwitchedAspectRatio && isPlaying)
        {
          Log.Debug("ViewModeSwitcher: CheckAspectRatios() Video AR: {0}, Last Video AR: {1}, Pixel AR: {2}", aspectRatio, LastSwitchedAspectRatio, PixelAspectRatio);
          LastSwitchedAspectRatio = aspectRatio;
          ProcessRules();
          LastDetectionResult = false;
        }
      }
    }

  }
}