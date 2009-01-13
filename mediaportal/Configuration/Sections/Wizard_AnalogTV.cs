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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using DShowNET;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_AnalogTV : Wizard_ScanBase
  {
    private MPGroupBox groupBox1;
    private MPGroupBox groupBox2;
    private MPGroupBox groupBox3;
    private MPLabel label1;
    private MPLabel label2;
    private MPComboBox cbCountry;

    private MPLabel mpLabel2;
    private MPLabel mpLabel1;
    private MPLabel mpLabel3;
    private MPLabel mpLabel4;
    private MPLabel mpLabel5;
    private MPComboBox cbInput;
    private MPComboBox cbCities;
    private MPButton buttonImport;
    private MPButton buttonAdd;
    private MPButton buttonClear;
    private MPContextMenuStrip contextMenuStrip;
    private ImageList imageListContextMenu;


    private Panel panel1;
    private ListView listView1;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ImageList imageList1;
    private const string REORDER = "Reorder";


    private bool _groupBox2Enabled = true;
    private bool _clearButtonEnabled = true;
    private bool _loadingInfo = false;
    private XmlDocument docSetup;

    public Wizard_AnalogTV()
      : this("Analog TV")
    {
      CheckForIllegalCrossThreadCalls = false;
    }

    public Wizard_AnalogTV(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
      CheckForIllegalCrossThreadCalls = false;
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
        new System.ComponentModel.ComponentResourceManager(typeof (Wizard_AnalogTV));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
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
      this.buttonAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonClear = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbInput = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.contextMenuStrip = new MediaPortal.UserInterface.Controls.MPContextMenuStrip();
      this.imageListContextMenu = new System.Windows.Forms.ImageList(this.components);

      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Controls.Add(this.listView1);
      this.groupBox1.Controls.Add(this.lblStatus2);
      this.groupBox1.Controls.Add(this.mpLabel3);
      this.groupBox1.Controls.Add(this.progressBarQuality);
      this.groupBox1.Controls.Add(this.mpLabel2);
      this.groupBox1.Controls.Add(this.mpLabel1);
      this.groupBox1.Controls.Add(this.progressBarStrength);
      this.groupBox1.Controls.Add(this.lblStatus);
      this.groupBox1.Controls.Add(this.progressBarProgress);
      this.groupBox1.Controls.Add(this.buttonScan);
      this.groupBox1.Controls.Add(this.buttonAdd);
      this.groupBox1.Controls.Add(this.buttonClear);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.groupBox3);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(5, 5);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(532, 391);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Setup Analog TV";
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.WindowFrame;
      this.panel1.Location = new System.Drawing.Point(16, 205);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(222, 183);
      this.panel1.TabIndex = 20;
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                        {
                                          this.columnHeader1,
                                          this.columnHeader2
                                        });
      this.listView1.Location = new System.Drawing.Point(245, 205);
      this.listView1.MultiSelect = false;
      this.listView1.FullRowSelect = true;
      this.listView1.AllowDrop = true;
      this.listView1.HideSelection = false;
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(151, 183);
      this.listView1.TabIndex = 21;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.SmallImageList = this.imageList1;
      this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
      this.listView1.ItemActivate += new EventHandler(this.listView1_ItemActivate);
      this.listView1.KeyUp += new KeyEventHandler(this.listView1_KeyUp);
      this.listView1.ItemDrag += new ItemDragEventHandler(listView1_OnItemDrag);
      this.listView1.DragDrop += new DragEventHandler(listView1_OnDragDrop);
      this.listView1.DragEnter += new DragEventHandler(listView1_OnDragEnter);
      this.listView1.DragOver += new DragEventHandler(listView1_OnDragOver);
      this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 100;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Ch.";
      this.columnHeader2.Width = 50;
      // 
      // imageList1
      // 
      this.imageList1.ImageStream =
        ((System.Windows.Forms.ImageListStreamer) (resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "");
      this.imageList1.Images.SetKeyName(1, "");
      // 
      // contextMenuStrip
      // 
      this.contextMenuStrip.BackColor = System.Drawing.SystemColors.Window;
      this.contextMenuStrip.MinimumSize = new System.Drawing.Size(10, 0);
      this.contextMenuStrip.Name = "contextMenuStrip";
      this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
      this.contextMenuStrip.TabStop = true;
      // 
      // imageListContextMenu
      // 
      this.imageListContextMenu.ImageStream =
        ((System.Windows.Forms.ImageListStreamer) (resources.GetObject("imageListContextMenu.ImageStream")));
      this.imageListContextMenu.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListContextMenu.Images.SetKeyName(0, "edit.png");
      this.imageListContextMenu.Images.SetKeyName(1, "tvoff.png");
      this.imageListContextMenu.Images.SetKeyName(2, "delete.png");
      this.imageListContextMenu.Images.SetKeyName(3, "deleteall.png");
      this.imageListContextMenu.Images.SetKeyName(4, "new.png");
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
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(114, 100);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(325, 16);
      this.progressBarProgress.TabIndex = 4;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(454, 64);
      this.buttonScan.Name = "buttonScan";
      this.buttonScan.Size = new System.Drawing.Size(72, 22);
      this.buttonScan.TabIndex = 3;
      this.buttonScan.Text = "Scan";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // buttonAdd
      // 
      this.buttonAdd.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAdd.Location = new System.Drawing.Point(404, 336);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(72, 22);
      this.buttonAdd.TabIndex = 4;
      this.buttonAdd.Text = "Add";
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.itemAdd_Click);
      // 
      // buttonClear
      // 
      this.buttonClear.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonClear.Location = new System.Drawing.Point(404, 366);
      this.buttonClear.Name = "buttonClear";
      this.buttonClear.Size = new System.Drawing.Size(72, 22);
      this.buttonClear.TabIndex = 5;
      this.buttonClear.Text = "Delete All";
      this.buttonClear.UseVisualStyleBackColor = true;
      this.buttonClear.Click += new System.EventHandler(this.itemClear_Click);
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
      this.label1.Text = "Select your country and input source and press \"Scan\" to scan for TV channels. Or" +
                         " select your city and press \"Download\" to import TV channels from MediaPortal We" +
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
      this.groupBox2.Text = "Channel Download";
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
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBox3.Text = "TV Tuner";
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
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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
      // Wizard_AnalogTV
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Wizard_AnalogTV";
      this.Size = new System.Drawing.Size(545, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
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
          if ((dev.Network == NetworkType.Analog) && (dev.SupportsTV))
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
            if ((dev.Network == NetworkType.Analog) && (dev.SupportsTV))
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
      GUIGraphicsContext.ActiveForm = this.Handle;
      Point videoWindowLocation = new Point(this.panel1.Location.X + this.groupBox1.Location.X,
                                            this.panel1.Location.Y + this.groupBox1.Location.Y);
      GUIGraphicsContext.VideoWindow = new Rectangle(videoWindowLocation, panel1.Size);

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
      buttonClear.Enabled = _clearButtonEnabled;
      buttonAdd.Enabled = true;
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(false);
        wizard.DisableNext(false);
      }
      MapTvToOtherCards(_card.ID);
    }

    private void dlg_OnScanStarted(object sender, EventArgs args)
    {
      groupBox3.Enabled = false;
      _groupBox2Enabled = groupBox2.Enabled;
      groupBox2.Enabled = false;
      _clearButtonEnabled = buttonClear.Enabled;
      buttonClear.Enabled = false;
      buttonAdd.Enabled = false;
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
        int newTvChannels;
        int updatedTvChannels;
        OnStatus2("");
        Cursor.Current = Cursors.WaitCursor;
        buttonImport.Enabled = false;
        ImportAnalogChannels(nodeAnalog.InnerText, out newTvChannels, out updatedTvChannels);
        MapTvToOtherCards(_card.ID);
        OnStatus2(String.Format("Imported {0} new tv channels, {1} updated tv channels", newTvChannels,
                                updatedTvChannels));
        buttonImport.Enabled = true;
        Cursor.Current = Cursors.Default;
        UpdateList();
      }
      else
      {
        OnStatus2("");
      }
    }

    private void ImportAnalogChannels(string xmlFile, out int newTvChannels, out int updatedTvChannels)
    {
      newTvChannels = 0;
      updatedTvChannels = 0;
      try
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
          string channelName = name.Value;
          int channelNumber = -1;
          long channelFrequency = 0;
          try
          {
            channelNumber = Int32.Parse(number.Value);
          }
          catch (Exception)
          {
          }
          try
          {
            channelFrequency = ConvertToTvFrequency(frequency.Value);
          }
          catch (Exception)
          {
          }
          int channelId = TVDatabase.GetChannelId(channelName);
          TVChannel tvChan = TVDatabase.GetChannelById(channelId);
          if (tvChan == null)
          {
            //doesn't exists
            tvChan = new TVChannel();
            tvChan.Scrambled = false;
            //then add a new channel to the database
            tvChan.Name = channelName;
            tvChan.ID = -1;
            tvChan.Number = channelNumber;
            tvChan.Frequency = channelFrequency;
            tvChan.Sort = 40000;
            Log.Info("Wizard_AnalogTV: add new channel for {0}:{1}:{2}", tvChan.Name, tvChan.Number, tvChan.Sort);
            int id = TVDatabase.AddChannel(tvChan);
            if (id < 0)
            {
              Log.Error("Wizard_AnalogTV: failed to add new channel for {0}:{1}:{2} to database", tvChan.Name,
                        tvChan.Number, tvChan.Sort);
            }
            newTvChannels++;
          }
          else
          {
            tvChan.Name = channelName;
            tvChan.Number = channelNumber;
            tvChan.Frequency = channelFrequency;
            TVDatabase.UpdateChannel(tvChan, tvChan.Sort);
            updatedTvChannels++;
            Log.Info("Wizard_AnalogTV: update channel {0}:{1}:{2} {3}", tvChan.Name, tvChan.Number, tvChan.Sort,
                     tvChan.ID);
          }
          TVDatabase.MapChannelToCard(tvChan.ID, _card.ID);
          TVGroup group = new TVGroup();
          group.GroupName = "Analog";
          int groupid = TVDatabase.AddGroup(group);
          group.ID = groupid;
          TVDatabase.MapChannelToGroup(group, tvChan);
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Cannot connect to MediaPortal Web Site", "MediaPortal Settings", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }
    }

    private long ConvertToTvFrequency(string frequency)
    {
      if (frequency.Trim() == string.Empty)
      {
        return 0;
      }
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
          _card.StartViewing(selectedChannel);
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
          xmlwriter.SetValue("capture", "tuner", cbInput.Text);
        }
        if (listView1.SelectedIndices.Count > 0)
        {
          _card.DeleteGraph();
          string selectedChannel = listView1.SelectedItems[0].Text;
          _card.StartViewing(selectedChannel);
        }
      }
    }

    private void DeleteChannel()
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        string selectedChannel = listView1.SelectedItems[0].Text;
        TVDatabase.RemoveChannel(selectedChannel);
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
          buttonClear.Enabled = false;
          _card.DeleteGraph();
        }
      }
    }

    private void itemStop_Click(object sender, EventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        _card.DeleteGraph();
        listView1.SelectedItems[0].Selected = false;
        listView1.Select();
      }
    }

    private void itemDel_Click(object sender, EventArgs e)
    {
      DeleteChannel();
    }

    private void itemClear_Click(object sender, EventArgs e)
    {
      DialogResult result = MessageBox.Show(this, "Are you sure you want to delete all channels?", "Delete channels",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
      {
        return;
      }
      _card.DeleteGraph();
      foreach (ListViewItem item in listView1.Items)
      {
        TVDatabase.RemoveChannel(item.Text);
      }
      listView1.Items.Clear();
      buttonClear.Enabled = false;
    }

    private void itemAdd_Click(object sender, EventArgs e)
    {
      EditTVChannelForm editChannel = new EditTVChannelForm();
      TelevisionChannel editedChannel = editChannel.Channel;
      editedChannel.ID = -1;
      editChannel.SortingPlace = listView1.Items.Count + _card.ID*100000;
      editChannel.Channel = editedChannel;
      DialogResult dialogResult = editChannel.ShowDialog(this);

      if (dialogResult == DialogResult.OK)
      {
        editedChannel = editChannel.Channel;
        TVChannel tvChan = new TVChannel();
        tvChan.ID = editedChannel.ID;
        tvChan.Name = editChannel.Name;
        tvChan.Sort = editChannel.SortingPlace;
        TVDatabase.MapChannelToCard(tvChan.ID, _card.ID);
        TVGroup group = new TVGroup();
        group.GroupName = "Analog";
        int groupid = TVDatabase.AddGroup(group);
        group.ID = groupid;
        TVDatabase.MapChannelToGroup(group, tvChan);
        UpdateList();
      }
    }

    private void listView1_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        contextMenuStrip.Items.Clear();
        ToolStripItem item = null;
        if ((listView1.SelectedIndices.Count > 0) && (_card != null))
        {
          item = contextMenuStrip.Items.Add("Stop Viewing");
          item.Click += new EventHandler(itemStop_Click);
          item.Image = imageListContextMenu.Images[1];
          if (_card.TVChannel == string.Empty)
          {
            item.Enabled = false;
          }
          item = contextMenuStrip.Items.Add("Edit Channel");
          item.Click += new EventHandler(listView1_ItemActivate);
          item.Image = imageListContextMenu.Images[0];
          item = contextMenuStrip.Items.Add("Delete Channel");
          item.Click += new EventHandler(itemDel_Click);
          item.Image = imageListContextMenu.Images[2];
          contextMenuStrip.Items.Add(new ToolStripSeparator());
        }
        item = contextMenuStrip.Items.Add("Add New Channel");
        item.Click += new EventHandler(itemAdd_Click);
        item.Image = imageListContextMenu.Images[4];
        item = contextMenuStrip.Items.Add("Delete All Channels");
        item.Click += new EventHandler(itemClear_Click);
        item.Image = imageListContextMenu.Images[3];
        contextMenuStrip.Show(MousePosition);
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        string selectedChannel = listView1.SelectedItems[0].Text;
        listView1.Update();
        if (_card.IsRadio)
        {
          _card.DeleteGraph();
        }
        _card.StartViewing(selectedChannel);
      }
    }

    private void listView1_KeyUp(Object o, KeyEventArgs e)
    {
      if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Back)
      {
        DeleteChannel();
      }
    }

    private void listView1_ItemActivate(object sender, EventArgs e)
    {
      if ((listView1.SelectedIndices.Count > 0) && (_card != null))
      {
        string selectedChannel = listView1.SelectedItems[0].Text;
        TVChannel channel = TVDatabase.GetChannelById(TVDatabase.GetChannelId(selectedChannel));

        TelevisionChannel tvChannel = new TelevisionChannel();

        tvChannel.ID = channel.ID;
        tvChannel.Channel = channel.Number;
        tvChannel.Name = channel.Name;
        tvChannel.Frequency = channel.Frequency;
        tvChannel.External = channel.External;
        tvChannel.ExternalTunerChannel = channel.ExternalTunerChannel;
        tvChannel.VisibleInGuide = channel.VisibleInGuide;
        tvChannel.Country = channel.Country;
        tvChannel.standard = channel.TVStandard;
        tvChannel.Scrambled = channel.Scrambled;

        EditTVChannelForm editChannel = new EditTVChannelForm();
        editChannel.Channel = tvChannel;
        editChannel.SortingPlace = channel.Sort;

        DialogResult dialogResult = editChannel.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          TelevisionChannel editedChannel = editChannel.Channel;
          ListViewItem item = listView1.SelectedItems[0];
          item.ImageIndex = 0;
          if (editedChannel.Scrambled)
          {
            item.ImageIndex = 1;
          }
          item.Text = editedChannel.Name;
          item.SubItems[1].Text = editedChannel.Channel.ToString();
          listView1.Update();
          if (channel.Number != editedChannel.Channel)
          {
            _card.DeleteGraph();
            _card.StartViewing(selectedChannel);
          }
        }
      }
    }

    private void listView1_OnDragDrop(object sender, DragEventArgs e)
    {
      if (listView1.SelectedItems.Count == 0)
      {
        return;
      }
      Point cp = listView1.PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = listView1.GetItemAt(cp.X, cp.Y);
      if (dragToItem == null)
      {
        return;
      }
      int dropIndex = dragToItem.Index;
      if (dropIndex > listView1.SelectedItems[0].Index)
      {
        dropIndex++;
      }
      ArrayList insertItems =
        new ArrayList(listView1.SelectedItems.Count);
      foreach (ListViewItem item in listView1.SelectedItems)
      {
        insertItems.Add(item.Clone());
      }
      for (int i = insertItems.Count - 1; i >= 0; i--)
      {
        ListViewItem insertItem =
          (ListViewItem) insertItems[i];
        listView1.Items.Insert(dropIndex, insertItem);
      }
      foreach (ListViewItem removeItem in listView1.SelectedItems)
      {
        listView1.Items.Remove(removeItem);
      }
      listView1.Focus();
      ((ListViewItem) insertItems[0]).Selected = true;
      listView1.FocusedItem = (ListViewItem) insertItems[0];
      listView1.Select();
    }

    private void listView1_OnDragOver(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.Text))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      Point cp = listView1.PointToClient(new Point(e.X, e.Y));
      ListViewItem hoverItem = listView1.GetItemAt(cp.X, cp.Y);
      if (hoverItem == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      foreach (ListViewItem moveItem in listView1.SelectedItems)
      {
        if (moveItem.Index == hoverItem.Index)
        {
          e.Effect = DragDropEffects.None;
          hoverItem.EnsureVisible();
          return;
        }
      }
      String text = (String) e.Data.GetData(REORDER.GetType());
      if (text.CompareTo(REORDER) == 0)
      {
        e.Effect = DragDropEffects.Move;
        hoverItem.EnsureVisible();
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
    }

    private void listView1_OnDragEnter(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.Text))
      {
        e.Effect = DragDropEffects.None;
        return;
      }
      String text = (String) e.Data.GetData(REORDER.GetType());
      if (text.CompareTo(REORDER) == 0)
      {
        e.Effect = DragDropEffects.Move;
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
    }

    private void listView1_OnItemDrag(object sender, ItemDragEventArgs e)
    {
      listView1.DoDragDrop(REORDER, DragDropEffects.Move);
    }

    private void MapTvToOtherCards(int id)
    {
      ArrayList tvchannels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref tvchannels, id);
      TVCaptureCards cards = new TVCaptureCards();
      cards.LoadCaptureCards();
      foreach (TVCaptureDevice dev in cards.captureCards)
      {
        if ((dev.Network == NetworkType.Analog) && (dev.ID != id) && (dev.SupportsTV))
        {
          foreach (TVChannel chan in tvchannels)
          {
            TVDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbInput.SelectedItem = xmlreader.GetValueAsString("capture", "tuner", "Antenna");
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
        xmlwriter.SetValue("capture", "tuner", cbInput.Text);
        if (cbCountry.Text.Length > 0)
        {
          TunerCountry tunerCountry = cbCountry.SelectedItem as TunerCountry;
          xmlwriter.SetValue("capture", "countryname", tunerCountry.Country);
          xmlwriter.SetValue("capture", "country", tunerCountry.Id.ToString());
        }
      }
      foreach (ListViewItem item in listView1.Items)
      {
        string selectedChannel = item.Text;
        TVDatabase.SetChannelSort(selectedChannel, item.Index + _card.ID*100000);
      }
    }

    public override void UpdateList()
    {
      listView1.Items.Clear();
      if (_card == null)
      {
        return;
      }
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref channels, _card.ID);
      if (Scanning)
      {
        _clearButtonEnabled = (channels.Count != 0);
      }
      else
      {
        buttonClear.Enabled = (channels.Count != 0);
      }
      foreach (TVChannel channel in channels)
      {
        if (channel.Number < (int) ExternalInputs.svhs)
        {
          ListViewItem item = new ListViewItem();
          item.ImageIndex = 0;
          if (channel.Scrambled)
          {
            item.ImageIndex = 1;
          }
          item.Text = channel.Name;
          item.SubItems.Add(channel.Number.ToString());
          listView1.Items.Add(item);
        }
      }
      listView1.Update();
      TVChannels.UpdateList();
    }
  }
}