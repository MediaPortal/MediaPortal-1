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
    public class GuiRecordings : GUIWindow
    {
        public const int WindowID = 6661;

        #region Constructor
        public GuiRecordings()
        {
            GetID = (int)GuiRecordings.WindowID;
        }
        #endregion


        #region Private Variables
        private System.Threading.Thread WaitCursorThread = null;
        private bool WaitCursorActive = false;
        private WaitCursor _WaitCursor = null;

        private DreamBox.Core _Dreambox = null;
        PlayListPlayer playlistPlayer;
        private System.ComponentModel.BackgroundWorker _backgroundWorker = new System.ComponentModel.BackgroundWorker();
        #endregion

        #region Private Enumerations
        enum Controls
        {
            List = 50
        }
        #endregion

        #region Overrides
        public override bool Init()
        {
            playlistPlayer = PlayListPlayer.SingletonPlayer;
            LoadSettings();

            return Load(GUIGraphicsContext.Skin + @"\mydreamboxrecordings.xml");
        }
        public override void OnAction(Action action)
        {
            switch (action.wID)
            {
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    {
                        GUIWindowManager.ShowPreviousWindow();
                        return;
                    }
            }

            base.OnAction(action);
        }
        protected override void OnPageLoad()
        {
            SetLabels();
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
                        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(604));
                        //FillRecordings();
                        _backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(_backgroundWorker_DoWork);
                        _backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
                        _backgroundWorker.RunWorkerAsync();
                        
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
                                PlayRecording(GUIControl.GetListItem(GetID, (int)Controls.List, iItem));
                            }
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
        void FillRecordings()
        {
            DataTable dt = _Dreambox.Data.Recordings.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                GUIListItem li = new GUIListItem();
                li.Label = row["Name"].ToString();
                if (li.Label == _Dreambox.CurrentChannel.Name)
                {
                    li.Selected = true;
                    try
                    {
                        GUIPropertyManager.SetProperty("#Play.Current.File", li.Label);
                        GUIPropertyManager.SetProperty("#Play.Current.Title", li.Label);
                    }
                    catch (Exception) { }
                }
                GUIControl.AddListItemControl(GetID, (int)Controls.List, li);
                string strObjects = String.Format("{0} {1}", GUIControl.GetItemCount(GetID, (int)Controls.List).ToString(), GUILocalizeStrings.Get(632));
                GUIPropertyManager.SetProperty("#itemcount", strObjects);
            }
        }
        void PlayRecording(GUIListItem listItem)
        {
            string uri = GetChannelRef(listItem.Label);
            DreamBox.ChannelInfo channelInfo = _Dreambox.CurrentChannel;
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
            url = url + "$" + listItem.Label + "$" + ".gary";

            string currentChannelName = _Dreambox.CurrentChannel.Name;
           

            if (uri != _Dreambox.XML.CurrentService.ServiceReference)
            {
                //_Dreambox.RemoteControl.Zap(uri);
            }
            string recordingUrl = serverUrl + ":31342/" + uri.Substring(uri.IndexOf("hdd"));
            string channelName = uri.Substring(uri.IndexOf("hdd")).Replace("hdd%2fmovie%2f", "").Replace("%20", " ").Replace("%2f", "/").Replace("%2e", ".").Replace("%2d", "-").Replace("%5f", "-").Replace(".ts", "");

            recordingUrl = recordingUrl.Replace("%2f", "/").Replace("%2e", ".").Replace("%2d", "-").Replace("%5f", "_");
            Play(recordingUrl);
            if (channelName != "")
            {
                GUIPropertyManager.SetProperty("#Play.Current.File", channelName);
                GUIPropertyManager.SetProperty("#Play.Current.Title", channelName);
                GUIPropertyManager.SetProperty("#Play.Current.Channel", channelName);
                GUIPropertyManager.SetProperty("#view", channelName);
                GUIPropertyManager.SetProperty("#currentmodule", channelName);
            }
        }
        string GetChannelRef(string channelName)
        {
            string reference = "";
            //if (BoutiqueReference.Contains("/"))
            //{
            //    int index = BoutiqueReference.IndexOf("/");
            //    BoutiqueReference = BoutiqueReference.Substring(0, index);
            //}
            DataTable dt = _Dreambox.Data.Recordings.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                if (row["Name"].ToString() == channelName)
                {
                    reference = row["Ref"].ToString();
                    break;
                }
            }
            return reference;
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
        }
        void SetLabels()
        {
            string currentChannel = GUIPropertyManager.GetProperty("#view");
            if (currentChannel == "")
                return;
            GUIPropertyManager.SetProperty("#Play.Current.File", currentChannel);
            GUIPropertyManager.SetProperty("#Play.Current.Title", currentChannel);
            GUIPropertyManager.SetProperty("#view", currentChannel);
            GUIPropertyManager.SetProperty("#currentmodule", currentChannel);
        }

        private void ShowWaitCursorAsync()
        {
            _WaitCursor = new WaitCursor();

            while (WaitCursorActive)
            {
                System.Threading.Thread.Sleep(800);
                GUIWindowManager.Process();
            }

            if (_WaitCursor != null)
            {
                _WaitCursor.Dispose();
                _WaitCursor = null;
            }
        }
        private void ShowWaitCursor()
        {
            HideWaitCursor();

            if (WaitCursorThread == null)
            {
                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(ShowWaitCursorAsync);
                WaitCursorThread = new System.Threading.Thread(ts);
            }

            else
            {
                if (WaitCursorThread.IsAlive)
                {
                    WaitCursorThread.Abort();
                }

                WaitCursorThread = null;
                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(ShowWaitCursorAsync);
                WaitCursorThread = new System.Threading.Thread(ts);
            }

            WaitCursorActive = true;
            WaitCursorThread.Start();

            GUIWindowManager.Process();
        }
        private void HideWaitCursor()
        {
            if (!WaitCursorActive && _WaitCursor == null && WaitCursorThread == null)
                return;

            WaitCursorActive = false;

            // Dispose of the WaitCursor object
            if (_WaitCursor != null)
            {
                _WaitCursor.Dispose();
                _WaitCursor = null;
            }

            // Make sure the thread is dead
            if (WaitCursorThread != null)
            {
                if (WaitCursorThread.IsAlive)
                    WaitCursorThread.Abort();

                WaitCursorThread = null;
            }
        }

        void _backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _backgroundWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(_backgroundWorker_DoWork);
            _backgroundWorker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
            HideWaitCursor();
        }
        void _backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ShowWaitCursor();
            FillRecordings();
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion




    }




}