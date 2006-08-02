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
    private AudioscrobblerBase scrobbler;
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        checkBoxdisableTimerThread.Checked = xmlreader.GetValueAsBool("audioscrobbler", "disabletimerthread", true);
        checkBoxDismissOnError.Checked = xmlreader.GetValueAsBool("audioscrobbler", "dismisscacheonerror", true);
        checkBoxLogVerbose.Checked = xmlreader.GetValueAsBool("audioscrobbler", "usedebuglog", false);
        textBoxASUsername.Text = xmlreader.GetValueAsString("audioscrobbler", "user", "");
        if (textBoxASUsername.Text == "")
        {
          tabControlASSettings.TabPages.RemoveAt(3);
          tabControlASSettings.TabPages.RemoveAt(2);
          tabControlASSettings.TabPages.RemoveAt(1);
        }
          
        EncryptDecrypt Crypter = new EncryptDecrypt();
        string tmpPass;
        tmpPass = xmlreader.GetValueAsString("audioscrobbler", "pass", "");
        if (tmpPass != String.Empty)
        {
          try
          {
            EncryptDecrypt DCrypter = new EncryptDecrypt();
            maskedTextBoxASPassword.Text = DCrypter.Decrypt(tmpPass);
          }
          catch (Exception ex)
          {
            //Log.Write("Audioscrobbler: Password decryption failed {0}", ex.Message);
          }
        }
      }
    }

    protected void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("audioscrobbler", "disabletimerthread", checkBoxdisableTimerThread.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "dismisscacheonerror", checkBoxDismissOnError.Checked);         
        xmlwriter.SetValueAsBool("audioscrobbler", "usedebuglog", checkBoxLogVerbose.Checked);

        xmlwriter.SetValue("audioscrobbler", "user", textBoxASUsername.Text);
        try
        {
          EncryptDecrypt Crypter = new EncryptDecrypt();
          xmlwriter.SetValue("audioscrobbler", "pass", Crypter.Encrypt(maskedTextBoxASPassword.Text));
        }
        catch (Exception ex)
        {
          //Log.Write("Audioscrobbler: Password encryption failed {0}", ex.Message);
        }
      }
    }
    #endregion

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

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

    private void buttonClearCache_Click(object sender, EventArgs e)
    {
      scrobbler = new AudioscrobblerBase();
      scrobbler.ClearQueue();
    }

    private void buttonRefreshRecent_Click(object sender, EventArgs e)
    {
      buttonRefreshRecent.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>();
      songList = getXMLData(lastFMFeed.recenttracks);
      for (int i = 0; i < songList.Count; i++)
        listViewRecentTracks.Items.Add(songList[i].ToShortString());
      buttonRefreshRecent.Enabled = true;
    }

    private void buttonArtistsRefresh_Click(object sender, EventArgs e)
    {
      buttonArtistsRefresh.Enabled = false;
      listViewTopArtists.Clear();
      songList = new List<Song>();
      songList = getXMLData(lastFMFeed.topartists);
      for (int i = 0; i < songList.Count; i++)
        listViewTopArtists.Items.Add(songList[i].ToLastFMString());
      buttonArtistsRefresh.Enabled = true;
    }

    private void buttonTopTracks_Click(object sender, EventArgs e)
    {
      buttonTopTracks.Enabled = false;
      listViewTopTracks.Clear();
      songList = new List<Song>();
      songList = getXMLData(lastFMFeed.toptracks);
      for (int i = 0; i < songList.Count; i++)
        listViewTopTracks.Items.Add(songList[i].ToLastFMString());
      buttonTopTracks.Enabled = true;
    }

    private void buttonRefreshSuggestions_Click(object sender, EventArgs e)
    {
      buttonRefreshSuggestions.Enabled = false;
      scrobbler = new AudioscrobblerBase();
      scrobbler.Disconnect();
      scrobbler.ArtistMatchPercent = trackBarArtistMatch.Value;
      trackBarArtistMatch.Hide();
      labelArtistMatch.Hide();      
      progressBarSuggestions.Show();      
      listViewSuggestions.Clear();
      progressBarSuggestions.PerformStep();
      songList = new List<Song>();
      similarList = new List<Song>();

      songList = scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "topartists.xml", @"//topartists/artist", lastFMFeed.topartists);
      progressBarSuggestions.PerformStep();
      progressBarSuggestions.PerformStep();
      
      for (int i = 0; i <= 6; i++)
      {
        similarList.AddRange(scrobbler.ParseXMLDocForSimilarArtists(songList[i].ToURLArtistString()));
        progressBarSuggestions.PerformStep();
      }

      for (int i = 0; i < similarList.Count; i++)
      {
        if (!listViewSuggestions.Items.ContainsKey(similarList[i].ToLastFMString()))
          listViewSuggestions.Items.Add(similarList[i].ToLastFMString());
      }
      progressBarSuggestions.PerformStep();
      buttonRefreshSuggestions.Enabled = true;
      progressBarSuggestions.Hide();
      trackBarArtistMatch.Show();
      labelArtistMatch.Show();
    }

    private List<Song> getXMLData(lastFMFeed feed_)
    {
      scrobbler = new AudioscrobblerBase();
      scrobbler.Disconnect();
      //scrobbler.ParseXMLDoc(@"C:\recenttracks.xml", "name");
      switch (feed_)
      {
        case lastFMFeed.recenttracks:
          return scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
        case lastFMFeed.topartists:
          return scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "topartists.xml", @"//topartists/artist", feed_);
        case lastFMFeed.toptracks:
          return scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "toptracks.xml", @"//toptracks/track", feed_);

        default:
          return scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
      }      
    }

  }
}