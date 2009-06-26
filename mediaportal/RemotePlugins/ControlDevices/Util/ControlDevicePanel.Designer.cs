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

namespace MediaPortal.ControlDevices
{
  partial class ControlDevicePanel
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.grpStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.grpSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ctrlVerbose = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ctrlOutput = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.ctrlInput = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.grpDevice = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ctrlDevicePanel = new System.Windows.Forms.Panel();
      this.ctrlStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ctrlDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.ctrlAdvanced = new MediaPortal.UserInterface.Controls.MPButton();
      this.ctrlMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.grpStatus.SuspendLayout();
      this.grpSettings.SuspendLayout();
      this.grpDevice.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpStatus
      // 
      this.grpStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.grpStatus.Controls.Add(this.ctrlStatus);
      this.grpStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpStatus.Location = new System.Drawing.Point(7, 79);
      this.grpStatus.Name = "grpStatus";
      this.grpStatus.Size = new System.Drawing.Size(216, 165);
      this.grpStatus.TabIndex = 5;
      this.grpStatus.TabStop = false;
      this.grpStatus.Text = "Status";
      // 
      // grpSettings
      // 
      this.grpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpSettings.Controls.Add(this.ctrlMapping);
      this.grpSettings.Controls.Add(this.ctrlVerbose);
      this.grpSettings.Controls.Add(this.ctrlOutput);
      this.grpSettings.Controls.Add(this.ctrlInput);
      this.grpSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpSettings.Location = new System.Drawing.Point(7, 6);
      this.grpSettings.Name = "grpSettings";
      this.grpSettings.Size = new System.Drawing.Size(437, 67);
      this.grpSettings.TabIndex = 3;
      this.grpSettings.TabStop = false;
      this.grpSettings.Text = "Settings";
      // 
      // ctrlVerbose
      // 
      this.ctrlVerbose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.ctrlVerbose.AutoSize = true;
      this.ctrlVerbose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ctrlVerbose.Location = new System.Drawing.Point(188, 19);
      this.ctrlVerbose.Name = "ctrlVerbose";
      this.ctrlVerbose.Size = new System.Drawing.Size(107, 17);
      this.ctrlVerbose.TabIndex = 2;
      this.ctrlVerbose.Text = "Extensive logging";
      this.ctrlVerbose.UseVisualStyleBackColor = true;
      this.ctrlVerbose.CheckedChanged += new System.EventHandler(this.ctrlVerbose_CheckedChanged);
      // 
      // ctrlOutput
      // 
      this.ctrlOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.ctrlOutput.AutoSize = true;
      this.ctrlOutput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ctrlOutput.Location = new System.Drawing.Point(6, 42);
      this.ctrlOutput.Name = "ctrlOutput";
      this.ctrlOutput.Size = new System.Drawing.Size(143, 17);
      this.ctrlOutput.TabIndex = 1;
      this.ctrlOutput.Text = "Enable as Output Device";
      this.ctrlOutput.UseVisualStyleBackColor = true;
      this.ctrlOutput.CheckedChanged += new System.EventHandler(this.ctrlOutpu_CheckedChanged);
      // 
      // ctrlInput
      // 
      this.ctrlInput.AutoSize = true;
      this.ctrlInput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.ctrlInput.Location = new System.Drawing.Point(6, 19);
      this.ctrlInput.Name = "ctrlInput";
      this.ctrlInput.Size = new System.Drawing.Size(135, 17);
      this.ctrlInput.TabIndex = 0;
      this.ctrlInput.Text = "Enable as Input Device";
      this.ctrlInput.UseVisualStyleBackColor = true;
      this.ctrlInput.CheckedChanged += new System.EventHandler(this.ctrlInput_CheckedChanged);
      // 
      // grpDevice
      // 
      this.grpDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpDevice.Controls.Add(this.ctrlAdvanced);
      this.grpDevice.Controls.Add(this.ctrlDefaults);
      this.grpDevice.Controls.Add(this.ctrlDevicePanel);
      this.grpDevice.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpDevice.Location = new System.Drawing.Point(229, 79);
      this.grpDevice.Name = "grpDevice";
      this.grpDevice.Size = new System.Drawing.Size(215, 165);
      this.grpDevice.TabIndex = 6;
      this.grpDevice.TabStop = false;
      this.grpDevice.Text = "Device";
      // 
      // ctrlDevicePanel
      // 
      this.ctrlDevicePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ctrlDevicePanel.Location = new System.Drawing.Point(9, 20);
      this.ctrlDevicePanel.Name = "ctrlDevicePanel";
      this.ctrlDevicePanel.Size = new System.Drawing.Size(200, 109);
      this.ctrlDevicePanel.TabIndex = 2;
      // 
      // ctrlStatus
      // 
      this.ctrlStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ctrlStatus.Location = new System.Drawing.Point(7, 20);
      this.ctrlStatus.Name = "ctrlStatus";
      this.ctrlStatus.Size = new System.Drawing.Size(203, 139);
      this.ctrlStatus.TabIndex = 0;
      // 
      // ctrlDefaults
      // 
      this.ctrlDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.ctrlDefaults.Location = new System.Drawing.Point(129, 135);
      this.ctrlDefaults.Name = "ctrlDefaults";
      this.ctrlDefaults.Size = new System.Drawing.Size(80, 23);
      this.ctrlDefaults.TabIndex = 3;
      this.ctrlDefaults.Text = "Defaults";
      this.ctrlDefaults.UseVisualStyleBackColor = true;
      this.ctrlDefaults.Click += new System.EventHandler(this.ctrlDefaults_Click);
      // 
      // ctrlAdvanced
      // 
      this.ctrlAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.ctrlAdvanced.Location = new System.Drawing.Point(43, 135);
      this.ctrlAdvanced.Name = "ctrlAdvanced";
      this.ctrlAdvanced.Size = new System.Drawing.Size(80, 23);
      this.ctrlAdvanced.TabIndex = 4;
      this.ctrlAdvanced.Text = "Advanced";
      this.ctrlAdvanced.UseVisualStyleBackColor = true;
      this.ctrlAdvanced.Click += new System.EventHandler(this.ctrlAdvanced_Click);
      // 
      // ctrlMapping
      // 
      this.ctrlMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ctrlMapping.Location = new System.Drawing.Point(340, 25);
      this.ctrlMapping.Name = "ctrlMapping";
      this.ctrlMapping.Size = new System.Drawing.Size(80, 23);
      this.ctrlMapping.TabIndex = 3;
      this.ctrlMapping.Text = "Mapping";
      this.ctrlMapping.UseVisualStyleBackColor = true;
      this.ctrlMapping.Click += new System.EventHandler(this.ctrlMapping_Click);
      // 
      // ctrlDeviceTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.Controls.Add(this.grpDevice);
      this.Controls.Add(this.grpStatus);
      this.Controls.Add(this.grpSettings);
      this.Name = "ctrlDeviceTab";
      this.Padding = new System.Windows.Forms.Padding(3);
      this.Size = new System.Drawing.Size(450, 250);
      this.grpStatus.ResumeLayout(false);
      this.grpSettings.ResumeLayout(false);
      this.grpSettings.PerformLayout();
      this.grpDevice.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox grpStatus;
    private MediaPortal.UserInterface.Controls.MPGroupBox grpSettings;
    private MediaPortal.UserInterface.Controls.MPCheckBox ctrlVerbose;
    private MediaPortal.UserInterface.Controls.MPCheckBox ctrlOutput;
    private MediaPortal.UserInterface.Controls.MPCheckBox ctrlInput;
    private MediaPortal.UserInterface.Controls.MPGroupBox grpDevice;
    private System.Windows.Forms.Panel ctrlDevicePanel;
    private MediaPortal.UserInterface.Controls.MPLabel ctrlStatus;
    private MediaPortal.UserInterface.Controls.MPButton ctrlMapping;
    private MediaPortal.UserInterface.Controls.MPButton ctrlAdvanced;
    private MediaPortal.UserInterface.Controls.MPButton ctrlDefaults;
  }
}
