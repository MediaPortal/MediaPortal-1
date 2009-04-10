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
using System.Xml;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for TvChannelLookupService.
  /// </summary>
  public class TvChannelLookupService : MPConfigForm
  {
    private MPLabel label1;
    private MPLabel label2;
    private MPButton button1;
    private MPComboBox cbCountries;
    private MPComboBox cbCities;
    private XmlDocument docSetup;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public TvChannelLookupService()
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
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbCountries = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbCities = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.TabIndex = 0;
      this.label1.Text = "Country:";
      // 
      // cbCountries
      // 
      this.cbCountries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountries.Location = new System.Drawing.Point(80, 16);
      this.cbCountries.Name = "cbCountries";
      this.cbCountries.Size = new System.Drawing.Size(280, 21);
      this.cbCountries.Sorted = true;
      this.cbCountries.TabIndex = 1;
      this.cbCountries.SelectedIndexChanged += new System.EventHandler(this.cbCountries_SelectedIndexChanged);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.TabIndex = 2;
      this.label2.Text = "City:";
      // 
      // cbCities
      // 
      this.cbCities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCities.Location = new System.Drawing.Point(80, 48);
      this.cbCities.Name = "cbCities";
      this.cbCities.Size = new System.Drawing.Size(280, 21);
      this.cbCities.Sorted = true;
      this.cbCities.TabIndex = 3;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(288, 88);
      this.button1.Name = "button1";
      this.button1.TabIndex = 4;
      this.button1.Text = "Import";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // TvChannelLookupService
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(384, 134);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.cbCities);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cbCountries);
      this.Controls.Add(this.label1);
      this.Name = "TvChannelLookupService";
      this.Text = "TV Channel Lookup Service";
      this.Load += new System.EventHandler(this.TvChannelLookupService_Load);
      this.ResumeLayout(false);
    }

    #endregion

    private void TvChannelLookupService_Load(object sender, EventArgs e)
    {
      cbCountries.Items.Clear();
      cbCities.Items.Clear();
      docSetup = new XmlDocument();
      docSetup.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
      XmlNodeList listCountries = docSetup.DocumentElement.SelectNodes("/mediaportal/country");
      foreach (XmlNode nodeCountry in listCountries)
      {
        XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
        cbCountries.Items.Add(nodeCountryName.Value);
      }

      if (cbCountries.Items.Count > 0 && cbCountries.SelectedIndex < 0)
      {
        cbCountries.SelectedIndex = 0;
      }
      FillInCities();
    }

    private void FillInCities()
    {
      string country = (string) cbCountries.SelectedItem;
      cbCities.Items.Clear();
      XmlNodeList listCountries = docSetup.DocumentElement.SelectNodes("/mediaportal/country");
      foreach (XmlNode nodeCountry in listCountries)
      {
        XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
        if (nodeCountryName.Value == country)
        {
          XmlNodeList listCities = nodeCountry.SelectNodes("city");
          foreach (XmlNode nodeCity in listCities)
          {
            XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
            cbCities.Items.Add(listCitiesName.Value);
          }
          break;
        }
      }
      if (cbCities.Items.Count > 0 && cbCities.SelectedIndex < 0)
      {
        cbCities.SelectedIndex = 0;
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      string country = (string) cbCountries.SelectedItem;
      string city = (string) cbCities.SelectedItem;
      XmlDocument doc = new XmlDocument();
      doc.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
      XmlNodeList listCountries = doc.DocumentElement.SelectNodes("/mediaportal/country");
      foreach (XmlNode nodeCountry in listCountries)
      {
        XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
        if (nodeCountryName.Value == country)
        {
          XmlNodeList listCities = nodeCountry.SelectNodes("city");
          foreach (XmlNode nodeCity in listCities)
          {
            XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
            if (listCitiesName.Value == city)
            {
              XmlNode nodeAnalog = nodeCity.SelectSingleNode("analog");
              ImportAnalogChannels(nodeAnalog.InnerText);
              this.Close();
              return;
            }
          }
        }
      }
    }

    private void ImportAnalogChannels(string xmlFile)
    {
      XmlDocument doc = new XmlDocument();
      UriBuilder builder = new UriBuilder("http", "www.team-mediaportal.com", 80, "tvsetup/analog/" + xmlFile);
      doc.Load(builder.Uri.AbsoluteUri);
      XmlNodeList listTvChannels = doc.DocumentElement.SelectNodes("/mediaportal/tv/channel");
      foreach (XmlNode nodeChannel in listTvChannels)
      {
        XmlNode name = nodeChannel.Attributes.GetNamedItem("name");
        XmlNode number = nodeChannel.Attributes.GetNamedItem("number");
        XmlNode frequency = nodeChannel.Attributes.GetNamedItem("frequency");
        TVChannel chan = new TVChannel();
        chan.Name = name.Value;
        chan.Frequency = 0;
        try
        {
          chan.Number = Int32.Parse(number.Value);
        }
        catch (Exception)
        {
        }
        try
        {
          chan.Frequency = ConvertToTvFrequency(frequency.Value, ref chan);
        }
        catch (Exception)
        {
        }
        TVDatabase.AddChannel(chan);
      }
      XmlNodeList listRadioChannels = doc.DocumentElement.SelectNodes("/mediaportal/radio/channel");
      foreach (XmlNode nodeChannel in listRadioChannels)
      {
        XmlNode name = nodeChannel.Attributes.GetNamedItem("name");
        XmlNode frequency = nodeChannel.Attributes.GetNamedItem("frequency");
        MediaPortal.Radio.Database.RadioStation chan = new MediaPortal.Radio.Database.RadioStation();
        chan.Name = name.Value;
        chan.Frequency = ConvertToFrequency(frequency.Value);
        RadioDatabase.AddStation(ref chan);
      }
    }

    private long ConvertToFrequency(string frequency)
    {
      if (frequency.Trim() == string.Empty)
      {
        return 0;
      }
      float testValue = 189.24f;
      string usage = testValue.ToString("f2");
      if (usage.IndexOf(".") >= 0)
      {
        frequency = frequency.Replace(",", ".");
      }
      if (usage.IndexOf(",") >= 0)
      {
        frequency = frequency.Replace(".", ",");
      }
      double freqValue = Convert.ToDouble(frequency);
      freqValue *= 1000000;
      return (long) (freqValue);
    }


    private long ConvertToTvFrequency(string frequency, ref TVChannel chan)
    {
      if (frequency.Trim() == string.Empty)
      {
        return 0;
      }
      chan.Number = TVDatabase.FindFreeTvChannelNumber(chan.Number);
      frequency = frequency.ToUpper();
      for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
      {
        if (frequency.Equals(TVChannel.SpecialChannels[i].Name))
        {
          return TVChannel.SpecialChannels[i].Frequency;
        }
      }

      float testValue = 189.24f;
      string usage = testValue.ToString("f2");
      if (usage.IndexOf(".") >= 0)
      {
        frequency = frequency.Replace(",", ".");
      }
      if (usage.IndexOf(",") >= 0)
      {
        frequency = frequency.Replace(".", ",");
      }
      double freqValue = Convert.ToDouble(frequency);
      freqValue *= 1000000;
      return (long) (freqValue);
    }

    private void cbCountries_SelectedIndexChanged(object sender, EventArgs e)
    {
      FillInCities();
    }
  }
}