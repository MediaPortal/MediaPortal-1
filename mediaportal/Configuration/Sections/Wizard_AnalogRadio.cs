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
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using DShowNET;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_AnalogRadio : Wizard_ScanBase
  {
    private MPGroupBox groupBox1;
    private MPGroupBox groupBox2;
    private MPGroupBox groupBox3;
    private MPGroupBox groupBox4;
    private MPLabel label1;
    private MPLabel label2;
    private MPComboBox cbCountry;

    private MPLabel mpLabel2;
    private MPLabel mpLabel1;
    private MPLabel mpLabel3;
    private MPLabel mpLabel4;
    private MPLabel mpLabel5;
    private MPLabel mpLabel6;
    private MPComboBox cbInput;
    private MPComboBox cbCities;
    private MPButton buttonImport;
    private MPComboBox sensitivityComboBox;

    private MPListView listView1;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ImageList imageList1;


    private bool _groupBox2Enabled = true;
    private bool _loadingInfo = false;
    private XmlDocument docSetup;


    public Wizard_AnalogRadio()
      : this("Analog TV")
    {
    }

    public Wizard_AnalogRadio(string name)
      : base(name)
    {
      Radio = true;
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (Wizard_AnalogRadio));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.lblStatus2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarQuality = new System.Windows.Forms.ProgressBar();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarStrength = new System.Windows.Forms.ProgressBar();
      this.lblStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBarProgress = new System.Windows.Forms.ProgressBar();
      this.buttonScan = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbCities = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbInput = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.sensitivityComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.listView1);
      this.groupBox1.Controls.Add(this.lblStatus2);
      this.groupBox1.Controls.Add(this.mpLabel3);
      this.groupBox1.Controls.Add(this.progressBarQuality);
      this.groupBox1.Controls.Add(this.mpLabel2);
      this.groupBox1.Controls.Add(this.mpLabel1);
      this.groupBox1.Controls.Add(this.progressBarStrength);
      this.groupBox1.Controls.Add(this.lblStatus);
      this.groupBox1.Controls.Add(this.progressBarProgress);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.groupBox3);
      this.groupBox1.Controls.Add(this.groupBox4);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(5, 5);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(532, 391);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Setup Analog Radio";
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                        {
                                          this.columnHeader1,
                                          this.columnHeader2,
                                          this.columnHeader3
                                        });
      this.listView1.Location = new System.Drawing.Point(16, 205);
      this.listView1.MultiSelect = false;
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(380, 183);
      this.listView1.TabIndex = 21;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.LargeImageList = this.imageList1;
      this.listView1.ShowItemToolTips = true;
      this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      this.listView1.ItemActivate += new EventHandler(this.listView1_ItemActivate);
      this.listView1.KeyUp += new KeyEventHandler(this.listView1_KeyUp);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Station";
      this.columnHeader1.Width = 100;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Frequency";
      this.columnHeader2.Width = 50;
      // 
      // columnHeader2
      // 
      this.columnHeader3.Text = "Channel";
      this.columnHeader3.Width = 50;
      // 
      // imageList1
      // 

      this.imageList1.ImageStream =
        ((System.Windows.Forms.ImageListStreamer) (resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "");
      // 
      // lblStatus2
      // 
      this.lblStatus2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular,
                                                     System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.lblStatus2.Location = new System.Drawing.Point(16, 185);
      this.lblStatus2.Name = "lblStatus2";
      this.lblStatus2.Size = new System.Drawing.Size(415, 17);
      this.lblStatus2.TabIndex = 18;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(16, 100);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(87, 16);
      this.mpLabel3.TabIndex = 17;
      this.mpLabel3.Text = "Progress:";
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
      this.progressBarQuality.Location = new System.Drawing.Point(114, 140);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(325, 16);
      this.progressBarQuality.Step = 1;
      this.progressBarQuality.TabIndex = 16;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(16, 142);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(87, 14);
      this.mpLabel2.TabIndex = 15;
      this.mpLabel2.Text = "Signal quality:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 121);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(87, 16);
      this.mpLabel1.TabIndex = 14;
      this.mpLabel1.Text = "Signal strength:";
      // 
      // progressBarStrength
      // 
      this.progressBarStrength.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
      this.progressBarStrength.Location = new System.Drawing.Point(114, 121);
      this.progressBarStrength.Name = "progressBarStrength";
      this.progressBarStrength.Size = new System.Drawing.Size(325, 16);
      this.progressBarStrength.Step = 1;
      this.progressBarStrength.TabIndex = 13;
      // 
      // lblStatus
      // 
      this.lblStatus.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular,
                                                    System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.lblStatus.Location = new System.Drawing.Point(16, 161);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(415, 17);
      this.lblStatus.TabIndex = 5;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
      this.progressBarProgress.Location = new System.Drawing.Point(114, 100);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(325, 16);
      this.progressBarProgress.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(510, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Select your country and input source and press \"Scan\" to scan for Radio Stations. Or" +
                         " select your city and press \"Download\" to import Radio Stations from MediaPortal We" +
                         "b Site.";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.cbCities);
      this.groupBox2.Controls.Add(this.mpLabel5);
      this.groupBox2.Controls.Add(this.buttonImport);
      this.groupBox2.Enabled = false;
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(404, 205);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(122, 108);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Radio Download";
      // 
      // cbCities
      // 
      this.cbCities.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCities.Items.AddRange(new object[]
                                     {
                                       "Loading..."
                                     });
      this.cbCities.Location = new System.Drawing.Point(5, 45);
      this.cbCities.Name = "cbCities";
      this.cbCities.Size = new System.Drawing.Size(111, 21);
      this.cbCities.TabIndex = 2;
      // 
      // mpLabel5
      // 
      this.mpLabel5.Location = new System.Drawing.Point(6, 25);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(88, 17);
      this.mpLabel5.TabIndex = 19;
      this.mpLabel5.Text = "City:";
      // 
      // buttonImport
      // 
      this.buttonImport.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonImport.Location = new System.Drawing.Point(5, 72);
      this.buttonImport.Name = "buttonImport";
      this.buttonImport.Size = new System.Drawing.Size(72, 22);
      this.buttonImport.TabIndex = 3;
      this.buttonImport.Text = "Download";
      this.buttonImport.UseVisualStyleBackColor = true;
      this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click_1);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left))));
      this.groupBox3.Controls.Add(this.cbCountry);
      this.groupBox3.Controls.Add(this.mpLabel4);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.cbInput);
      this.groupBox3.Enabled = true;
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(16, 48);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(420, 46);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Radio Tuner";
      // 
      // cbCountry
      // 
      this.cbCountry.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
      this.cbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountry.Location = new System.Drawing.Point(76, 16);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(113, 21);
      this.cbCountry.TabIndex = 1;
      this.cbCountry.SelectedIndexChanged += new System.EventHandler(this.cbCountries_SelectedIndexChanged_1);
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new System.Drawing.Point(209, 20);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(88, 17);
      this.mpLabel4.TabIndex = 19;
      this.mpLabel4.Text = "Input Source:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(6, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(67, 17);
      this.label2.TabIndex = 1;
      this.label2.Text = "Country:";
      // 
      // cbInput
      // 
      this.cbInput.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
      this.cbInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbInput.Items.AddRange(new object[]
                                    {
                                      "Antenna",
                                      "Cable"
                                    });
      this.cbInput.Location = new System.Drawing.Point(301, 17);
      this.cbInput.Name = "cbInput";
      this.cbInput.Size = new System.Drawing.Size(113, 21);
      this.cbInput.TabIndex = 2;
      this.cbInput.SelectedIndexChanged += new System.EventHandler(this.cbInput_SelectedIndexChanged_1);
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
      this.groupBox4.Controls.Add(this.buttonScan);
      this.groupBox4.Controls.Add(this.mpLabel6);
      this.groupBox4.Controls.Add(this.sensitivityComboBox);
      this.groupBox4.Enabled = true;
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(446, 48);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(82, 95);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Scan";
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
      this.buttonScan.Location = new System.Drawing.Point(5, 14);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(72, 22);
      this.buttonScan.TabIndex = 3;
      this.buttonScan.Text = "Scan";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // mpLabel6
      // 
      this.mpLabel6.Location = new System.Drawing.Point(4, 46);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(60, 17);
      this.mpLabel6.TabIndex = 19;
      this.mpLabel6.Text = "Sensitivity:";
      // 
      // sensitivityComboBox
      // 
      this.sensitivityComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
      this.sensitivityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.sensitivityComboBox.Items.AddRange(new object[]
                                                {
                                                  "High",
                                                  "Medium",
                                                  "Low"
                                                });
      this.sensitivityComboBox.SelectedIndex = 0;
      this.sensitivityComboBox.Location = new System.Drawing.Point(5, 68);
      this.sensitivityComboBox.Name = "sensitivityComboBox";
      this.sensitivityComboBox.Size = new System.Drawing.Size(72, 21);
      this.sensitivityComboBox.TabIndex = 3;
      this.sensitivityComboBox.SelectedIndexChanged +=
        new System.EventHandler(this.sensitivityComboBox_SelectedIndexChanged);
      // 
      // Wizard_AnalogTV
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Wizard_AnalogRadio";
      this.Size = new System.Drawing.Size(545, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_card == null)
      {
        Hashtable devices = TVCaptureDevice.GetTVCaptureDevices();
        IEnumerator enumerator = devices.Values.GetEnumerator();
        while (enumerator.MoveNext())
        {
          TVCaptureDevice dev = (TVCaptureDevice) enumerator.Current;
          if ((dev.Network == NetworkType.Analog) && (dev.SupportsRadio))
          {
            _card = dev;
            break;
          }
        }
        if (_card == null)
        {
          TVCaptureCards cards = new TVCaptureCards();
          cards.LoadCaptureCards();
          foreach (TVCaptureDevice dev in cards.captureCards)
          {
            if ((dev.Network == NetworkType.Analog) && (dev.SupportsRadio))
            {
              _card = dev;
              break;
            }
          }
        }
      }
      this.cbCountry.Items.AddRange(TunerCountries.Countries);
      this.cbCities.Items.Add("Loading...");
      this.cbCities.SelectedIndex = 0;

      LoadSettings();
      UpdateList();
      DownloadCities();
      this.OnScanFinished += new ScanFinishedHandler(this.dlg_OnScanFinished);
      this.OnScanStarted += new ScanStartedHandler(this.dlg_OnScanStarted);
    }

    protected override String[] GetScanParameters()
    {
      return null;
    }

    private void dlg_OnScanFinished(object sender, EventArgs args)
    {
      groupBox3.Enabled = true;
      listView1.Enabled = true;
      groupBox2.Enabled = _groupBox2Enabled;
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(false);
        wizard.DisableNext(false);
      }
      MapRadioToOtherCards(_card.ID);
    }

    private void dlg_OnScanStarted(object sender, EventArgs args)
    {
      groupBox3.Enabled = false;
      _groupBox2Enabled = groupBox2.Enabled;
      groupBox2.Enabled = false;
      listView1.Enabled = false;
      int countryId = 31;
      if (cbCountry.SelectedItem != null)
      {
        TunerCountry tunerCountry = cbCountry.SelectedItem as TunerCountry;
        countryId = tunerCountry.Id;
      }
      _card.DefaultCountryCode = countryId;
      Boolean isCableInput = false;
      if (!cbInput.Text.Equals("Antenna"))
      {
        isCableInput = true;
      }
      _card.IsCableInput = isCableInput;
      int Sensitivity = 1;
      switch (sensitivityComboBox.Text)
      {
        case "High":
          Sensitivity = 10;
          break;

        case "Medium":
          Sensitivity = 2;
          break;

        case "Low":
          Sensitivity = 1;
          break;
      }
      _card.RadioSensitivity = Sensitivity;

      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(true);
        wizard.DisableNext(true);
      }
    }

    private void DownloadCities()
    {
      Thread thread = new Thread(new ThreadStart(LoadXml));
      thread.IsBackground = true;
      thread.Start();
    }

    private void LoadXml()
    {
      try
      {
        _loadingInfo = true;
        this.cbCities.Items.Clear();
        this.cbCities.Items.AddRange(new object[]
                                       {
                                         "Loading..."
                                       });
        cbCities.SelectedIndex = 0;
        cbCities.Update();
        docSetup = new XmlDocument();
        docSetup.Load("http://www.team-mediaportal.com/tvsetup/setup.xml");
        _loadingInfo = false;
        FillInCities();
      }
      catch (Exception)
      {
        this.cbCities.Items.Clear();
        this.cbCities.Items.AddRange(new object[]
                                       {
                                         "No info available"
                                       });
        cbCities.SelectedIndex = 0;
        cbCities.Update();
        docSetup = null;
        MessageBox.Show("Cannot connect to MediaPortal Web Site", "MediaPortal Settings", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        return;
      }
    }

    private void FillInCities()
    {
      if (docSetup == null)
      {
        if (!_loadingInfo)
        {
          cbCities.Items.Clear();
          cbCities.Items.Add("No info available");
          cbCities.SelectedIndex = 0;
          cbCities.Update();
          groupBox2.Enabled = false;
        }
        return;
      }
      String countryName = "";
      if (cbCountry.SelectedItem != null)
      {
        countryName = cbCountry.SelectedItem.ToString();
      }
      XPathNavigator nav = docSetup.CreateNavigator();
      // Ensure we are at the root node
      nav.MoveToRoot();
      XPathExpression expr = nav.Compile("/mediaportal/country[@name='" + countryName + "']");
      XPathNavigator countryNav = nav.SelectSingleNode(expr);
      if (countryNav != null)
      {
        cbCities.Items.Clear();
        XmlNode nodeCountry = ((IHasXmlNode) countryNav).GetNode();
        XmlNodeList listCities = nodeCountry.SelectNodes("city");
        foreach (XmlNode nodeCity in listCities)
        {
          XmlNode listCitiesName = nodeCity.Attributes.GetNamedItem("name");
          cbCities.Items.Add(listCitiesName.Value);
        }
        if (cbCities.Items.Count > 0 && cbCities.SelectedIndex < 0)
        {
          cbCities.SelectedIndex = 0;
        }
        if (this.Scanning)
        {
          _groupBox2Enabled = true;
        }
        else
        {
          groupBox2.Enabled = true;
        }
      }
      else
      {
        cbCities.Items.Clear();
        cbCities.Items.Add("No info available");
        cbCities.SelectedIndex = 0;
        groupBox2.Enabled = false;
      }
    }

    private void buttonImport_Click_1(object sender, EventArgs e)
    {
      string country = cbCountry.SelectedItem.ToString();
      string city = (string) cbCities.SelectedItem;
      XPathNavigator nav = docSetup.CreateNavigator();
      // Ensure we are at the root node
      nav.MoveToRoot();
      XPathExpression expr = nav.Compile("/mediaportal/country[@name='" + country + "']/city[@name=\"" + city + "\"]");
      XPathNavigator cityNav = nav.SelectSingleNode(expr);
      if (cityNav != null)
      {
        XmlNode nodeCity = ((IHasXmlNode) cityNav).GetNode();
        XmlNode nodeAnalog = nodeCity.SelectSingleNode("analog");
        int newRadioChannels;
        int updatedRadioChannels;
        OnStatus2("");
        Cursor.Current = Cursors.WaitCursor;
        buttonImport.Enabled = false;
        ImportAnalogChannels(nodeAnalog.InnerText, out newRadioChannels, out updatedRadioChannels);
        OnStatus2(String.Format("Imported {0} new radio stations, {1} updated radio stations", newRadioChannels,
                                updatedRadioChannels));
        MapRadioToOtherCards(_card.ID);
        buttonImport.Enabled = true;
        Cursor.Current = Cursors.Default;
        UpdateList();
      }
      else
      {
        OnStatus2("");
      }
    }

    private void ImportAnalogChannels(string xmlFile, out int newRadioChannels, out int updatedRadioChannels)
    {
      newRadioChannels = 0;
      updatedRadioChannels = 0;
      try
      {
        XmlDocument doc = new XmlDocument();
        UriBuilder builder = new UriBuilder("http", "www.team-mediaportal.com", 80, "tvsetup/analog/" + xmlFile);
        doc.Load(builder.Uri.AbsoluteUri);
        XmlNodeList listRadioChannels = doc.DocumentElement.SelectNodes("/mediaportal/radio/channel");
        foreach (XmlNode nodeChannel in listRadioChannels)
        {
          XmlNode name = nodeChannel.Attributes.GetNamedItem("name");
          XmlNode frequency = nodeChannel.Attributes.GetNamedItem("frequency");
          string stationName = name.Value;
          long stationFrequency = 0;
          try
          {
            stationFrequency = ConvertToFrequency(frequency.Value);
          }
          catch (Exception)
          {
          }

          MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
          if (!RadioDatabase.GetStation(stationName, out station))
          {
            //doesn't exists
            //then add a new station to the database
            station.Scrambled = false;
            station.Name = stationName;
            station.Frequency = stationFrequency;
            station.Sort = 40000;
            station.Channel = GetUniqueNumber();
            Log.Info("Wizard_AnalogRadio: add new station for {0}:{1}", station.Name, station.Frequency);
            int id = RadioDatabase.AddStation(ref station);
            if (id < 0)
            {
              Log.Error("Wizard_AnalogRadio: failed to add new station for {0}:{1} to database", station.Name,
                        station.Frequency);
            }
            newRadioChannels++;
          }
          else
          {
            station.Name = stationName;
            station.Frequency = stationFrequency;
            RadioDatabase.UpdateStation(station);
            updatedRadioChannels++;
            Log.Info("Wizard_AnalogRadio: update station {0}:{1} {2}", station.Name, station.Frequency, station.ID);
          }
          RadioDatabase.MapChannelToCard(station.ID, _card.ID);
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Cannot connect to MediaPortal Web Site", "MediaPortal Settings", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }
    }

    private int GetUniqueNumber()
    {
      ArrayList stations = new ArrayList();
      RadioDatabase.GetStations(ref stations);
      int number = 1;
      while (true)
      {
        bool unique = true;
        foreach (MediaPortal.Radio.Database.RadioStation station in stations)
        {
          if (station.Channel == number)
          {
            unique = false;
            break;
          }
        }
        if (!unique)
        {
          number++;
        }
        else
        {
          return number;
        }
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

    private void cbCountries_SelectedIndexChanged_1(object sender, EventArgs e)
    {
      if ((cbCountry.SelectedItem != null) && (_card != null))
      {
        TunerCountry tunerCountry = cbCountry.SelectedItem as TunerCountry;
        _card.DefaultCountryCode = tunerCountry.Id;
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("capture", "countryname", tunerCountry.Country);
          xmlwriter.SetValue("capture", "country", tunerCountry.Id.ToString());
        }
        if (listView1.SelectedIndices.Count > 0)
        {
          _card.DeleteGraph();
          string selectedChannel = listView1.SelectedItems[0].Text;
          MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
          if (RadioDatabase.GetStation(selectedChannel, out station))
          {
            _card.StartRadio(station);
          }
        }
      }
      FillInCities();
    }

    private void cbInput_SelectedIndexChanged_1(object sender, EventArgs e)
    {
      if ((cbInput.SelectedItem != null) && (_card != null))
      {
        Boolean isCableInput = false;
        if (!cbInput.Text.Equals("Antenna"))
        {
          isCableInput = true;
        }
        _card.IsCableInput = isCableInput;
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("capture", "radiotuner", cbInput.Text);
        }
        if (listView1.SelectedIndices.Count > 0)
        {
          _card.DeleteGraph();
          string selectedChannel = listView1.SelectedItems[0].Text;
          MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
          if (RadioDatabase.GetStation(selectedChannel, out station))
          {
            _card.StartRadio(station);
          }
        }
      }
    }

    private void sensitivityComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((sensitivityComboBox.SelectedItem != null) && (_card != null))
      {
        int Sensitivity = 1;
        switch (sensitivityComboBox.Text)
        {
          case "High":
            Sensitivity = 10;
            break;

          case "Medium":
            Sensitivity = 2;
            break;

          case "Low":
            Sensitivity = 1;
            break;
        }
        _card.RadioSensitivity = Sensitivity;
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        string selectedChannel = listView1.SelectedItems[0].Text;
        listView1.Update();
        MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
        if (RadioDatabase.GetStation(selectedChannel, out station))
        {
          if (!_card.IsRadio)
          {
            _card.DeleteGraph();
          }
          _card.StartRadio(station);
        }
      }
    }

    private void listView1_KeyUp(Object o, KeyEventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Back)
        {
          string selectedChannel = listView1.SelectedItems[0].Text;
          RadioDatabase.RemoveStation(selectedChannel);
          int index = listView1.SelectedItems[0].Index;
          listView1.Items.Remove(listView1.SelectedItems[0]);
          listView1.Update();
          if (listView1.Items.Count > 0)
          {
            if (index >= listView1.Items.Count)
            {
              index = listView1.Items.Count - 1;
            }
            listView1.SelectedIndices.Clear();
            listView1.SelectedIndices.Add(index);
          }
          else
          {
            _card.DeleteGraph();
          }
        }
      }
    }

    private void listView1_ItemActivate(object sender, EventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        string selectedChannel = listView1.SelectedItems[0].Text;
        ListViewItem listItem = listView1.SelectedItems[0];
        MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
        if (RadioDatabase.GetStation(selectedChannel, out station))
        {
          RadioStation stationToEdit = new RadioStation();
          stationToEdit.ID = station.ID;
          stationToEdit.Genre = station.Genre;
          stationToEdit.Frequency = station.Frequency;
          stationToEdit.Bitrate = station.BitRate;
          stationToEdit.Name = station.Name;
          stationToEdit.Scrambled = station.Scrambled;
          stationToEdit.Type = "Radio";
          stationToEdit.URL = station.URL;
          EditRadioStationForm editStation = new EditRadioStationForm();
          editStation.Station = stationToEdit;
          DialogResult dialogResult = editStation.ShowDialog(this);

          if (dialogResult == DialogResult.OK)
          {
            //
            // Remove URL if we have a normal radio station
            //
            if (editStation.Station.Type.Equals("Radio"))
            {
              editStation.Station.URL = string.Empty;
            }

            listItem.SubItems[0].Text = editStation.Station.Name;
            listItem.SubItems[1].Text = editStation.Station.Frequency.ToString(Frequency.Format.MegaHertz);

            station.Scrambled = editStation.Station.Scrambled;
            station.ID = editStation.Station.ID;
            station.Name = editStation.Station.Name;
            station.Genre = editStation.Station.Genre;
            station.BitRate = editStation.Station.Bitrate;
            station.URL = editStation.Station.URL;
            long currentTunedFrequency = station.Frequency;
            station.Frequency = editStation.Station.Frequency.Hertz;
            if (station.Frequency < 1000)
            {
              station.Frequency *= 1000000L;
            }
            RadioDatabase.UpdateStation(station);
            listView1.Update();
            if (station.Frequency != currentTunedFrequency)
            {
              _card.DeleteGraph();
              _card.StartRadio(station);
            }
          }
        }
      }
    }

    private void MapRadioToOtherCards(int id)
    {
      ArrayList radiochannels = new ArrayList();
      RadioDatabase.GetStationsForCard(ref radiochannels, id);
      TVCaptureCards cards = new TVCaptureCards();
      cards.LoadCaptureCards();
      foreach (TVCaptureDevice dev in cards.captureCards)
      {
        if ((dev.Network == NetworkType.Analog) && (dev.ID != id) && (dev.SupportsRadio))
        {
          foreach (MediaPortal.Radio.Database.RadioStation station in radiochannels)
          {
            RadioDatabase.MapChannelToCard(station.ID, dev.ID);
          }
        }
      }
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbInput.SelectedItem = xmlreader.GetValueAsString("capture", "radiotuner", "Antenna");
        string countryName = xmlreader.GetValueAsString("capture", "countryname", "The Netherlands");
        int countryId = xmlreader.GetValueAsInt("capture", "country", 31);

        TunerCountry country = TunerCountries.GetTunerCountry(countryName);
        if (country == null)
        {
          country = TunerCountries.GetTunerCountryFromID(countryId);
        }
        cbCountry.SelectedItem = country;
      }
    }


    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("capture", "radiotuner", cbInput.Text);
        if (cbCountry.Text.Length > 0)
        {
          TunerCountry tunerCountry = cbCountry.SelectedItem as TunerCountry;
          xmlwriter.SetValue("capture", "countryname", tunerCountry.Country);
          xmlwriter.SetValue("capture", "country", tunerCountry.Id.ToString());
        }
      }
    }

    public override void UpdateList()
    {
      listView1.Items.Clear();
      if (_card == null)
      {
        return;
      }
      ArrayList stations = new ArrayList();
      RadioDatabase.GetStationsForCard(ref stations, _card.ID);
      foreach (MediaPortal.Radio.Database.RadioStation station in stations)
      {
        ListViewItem item = new ListViewItem();
        item.ImageIndex = 0;
        //                  if (station.Scrambled)
        //                    item.ImageIndex = 1;
        item.Text = station.Name;
        float frequency = ((float) station.Frequency)/1000000;
        string description = String.Format("{0:###.##} MHz.", frequency);
        item.SubItems.Add(description);
        item.SubItems.Add(station.Channel.ToString());
        listView1.Items.Add(item);
      }
      RadioStations.UpdateList();
    }
  }
}