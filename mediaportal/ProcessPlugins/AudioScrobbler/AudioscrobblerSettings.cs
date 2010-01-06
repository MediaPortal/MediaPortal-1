#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

namespace MediaPortal.AudioScrobbler
{
  public partial class AudioscrobblerSettings : MPConfigForm
  {
    private AudioscrobblerUtils lastFmLookup;
    private List<Song> songList = null;
    private List<Song> similarList = null;
    private string _currentUser = string.Empty;

    public AudioscrobblerSettings()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation

    protected void LoadSettings()
    {
      try
      {
        MusicDatabase mdb = MusicDatabase.Instance;
        List<string> scrobbleusers = new List<string>();
        string tmpuser = "";
        string tmppass = "";
        groupBoxProfile.Visible = false;

        using (Settings xmlreader = new MPSettings())
        {
          tmpuser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
          checkBoxEnableNowPlaying.Checked = xmlreader.GetValueAsBool("audioscrobbler", "EnableNowPlaying", true);

          scrobbleusers = mdb.GetAllScrobbleUsers();
          // no users in database
          if (scrobbleusers.Count == 0)
          {
            tabControlLiveFeeds.Enabled = false;
            tabControlSettings.TabPages.RemoveAt(1);
            tabControlSettings.TabPages.RemoveAt(1);
            tabControlSettings.TabPages.RemoveAt(1);
            labelNoUser.Visible = true;
          }
            // only load settings if a user is present
          else
          {
            int selected = 0;
            int count = 0;
            foreach (string scrobbler in scrobbleusers)
            {
              if (!comboBoxUserName.Items.Contains(scrobbler))
              {
                comboBoxUserName.Items.Add(scrobbler);
              }

              if (scrobbler == tmpuser)
              {
                selected = count;
              }
              count++;
            }
            comboBoxUserName.SelectedIndex = selected;
            buttonDelUser.Enabled = true;

            tmppass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(_currentUser)), "");

            EncryptDecrypt Crypter = new EncryptDecrypt();

            if (tmppass != string.Empty)
            {
              try
              {
                EncryptDecrypt DCrypter = new EncryptDecrypt();
                maskedTextBoxASPassword.Text = DCrypter.Decrypt(tmppass);
              }
              catch (Exception)
              {
                //Log.Info("Audioscrobbler: Password decryption failed {0}", ex.Message);
              }
            }

            int tmpNMode = 1;
            int tmpRand = 77;
            int tmpArtists = 2;
            int tmpPreferTracks = 2;
            int tmpOfflineMode = 0;
            string tmpUserID = Convert.ToString(mdb.AddScrobbleUser(_currentUser));

            checkBoxLogVerbose.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", -1) == 1) ? true : false;
            tmpRand = mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", -1);
            checkBoxEnableSubmits.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", -1) == 1)
                                              ? true
                                              : false;
            checkBoxScrobbleDefault.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", -1) == 1)
                                                ? true
                                                : false;
            tmpArtists = mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", -1);
            //numericUpDownTracksPerArtist.Value = mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", -1);
            tmpNMode = mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", -1);

            tmpOfflineMode = mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", -1);
            tmpPreferTracks = mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", -1);
            checkBoxReAddArtist.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", -1) == 1)
                                            ? true
                                            : false;

            numericUpDownSimilarArtist.Value = (tmpArtists > 0) ? tmpArtists : 2;
            trackBarRandomness.Value = (tmpRand >= 25) ? tmpRand : 25;
            trackBarConsiderCount.Value = (tmpPreferTracks >= 0) ? tmpPreferTracks : 2;
            comboBoxOfflineMode.SelectedIndex = tmpOfflineMode;

            lastFmLookup = AudioscrobblerUtils.Instance;

            switch (tmpNMode)
            {
              case 3:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
                comboBoxNeighbourMode.SelectedIndex = 0;
                comboBoxNModeSelect.SelectedIndex = 0;
                break;
              case 1:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
                comboBoxNeighbourMode.SelectedIndex = 1;
                comboBoxNModeSelect.SelectedIndex = 1;
                break;
              case 0:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
                comboBoxNeighbourMode.SelectedIndex = 2;
                comboBoxNModeSelect.SelectedIndex = 2;
                break;
              default:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
                comboBoxNeighbourMode.SelectedIndex = 1;
                comboBoxNModeSelect.SelectedIndex = 1;
                break;
            }

            LoadProfileDetails(tmpuser);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler settings could not be loaded: {0}", ex.Message);
      }
    }

