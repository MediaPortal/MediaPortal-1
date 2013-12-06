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

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  public partial class LastFMAuthentication : Form
  {
    public LastFMAuthentication()
    {
      InitializeComponent();
      txtUserName.Text = MusicDatabase.Instance.GetLastFMUser();
    }

    private void btnSubmit_Click(object sender, EventArgs e)
    {
      Submit();
    }

    private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == (char) Keys.Return)
      {
        Submit();
      }
    }

    private void Submit()
    {
      var userName = txtUserName.Text;
      var password = txtPassword.Text;

      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
      {
        MessageBox.Show("Enter a last.fm Username and password", "Missing username / password", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      string sessionKey;
      try
      {
        sessionKey = LastFMLibrary.AuthGetMobileSession(userName, password);
      }
      catch (LastFMException ex)
      {
        MessageBox.Show("Error adding user.\n" + ex.Message, "Error adding user", MessageBoxButtons.OK, MessageBoxIcon.Error);
        this.Close();
        return;
      }

      MusicDatabase.Instance.AddLastFMUser(userName, sessionKey);

      try
      {
        var user = LastFMLibrary.GetUserInfo();
        if (user != null)
        {
          MessageBox.Show("User: " + userName + " Added", "User Added", MessageBoxButtons.OK, MessageBoxIcon.Information);  
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Error adding user.\nUser: " + userName, "Error Adding User", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      this.Close();
    }
  }
}

