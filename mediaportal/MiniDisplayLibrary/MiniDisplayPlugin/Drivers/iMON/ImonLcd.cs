using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Microsoft.Win32;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// iMON LCD Driver for iMON Manager >= 8.01.0419
    /// </summary>
    public class ImonLcd : ImonBase
    {
        Thread _monitorMediaThread;
        volatile bool _stopMonitorMediaThread = false;
        MediaTypes _currentMediaTypes;
        const int MediaTotalTime = 100;
        int _currentMediaCurrentPosition = 0;
        byte _currentSpeakers = 0;
        byte _currentSpeakerRR = 0;
        byte _currentVideoCodecs = 0;
        byte _currentAudioCodecs = 0;
        byte _currentAspectRatioIcons = 0;
        ImonMediaInfo _videoMediaInfo = new ImonMediaInfo();
        ImonMediaInfo _audioMediaInfo = new ImonMediaInfo();
        ActiveWindowInfo _activeWindowInfo = new ActiveWindowInfo();
        private AdvancedSettings AdvSettings;
        bool _preferLine1General;
        bool _preferLine1Playback;
        
        public ImonLcd()
        {
            UnsupportedDeviceErrorMessage = "Only LCDs are supported";
            Description = "iMON LCD for iMON Manager >= 8.01.0419";
            Name = "iMONLCD";
            DisplayType = DSPType.DSPN_DSP_LCD;

            LoadAdvancedSettings();
            AdvancedSettings.OnSettingsChanged += 
                AdvancedSettings_OnSettingsChanged;

            SystemEvents.PowerModeChanged += 
                SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            LogDebug("(IDisplay) ImonLcd.SystemEvents_PowerModeChanged() called, power mode is now " + e.Mode.ToString());
            SetDefaults();
            LogDebug("(IDisplay) ImonLcd.SystemEvents_PowerModeChanged() completed");
        }

        private void SetDefaults()
        {
            LogDebug("(IDisplay) ImonLcd.SetDefaults() called");
            _stopMonitorMediaThread = false;
            _currentMediaCurrentPosition = 0;
            _currentSpeakers = 0;
            _currentSpeakerRR = 0;
            _currentVideoCodecs = 0;
            _currentAudioCodecs = 0;
            _currentAspectRatioIcons = 0;
            LogDebug("(IDisplay) ImonLcd.SetDefaults() completed");
        }

        [Flags]
        enum OrangeDiskPieces : byte
        {
            Piece1 = 0x80, // top piece
            Piece2 = 0x40, // next piece counter-clockwise
            Piece3 = 0x20, // and so on...
            Piece4 = 0x10,
            Piece5 = 0x8,
            Piece6 = 0x4,
            Piece7 = 0x2,
            Piece8 = 0x1,
            None = 0x0
        }

        enum OrangeDiskIcon : byte
        {
            On = 0x80,
            Off = 0x0
        }

        [Flags]
        enum MediaTypes : byte
        {
            Music = 0x80,
            Movie = 0x40,
            Photo = 0x20,
            Cd_Dvd = 0x10,
            Tv = 0x8,
            WebCasting = 0x4,
            News_Weather = 0x2,
            None = 0x0
        }

        [Flags]
        enum Speakers : byte
        {
            L = 0x80,
            C = 0x40,
            R = 0x20,
            SL = 0x10,
            LFE = 0x8,
            SR = 0x4,
            RL = 0x2,
            SPDIF = 0x1,
            None = 0x0
        }

        enum SpeakersRR : byte
        {
            RR = 0x80,
            Off = 0x0
        }

        [Flags]
        enum VideoCodecs : byte
        {
            MPG = 0x80,
            DIVX = 0x40,
            XVID = 0x20,
            WMV = 0x10,
            MPG2 = 0x8,
            AC3 = 0x4,
            DTS = 0x2,
            WMA = 0x1,
            None = 0x0
        }

        [Flags]
        enum AudioCodecs : byte
        {
            MP3 = 0x80,
            OGG = 0x40,
            WMA = 0x20,
            WAV = 0x10,
            None = 0x0
        }

        [Flags]
        enum AspectRatios : byte
        {
            SRC = 0x80,
            FIT = 0x40,
            TV = 0x20,
            HDTV = 0x10,
            SCR1 = 0x8,
            SCR2 = 0x4,
            None = 0x0
        }

        [Flags]
        enum EtcIcons : byte
        {
            Repeat = 0x80,
            Shuffle = 0x40,
            Alarm = 0x20,
            Recording = 0x10,
            Volume = 0x8,
            Time = 0x4,
            None = 0x0
        }

        public override void Configure()
        {
            LogDebug("(IDisplay) ImonLcd.Configure() called");
            Form form = new ImonLcd_AdvancedSetupForm();
            form.ShowDialog();
            form.Dispose();
            LogDebug("(IDisplay) ImonLcd.Configure() completed");
        }

        public override void SetLine(int line, string message)
        {
            LogDebug("(IDisplay) ImonLcd.SetLine() called");
            if (message == null)
            {
                LogDebug("(IDisplay) ImonLcd.SetDefaults(): empty message, return");
                return;
            }

            int lineToCheckFirst, lineToCheckSecond;
            if (g_Player.Player != null && g_Player.Player.Playing)
            {
                LogDebug("(IDisplay) ImonLcd.SetLine(): determing line to display during playback");
                if (_preferLine1Playback)
                {
                    lineToCheckFirst = 0;
                    lineToCheckSecond = 1;
                }
                else
                {
                    lineToCheckFirst = 1;
                    lineToCheckSecond = 0;
                }
            }
            else
            {
                LogDebug("(IDisplay) ImonLcd.SetLine(): determine line to display during general use");
                if (_preferLine1General)
                {
                    lineToCheckFirst = 0;
                    lineToCheckSecond = 1;
                }
                else
                {
                    lineToCheckFirst = 1;
                    lineToCheckSecond = 0;
                }
            }
            LogDebug("(IDisplay) ImonLcd.SetLine(): preferred 0-based line: " + lineToCheckFirst);

            if (line == lineToCheckFirst)
            {
                Line1 = message;
                IDW_SetLcdText(Line1);
                LogDebug("(IDisplay) ImonLcd.SetLine(): displaying 0-based line (first choice): " + line + ", message: " + Line1);
            }
            else if (line == lineToCheckSecond)
            {
                if (Line1.Trim().Length == 0 && message.Length > 0)
                {
                    Line1 = message;
                    IDW_SetLcdText(Line1);
                    LogDebug("(IDisplay) ImonLcd.SetLine(): displaying 0-based line (second choice): " + line + ", message: " + Line1);
                }
            }
            LogDebug("(IDisplay) ImonLcd.SetDefaults() completed");
        }

        private void ThreadMain()
        {
            try
            {
                MonitorMedia();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Error("iMON LCD Display thread stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);               
            }
        }

        private void MonitorMedia()
        {
            LogDebug("(IDisplay) ImonLcd.MonitorMedia() called");
            while (true)
            {
                LogDebug("(IDisplay) ImonLcd.MonitorMedia() thread loop begin");
                if (_stopMonitorMediaThread)
                {
                    IDW_SetLcdAllIcons(false);
                    return;
                }

                SetMediaTypeIcons();
                SetCodecs();
                SetMediaProgress();
                SetSpeakerConfig();
                SetAspectRatio();

                Thread.Sleep(300);
                LogDebug("(IDisplay) ImonLcd.MonitorMedia() thread loop end");
            }
            LogDebug("(IDisplay) ImonLcd.MonitorMedia() complete");
        }

        private void SetAspectRatio()
        {
            LogDebug("(IDisplay) ImonLcd.SetAspectRatio() called");
            byte newAspectRatioIcons = 0;

            if (g_Player.Player != null && g_Player.Player.Playing &&
                g_Player.MediaInfo != null && g_Player.IsVideo)
            {
                int videoHeight = g_Player.MediaInfo.Height;

                if (videoHeight >= 720)
                {
                    newAspectRatioIcons |= (byte)AspectRatios.HDTV;
                }
                else if (videoHeight > 0)
                {
                    newAspectRatioIcons |= (byte)AspectRatios.TV;
                }
                LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): determined video height: " + videoHeight);

                if (g_Player.ARType == Geometry.Type.Original)
                {
                    newAspectRatioIcons |= (byte)AspectRatios.SRC;
                }
                else
                {
                    newAspectRatioIcons |= (byte)AspectRatios.FIT;
                }
                LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): determined ARType: " + g_Player.ARType.ToString());
            }
            else
            {
                newAspectRatioIcons = 0;
            }

            if (newAspectRatioIcons != _currentAspectRatioIcons)
            {
                LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): new settings found, call API");
                IDW_SetLcdAspectRatioIcon(newAspectRatioIcons);
                _currentAspectRatioIcons = newAspectRatioIcons;
            }
            LogDebug("(IDisplay) ImonLcd.SetAspectRatio() completed");
        }

        private void SetSpeakerConfig()
        {
            LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig() called");
            byte newSpeakers = 0, newSpeakerRR = 0;

            if (g_Player.Player != null && g_Player.Player.Playing && g_Player.MediaInfo != null)
            {
                switch (g_Player.MediaInfo.AudioChannels)
                {
                    case 1: // mono
                    case 2: // stereo
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        break;
                    case 3: // 2.1
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.LFE;
                        break;
                    case 4: // quad
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.SL;
                        newSpeakers |= (byte)Speakers.SR;
                        break;
                    case 5: // surround
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.SL;
                        newSpeakers |= (byte)Speakers.SR;
                        newSpeakers |= (byte)Speakers.C;
                        break;
                    case 6: // 5.1
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.RL;
                        newSpeakers |= (byte)Speakers.C;
                        newSpeakers |= (byte)Speakers.LFE;
                        newSpeakerRR |= (byte)SpeakersRR.RR;
                        break;
                    case 7: // 6.1
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.SL;
                        newSpeakers |= (byte)Speakers.SR;
                        newSpeakers |= (byte)Speakers.C;
                        newSpeakers |= (byte)Speakers.LFE;
                        newSpeakers |= (byte)Speakers.RL;
                        break;
                    case 8: // 7.1
                        newSpeakers |= (byte)Speakers.L;
                        newSpeakers |= (byte)Speakers.R;
                        newSpeakers |= (byte)Speakers.SL;
                        newSpeakers |= (byte)Speakers.SR;
                        newSpeakers |= (byte)Speakers.C;
                        newSpeakers |= (byte)Speakers.LFE;
                        newSpeakers |= (byte)Speakers.RL;
                        newSpeakerRR |= (byte)SpeakersRR.RR;
                        break;
                    default: // no audio/unknown
                        newSpeakers = newSpeakerRR = 0;
                        break;
                }
                LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig(): determined speaker config, speakers: " + newSpeakers + ", speakerRR: " + newSpeakerRR);
            }
            else
            {
                newSpeakers = newSpeakerRR = 0;
            }
            if (newSpeakers != _currentSpeakers || newSpeakerRR != _currentSpeakerRR)
            {
                LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig(): new settings found, call API");
                IDW_SetLcdSpeakerIcon(newSpeakers, newSpeakerRR);
                _currentSpeakers = newSpeakers;
                _currentSpeakerRR = newSpeakerRR;
            }
            LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig() completed");
        }

        private void SetCodecs()
        {
            LogDebug("(IDisplay) ImonLcd.SetCodecs() called");
            byte newVideoCodecs = 0;
            byte newAudioCodecs = 0;
            if (g_Player.Player != null && g_Player.Player.Playing)
            {
                if (g_Player.MediaInfo != null && g_Player.IsVideo)
                {
                    // video playback
                    _videoMediaInfo.Format = g_Player.MediaInfo.VideoCodec;
                    _audioMediaInfo.Format = g_Player.MediaInfo.AudioCodec;

                    // video stream
                    if (_videoMediaInfo.IsMpg)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.MPG;
                    }
                    else if (_videoMediaInfo.IsMpg2)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.MPG2;
                    }
                    else if (_videoMediaInfo.IsDivx)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.DIVX;
                    }
                    else if (_videoMediaInfo.IsXvid)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.XVID;
                    }
                    else if (_videoMediaInfo.IsWmv)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.WMV;
                    }

                    // audio stream
                    if (_audioMediaInfo.IsMp3)
                    {
                        newAudioCodecs |= (byte)AudioCodecs.MP3;
                    }
                    else if (_audioMediaInfo.IsWma)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.WMA;
                    }
                    else if (_audioMediaInfo.IsDts)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.DTS;
                    }
                    else if (_audioMediaInfo.IsAc3)
                    {
                        newVideoCodecs |= (byte)VideoCodecs.AC3;
                    }
                    else if (_audioMediaInfo.IsOgg)
                    {
                        newAudioCodecs |= (byte)AudioCodecs.OGG;
                    }
                    LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined video codec: " + newVideoCodecs);
                    LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined audio codec: " + newAudioCodecs);
                }
                else if (g_Player.IsMusic)
                {
                    // music playback
                    string currentFile = GUIPropertyManager.GetProperty("#Play.Current.File");
                    LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined current music file: " + currentFile);
                    if (currentFile != null && currentFile.Length > 0)
                    {
                        string extension = System.IO.Path.GetExtension(currentFile).ToUpper();

                        switch (extension)
                        {
                            case ".MP3":
                                newAudioCodecs |= (byte)AudioCodecs.MP3;
                                break;
                            case ".OGG":
                                newAudioCodecs |= (byte)AudioCodecs.OGG;
                                break;
                            case ".WMA":
                                newAudioCodecs |= (byte)AudioCodecs.WMA;
                                break;
                            case ".WAV":
                                newAudioCodecs |= (byte)AudioCodecs.WAV;
                                break;
                            default:
                                break;
                        }
                        LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined music codec by extension: " + newAudioCodecs);
                    }
                }
            }
            else
            {
                newVideoCodecs = 0;
            }

            if (newVideoCodecs != _currentVideoCodecs)
            {
                LogDebug("(IDisplay) ImonLcd.SetCodecs(): new video settings found, call API");
                IDW_SetLcdVideoCodecIcon(newVideoCodecs);
                _currentVideoCodecs = newVideoCodecs;
            }

            if (newAudioCodecs != _currentAudioCodecs)
            {
                LogDebug("(IDisplay) ImonLcd.SetCodecs(): new audio settings found, call API");
                IDW_SetLcdAudioCodecIcon(newAudioCodecs);
                _currentAudioCodecs = newAudioCodecs;
            }
            LogDebug("(IDisplay) ImonLcd.SetCodecs() completed");
        }

        private void SetMediaProgress()
        {
            LogDebug("(IDisplay) ImonLcd.SetMediaProgress() called");
            int newMediaCurrentPosition = 0;

            if (g_Player.Player != null && g_Player.Player.Playing)
            {
                double duration = g_Player.Duration;
                double currentPosition = g_Player.CurrentPosition;
                if (duration == 0)
                {
                    newMediaCurrentPosition = 0;
                }
                else
                {
                    newMediaCurrentPosition =
                        (int)((g_Player.CurrentPosition / g_Player.Duration) * MediaTotalTime);
                }
                LogDebug("(IDisplay) ImonLcd.SetMediaProgress(): determined media position: " + newMediaCurrentPosition);
            }
            else
            {
                newMediaCurrentPosition = 0;
            }

            if (newMediaCurrentPosition != _currentMediaCurrentPosition)
            {
                LogDebug("(IDisplay) ImonLcd.SetMediaProgress(): new settings found, call API");
                IDW_SetLcdProgress(newMediaCurrentPosition, MediaTotalTime);
                _currentMediaCurrentPosition = newMediaCurrentPosition;
            }
            LogDebug("(IDisplay) ImonLcd.SetMediaProgress() completed");
        }

        private void SetMediaTypeIcons()
        {
            LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons() called");
            MediaTypes newMediaTypes = MediaTypes.None;

            _activeWindowInfo.ActiveWindow = GUIWindowManager.ActiveWindow;
            if (_activeWindowInfo.IsWeather)
            {
                newMediaTypes |= MediaTypes.News_Weather;
                LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): in weather plugin");
            }

            if (g_Player.Player != null && g_Player.Player.Playing)
            {
                if (g_Player.IsCDA)
                {
                    newMediaTypes |= MediaTypes.Cd_Dvd;
                    newMediaTypes |= MediaTypes.Music;
                }
                if (g_Player.IsDVD || g_Player.IsDVDMenu)
                {
                    newMediaTypes |= MediaTypes.Cd_Dvd;
                    newMediaTypes |= MediaTypes.Movie;
                }
                if (g_Player.IsMusic)
                {
                    newMediaTypes |= MediaTypes.Music;
                }
                if (g_Player.IsTV || g_Player.IsTVRecording)
                {
                    newMediaTypes |= MediaTypes.Tv;
                }
                if (g_Player.IsVideo)
                {
                    newMediaTypes |= MediaTypes.Movie;
                }
                if (_activeWindowInfo.IsWebCasting)
                {
                    newMediaTypes |= MediaTypes.WebCasting;
                }
                LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): determined media type: " + newMediaTypes);
            }

            if (_currentMediaTypes != newMediaTypes)
            {
                LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): new settings found, call API");
                IDW_SetLcdMediaTypeIcon((byte)newMediaTypes);
                _currentMediaTypes = newMediaTypes;
            }
            LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons() completed");
        }

        public override void Initialize()
        {
            LogDebug("(IDisplay) ImonLcd.Initialize() called");
            base.Initialize();
            _monitorMediaThread = new Thread(new ThreadStart(ThreadMain));
            _monitorMediaThread.Start();
            LogDebug("(IDisplay) ImonLcd.Initialize() completed");
        }

        public override void CleanUp()
        {
            LogDebug("(IDisplay) ImonLcd.CleanUp(): called");
            if (!Initialized)
            {
                return;
            }

            _stopMonitorMediaThread = true;
            int maxWaitTime = 10000; // 10 seconds
            int waitTime = 0;
            while (_monitorMediaThread.IsAlive)
            {
                waitTime += 200;
                Thread.Sleep(200);
                if (waitTime >= maxWaitTime)
                {
                    try
                    {
                        _monitorMediaThread.Abort();
                    }
                    catch (ThreadAbortException ex)
                    {
                        Log.Error(
                            "(IDisplay) ImonLcd.CleanUp(): unable to exit mediaMonitorThread cleanly: "
                            + ex.Message + "\n" + ex.StackTrace);
                    }
                    break;
                }
            }

            IDW_Uninit();
            Initialized = false;
            LogDebug("(IDisplay) ImonLcd.CleanUp(): completed");
        }

        public override void Dispose()
        {
            base.Dispose();

            AdvancedSettings.OnSettingsChanged -=
                AdvancedSettings_OnSettingsChanged;
            SystemEvents.PowerModeChanged -=
                SystemEvents_PowerModeChanged;
        }

        private void LoadAdvancedSettings()
        {
            AdvSettings = AdvancedSettings.Load();
            _preferLine1General = AdvSettings.PreferFirstLineGeneral;
            _preferLine1Playback = AdvSettings.PreferFirstLinePlayback;
        }

        private void AdvancedSettings_OnSettingsChanged()
        {
            LogDebug("ImonLcd.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS");

            CleanUp();
            Thread.Sleep(100);
            LoadAdvancedSettings();

            Initialize();
        }

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdText(
            [MarshalAs(UnmanagedType.LPWStr)] string line);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdAllIcons(
            [MarshalAs(UnmanagedType.Bool)] bool on);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdOrangeIcon(
            byte iconData1, byte iconData2);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdMediaTypeIcon(byte iconData);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdSpeakerIcon(
            byte iconData1, byte iconData2);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdVideoCodecIcon(byte iconData);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdAudioCodecIcon(byte iconData);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdAspectRatioIcon(byte iconData);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdEtcIcon(byte iconData);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern DSPResult IDW_SetLcdProgress(
            int currentPosition, int total);

        private class ImonMediaInfo
        {
            const string _mpgCodec = "MPEG VIDEO";
            readonly string[] _divxCodecs = { "DX50", "DIVX", "DIV3", "3IV" };
            const string _xvidCodec = "XVID";
            const string _wmvCodec = "VC-1";
            const string _mpg2Codec = "AVC";
            const string _dtsCodec = "DTS";
            const string _wmaCodec = "WMA";
            const string _ac3Codec = "AC-3";
            const string _mp3Codec = "LAYER 3";
            const string _oggCodec = "VORBIS";
            private string _format;

            public string Format
            {
                get { return _format; }
                set
                {
                    _format = value;
                    if (_format != null && _format.Length > 0)
                    {
                        _format = _format.ToUpper();
                    }
                }
            }

            public bool IsMpg
            {
                get { return IsCodec(_mpgCodec); }
            }

            public bool IsDivx
            {
                get
                {
                    if (Format != null && Format.Length > 0)
                    {
                        foreach (string codec in _divxCodecs)
                        {
                            if (Format.IndexOf(codec) >= 0)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    return false;
                }
            }

            public bool IsXvid
            {
                get { return IsCodec(_xvidCodec); }
            }

            public bool IsWmv
            {
                get { return IsCodec(_wmvCodec); }
            }

            public bool IsMpg2
            {
                get { return IsCodec(_mpg2Codec); }
            }

            public bool IsWma
            {
                get { return IsCodec(_wmaCodec); }
            }

            public bool IsDts
            {
                get { return IsCodec(_dtsCodec); }
            }

            public bool IsAc3
            {
                get { return IsCodec(_ac3Codec); }
            }

            public bool IsMp3
            {
                get { return IsCodec(_mp3Codec); }
            }

            public bool IsOgg
            {
                get { return IsCodec(_oggCodec); }
            }

            private bool IsCodec(string codec)
            {
                if (Format != null && Format.Length > 0)
                {
                    if (Format.IndexOf(codec) >= 0)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        private class ActiveWindowInfo
        {
            //SL: That constant was removed find a way to get this at some point 
            //const int Weather = (int)GUIWindow.Window.WINDOW_WEATHER;
            readonly int[] MyNetflix = { 10099 };
            readonly int[] MyOnlineVideos = { 4755, 4757, 4758, 4759 };
            readonly int[] MyTrailers = { 5900 };

            readonly List<int> OnlineStreamingPluginWindows;

            public int ActiveWindow { get; set; }

            public bool IsWeather
            {
                //SL: just use dummy for now
                //get { return ActiveWindow == Weather; }
                get { return false; }
            }

            public bool IsWebCasting
            {
                get { return OnlineStreamingPluginWindows.Contains(ActiveWindow); }
            }

            public ActiveWindowInfo()
            {
                OnlineStreamingPluginWindows = new List<int>();
                OnlineStreamingPluginWindows.AddRange(MyNetflix);
                OnlineStreamingPluginWindows.AddRange(MyOnlineVideos);
                OnlineStreamingPluginWindows.AddRange(MyTrailers);
            }
        }

        [Serializable]
        public class AdvancedSettings
        {
            #region Delegates

            public delegate void OnSettingsChangedHandler();

            #endregion

            private static AdvancedSettings m_Instance;
            public const string m_Filename = "MiniDisplay_ImonLcd.xml";

            public static AdvancedSettings Instance
            {
                get
                {
                    if (m_Instance == null)
                    {
                        m_Instance = Load();
                    }
                    return m_Instance;
                }
                set { m_Instance = value; }
            }

            [XmlAttribute]
            public bool PreferFirstLineGeneral { get; set; }

            [XmlAttribute]
            public bool PreferFirstLinePlayback { get; set; }

            public static event OnSettingsChangedHandler OnSettingsChanged;

            private static void Default(AdvancedSettings _settings)
            {
                _settings.PreferFirstLineGeneral = true;
                _settings.PreferFirstLinePlayback = true;
            }

            public static AdvancedSettings Load()
            {
                AdvancedSettings settings;
                LogDebug("ImonLcd.AdvancedSettings.Load(): started");
                if (File.Exists(Config.GetFile(Config.Dir.Config, m_Filename)))
                {
                    LogDebug("ImonLcd.AdvancedSettings.Load(): Loading settings from XML file");
                    var serializer = new XmlSerializer(typeof(AdvancedSettings));
                    var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, m_Filename));
                    settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                else
                {
                    LogDebug("ImonLcd.AdvancedSettings.Load(): Loading settings from defaults");
                    settings = new AdvancedSettings();
                    Default(settings);
                    LogDebug("ImonLcd.AdvancedSettings.Load(): Loaded settings from defaults");
                }
                LogDebug("ImonLcd.AdvancedSettings.Load(): completed");
                return settings;
            }

            public static void NotifyDriver()
            {
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }

            public static void Save()
            {
                Save(Instance);
            }

            public static void Save(AdvancedSettings ToSave)
            {
                var serializer = new XmlSerializer(typeof(AdvancedSettings));
                var writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, m_Filename),
                                               Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 2 };
                serializer.Serialize(writer, ToSave);
                writer.Close();
            }

            public static void SetDefaults()
            {
                Default(Instance);
            }
        }
    }
}
