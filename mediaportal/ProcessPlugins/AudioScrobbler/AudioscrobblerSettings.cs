#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web.Security;
using System.Windows.Forms;

using MediaPortal.Music.Database;
using MediaPortal.Util;


namespace MediaPortal.AudioScrobbler
{
  public partial class AudioscrobblerSettings : Form
  {
    private AudioscrobblerUtils lastFmLookup;
    List<Song> songList = null;
    List<Song> similarList = null;

    public AudioscrobblerSettings()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation
    protected void LoadSettings()
    {
      string tmpuser = "";
      string tmppass = "";
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        tmpuser = xmlreader.GetValueAsString("audioscrobbler", "user", "");

        if (tmpuser == "")
        {
          tabControlASSettings.Enabled = false;
        }
        // only load settings if a user is present
        else
        {
          textBoxASUsername.Text = tmpuser;
          //EncryptDecrypt Crypter = new EncryptDecrypt();
          //string tmpPass;
          //tmpPass = xmlreader.GetValueAsString("audioscrobbler", "pass", "");
          //if (tmpPass != String.Empty)
          //{
          //  try
          //  {
          //    EncryptDecrypt DCrypter = new EncryptDecrypt();
          //    maskedTextBoxASPassword.Text = DCrypter.Decrypt(tmpPass);
          //  }
          //  catch (Exception)
          //  {
          //    //Log.Write("Audioscrobbler: Password decryption failed {0}", ex.Message);
          //  }
          //}

          CheckOrSetDefaultDBSettings(tmpuser);
          // run twice for now - 1st run = inserting -1 / 2nd run = setting defaults
          CheckOrSetDefaultDBSettings(tmpuser);

          int tmpNMode = 1;
          int tmpRand = 77;
          MusicDatabase mdb = new MusicDatabase();
          //Log.Write("DEBUG **** : {0}", tmpuser);
          tmppass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "");
          //Log.Write("DEBUG **** : {0}", tmppass);
          maskedTextBoxASPass.Text = tmppass;

          checkBoxLogVerbose.Checked = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iDebugLog", -1) == 1) ? true : false;
          tmpRand = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iRandomness", -1);
          checkBoxEnableSubmits.Checked = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iSubmitOn", -1) == 1) ? true : false;
          checkBoxScrobbleDefault.Checked = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iScrobbleDefault", -1) == 1) ? true : false;
          numericUpDownSimilarArtist.Value = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iAddArtists", -1);
          numericUpDownTracksPerArtist.Value = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iAddTracks", -1);
          tmpNMode = mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(tmpuser)), "iNeighbourMode", -1);

