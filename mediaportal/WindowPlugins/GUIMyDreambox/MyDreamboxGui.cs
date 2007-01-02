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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Configuration;
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


        private static bool ChangeChannel = false;
        private static string _CurrentChannelName = "";
        private static CurrentServiceData _OldChannel = null;

        private System.ComponentModel.BackgroundWorker _backgroundWorker = new System.ComponentModel.BackgroundWorker();
        private System.ComponentModel.BackgroundWorker _bouquetWorker = new System.ComponentModel.BackgroundWorker();
        private System.ComponentModel.BackgroundWorker _channelWorker = new System.ComponentModel.BackgroundWorker();
        private System.ComponentModel.BackgroundWorker _TVScreenWorker = new System.ComponentModel.BackgroundWorker();

        private delegate void ServiceDataHandler(CurrentServiceData currentServiceData);
        private delegate void BouquetDataHandler(DataSet bouquets);
        private delegate void ChannelDataHandler(DataSet channels);
        private delegate void TVScreenHandler();


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
        private System.Windows.Forms.Timer _ChannelTimer = new System.Windows.Forms.Timer();

        private static DreamBox.Core _Dreambox = null;
        PlayListPlayer playlistPlayer;
        private string BoutiqueReference = string.Empty;

        TimeSpan VideoStarted = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        private double StartAmountSeconds = 0;

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
            RadioButton = 14,
            RecordingsButton = 11,
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
            if (_Dreambox == null)
                LoadSettings();

            _backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
            _TVScreenWorker.DoWork += new DoWorkEventHandler(_TVScreenWorker_DoWork);
            _TVScreenWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_TVScreenWorker_RunWorkerCompleted);

            //_bouquetWorker.DoWork += new DoWorkEventHandler(_bouquetWorker_DoWork);
            //_channelWorker.DoWork += new DoWorkEventHandler(_channelWorker_DoWork);
            
            

            //SetLabels();
            _ChannelTimer.Interval = 2000;
            _ChannelTimer.Tick += new EventHandler(_ChannelTimer_Tick);
            _ChannelTimer.Start();
            base.OnPageLoad();
        }


        public override bool OnMessage(GUIMessage message)
        {
            if (message.Message != GUIMessage.MessageType.GUI_MSG_SETFOCUS)
            {
                switch (message.Message)
                {
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
                                            _SelectedBouquetID = i + 1;
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
                            if (iControl == (int)Controls.RadioButton)
                            {
                                playlistPlayer.g_Player.Play(@"http://dreambox:31339/0,013d,0bff,0c00.ts");
                                return true;
                            }
                            if (iControl == (int)Controls.ChannelButton)
                            {
                                //Show list with channels
                                GUIDialogMenu menu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
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
                                        ChangeChannelButtonClicked(channelRef);
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
                                //GUIWindowManager.ActivateWindow(GuiMain.WindowID);
                                return true;
                            }



                            return true;
                        }
                    case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
                        {

                        }
                        break;

                }
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
                case Action.ActionType.ACTION_PAGE_DOWN:
                    {
                        string channelRef = PreviousChannelRef();
                        _Dreambox.RemoteControl.Zap(channelRef);
                        ChangeChannelButtonClicked(channelRef);
                        return;
                    }
                case Action.ActionType.ACTION_PAGE_UP:
                    {
                        string channelRef = NextChannelRef();
                        _Dreambox.RemoteControl.Zap(channelRef);
                        ChangeChannelButtonClicked(channelRef);
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
            //string path = Path.Combine(Config.Get(Config.Dir.Config), "MediaPortal.xml");
            //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(path))
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                string server = xmlreader.GetValue("mydreambox", "IP");
                string username = xmlreader.GetValue("mydreambox", "UserName");
                string password = xmlreader.GetValue("mydreambox", "Password");
                try
                {
                    _Dreambox = new DreamBox.Core("http://" + server, username, password);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
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
            if (!_backgroundWorker.IsBusy)
                _backgroundWorker.RunWorkerAsync();
        }



        #endregion

        #region Update OSD
        void _StatusTimer_Tick(object sender, EventArgs e)
        {
            if (!_backgroundWorker.IsBusy)
                _backgroundWorker.RunWorkerAsync();

        }


        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            DreamBox.Core box = null;
            //string path = Path.Combine(Config.Get(Config.Dir.Config), "MediaPortal.xml");
            //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(path))
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                string server = xmlreader.GetValue("mydreambox", "IP");
                string username = xmlreader.GetValue("mydreambox", "UserName");
                string password = xmlreader.GetValue("mydreambox", "Password");
                try
                {
                    box = new DreamBox.Core("http://" + server, username, password);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
            _CurrentChannelName = btnChannel.Label;
            CurrentServiceData data = box.XML.CurrentService;
            HandleServiceData(data);
            //this.Invoke(new ServiceDataHandler(HandleServiceData), new object[] { data });
        }

        void HandleServiceData(CurrentServiceData data)
        {
            if (_CurrentChannelName != data.ServiceName) { ChangeChannel = true; };

            GUIPropertyManager.SetProperty("#TV.View.channel", data.ServiceName);
            GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(875) + ": " + data.CurrentEvent.Description);
            GUIPropertyManager.SetProperty("#TV.View.start", data.CurrentEvent.Time);
            GUIPropertyManager.SetProperty("#TV.View.stop", data.NextEvent.Time);
            GUIPropertyManager.SetProperty("#TV.View.description", "Next: " + data.NextEvent.Description);
            btnBouquet.Label = GUILocalizeStrings.Get(971);
            btnChannel.Label = GUILocalizeStrings.Get(602) + " " + data.ServiceName;

            if (data.ServiceReference.EndsWith(".ts")) // Dreambox is in video playback mode
            {
                GUIPropertyManager.SetProperty("#TV.View.channel", GUILocalizeStrings.Get(157));
                GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(875) + ": " + data.CurrentEvent.Description);
                return;
            }
        }
        #endregion

        void ChangeChannelButtonClicked(string uri)
        {
            //TV: url should be like http://192.168.2.128:31339/0,089c,0201,0064,ffffffff
            //RADIO: url sould be like http://192.168.2.128:31343/7e
            //ZAP: selected value are like 1:0:1:fab:451:35:c00000:0:0:0:


            _OldChannel = _Dreambox.XML.CurrentService;
            StreamInfoData data = _Dreambox.XML.StreamInfo;
            string serverUrl = _Dreambox.Url;
            try
            {
                serverUrl = serverUrl.Substring(7);
                serverUrl = serverUrl.Substring(0, serverUrl.LastIndexOf(':'));
            }
            catch
            { }
            serverUrl = @"http://" + serverUrl;
            // zap channels

            // check if current channel is not clicked
            if (uri != data.ServiceReference)
            {
                _Dreambox.RemoteControl.Zap(uri);
            }
            if (!_TVScreenWorker.IsBusy)
                _TVScreenWorker.RunWorkerAsync();
        }

        void _TVScreenWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_OldChannel != null)
            {
                CurrentServiceData newChannel = _Dreambox.XML.CurrentService;
                while (!ChangeChannel && (_OldChannel.ServiceName != newChannel.ServiceName))
                {

                    // let thread wait (not clean! I know!)
                    System.Threading.Thread.Sleep(1);
                }
                ChangeChannel = true;
            }


        }
        void _TVScreenWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // thread completed, now change channel
            if (ChangeChannel)
            {
                System.Threading.Thread.Sleep(500);
                ChangeChannel = false;
                //this.Invoke(new TVScreenHandler(PlayCurrentChannel), new object[] { });
                PlayCurrentChannel();
            }

        }


        string NextChannelRef()
        {
            int index = -1;
            for (int i = 0; i < _Channels.Rows.Count; i++)
            {
                if (_Channels.Rows[i]["Ref"].ToString() == _Dreambox.XML.CurrentService.ServiceReference)
                {
                    index = i+1;
                    break;
                }
            }
            return _Channels.Rows[index]["Ref"].ToString();
        }
        string PreviousChannelRef()
        {
            int index = -1;
            for (int i = 0; i < _Channels.Rows.Count; i++)
            {
                if (_Channels.Rows[i]["Ref"].ToString() == _Dreambox.XML.CurrentService.ServiceReference)
                {
                    index = i - 1;
                    break;
                }
            }
            return _Channels.Rows[index]["Ref"].ToString();
        }
    }




}