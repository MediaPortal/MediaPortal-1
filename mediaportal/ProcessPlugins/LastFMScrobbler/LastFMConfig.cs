using System;
using System.Windows.Forms;
using MediaPortal.Profile;

namespace MediaPortal.LastFM
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
      }
    }

    private void btnAuthenticate_Click(object sender, EventArgs e)
    {
      _token = LastFMLibrary.AuthGetToken();
      var authURL = LastFMLibrary.AuthGetAuthURL(_token);
      System.Diagnostics.Process.Start(authURL);
      btnAuthenticate.Enabled = false;
      btnSecond.Enabled = true;
    }

    private void btnSecond_Click(object sender, EventArgs e)
    {
      var SK = LastFMLibrary.AuthGetSession(_token);

      if (string.IsNullOrEmpty(SK))
      {
        MessageBox.Show("Error authenticating\nPlease try again.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        btnAuthenticate.Enabled = true;
        btnSecond.Enabled = false;
        return;
      }
      using (var xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("lastfm:test", "SK", SK);
      }
      MessageBox.Show("Authentication Succesful", "Authentication Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
  }
}
