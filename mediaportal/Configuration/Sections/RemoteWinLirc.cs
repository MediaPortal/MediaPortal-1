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
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class RemoteWinLirc : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPCheckBox inputCheckBox;
    private MPGroupBox groupBox2;
    private MPLabel PathToWinlircLabel;
    private MPTextBox pathToWinlircTextBox;
    private MPButton browsePathToWinlircButton;
    private MPTextBox infoTextBox;
    private OpenFileDialog openFileDialog1;
    private MPTextBox IRDelayTextBox;
    private MPLabel IRDelayLabel;
    private IContainer components = null;

    public RemoteWinLirc()
      : this("WINLIRC")
    {
    }

    public RemoteWinLirc(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // 
      // Initialize the WINLIRC component
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

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        inputCheckBox.Checked = xmlreader.GetValueAsBool("WINLIRC", "enabled", false);
        pathToWinlircTextBox.Text = xmlreader.GetValueAsString("WINLIRC", "winlircpath", "");
        IRDelayTextBox.Text = xmlreader.GetValueAsString("WINLIRC", "delay", "300");
        //useMultipleCheckBox.Checked = xmlreader.GetValueAsString("WINLIRC", "use_multiple_remotes", "true") == "true";
        //remoteNameTextBox.Text = xmlreader.GetValueAsString("WINLIRC", "remote", "") ;
        //repeatValTextBox.Text = xmlreader.GetValueAsString("WINLIRC", "repeat", "0");
        //enterCheckBox.Checked = xmlreader.GetValueAsString("WINLIRC", "needs_enter", "false") == "true";
      }
      UpdateForm();
      LoadInfo();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("WINLIRC", "enabled", inputCheckBox.Checked);
        xmlwriter.SetValue("WINLIRC", "winlircpath", pathToWinlircTextBox.Text);
        if (IsInteger(IRDelayTextBox.Text) == false)
        {
          IRDelayTextBox.Text = "300";
        }
        xmlwriter.SetValue("WINLIRC", "delay", IRDelayTextBox.Text);
        //xmlwriter.SetValue("WINLIRC", "use_multiple_remotes", useMultipleCheckBox.Checked ? "true" : "false");
        //xmlwriter.SetValue("WINLIRC", "remote", remoteNameTextBox.Text);
        //xmlwriter.SetValue("WINLIRC", "repeat", repeatValTextBox.Text);			
        //xmlwriter.SetValue("WINLIRC", "needs_enter", enterCheckBox.Checked ? "true" : "false");			
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.IRDelayTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.IRDelayLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.browsePathToWinlircButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.pathToWinlircTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.PathToWinlircLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.infoTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.IRDelayTextBox);
      this.groupBox1.Controls.Add(this.IRDelayLabel);
      this.groupBox1.Controls.Add(this.browsePathToWinlircButton);
      this.groupBox1.Controls.Add(this.pathToWinlircTextBox);
      this.groupBox1.Controls.Add(this.PathToWinlircLabel);
      this.groupBox1.Controls.Add(this.inputCheckBox);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 112);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General settings";
      // 
      // IRDelayTextBox
      // 
      this.IRDelayTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.IRDelayTextBox.Location = new System.Drawing.Point(168, 76);
      this.IRDelayTextBox.Name = "IRDelayTextBox";
      this.IRDelayTextBox.Size = new System.Drawing.Size(288, 20);
      this.IRDelayTextBox.TabIndex = 5;
      this.IRDelayTextBox.Text = "300";
      // 
      // IRDelayLabel
      // 
      this.IRDelayLabel.Location = new System.Drawing.Point(16, 80);
      this.IRDelayLabel.Name = "IRDelayLabel";
      this.IRDelayLabel.Size = new System.Drawing.Size(88, 16);
      this.IRDelayLabel.TabIndex = 4;
      this.IRDelayLabel.Text = "IR delay (msec):";
      // 
      // browsePathToWinlircButton
      // 
      this.browsePathToWinlircButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browsePathToWinlircButton.Location = new System.Drawing.Point(384, 51);
      this.browsePathToWinlircButton.Name = "browsePathToWinlircButton";
      this.browsePathToWinlircButton.Size = new System.Drawing.Size(72, 22);
      this.browsePathToWinlircButton.TabIndex = 3;
      this.browsePathToWinlircButton.Text = "Browse";
      this.browsePathToWinlircButton.Click += new System.EventHandler(this.browsePathToWinlircButton_Click);
      // 
      // pathToWinlircTextBox
      // 
      this.pathToWinlircTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pathToWinlircTextBox.Location = new System.Drawing.Point(168, 52);
      this.pathToWinlircTextBox.Name = "pathToWinlircTextBox";
      this.pathToWinlircTextBox.Size = new System.Drawing.Size(208, 20);
      this.pathToWinlircTextBox.TabIndex = 2;
      this.pathToWinlircTextBox.Text = "";
      // 
      // PathToWinlircLabel
      // 
      this.PathToWinlircLabel.Location = new System.Drawing.Point(16, 56);
      this.PathToWinlircLabel.Name = "PathToWinlircLabel";
      this.PathToWinlircLabel.Size = new System.Drawing.Size(96, 16);
      this.PathToWinlircLabel.TabIndex = 1;
      this.PathToWinlircLabel.Text = "Path to WinLIRC:";
      // 
      // inputCheckBox
      // 
      this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
      this.inputCheckBox.Name = "inputCheckBox";
      this.inputCheckBox.Size = new System.Drawing.Size(248, 16);
      this.inputCheckBox.TabIndex = 0;
      this.inputCheckBox.Text = "Enable WINLIRC for output to external devices";
      this.inputCheckBox.CheckedChanged += new System.EventHandler(this.inputCheckBox_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.infoTextBox);
      this.groupBox2.Location = new System.Drawing.Point(0, 120);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 288);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Information";
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
      this.infoTextBox.Size = new System.Drawing.Size(440, 248);
      this.infoTextBox.TabIndex = 0;
      this.infoTextBox.Text = "";
      this.infoTextBox.WordWrap = false;
      // 
      // WINLIRC
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "WINLIRC";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.WINLIRC_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private void inputCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      UpdateForm();
    }

    private void UpdateForm()
    {
      PathToWinlircLabel.Enabled = inputCheckBox.Checked;
      pathToWinlircTextBox.Enabled = inputCheckBox.Checked;
      browsePathToWinlircButton.Enabled = inputCheckBox.Checked;
      IRDelayTextBox.Enabled = inputCheckBox.Checked;
      IRDelayLabel.Enabled = inputCheckBox.Checked;
      //remoteNameLabel.Enabled  = inputCheckBox.Checked;
      //remoteNameTextBox.Enabled  = inputCheckBox.Checked;
    }

    private void WINLIRC_Load(object sender, EventArgs e)
    {
    }

    private void browsePathToWinlircButton_Click(object sender, EventArgs e)
    {
      using (openFileDialog1 = new OpenFileDialog())
      {
        openFileDialog1.FileName = pathToWinlircTextBox.Text;
        openFileDialog1.CheckFileExists = true;
        openFileDialog1.RestoreDirectory = true;
        openFileDialog1.Filter = "exe files (*.exe)|*.exe";
        openFileDialog1.FilterIndex = 0;
        openFileDialog1.Title = "Select WinLirc Executable";

        DialogResult dialogResult = openFileDialog1.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
          pathToWinlircTextBox.Text = openFileDialog1.FileName;
        }
      }
    }

    private void LoadInfo()
    {
      string[] lines = {
                         "::Winlirc::\n",
                         "> Winlirc must be installed on your PC",
                         "> You should set-up Winlirc with the remote(s) you require.",
                         "> To have Winlirc start when MP does, set the Path to WinLIRC.",
                         "> You should use short names for remotes (like CABLE or AMP).",
                         "> All remotes should be in 1 config file.",
                         "",
                         "::MediaPortal::",
                         "Set-up your external channels in this format...",
                         "  Remote1:Repeat:Code1[,Code2[,Code...]]|Remote2:Repeat:Code1[,Code2[,Code...]]",
                         "Examples...",
                         "  PACE4000:0:1,0,2,OK",
                         "  PACE4000:0:1,0,2,OK|AMP:2:PowerOn",
                         "  X10:8:dim_lounge|AMP:2:PowerOn|HIFI:0:mode_radio,3",
                         "",
                         "::ADDITIONAL::",
                         "> Check out the remote configs @ http://lirc.sourceforge.net/remotes/"
                       };
      infoTextBox.Lines = lines;
    }

    private bool IsInteger(object Expression)
    {
      bool isNum;
      double retNum;
      isNum = Double.TryParse(Convert.ToString(Expression), NumberStyles.Integer, NumberFormatInfo.InvariantInfo,
                              out retNum);
      return isNum;
    }
  }
}