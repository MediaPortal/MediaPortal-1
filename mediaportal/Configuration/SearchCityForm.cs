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
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxNewCity;
    private System.Windows.Forms.ListBox listBoxCityResults;
    private MediaPortal.UserInterface.Controls.MPTextBox searchTextBox;
    private MediaPortal.UserInterface.Controls.MPButton buttonCitySearch;
    private MediaPortal.UserInterface.Controls.MPButton buttonAddCity;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel labelCity;
    private MediaPortal.UserInterface.Controls.MPLabel labelCityResults;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCityDetails;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlCityURLs;
    private TabPage tabPageSatImg;
    private TabPage tabPageTempImg;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxSatURL;
    private TabPage tabPageUVImg;
    private TabPage tabPageWindsImg;
    private TabPage tabPageHumImg;
    private TabPage tabPagePrecImg;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxTempURL;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxUVURL;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxWindURL;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxHumURL;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPrecURL;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancelCity;
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
      this.groupBoxNewCity = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonCancelCity = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxCityDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tabControlCityURLs = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSatImg = new System.Windows.Forms.TabPage();
      this.textBoxSatURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageTempImg = new System.Windows.Forms.TabPage();
      this.textBoxTempURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageUVImg = new System.Windows.Forms.TabPage();
      this.textBoxUVURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageWindsImg = new System.Windows.Forms.TabPage();
      this.textBoxWindURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageHumImg = new System.Windows.Forms.TabPage();
      this.textBoxHumURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPagePrecImg = new System.Windows.Forms.TabPage();
      this.textBoxPrecURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.listBoxCityResults = new System.Windows.Forms.ListBox();
      this.labelCityResults = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonAddCity = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelCity = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonCitySearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.searchTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBoxNewCity.SuspendLayout();
      this.groupBoxCityDetails.SuspendLayout();
      this.tabControlCityURLs.SuspendLayout();
      this.tabPageSatImg.SuspendLayout();
      this.tabPageTempImg.SuspendLayout();
      this.tabPageUVImg.SuspendLayout();
      this.tabPageWindsImg.SuspendLayout();
      this.tabPageHumImg.SuspendLayout();
      this.tabPagePrecImg.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxNewCity
      // 
      this.groupBoxNewCity.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.groupBoxNewCity.Controls.Add(this.buttonCancelCity);
      this.groupBoxNewCity.Controls.Add(this.groupBoxCityDetails);
      this.groupBoxNewCity.Controls.Add(this.labelCity);
      this.groupBoxNewCity.Controls.Add(this.buttonCitySearch);
      this.groupBoxNewCity.Controls.Add(this.searchTextBox);
      this.groupBoxNewCity.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxNewCity.Location = new System.Drawing.Point(8, 8);
      this.groupBoxNewCity.Name = "groupBoxNewCity";
      this.groupBoxNewCity.Size = new System.Drawing.Size(490, 356);
      this.groupBoxNewCity.TabIndex = 0;
      this.groupBoxNewCity.TabStop = false;
      this.groupBoxNewCity.Text = "Add new city";
      // 
      // buttonCancelCity
      // 
      this.buttonCancelCity.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.buttonCancelCity.Location = new System.Drawing.Point(381, 327);
      this.buttonCancelCity.Name = "buttonCancelCity";
      this.buttonCancelCity.Size = new System.Drawing.Size(75, 23);
      this.buttonCancelCity.TabIndex = 38;
      this.buttonCancelCity.Text = "Cancel";
      this.buttonCancelCity.UseVisualStyleBackColor = true;
      this.buttonCancelCity.Visible = false;
      // 
      // groupBoxCityDetails
      // 
      this.groupBoxCityDetails.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.groupBoxCityDetails.Controls.Add(this.tabControlCityURLs);
      this.groupBoxCityDetails.Controls.Add(this.listBoxCityResults);
      this.groupBoxCityDetails.Controls.Add(this.labelCityResults);
      this.groupBoxCityDetails.Controls.Add(this.label3);
      this.groupBoxCityDetails.Controls.Add(this.buttonAddCity);
      this.groupBoxCityDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCityDetails.Location = new System.Drawing.Point(18, 47);
      this.groupBoxCityDetails.Name = "groupBoxCityDetails";
      this.groupBoxCityDetails.Size = new System.Drawing.Size(453, 274);
      this.groupBoxCityDetails.TabIndex = 37;
      this.groupBoxCityDetails.TabStop = false;
      // 
      // tabControlCityURLs
      // 
      this.tabControlCityURLs.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.tabControlCityURLs.Controls.Add(this.tabPageSatImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageTempImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageUVImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageWindsImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageHumImg);
      this.tabControlCityURLs.Controls.Add(this.tabPagePrecImg);
      this.tabControlCityURLs.Location = new System.Drawing.Point(9, 175);
      this.tabControlCityURLs.Name = "tabControlCityURLs";
      this.tabControlCityURLs.SelectedIndex = 0;
      this.tabControlCityURLs.Size = new System.Drawing.Size(433, 58);
      this.tabControlCityURLs.TabIndex = 37;
      // 
      // tabPageSatImg
      // 
      this.tabPageSatImg.Controls.Add(this.textBoxSatURL);
      this.tabPageSatImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageSatImg.Name = "tabPageSatImg";
      this.tabPageSatImg.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSatImg.Size = new System.Drawing.Size(425, 32);
      this.tabPageSatImg.TabIndex = 0;
      this.tabPageSatImg.Text = "Satellite";
      this.tabPageSatImg.UseVisualStyleBackColor = true;
      // 
      // textBoxSatURL
      // 
      this.textBoxSatURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxSatURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSatURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxSatURL.Name = "textBoxSatURL";
      this.textBoxSatURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxSatURL.TabIndex = 25;
      // 
      // tabPageTempImg
      // 
      this.tabPageTempImg.Controls.Add(this.textBoxTempURL);
      this.tabPageTempImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageTempImg.Name = "tabPageTempImg";
      this.tabPageTempImg.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTempImg.Size = new System.Drawing.Size(425, 32);
      this.tabPageTempImg.TabIndex = 1;
      this.tabPageTempImg.Text = "Temperature";
      this.tabPageTempImg.UseVisualStyleBackColor = true;
      // 
      // textBoxTempURL
      // 
      this.textBoxTempURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxTempURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTempURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxTempURL.Name = "textBoxTempURL";
      this.textBoxTempURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxTempURL.TabIndex = 27;
      // 
      // tabPageUVImg
      // 
      this.tabPageUVImg.Controls.Add(this.textBoxUVURL);
      this.tabPageUVImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageUVImg.Name = "tabPageUVImg";
      this.tabPageUVImg.Size = new System.Drawing.Size(425, 32);
      this.tabPageUVImg.TabIndex = 2;
      this.tabPageUVImg.Text = "UV Index";
      this.tabPageUVImg.UseVisualStyleBackColor = true;
      // 
      // textBoxUVURL
      // 
      this.textBoxUVURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxUVURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxUVURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxUVURL.Name = "textBoxUVURL";
      this.textBoxUVURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxUVURL.TabIndex = 40;
      // 
      // tabPageWindsImg
      // 
      this.tabPageWindsImg.Controls.Add(this.textBoxWindURL);
      this.tabPageWindsImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageWindsImg.Name = "tabPageWindsImg";
      this.tabPageWindsImg.Size = new System.Drawing.Size(425, 32);
      this.tabPageWindsImg.TabIndex = 3;
      this.tabPageWindsImg.Text = "Winds";
      this.tabPageWindsImg.UseVisualStyleBackColor = true;
      // 
      // textBoxWindURL
      // 
      this.textBoxWindURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxWindURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxWindURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxWindURL.Name = "textBoxWindURL";
      this.textBoxWindURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxWindURL.TabIndex = 31;
      // 
      // tabPageHumImg
      // 
      this.tabPageHumImg.Controls.Add(this.textBoxHumURL);
      this.tabPageHumImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageHumImg.Name = "tabPageHumImg";
      this.tabPageHumImg.Size = new System.Drawing.Size(425, 32);
      this.tabPageHumImg.TabIndex = 4;
      this.tabPageHumImg.Text = "Humidity";
      this.tabPageHumImg.UseVisualStyleBackColor = true;
      // 
      // textBoxHumURL
      // 
      this.textBoxHumURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxHumURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxHumURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxHumURL.Name = "textBoxHumURL";
      this.textBoxHumURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxHumURL.TabIndex = 33;
      // 
      // tabPagePrecImg
      // 
      this.tabPagePrecImg.Controls.Add(this.textBoxPrecURL);
      this.tabPagePrecImg.Location = new System.Drawing.Point(4, 22);
      this.tabPagePrecImg.Name = "tabPagePrecImg";
      this.tabPagePrecImg.Size = new System.Drawing.Size(425, 32);
      this.tabPagePrecImg.TabIndex = 5;
      this.tabPagePrecImg.Text = "Precipitation";
      this.tabPagePrecImg.UseVisualStyleBackColor = true;
      // 
      // textBoxPrecURL
      // 
      this.textBoxPrecURL.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.textBoxPrecURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPrecURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxPrecURL.Name = "textBoxPrecURL";
      this.textBoxPrecURL.Size = new System.Drawing.Size(413, 20);
      this.textBoxPrecURL.TabIndex = 35;
      // 
      // listBoxCityResults
      // 
      this.listBoxCityResults.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.listBoxCityResults.Location = new System.Drawing.Point(62, 18);
      this.listBoxCityResults.Name = "listBoxCityResults";
      this.listBoxCityResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.listBoxCityResults.Size = new System.Drawing.Size(378, 82);
      this.listBoxCityResults.TabIndex = 6;
      this.listBoxCityResults.SelectedIndexChanged += new System.EventHandler(this.listBoxCityResults_SelectedIndexChanged);
      // 
      // labelCityResults
      // 
      this.labelCityResults.Location = new System.Drawing.Point(8, 18);
      this.labelCityResults.Name = "labelCityResults";
      this.labelCityResults.Size = new System.Drawing.Size(48, 32);
      this.labelCityResults.TabIndex = 36;
      this.labelCityResults.Text = "Cities found:";
      // 
      // label3
      // 
      this.label3.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.label3.AutoEllipsis = true;
      this.label3.Location = new System.Drawing.Point(8, 126);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(439, 48);
      this.label3.TabIndex = 24;
      this.label3.Text = "Below you can enter the location of the various weather images.\r\nThe image locati" +
          "on should be entered as a normal http-address.\r\nYou\'ll find many pictures at www" +
          ".weather.com or your local news site.";
      // 
      // buttonAddCity
      // 
      this.buttonAddCity.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.buttonAddCity.Enabled = false;
      this.buttonAddCity.Location = new System.Drawing.Point(363, 239);
      this.buttonAddCity.Name = "buttonAddCity";
      this.buttonAddCity.Size = new System.Drawing.Size(75, 23);
      this.buttonAddCity.TabIndex = 0;
      this.buttonAddCity.Text = "Add City";
      this.buttonAddCity.UseVisualStyleBackColor = true;
      this.buttonAddCity.Click += new System.EventHandler(this.buttonAddCity_Click);
      // 
      // labelCity
      // 
      this.labelCity.AutoSize = true;
      this.labelCity.Location = new System.Drawing.Point(16, 24);
      this.labelCity.Name = "labelCity";
      this.labelCity.Size = new System.Drawing.Size(63, 13);
      this.labelCity.TabIndex = 35;
      this.labelCity.Text = "Search city:";
      // 
      // buttonCitySearch
      // 
      this.buttonCitySearch.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.buttonCitySearch.Enabled = false;
      this.buttonCitySearch.Location = new System.Drawing.Point(381, 19);
      this.buttonCitySearch.Name = "buttonCitySearch";
      this.buttonCitySearch.Size = new System.Drawing.Size(75, 23);
      this.buttonCitySearch.TabIndex = 1;
      this.buttonCitySearch.Text = "Search";
      this.buttonCitySearch.UseVisualStyleBackColor = true;
      this.buttonCitySearch.Click += new System.EventHandler(this.buttonCitySearch_Click);
      // 
      // searchTextBox
      // 
      this.searchTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.searchTextBox.BorderColor = System.Drawing.Color.Empty;
      this.searchTextBox.Location = new System.Drawing.Point(80, 21);
      this.searchTextBox.Name = "searchTextBox";
      this.searchTextBox.Size = new System.Drawing.Size(292, 20);
      this.searchTextBox.TabIndex = 0;
      this.searchTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyUp);
      this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
      // 
      // SearchCityForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(506, 372);
      this.Controls.Add(this.groupBoxNewCity);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "SearchCityForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add new cities to my weather";
      this.Load += new System.EventHandler(this.SearchCityForm_Load);
      this.groupBoxNewCity.ResumeLayout(false);
      this.groupBoxNewCity.PerformLayout();
      this.groupBoxCityDetails.ResumeLayout(false);
      this.tabControlCityURLs.ResumeLayout(false);
      this.tabPageSatImg.ResumeLayout(false);
      this.tabPageSatImg.PerformLayout();
      this.tabPageTempImg.ResumeLayout(false);
      this.tabPageTempImg.PerformLayout();
      this.tabPageUVImg.ResumeLayout(false);
      this.tabPageUVImg.PerformLayout();
      this.tabPageWindsImg.ResumeLayout(false);
      this.tabPageWindsImg.PerformLayout();
      this.tabPageHumImg.ResumeLayout(false);
      this.tabPageHumImg.PerformLayout();
      this.tabPagePrecImg.ResumeLayout(false);
      this.tabPagePrecImg.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonCitySearch_Click( object sender, System.EventArgs e )
    {
      
      //
      // Disable add button
      //
      buttonAddCity.Enabled = false;

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
        listBoxCityResults.Items.Clear();

        foreach (WeatherChannel.City city in cities)
        {
          listBoxCityResults.Items.Add(city);

          if (listBoxCityResults.Items.Count == 1)
            listBoxCityResults.SelectedItem = listBoxCityResults.Items[0];
        }
        if (listBoxCityResults.Items.Count > 0)
        {
          groupBoxCityDetails.Visible = true;
          buttonCancelCity.Visible = true;
          this.Height = 488 + 40;
        }
        else
          MessageBox.Show("No cities found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

      }
      catch (Exception ex)
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
    private void searchTextBox_TextChanged( object sender, System.EventArgs e )
    {
      buttonCitySearch.Enabled = searchTextBox.Text.Length > 0;
    }

    private void searchTextBox_KeyUp( object sender, System.Windows.Forms.KeyEventArgs e )
    {
        if (e.KeyCode == Keys.Enter)
            this.buttonCitySearch.PerformClick();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonAddCity_Click(object sender, System.EventArgs e)
    {
      foreach (WeatherChannel.City city in listBoxCityResults.SelectedItems)
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
    private void listBoxCityResults_SelectedIndexChanged( object sender, System.EventArgs e )
    {
      buttonAddCity.Enabled = listBoxCityResults.SelectedItems.Count > 0;
    }

    private void SearchCityForm_Load(object sender, System.EventArgs e)
    {
      this.Height = 56 + 50;
      groupBoxCityDetails.Visible = false;
    }

    public string SatteliteImage
    {
      get { return textBoxSatURL.Text; }
      set { textBoxSatURL.Text = value; }
    }

    public string TemperatureImage
    {
      get { return textBoxTempURL.Text; }
      set { textBoxTempURL.Text = value; }
    }

    public string UVIndexImage
    {
      get { return textBoxUVURL.Text; }
      set { textBoxUVURL.Text = value; }
    }

    public string WindsImage
    {
      get { return textBoxWindURL.Text; }
      set { textBoxWindURL.Text = value; }
    }

    public string HumidityImage
    {
      get { return textBoxHumURL.Text; }
      set { textBoxHumURL.Text = value; }
    }

    public string PrecipitationImage
    {
      get { return textBoxPrecURL.Text; }
      set { textBoxPrecURL.Text = value; }
    }

  }
}