    private void LoadProfileDetails(string aUser)
    {
      List<Song> myProfile = new List<Song>(1);
      myProfile = AudioscrobblerUtils.Instance.getAudioScrobblerFeed(lastFMFeed.profile, aUser);
      if (myProfile.Count == 1)
      {
        try
        {
          Bitmap preview = new Bitmap(AudioscrobblerUtils.DownloadTempFile(myProfile[0].WebImage));
          pictureBoxAvatar.Image = preview;
          groupBoxProfile.Visible = true;
          lblProfRealname.Text = myProfile[0].Title;
          lblProfPlaycount.Text = Convert.ToString(myProfile[0].TimesPlayed);
          lblProfRegistered.Text = myProfile[0].DateTimePlayed.ToShortDateString();
        }
        catch (Exception) {}
      }
    }

    protected void SaveSettings()
    {
      MusicDatabase mdb = MusicDatabase.Instance;
      int usedebuglog = 0;
      int submitsenabled = 1;
      int scrobbledefault = 1;
      int randomness = 77;
      int artisttoadd = 3;
      int trackstoadd = 1;
      int neighbourmode = 1;

      int offlinemode = 0;
      int prefercount = 2;
      int rememberstartartist = 1;

      if (comboBoxUserName.Text != string.Empty)
      {
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValue("audioscrobbler", "user", comboBoxUserName.Text);
          xmlwriter.SetValueAsBool("audioscrobbler", "EnableNowPlaying", checkBoxEnableNowPlaying.Checked);

          string tmpPass = "";
          string tmpUserID = "";
          try
          {
            EncryptDecrypt Crypter = new EncryptDecrypt();
            tmpPass = Crypter.Encrypt(maskedTextBoxASPassword.Text);
          }
          catch (Exception)
          {
            //Log.Info("Audioscrobbler: Password encryption failed {0}", ex.Message);
          }

          // checks and adds the user if necessary + updates the password;
          mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(comboBoxUserName.Text)), tmpPass);

          if (checkBoxLogVerbose != null)
          {
            usedebuglog = checkBoxLogVerbose.Checked ? 1 : 0;
          }
          if (checkBoxEnableSubmits != null)
          {
            submitsenabled = checkBoxEnableSubmits.Checked ? 1 : 0;
          }
          if (checkBoxScrobbleDefault != null)
          {
            scrobbledefault = checkBoxScrobbleDefault.Checked ? 1 : 0;
          }
          if (trackBarRandomness != null)
          {
            randomness = trackBarRandomness.Value;
          }
          if (numericUpDownSimilarArtist != null)
          {
            artisttoadd = (int)numericUpDownSimilarArtist.Value;
          }
          //if (numericUpDownTracksPerArtist != null)
          //  trackstoadd = (int)numericUpDownTracksPerArtist.Value;
          if (lastFmLookup != null)
          {
            neighbourmode = (int)lastFmLookup.CurrentNeighbourMode;
          }
          else
          {
            Log.Info("DEBUG *** lastFMLookup was null. neighbourmode: {0}", Convert.ToString(neighbourmode));
          }

          if (comboBoxOfflineMode != null)
          {
            offlinemode = comboBoxOfflineMode.SelectedIndex;
          }
          if (trackBarConsiderCount != null)
          {
            prefercount = trackBarConsiderCount.Value;
          }
          if (checkBoxReAddArtist != null)
          {
            rememberstartartist = checkBoxReAddArtist.Checked ? 1 : 0;
          }

