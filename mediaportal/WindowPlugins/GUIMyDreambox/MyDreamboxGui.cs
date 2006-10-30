using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Playlists;
using System.Xml;
using System.IO;
using DreamBox;
using System.Data;

namespace MediaPortal.GUI.Dreambox
{
    public class MyDreamboxGui : GUIWindow
    {
        public const int WindowID = 6660;


        #region Constructor
        public MyDreamboxGui()
			{
                GetID = (int)MyDreamboxGui.WindowID;
			}
		#endregion

        #region SkinControlAttributes
        [SkinControlAttribute(6)]
        protected GUIButtonControl btnBouquet = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnChannel = null;
        [SkinControlAttribute(8)]
        protected GUIToggleButtonControl btnTVOnOff = null;
        [SkinControlAttribute(50)]
        protected GUIFacadeControl facadeView = null;
        [SkinControlAttribute(20)]
        protected GUIProgressControl progressBar = null;
        #endregion

        #region Private Variables
        private System.ComponentModel.BackgroundWorker _backgroundWorker = new System.ComponentModel.BackgroundWorker();
        private System.Windows.Forms.Timer _ChannelTimer = new System.Windows.Forms.Timer();

        private DreamBox.Core _Dreambox = null;
        PlayListPlayer playlistPlayer;
        private string BoutiqueReference = string.Empty;
        private static bool Processing = false;

        TimeSpan VideoStarted = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        private double StartAmountSeconds = 0;
        TimeSpan VideoNow = new TimeSpan();

        private static DataTable _Bouquets = null;
        private static DataTable _Channels = null;
        private static int _SelectedBouquetID = -1;
        private static string _SelectedBouquetRef = "";
        #endregion

        #region Private Enumerations
        enum Controls
        {
            BouquetButton = 6,
            ChannelButton = 7,
            TVButton = 3,
            TVOnOff = 8,
            RecordingsButton = 11,
            RadioButton = 14,
            List = 50
        }
        #endregion

