using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AXVLC;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
//using MediaPortal.Util;

namespace MediaPortal.VideoLanPlugin
{
    public class VideoLanPlugin : IExternalPlayer 
    {
        public static string ChannelName = "";
        public static bool _playerIsPaused;
        int _PreviousWindowID = 0;
        string _currentFile = String.Empty;
        bool _started;
        bool _ended;
        double _duration;
        double _currentPosition;
        DateTime _updateTimer;
        private string[] _supportedExtension = new string[2];

        bool _isFullScreen = false;
        bool _notifyPlaying = true;
        int _positionX = 10, _positionY = 10, _videoWidth = 100, _videoHeight = 100;



        public static VideoLanControl vlcControl = null;

        public AxAXVLC.AxVLCPlugin Control
        {
            get
            {
                return vlcControl.Player;
            }
        }
       

        public VideoLanPlugin()
        {
            
        }

        public override string Description()
        {
            return "Videolan Player";
        }

        public override string PlayerName
        {
            get { return "VideoLan"; }
        }

        public override string AuthorName
        {
            get { return "ZipperZip"; }
        }

        public override string VersionNumber
        {
            get { return "0.1"; }
        }

        public override string[] GetAllSupportedExtensions()
        {
            _supportedExtension[0] = ".mp3";
            _supportedExtension[1] = ".avi";

            return _supportedExtension;
        }

        public override bool SupportsFile(string filename)
        {
            //_supportedExtension[0] = "mp3";
            //_supportedExtension[1] = ".avi";
            string ext = null;
            int dot = filename.LastIndexOf(".");    // couldn't find the dot to get the extension
            if (dot == -1) return false;

            ext = filename.Substring(dot).Trim();
            if (ext.Length == 0) return false;   // no extension so return false;

            ext = ext.ToLower();

            
            //_supportedExtension[0] = ".mp3";
            //_supportedExtension[1] = ".avi";
            //_supportedExtension[2] = ".mp4";
            //_supportedExtension[3] = ".mov";
            _supportedExtension[0] = ".ts";
            _supportedExtension[1] = ".gary";
            for (int i = 0; i < _supportedExtension.Length; i++)
            {
                if (_supportedExtension[i].Equals(ext))
                    return true;
            }

            // could not match the extension, so return false;
            return false;
        }

