#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.ComponentModel;
using System.Diagnostics;
using MediaPortal.Profile;
using MediaPortal.RedEyeIR;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class RemoteRedEye : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPCheckBox inputCheckBox;
    private MPGroupBox groupBox2;
    private MPLabel statusLabel;
    private MPLabel label1;
    private MPComboBox CommPortCombo;
    private IContainer components = null;
    private MPLabel label6;
    private MPComboBox CommandDelayCombo;
    private MPLabel label7;
    private MPTextBox testRedeyeChannelsTextBox;
    private MPLabel label2;
    private MPTextBox infoTextBox;
    private MPButton buttonIRDA;
    private MPButton buttonRC5;
    private MPButton buttonSKY;
    private MPGroupBox groupBox3;
    private MPButton cmdTest;


    public RemoteRedEye()
      : this("RedEye")
    {
    }

    public RemoteRedEye(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // 
      // Initialize the RedEye component
      //
      RedEye.Create(new RedEye.OnRemoteCommand(OnRemoteCommand));
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      RedEye.Instance.Close();
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        inputCheckBox.Checked = xmlreader.GetValueAsString("RedEye", "internal", "false") == "true";
        CommandDelayCombo.Text = xmlreader.GetValueAsString("RedEye", "delay", "300");
        CommPortCombo.Text = xmlreader.GetValueAsString("RedEye", "commport", "COM1:");
        this.LoadInfo();
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("RedEye", "internal", inputCheckBox.Checked ? "true" : "false");
        xmlwriter.SetValue("RedEye", "commport", CommPortCombo.Text);
        xmlwriter.SetValue("RedEye", "delay", CommandDelayCombo.Text);
      }
    }

    public void Close()
    {
      RedEye.Instance.Close();
    }

    private void LoadInfo()
    {
      string[] lines = {
                         "REDEYE SERIAL REMOTE for CABLE and SKY STB's",
                         "http://www.redremote.co.uk/serial",
                         "",
                         "N.B. Make sure the ComPort in use by serialUIR is not set to the ComPort",
                         "RedEye is using, you will get strange results even if serialUIR is not enabled for",
                         "MediaPortal and possibly signals sent to RedEye which will react with your STB",
                         "and cause confusion",
                         "",
                         "",
                         ">MediaPortal Configuration<",
                         "Check the 'Enable Redeye' Option",
                         "Choose the ComPort the unit is attached to ",
                         "(N.B. Test Button & Signal type Buttons only available if the Port selected is available)",
                         "Choose the signal type for the type of STB you have (See documentation on site..",
                         "You only need do this once as the unit will remember your choice) ",
                         "",
                         "In the Channel Setup for your TV simply put the channel number you would normally ",
                         "select with your STB remote in the text box for the External Channel.",
                         "You can test your configuration by typeing in a channel number and clicking 'Test'.",
                         "",
                         "N.B. If the external Channel number has not changed the Plugin will not attempt",
                         "to send an instruction to RedEye, this makes a smoother interface, please remember",
                         "this when you are testing the unit."
                       };
      infoTextBox.Lines = lines;
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cmdTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.testRedeyeChannelsTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonIRDA = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRC5 = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonSKY = new MediaPortal.UserInterface.Controls.MPButton();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.CommandDelayCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.CommPortCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.infoTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.statusLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cmdTest);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.testRedeyeChannelsTextBox);
      this.groupBox1.Controls.Add(this.buttonIRDA);
      this.groupBox1.Controls.Add(this.buttonRC5);
      this.groupBox1.Controls.Add(this.buttonSKY);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.CommandDelayCombo);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.CommPortCombo);
      this.groupBox1.Controls.Add(this.inputCheckBox);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 184);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General settings";
      // 
      // cmdTest
      // 
      this.cmdTest.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdTest.Location = new System.Drawing.Point(384, 152);
      this.cmdTest.Name = "cmdTest";
      this.cmdTest.Size = new System.Drawing.Size(72, 22);
      this.cmdTest.TabIndex = 11;
      this.cmdTest.Text = "Test";
      this.cmdTest.Click += new System.EventHandler(this.cmdTest_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 128);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(48, 16);
      this.label2.TabIndex = 9;
      this.label2.Text = "Channels:";
      // 
      // testRedeyeChannelsTextBox
      // 
      this.testRedeyeChannelsTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.testRedeyeChannelsTextBox.Location = new System.Drawing.Point(168, 124);
      this.testRedeyeChannelsTextBox.MaxLength = 3;
      this.testRedeyeChannelsTextBox.Name = "testRedeyeChannelsTextBox";
      this.testRedeyeChannelsTextBox.Size = new System.Drawing.Size(288, 20);
      this.testRedeyeChannelsTextBox.TabIndex = 10;
      this.testRedeyeChannelsTextBox.Text = "";
      // 
      // buttonIRDA
      // 
      this.buttonIRDA.Location = new System.Drawing.Point(167, 54);
      this.buttonIRDA.Name = "buttonIRDA";
      this.buttonIRDA.Size = new System.Drawing.Size(72, 16);
      this.buttonIRDA.TabIndex = 2;
      this.buttonIRDA.Text = "IRDA";
      this.buttonIRDA.Click += new System.EventHandler(this.buttonIRDA_Click);
      // 
      // buttonRC5
      // 
      this.buttonRC5.Location = new System.Drawing.Point(247, 54);
      this.buttonRC5.Name = "buttonRC5";
      this.buttonRC5.Size = new System.Drawing.Size(72, 16);
      this.buttonRC5.TabIndex = 3;
      this.buttonRC5.Text = "RC5";
      this.buttonRC5.Click += new System.EventHandler(this.buttonRC5_Click);
      // 
      // buttonSKY
      // 
      this.buttonSKY.Location = new System.Drawing.Point(327, 54);
      this.buttonSKY.Name = "buttonSKY";
      this.buttonSKY.Size = new System.Drawing.Size(72, 16);
      this.buttonSKY.TabIndex = 4;
      this.buttonSKY.Text = "SKY";
      this.buttonSKY.Click += new System.EventHandler(this.buttonSKY_Click);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 56);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(64, 16);
      this.label7.TabIndex = 1;
      this.label7.Text = "Signal Type:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 104);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(72, 16);
      this.label6.TabIndex = 7;
      this.label6.Text = "Delay (msec.):";
      // 
      // CommandDelayCombo
      // 
      this.CommandDelayCombo.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.CommandDelayCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.CommandDelayCombo.Items.AddRange(new object[]
                                              {
                                                "150",
                                                "200",
                                                "250",
                                                "300",
                                                "350",
                                                "400",
                                                "450",
                                                "500",
                                                "550",
                                                "600",
                                                "650",
                                                "700",
                                                "750",
                                                "800",
                                                "850",
                                                "900",
                                                "950",
                                                "1000",
                                                "1050",
                                                "1100",
                                                "1150",
                                                "1200"
                                              });
      this.CommandDelayCombo.Location = new System.Drawing.Point(168, 100);
      this.CommandDelayCombo.Name = "CommandDelayCombo";
      this.CommandDelayCombo.Size = new System.Drawing.Size(288, 21);
      this.CommandDelayCombo.TabIndex = 8;
      this.CommandDelayCombo.SelectedIndexChanged += new System.EventHandler(this.CommandDelayCombo_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 80);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(24, 16);
      this.label1.TabIndex = 5;
      this.label1.Text = "Port:";
      // 
      // CommPortCombo
      // 
      this.CommPortCombo.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.CommPortCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.CommPortCombo.Items.AddRange(new object[]
                                          {
                                            "COM1:",
                                            "COM2:",
                                            "COM3:",
                                            "COM4:",
                                            "COM5:",
                                            "COM6:",
                                            "COM7:",
                                            "COM8:"
                                          });
      this.CommPortCombo.Location = new System.Drawing.Point(168, 76);
      this.CommPortCombo.Name = "CommPortCombo";
      this.CommPortCombo.Size = new System.Drawing.Size(288, 21);
      this.CommPortCombo.TabIndex = 6;
      this.CommPortCombo.SelectedIndexChanged += new System.EventHandler(this.CommPortCombo_SelectedIndexChanged);
      // 
      // inputCheckBox
      // 
      this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
      this.inputCheckBox.Name = "inputCheckBox";
      this.inputCheckBox.Size = new System.Drawing.Size(128, 16);
      this.inputCheckBox.TabIndex = 0;
      this.inputCheckBox.Text = "Enable RedEye Serial";
      this.inputCheckBox.CheckedChanged += new System.EventHandler(this.inputCheckBox_CheckedChanged);
      // 
      // infoTextBox
      // 
      this.infoTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.infoTextBox.BackColor = System.Drawing.SystemColors.Control;
      this.infoTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.infoTextBox.Location = new System.Drawing.Point(16, 24);
      this.infoTextBox.Multiline = true;
      this.infoTextBox.Name = "infoTextBox";
      this.infoTextBox.ReadOnly = true;
      this.infoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.infoTextBox.Size = new System.Drawing.Size(440, 104);
      this.infoTextBox.TabIndex = 0;
      this.infoTextBox.Text = "";
      this.infoTextBox.WordWrap = false;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.statusLabel);
      this.groupBox2.Location = new System.Drawing.Point(0, 192);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 64);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Device Status";
      // 
      // statusLabel
      // 
      this.statusLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.statusLabel.Location = new System.Drawing.Point(16, 24);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(440, 32);
      this.statusLabel.TabIndex = 0;
      this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.statusLabel.Click += new System.EventHandler(this.statusLabel_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.infoTextBox);
      this.groupBox3.Location = new System.Drawing.Point(0, 264);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(472, 144);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Information";
      // 
      // RedEye
      // 
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "RedEye";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private void OnRemoteCommand(object command)
    {
      Debug.WriteLine("Remote Command = " + command.ToString());
    }


    private void SetReOpenStatus(bool available)
    {
      if (available)
      {
        statusLabel.Text = "Port " + CommPortCombo.Text + " available";
        buttonIRDA.Enabled = true;
        buttonRC5.Enabled = true;
        buttonSKY.Enabled = true;
        cmdTest.Enabled = true;
      }
      else
      {
        statusLabel.Text = "Error : Port " + CommPortCombo.Text + " unavailable !";
        buttonIRDA.Enabled = false;
        buttonRC5.Enabled = false;
        buttonSKY.Enabled = false;
        cmdTest.Enabled = false;
      }
    }

    private void CommPortCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetReOpenStatus(RedEye.Instance.SetPort(CommPortCombo.Text));
    }

    private void inputCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      RedEye.Instance.InternalCommandsActive = inputCheckBox.Checked;
    }

    private void buttonIRDA_Click(object sender, EventArgs e)
    {
      RedEye.Instance.SetIRDA();
    }

    private void buttonRC5_Click(object sender, EventArgs e)
    {
      RedEye.Instance.SetRC5();
    }

    private void buttonSKY_Click(object sender, EventArgs e)
    {
      RedEye.Instance.SetSKY();
    }

    private void cmdTest_Click(object sender, EventArgs e)
    {
      if (testRedeyeChannelsTextBox.Text.Length > 0)
      {
        RedEye.Instance.ChangeTunerChannel(testRedeyeChannelsTextBox.Text);
        testRedeyeChannelsTextBox.Text = "";
      }
    }

    private void CommandDelayCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      RedEye.Instance.CommandDelay = int.Parse(CommandDelayCombo.Text);
    }

    private void statusLabel_Click(object sender, EventArgs e)
    {
    }
  }
}