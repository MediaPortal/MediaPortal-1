#region Copyright (C) 2014 Team MediaPortal

// Copyright (C) 2014 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// SoundGraph iMON LCD.
    /// </summary>
    public class SoundGraphImonLcd : SoundGraphImon
    {

        SoundGraphDisplay.MediaTypes _currentMediaTypes;
        const int MediaTotalTime = 100;
        int _currentMediaCurrentPosition = 0;
        byte _currentSpeakers = 0;
        byte _currentSpeakerRR = 0;
        byte _currentVideoCodecs = 0;
        byte _currentAudioCodecs = 0;
        byte _currentAspectRatioIcons = 0;
        SoundGraphDisplay.EtcIcons _currentEtcIcons = 0;
        ImonMediaInfo _videoMediaInfo = new ImonMediaInfo();
        ImonMediaInfo _audioMediaInfo = new ImonMediaInfo();
        ActiveWindowInfo _activeWindowInfo = new ActiveWindowInfo();
        DateTime LastIconUpdateTime = DateTime.MinValue;
 
        //Constructor
        public SoundGraphImonLcd()
        {
        }

        public override bool IsLcd(){ return true; }
        public override bool IsVfd() { return false; }
        public override string Name() { return "iMON LCD"; }


        public override void Update()
        {
            //Check if we need to show EQ this is also taking into account our various settings.
            iSettings.iEq._EqDataAvailable = MiniDisplayHelper.GetEQ(ref iSettings.iEq);
            if (iSettings.iEq._EqDataAvailable)
            {
                //SetAndRollEqData();
                UpdateEq();
            }
            else if (NeedTextUpdate)
            {
                //Not showing EQ then display our lines
                //Only show the second line for now

                bool isPlaying = g_Player.Player != null && g_Player.Player.Playing;

                if (isPlaying && iSettings.PreferFirstLinePlayback)
                {
                    SoundGraphDisplay.IDW_SetLcdText(TextTopLine);
                }
                else if (!isPlaying && iSettings.PreferFirstLineGeneral)
                {
                    SoundGraphDisplay.IDW_SetLcdText(TextTopLine);
                }
                else
                {
                    SoundGraphDisplay.IDW_SetLcdText(TextBottomLine);
                }

                NeedTextUpdate = false;
            }

            //Update our icons here, only very N seconds
            if (SoundGraphDisplay.IsElapsed(LastIconUpdateTime, 2))
            {
                SetMediaTypeIcons();
                SetCodecs();
                SetMediaProgress();
                SetSpeakerConfig();
                SetAspectRatio();
                SetEtcIcons();
                LastIconUpdateTime = DateTime.Now;
            }
        }
        

        public override void Configure()
        {
            //Open our advanced setting dialog
            SoundGraphDisplay.LogDebug("SoundGraphImonLcd.Configure() called");
            Form form = new SoundGraphImonSettingsForm();
            form.ShowDialog();
            form.Dispose();
            SoundGraphDisplay.LogDebug("SoundGraphImonLcd.Configure() completed");

        }

        //Here comes icon management methods

        private void SetEtcIcons()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetEtcIcons() called");
            SoundGraphDisplay.EtcIcons newEtcIcons = SoundGraphDisplay.EtcIcons.None;

            if (MiniDisplayHelper.MPStatus.Media_IsRecording)
            {
                newEtcIcons |= SoundGraphDisplay.EtcIcons.Recording;
            }

            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetEtcIcons(): determined media type: " + newEtcIcons);

            if (_currentEtcIcons != newEtcIcons)
            {
                Log.Info("(IDisplay) ImonLcd.SetEtcIcons(): new settings found, call API");
                SoundGraphDisplay.IDW_SetLcdEtcIcon((byte)newEtcIcons);
                _currentEtcIcons = newEtcIcons;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetEtcIcons() completed");
        }

        private void SetAspectRatio()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetAspectRatio() called");
            byte newAspectRatioIcons = 0;

            if (g_Player.Player != null && g_Player.Player.Playing &&
                g_Player.MediaInfo != null && g_Player.IsVideo)
            {
                int videoHeight = g_Player.MediaInfo.Height;

                if (videoHeight >= 720)
                {
                    newAspectRatioIcons |= (byte)SoundGraphDisplay.AspectRatios.HDTV;
                }
                else if (videoHeight > 0)
                {
                    newAspectRatioIcons |= (byte)SoundGraphDisplay.AspectRatios.TV;
                }
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): determined video height: " + videoHeight);

                if (g_Player.ARType == Geometry.Type.Original)
                {
                    newAspectRatioIcons |= (byte)SoundGraphDisplay.AspectRatios.SRC;
                }
                else
                {
                    newAspectRatioIcons |= (byte)SoundGraphDisplay.AspectRatios.FIT;
                }
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): determined ARType: " + g_Player.ARType.ToString());
            }
            else
            {
                newAspectRatioIcons = 0;
            }

            if (newAspectRatioIcons != _currentAspectRatioIcons)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetAspectRatio(): new settings found, call API");
                SoundGraphDisplay.IDW_SetLcdAspectRatioIcon(newAspectRatioIcons);
                _currentAspectRatioIcons = newAspectRatioIcons;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetAspectRatio() completed");
        }

        private void SetSpeakerConfig()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig() called");
            byte newSpeakers = 0, newSpeakerRR = 0;

            if (g_Player.Player != null && g_Player.Player.Playing && g_Player.MediaInfo != null)
            {
                switch (g_Player.MediaInfo.AudioChannels)
                {
                    case 1: // mono
                    case 2: // stereo
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        break;
                    case 3: // 2.1
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.LFE;
                        break;
                    case 4: // quad
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SL;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SR;
                        break;
                    case 5: // surround
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SL;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SR;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.C;
                        break;
                    case 6: // 5.1
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.RL;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.C;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.LFE;
                        newSpeakerRR |= (byte)SoundGraphDisplay.SpeakersRR.RR;
                        break;
                    case 7: // 6.1
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SL;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SR;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.C;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.LFE;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.RL;
                        break;
                    case 8: // 7.1
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.L;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.R;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SL;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.SR;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.C;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.LFE;
                        newSpeakers |= (byte)SoundGraphDisplay.Speakers.RL;
                        newSpeakerRR |= (byte)SoundGraphDisplay.SpeakersRR.RR;
                        break;
                    default: // no audio/unknown
                        newSpeakers = newSpeakerRR = 0;
                        break;
                }
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig(): determined speaker config, speakers: " + newSpeakers + ", speakerRR: " + newSpeakerRR);
            }
            else
            {
                newSpeakers = newSpeakerRR = 0;
            }
            if (newSpeakers != _currentSpeakers || newSpeakerRR != _currentSpeakerRR)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig(): new settings found, call API");
                SoundGraphDisplay.IDW_SetLcdSpeakerIcon(newSpeakers, newSpeakerRR);
                _currentSpeakers = newSpeakers;
                _currentSpeakerRR = newSpeakerRR;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetSpeakerConfig() completed");
        }

        private void SetCodecs()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs() called");
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
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.MPG;
                    }
                    else if (_videoMediaInfo.IsMpg2)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.MPG2;
                    }
                    else if (_videoMediaInfo.IsDivx)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.DIVX;
                    }
                    else if (_videoMediaInfo.IsXvid)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.XVID;
                    }
                    else if (_videoMediaInfo.IsWmv)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.WMV;
                    }

                    // audio stream
                    if (_audioMediaInfo.IsMp3)
                    {
                        newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.MP3;
                    }
                    else if (_audioMediaInfo.IsWma)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.WMA;
                    }
                    else if (_audioMediaInfo.IsDts)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.DTS;
                    }
                    else if (_audioMediaInfo.IsAc3)
                    {
                        newVideoCodecs |= (byte)SoundGraphDisplay.VideoCodecs.AC3;
                    }
                    else if (_audioMediaInfo.IsOgg)
                    {
                        newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.OGG;
                    }
                    SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined video codec: " + newVideoCodecs);
                    SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined audio codec: " + newAudioCodecs);
                }
                else if (g_Player.IsMusic)
                {
                    // music playback                    
                    string currentFile = g_Player.currentFileName;
                    SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined current music file: " + currentFile);
                    if (currentFile != null && currentFile.Length > 0)
                    {
                        string extension = System.IO.Path.GetExtension(currentFile).ToUpper();

                        switch (extension)
                        {
                            case ".MP3":
                                newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.MP3;
                                break;
                            case ".OGG":
                                newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.OGG;
                                break;
                            case ".WMA":
                                newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.WMA;
                                break;
                            case ".WAV":
                                newAudioCodecs |= (byte)SoundGraphDisplay.AudioCodecs.WAV;
                                break;
                            default:
                                break;
                        }
                        SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): determined music codec by extension: " + newAudioCodecs);
                    }
                }
            }
            else
            {
                newVideoCodecs = 0;
            }

            if (newVideoCodecs != _currentVideoCodecs)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): new video settings found, call API");
                SoundGraphDisplay.IDW_SetLcdVideoCodecIcon(newVideoCodecs);
                _currentVideoCodecs = newVideoCodecs;
            }

            if (newAudioCodecs != _currentAudioCodecs)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs(): new audio settings found, call API");
                SoundGraphDisplay.IDW_SetLcdAudioCodecIcon(newAudioCodecs);
                _currentAudioCodecs = newAudioCodecs;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetCodecs() completed");
        }

        private void SetMediaProgress()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaProgress() called");
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
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaProgress(): determined media position: " + newMediaCurrentPosition);
            }
            else
            {
                newMediaCurrentPosition = 0;
            }

            if (newMediaCurrentPosition != _currentMediaCurrentPosition)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaProgress(): new settings found, call API");
                SoundGraphDisplay.IDW_SetLcdProgress(newMediaCurrentPosition, MediaTotalTime);
                _currentMediaCurrentPosition = newMediaCurrentPosition;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaProgress() completed");
        }

        private void SetMediaTypeIcons()
        {
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons() called");
            SoundGraphDisplay.MediaTypes newMediaTypes = SoundGraphDisplay.MediaTypes.None;

            _activeWindowInfo.ActiveWindow = GUIWindowManager.ActiveWindow;
            if (_activeWindowInfo.IsWeather)
            {
                newMediaTypes |= SoundGraphDisplay.MediaTypes.News_Weather;
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): in weather plugin");
            }

            if (g_Player.Player != null && g_Player.Player.Playing)
            {
                if (g_Player.IsCDA)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Cd_Dvd;
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Music;
                }
                if (g_Player.IsDVD || g_Player.IsDVDMenu)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Cd_Dvd;
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Movie;
                }
                if (g_Player.IsMusic)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Music;
                }
                if (g_Player.IsTV || g_Player.IsTVRecording)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Tv;
                }
                if (g_Player.IsVideo)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.Movie;
                }
                if (_activeWindowInfo.IsWebCasting)
                {
                    newMediaTypes |= SoundGraphDisplay.MediaTypes.WebCasting;
                }
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): determined media type: " + newMediaTypes);
            }

            if (_currentMediaTypes != newMediaTypes)
            {
                SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons(): new settings found, call API");
                SoundGraphDisplay.IDW_SetLcdMediaTypeIcon((byte)newMediaTypes);
                _currentMediaTypes = newMediaTypes;
            }
            SoundGraphDisplay.LogDebug("(IDisplay) ImonLcd.SetMediaTypeIcons() completed");
        }

        //Settings stuff
 
    }

    /// <summary>
    /// Media Info used to managed our icons.
    /// </summary>
    public class ImonMediaInfo
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

    /// <summary>
    /// Active Window Info used to managed our icons.
    /// </summary>
    public class ActiveWindowInfo
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
}