          trackBarRandomness.Value = (tmpRand >= 25) ? tmpRand : 25;
          lastFmLookup = new AudioscrobblerUtils();
          //int tmpNMode = xmlreader.GetValueAsInt("audioscrobbler", "neighbourmode", 1);

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
        }
      }
    }

    protected void CheckOrSetDefaultDBSettings(string userName_)
    {
      MusicDatabase mdb = new MusicDatabase();
      // disable log
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iDebugLog", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iDebugLog", 0);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iDebugLog");
      }
      // set randomness
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iRandomness", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iRandomness", 77);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iRandomness");
      }     
      // enable scrobbling
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iSubmitOn", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iSubmitOn", 1);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iSubmitOn");
      }
      // disable Scrobble On on startup
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iScrobbleDefault", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iScrobbleDefault", 0);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iScrobbleDefault");
      }
      // consider 3 artists to add
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iAddArtists", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iAddArtists", 3);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iAddArtists");
      }
      // consider adding 1 track per artist
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iAddTracks", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iAddTracks", 1);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iAddTracks");
      }
      // set neighbour mode to weekly artists
      if (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iNeighbourMode", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(userName_)), "iNeighbourMode", 1);
        //Log.Write("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iNeighbourMode");
      }
    }

    protected void SaveSettings()
    {
      int usedebuglog = 0;
      int submitsenabled = 1;
      int scrobbledefault = 0;
      int randomness = 77;
      int artisttoadd = 3;
      int trackstoadd = 1;
      int neighbourmode = 1;

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("audioscrobbler", "user", textBoxASUsername.Text);
        string tmpPass = "";
        try
        {
          EncryptDecrypt Crypter = new EncryptDecrypt();
          tmpPass = Crypter.Encrypt(maskedTextBoxASPassword.Text);
        }
        catch (Exception)
        {
          //Log.Write("Audioscrobbler: Password encryption failed {0}", ex.Message);
        }
        //xmlwriter.SetValue("audioscrobbler", "pass", tmpPass);

        if (checkBoxLogVerbose != null)
          usedebuglog = checkBoxLogVerbose.Checked ? 1 : 0;
        if (checkBoxEnableSubmits != null)
          submitsenabled = checkBoxEnableSubmits.Checked ? 1 : 0;
        if (checkBoxScrobbleDefault != null)
          scrobbledefault = checkBoxScrobbleDefault.Checked ? 1 : 0;
        if (trackBarRandomness != null)
          randomness = trackBarRandomness.Value;
        if (numericUpDownSimilarArtist != null)
          artisttoadd = (int)numericUpDownSimilarArtist.Value;
        if (numericUpDownTracksPerArtist != null)
          trackstoadd = (int)numericUpDownTracksPerArtist.Value;
        if (lastFmLookup != null)
          neighbourmode = (int)lastFmLookup.CurrentNeighbourMode;

        MusicDatabase mdb = new MusicDatabase();
        // checks and adds the user if necessary + updates the password;
        mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), tmpPass);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iDebugLog", usedebuglog);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iRandomness", randomness);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iSubmitOn", submitsenabled);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iScrobbleDefault", scrobbledefault);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iAddArtists", artisttoadd);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iAddTracks", trackstoadd);
        mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(textBoxASUsername.Text)), "iNeighbourMode", neighbourmode);
      }
    }
    #endregion

    #region control events
    private void linkLabelMPGroup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      // Determine which link was clicked within the LinkLabel.
      this.linkLabelMPGroup.Links[linkLabelMPGroup.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "http://www.last.fm/group/MediaPortal%2BUsers");
      }
      catch
      {
      }
    }

    private void linkLabelNewUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      this.linkLabelNewUser.Links[linkLabelNewUser.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "https://www.last.fm/join/");
      }
      catch
      {
      }
    }

    private void textBoxASUsername_Leave(object sender, EventArgs e)
    {
      if (textBoxASUsername.Text != "")
      {
        tabControlASSettings.Enabled = true;
      }
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

    private void textBoxTagToSearch_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        buttonGetTaggedArtists_Click(sender, e);
    }

    #endregion

    #region Button events

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void buttonTagsRefresh_Click(object sender, EventArgs e)
    {
      buttonTagsRefresh.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptags, "");      
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(songList[i].ToLastFMString());
      buttonTagsRefresh.Enabled = true;
    }

    private void buttonGetTaggedArtists_Click(object sender, EventArgs e)
    {
      buttonGetTaggedArtists.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedartists, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
//      songList = lastFmLookup.getTagsForTrack("Metallica","Battery");
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(songList[i].ToLastFMMatchString(false));
      buttonGetTaggedArtists.Enabled = true;
    }

    private void buttonTaggedAlbums_Click(object sender, EventArgs e)
    {
      buttonTaggedAlbums.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedalbums, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(songList[i].ToLastFMMatchString(false));
      buttonTaggedAlbums.Enabled = true;
    }

    private void buttonTaggedTracks_Click(object sender, EventArgs e)
    {
      buttonTaggedTracks.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedtracks, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(songList[i].ToLastFMMatchString(false));
      buttonTaggedTracks.Enabled = true;
    }

    private void buttonRefreshRecent_Click(object sender, EventArgs e)
    {
      buttonRefreshRecent.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>(); ;
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.recenttracks, "");
      for (int i = 0; i < songList.Count; i++)
        listViewRecentTracks.Items.Add(songList[i].ToShortString());
      buttonRefreshRecent.Enabled = true;
    }

    private void buttonArtistsRefresh_Click(object sender, EventArgs e)
    {
      buttonArtistsRefresh.Enabled = false;
      listViewTopArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, "");
      for (int i = 0; i < songList.Count; i++)
        listViewTopArtists.Items.Add(songList[i].ToLastFMString());
      buttonArtistsRefresh.Enabled = true;
    }

    private void buttonRefreshWeeklyArtists_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyArtists.Enabled = false;
      listViewWeeklyArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklyartistchart, "");
      for (int i = 0; i < songList.Count; i++)
        listViewWeeklyArtists.Items.Add(songList[i].ToLastFMString());
      buttonRefreshWeeklyArtists.Enabled = true;
    }

    private void buttonTopTracks_Click(object sender, EventArgs e)
    {
      buttonTopTracks.Enabled = false;
      listViewTopTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptracks, "");
      for (int i = 0; i < songList.Count; i++)
        listViewTopTracks.Items.Add(songList[i].ToLastFMString());
      buttonTopTracks.Enabled = true;
    }

    private void buttonRefreshWeeklyTracks_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyTracks.Enabled = false;
      listViewWeeklyTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklytrackchart, "");
      for (int i = 0; i < songList.Count; i++)
        listViewWeeklyTracks.Items.Add(songList[i].ToLastFMString());
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

    private void buttonRefreshSuggestions_Click(object sender, EventArgs e)
    {
      changeControlsSuggestions(true);
      lastFmLookup.ArtistMatchPercent = trackBarArtistMatch.Value;

      progressBarSuggestions.PerformStep();
      songList = new List<Song>();
      similarList = new List<Song>();

      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, "");
      progressBarSuggestions.PerformStep();

      if (songList.Count > 7)
      {
        for (int i = 0; i <= 7; i++)
        {
          similarList.AddRange(lastFmLookup.getSimilarArtists(songList[i].ToURLArtistString(), false));
          progressBarSuggestions.PerformStep();
        }

        for (int i = 0; i < similarList.Count; i++)
        {
          //if (!listViewSuggestions.Items.ContainsKey(similarList[i].ToLastFMString()))
          //  listViewSuggestions.Items.Add(similarList[i].ToLastFMString());
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewSuggestions.Items.Count; j++)
          {
            if (listViewSuggestions.Items[j].Text == similarList[i].ToLastFMMatchString(false))
              foundDoubleEntry = true;
          }
          if (!foundDoubleEntry)
            listViewSuggestions.Items.Add(similarList[i].ToLastFMMatchString(false));
        }
      }
      else
        listViewSuggestions.Items.Add("Not enough overall top artists found");
      progressBarSuggestions.PerformStep();
      changeControlsSuggestions(false);
    }

    private void buttonRefreshNeighbours_Click(object sender, EventArgs e)
    {
      buttonRefreshNeighbours.Enabled = false;
      listViewNeighbours.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.neighbours, "");

      for (int i = 0; i < songList.Count; i++)
        listViewNeighbours.Items.Add(songList[i].ToLastFMString());
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

      for (int i = 0; i < songList.Count; i++)
        listViewNeighbours.Items.Add(songList[i].ToLastFMString());
      buttonRefreshNeigboursArtists.Enabled = true;
      if (listViewNeighbours.Items.Count > 0)
        buttonNeighboursFilter.Enabled = true;
      else
        buttonNeighboursFilter.Enabled = false;
    }

    private void buttonNeighboursFilter_Click(object sender, EventArgs e)
    {
      //      buttonNeighboursFilter.Enabled = false;
      listViewNeighbours.Clear();
      tabControlASSettings.Enabled = false;
      ArrayList artistsInDB = new ArrayList();
      songList = new List<Song>();
      songList = lastFmLookup.getNeighboursArtists(false);

      for (int i = 0; i < songList.Count; i++)
      {
        MusicDatabase mdb = new MusicDatabase();
        if (!mdb.GetArtists(4, songList[i].Artist, ref artistsInDB))
        {
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewNeighbours.Items.Count; j++)
          {
            if (listViewNeighbours.Items[j].Text == songList[i].Artist)
              foundDoubleEntry = true;
          }
          if (!foundDoubleEntry)
            listViewNeighbours.Items.Add(songList[i].Artist);
        }
        //else
        //  MessageBox.Show("Artist " + songList[i].Artist + " already in DB!");
      }
      tabControlASSettings.Enabled = true;
    }

    #endregion

  }
}