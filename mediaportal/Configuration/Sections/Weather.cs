#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Weather : SectionSettings
  {
    private const int MaximumCities = 20;

    private enum WindUnit : int
    {
      Kmh = 0,
      mph = 1,
      ms = 2,
      Kn = 3,
      Bft = 4,
    }

    private WindUnit selectedWindUnit = WindUnit.Bft;
    private MPGroupBox groupBox1;
    private MPGroupBox mpGroupBox1;
    private MPLabel label1;
    private MPComboBox temperatureComboBox;
    private MPComboBox windSpeedComboBox;
    private MPLabel label2;
    private MPLabel label3;
    private MPButton removeButton;
    private MPButton searchButton;
    private MPListView citiesListView;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private MPTextBox intervalTextBox;
    private MPButton editButton;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader7;
    private ColumnHeader columnHeader8;
    private IContainer components = null;

    public Weather()
      : this("Weather")
    {
    }

    public Weather(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      //
      // Populate combo boxes with default values
      //
      temperatureComboBox.Items.AddRange(new string[] {"Celsius", "Fahrenheit"});
      windSpeedComboBox.Items.AddRange(new string[]
                                         {"kilometers / hour", "miles / hour", "meters / second", "Knots", "Beaufort"});
    }

    /// <summary>
    /// Loads weather settings from MediaPortal.xml
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        int loadWind = xmlreader.GetValueAsInt("weather", "speed", 4);
        switch (loadWind)
        {
          case 0:
            selectedWindUnit = WindUnit.Kmh;
            windSpeedComboBox.Text = "kilometers / hour";
            break;
          case 1:
            selectedWindUnit = WindUnit.mph;
            windSpeedComboBox.Text = "miles / hour";
            break;
          case 2:
            selectedWindUnit = WindUnit.ms;
            windSpeedComboBox.Text = "meters / second";
            break;
          case 3:
            selectedWindUnit = WindUnit.Kn;
            windSpeedComboBox.Text = "Knots";
            break;
          case 4:
            selectedWindUnit = WindUnit.Bft;
            windSpeedComboBox.Text = "Beaufort";
            break;
          default:
            selectedWindUnit = WindUnit.Bft;
            windSpeedComboBox.Text = "Beaufort";
            break;
        }
        // Get temperature measurement type
        string temperature = xmlreader.GetValueAsString("weather", "temperature", "C");
        if (temperature.Equals("C"))
        {
          temperatureComboBox.Text = "Celsius";
        }
        else if (temperature.Equals("F"))
        {
          temperatureComboBox.Text = "Fahrenheit";
        }
        // Get refresh interval setting
        intervalTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("weather", "refresh", 60));
        // Get number of cities and city information
        for (int index = 0; index < MaximumCities; index++)
        {
          string cityName = String.Format("city{0}", index);
          string cityCode = String.Format("code{0}", index);
          string citySat = String.Format("sat{0}", index);
          string cityTemp = String.Format("temp{0}", index);
          string cityUV = String.Format("uv{0}", index);
          string cityWinds = String.Format("winds{0}", index);
          string cityHumid = String.Format("humid{0}", index);
          string cityPrecip = String.Format("precip{0}", index);
          //Read city information from index
          string cityNameData = xmlreader.GetValueAsString("weather", cityName, "");
          string cityCodeData = xmlreader.GetValueAsString("weather", cityCode, "");
          string citySatData = xmlreader.GetValueAsString("weather", citySat, "");
          string cityTempData = xmlreader.GetValueAsString("weather", cityTemp, "");
          string cityUVData = xmlreader.GetValueAsString("weather", cityUV, "");
          string cityWindsData = xmlreader.GetValueAsString("weather", cityWinds, "");
          string cityHumidData = xmlreader.GetValueAsString("weather", cityHumid, "");
          string cityPrecipData = xmlreader.GetValueAsString("weather", cityPrecip, "");
          if (cityNameData.Length > 0 && cityCodeData.Length > 0)
          {
            citiesListView.Items.Add(
              new ListViewItem(new string[]
                                 {
                                   cityNameData, cityCodeData, citySatData, cityTempData, cityUVData, cityWindsData,
                                   cityHumidData, cityPrecipData
                                 }));
          }
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (windSpeedComboBox.Text.Equals("kilometers / hour"))
        {
          selectedWindUnit = WindUnit.Kmh;
        }
        else if (windSpeedComboBox.Text.Equals("miles / hour"))
        {
          selectedWindUnit = WindUnit.mph;
        }
        else if (windSpeedComboBox.Text.Equals("meters / second"))
        {
          selectedWindUnit = WindUnit.ms;
        }
        else if (windSpeedComboBox.Text.Equals("Knots"))
        {
          selectedWindUnit = WindUnit.Kn;
        }
        else if (windSpeedComboBox.Text.Equals("Beaufort"))
        {
          selectedWindUnit = WindUnit.Bft;
        }
        // Write the speed units
        xmlwriter.SetValue("weather", "speed", (int) selectedWindUnit);
        // Define the temperature measurement
        string temperature = string.Empty;
        if (temperatureComboBox.Text.Equals("Celsius"))
        {
          temperature = "C";
        }
        else if (temperatureComboBox.Text.Equals("Fahrenheit"))
        {
          temperature = "F";
        }
        xmlwriter.SetValue("weather", "temperature", temperature);
        // Define the interval time between weather updates
        xmlwriter.SetValue("weather", "refresh", intervalTextBox.Text);
        // Save city information
        for (int index = 0; index < MaximumCities; index++)
        {
          string cityName = String.Format("city{0}", index);
          string cityCode = String.Format("code{0}", index);
          string citySat = String.Format("sat{0}", index);
          string cityTemp = String.Format("temp{0}", index);
          string cityUV = String.Format("uv{0}", index);
          string cityWinds = String.Format("winds{0}", index);
          string cityHumid = String.Format("humid{0}", index);
          string cityPrecip = String.Format("precip{0}", index);
          string cityNameData = string.Empty;
          string cityCodeData = string.Empty;
          string citySatData = string.Empty;
          string cityTempData = string.Empty;
          string cityUVData = string.Empty;
          string cityWindsData = string.Empty;
          string cityHumidData = string.Empty;
          string cityPrecipData = string.Empty;
          if (citiesListView.Items != null && citiesListView.Items.Count > index)
          {
            cityNameData = citiesListView.Items[index].SubItems[0].Text;
            cityCodeData = citiesListView.Items[index].SubItems[1].Text;
            citySatData = citiesListView.Items[index].SubItems[2].Text;
            cityTempData = citiesListView.Items[index].SubItems[3].Text;
            cityUVData = citiesListView.Items[index].SubItems[4].Text;
            cityWindsData = citiesListView.Items[index].SubItems[5].Text;
            cityHumidData = citiesListView.Items[index].SubItems[6].Text;
            cityPrecipData = citiesListView.Items[index].SubItems[7].Text;
          }
          xmlwriter.SetValue("weather", cityName, cityNameData);
          xmlwriter.SetValue("weather", cityCode, cityCodeData);
          xmlwriter.SetValue("weather", citySat, citySatData);
          xmlwriter.SetValue("weather", cityTemp, cityTempData);
          xmlwriter.SetValue("weather", cityUV, cityUVData);
          xmlwriter.SetValue("weather", cityWinds, cityWindsData);
          xmlwriter.SetValue("weather", cityHumid, cityHumidData);
          xmlwriter.SetValue("weather", cityPrecip, cityPrecipData);
        }
      }
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.intervalTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.windSpeedComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.temperatureComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.searchButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.removeButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.citiesListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.intervalTextBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.windSpeedComboBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.temperatureComboBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 104);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // intervalTextBox
      // 
      this.intervalTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.intervalTextBox.Location = new System.Drawing.Point(168, 68);
      this.intervalTextBox.Name = "intervalTextBox";
      this.intervalTextBox.Size = new System.Drawing.Size(288, 20);
      this.intervalTextBox.TabIndex = 5;
      this.intervalTextBox.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(150, 16);
      this.label3.TabIndex = 4;
      this.label3.Text = "Refresh interval (minutes):";
      // 
      // windSpeedComboBox
      // 
      this.windSpeedComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.windSpeedComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.windSpeedComboBox.Location = new System.Drawing.Point(168, 44);
      this.windSpeedComboBox.Name = "windSpeedComboBox";
      this.windSpeedComboBox.Size = new System.Drawing.Size(288, 21);
      this.windSpeedComboBox.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(120, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Wind speed shown as:";
      // 
      // temperatureComboBox
      // 
      this.temperatureComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.temperatureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.temperatureComboBox.Location = new System.Drawing.Point(168, 20);
      this.temperatureComboBox.Name = "temperatureComboBox";
      this.temperatureComboBox.Size = new System.Drawing.Size(288, 21);
      this.temperatureComboBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(120, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Temperature shown in:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.editButton);
      this.mpGroupBox1.Controls.Add(this.searchButton);
      this.mpGroupBox1.Controls.Add(this.removeButton);
      this.mpGroupBox1.Controls.Add(this.citiesListView);
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 112);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 240);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Cities";
      // 
      // editButton
      // 
      this.editButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Enabled = false;
      this.editButton.Location = new System.Drawing.Point(384, 208);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(72, 22);
      this.editButton.TabIndex = 3;
      this.editButton.Text = "Edit";
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // searchButton
      // 
      this.searchButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.searchButton.Location = new System.Drawing.Point(224, 208);
      this.searchButton.Name = "searchButton";
      this.searchButton.Size = new System.Drawing.Size(72, 22);
      this.searchButton.TabIndex = 1;
      this.searchButton.Text = "Add";
      this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
      // 
      // removeButton
      // 
      this.removeButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.removeButton.Enabled = false;
      this.removeButton.Location = new System.Drawing.Point(304, 208);
      this.removeButton.Name = "removeButton";
      this.removeButton.Size = new System.Drawing.Size(72, 22);
      this.removeButton.TabIndex = 2;
      this.removeButton.Text = "Remove";
      this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
      // 
      // citiesListView
      // 
      this.citiesListView.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.citiesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                             {
                                               this.columnHeader1,
                                               this.columnHeader2,
                                               this.columnHeader3,
                                               this.columnHeader4,
                                               this.columnHeader5,
                                               this.columnHeader6,
                                               this.columnHeader7,
                                               this.columnHeader8
                                             });
      this.citiesListView.FullRowSelect = true;
      this.citiesListView.Location = new System.Drawing.Point(16, 24);
      this.citiesListView.Name = "citiesListView";
      this.citiesListView.Size = new System.Drawing.Size(440, 176);
      this.citiesListView.TabIndex = 0;
      this.citiesListView.View = System.Windows.Forms.View.Details;
      this.citiesListView.SelectedIndexChanged += new System.EventHandler(this.citiesListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "City";
      this.columnHeader1.Width = 125;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Code";
      this.columnHeader2.Width = 78;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Satellite image";
      this.columnHeader3.Width = 181;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Temperature image";
      this.columnHeader4.Width = 201;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "UV Index image";
      this.columnHeader5.Width = 201;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Winds Image";
      this.columnHeader6.Width = 201;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Humidity image";
      this.columnHeader7.Width = 201;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "Precipitation image";
      this.columnHeader8.Width = 101;
      // 
      // Weather
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "Weather";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// Search for a city from weather.com
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void searchButton_Click(object sender, EventArgs e)
    {
      SearchCityForm searchForm = new SearchCityForm();
      // Show dialog
      if (searchForm.ShowDialog(this) == DialogResult.OK)
      {
        // Fetch selected cities
        ArrayList cities = searchForm.SelectedCities;
        foreach (WeatherChannel.City city in cities)
        {
          citiesListView.Items.Add(
            new ListViewItem(new string[]
                               {
                                 city.Name, city.Id, searchForm.SatteliteImage, searchForm.TemperatureImage,
                                 searchForm.UVIndexImage, searchForm.WindsImage, searchForm.HumidityImage,
                                 searchForm.PrecipitationImage
                               }));
        }
      }
    }

    private void removeButton_Click(object sender, EventArgs e)
    {
      int numberItems = citiesListView.SelectedItems.Count;
      for (int index = 0; index < numberItems; index++)
      {
        citiesListView.Items.RemoveAt(citiesListView.SelectedIndices[0]);
      }
    }

    private void editButton_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem listItem in citiesListView.SelectedItems)
      {
        EditWeatherCityForm cityForm = new EditWeatherCityForm();
        // Set current image location
        cityForm.SatteliteImage = listItem.SubItems[2].Text;
        cityForm.TemperatureImage = listItem.SubItems[3].Text;
        cityForm.UVIndexImage = listItem.SubItems[4].Text;
        cityForm.WindsImage = listItem.SubItems[5].Text;
        cityForm.HumidityImage = listItem.SubItems[6].Text;
        cityForm.PrecipitationImage = listItem.SubItems[7].Text;
        DialogResult dialogResult = cityForm.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
          // Fetch selected image location
          listItem.SubItems[2].Text = cityForm.SatteliteImage;
          listItem.SubItems[3].Text = cityForm.TemperatureImage;
          listItem.SubItems[4].Text = cityForm.UVIndexImage;
          listItem.SubItems[5].Text = cityForm.WindsImage;
          listItem.SubItems[6].Text = cityForm.HumidityImage;
          listItem.SubItems[7].Text = cityForm.PrecipitationImage;
        }
      }
    }

    private void citiesListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      editButton.Enabled = removeButton.Enabled = (citiesListView.SelectedItems.Count > 0);
    }
  }
}