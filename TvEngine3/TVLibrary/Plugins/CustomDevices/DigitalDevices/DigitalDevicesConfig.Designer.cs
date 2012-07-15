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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  partial class DigitalDevicesConfig
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

    /// <summary> 
    /// The user interface for this plugin is custom-designed so that the number and contents of fields
    /// can be adjusted automatically based on the number of Digital Devices CI slots detected in the
    /// host system. Nothing will show in the designer.
    /// </summary>
    private void InitializeComponent()
    {
      int groupHeight = 73;
      int groupPadding = 10;
      int componentCount = 6;

      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DigitalDevicesConfig));
      this.SuspendLayout();
      for (int i = 0; i < _ciSlots.Count; i++)
      {
        // Groupbox wrapper for each CI slot.
        GroupBox gb = new GroupBox();
        gb.SuspendLayout();
        gb.Location = new Point(3, 3 + (i * (groupHeight + groupPadding)));
        gb.Name = "groupBox" + i;
        gb.Size = new Size(446, groupHeight);
        gb.TabIndex = (i * componentCount) + 1;
        gb.TabStop = false;
        gb.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        gb.Text = "CI Slot " + (i + 1) + " - " + _ciSlots[i].DeviceName;

        // CAM name.
        Label camNameLabel = new Label();
        camNameLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        camNameLabel.Location = new Point(6, 22);
        camNameLabel.Name = "camName" + i;
        camNameLabel.Size = new Size(412, 20);
        camNameLabel.TabIndex = (i * componentCount) + 2;
        camNameLabel.Text = "CAM Name: " + _ciSlots[i].CamRootMenuTitle;
        gb.Controls.Add(camNameLabel);

        // Decrypt limit label.
        Label decryptLimitLabel = new Label();
        decryptLimitLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        decryptLimitLabel.Location = new Point(6, 46);
        decryptLimitLabel.Name = "decryptLimitLabel" + i;
        decryptLimitLabel.Size = new Size(71, 13);
        decryptLimitLabel.TabIndex = (i * componentCount) + 3;
        decryptLimitLabel.Text = "Decrypt Limit:";
        gb.Controls.Add(decryptLimitLabel);

        // Decrypt limit control.
        NumericUpDown decryptLimitControl = new NumericUpDown();
        ((ISupportInitialize)decryptLimitControl).BeginInit();
        decryptLimitControl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        decryptLimitControl.Location = new Point(83, 43);
        decryptLimitControl.Name = "decryptLimitControl" + i;
        decryptLimitControl.Size = new Size(44, 20);
        decryptLimitControl.TabIndex = (i * componentCount) + 4;
        decryptLimitControl.TextAlign = HorizontalAlignment.Center;
        ((ISupportInitialize)decryptLimitControl).EndInit();
        gb.Controls.Add(decryptLimitControl);
        _decryptLimits[i] = decryptLimitControl;

        // Provider list label.
        Label providerLabel = new Label();
        providerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        providerLabel.Location = new Point(150, 46);
        providerLabel.Name = "providerListLabel" + i;
        providerLabel.Size = new Size(60, 13);
        providerLabel.TabIndex = (i * componentCount) + 5;
        providerLabel.Text = "Provider(s):";
        gb.Controls.Add(providerLabel);

        // Provider list control.
        MPTextBox providerListControl = new MPTextBox();
        providerListControl.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        providerListControl.Location = new Point(216, 43);
        providerListControl.Name = "providerListControl" + i;
        providerListControl.Size = new Size(212, 20);
        providerListControl.TabIndex = (i * componentCount) + 6;
        gb.Controls.Add(providerListControl);
        _providerLists[i] = providerListControl;

        gb.ResumeLayout(false);
        gb.PerformLayout();
        this.Controls.Add(gb);
      }

      if (_ciSlots.Count == 0)
      {
        Label noSlotsLabel = new Label();
        noSlotsLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        noSlotsLabel.Location = new Point(6, 5);
        noSlotsLabel.Name = "noSlotsLabel";
        noSlotsLabel.Size = new Size(412, 20);
        noSlotsLabel.TabIndex = 1;
        noSlotsLabel.Text = "No Digital Devices CI slots detected...";
        this.Controls.Add(noSlotsLabel);
      }

      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Name = "DigitalDevicesConfig";
      this.Size = new System.Drawing.Size(460, 380);
      this.ResumeLayout(false);
    }
  }
}
