#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Windows.Forms;
using MediaPortal.LastFM;
using MediaPortal.Music.Database;
using MediaPortal.Profile;

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  public partial class LastFMConfig : Form
  {

    private string _token;

    public LastFMConfig()
    {
      InitializeComponent();
      using (var xmlreader = new MPSettings())
      {
        chkAutoDJ.Checked = xmlreader.GetValueAsBool("lastfm:test", "autoDJ", true);
        numRandomness.Value = xmlreader.GetValueAsInt("lastfm:test", "randomness", 100);
        chkAnnounce.Checked = xmlreader.GetValueAsBool("lastfm:test", "announce", true);
        chkScrobble.Checked = xmlreader.GetValueAsBool("lastfm:test", "scrobble", true);
      }

      if (string.IsNullOrEmpty(MusicDatabase.Instance.GetLastFMSK())) return;

      var user = LastFMLibrary.GetUserInfo(MusicDatabase.Instance.GetLastFMUser());
      if (user == null || string.IsNullOrEmpty(user.UserImgURL)) return;

      pbLastFMUser.ImageLocation = user.UserImgURL;
    }

   private void chkAutoDJ_CheckedChanged(object sender, EventArgs e)
    {
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("lastfm:test", "autoDJ", chkAutoDJ.Checked);
      }
    }

    private void numRandomness_ValueChanged(object sender, EventArgs e)
    {
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("lastfm:test", "randomness", numRandomness.Value);
      }
    }

    private void chkAnnounce_CheckedChanged(object sender, EventArgs e)
    {
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("lastfm:test", "announce", chkAnnounce.Checked);
      }
    }

    private void chkScrobble_CheckedChanged(object sender, EventArgs e)
    {
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("lastfm:test", "scrobble", chkScrobble.Checked);
      }
    }

    private void btnWebAuthenticate_Click(object sender, EventArgs e)
    {
      var frm = new LastFMAuthentication();
      frm.Show();

      //var user = LastFMLibrary.GetUserInfo();
      //pbLastFMUser.ImageLocation = user.UserImgURL;
    }


  }
}
