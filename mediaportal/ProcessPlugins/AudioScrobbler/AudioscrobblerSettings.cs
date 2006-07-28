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
using System.Windows.Forms;

using MediaPortal.Music.Database;


namespace MediaPortal.AudioScrobbler
{
  public partial class AudioscrobblerSettings : Form
  {
    private AudioscrobblerBase scrobbler;
    List<Song> songList = null;

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
        textBoxASUsername.Text = xmlreader.GetValueAsString("audioscrobbler", "username", "");
        maskedTextBoxASPassword.Text = xmlreader.GetValueAsString("audioscrobbler", "password", "");
      }
    }

    protected void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("audioscrobbler", "username", textBoxASUsername.Text);
        xmlwriter.SetValue("audioscrobbler", "password", maskedTextBoxASPassword.Text);        
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

    private List<Song> getXMLData(lastFMFeed feed_)
    {
      scrobbler = new AudioscrobblerBase();
      scrobbler.Disconnect();
      //scrobbler.ParseXMLDoc(@"C:\recenttracks.xml", "name");
      return scrobbler.ParseXMLDoc(@"http://ws.audioscrobbler.com/1.0/user/" + scrobbler.Username + "/" + "recenttracks.xml", @"//recenttracks/track", feed_);
    }
  }
}