        public override bool Play(string strFile)
        {
            if (strFile.EndsWith(".gary"))
            {
                strFile = strFile.Replace(".gary", "");
                ChannelName = strFile.Substring(strFile.IndexOf("$")).Replace("$", "");
                strFile = strFile.Substring(0, strFile.IndexOf("$"));
                vlcControl = null;
            }
            try
            {
                if (vlcControl == null)
                {
                    vlcControl = new VideoLanControl();
                    vlcControl = VideoLanControl.Instance;

                    //vlcControl.Player.playEvent += AxAXVLC.AxVLCPluginEven(Player_playEvent);
                    ////vlcControl.Player.pauseEvent += new EventHandler(Player_pauseEvent);
                    //vlcControl.Player.stopEvent += new EventHandler(Player_stopEvent);
                    //vlcControl.Player.addTarget(strFile, null, VLCPlaylistMode.VLCPlayListAppendAndGo, 0);
                }
                GUIGraphicsContext.OnNewAction += new OnActionHandler(OnAction2);
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
                if (ChannelName.Length > 0) { msg.Label = ChannelName; }
                else
                    msg.Label = CurrentFile;
                GUIWindowManager.SendThreadMessage(msg);

                //GUIWindowManager.OnNewAction +=new OnActionHandler(OnNewAction);
                

                _started = false;
                _ended = false;

                string[] option = new string[2];
                option[0] = "--audio-visual=Goom";
                option[1] = "--no-overlay";



                vlcControl.Player.playlistClear();
                vlcControl.Player.addTarget(strFile, null, VLCPlaylistMode.VLCPlayListAppendAndGo, 0);
                //object key = vlcControl.Player.getVariable("key-jump+long");
                //vlcControl.Player.setVariable("key-pressed", key);
                //object key2 = vlcControl.Player.getVariable("key-subtitle-track");
                //vlcControl.Player.setVariable("key-pressed", key2);
        

                GUIGraphicsContext.form.Controls.Add(vlcControl);
                vlcControl.Visible = true;
                vlcControl.Enabled = false;
                GUIGraphicsContext.form.Focus();

                _started = true;
                SetWindows();
                _playerIsPaused = false;
                _currentFile = strFile;
                _duration = -1;
                _currentPosition = -1;
                //playstate??
                _updateTimer = DateTime.MinValue;
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
        public void OnAction2(Action foAction)
        {
            if (foAction.wID == Action.ActionType.ACTION_NEXT_AUDIO)
            {
                object key = vlcControl.Player.getVariable("key-audio-track");
                vlcControl.Player.setVariable("key-pressed", key);
            }
            if (foAction.wID == Action.ActionType.ACTION_FORWARD)
            {
                object key = vlcControl.Player.getVariable("key-faster");
                vlcControl.Player.setVariable("key-pressed", key);
            }
            if (foAction.wID == Action.ActionType.ACTION_REWIND)
            {
                object key = vlcControl.Player.getVariable("key-slower");
                vlcControl.Player.setVariable("key-pressed", key);
            }
            if (foAction.wID == Action.ActionType.ACTION_TAKE_SCREENSHOT)
            {
                object key = vlcControl.Player.getVariable("key-snapshot");
                vlcControl.Player.setVariable("key-pressed", key);
            }
            if (foAction.wID == Action.ActionType.ACTION_ASPECT_RATIO)
            {
                object key = vlcControl.Player.getVariable("key-aspect-ratio");
                vlcControl.Player.setVariable("key-pressed", key);
            }
            if (foAction.wID == Action.ActionType.ACTION_SHOW_GUI)
            {
                int id = GUIWindowManager.ActiveWindow;
                    

                // should size back and forth from max to normal
                //if (!GUIWindowManager.ActiveWindow.Equals(GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO) && _isFullScreen == true)
                //{
                //    _isFullScreen = false;
                //    vlcControl.Width = 100;
                //    //vlcControl.Player.Width = 100;
                //    vlcControl.Height = 100;
                //   // vlcControl.Player.Height = 100;
                //    //SetWindows();
                //}
                //else
                //{
                //    _isFullScreen = true;
                //    vlcControl.Player.fullscreen();
                //}



                //Stop();
                //Log.Write("g_Player _Player playing:{0}",g_Player.Player.Playing);
                //Log.Write("VideoLanPlayer playing:{0}", this.Playing);
                //g_Player.Stop();

                //if (g_Player.Playing) { g_Player.Stop(); }
            }
        }
        //public virtual void OnNewAction(Action action)
        //{
        //    if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
        //    {
        //        vlcControl.Player.stop();
        //        vlcControl.Visible = false;
        //        _playerIsPaused = false;
        //        _started = false;
        //    }
            
        //    if (action.wID == Action.ActionType.ACTION_BIG_STEP_FORWARD)
        //    {
        //        object key = vlcControl.Player.getVariable("key-jump+long");
        //        vlcControl.Player.setVariable("key-pressed", key);
        //    }
            


        //}

        void Player_playEvent()
        {
            Log.Write("VideoLanPlugin: Playback started: {0}, {1}", vlcControl.Player.MRL, vlcControl.Player.Length);
            _started = true;
        }

        void Player_stopEvent()
        {
            Log.Write("VideoLanPlugin: Playback stopped: {0}, {1}", vlcControl.Player.MRL, vlcControl.Player.Length);
            vlcControl.Player.stop();
            _ended = true;
        }

        private void UpdateStatus()
        {
            if (_started == false) return;
            TimeSpan ts = DateTime.Now - _updateTimer;
            if (ts.TotalSeconds >= 1 || _duration < 0 || _started == false)
            {
                _duration = (double)vlcControl.Player.Length;
                _currentPosition = (double)vlcControl.Player.Time;
                _updateTimer = DateTime.Now;
            }

        }

        

        public override double Duration
        {
            get
            {
                if (vlcControl == null) return 0.0d;
                UpdateStatus();
                if (_started == false) return 300;
                try
                {
                    return _duration;
                }
                catch (Exception)
                {
                    vlcControl = null;
                    return 0.0d;
                }
            }
        }

        public override double CurrentPosition
        {
            get
            {
                if (vlcControl == null) return 0.0d;
                UpdateStatus();
                if (_started == false) return 300;
                try
                {
                    return _currentPosition;
                }
                catch (Exception)
                {
                    vlcControl = null;
                    return 0.0d;
                }
            }

        }
        public override bool Ended
        {
            get
            {
                return _ended;
            }
        }
        public override bool Playing
        {
            get
            {
                try
                {
                    
                    if (vlcControl == null)
                        return false;
                    UpdateStatus();
                    
                    if (_started == false)
                        return false;
                    if (Paused) return true;
                       return true;
                     
                    
                }
                catch (Exception)
                {
                    vlcControl = null;
                    return false;
                }

            }
        }

        

        public override void Pause()
        {
            if (vlcControl == null) return;
            UpdateStatus();
            if (_started == false) return;
            try
            {
                if (Paused)
                {
                    vlcControl.Player.play();
                    _playerIsPaused = false;
                }
                else
                {
                    object key = vlcControl.Player.getVariable("key-jump+long");
                    vlcControl.Player.setVariable("key-pressed", key);
                    //object key2 = vlcControl.Player.getVariable("key-subtitle-track");
                    //vlcControl.Player.setVariable("key-pressed", key2);
                    vlcControl.Player.pause();
                    //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED, 0, 0, 0, 0, 0, null);
                    //msg.Label = _currentFile;
                    //GUIWindowManager.SendThreadMessage(msg); 
                    _playerIsPaused = true;
                }
            }
            catch (Exception)
            {
                vlcControl = null;
                return;
            }
        }

        //public override void OnAction(Action action)
        //{
        //    if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
        //    {
        //        vlcControl.Player.stop();
        //        vlcControl.Visible = false;
        //        _playerIsPaused = false;
        //        _started = false;
        //    }

        //    base.OnAction(action);
        //}

        

        

    
        

        public override bool Paused
        {
            get
            {
                try
                {
                    if (_started == false) return false;
                    return _playerIsPaused;
                }
                catch (Exception)
                {
                    vlcControl = null;
                    return false;
                }
            }
        }


        public override string CurrentFile
        {
            get
            {
                return _currentFile;
            }
        }


        public override void  Process()
        {
            
            UpdateStatus();
    
            // here the full screen pro can be tested
            SetWindows(); // make screen small or big

            if (CurrentPosition >= 10.0)
            {
                if (_notifyPlaying)
                {
                    _notifyPlaying = false;
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
                    if (ChannelName.Length > 0) { msg.Label = ChannelName; }
                    else
                        msg.Label = CurrentFile;

                    GUIWindowManager.SendThreadMessage(msg);
                    Log.Write("Message Playing 10 sec sent");
                }
            }
        }

        public override void Stop()
        {
            if (vlcControl == null) return;
            try
            {
                vlcControl.Player.stop();
                vlcControl.Visible = false;
                vlcControl.ClientSize = new Size(0, 0);
                _playerIsPaused = false;
                _started = false;
                //Playing = false;
                GUIGraphicsContext.OnNewAction -= new OnActionHandler(OnAction2);
            }
            catch (Exception)
            {
                vlcControl = null;
            }
        }

        public override bool HasVideo
        {
            get
            {
                if (_currentFile.Contains("mp4"))
                    return true;
                if (_currentFile.EndsWith(".mov"))
                    return true;
                if (_currentFile.Contains("mp3"))
                    return false;
                return true;
            }
        }

      public override bool FullScreen
      {
          /*
        get
        {
          try
          {
            if (_isFullScreen)
            {
              _isFullScreen = false;
              SetWindows();
              return false;
            }
            if (!_isFullScreen)
            {
              _isFullScreen = true;
              SetWindows();
              return true;
            }
            return true;
          }
          catch (Exception)
          {
            vlcControl = null;
            return false;
          }
        }
           */
          get
          {
              return GUIGraphicsContext.IsFullScreenVideo;
          }
          set
          {
              if (value != _isFullScreen)
              {
                  _isFullScreen = value;           
              }
          }
      }

            
        
        

        private void SetWindows()
        {
            try
            {
                if (_isFullScreen) //if (g_Player.HasVideo)
                {
                    _positionX = GUIGraphicsContext.OverScanLeft;
                    _positionY = GUIGraphicsContext.OverScanTop;
                    _videoHeight = GUIGraphicsContext.OverScanHeight;
                    _videoWidth = GUIGraphicsContext.OverScanWidth;

                    if (GUIWindowManager.ActiveWindow > 0)
                        _PreviousWindowID = GUIWindowManager.ActiveWindow;

                    // do checks
                    Point location = new Point(_positionX, _positionY);
                    Size size = new Size(_videoWidth, _videoHeight);
                    if (vlcControl.Location != location)
                    {
                        vlcControl.Location = location;
                        vlcControl.Player.Location = location;
                        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                    }
                    if (vlcControl.Size != size)
                    {
                        vlcControl.ClientSize = size;
                        vlcControl.Player.ClientSize = size;
                        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

                    }


                }
                else
                {
                    // normal view

                    _positionX = GUIGraphicsContext.VideoWindow.Left;
                    _positionY = GUIGraphicsContext.VideoWindow.Top;
                    _videoWidth = GUIGraphicsContext.VideoWindow.Width;
                    _videoHeight = GUIGraphicsContext.VideoWindow.Height;

                    // do checks
                    Point location = new Point(_positionX, _positionY);
                    Size size = new Size(_videoWidth, _videoHeight);
                    if (vlcControl.Location != location)
                    {
                        vlcControl.Location = location;
                        vlcControl.Player.Location = new Point(0, 0);
                        //GUIWindowManager.ActivateWindow((int)6801);
                    }
                    if (vlcControl.Size != size)
                    {
                        vlcControl.ClientSize = size;
                        vlcControl.Player.ClientSize = size;
                        //GUIWindowManager.ActivateWindow((int)6801);

                    }

                }
            }
            catch { }
            
        }

        public override void SeekRelative(double dTime)
        {
            double dCurTime = CurrentPosition;
            dTime = dCurTime + dTime;
            if (dTime < 0.0d) dTime = 0.0d;
            if (dTime < Duration)
            {
                SeekAbsolute(dTime);
            }
        }

        public override void SeekAbsolute(double dTime)
        {
            if (dTime < 0.0d) dTime = 0.0d;
            if (dTime < Duration)
            {
                if (vlcControl == null) return;
                try
                {
                    vlcControl.Player.Time = (int)dTime;
                }
                catch (Exception) { }
            }
        }

        public override void SeekRelativePercentage(int iPercentage)
        {
            double dCurrentPos = CurrentPosition;
            double dDuration = Duration;

            double fCurPercent = (dCurrentPos / Duration) * 100.0d;
            double fOnePercent = Duration / 100.0d;
            fCurPercent = fCurPercent + (double)iPercentage;
            fCurPercent *= fOnePercent;
            if (fCurPercent < 0.0d) fCurPercent = 0.0d;
            if (fCurPercent < Duration)
            {
                SeekAbsolute(fCurPercent);
            }
        }


        public override void SeekAsolutePercentage(int iPercentage)
        {
            if (iPercentage < 0) iPercentage = 0;
            if (iPercentage >= 100) iPercentage = 100;
            double fPercent = Duration / 100.0f;
            fPercent *= (double)iPercentage;
            SeekAbsolute(fPercent);
        }
        
 //       private void OnAction(Action action)
 //       {
 //           if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
 //           {
 //               vlcControl.Player.stop();
 ////               return true;
 //           }
 //           if (action.wID == Action.ActionType.ACTION_PAUSE)
 //           {
 //               vlcControl.Player.pause();
 ////               return true;
 //           }
 ////           return false;
 //       }


    }
    }

