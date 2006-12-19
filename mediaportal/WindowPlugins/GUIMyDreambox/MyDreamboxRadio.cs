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
    public class MyDreamboxRadio : GUIWindow
    {
        public const int WindowID = 6662;


        #region Constructor
        public MyDreamboxRadio()
        {
            GetID = (int)MyDreamboxRadio.WindowID;
        }
        #endregion

        #region SkinControlAttributes
        //[SkinControlAttribute(6)]
        //protected GUIButtonControl btnBouquet = null;
        //[SkinControlAttribute(7)]
        //protected GUIButtonControl btnChannel = null;
        //[SkinControlAttribute(8)]
        //protected GUIToggleButtonControl btnTVOnOff = null;
        [SkinControlAttribute(50)]
        protected GUIFacadeControl facadeView = null;
        //[SkinControlAttribute(20)]
        //protected GUIProgressControl progressBar = null;
        #endregion

        #region Private Variables
        private System.ComponentModel.BackgroundWorker _backgroundWorker = new System.ComponentModel.BackgroundWorker();
        private System.Windows.Forms.Timer _ChannelTimer = new System.Windows.Forms.Timer();

        private DreamBox.Core _Dreambox = null;
        PlayListPlayer playlistPlayer;
        private string BoutiqueReference = string.Empty;

        TimeSpan VideoStarted = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        private static DataTable _Bouquets = null;
        private static DataTable _Channels = null;
        private static int _SelectedBouquetID = -1;
        private static string _SelectedBouquetRef = "";
        #endregion

        #region Private Enumerations
        enum Controls
        {
            GroupButton = 9,
            ChannelButton = 10,
            List = 50
        }
        #endregion

        #region Overrides
        public override bool Init()
        {
            //playlistPlayer = PlayListPlayer.SingletonPlayer;
            LoadSettings();

            return Load(GUIGraphicsContext.Skin + @"\mydreamboxradio.xml");
        }
        protected override void OnPageLoad()
        {
            //SetLabels();
            //_ChannelTimer.Interval = 2000;
            //_ChannelTimer.Tick += new EventHandler(_ChannelTimer_Tick);
            //_ChannelTimer.Start();
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
                        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(665));

                        _Bouquets = _Dreambox.Data.UserRadioBouquets.Tables[0];
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
                                // play selected Radio Station
                                string channelRef = "";
                                string channelName = facadeView.SelectedListItem.Label;
                                if (channelName != "")
                                {
                                    for (int i = 0; i < _Channels.Rows.Count; i++)
                                    {
                                        if (_Channels.Rows[i]["Name"].ToString() == channelName)
                                        {
                                            channelRef = _Channels.Rows[i]["Ref"].ToString();
                                            if (channelRef.Length > 0)
                                            {
                                                // zap to that channel
                                                StopPlaying();
                                                _Dreambox.RemoteControl.Zap(channelRef);
                                                PlayCurrentChannel();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (iControl == (int)Controls.GroupButton)
                        {
                            GUIDialogMenu menu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
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
                                facadeView.Clear();
                                if (bouquetRef.Length > 0)
                                {
                                    // set new _SelectedBouquetRef
                                    _SelectedBouquetRef = bouquetRef;
                                    // get new list of channels
                                    _Channels = _Dreambox.Data.Channels(bouquetRef).Tables[0];

                                    // show channels
                                    for (int i = 0; i < _Channels.Rows.Count; i++)
                                    {
                                        facadeView.Add(new GUIListItem(_Channels.Rows[i]["Name"].ToString()));
                                    }
                                }
                            }
                        }
                        if (iControl == (int)Controls.ChannelButton)
                        {

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
                        //if (btnTVOnOff.Selected)
                        //{
                        //    StopPlaying();
                        //    _Dreambox.RemoteControl.Left();
                        //}
                        return;
                    }
                case Action.ActionType.ACTION_NEXT_ITEM:
                    {
                        //if (btnTVOnOff.Selected)
                        //{
                        //    StopPlaying();
                        //    _Dreambox.RemoteControl.Right();

                        //}
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
            //if (!btnTVOnOff.Selected)
            //    return;

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
            channelInfo = _Dreambox.CurrentChannel;
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
            //_ChannelTimer.Stop();
            //_ChannelTimer.Tick -= new EventHandler(_ChannelTimer_Tick);
        }


        void _ChannelTimer_Tick(object sender, EventArgs e)
        {
            

          

        }



        #endregion


    }




}