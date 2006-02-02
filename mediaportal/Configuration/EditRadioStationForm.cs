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
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditRadioStationForm.
  /// </summary>
  public class EditRadioStationForm : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton closeButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox nameTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPTextBox genreTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPTextBox bitrateTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPTextBox urlTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPComboBox typeComboBox;
    private MediaPortal.UserInterface.Controls.MPTextBox frequencyTextBox;
    private MediaPortal.UserInterface.Controls.MPButton searchButton;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    RadioStation station = new RadioStation();

    public EditRadioStationForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // Set default settings
      //
      UpdateControlStates();
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
      this.typeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.urlTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bitrateTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.genreTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.frequencyTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.nameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.closeButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.searchButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.typeComboBox);
      this.groupBox1.Controls.Add(this.urlTextBox);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.bitrateTextBox);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.genreTextBox);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.frequencyTextBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.nameTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(360, 196);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Radio Station";
      // 
      // typeComboBox
      // 
      this.typeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.typeComboBox.Items.AddRange(new object[] {
															  "Radio",
															  "Stream"});
      this.typeComboBox.Location = new System.Drawing.Point(121, 27);
      this.typeComboBox.Name = "typeComboBox";
      this.typeComboBox.Size = new System.Drawing.Size(224, 21);
      this.typeComboBox.TabIndex = 0;
      this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
      // 
      // urlTextBox
      // 
      this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.urlTextBox.Location = new System.Drawing.Point(120, 152);
      this.urlTextBox.Name = "urlTextBox";
      this.urlTextBox.Size = new System.Drawing.Size(224, 20);
      this.urlTextBox.TabIndex = 5;
      this.urlTextBox.Text = "";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 155);
      this.label6.Name = "label6";
      this.label6.TabIndex = 10;
      this.label6.Text = "URL";
      // 
      // bitrateTextBox
      // 
      this.bitrateTextBox.Location = new System.Drawing.Point(120, 127);
      this.bitrateTextBox.MaxLength = 3;
      this.bitrateTextBox.Name = "bitrateTextBox";
      this.bitrateTextBox.Size = new System.Drawing.Size(40, 20);
      this.bitrateTextBox.TabIndex = 4;
      this.bitrateTextBox.Text = "";
      this.bitrateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.bitrateTextBox_KeyPress);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 130);
      this.label5.Name = "label5";
      this.label5.TabIndex = 8;
      this.label5.Text = "Bitrate";
      // 
      // genreTextBox
      // 
      this.genreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.genreTextBox.Location = new System.Drawing.Point(120, 102);
      this.genreTextBox.Name = "genreTextBox";
      this.genreTextBox.Size = new System.Drawing.Size(224, 20);
      this.genreTextBox.TabIndex = 3;
      this.genreTextBox.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 105);
      this.label4.Name = "label4";
      this.label4.TabIndex = 6;
      this.label4.Text = "Genre";
      // 
      // frequencyTextBox
      // 
      this.frequencyTextBox.Location = new System.Drawing.Point(120, 77);
      this.frequencyTextBox.MaxLength = 9;
      this.frequencyTextBox.Name = "frequencyTextBox";
      this.frequencyTextBox.Size = new System.Drawing.Size(112, 20);
      this.frequencyTextBox.TabIndex = 2;
      this.frequencyTextBox.Text = "";
      this.frequencyTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frequencyTextBox_KeyPress);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 80);
      this.label3.Name = "label3";
      this.label3.TabIndex = 4;
      this.label3.Text = "Frequency";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.Location = new System.Drawing.Point(120, 52);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(224, 20);
      this.nameTextBox.TabIndex = 1;
      this.nameTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 55);
      this.label2.Name = "label2";
      this.label2.TabIndex = 2;
      this.label2.Text = "Name";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 30);
      this.label1.Name = "label1";
      this.label1.TabIndex = 0;
      this.label1.Text = "Type";
      // 
      // closeButton
      // 
      this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.closeButton.Location = new System.Drawing.Point(292, 213);
      this.closeButton.Name = "closeButton";
      this.closeButton.TabIndex = 2;
      this.closeButton.Text = "Close";
      this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(211, 213);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 1;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // searchButton
      // 
      this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.searchButton.Enabled = false;
      this.searchButton.Location = new System.Drawing.Point(16, 214);
      this.searchButton.Name = "searchButton";
      this.searchButton.Size = new System.Drawing.Size(104, 22);
      this.searchButton.TabIndex = 0;
      this.searchButton.Text = "Search SHOUTcast";
      this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
      // 
      // EditRadioStationForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.closeButton;
      this.ClientSize = new System.Drawing.Size(376, 248);
      this.Controls.Add(this.searchButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.closeButton);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(384, 272);
      this.Name = "EditRadioStationForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Enter properties for the radio station";
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void okButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void closeButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void typeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      UpdateControlStates();
    }

    private void UpdateControlStates()
    {
      //
      // Make default selection
      //
      if (typeComboBox.SelectedItem == null)
        typeComboBox.SelectedItem = "Radio";

      bitrateTextBox.Enabled = urlTextBox.Enabled = searchButton.Enabled = typeComboBox.SelectedItem.Equals("Stream");
      frequencyTextBox.Enabled = !urlTextBox.Enabled && !searchButton.Enabled;
    }

    private void frequencyTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Make sure we only type one comma or dot
      //
      if (e.KeyChar == '.' || e.KeyChar == ',')
      {
        if (frequencyTextBox.Text.IndexOfAny(new char[] { ',', '.' }) >= 0)
        {
          e.Handled = true;
          return;
        }
      }

      if (char.IsNumber(e.KeyChar) == false && (e.KeyChar != 8 && e.KeyChar != '.' && e.KeyChar != ','))
      {
        e.Handled = true;
      }
    }

    private void bitrateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void searchButton_Click(object sender, System.EventArgs e)
    {
      SearchSHOUTcast fm = new SearchSHOUTcast();
      DialogResult dialogResult = fm.ShowDialog(this);
      if (fm.Station == null) return;
      nameTextBox.Text = fm.Station.Name;
      bitrateTextBox.Text = fm.Station.Bitrate.ToString();
      urlTextBox.Text = fm.Station.URL;
    }
    public RadioStation Station
    {
      get
      {

        station.Type = (string)typeComboBox.SelectedItem;
        station.Name = nameTextBox.Text;

        try
        {

          if (frequencyTextBox.Text.IndexOfAny(new char[] { ',', '.' }) >= 0)
          {
            char[] separators = new char[] { '.', ',' };

            for (int index = 0; index < separators.Length; index++)
            {
              try
              {
                frequencyTextBox.Text = frequencyTextBox.Text.Replace(',', separators[index]);
                frequencyTextBox.Text = frequencyTextBox.Text.Replace('.', separators[index]);

                //
                // MegaHerz
                //
                station.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);

                break;
              }
              catch
              {
                //
                // Failed to convert, try next separator
                //
              }
            }
          }
          else
          {
            //
            // Herz
            //
            if (frequencyTextBox.Text.Length > 3)
            {
              station.Frequency = Convert.ToInt32(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0");
            }
            else
            {
              station.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);
            }
          }
        }
        catch
        {
          station.Frequency = 0;
        }

        station.Genre = genreTextBox.Text.Length > 0 ? genreTextBox.Text : Strings.Unknown;
        station.Bitrate = Convert.ToInt32(bitrateTextBox.Text.Length > 0 ? bitrateTextBox.Text : "0");
        station.URL = urlTextBox.Text;

        return station;
      }

      set
      {
        station = value as RadioStation;

        typeComboBox.SelectedItem = (string)station.Type;
        nameTextBox.Text = station.Name;
        frequencyTextBox.Text = station.Frequency.ToString(Frequency.Format.MegaHerz);
        genreTextBox.Text = station.Genre;
        bitrateTextBox.Text = station.Bitrate.ToString();
        urlTextBox.Text = station.URL;
      }
    }
  }
}
