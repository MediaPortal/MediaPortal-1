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
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditWeatherCityForm.
  /// </summary>
  public class EditWeatherCityForm : MPConfigForm
  {
    private MPButton cancelButton;
    private MPGroupBox groupBox1;
    private MPLabel label3;
    private MPLabel label1;
    private FolderBrowserDialog folderBrowserDialog;
    private MPButton okButton;
    private MPTextBox locationTextBox;
    private MPLabel label2;
    private MPTextBox temperatureTextBox;
    private MPLabel label4;
    private MPLabel label5;
    private MPTextBox windsTextBox;
    private MPLabel label6;
    private MPTextBox humidityTextBox;
    private MPLabel label7;
    private MPTextBox uvIndexTextBox;
    private MPTextBox precipitationTextBox;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public EditWeatherCityForm()
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
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.precipitationTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.humidityTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.windsTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.uvIndexTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.temperatureTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.locationTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(335, 336);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 10;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.precipitationTextBox);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.humidityTextBox);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.windsTextBox);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.uvIndexTextBox);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.temperatureTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.locationTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(9, 9);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(400, 320);
      this.groupBox1.TabIndex = 9;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "City settings";
      // 
      // precipitationTextBox
      // 
      this.precipitationTextBox.BorderColor = System.Drawing.Color.Empty;
      this.precipitationTextBox.Location = new System.Drawing.Point(16, 288);
      this.precipitationTextBox.Name = "precipitationTextBox";
      this.precipitationTextBox.Size = new System.Drawing.Size(336, 20);
      this.precipitationTextBox.TabIndex = 21;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 272);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(176, 16);
      this.label7.TabIndex = 20;
      this.label7.Text = "Precipitation Image";
      // 
      // humidityTextBox
      // 
      this.humidityTextBox.BorderColor = System.Drawing.Color.Empty;
      this.humidityTextBox.Location = new System.Drawing.Point(16, 248);
      this.humidityTextBox.Name = "humidityTextBox";
      this.humidityTextBox.Size = new System.Drawing.Size(336, 20);
      this.humidityTextBox.TabIndex = 19;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 232);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(168, 16);
      this.label6.TabIndex = 18;
      this.label6.Text = "Humidity image";
      // 
      // windsTextBox
      // 
      this.windsTextBox.BorderColor = System.Drawing.Color.Empty;
      this.windsTextBox.Location = new System.Drawing.Point(16, 208);
      this.windsTextBox.Name = "windsTextBox";
      this.windsTextBox.Size = new System.Drawing.Size(336, 20);
      this.windsTextBox.TabIndex = 17;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 192);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(168, 16);
      this.label5.TabIndex = 16;
      this.label5.Text = "Winds image";
      // 
      // uvIndexTextBox
      // 
      this.uvIndexTextBox.BorderColor = System.Drawing.Color.Empty;
      this.uvIndexTextBox.Location = new System.Drawing.Point(16, 168);
      this.uvIndexTextBox.Name = "uvIndexTextBox";
      this.uvIndexTextBox.Size = new System.Drawing.Size(336, 20);
      this.uvIndexTextBox.TabIndex = 15;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 152);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(160, 16);
      this.label4.TabIndex = 14;
      this.label4.Text = "UV Index image";
      // 
      // temperatureTextBox
      // 
      this.temperatureTextBox.BorderColor = System.Drawing.Color.Empty;
      this.temperatureTextBox.Location = new System.Drawing.Point(16, 128);
      this.temperatureTextBox.Name = "temperatureTextBox";
      this.temperatureTextBox.Size = new System.Drawing.Size(336, 20);
      this.temperatureTextBox.TabIndex = 13;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 112);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(168, 16);
      this.label2.TabIndex = 12;
      this.label2.Text = "Temperature image";
      // 
      // label3
      // 
      this.label3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(368, 40);
      this.label3.TabIndex = 11;
      this.label3.Text = "Below you can enter the location of the various weather images. The image locatio" +
                         "n should be entered as a normal http-address.";
      // 
      // locationTextBox
      // 
      this.locationTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.locationTextBox.BorderColor = System.Drawing.Color.Empty;
      this.locationTextBox.Location = new System.Drawing.Point(16, 88);
      this.locationTextBox.Name = "locationTextBox";
      this.locationTextBox.Size = new System.Drawing.Size(336, 20);
      this.locationTextBox.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 72);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(168, 23);
      this.label1.TabIndex = 6;
      this.label1.Text = "Satellite image";
      // 
      // okButton
      // 
      this.okButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(255, 336);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 11;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // EditWeatherCityForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(418, 368);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "EditWeatherCityForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "EditWeatherCityForm";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    private void okButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    public string SatteliteImage
    {
      get { return locationTextBox.Text; }
      set { locationTextBox.Text = value; }
    }

    public string TemperatureImage
    {
      get { return temperatureTextBox.Text; }
      set { temperatureTextBox.Text = value; }
    }

    public string UVIndexImage
    {
      get { return uvIndexTextBox.Text; }
      set { uvIndexTextBox.Text = value; }
    }

    public string WindsImage
    {
      get { return windsTextBox.Text; }
      set { windsTextBox.Text = value; }
    }

    public string HumidityImage
    {
      get { return humidityTextBox.Text; }
      set { humidityTextBox.Text = value; }
    }

    public string PrecipitationImage
    {
      get { return precipitationTextBox.Text; }
      set { precipitationTextBox.Text = value; }
    }
  }
}