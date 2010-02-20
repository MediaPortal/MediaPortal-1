#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Profile;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditRadioStationForm.
  /// </summary>
  public class MpcHcSubsForm : MPConfigForm
  {
    private MPGroupBox groupBox1;
    private MPButton buttonClose;
    private MPButton buttonOK;
    private MPLabel label1;
    private MPLabel label3;
    private MPComboBox textureComboBox;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    private MPNumericUpDown subPicsAheadUpDown;
    private MPLabel mpLabel1;
    private MPCheckBox disableAnimCheckBox;
    private MPLabel mpLabel2;
    private MPCheckBox pow2texCheckBox;

    public MpcHcSubsForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.disableAnimCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pow2texCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.subPicsAheadUpDown = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.textureComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.subPicsAheadUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.disableAnimCheckBox);
      this.groupBox1.Controls.Add(this.mpLabel2);
      this.groupBox1.Controls.Add(this.pow2texCheckBox);
      this.groupBox1.Controls.Add(this.mpLabel1);
      this.groupBox1.Controls.Add(this.subPicsAheadUpDown);
      this.groupBox1.Controls.Add(this.textureComboBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(450, 129);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Options";
      // 
      // disableAnimCheckBox
      // 
      this.disableAnimCheckBox.AutoSize = true;
      this.disableAnimCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.disableAnimCheckBox.Location = new System.Drawing.Point(104, 96);
      this.disableAnimCheckBox.Name = "disableAnimCheckBox";
      this.disableAnimCheckBox.Size = new System.Drawing.Size(137, 17);
      this.disableAnimCheckBox.TabIndex = 16;
      this.disableAnimCheckBox.Text = "(Set for slow computers)";
      this.disableAnimCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(6, 98);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(157, 22);
      this.mpLabel2.TabIndex = 15;
      this.mpLabel2.Text = "Disable Animation:";
      // 
      // pow2texCheckBox
      // 
      this.pow2texCheckBox.AutoSize = true;
      this.pow2texCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.pow2texCheckBox.Location = new System.Drawing.Point(293, 63);
      this.pow2texCheckBox.Name = "pow2texCheckBox";
      this.pow2texCheckBox.Size = new System.Drawing.Size(139, 17);
      this.pow2texCheckBox.TabIndex = 14;
      this.pow2texCheckBox.Text = "Round to the power of 2";
      this.pow2texCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(293, 27);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(151, 37);
      this.mpLabel1.TabIndex = 13;
      this.mpLabel1.Text = "Set to 0 to disable buffering (not recommended)";
      // 
      // subPicsAheadUpDown
      // 
      this.subPicsAheadUpDown.Location = new System.Drawing.Point(232, 30);
      this.subPicsAheadUpDown.Name = "subPicsAheadUpDown";
      this.subPicsAheadUpDown.Size = new System.Drawing.Size(41, 20);
      this.subPicsAheadUpDown.TabIndex = 12;
      this.subPicsAheadUpDown.Value = new decimal(new int[]
                                                    {
                                                      3,
                                                      0,
                                                      0,
                                                      0
                                                    });
      // 
      // textureComboBox
      // 
      this.textureComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textureComboBox.BorderColor = System.Drawing.Color.Empty;
      this.textureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.textureComboBox.Items.AddRange(new object[]
                                            {
                                              "Desktop",
                                              "Medium",
                                              "Low"
                                            });
      this.textureComboBox.Location = new System.Drawing.Point(169, 62);
      this.textureComboBox.Name = "textureComboBox";
      this.textureComboBox.Size = new System.Drawing.Size(104, 21);
      this.textureComboBox.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(6, 65);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(157, 23);
      this.label3.TabIndex = 4;
      this.label3.Text = "Maximum texture resolution:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(220, 24);
      this.label1.TabIndex = 0;
      this.label1.Text = "Number of subpictures to buffer ahead:";
      // 
      // buttonClose
      // 
      this.buttonClose.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonClose.Location = new System.Drawing.Point(383, 155);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(75, 23);
      this.buttonClose.TabIndex = 2;
      this.buttonClose.Text = "Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.Location = new System.Drawing.Point(302, 155);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.okButton_Click);
      // 
      // MpcHcSubsForm
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonClose;
      this.ClientSize = new System.Drawing.Size(466, 200);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.buttonClose);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MinimumSize = new System.Drawing.Size(472, 212);
      this.Name = "MpcHcSubsForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MPC-HC advanced properties";
      this.Load += new System.EventHandler(this.MpcHcSubsForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.subPicsAheadUpDown)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private void okButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("subtitles", "subPicsBufferAhead", subPicsAheadUpDown.Value);
        xmlwriter.SetValue("subtitles", "textureSize", textureComboBox.SelectedItem);
        xmlwriter.SetValueAsBool("subtitles", "pow2tex", pow2texCheckBox.Checked);
        xmlwriter.SetValueAsBool("subtitles", "disableAnimation", disableAnimCheckBox.Checked);
      }
      this.Hide();
    }

    private void closeButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void MpcHcSubsForm_Load(object sender, EventArgs e)
    {
      using (Settings xmlreader = new MPSettings())
      {
        int subPicsBufferAhead = xmlreader.GetValueAsInt("subtitles", "subPicsBufferAhead", 3);
        subPicsAheadUpDown.Value = subPicsBufferAhead;
        bool pow2textures = xmlreader.GetValueAsBool("subtitles", "pow2tex", false);
        pow2texCheckBox.Checked = pow2textures;
        string textureSize = xmlreader.GetValueAsString("subtitles", "textureSize", "Medium");
        textureComboBox.SelectedItem = textureSize;
        bool disableAnimation = xmlreader.GetValueAsBool("subtitles", "disableAnimation", true);
        disableAnimCheckBox.Checked = disableAnimation;
      }
    }
  }
}