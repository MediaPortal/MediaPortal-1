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
using SmarDtvUsbCi;

namespace SetupTv.Sections
{
  partial class SmarDtvUsbCiConfig
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
    /// The user interface for this plugin is custom-designed so that additional CI products can be easily added
    /// simply by defining an additional product in SmarDtvUsbCiProducts.GetProductList(). Nothing will show in
    /// the designer.
    /// </summary>
    private void InitializeComponent()
    {
      int groupHeight = 103;
      int groupPadding = 10;
      int componentCount = 5;
      _products = SmarDtvUsbCiProducts.GetProductList();
      _tunerSelections = new MPComboBox[_products.Count];
      _installStateLabels = new Label[_products.Count];

      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmarDtvUsbCiConfig));
      this.SuspendLayout();
      for (int i = 0; i < _products.Count; i++)
      {
        // Groupbox wrapper for each CI product.
        GroupBox gb = new GroupBox();
        gb.SuspendLayout();
        gb.Location = new Point(3, 3 + (i * (groupHeight + groupPadding)));
        gb.Name = "groupBox" + i;
        gb.Size = new Size(446, groupHeight);
        gb.TabIndex = (i * componentCount) + 1;
        gb.TabStop = false;
        gb.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        gb.Text = _products[i].ProductName;

        // CI product install state label.
        Label installStateLabel = new Label();
        installStateLabel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        installStateLabel.Location = new Point(6, 22);
        installStateLabel.Name = "installStateLabel";
        installStateLabel.Size = new Size(412, 20);
        installStateLabel.TabIndex = (i * componentCount) + 2;
        gb.Controls.Add(installStateLabel);
        _installStateLabels[i] = installStateLabel;

        // Tuner selection label.
        Label tunerSelectionLabel = new Label();
        tunerSelectionLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        tunerSelectionLabel.Location = new Point(6, 45);
        tunerSelectionLabel.Name = "tunerSelectionLabel" + i;
        tunerSelectionLabel.Size = new System.Drawing.Size(412, 18);
        tunerSelectionLabel.TabIndex = (i * componentCount) + 3;
        tunerSelectionLabel.Text = "Select a digital tuner to use the CI module with:";
        gb.Controls.Add(tunerSelectionLabel);

        // Tuner icon.
        PictureBox tunerSelectionPicture = new PictureBox();
        ((ISupportInitialize)tunerSelectionPicture).BeginInit();
        tunerSelectionPicture.Image = (Image)resources.GetObject("tunerSelectionPicture.Image");
        tunerSelectionPicture.Location = new Point(24, 68);
        tunerSelectionPicture.Name = "tunerSelectionPicture" + i;
        tunerSelectionPicture.Size = new Size(33, 23);
        tunerSelectionPicture.SizeMode = PictureBoxSizeMode.AutoSize;
        tunerSelectionPicture.TabIndex = (i * componentCount) + 4;
        tunerSelectionPicture.TabStop = false;
        ((ISupportInitialize)tunerSelectionPicture).EndInit();
        gb.Controls.Add(tunerSelectionPicture);

        // Tuner selection.
        MPComboBox tunerSelectionCombo = new MPComboBox();
        tunerSelectionCombo.FormattingEnabled = true;
        tunerSelectionCombo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        tunerSelectionCombo.Location = new Point(80, 68);
        tunerSelectionCombo.Name = "tunerSelectionCombo" + i;
        tunerSelectionCombo.Size = new Size(340, 20);
        tunerSelectionCombo.TabIndex = (i * componentCount) + 5;
        gb.Controls.Add(tunerSelectionCombo);
        _tunerSelections[i] = tunerSelectionCombo;

        gb.ResumeLayout(false);
        gb.PerformLayout();
        this.Controls.Add(gb);
      }

      // "Tips" section heading.
      Label tipHeadingLabel = new Label();
      tipHeadingLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
      tipHeadingLabel.ForeColor = Color.Black;
      tipHeadingLabel.Location = new Point(11, _products.Count * (groupHeight + groupPadding));
      tipHeadingLabel.Name = "tipHeadingLabel";
      tipHeadingLabel.Size = new Size(412, 16);
      tipHeadingLabel.TabIndex = (_products.Count * componentCount) + 1;
      tipHeadingLabel.Text = "Tips:";
      this.Controls.Add(tipHeadingLabel);

      // Tips.
      Label tipsLabel = new Label();
      tipsLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
      tipsLabel.ForeColor = Color.Black;
      tipsLabel.Location = new Point(11, (_products.Count * (groupHeight + groupPadding)) + 20);
      tipsLabel.Name = "tipsLabel";
      tipsLabel.Size = new Size(438, 105);
      tipsLabel.TabIndex = (_products.Count * componentCount) + 2;
      tipsLabel.Text = resources.GetString("tipsLabel.Text");
      this.Controls.Add(tipsLabel);

      // Overall configuration.
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Name = "SmarDtvUsbCiConfig";
      this.Size = new Size(483, 450);

      this.ResumeLayout(false);
    }
  }
}
