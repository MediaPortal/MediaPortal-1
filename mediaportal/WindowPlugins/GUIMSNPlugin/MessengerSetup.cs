#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Util;

namespace MediaPortal.GUI.MSN
{
  /// <summary>
  /// Summary description for MessengerSetup.
  /// </summary>
  public class MessengerSetup : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxEMail;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPassword;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkSignIn;
    private MediaPortal.UserInterface.Controls.MPComboBox InitialStatusBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkPopupWindow;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbUseProxyServer;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPTextBox tbProxyPassword;
    private MediaPortal.UserInterface.Controls.MPTextBox tbProxyUserName;
    private MediaPortal.UserInterface.Controls.MPTextBox tbProxyHost;
    private MediaPortal.UserInterface.Controls.MPComboBox cbProxyType;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPTextBox tbProxyPort;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MessengerSetup()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxEMail = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.chkSignIn = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.InitialStatusBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkPopupWindow = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbProxyType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbUseProxyServer = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbProxyPassword = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbProxyUserName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbProxyPort = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbProxyHost = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Email address:";
      // 
      // textBoxEMail
      // 
      this.textBoxEMail.BorderColor = System.Drawing.Color.Empty;
      this.textBoxEMail.Location = new System.Drawing.Point(19, 38);
      this.textBoxEMail.Name = "textBoxEMail";
      this.textBoxEMail.Size = new System.Drawing.Size(232, 20);
      this.textBoxEMail.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 67);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Password:";
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPassword.Location = new System.Drawing.Point(19, 86);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(232, 20);
      this.textBoxPassword.TabIndex = 2;
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(471, 275);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(84, 26);
      this.buttonOK.TabIndex = 4;
      this.buttonOK.Text = "Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // chkSignIn
      // 
      this.chkSignIn.AutoSize = true;
      this.chkSignIn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkSignIn.Location = new System.Drawing.Point(19, 167);
      this.chkSignIn.Name = "chkSignIn";
      this.chkSignIn.Size = new System.Drawing.Size(79, 17);
      this.chkSignIn.TabIndex = 3;
      this.chkSignIn.Text = "Auto sign-in";
      this.chkSignIn.UseVisualStyleBackColor = true;
      // 
      // InitialStatusBox
      // 
      this.InitialStatusBox.BorderColor = System.Drawing.Color.Empty;
      this.InitialStatusBox.Items.AddRange(new object[] {
            "Online",
            "Away",
            "Busy",
            "Be Right Back",
            "Hidden"});
      this.InitialStatusBox.Location = new System.Drawing.Point(19, 132);
      this.InitialStatusBox.Name = "InitialStatusBox";
      this.InitialStatusBox.Size = new System.Drawing.Size(232, 21);
      this.InitialStatusBox.TabIndex = 6;
      this.InitialStatusBox.Text = "Online";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 113);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(100, 16);
      this.label3.TabIndex = 7;
      this.label3.Text = "Initial status:";
      // 
      // chkPopupWindow
      // 
      this.chkPopupWindow.AutoSize = true;
      this.chkPopupWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkPopupWindow.Location = new System.Drawing.Point(19, 190);
      this.chkPopupWindow.Name = "chkPopupWindow";
      this.chkPopupWindow.Size = new System.Drawing.Size(240, 17);
      this.chkPopupWindow.TabIndex = 8;
      this.chkPopupWindow.Text = "Popup chat window while watching TV/Video";
      this.chkPopupWindow.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.chkPopupWindow);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.chkSignIn);
      this.groupBox1.Controls.Add(this.textBoxEMail);
      this.groupBox1.Controls.Add(this.InitialStatusBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.textBoxPassword);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(268, 246);
      this.groupBox1.TabIndex = 9;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "MSN details:";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.cbProxyType);
      this.groupBox2.Controls.Add(this.cbUseProxyServer);
      this.groupBox2.Controls.Add(this.tbProxyPassword);
      this.groupBox2.Controls.Add(this.tbProxyUserName);
      this.groupBox2.Controls.Add(this.tbProxyPort);
      this.groupBox2.Controls.Add(this.tbProxyHost);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(286, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(269, 246);
      this.groupBox2.TabIndex = 10;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Proxy connection settings";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(19, 166);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(91, 13);
      this.label7.TabIndex = 1;
      this.label7.Text = "Proxy server type:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(19, 113);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(84, 13);
      this.label6.TabIndex = 1;
      this.label6.Text = "Proxy password:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(19, 67);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(85, 13);
      this.label5.TabIndex = 1;
      this.label5.Text = "Proxy username:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(182, 22);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(29, 13);
      this.label8.TabIndex = 1;
      this.label8.Text = "Port:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(17, 22);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(68, 13);
      this.label4.TabIndex = 1;
      this.label4.Text = "Proxy server:";
      // 
      // cbProxyType
      // 
      this.cbProxyType.BorderColor = System.Drawing.Color.Empty;
      this.cbProxyType.Items.AddRange(new object[] {
            "Socks 4",
            "Socks 5"});
      this.cbProxyType.Location = new System.Drawing.Point(21, 182);
      this.cbProxyType.Name = "cbProxyType";
      this.cbProxyType.Size = new System.Drawing.Size(232, 21);
      this.cbProxyType.TabIndex = 6;
      this.cbProxyType.Text = "Socks 5";
      // 
      // cbUseProxyServer
      // 
      this.cbUseProxyServer.AutoSize = true;
      this.cbUseProxyServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbUseProxyServer.Location = new System.Drawing.Point(22, 216);
      this.cbUseProxyServer.Name = "cbUseProxyServer";
      this.cbUseProxyServer.Size = new System.Drawing.Size(103, 17);
      this.cbUseProxyServer.TabIndex = 0;
      this.cbUseProxyServer.Text = "Use proxy server";
      this.cbUseProxyServer.UseVisualStyleBackColor = true;
      // 
      // tbProxyPassword
      // 
      this.tbProxyPassword.BorderColor = System.Drawing.Color.Empty;
      this.tbProxyPassword.Location = new System.Drawing.Point(21, 132);
      this.tbProxyPassword.Name = "tbProxyPassword";
      this.tbProxyPassword.PasswordChar = '*';
      this.tbProxyPassword.Size = new System.Drawing.Size(232, 20);
      this.tbProxyPassword.TabIndex = 2;
      // 
      // tbProxyUserName
      // 
      this.tbProxyUserName.BorderColor = System.Drawing.Color.Empty;
      this.tbProxyUserName.Location = new System.Drawing.Point(20, 86);
      this.tbProxyUserName.Name = "tbProxyUserName";
      this.tbProxyUserName.Size = new System.Drawing.Size(232, 20);
      this.tbProxyUserName.TabIndex = 2;
      // 
      // tbProxyPort
      // 
      this.tbProxyPort.BorderColor = System.Drawing.Color.Empty;
      this.tbProxyPort.Location = new System.Drawing.Point(185, 38);
      this.tbProxyPort.Name = "tbProxyPort";
      this.tbProxyPort.Size = new System.Drawing.Size(67, 20);
      this.tbProxyPort.TabIndex = 2;
      // 
      // tbProxyHost
      // 
      this.tbProxyHost.BorderColor = System.Drawing.Color.Empty;
      this.tbProxyHost.Location = new System.Drawing.Point(20, 38);
      this.tbProxyHost.Name = "tbProxyHost";
      this.tbProxyHost.Size = new System.Drawing.Size(144, 20);
      this.tbProxyHost.TabIndex = 2;
      // 
      // MessengerSetup
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(577, 317);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "MessengerSetup";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MessengerSetup";
      this.Load += new System.EventHandler(this.MessengerSetup_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        xmlWriter.SetValue("MSNmessenger", "email", textBoxEMail.Text);
        xmlWriter.SetValue("MSNmessenger", "password", textBoxPassword.Text);
        xmlWriter.SetValue("MSNmessenger", "initialstatus", InitialStatusBox.Text);

        int iSignIn = 0;
        if (chkSignIn.Checked) iSignIn = 1;
        xmlWriter.SetValue("MSNmessenger", "autosignin", iSignIn.ToString());

        int iPopup = 0;
        if (chkPopupWindow.Checked) iPopup = 1;
        xmlWriter.SetValue("MSNmessenger", "popupwindow", iPopup.ToString());

        xmlWriter.SetValueAsBool("MSNmessenger", "useproxy", cbUseProxyServer.Checked);
        xmlWriter.SetValue("MSNmessenger", "proxyhost", tbProxyHost.Text);
        xmlWriter.SetValue("MSNmessenger", "proxyport", tbProxyPort.Text);
        xmlWriter.SetValue("MSNmessenger", "proxyusername", tbProxyUserName.Text);
        xmlWriter.SetValue("MSNmessenger", "proxypassword", tbProxyPassword.Text);
        xmlWriter.SetValue("MSNmessenger", "proxytype", cbProxyType.SelectedIndex.ToString());

      }
      this.Close();
    }

    private void MessengerSetup_Load(object sender, System.EventArgs e)
    {

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        textBoxEMail.Text = xmlreader.GetValueAsString("MSNmessenger", "email", "");
        textBoxPassword.Text = xmlreader.GetValueAsString("MSNmessenger", "password", "");
        InitialStatusBox.Text = xmlreader.GetValueAsString("MSNmessenger", "initialstatus", "Online");

        chkSignIn.Checked = false;
        if (xmlreader.GetValueAsInt("MSNmessenger", "autosignin", 0) == 1) chkSignIn.Checked = true;
        chkPopupWindow.Checked = false;
        if (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1) chkPopupWindow.Checked = true;


        cbUseProxyServer.Checked = xmlreader.GetValueAsBool("MSNmessenger", "useproxy", false);
        tbProxyHost.Text = xmlreader.GetValueAsString("MSNmessenger", "proxyhost", "");
        tbProxyPort.Text = xmlreader.GetValueAsInt("MSNmessenger", "proxyport", 8080).ToString();
        tbProxyUserName.Text = xmlreader.GetValueAsString("MSNmessenger", "proxyusername", "");
        tbProxyPassword.Text = xmlreader.GetValueAsString("MSNmessenger", "proxypassword", "");
        cbProxyType.SelectedIndex = xmlreader.GetValueAsInt("MSNmessenger", "proxytype", 1);
      }
    }

    private void label3_Click(object sender, System.EventArgs e)
    {

    }
  }
}
