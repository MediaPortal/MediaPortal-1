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

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for SearchCityForm.
	/// </summary>
	public class SearchCityForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPButton closeButton;
		private System.Windows.Forms.ListBox resultListBox;
		private System.Windows.Forms.TextBox searchTextBox;
		private MediaPortal.UserInterface.Controls.MPButton searchButton;
		private MediaPortal.UserInterface.Controls.MPButton addButton;
		private System.Windows.Forms.TextBox precipitationTextBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox humidityTextBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox windsTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox uvIndexTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox temperatureTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox locationTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SearchCityForm()
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.locationTextBox = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.uvIndexTextBox = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.humidityTextBox = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.temperatureTextBox = new System.Windows.Forms.TextBox();
      this.resultListBox = new System.Windows.Forms.ListBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.windsTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.precipitationTextBox = new System.Windows.Forms.TextBox();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.closeButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.label8 = new System.Windows.Forms.Label();
      this.searchButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.searchTextBox = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.searchButton);
      this.groupBox1.Controls.Add(this.searchTextBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(480, 464);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Add new city";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.locationTextBox);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.uvIndexTextBox);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.humidityTextBox);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.temperatureTextBox);
      this.groupBox2.Controls.Add(this.resultListBox);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.windsTextBox);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.precipitationTextBox);
      this.groupBox2.Controls.Add(this.addButton);
      this.groupBox2.Controls.Add(this.closeButton);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(16, 56);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(456, 400);
      this.groupBox2.TabIndex = 37;
      this.groupBox2.TabStop = false;
      // 
      // locationTextBox
      // 
      this.locationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.locationTextBox.Location = new System.Drawing.Point(24, 128);
      this.locationTextBox.Name = "locationTextBox";
      this.locationTextBox.Size = new System.Drawing.Size(336, 20);
      this.locationTextBox.TabIndex = 23;
      this.locationTextBox.Text = "";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(8, 312);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(176, 16);
      this.label7.TabIndex = 33;
      this.label7.Text = "Precipitation Image";
      // 
      // uvIndexTextBox
      // 
      this.uvIndexTextBox.Location = new System.Drawing.Point(24, 208);
      this.uvIndexTextBox.Name = "uvIndexTextBox";
      this.uvIndexTextBox.Size = new System.Drawing.Size(336, 20);
      this.uvIndexTextBox.TabIndex = 28;
      this.uvIndexTextBox.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(8, 232);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(168, 16);
      this.label5.TabIndex = 29;
      this.label5.Text = "Winds image";
      // 
      // humidityTextBox
      // 
      this.humidityTextBox.Location = new System.Drawing.Point(24, 288);
      this.humidityTextBox.Name = "humidityTextBox";
      this.humidityTextBox.Size = new System.Drawing.Size(336, 20);
      this.humidityTextBox.TabIndex = 32;
      this.humidityTextBox.Text = "";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(8, 272);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(168, 16);
      this.label6.TabIndex = 31;
      this.label6.Text = "Humidity image";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 112);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(168, 23);
      this.label1.TabIndex = 22;
      this.label1.Text = "Satellite image";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 192);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(160, 16);
      this.label4.TabIndex = 27;
      this.label4.Text = "UV Index image";
      // 
      // temperatureTextBox
      // 
      this.temperatureTextBox.Location = new System.Drawing.Point(24, 168);
      this.temperatureTextBox.Name = "temperatureTextBox";
      this.temperatureTextBox.Size = new System.Drawing.Size(336, 20);
      this.temperatureTextBox.TabIndex = 26;
      this.temperatureTextBox.Text = "";
      // 
      // resultListBox
      // 
      this.resultListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.resultListBox.Location = new System.Drawing.Point(80, 16);
      this.resultListBox.Name = "resultListBox";
      this.resultListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.resultListBox.Size = new System.Drawing.Size(264, 56);
      this.resultListBox.TabIndex = 6;
      this.resultListBox.SelectedIndexChanged += new System.EventHandler(this.resultListBox_SelectedIndexChanged);
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(16, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(48, 32);
      this.label9.TabIndex = 36;
      this.label9.Text = "Cities found:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 152);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(168, 16);
      this.label2.TabIndex = 25;
      this.label2.Text = "Temperature image";
      // 
      // windsTextBox
      // 
      this.windsTextBox.Location = new System.Drawing.Point(24, 248);
      this.windsTextBox.Name = "windsTextBox";
      this.windsTextBox.Size = new System.Drawing.Size(336, 20);
      this.windsTextBox.TabIndex = 30;
      this.windsTextBox.Text = "";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(16, 80);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(408, 32);
      this.label3.TabIndex = 24;
      this.label3.Text = "Below you can enter the location of the various weather images. The image locatio" +
        "n should be entered as a normal http-address.";
      // 
      // precipitationTextBox
      // 
      this.precipitationTextBox.Location = new System.Drawing.Point(24, 328);
      this.precipitationTextBox.Name = "precipitationTextBox";
      this.precipitationTextBox.Size = new System.Drawing.Size(336, 20);
      this.precipitationTextBox.TabIndex = 34;
      this.precipitationTextBox.Text = "";
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Enabled = false;
      this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.addButton.Location = new System.Drawing.Point(288, 368);
      this.addButton.Name = "addButton";
      this.addButton.TabIndex = 0;
      this.addButton.Text = "OK";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // closeButton
      // 
      this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.closeButton.Location = new System.Drawing.Point(376, 368);
      this.closeButton.Name = "closeButton";
      this.closeButton.TabIndex = 1;
      this.closeButton.Text = "Cancel";
      this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 24);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(56, 16);
      this.label8.TabIndex = 35;
      this.label8.Text = "City:";
      // 
      // searchButton
      // 
      this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.searchButton.Enabled = false;
      this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.searchButton.Location = new System.Drawing.Point(360, 24);
      this.searchButton.Name = "searchButton";
      this.searchButton.TabIndex = 1;
      this.searchButton.Text = "Search";
      this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
      // 
      // searchTextBox
      // 
      this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.searchTextBox.Location = new System.Drawing.Point(72, 24);
      this.searchTextBox.Name = "searchTextBox";
      this.searchTextBox.Size = new System.Drawing.Size(272, 20);
      this.searchTextBox.TabIndex = 0;
      this.searchTextBox.Text = "";
      this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
      // 
      // SearchCityForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(506, 480);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "SearchCityForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add new cities to my weather";
      this.Load += new System.EventHandler(this.SearchCityForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void searchButton_Click(object sender, System.EventArgs e)
		{
			//
			// Disable add button
			//
			addButton.Enabled = false;

      try
      {
        //
        // Perform actual search
        //
        WeatherChannel weather = new WeatherChannel();
        ArrayList cities = weather.SearchCity(searchTextBox.Text);

        //
        // Clear previous results
        //
        resultListBox.Items.Clear();

        foreach(WeatherChannel.City city in cities)
        {
          resultListBox.Items.Add(city);

          if(resultListBox.Items.Count == 1)
            resultListBox.SelectedItem = resultListBox.Items[0];
        }
				if (resultListBox.Items.Count>0)
				{
					groupBox2.Visible=true;
					this.Height=488+40;
				}
				else
					MessageBox.Show("No cities found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
		}

		/// <summary>
		/// 
		/// </summary>
		public ArrayList SelectedCities
		{
			get 
			{
				return selectedCities;
			}
		}
		ArrayList selectedCities = new ArrayList();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void searchTextBox_TextChanged(object sender, System.EventArgs e)
		{
			searchButton.Enabled = searchTextBox.Text.Length > 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void addButton_Click(object sender, System.EventArgs e)
		{
			foreach(WeatherChannel.City city in resultListBox.SelectedItems)
			{
				selectedCities.Add(city);
			}

			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void resultListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			addButton.Enabled = resultListBox.SelectedItems.Count > 0;
		}

		private void SearchCityForm_Load(object sender, System.EventArgs e)
		{
			this.Height=56+50;
			groupBox2.Visible=false;
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
