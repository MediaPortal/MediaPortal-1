#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#region Using

using System;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#endregion

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVServer : SectionSettings
  {
    #region Variables

    /// <summary>
    /// The verified hostname of the TV Server
    /// </summary>
    private static string _verifiedHostname = string.Empty;

    /// <summary>
    /// The hostname from the settings
    /// </summary>
    private string _settingsHostname;

    public int pluginVersion;

    #endregion

    #region Constructor

    public TVServer()
      : this("TV Server") {}

    public TVServer(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    #endregion

    #region Public methods and properties

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        // Get hostname entry
        _settingsHostname = xmlreader.GetValueAsString("tvservice", "hostname", string.Empty);
        if (string.IsNullOrEmpty(_settingsHostname))
        {
          // Set hostname to local host
          mpTextBoxHostname.Text = Dns.GetHostName();
          _verifiedHostname = string.Empty;
          Log.Debug("LoadSettings: set hostname to local host: \"{0}\"", mpTextBoxHostname.Text);
        }
        else
        {
          // Take verified hostname from MediaPortal.xml
          mpTextBoxHostname.Text = _settingsHostname;
          _verifiedHostname = mpTextBoxHostname.Text;
          mpTextBoxHostname.BackColor = Color.YellowGreen;    // verified
          Log.Debug("LoadSettings: take hostname from settings: \"{0}\"", mpTextBoxHostname.Text);
        }

        mpCheckBoxIsWakeOnLanEnabled.Checked = xmlreader.GetValueAsBool("tvservice", "isWakeOnLanEnabled", false);
        mpNumericTextBoxWOLTimeOut.Text = xmlreader.GetValueAsString("tvservice", "WOLTimeOut", "10");
        mpCheckBoxIsAutoMacAddressEnabled.Checked = xmlreader.GetValueAsBool("tvservice", "isAutoMacAddressEnabled",
                                                                             true);
        mpTextBoxMacAddress.Text = xmlreader.GetValueAsString("tvservice", "macAddress", "00:00:00:00:00:00");

        mpCheckBoxIsWakeOnLanEnabled_CheckedChanged(null, null);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        // If hostname is empty, use local hostname
        if (string.IsNullOrEmpty(mpTextBoxHostname.Text))
          mpTextBoxHostname.Text = Dns.GetHostName();

        // Save hostname only, if it is verified
        if (mpTextBoxHostname.BackColor == Color.YellowGreen ||
          (mpTextBoxHostname.BackColor != Color.Red && VerifyHostname(mpTextBoxHostname.Text)))
        {
          // Hostname is valid, update database connection
          Log.Debug("SaveSettings: hostname is valid - update gentle.config if needed");
          if (UpdateGentleConfig(mpTextBoxHostname.Text, mpTextBoxHostname.Text.Equals(_settingsHostname)))
          {
            Log.Debug("SaveSettings: update gentle.config was successfull - save hostname");
            xmlwriter.SetValue("tvservice", "hostname", mpTextBoxHostname.Text);
            _verifiedHostname = mpTextBoxHostname.Text;
          }
          else
          {
            Log.Debug("SaveSettings: error in updating gentle.config - save empty string");
            xmlwriter.SetValue("tvservice", "hostname", string.Empty);
            mpTextBoxHostname.BackColor = Color.Red;
            _verifiedHostname = string.Empty;
          }
        }
        else
        {
          // Hostname is invalid  
          Log.Debug("SaveSettings: hostname is invalid - save empty string");
          mpTextBoxHostname.BackColor = Color.Red;
          xmlwriter.SetValue("tvservice", "hostname", string.Empty);
          _verifiedHostname = string.Empty;
        }

        xmlwriter.SetValueAsBool("tvservice", "isWakeOnLanEnabled", mpCheckBoxIsWakeOnLanEnabled.Checked);
        xmlwriter.SetValue("tvservice", "WOLTimeOut", mpNumericTextBoxWOLTimeOut.Text);
        xmlwriter.SetValueAsBool("tvservice", "isAutoMacAddressEnabled", mpCheckBoxIsAutoMacAddressEnabled.Checked);
        xmlwriter.SetValue("tvservice", "macAddress", mpTextBoxMacAddress.Text);
      }
    }

    public static string Hostname
    {
      get { return _verifiedHostname; }
    }

    #endregion

    #region Designer generated code

    private MPRadioButton radioButton1;
    private MPGroupBox mpGroupBox2;
    private MPButton mpButtonTestConnectioname;
    private MPTextBox mpTextBoxHostname;
    private MPLabel mpLabel3;
    private MPGroupBox mpGroupBox900;
    private MPNumericTextBox mpNumericTextBoxWOLTimeOut;
    private MPLabel mpLabelWOLTimeOut;
    private MPTextBox mpTextBoxMacAddress;
    private MPLabel mpLabel400;
    private MPCheckBox mpCheckBoxIsAutoMacAddressEnabled;
    private MPCheckBox mpCheckBoxIsWakeOnLanEnabled;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButtonTestConnectioname = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox900 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpNumericTextBoxWOLTimeOut = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabelWOLTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxMacAddress = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel400 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxIsAutoMacAddressEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxIsWakeOnLanEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox900.SuspendLayout();
      this.SuspendLayout();
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpButtonTestConnectioname);
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(462, 53);
      this.mpGroupBox2.TabIndex = 17;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TV-Server";
      // 
      // mpButtonTestConnectioname
      // 
      this.mpButtonTestConnectioname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonTestConnectioname.Location = new System.Drawing.Point(325, 20);
      this.mpButtonTestConnectioname.Name = "mpButtonTestConnectioname";
      this.mpButtonTestConnectioname.Size = new System.Drawing.Size(131, 23);
      this.mpButtonTestConnectioname.TabIndex = 9;
      this.mpButtonTestConnectioname.Text = "Test connection";
      this.mpButtonTestConnectioname.UseVisualStyleBackColor = true;
      this.mpButtonTestConnectioname.Click += new System.EventHandler(this.mpButtonTestHostname_Click);
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxHostname.Location = new System.Drawing.Point(126, 22);
      this.mpTextBoxHostname.Name = "mpTextBoxHostname";
      this.mpTextBoxHostname.Size = new System.Drawing.Size(152, 20);
      this.mpTextBoxHostname.TabIndex = 6;
      this.mpTextBoxHostname.TextChanged += new System.EventHandler(this.mpTextBoxHostname_TextChanged);
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(19, 25);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(58, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Hostname:";
      // 
      // mpGroupBox900
      // 
      this.mpGroupBox900.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.mpGroupBox900.Controls.Add(this.mpNumericTextBoxWOLTimeOut);
      this.mpGroupBox900.Controls.Add(this.mpLabelWOLTimeOut);
      this.mpGroupBox900.Controls.Add(this.mpTextBoxMacAddress);
      this.mpGroupBox900.Controls.Add(this.mpLabel400);
      this.mpGroupBox900.Controls.Add(this.mpCheckBoxIsAutoMacAddressEnabled);
      this.mpGroupBox900.Controls.Add(this.mpCheckBoxIsWakeOnLanEnabled);
      this.mpGroupBox900.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox900.Location = new System.Drawing.Point(6, 59);
      this.mpGroupBox900.Name = "mpGroupBox900";
      this.mpGroupBox900.Size = new System.Drawing.Size(462, 126);
      this.mpGroupBox900.TabIndex = 18;
      this.mpGroupBox900.TabStop = false;
      this.mpGroupBox900.Text = "Wake-On-Lan";
      // 
      // mpNumericTextBoxWOLTimeOut
      // 
      this.mpNumericTextBoxWOLTimeOut.AutoCompleteCustomSource.AddRange(new string[] {
            "10",
            "20",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90"});
      this.mpNumericTextBoxWOLTimeOut.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.mpNumericTextBoxWOLTimeOut.Enabled = false;
      this.mpNumericTextBoxWOLTimeOut.Location = new System.Drawing.Point(126, 42);
      this.mpNumericTextBoxWOLTimeOut.MaxLength = 4;
      this.mpNumericTextBoxWOLTimeOut.Name = "mpNumericTextBoxWOLTimeOut";
      this.mpNumericTextBoxWOLTimeOut.Size = new System.Drawing.Size(45, 20);
      this.mpNumericTextBoxWOLTimeOut.TabIndex = 9;
      this.mpNumericTextBoxWOLTimeOut.Tag = "Default timeout is 10 seconds";
      this.mpNumericTextBoxWOLTimeOut.Text = "10";
      this.mpNumericTextBoxWOLTimeOut.Value = 10;
      this.mpNumericTextBoxWOLTimeOut.WordWrap = false;
      // 
      // mpLabelWOLTimeOut
      // 
      this.mpLabelWOLTimeOut.AutoSize = true;
      this.mpLabelWOLTimeOut.Location = new System.Drawing.Point(41, 45);
      this.mpLabelWOLTimeOut.Name = "mpLabelWOLTimeOut";
      this.mpLabelWOLTimeOut.Size = new System.Drawing.Size(72, 13);
      this.mpLabelWOLTimeOut.TabIndex = 8;
      this.mpLabelWOLTimeOut.Text = "WOL timeout:";
      // 
      // mpTextBoxMacAddress
      // 
      this.mpTextBoxMacAddress.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxMacAddress.Location = new System.Drawing.Point(126, 91);
      this.mpTextBoxMacAddress.MaxLength = 17;
      this.mpTextBoxMacAddress.Name = "mpTextBoxMacAddress";
      this.mpTextBoxMacAddress.Size = new System.Drawing.Size(97, 20);
      this.mpTextBoxMacAddress.TabIndex = 7;
      this.mpTextBoxMacAddress.Text = "00:00:00:00:00:00";
      // 
      // mpLabel400
      // 
      this.mpLabel400.AutoSize = true;
      this.mpLabel400.Location = new System.Drawing.Point(41, 94);
      this.mpLabel400.Name = "mpLabel400";
      this.mpLabel400.Size = new System.Drawing.Size(74, 13);
      this.mpLabel400.TabIndex = 6;
      this.mpLabel400.Text = "MAC Address:";
      // 
      // mpCheckBoxIsAutoMacAddressEnabled
      // 
      this.mpCheckBoxIsAutoMacAddressEnabled.AutoSize = true;
      this.mpCheckBoxIsAutoMacAddressEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxIsAutoMacAddressEnabled.Location = new System.Drawing.Point(44, 68);
      this.mpCheckBoxIsAutoMacAddressEnabled.Name = "mpCheckBoxIsAutoMacAddressEnabled";
      this.mpCheckBoxIsAutoMacAddressEnabled.Size = new System.Drawing.Size(192, 17);
      this.mpCheckBoxIsAutoMacAddressEnabled.TabIndex = 1;
      this.mpCheckBoxIsAutoMacAddressEnabled.Text = "Auto-configure server MAC Address";
      this.mpCheckBoxIsAutoMacAddressEnabled.UseVisualStyleBackColor = true;
      this.mpCheckBoxIsAutoMacAddressEnabled.CheckedChanged += new System.EventHandler(this.mpCheckBoxIsAutoMacAddressEnabled_CheckedChanged);
      // 
      // mpCheckBoxIsWakeOnLanEnabled
      // 
      this.mpCheckBoxIsWakeOnLanEnabled.AutoSize = true;
      this.mpCheckBoxIsWakeOnLanEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxIsWakeOnLanEnabled.Location = new System.Drawing.Point(22, 19);
      this.mpCheckBoxIsWakeOnLanEnabled.Name = "mpCheckBoxIsWakeOnLanEnabled";
      this.mpCheckBoxIsWakeOnLanEnabled.Size = new System.Drawing.Size(172, 17);
      this.mpCheckBoxIsWakeOnLanEnabled.TabIndex = 0;
      this.mpCheckBoxIsWakeOnLanEnabled.Text = "Wake up TV Server as needed";
      this.mpCheckBoxIsWakeOnLanEnabled.UseVisualStyleBackColor = true;
      this.mpCheckBoxIsWakeOnLanEnabled.CheckedChanged += new System.EventHandler(this.mpCheckBoxIsWakeOnLanEnabled_CheckedChanged);
      // 
      // TV
      // 
      this.Controls.Add(this.mpGroupBox900);
      this.Controls.Add(this.mpGroupBox2);
      this.Name = "TV";
      this.Size = new System.Drawing.Size(472, 427);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox900.ResumeLayout(false);
      this.mpGroupBox900.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    #region Private methods

    /// <summary>
    /// Verifies that the given hostname is a working tv server
    /// </summary>
    /// <param name="hostname">The TV server's hostname</param>
    /// <returns>Returns treu, if the hostname is a tv server</returns>
    private bool VerifyHostname(string hostname)
    {
      // See if hostname is an active client
      Ping ping = new Ping();
      PingOptions pingOptions = new PingOptions() { DontFragment = true, Ttl = 1 };
      PingReply rep;
      try
      {
        rep = ping.Send(hostname, 100, new byte[] { 1 }, pingOptions);
      }
      catch (Exception ex)
      {
        Log.Debug("VerifyHostname: unable to detect host \"{0}\"" + Environment.NewLine + "{1}", hostname, ex.Message);
        MessageBox.Show(string.Format("Unable to detect host \"{0}\"" + Environment.NewLine + "{1}", hostname, ex.Message),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }
      if (rep.Status == IPStatus.TtlExpired || rep.Status == IPStatus.TimedOut || rep.Status == IPStatus.TimeExceeded)
      {
        Log.Debug("VerifyHostname: unable to detect host \"{0}\"", hostname);
        MessageBox.Show(string.Format("Unable to detect host \"{0}\"", hostname),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      // See if the tv server port is accessible
      TcpClient client = new TcpClient();
      try
      {
        client.Connect(hostname, 31456);
      }
      catch (Exception)
      {
        Log.Debug("VerifyHostname: unable to connect to TV server on host \"{0}\"", hostname);
        MessageBox.Show(string.Format("Unable to connect to TV server on host \"{0}\"", hostname),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }
      client.Close();

      // Set the hostname of the TV server
      TvServerRemote.HostName = hostname;

      // Get the database connection string from the TV server
      string connectionString, provider;
      TvServerRemote.GetDatabaseConnectionString(out connectionString, out provider);
      if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(provider))
      {
        Log.Debug("VerifyHostname: unable to get data from TV server on host \"{0}\"", hostname);
        MessageBox.Show(string.Format("Unable to get data from TV server on host \"{0}\"", hostname),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      Log.Debug("VerifyHostname: verified a working TV server on host \"{0}\"", hostname);
      return true;
    }

    /// <summary>
    /// Updates the database connection string in the gentle.config file
    /// if the hostname has been changed or gentle.config contains "-" as server name.
    /// The connection string is fetched from the TV server.
    /// </summary>
    /// <param name="hostname">The TV server's hostname</param>
    /// <param name="unchanged">Indicates if the hostname has been loaded from the settings</param>
    /// <returns>Returns true, if the gentle.config file is updated</returns>
    private bool UpdateGentleConfig(string hostname, bool unchanged)
    {
      Log.Debug("UpdateGentleConfig({0}, {1})", hostname, unchanged);

      // Load the gentle.config file with the database connection string
      XmlNode node, nodeProvider;
      XmlDocument doc = new XmlDocument();
      try
      {
        doc.Load(Config.GetFile(Config.Dir.Config, "gentle.config"));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        node = nodeKey.Attributes.GetNamedItem("connectionString");
        nodeProvider = nodeKey.Attributes.GetNamedItem("name");
      }
      catch (Exception ex)
      {
        Log.Error("UpdateGentleConfig: unable to open gentle.config" + Environment.NewLine + "{0}", ex.Message);
        MessageBox.Show(string.Format("Unable to open gentle.config" + Environment.NewLine + "{0}", ex.Message),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      // See if gentle.config has to be updated
      if (unchanged && node.InnerText.IndexOf("Server=-;") == -1)
      {
        Log.Debug("UpdateGentleConfig: no need to update gentle.config file");
        return true;
      }

      // Verify tvServer (could be down at the moment)
      if (!VerifyHostname(hostname))
      {
        Log.Error("UpdateGentleConfig: unable to contact TV server on host \"{0}\"", hostname);
        return false;
      }

      // Get the database connection string from the TV server
      string connectionString, provider;
      TvServerRemote.GetDatabaseConnectionString(out connectionString, out provider);
      if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(provider))
      {
        Log.Error("UpdateGentleConfig: unable to get database connection string from TV server \"{0}\"", hostname);
        MessageBox.Show(string.Format("Unable to get database connection string from TV server \"{0}\"", hostname),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      // Save the gentle.config file with the database connection string
      try
      {
        node.InnerText = connectionString;
        nodeProvider.InnerText = provider;
        doc.Save(Config.GetFile(Config.Dir.Config, "gentle.config"));
      }
      catch (Exception ex)
      {
        Log.Error("UpdateGentleConfig: unable to modify gentle.config" + Environment.NewLine + "{0}", ex.Message);
        MessageBox.Show(string.Format("Unable to modify gentle.config" + Environment.NewLine + "{0}", ex.Message),
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      Log.Debug("UpdateGentleConfig: updated gentle.config with connectionString \"{0}\" for provider \"{1}\"", connectionString, provider);
      return true;
    }

    private void mpCheckBoxIsAutoMacAddressEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (mpCheckBoxIsAutoMacAddressEnabled.Checked)
      {
        mpTextBoxMacAddress.Enabled = false;
      }
      else
      {
        mpTextBoxMacAddress.Enabled = true;
      }
    }

    private void mpCheckBoxIsWakeOnLanEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (mpCheckBoxIsWakeOnLanEnabled.Checked)
      {
        mpNumericTextBoxWOLTimeOut.Enabled = true;
        mpCheckBoxIsAutoMacAddressEnabled.Enabled = true;

        if (mpCheckBoxIsAutoMacAddressEnabled.Checked)
        {
          mpTextBoxMacAddress.Enabled = false;
        }
        else
        {
          mpTextBoxMacAddress.Enabled = true;
        }
      }
      else
      {
        mpNumericTextBoxWOLTimeOut.Enabled = false;
        mpCheckBoxIsAutoMacAddressEnabled.Enabled = false;
        mpTextBoxMacAddress.Enabled = false;
      }
    }

    private void mpButtonTestHostname_Click(object sender, EventArgs e)
    {
      // Save current cursor, display wait cursor and disable button
      Cursor currentCursor = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;
      mpButtonTestConnectioname.Enabled = false;

      // Save hostname from textbox
      string text = mpTextBoxHostname.Text;

      // If hostname is empty, try local hostname
      if (string.IsNullOrEmpty(text))
      {
        text = Dns.GetHostName();
        mpTextBoxHostname.Text = "Trying local host...";
      }
      else
        mpTextBoxHostname.Text = "Trying " + text + "...";
      mpTextBoxHostname.Refresh();

      // Restore hostname to textbox and verify it
      mpTextBoxHostname.Text = text;
      if (!VerifyHostname(mpTextBoxHostname.Text))
      {
        // No connection to tv server
        mpTextBoxHostname.BackColor = Color.Red;
        _verifiedHostname = string.Empty;

        // Reset cursor, enable button
        Cursor.Current = currentCursor;
        mpButtonTestConnectioname.Enabled = true;
      }
      else
      {
        // Connection to tv server successful
        mpTextBoxHostname.BackColor = Color.YellowGreen;
        _verifiedHostname = mpTextBoxHostname.Text;

        // Disable WOL for localhost
        try
        {
          if (Dns.GetHostEntry(_verifiedHostname).HostName == Dns.GetHostName())
            mpCheckBoxIsWakeOnLanEnabled.Checked = false;
        }
        catch { }

        // Reset cursor, enable button
        Cursor.Current = currentCursor;
        mpButtonTestConnectioname.Enabled = true;

        // Show success message
        MessageBox.Show("Connection to the TV server successful",
          "TV Client Settings", MessageBoxButtons.OK, MessageBoxIcon.None);
      }
    }

    private void mpTextBoxHostname_TextChanged(object sender, EventArgs e)
    {
      mpTextBoxHostname.BackColor = mpTextBoxMacAddress.BackColor;
    }

    #endregion
  }
}