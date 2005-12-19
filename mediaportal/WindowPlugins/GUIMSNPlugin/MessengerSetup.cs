/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.GUI.MSN
{
  /// <summary>
  /// Summary description for MessengerSetup.
  /// </summary>
  public class MessengerSetup : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxEMail;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.CheckBox chkSignIn;
    private System.Windows.Forms.ComboBox InitialStatusBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckBox chkPopupWindow;
    private GroupBox groupBox1;
    private GroupBox groupBox2;
    private CheckBox cbUseProxyServer;
    private Label label4;
    private Label label7;
    private Label label6;
    private Label label5;
    private TextBox tbProxyPassword;
    private TextBox tbProxyUserName;
    private TextBox tbProxyHost;
    private ComboBox cbProxyType;
    private Label label8;
    private TextBox tbProxyPort;
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
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxEMail = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxPassword = new System.Windows.Forms.TextBox();
      this.buttonOK = new System.Windows.Forms.Button();
      this.chkSignIn = new System.Windows.Forms.CheckBox();
      this.InitialStatusBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.chkPopupWindow = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.cbProxyType = new System.Windows.Forms.ComboBox();
      this.cbUseProxyServer = new System.Windows.Forms.CheckBox();
      this.tbProxyPassword = new System.Windows.Forms.TextBox();
      this.tbProxyUserName = new System.Windows.Forms.TextBox();
      this.tbProxyPort = new System.Windows.Forms.TextBox();
      this.tbProxyHost = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "E-mail adres:";
      // 
      // textBoxEMail
      // 
      this.textBoxEMail.Location = new System.Drawing.Point(19, 38);
      this.textBoxEMail.Name = "textBoxEMail";
      this.textBoxEMail.Size = new System.Drawing.Size(232, 20);
      this.textBoxEMail.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(6, 67);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Password:";
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.Location = new System.Drawing.Point(19, 86);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(232, 20);
      this.textBoxPassword.TabIndex = 2;
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(537, 266);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(56, 23);
      this.buttonOK.TabIndex = 4;
      this.buttonOK.Text = "Ok";
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // chkSignIn
      // 
      this.chkSignIn.Location = new System.Drawing.Point(9, 169);
      this.chkSignIn.Name = "chkSignIn";
      this.chkSignIn.Size = new System.Drawing.Size(168, 24);
      this.chkSignIn.TabIndex = 3;
      this.chkSignIn.Text = "Auto sign-in";
      // 
      // InitialStatusBox
      // 
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
      this.label3.Location = new System.Drawing.Point(6, 113);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(100, 16);
      this.label3.TabIndex = 7;
      this.label3.Text = "Initial status:";
      // 
      // chkPopupWindow
      // 
      this.chkPopupWindow.Location = new System.Drawing.Point(9, 190);
      this.chkPopupWindow.Name = "chkPopupWindow";
      this.chkPopupWindow.Size = new System.Drawing.Size(256, 24);
      this.chkPopupWindow.TabIndex = 8;
      this.chkPopupWindow.Text = "Popup chat window while watching TV/Video";
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
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(268, 246);
      this.groupBox1.TabIndex = 9;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "MSN Details:";
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
      this.groupBox2.Location = new System.Drawing.Point(286, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(315, 248);
      this.groupBox2.TabIndex = 10;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Proxy connection settings";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(8, 189);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(91, 13);
      this.label7.TabIndex = 1;
      this.label7.Text = "Proxy server type:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(8, 140);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(84, 13);
      this.label6.TabIndex = 1;
      this.label6.Text = "Proxy password:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(7, 93);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(85, 13);
      this.label5.TabIndex = 1;
      this.label5.Text = "Proxy username:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(182, 45);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(28, 13);
      this.label8.TabIndex = 1;
      this.label8.Text = "port:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(7, 45);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(70, 13);
      this.label4.TabIndex = 1;
      this.label4.Text = "Proxy Server:";
      // 
      // cbProxyType
      // 
      this.cbProxyType.Items.AddRange(new object[] {
            "Socks 4",
            "Socks 5"});
      this.cbProxyType.Location = new System.Drawing.Point(22, 205);
      this.cbProxyType.Name = "cbProxyType";
      this.cbProxyType.Size = new System.Drawing.Size(232, 21);
      this.cbProxyType.TabIndex = 6;
      this.cbProxyType.Text = "Socks 5";
      // 
      // cbUseProxyServer
      // 
      this.cbUseProxyServer.AutoSize = true;
      this.cbUseProxyServer.Location = new System.Drawing.Point(10, 20);
      this.cbUseProxyServer.Name = "cbUseProxyServer";
      this.cbUseProxyServer.Size = new System.Drawing.Size(105, 17);
      this.cbUseProxyServer.TabIndex = 0;
      this.cbUseProxyServer.Text = "Use proxy server";
      this.cbUseProxyServer.UseVisualStyleBackColor = true;
      // 
      // tbProxyPassword
      // 
      this.tbProxyPassword.Location = new System.Drawing.Point(22, 158);
      this.tbProxyPassword.Name = "tbProxyPassword";
      this.tbProxyPassword.PasswordChar = '*';
      this.tbProxyPassword.Size = new System.Drawing.Size(232, 20);
      this.tbProxyPassword.TabIndex = 2;
      // 
      // tbProxyUserName
      // 
      this.tbProxyUserName.Location = new System.Drawing.Point(21, 113);
      this.tbProxyUserName.Name = "tbProxyUserName";
      this.tbProxyUserName.Size = new System.Drawing.Size(232, 20);
      this.tbProxyUserName.TabIndex = 2;
      // 
      // tbProxyPort
      // 
      this.tbProxyPort.Location = new System.Drawing.Point(185, 61);
      this.tbProxyPort.Name = "tbProxyPort";
      this.tbProxyPort.Size = new System.Drawing.Size(67, 20);
      this.tbProxyPort.TabIndex = 2;
      // 
      // tbProxyHost
      // 
      this.tbProxyHost.Location = new System.Drawing.Point(21, 61);
      this.tbProxyHost.Name = "tbProxyHost";
      this.tbProxyHost.Size = new System.Drawing.Size(144, 20);
      this.tbProxyHost.TabIndex = 2;
      // 
      // MessengerSetup
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(636, 301);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.groupBox1);
      this.Name = "MessengerSetup";
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
      using (MediaPortal.Profile.Xml xmlWriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