          tmpUserID = Convert.ToString(mdb.AddScrobbleUser(comboBoxUserName.Text));
          mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", usedebuglog);
          mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", randomness);
          mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", submitsenabled);
          mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", scrobbledefault);
          mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", artisttoadd);
          mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", trackstoadd);
          mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", neighbourmode);
          mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", offlinemode);
          mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", prefercount);
          mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", rememberstartartist);
          //}
        }
      }
    }

    #endregion

    #region Control events

    // Implements the manual sorting of items by columns.
    private class ListViewItemComparer : IComparer
    {
      private int col;

      public ListViewItemComparer()
      {
        col = 0;
      }

      public ListViewItemComparer(int column)
      {
        col = column;
      }

      public int Compare(object x, object y)
      {
        try
        {
          double xval = Convert.ToDouble(((ListViewItem)x).SubItems[col].Text, NumberFormatInfo.InvariantInfo);
          double yval = Convert.ToDouble(((ListViewItem)y).SubItems[col].Text, NumberFormatInfo.InvariantInfo);

          return -(xval.CompareTo(yval));
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    private void trackBarRandomness_MouseHover(object sender, EventArgs e)
    {
      toolTipRandomness.SetToolTip(trackBarRandomness,
                                   "If you lower the percentage value you'll get results more similar");
      toolTipRandomness.Active = true;
    }

    private void trackBarRandomness_MouseLeave(object sender, EventArgs e)
    {
      toolTipRandomness.Active = false;
    }

    private void labelSimilarArtistsUpDown_MouseHover(object sender, EventArgs e)
    {
      toolTipRandomness.SetToolTip(labelSimilarArtistsUpDown,
                                   "Increase if you do not get enough songs - lower if the playlist grows too fast");
      toolTipRandomness.Active = true;
    }

    private void labelSimilarArtistsUpDown_MouseLeave(object sender, EventArgs e)
    {
      toolTipRandomness.Active = false;
    }

    private void linkLabelMPGroup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      // Determine which link was clicked within the LinkLabel.
      //this.linkLabelMPGroup.Links[linkLabelMPGroup.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "http://www.last.fm/group/MediaPortal%2BUsers");
      }
      catch {}
    }

    private void linkLabelNewUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      //this.linkLabelNewUser.Links[linkLabelNewUser.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "https://www.last.fm/join/");
      }
      catch {}
    }

    private void trackBarArtistMatch_ValueChanged(object sender, EventArgs e)
    {
      labelTrackBarValue.Text = Convert.ToString(trackBarArtistMatch.Value);
    }

    private void trackBarRandomness_ValueChanged(object sender, EventArgs e)
    {
      labelPercRand.Text = Convert.ToString(trackBarRandomness.Value);
    }

    private void comboBoxNeighbourMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxNeighbourMode.SelectedIndex)
      {
        case 0:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
          break;
        case 1:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
        case 2:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
          break;
      }
    }

    private void comboBoxNModeSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxNModeSelect.SelectedIndex)
      {
        case 0:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
          break;
        case 1:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
        case 2:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
          break;
      }
    }

    private void trackBarConsiderCount_ValueChanged(object sender, EventArgs e)
    {
      switch (trackBarConsiderCount.Value)
      {
        case 0:
          labelPlaycountHint.Text = "add only unheard tracks for artist";
          break;
        case 1:
          labelPlaycountHint.Text = "add only rarely heard tracks for artist";
          break;
        case 2:
          labelPlaycountHint.Text = "add totally random tracks for artist";
          break;
        case 3:
          labelPlaycountHint.Text = "add only popular tracks for artist";
          break;
      }
    }

    private void textBoxTagToSearch_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        buttonTaggedArtists_Click(sender, e);
      }
    }

    private void comboBoxUserName_SelectedIndexChanged(object sender, EventArgs e)
    {
      _currentUser = comboBoxUserName.Text;
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("audioscrobbler", "user", _currentUser);
      }
      LoadSettings();
    }


    private void maskedTextBoxASPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        buttonOk_Click(sender, e);
      }
    }

    #endregion

    #region Internal formatting

    private ListViewItem BuildListViewItemSingleTag(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewArtist(Song song_, bool showPlayed_, bool showMatch_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);
      if (showPlayed_)
      {
        listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      }
      if (showMatch_)
      {
        listItem.SubItems.Add(song_.LastFMMatch);
      }
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewArtistAlbum(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);
      listItem.SubItems.Add(song_.Album);
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewTrackArtist(Song song_, bool showPlayed_, bool showDate_)
    {
      ListViewItem listItem = new ListViewItem(song_.Title);
      listItem.SubItems.Add(song_.Artist);
      if (showPlayed_)
      {
        listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      }
      if (showDate_)
      {
        listItem.SubItems.Add(song_.getQueueTime(false));
      }
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewFullTrack(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Title);
      listItem.SubItems.Add(song_.Artist);
      listItem.SubItems.Add(song_.Album);
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    #endregion

    #region Button events

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      if (maskedTextBoxASPassword.Text.Length > 0)
      {
        SaveSettings();
        this.Close();
      }
      else
      {
        MessageBox.Show("No password has been configured yet!");
      }
    }

    private void buttonTagsRefresh_Click(object sender, EventArgs e)
    {
      buttonTagsRefresh.Enabled = false;
      listViewTags.Clear();
      ListViewItem listItem = new ListViewItem();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptags, _currentUser);
      listViewTags.BeginUpdate();
      listViewTags.Columns.Add("Your tags", 170);
      listViewTags.Columns.Add("Popularity", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTags.Items.Add(BuildListViewItemSingleTag(songList[i]));
      }
      listViewTags.EndUpdate();
      buttonTagsRefresh.Enabled = true;
    }

    private void buttonTaggedArtists_Click(object sender, EventArgs e)
    {
      buttonGetTaggedArtists.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedartists, HttpUtility.UrlEncode(textBoxTagToSearch.Text),
                                              checkBoxTagRandomize.Checked, checkBoxLocalOnly.Checked);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Popularity", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTags.Items.Add(BuildListViewArtist(songList[i], true, false));
      }
      buttonGetTaggedArtists.Enabled = true;
    }

    private void buttonTaggedAlbums_Click(object sender, EventArgs e)
    {
      buttonTaggedAlbums.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedalbums, HttpUtility.UrlEncode(textBoxTagToSearch.Text),
                                              checkBoxTagRandomize.Checked, checkBoxLocalOnly.Checked);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Album", 170);
      listViewTags.Columns.Add("Popularity", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTags.Items.Add(BuildListViewArtistAlbum(songList[i]));
      }
      buttonTaggedAlbums.Enabled = true;
    }

    private void buttonTaggedTracks_Click(object sender, EventArgs e)
    {
      buttonTaggedTracks.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedtracks, HttpUtility.UrlEncode(textBoxTagToSearch.Text),
                                              checkBoxTagRandomize.Checked, checkBoxLocalOnly.Checked);
      listViewTags.Columns.Add("Track", 170);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Played", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTags.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      }
      buttonTaggedTracks.Enabled = true;
    }

    private void buttonRefreshRecent_Click(object sender, EventArgs e)
    {
      buttonRefreshRecent.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>();
      ;
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.recenttracks, _currentUser);
      listViewRecentTracks.Columns.Add("Track", 155);
      listViewRecentTracks.Columns.Add("Artist", 155);
      listViewRecentTracks.Columns.Add("Date", 115);

      for (int i = 0; i < songList.Count; i++)
      {
        listViewRecentTracks.Items.Add(BuildListViewTrackArtist(songList[i], false, true));
      }
      buttonRefreshRecent.Enabled = true;
    }

    private void buttonRefreshRecentLoved_Click(object sender, EventArgs e)
    {
      buttonRefreshRecentLoved.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>();
      ;
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.recentlovedtracks, _currentUser);
      listViewRecentTracks.Columns.Add("Track", 155);
      listViewRecentTracks.Columns.Add("Artist", 155);
      listViewRecentTracks.Columns.Add("Date", 115);

      for (int i = 0; i < songList.Count; i++)
      {
        listViewRecentTracks.Items.Add(BuildListViewTrackArtist(songList[i], false, true));
      }
      buttonRefreshRecentLoved.Enabled = true;
    }

    private void buttonRefreshRecentBanned_Click(object sender, EventArgs e)
    {
      buttonRefreshRecentBanned.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>();
      ;
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.recentbannedtracks, _currentUser);
      listViewRecentTracks.Columns.Add("Track", 155);
      listViewRecentTracks.Columns.Add("Artist", 155);
      listViewRecentTracks.Columns.Add("Date", 115);

      for (int i = 0; i < songList.Count; i++)
      {
        listViewRecentTracks.Items.Add(BuildListViewTrackArtist(songList[i], false, true));
      }
      buttonRefreshRecentBanned.Enabled = true;
    }

    private void buttonArtistsRefresh_Click(object sender, EventArgs e)
    {
      buttonArtistsRefresh.Enabled = false;
      listViewTopArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, _currentUser);
      listViewTopArtists.Columns.Add("Artist", 200);
      listViewTopArtists.Columns.Add("Play count", 100);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTopArtists.Items.Add(BuildListViewArtist(songList[i], true, false));
      }
      buttonArtistsRefresh.Enabled = true;
    }

    private void buttonRefreshWeeklyArtists_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyArtists.Enabled = false;
      listViewWeeklyArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklyartistchart, _currentUser);
      listViewWeeklyArtists.Columns.Add("Artist", 200);
      listViewWeeklyArtists.Columns.Add("Play count", 100);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewWeeklyArtists.Items.Add(BuildListViewArtist(songList[i], true, false));
      }
      buttonRefreshWeeklyArtists.Enabled = true;
    }

    private void buttonTopTracks_Click(object sender, EventArgs e)
    {
      buttonTopTracks.Enabled = false;
      listViewTopTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptracks, _currentUser);
      listViewTopTracks.Columns.Add("Track", 170);
      listViewTopTracks.Columns.Add("Artist", 170);
      listViewTopTracks.Columns.Add("Play count", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewTopTracks.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      }
      buttonTopTracks.Enabled = true;
    }

    private void buttonRefreshWeeklyTracks_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyTracks.Enabled = false;
      listViewWeeklyTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklytrackchart, _currentUser);
      listViewWeeklyTracks.Columns.Add("Track", 170);
      listViewWeeklyTracks.Columns.Add("Artist", 170);
      listViewWeeklyTracks.Columns.Add("Play count", 70);
      for (int i = 0; i < songList.Count; i++)
      {
        listViewWeeklyTracks.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      }
      buttonRefreshWeeklyTracks.Enabled = true;
    }

    private void changeControlsSuggestions(bool runningNow_)
    {
      if (runningNow_)
      {
        buttonRefreshSuggestions.Enabled = false;
        trackBarArtistMatch.Hide();
        labelArtistMatch.Hide();
        labelTrackBarValue.Hide();
        progressBarSuggestions.Value = 0;
        progressBarSuggestions.Visible = true;
        listViewSuggestions.Clear();
      }
      else
      {
        progressBarSuggestions.Visible = false;
        trackBarArtistMatch.Show();
        labelArtistMatch.Show();
        labelTrackBarValue.Show();
        buttonRefreshSuggestions.Enabled = true;
      }
    }

    private void buttonRefreshRecommendations_Click(object sender, EventArgs e)
    {
      buttonRefreshSysRecs.Enabled = false;
      listViewSysRecs.Clear();
      songList = new List<Song>();

      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.systemrecs, _currentUser);
      listViewSysRecs.Columns.Add("Artist", 250);

      for (int i = 0; i < songList.Count; i++)
      {
        listViewSysRecs.Items.Add(BuildListViewArtist(songList[i], false, false));
      }

      buttonRefreshSysRecs.Enabled = true;
    }


    private void buttonRefreshSuggestions_Click(object sender, EventArgs e)
    {
      changeControlsSuggestions(true);
      lastFmLookup.ArtistMatchPercent = trackBarArtistMatch.Value;

      //      progressBarSuggestions.PerformStep();
      songList = new List<Song>();
      similarList = new List<Song>();

      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, _currentUser);
      progressBarSuggestions.PerformStep();

      listViewSuggestions.Columns.Add("Artist", 250);
      listViewSuggestions.Columns.Add("Match", 100);

      if (songList.Count > 7)
      {
        listViewSuggestions.SuspendLayout();

        for (int i = 0; i <= 7; i++)
        {
          similarList.AddRange(lastFmLookup.getSimilarArtists(songList[i].ToURLArtistString(), false));
          progressBarSuggestions.PerformStep();
        }

        for (int i = 0; i < similarList.Count; i++)
        {
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewSuggestions.Items.Count; j++)
          {
            if (listViewSuggestions.Items[j].Text == similarList[i].Artist)
            {
              foundDoubleEntry = true;
              break;
            }
          }
          if (!foundDoubleEntry)
          {
            listViewSuggestions.Items.Add(BuildListViewArtist(similarList[i], false, true));
          }
        }

        listViewSuggestions.Sorting = SortOrder.Descending;
        listViewSuggestions.ListViewItemSorter = new ListViewItemComparer(1);
        progressBarSuggestions.PerformStep();

        listViewSuggestions.ResumeLayout();
      }
      else
      {
        listViewSuggestions.Items.Add("Not enough overall top artists found");
      }
      progressBarSuggestions.PerformStep();
      changeControlsSuggestions(false);
    }

    private void buttonRefreshNeighbours_Click(object sender, EventArgs e)
    {
      buttonRefreshNeighbours.Enabled = false;
      listViewNeighbours.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.neighbours, _currentUser);

      listViewNeighbours.Columns.Add("Your neighbours", 250);
      listViewNeighbours.Columns.Add("Match", 100);

      for (int i = 0; i < songList.Count; i++)
      {
        listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], false, true));
      }
      buttonRefreshNeighbours.Enabled = true;

      if (listViewNeighbours.Items.Count > 0)
      {
        buttonRefreshNeigboursArtists.Enabled = true;
        comboBoxNeighbourMode.Enabled = true;
      }
      else
      {
        buttonRefreshNeigboursArtists.Enabled = false;
        comboBoxNeighbourMode.Enabled = false;
      }
    }

    private void buttonRefreshNeigboursArtists_Click(object sender, EventArgs e)
    {
      buttonRefreshNeigboursArtists.Enabled = false;
      listViewNeighbours.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getNeighboursArtists(false);
      listViewNeighbours.Columns.Add("Artist", 170);
      listViewNeighbours.Columns.Add("Play count", 70);

      for (int i = 0; i < songList.Count; i++)
      {
        bool foundDoubleEntry = false;
        for (int j = 0; j < listViewNeighbours.Items.Count; j++)
        {
          if (listViewNeighbours.Items[j].Text == songList[i].Artist)
          {
            foundDoubleEntry = true;
            break;
          }
        }
        if (!foundDoubleEntry)
        {
          listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], true, false));
        }
      }
      buttonRefreshNeigboursArtists.Enabled = true;
      if (listViewNeighbours.Items.Count > 0)
      {
        listViewNeighbours.ListViewItemSorter = new ListViewItemComparer(1);
        buttonNeighboursFilter.Enabled = true;
      }
      else
      {
        buttonNeighboursFilter.Enabled = false;
      }
    }

    private void buttonNeighboursFilter_Click(object sender, EventArgs e)
    {
      //      buttonNeighboursFilter.Enabled = false;
      listViewNeighbours.Clear();
      tabControlLiveFeeds.Enabled = false;
      ArrayList artistsInDB = new ArrayList();
      songList = new List<Song>();
      songList = lastFmLookup.getNeighboursArtists(false);
      listViewNeighbours.Columns.Add("Artist", 250);
      for (int i = 0; i < songList.Count; i++)
      {
        MusicDatabase mdb = MusicDatabase.Instance;
        if (mdb.GetArtists(4, songList[i].Artist, ref artistsInDB))
        {
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewNeighbours.Items.Count; j++)
          {
            if (listViewNeighbours.Items[j].Text == songList[i].Artist)
            {
              foundDoubleEntry = true;
              break;
            }
          }
          if (!foundDoubleEntry)
          {
            listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], false, false));
          }
        }
        //else
        //  MessageBox.Show("Artist " + songList[i].Artist + " already in DB!");
      }
      tabControlLiveFeeds.Enabled = true;
    }


    private void buttonCoverArtistsRefresh_Click(object sender, EventArgs e)
    {
      buttonCoverArtistsRefresh.Enabled = false;
      buttonCoverArtistsLookup.Enabled = false;
      listViewCoverArtists.Clear();
      ArrayList CoverArtists = new ArrayList();
      MusicDatabase mdb = MusicDatabase.Instance;
      if (mdb.GetAllArtists(ref CoverArtists))
      {
        listViewCoverArtists.Columns.Add("Artist", 300);
        listViewCoverArtists.Columns.Add("Low res", 60);
        listViewCoverArtists.Columns.Add("High res", 60);

        progressBarCoverArtists.Maximum = CoverArtists.Count;
        progressBarCoverArtists.Value = 0;
        progressBarCoverArtists.Visible = true;

        for (int i = 0; i < CoverArtists.Count; i++)
        {
          try
          {
            string curArtist = CoverArtists[i].ToString();
            ListViewItem listItem = new ListViewItem(curArtist);

            // check low res
            string strThumb = Util.Utils.GetCoverArt(Thumbs.MusicArtists, curArtist);
            if (File.Exists(strThumb))
            {
              listItem.SubItems.Add((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listItem.SubItems.Add("none");
              Log.Info("Audioscrobbler: Artist cover missing: {0}", curArtist);
            }

            // check high res
            strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
            if (File.Exists(strThumb))
            {
              listItem.SubItems.Add((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listItem.SubItems.Add("none");
            }

            listViewCoverArtists.Items.Add(listItem);

            progressBarCoverArtists.Value = i + 1;
          }
          catch (Exception) {}
        }
        progressBarCoverArtists.Visible = false;
      }
      buttonCoverArtistsRefresh.Enabled = true;
      buttonCoverArtistsLookup.Enabled = true;
    }

    private void buttonCoverArtistLookup_Click(object sender, EventArgs e)
    {
      buttonCoverArtistsRefresh.Enabled = false;
      buttonCoverArtistsLookup.Enabled = false;

      if (listViewCoverArtists.Items.Count > 0)
      {
        string strVariousArtists = GUILocalizeStrings.Get(340).ToLowerInvariant();
        progressBarCoverArtists.Maximum = listViewCoverArtists.Items.Count;
        progressBarCoverArtists.Value = 0;
        progressBarCoverArtists.Visible = true;

        for (int i = 0; i < listViewCoverArtists.Items.Count; i++)
        {
          if (listViewCoverArtists.Items[i].SubItems[2].Text == "none" || !checkBoxCoverArtistsMissing.Checked)
          {
            string curArtist = listViewCoverArtists.Items[i].Text;
            if (curArtist.ToLowerInvariant() == "various artists" || curArtist.ToLowerInvariant() == strVariousArtists ||
                curArtist.ToLowerInvariant() == "unknown")
            {
              listViewCoverArtists.Items[i].ForeColor = Color.LightGray;
              continue;
            }

            lastFmLookup.getArtistInfo(curArtist);
            // let's check and update the artist's status
            string strThumb = Util.Utils.GetCoverArt(Thumbs.MusicArtists, curArtist);
            if (File.Exists(strThumb))
            {
              listViewCoverArtists.Items[i].ForeColor = Color.DarkGreen;
              listViewCoverArtists.Items[i].SubItems[1].Text = ((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listViewCoverArtists.Items[i].ForeColor = Color.Red;
            }

            strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
            if (File.Exists(strThumb))
            {
              listViewCoverArtists.Items[i].ForeColor = Color.DarkGreen;
              listViewCoverArtists.Items[i].SubItems[2].Text = ((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listViewCoverArtists.Items[i].ForeColor = Color.Red;
            }

            //listViewCoverArtists.RedrawItems(i, i, false);
            this.Refresh();
            listViewCoverArtists.Items[i].EnsureVisible();
          }

          progressBarCoverArtists.Value = i + 1;
        }
      }

      progressBarCoverArtists.Visible = false;
      buttonCoverArtistsRefresh.Enabled = true;
      buttonCoverArtistsLookup.Enabled = true;
    }

    private void buttonCoverAlbumsRefresh_Click(object sender, EventArgs e)
    {
      buttonCoverAlbumsRefresh.Enabled = false;
      buttonCoverAlbumsLookup.Enabled = false;
      listViewCoverAlbums.Clear();
      List<AlbumInfo> CoverAlbums = new List<AlbumInfo>();
      MusicDatabase mdb = MusicDatabase.Instance;
      if (mdb.GetAllAlbums(ref CoverAlbums))
      {
        listViewCoverAlbums.Columns.Add("Artist", 150);
        listViewCoverAlbums.Columns.Add("Album", 150);
        listViewCoverAlbums.Columns.Add("Low res", 60);
        listViewCoverAlbums.Columns.Add("High res", 60);

        progressBarCoverAlbums.Maximum = CoverAlbums.Count;
        progressBarCoverAlbums.Value = 0;
        progressBarCoverAlbums.Visible = true;

        for (int i = 0; i < CoverAlbums.Count; i++)
        {
          try
          {
            string curArtist = CoverAlbums[i].AlbumArtist.Trim(new char[] {'|', ' '});
            string curAlbum = CoverAlbums[i].Album;

            if (curArtist.ToLowerInvariant().Contains("unknown"))
            {
              curArtist = CoverAlbums[i].Artist.Trim(new char[] {'|', ' '});
            }

            ListViewItem listItem = new ListViewItem(curArtist);
            listItem.SubItems.Add(curAlbum);

            // check low res
            string strThumb = Util.Utils.GetAlbumThumbName(curArtist, curAlbum);
            if (File.Exists(strThumb))
            {
              listItem.SubItems.Add((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listItem.SubItems.Add("none");
              Log.Info("Audioscrobbler: Album cover missing: {0} - {1}", curArtist, curAlbum);
            }

            // check high res
            strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
            if (File.Exists(strThumb))
            {
              listItem.SubItems.Add((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listItem.SubItems.Add("none");
            }

            listViewCoverAlbums.Items.Add(listItem);

            progressBarCoverAlbums.Value = i + 1;
          }
          catch (Exception) {}
        }
        progressBarCoverAlbums.Visible = false;
      }
      buttonCoverAlbumsRefresh.Enabled = true;
      buttonCoverAlbumsLookup.Enabled = true;
    }

    private void buttonCoverAlbumsLookup_Click(object sender, EventArgs e)
    {
      buttonCoverAlbumsRefresh.Enabled = false;
      buttonCoverAlbumsLookup.Enabled = false;

      if (listViewCoverAlbums.Items.Count > 0)
      {
        string strVariousArtists = GUILocalizeStrings.Get(340).ToLowerInvariant();
        progressBarCoverAlbums.Maximum = listViewCoverAlbums.Items.Count;
        progressBarCoverAlbums.Value = 0;
        progressBarCoverAlbums.Visible = true;

        for (int i = 0; i < listViewCoverAlbums.Items.Count; i++)
        {
          if (listViewCoverAlbums.Items[i].SubItems[3].Text == "none" || !checkBoxCoverAlbumsMissing.Checked)
          {
            string curArtist = listViewCoverAlbums.Items[i].Text;
            string curAlbum = listViewCoverAlbums.Items[i].SubItems[1].Text;

            if (string.IsNullOrEmpty(curArtist) || string.IsNullOrEmpty(curAlbum))
            {
              continue;
            }
            if (curArtist.ToLowerInvariant() == "various artists" || curArtist.ToLowerInvariant() == strVariousArtists ||
                curArtist.ToLowerInvariant() == "unknown" || curAlbum.ToLowerInvariant() == "unknown")
            {
              listViewCoverAlbums.Items[i].ForeColor = Color.LightGray;
              continue;
            }

            lastFmLookup.getAlbumInfo(curArtist, curAlbum, false, false);
            // let's check and update the artist's status
            string strThumb = Util.Utils.GetAlbumThumbName(curArtist, curAlbum);
            if (File.Exists(strThumb))
            {
              listViewCoverAlbums.Items[i].ForeColor = Color.DarkGreen;
              listViewCoverAlbums.Items[i].SubItems[2].Text = ((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listViewCoverAlbums.Items[i].ForeColor = Color.Red;
            }

            strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
            if (File.Exists(strThumb))
            {
              listViewCoverAlbums.Items[i].ForeColor = Color.DarkGreen;
              listViewCoverAlbums.Items[i].SubItems[3].Text = ((new FileInfo(strThumb).Length / 1024) + "KB");
            }
            else
            {
              listViewCoverAlbums.Items[i].ForeColor = Color.Red;
            }

            //listViewCoverAlbums.RedrawItems(i, i, false);
            listViewCoverAlbums.Items[i].EnsureVisible();
            this.Refresh();
          }

          progressBarCoverAlbums.Value = i + 1;
        }
      }

      progressBarCoverAlbums.Visible = false;
      buttonCoverAlbumsRefresh.Enabled = true;
      buttonCoverAlbumsLookup.Enabled = true;
    }

    private void buttonAddUser_Click(object sender, EventArgs e)
    {
      if (tabControlSettings.TabCount > 1)
      {
        tabControlLiveFeeds.Enabled = false;
        tabControlSettings.TabPages.RemoveAt(1);
        tabControlSettings.TabPages.RemoveAt(1);
        tabControlSettings.TabPages.RemoveAt(1);
      }
      labelNewUserHint.Visible = true;
      comboBoxUserName.DropDownStyle = ComboBoxStyle.DropDown;
      comboBoxUserName.Text = string.Empty;
      maskedTextBoxASPassword.Text = string.Empty;
      groupBoxOptions.Enabled = false;
      groupBoxProfile.Visible = false;
      buttonAddUser.Enabled = false;
      buttonDelUser.Enabled = false;
      comboBoxUserName.Focus();
    }

    private void buttonDelUser_Click(object sender, EventArgs e)
    {
      MusicDatabase mdb = MusicDatabase.Instance;
      mdb.DeleteScrobbleUser(comboBoxUserName.Text);
      if (comboBoxUserName.Items.Count <= 1)
      {
        buttonDelUser.Enabled = false;
        maskedTextBoxASPassword.Clear();
      }
      comboBoxUserName.Items.Clear();
      LoadSettings();
    }

    #endregion
  }
}