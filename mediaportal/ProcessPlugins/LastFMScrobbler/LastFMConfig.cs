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
using MediaPortal.GUI.Library;
using MediaPortal.LastFM;
using MediaPortal.Music.Database;
using MediaPortal.Profile;

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  public partial class LastFMConfig : Form
  {

    public LastFMConfig()
    {
      InitializeComponent();
      using (var xmlreader = new MPSettings())
      {
        chkAutoDJ.Checked = xmlreader.GetValueAsBool("lastfm:test", "autoDJ", true);
        numRandomness.Value = xmlreader.GetValueAsInt("lastfm:test", "randomness", 100);
        chkAnnounce.Checked = xmlreader.GetValueAsBool("lastfm:test", "announce", true);
        chkScrobble.Checked = xmlreader.GetValueAsBool("lastfm:test", "scrobble", true);
        chkDiferentVersions.Checked = xmlreader.GetValueAsBool("lastfm:test", "allowDiffVersions", true);
      }

      if (string.IsNullOrEmpty(MusicDatabase.Instance.GetLastFMSK())) return;

      LastFMUser user;
      try
      {
        user = LastFMLibrary.GetUserInfo(MusicDatabase.Instance.GetLastFMUser());
      }
      catch (Exception ex)
      {
        Log.Error("Error getting user info for: {0}", MusicDatabase.Instance.GetLastFMUser());
        Log.Error(ex);
        return;
      }

      if (user == null || string.IsNullOrEmpty(user.UserImgURL)) return;

      pbLastFMUser.ImageLocation = user.UserImgURL;
    }

    private void btnWebAuthenticate_Click(object sender, EventArgs e)
    {
      var frm = new LastFMAuthentication();
      frm.Show();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void SaveSettings()
    {
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("lastfm:test", "autoDJ", chkAutoDJ.Checked);
        xmlwriter.SetValueAsBool("lastfm:test", "allowDiffVersions", chkDiferentVersions.Checked);
        xmlwriter.SetValueAsBool("lastfm:test", "avoidDuplicates", chkAvoidDuplicates.Checked);
        xmlwriter.SetValue("lastfm:test", "randomness", numRandomness.Value);
        xmlwriter.SetValueAsBool("lastfm:test", "announce", chkAnnounce.Checked);
        xmlwriter.SetValueAsBool("lastfm:test", "scrobble", chkScrobble.Checked);
      }      
    }

  }
}
