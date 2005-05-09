using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;

namespace MediaPortal.Configuration.Sections
{
	/// <summary>
	/// Summary description for TvChannelLookupService.
	/// </summary>
	public class TvChannelLookupService : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ComboBox cbCountries;
		private System.Windows.Forms.ComboBox cbCities;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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
			this.label1 = new System.Windows.Forms.Label();
			this.cbCountries = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCities = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
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
			this.cbCountries.Size = new System.Drawing.Size(184, 21);
			this.cbCountries.TabIndex = 1;
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
			this.cbCities.Size = new System.Drawing.Size(184, 21);
			this.cbCities.TabIndex = 3;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(192, 88);
			this.button1.Name = "button1";
			this.button1.TabIndex = 4;
			this.button1.Text = "Import";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// TvChannelLookupService
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 158);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.cbCities);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbCountries);
			this.Controls.Add(this.label1);
			this.Name = "TvChannelLookupService";
			this.Text = "TvChannelLookupService";
			this.Load += new System.EventHandler(this.TvChannelLookupService_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void TvChannelLookupService_Load(object sender, System.EventArgs e)
		{
			cbCountries.Items.Clear();
			cbCities.Items.Clear();
			XmlDocument doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList listCountries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				cbCountries.Items.Add(nodeCountryName.Value);
			}
			if (cbCountries.Items.Count> 0 && cbCountries.SelectedIndex<0)
				cbCountries.SelectedIndex=0;
			string country=(string)cbCountries.SelectedItem;
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				if (nodeCountryName.Value==country)
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
			if (cbCities.Items.Count> 0 && cbCities.SelectedIndex<0)
				cbCities.SelectedIndex=0;

		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			string country=(string)cbCountries.SelectedItem;
			string city=(string)cbCities.SelectedItem;
			XmlDocument doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/setup.xml");
			XmlNodeList listCountries = doc.DocumentElement.SelectNodes("/mediaportal/country");
			foreach (XmlNode nodeCountry in listCountries)
			{
				XmlNode nodeCountryName = nodeCountry.Attributes.GetNamedItem("name");
				if (nodeCountryName.Value==country)
				{
					XmlNodeList listCities = nodeCountry.SelectNodes("city");
					foreach (XmlNode nodeCity in listCities)
					{
						XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
						if (listCitiesName.Value==city)
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
		void ImportAnalogChannels(string xmlFile)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load("http://mediaportal.sourceforge.net/tvsetup/analog/"+xmlFile);
			XmlNodeList listTvChannels = doc.DocumentElement.SelectNodes("/mediaportal/tv/channel");
			foreach (XmlNode nodeChannel in listTvChannels)
			{
				XmlNode name					 = nodeChannel.Attributes.GetNamedItem("name");
				XmlNode number				 = nodeChannel.Attributes.GetNamedItem("number");
				XmlNode frequency			 = nodeChannel.Attributes.GetNamedItem("frequency");
				TVChannel chan =new TVChannel();
				chan.Name=name.Value;
				chan.Number=Int32.Parse(number.Value);
				chan.Frequency=ConvertToFrequency(frequency.Value);
				TVDatabase.AddChannel(chan);
			}
			XmlNodeList listRadioChannels = doc.DocumentElement.SelectNodes("/mediaportal/radio/channel");
			foreach (XmlNode nodeChannel in listRadioChannels)
			{
				XmlNode name					 = nodeChannel.Attributes.GetNamedItem("name");
				XmlNode frequency			 = nodeChannel.Attributes.GetNamedItem("frequency");
				MediaPortal.Radio.Database.RadioStation chan =new MediaPortal.Radio.Database.RadioStation();
				chan.Name=name.Value;
				chan.Frequency=ConvertToFrequency(frequency.Value);
				RadioDatabase.AddStation(ref chan);
			}
		}
		long ConvertToFrequency(string frequency)
		{
			float testValue=189.24f;
			string usage=testValue.ToString("f2");
			if (usage.IndexOf(".")>=0) frequency=frequency.Replace(",",".");
			if (usage.IndexOf(",")>=0) frequency=frequency.Replace(".",",");
			double freqValue=Convert.ToDouble(frequency);
			freqValue*=1000000;
			return (long)(freqValue);
		}
	}
}