        #region Overrides
        public override bool Init()
        {
            //playlistPlayer = PlayListPlayer.SingletonPlayer;
            LoadSettings();

            return Load(GUIGraphicsContext.Skin + @"\mydreamboxmain.xml");
        }
        protected override void OnPageLoad()
        {
            SetLabels();
            _ChannelTimer.Interval = 2000;
            _ChannelTimer.Tick += new EventHandler(_ChannelTimer_Tick);
            _ChannelTimer.Start();
            base.OnPageLoad();
        }


        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {


                case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
                    {
                        break;
                    }
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    {
                        base.OnMessage(message);
                        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(605));
                        StartAmountSeconds = VideoStarted.Seconds;

                        _Bouquets = _Dreambox.Data.UserTVBouquets.Tables[0];
                        _SelectedBouquetID = int.Parse(_Dreambox.CurrentChannel.CurrentServiceReference.ToLower().Replace("h", "").TrimStart('0'));
                        string selectedBouquetReference = _Bouquets.Rows[_SelectedBouquetID - 1]["Ref"].ToString();
                        _Channels = _Dreambox.Data.Channels(selectedBouquetReference).Tables[0];
                        return true;
                    }
                case GUIMessage.MessageType.GUI_MSG_CLICKED:
                    {
                        //get sender control
                        base.OnMessage(message);
                        int iControl = message.SenderControlId;

                        if (iControl == (int)Controls.List)
                        {
                            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
                            OnMessage(msg);
                            int iItem = (int)msg.Param1;
                            int iAction = (int)message.Param1;
                            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
                            {
                                // play
                            }
                        }
                        if (iControl == (int)Controls.BouquetButton)
                        {
                            //show list with bouquets
                            GUIDialogMenuBottomRight menu = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
                            menu.Reset();
                            menu.SetHeading(GUILocalizeStrings.Get(971) + ": ");
                            for (int i = 0; i < _Bouquets.Rows.Count; i++)
                            {
                                menu.Add(_Bouquets.Rows[i]["Name"].ToString());
                            }
                            menu.DoModal(GetID);
                            string bouquetName = menu.SelectedLabelText;
                            string bouquetRef = "";
                            if (bouquetName != "")
                            {
                                for (int i = 0; i < _Bouquets.Rows.Count; i++)
                                {
                                    if (_Bouquets.Rows[i]["Name"].ToString() == bouquetName)
                                    {
                                        bouquetRef = _Bouquets.Rows[i]["Ref"].ToString();
                                        _SelectedBouquetID = i+1;
                                        break;
                                    }
                                }
                                if (bouquetRef.Length > 0)
                                {
                                    // set new _SelectedBouquetRef
                                    _SelectedBouquetRef = bouquetRef;
                                    // get new list of channels
                                    _Channels = _Dreambox.Data.Channels(bouquetRef).Tables[0];

                                    // show channels
                                    menu.Reset();
                                    menu.SetHeading(GUILocalizeStrings.Get(602));
                                    for (int i = 0; i < _Channels.Rows.Count; i++)
                                    {
                                        menu.Add(_Channels.Rows[i]["Name"].ToString());
                                    }

                                    menu.DoModal(GetID);
                                    string channelName = menu.SelectedLabelText;
                                    string channelRef = "";
                                    if (channelName != "")
                                    {
                                        for (int i = 0; i < _Channels.Rows.Count; i++)
                                        {
                                            if (_Channels.Rows[i]["Name"].ToString() == channelName)
                                            {
                                                channelRef = _Channels.Rows[i]["Ref"].ToString();
                                                break;
                                            }
                                        }
                                        if (channelRef.Length > 0)
                                        {
                                            // zap to that channel
                                            StopPlaying();
                                            _Dreambox.RemoteControl.Zap(channelRef);
                                        }
                                    }//end
                                }
                            }
                            return true;
                        }
                        if (iControl == (int)Controls.ChannelButton)
                        {
                            //Show list with channels
                            GUIDialogMenuBottomRight menu = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
                            menu.Reset();
                            menu.SetHeading(GUILocalizeStrings.Get(602));
                            for (int i = 0; i < _Channels.Rows.Count; i++)
                            {
                                menu.Add(_Channels.Rows[i]["Name"].ToString());
                            }
                            
                            menu.DoModal(GetID);
                            string channelName = menu.SelectedLabelText;
                            string channelRef = "";
                            if (channelName != "")
                            {
                                for (int i = 0; i < _Channels.Rows.Count; i++)
                                {
                                    if (_Channels.Rows[i]["Name"].ToString() == channelName)
                                    {
                                        channelRef = _Channels.Rows[i]["Ref"].ToString();
                                        break;
                                    }
                                }
                                if (channelRef.Length > 0)
                                {
                                    // zap to that channel
                                    StopPlaying();
                                    _Dreambox.RemoteControl.Zap(channelRef);
                                }
                            }
                            return true;
                        }
                        if (iControl == (int)Controls.RecordingsButton)
                        {
                            //activate Recordings Screen
                            GUIWindowManager.ActivateWindow(6661);
                            return true;
                        }
                        if (iControl == (int)Controls.TVOnOff)
                        {
                            //check if TV is turned on or off
                            if (!btnTVOnOff.Selected)
                            {
                                // stop tv
                                StopPlaying();
                                _ChannelTimer.Stop();
                            }
                            else
                            {
                                // start tv
                                GUIPropertyManager.SetProperty("#view", "");
                                _ChannelTimer.Start();
                                PlayCurrentChannel();
                                
                            }
                            return true;
                        }
                        if (iControl == (int)Controls.RadioButton)
                        {
                            //activate Radio Screen
                            GUIWindowManager.ActivateWindow(6662);
                        }


                        return true;
                    }
                case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
                    {

                    }
                    break;

            }
            return base.OnMessage(message);

        } 
        public override void OnAction(Action action)
        {
            switch (action.wID)
            {
                case Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE:
                    {
                        // switch new Language

                        return;
                    }
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    {
                        GUIWindowManager.ShowPreviousWindow();
                        return;
                    }
                case Action.ActionType.ACTION_PREV_ITEM:
                    {
                        if (btnTVOnOff.Selected)
                        {
                            StopPlaying();
                            _Dreambox.RemoteControl.Left();
                        }
                        return;
                    }
                case Action.ActionType.ACTION_NEXT_ITEM:
                    {
                        if (btnTVOnOff.Selected)
                        {
                            StopPlaying();
                            _Dreambox.RemoteControl.Right();
                            
                        }
                        return;
                    }
            }

            base.OnAction(action);
        }
        #endregion

        #region Private Methods
        void LoadSettings()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                string server = xmlreader.GetValue("mydreambox", "IP");
                string username = xmlreader.GetValue("mydreambox", "UserName");
                string password = xmlreader.GetValue("mydreambox", "Password");
                try
                {
                    if (server.Length > 0)
                        _Dreambox = new DreamBox.Core("http://" + server, username, password);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }
        void SetLabels()
        {
            BoxInfo boxInfo = _Dreambox.BoxInfo;
            BoutiqueReference = boxInfo.ServiceReference;
            GUIPropertyManager.SetProperty("#TV.View.channel", boxInfo.ServiceName);
            GUIPropertyManager.SetProperty("#TV.View.title", boxInfo.NowSt);
            GUIPropertyManager.SetProperty("#TV.View.start", boxInfo.NowT);
            GUIPropertyManager.SetProperty("#TV.View.stop", boxInfo.NextT);
            GUIPropertyManager.SetProperty("#TV.View.description", "Next: " + boxInfo.NextT + " - " + boxInfo.NextSt);

            string currentChannel = GUIPropertyManager.GetProperty("#view");
            if (currentChannel == "")
                return;
            
            //GUIPropertyManager.SetProperty("#TV.View.description", currentChannel);
            GUIPropertyManager.SetProperty("#TV.View.description", "Playing recording: " + currentChannel);
            

        }
        void Play(string fileName)
        {
            playlistPlayer = new PlayListPlayer();
            playlistPlayer.Reset();

            PlayListItem playlistItem = new Playlists.PlayListItem();
            playlistItem.Type = PlayListItem.PlayListItemType.Unknown;
            playlistItem.FileName = fileName;


            playlistItem.Duration = 0;

            PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP);
            //playlist.Clear();
            playlist.Add(playlistItem);
            playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_TEMP;
            playlistPlayer.PlayNext();
            //g_Player.Play(fileName);
            Processing = false;

        }

        void PlayCurrentChannel()
        {
            if (!btnTVOnOff.Selected)
                return;

            DreamBox.ChannelInfo channelInfo = _Dreambox.CurrentChannel;
            channelInfo = _Dreambox.CurrentChannel;
            string serverUrl = _Dreambox.Url;
            try
            {
                serverUrl = serverUrl.Substring(7);
                serverUrl = serverUrl.Substring(0, serverUrl.LastIndexOf(':'));
            }
            catch
            { }
            serverUrl = @"http://" + serverUrl;
            string url = serverUrl + ":31339/0," + channelInfo.Pmt.Replace("h", ",").Trim() + channelInfo.Vpid.Replace("h", ",").Trim() + channelInfo.Apid.Replace("h", ",").Trim() + channelInfo.Pcrpid.Replace("h", "").Trim();
            url = url + "$" + GUIPropertyManager.GetProperty("#TV.View.channel") + "$" + ".gary";
            // zap channels
            SetLabels();
            Play(url);
            
        }
        void StopPlaying()
        {
            try
            {
                // stop tv
                BoutiqueReference = "stopped";
                g_Player.Stop();
            }
            catch { }
            
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _ChannelTimer.Stop();
            _ChannelTimer.Tick -= new EventHandler(_ChannelTimer_Tick);
        }


        void _ChannelTimer_Tick(object sender, EventArgs e)
        {
            BoxInfo boxInfo = _Dreambox.BoxInfo;

            if (Processing) // switching channels, do not run this again
                return;
            if (_Dreambox.CurrentChannel.Name != "")
            {
                GUIPropertyManager.SetProperty("#TV.View.channel", boxInfo.ServiceName);
                GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(875) + ": " + boxInfo.NowSt);
                GUIPropertyManager.SetProperty("#TV.View.start", boxInfo.NowT);
                GUIPropertyManager.SetProperty("#TV.View.stop", boxInfo.NextT);
                GUIPropertyManager.SetProperty("#TV.View.description", "Next: " + boxInfo.NextT + " - " + boxInfo.NextSt);
                btnBouquet.Label = GUILocalizeStrings.Get(971);
                btnChannel.Label = GUILocalizeStrings.Get(602) + " " + boxInfo.ServiceName;
                
            }

            if (boxInfo.ServiceReference.EndsWith(".ts")) // Dreambox is in video playback mode
            {
                Processing = true;
                GUIPropertyManager.SetProperty("#TV.View.channel", GUILocalizeStrings.Get(157));
                GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(875) + ": " + boxInfo.NowSt);
                string[] sTime = boxInfo.VideoTime.Split(':');
                //if (sTime.GetUpperBound(0) == 3)
                //{
                //    VideoNow = new TimeSpan(int.Parse(sTime[0]), int.Parse(sTime[1]), int.Parse(sTime[2]));
                //}
                //else
                //{
                //    VideoNow = new TimeSpan(0, int.Parse(sTime[0]), int.Parse(sTime[1]));
                //}
                //TimeSpan EndTime = VideoStarted + VideoNow;
                //TimeSpan NowTime = new TimeSpan(1, DateTime.Now.Minute, DateTime.Now.Second);
                //int PercentComplete = (int)((VideoStarted.TotalSeconds - VideoNow.TotalSeconds) / (EndTime.TotalSeconds - VideoStarted.TotalSeconds));
                //progressBar.Percentage = PercentComplete;
                //GUIPropertyManager.SetProperty("#TV.View.stop", EndTime.ToString());
                Processing = false;
                return;
            }
            if (BoutiqueReference != boxInfo.ServiceReference)
            {
                BoutiqueReference = boxInfo.ServiceReference;
                //boxInfo = _Dreambox.BoxInfo;
                //System.Threading.Thread.Sleep(2000); // Wait 2 second.
                PlayCurrentChannel();
            }
                
        }



        #endregion


    }




}