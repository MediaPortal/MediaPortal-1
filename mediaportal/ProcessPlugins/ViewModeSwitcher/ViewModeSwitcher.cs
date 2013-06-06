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
//    private CropSettings cropSettings = new CropSettings();
    private float overScan = 0f;
    private float LastAnamorphFactor; // Video anamorphic correction factor (Video AR/Pixel AR) 
    private bool isVideoReceived = false;
    private AutoResetEvent videoRecvEvent = new AutoResetEvent(false);
    private Geometry.Type LastSwitchedGeometry = Geometry.Type.Normal;
    private bool isPillarBox = false;
    private bool updatePending = false;    
    private string NewGeometryMessage = " ";
    private float fCropH = 0f; // stores the last hor crop value. 
    private float fCropV = 0f; // stores the last vertical crop value. 
    private float fWidthH = 0f; // stores the last 'real' video width value. 
    private float fHeightV = 0f; // stores the last 'real' video height value. 
    private bool workerBusy = false;
    private bool useMaxCrop = false;
    private bool useAutoCrop = false;
    private bool forceAutoCrop = false;
    private int LastWidth = 0; // stores the last frame grab width value. 
    private int LastHeight = 0; // stores the last frame grab height value. 


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
      if (!forceAutoCrop)
      {
        Log.Debug("ViewModeSwitcher: auto crop ON");
        enableLB = true;
        forceAutoCrop = true;
        useAutoCrop = true;
        workerEvent.Set();
        return "BB detect > Auto";
      }
      else
      {
        Log.Debug("ViewModeSwitcher: auto crop OFF");
        enableLB = true;
        forceAutoCrop = false;
        useAutoCrop = false;
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
      //Put the worker thread into idle mode
      isPlaying = false;
      videoRecvEvent.Reset();
      workerEvent.Set();
      videoRecvEvent.WaitOne(500);
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
      LastSwitchedGeometry = GUIGraphicsContext.ARType;
      isVideoReceived = false;
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
      isVideoReceived = false;
      videoRecvEvent.Reset();
      workerEvent.Set();
      videoRecvEvent.WaitOne(500);
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
      videoRecvEvent.Reset();
      stopWorkerThread = true;
      workerEvent.Set();
      videoRecvEvent.WaitOne(500);
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
          workerBusy = false;
          stopWorkerThread = false;
          videoRecvEvent.Set();
          return;
        }
        if (isVideoReceived && isPlaying)
        {
          workerBusy = true;
          if (debugToggle)  
          {
            Log.Debug("ViewModeSwitcher: Reset AR to original");
            GUIGraphicsContext.ARType = Geometry.Type.Zoom14to9;
            updatePending = true; //force AR type update
            LastSwitchedAspectRatio = 0f;
            LastDetectionResult = false;
          }     
          else if (!updatePending)
          { 
            if (LastSwitchedGeometry != GUIGraphicsContext.ARType)
            {
                //Geometry (zoom mode) has been changed by user
                //so disable black bar detection and 
                //reset cropping to fallback settings
              Log.Debug("ViewModeSwitcher: Zoom mode changed by user");
              enableLB = false;
              Crop((float)currentSettings.fboverScan);
              LastSwitchedGeometry = GUIGraphicsContext.ARType;
              updatePending = true;   
            }
            
            CheckAspectRatios();
            if ((loopCount%4) == 0)
            {
              if (useAutoCrop)
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
          workerBusy = false;
          LastSwitchedAspectRatio = 0f;
          LastAnamorphFactor = 0f;
          LastDetectionResult = true;
          isPillarBox = false;
          updatePending = false;
          forceAutoCrop = false;
          loopCount = 3; //Perform a black-bar detect one loop cycle after start
          
          if (currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: Worker thread -> idle");
          }
          videoRecvEvent.Set();
          workerEvent.WaitOne(); // reset automatically - sleep until triggered      
        }   
      }
    }

    /// <summary>
    /// checks if a rule is fitting and executes it
    /// </summary>    
    private void ProcessRules()
    {
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
      overScan = 0f;
      enableLB = false;
      useAutoCrop = forceAutoCrop;
      useMaxCrop = false;
      for (int i = 0; i < currentSettings.ViewModeRules.Count; i++)
      {
        Rule tmpRule = currentSettings.ViewModeRules[i];

        if (tmpRule.Enabled
            && tmpRule.ARFrom > 0f && tmpRule.ARTo > 0f
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
          if (tmpRule.AutoCrop)
          {
            useAutoCrop = true;
          }
          if (tmpRule.MaxCrop)
          {
            useMaxCrop = true;
          }
          if (tmpRule.OverScan > 0)
          {
            overScan = (float)tmpRule.OverScan;
            Crop(overScan);
          }
          else
          {
            overScan = (float)(Math.Min(currentSettings.CropLeft, currentSettings.CropRight));
            Crop(0f);
          }
          SetNewGeometry(tmpRule.Name, tmpRule.ViewMode);
          break; // do not process any additional rule after a rule fits (better for offset function)
        }
      }
      
      if (currentRule == -1) //No rule match
      {
        //process the fallback rule if enabled
        if (currentSettings.UseFallbackRule)
        {
          Log.Info("ViewModeSwitcher: Processing the fallback rule!");
          if (currentSettings.fboverScan > 0)
          {
            overScan = (float)currentSettings.fboverScan;
            Crop(overScan);
          }
          else
          {
            overScan = (float)(Math.Min(currentSettings.CropLeft, currentSettings.CropRight));
            Crop(0f);
          }
          SetNewGeometry("Fallback rule", currentSettings.FallBackViewMode);
        }
        else
        {
          Log.Info("ViewModeSwitcher: No rule found!");
          overScan = (float)(Math.Min(currentSettings.CropLeft, currentSettings.CropRight));
          Crop(0f); //Use TV cropping
          SetNewGeometry("No rule", GUIGraphicsContext.ARType); //Do nothing...
        }
      }
      
      updatePending = true;   
    }

    /// <summary>
    /// checks if a rule is fitting and executes it
    /// Used only for processing 'pillar box' video e.g. 4:3 inside 16:9
    /// </summary>    
    private bool CheckRulesPB(float negAR, int width, int height)
    {
      for (int i = 0; i < currentSettings.ViewModeRules.Count; i++)
      {
        Rule tmpRule = currentSettings.ViewModeRules[i];

        if (tmpRule.Enabled
            && tmpRule.ARFrom < 0f && tmpRule.ARTo < 0f
            && ((negAR <= tmpRule.ARFrom && negAR >= tmpRule.ARTo) ||
                (negAR >= tmpRule.ARFrom && negAR <= tmpRule.ARTo))
            && width >= tmpRule.MinWidth
            && width <= tmpRule.MaxWidth
            && height >= tmpRule.MinHeight
            && height <= tmpRule.MaxHeight)
        {

          if (currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: CheckRulesPB(), VideoAR " + negAR + "VideoWidth " + width + "VideoHeight " + height);
            Log.Info("ViewModeSwitcher: CheckRulesPB(), Rule \"" + tmpRule.Name + "\" fits conditions.");
          }
          if (tmpRule.OverScan > 0)
          {
            overScan = (float)tmpRule.OverScan;
          }
          SetNewGeometry(tmpRule.Name, tmpRule.ViewMode);
          return true; // do not process any additional rule after a rule fits
        }
      }
      return false;
    }

    private void Crop(float crop)
    {
      if (crop > 0f && LastSwitchedAspectRatio > 0f)
      {
        fCropH = crop;
        fCropV = crop / LastSwitchedAspectRatio;
      }
      else
      {
        fCropH = (float)(Math.Min(currentSettings.CropLeft, currentSettings.CropRight));
        fCropV = (float)(Math.Min(currentSettings.CropTop, currentSettings.CropBottom));
      }
    }

    /// <summary>
    /// This method handles the GUIGraphicsContext.OnVideoReceived() calls
    /// </summary>
    private void OnVideoReceived()
    {
      if (!isPlaying)
      {
        isVideoReceived = false;
        updatePending = false;
        return;
      }
      if (!isVideoReceived)
      {
        Log.Debug("ViewModeSwitcher: OnVideoReceived()");
        updatePending = false;
        LastSwitchedGeometry = GUIGraphicsContext.ARType;
        isVideoReceived = true;
        workerEvent.Set();
      }
      else if (!workerBusy)
      {
        workerEvent.Set();
      }
      
      if (updatePending)
      {
        if (currentSettings.verboseLog)
        {
          Log.Debug("ViewModeSwitcher: OnVideoReceived(), Updating AR and crop");
        }
        SetAspectRatio(NewGeometryMessage, LastSwitchedGeometry);
        SetCropMode();
        updatePending = false;
      }
    }

    /// <summary>
    /// Sets up a change to aspect ratio of MediaPortal (actually happens later)
    /// </summary>
    /// <param name="MessageString">Message text of the switch message</param>
    /// <param name="AR">the aspect ratio to switch to</param>
    private bool SetNewGeometry(string MessageString, Geometry.Type AR)
    {
      if (LastSwitchedGeometry == AR)
      {
        return false;
      }
      LastSwitchedGeometry = AR;
      NewGeometryMessage = MessageString;  
      return true;
    }

    /// <summary>
    /// Changes the aspect ratio of MediaPortal
    /// </summary>
    /// <param name="MessageString">Message text of the switch message</param>
    /// <param name="AR">the aspect ratio to switch to</param>
    private bool SetAspectRatio(string MessageString, Geometry.Type AR)
    {      
      if (GUIGraphicsContext.ARType == AR)
      {
        return false;
      }
        
      Log.Info("ViewModeSwitcher: Switching to viewmode: " + AR);
      GUIGraphicsContext.ARType = AR;
      
      return true;   
    }

    /// <summary>
    /// set the oversan of MediaPortal by setting crop parameters
    /// </summary>
    private void SetCropMode()
    {           
      float cropH = fCropH;
      float cropV = fCropV;

      if (LastSwitchedAspectRatio > 0f && (LastSwitchedGeometry != Geometry.Type.NonLinearStretch))
      {        
        if (useMaxCrop)
        { 
          if (fCropV > (fCropH / LastSwitchedAspectRatio)) 
          {
            //Adjust horiz crop value to match vertical crop value (without picture distortion)
            float extraHcropLimit = fWidthH * LastAnamorphFactor * 0.0625f;
            cropH = Math.Min(fCropV * LastSwitchedAspectRatio, fCropH + extraHcropLimit);
            cropV = cropH / LastSwitchedAspectRatio;
          }
          else
          {
            float extraVcropLimit = fHeightV * 0.0625f;      
            cropV = Math.Min(fCropH / LastSwitchedAspectRatio, fCropV + extraVcropLimit);
            cropH = cropV * LastSwitchedAspectRatio;
          }           
        }
        else
        {
          if (fCropV < (fCropH / LastSwitchedAspectRatio)) 
          {
            //Adjust horiz crop value to match vertical crop value (without picture distortion)
            cropH = fCropV * LastSwitchedAspectRatio;
          }
          else
          {
            cropV = fCropH / LastSwitchedAspectRatio;
          }
        }       
      }

      if (LastAnamorphFactor > 0f)
      { 
        //Correction to crop settings for anamorphic video i.e. when pixel aspect ratio != video aspect ratio
        //Log.Debug("ViewModeSwitcher: SetCropMode() Anamorph factor: {0}", LastAnamorphFactor);
        cropH /= LastAnamorphFactor;
      }       

      CropSettings tmpCropSettings = new CropSettings();

      tmpCropSettings.Left   = (int)cropH;
      tmpCropSettings.Right  = tmpCropSettings.Left;
      tmpCropSettings.Top    = (int)cropV;
      tmpCropSettings.Bottom = tmpCropSettings.Top;
     
      GUIMessage msg = new GUIMessage();
      msg.Message = GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP;
      msg.Object = tmpCropSettings;
      GUIWindowManager.SendMessage(msg);

      if (GUIGraphicsContext.IsFullScreenVideo && currentSettings.ShowSwitchMsg)
      {
        GUIMessage guiMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED, 0, 0, 0, 0, 0, null);
        guiMsg.Label = "ViewModeSwitcher";        
        guiMsg.Label2 = ("ViewMode: " + LastSwitchedGeometry + ", AR: " + LastSwitchedAspectRatio + ", Bar detect: " + (enableLB && !currentSettings.DisableLBGlobaly) + ", H crop: " + tmpCropSettings.Left + ", V crop: " + tmpCropSettings.Top);            
        guiMsg.Param1 = 3;

        GUIGraphicsContext.SendMessage(guiMsg);
      }   
    }

    /// <summary>
    /// Finds the bounds and crops accordingly immediately
    /// </summary>
    private void SingleCrop()
    {
      bool updateCrop = false;
      
      if (LastSwitchedAspectRatio < 0.1f)
      {
        LastDetectionResult = false;
        return;
      }
      
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
        frame.Dispose();
        frame = null;
        return;
      }
      
      float Hsym = 1.0f;
      float Vsym = 1.0f;
      if (bounds.Left > 20 || ((frame.Width - bounds.Right) > 20 && bounds.Left > 0))
      {
        Hsym = (float)(frame.Width - bounds.Right)/(float)bounds.Left;
      }
      if (bounds.Top > 20 || ((frame.Height - bounds.Bottom) > 20 && bounds.Top > 0))
      {
        Vsym = (float)(frame.Height - bounds.Bottom)/(float)bounds.Top;
      }
      
      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: SingleCrop(), Detected BB -> left: {0}, right: {1}, top: {2}, bottom: {3}, Hsym: {4}, Vsym: {5}", bounds.Left, (frame.Width - bounds.Right), bounds.Top, (frame.Height - bounds.Bottom), Hsym, Vsym); 
      }
      
      //Check for symmetry of black bars - asymmetric bars are probably a false detection
      if ((Hsym > 1.1) || (Hsym < 0.9) || (Vsym > 1.1) || (Vsym < 0.9))
      {
        if (currentSettings.verboseLog)
        {
          Log.Debug("ViewModeSwitcher: SingleCrop(), Symmetry check failed");
        }
        frame.Dispose();
        frame = null;
        return;
      }

      //Use the smallest black bar size
      float cropH = (float)(Math.Min(frame.Width - bounds.Right, bounds.Left));
      float cropV = (float)(Math.Min(frame.Height - bounds.Bottom, bounds.Top));

      //Work out actual picture aspect ratio
      float newasp = ((float)frame.Width - cropH * 2) / ((float)frame.Height - cropV * 2);      

      //Correction for anamorphic video i.e. when pixel aspect ratio != video aspect ratio.
      //After this correction, cropH value is in 'real' video pixels, not source pixels
      if (LastAnamorphFactor > 0f)
      {
        newasp *= LastAnamorphFactor;
        cropH *= LastAnamorphFactor; 
      }

      if (newasp < 1.1 || newasp > 2.7) // faulty crop
      {
        frame.Dispose();
        frame = null;
        return;
      }
      else // (newasp >= 1)
      {
        if (currentSettings.verboseLog)
        {
          Log.Debug("ViewModeSwitcher: SingleCrop(), Video AR: {0}, Cropped AR: {1}", LastSwitchedAspectRatio, newasp);      
        }
        
        //Check for 'Pillar Boxed' 4:3 inside 16:9 video
        if (LastSwitchedAspectRatio > 1.60 && LastSwitchedAspectRatio < 1.95
             && (CheckRulesPB(-newasp, frame.Width, frame.Height)))
        {
          if (LastSwitchedGeometry != Geometry.Type.NonLinearStretch)
          {
            //NonLinearStretch needs full side bar cropping, other modes don't
            cropH = overScan;
            cropV = overScan / LastSwitchedAspectRatio;
          }
          else
          {
            //Add overscan to NonLinearStretch
            cropH += overScan;
            cropV += overScan / LastSwitchedAspectRatio;
          }
          
          if (!isPillarBox) //Only update on first detection
          {
            updateCrop = true;
          }
          isPillarBox = true;
        }
        else
        {
          //Normal video 
          //Use overscan cropping if larger than detected black bars
          cropH = Math.Max(cropH, overScan);
          cropV = Math.Max(cropV, overScan / LastSwitchedAspectRatio);
          
          if (isPillarBox)
          {
            if (currentSettings.verboseLog)
            {
              Log.Debug("ViewModeSwitcher: SingleCrop(), PillarBox -> Normal");      
            }
            //Force CheckAspectRatios() update 
            updatePending = false;   
            isPillarBox = false;
            LastSwitchedAspectRatio = 0f;
            frame.Dispose();
            frame = null;
            return;
          }
          isPillarBox = false;          
        }
      } 

      if (currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: SingleCrop(), Real cropH: {0}, cropV: {1}", cropH, cropV);      
      }

      if ((Math.Abs(cropH - fCropH) > 5) || (Math.Abs(cropV - fCropV) > 3))
      {
        fCropH = cropH;
        fCropV = cropV;
        updateCrop = true;
      }      
          
      if (updateCrop)
      {
        updatePending = true;   
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
        // calculate the current aspect ratio
        float aspectRatio = (float)VMR9Util.g_vmr9.VideoAspectRatioX / (float)VMR9Util.g_vmr9.VideoAspectRatioY;
        float anamorphFactor = 0f;
        if (VMR9Util.g_vmr9.VideoWidth != 0 && VMR9Util.g_vmr9.VideoHeight != 0 )
        {
          fWidthH  = (float)VMR9Util.g_vmr9.VideoWidth;
          fHeightV = (float)VMR9Util.g_vmr9.VideoHeight;
          float pixelAR = fWidthH / fHeightV;
          anamorphFactor = (aspectRatio/pixelAR);   
        }
        if (aspectRatio != LastSwitchedAspectRatio || VMR9Util.g_vmr9.VideoHeight != LastHeight || VMR9Util.g_vmr9.VideoWidth != LastWidth)
        {
          Log.Debug("ViewModeSwitcher: CheckAspectRatios() Video AR: {0}, Last Video AR: {1}, Anamorph: {2}, Width: {3}, Height: {4}", aspectRatio, LastSwitchedAspectRatio, anamorphFactor, VMR9Util.g_vmr9.VideoWidth, VMR9Util.g_vmr9.VideoHeight);
          LastWidth  = VMR9Util.g_vmr9.VideoWidth; 
          LastHeight = VMR9Util.g_vmr9.VideoHeight;
          LastSwitchedAspectRatio = aspectRatio;
          LastAnamorphFactor = anamorphFactor;
          ProcessRules();
          LastDetectionResult = false;
        }
      }
    }

  }
}