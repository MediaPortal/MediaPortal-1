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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using TVCapture;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class TVCaptureCards : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPListView cardsListView;
    private MediaPortal.UserInterface.Controls.MPButton addButton;
    private MediaPortal.UserInterface.Controls.MPButton editButton;
    private MediaPortal.UserInterface.Controls.MPButton deleteButton;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    bool _init = false;

    //
    // Privare members
    //
    public ArrayList captureCards = new ArrayList();

    public TVCaptureCards()
      : this("Capture Cards")
    {
    }

    public TVCaptureCards(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      // Disable it TVE3
      //Plugins plugin = new Plugins();
      //string plugindesc = plugin.GetPluginDescription("My TV");

      //if (plugindesc.Contains("v3"))
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
        _init = true;
      }

    }

    public override void OnSectionActivated()
    {

      if (_init == false)
      {

        //
        // Load capture cards
        //
        LoadCaptureCards();

        // 
        // Populate the list view
        //
        PopulateListView();
        _init = true;
      }
    }
    private void PopulateListView()
    {
      cardsListView.Items.Clear();
      foreach (TVCaptureDevice captureCard in captureCards)
      {
        AddCaptureCard(captureCard);
      }
    }

    private void AddCaptureCard(TVCaptureDevice card)
    {
      //cardsListView.Items.Clear();
      ListViewItem listItem = new ListViewItem(new string[] { card.CommercialName, 
                                  card.FriendlyName,                                                                
																	card.UseForTV.ToString(),
																	card.UseForRecording.ToString()
																  });

      listItem.Tag = card;

      cardsListView.Items.Add(listItem);
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
      this.deleteButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.editButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.cardsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.cardsListView);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 216);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Enabled = false;
      this.deleteButton.Location = new System.Drawing.Point(384, 184);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(72, 22);
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Enabled = false;
      this.editButton.Location = new System.Drawing.Point(304, 184);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(72, 22);
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(224, 184);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(72, 22);
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // cardsListView
      // 
      this.cardsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cardsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                    this.columnHeader1,
                                                                                    this.columnHeader4,
                                                                                    this.columnHeader2,
                                                                                    this.columnHeader3});
      this.cardsListView.FullRowSelect = true;
      this.cardsListView.Location = new System.Drawing.Point(16, 24);
      this.cardsListView.Name = "cardsListView";
      this.cardsListView.Size = new System.Drawing.Size(440, 152);
      this.cardsListView.TabIndex = 0;
      this.cardsListView.View = System.Windows.Forms.View.Details;
      this.cardsListView.DoubleClick += new System.EventHandler(this.cardsListView_DoubleClick);
      this.cardsListView.SelectedIndexChanged += new System.EventHandler(this.cardsListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Capture card";
      this.columnHeader1.Width = 200;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Friendly name";
      this.columnHeader4.Width = 77;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Television";
      this.columnHeader2.Width = 63;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Recording";
      this.columnHeader3.Width = 96;
      // 
      // TVCaptureCards
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVCaptureCards";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void addButton_Click(object sender, System.EventArgs e)
    {
      //find unique ID for the new card
      int cardid = 1;
      bool found = false;
      do
      {
        found = false;
        foreach (ListViewItem item in cardsListView.Items)
        {
          TVCaptureDevice dev = item.Tag as TVCaptureDevice;
          if (dev.ID == cardid)
          {
            found = true;
            cardid++;
            break;
          }
        }
      } while (found);

      EditCaptureCardForm editCard = new EditCaptureCardForm(cardid, true, null);
      if (editCard.IsDisposed) return;

      DialogResult dialogResult = editCard.ShowDialog(this);
      if (dialogResult == DialogResult.OK)
      {
        AddCaptureCard(editCard.CaptureCard);
        captureCards.Add(editCard.CaptureCard);
      }
      SaveSettings();
    }

    private void editButton_Click(object sender, System.EventArgs e)
    {
      foreach (ListViewItem listItem in cardsListView.SelectedItems)
      {
        TVCaptureDevice device = listItem.Tag as TVCaptureDevice;
        EditCaptureCardForm editCard = new EditCaptureCardForm(device.ID, false, device);
        editCard.CaptureCard = device;

        DialogResult dialogResult = editCard.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          TVCaptureDevice captureCard = editCard.CaptureCard;

          listItem.Tag = captureCard;

          listItem.SubItems[0].Text = captureCard.CommercialName;
          listItem.SubItems[1].Text = captureCard.FriendlyName;
          listItem.SubItems[2].Text = captureCard.UseForTV.ToString();
          listItem.SubItems[3].Text = captureCard.UseForRecording.ToString();
        }
      }
      SaveSettings();
    }

    private void deleteButton_Click(object sender, System.EventArgs e)
    {
      int itemCount = cardsListView.SelectedItems.Count;

      for (int index = 0; index < itemCount; index++)
      {
        //
        // Fetch device
        //
        TVCaptureDevice card = cardsListView.Items[cardsListView.SelectedIndices[0]].Tag as TVCaptureDevice;

        TVDatabase.DeleteCard(card.ID);
        //
        // Remove it from the internal list
        //
        captureCards.Remove(card);

        //
        // Remove from the list view
        //
        cardsListView.Items.RemoveAt(cardsListView.SelectedIndices[0]);
      }
      SaveSettings();

      LoadCaptureCards();
      PopulateListView();
    }

    public void LoadCaptureCards()
    {

      if (File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
      {
        using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            //
            // Create Soap Formatter
            //
            SoapFormatter formatter = new SoapFormatter();

            //
            // Serialize
            //
            captureCards = new ArrayList();
            captureCards = (ArrayList)formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice)captureCards[i]).ID = (i + 1);

            }
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
						Log.Error("Recorder: LoadCaptureCards()");
          }
        }
      }
    }

    void SaveCaptureCards(ArrayList availableCards)
    {
      using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        //
        // Create Soap Formatter
        //
        SoapFormatter formatter = new SoapFormatter();

        //
        // Serialize
        //
        formatter.Serialize(fileStream, availableCards);

        //
        // Finally close our file stream
        //
        fileStream.Close();

      }
    }
    public override void SaveSettings()
    {
      if (_init == false) return;
      ArrayList availableCards = new ArrayList();

      foreach (ListViewItem listItem in cardsListView.Items)
      {
        availableCards.Add(listItem.Tag);
      }
      SaveCaptureCards(availableCards);
    }

    private void cardsListView_DoubleClick(object sender, System.EventArgs e)
    {
      editButton_Click(sender, e);
    }

    private void cardsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      editButton.Enabled = deleteButton.Enabled = (cardsListView.SelectedItems.Count > 0);
    }

    public void AddAllCards()
    {
      captureCards = new ArrayList();

      ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();
      ArrayList availableVideoDeviceMonikers = FilterHelper.GetVideoInputDeviceMonikers();
      ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
      /*			
            availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
            availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
            availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
            availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8170070&rev_01#5&267465cb&0&4828f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");
            availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8170070&rev_01#5&e6752e3&0&4820f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");
            availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_e8070070&rev_01#5&e6752e3&0&4020f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}");		
              availableVideoDevices.Add("FireDTV BDA Receiver DVBC");
              availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\avc#digital_everywhere&firedtv_c#ci&typ_5&id_0#1e04003600871200#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\{cb365890-165f-}");
              availableVideoDevices.Add("FusionHDTV, Transport Stream Capture");
              availableVideoDeviceMonikers.Add(@":@device:pnp:\\?\pci#ven_14f1&dev_8802&subsys_d50018ac&rev_05#4&13699180&0&3248#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\global");
      */

      string recFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      recFolder += @"\My Recordings";
      try
      {
        System.IO.Directory.CreateDirectory(recFolder);
      }
      catch (Exception) { }

      //enum all cards known in capturedefinitions.xml
      foreach (CaptureCardDefinition ccd in CaptureCardDefinitions.CaptureCards)
      {
        //enum all video capture devices on this system
        for (int i = 0; i < availableVideoDevices.Count; i++)
        {
          //treat the SSE2 DVB-S card as a general H/W card
          if (((string)(availableVideoDevices[i])) == "B2C2 MPEG-2 Source")
          {
            TVCaptureDevice cd = new TVCaptureDevice();
            cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
            cd.VideoDevice = (string)availableVideoDevices[i];
            cd.CommercialName = "Skystar 2";
            cd.CardType = TVCapture.CardTypes.Digital_SS2;
            cd.DeviceId = (string)availableVideoDevices[i];
            cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
            //cd.DeviceType					= "hw";
            cd.RecordingPath = recFolder;
            cd.UseForRecording = true;
            cd.UseForTV = true;
            cd.SupportsTV = true;
            cd.SupportsRadio = true;
            cd.Priority = 10;
            captureCards.Add(cd);

            string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), cd.FriendlyName);
            // save settings for get the filename in mp.xml
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
              xmlwriter.SetValue("dvb_ts_cards", "filename", filename);
            }
            availableVideoDeviceMonikers.RemoveAt(i);
            availableVideoDevices.RemoveAt(i);
            continue;
          }
          /*
          //treat the TTPremium card as a general H/W card
          if (((string)(availableVideoDevices[i])) == "TechnoTrend SAA7146 Capture (WDM)")
          {
            TVCaptureDevice cd = new TVCaptureDevice();
            cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
            cd.VideoDevice = (string)availableVideoDevices[i];
            cd.CommercialName = "Techno Trend Premium";
            cd.CardType = TVCapture.CardTypes.Digital_TTPremium;
            cd.DeviceId = (string)availableVideoDevices[i];
            cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
            //cd.DeviceType					= "hw";
            cd.RecordingPath = recFolder;
            cd.UseForRecording = true;
            cd.UseForTV = true;
            cd.SupportsTV = true;
            cd.SupportsRadio = true;
            cd.Priority = 10;
            captureCards.Add(cd);

            string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), cd.FriendlyName);
            // save settings for get the filename in mp.xml
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
              xmlwriter.SetValue("dvb_ts_cards", "filename", filename);
            }
            availableVideoDeviceMonikers.RemoveAt(i);
            availableVideoDevices.RemoveAt(i);
            continue;
          }
          */
          if (ccd.CaptureName == string.Empty) continue;
          if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
            ((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1))
          {
            TVCaptureDevice cd = new TVCaptureDevice();
            cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
            cd.VideoDevice = ccd.CaptureName;
            cd.CommercialName = ccd.CommercialName;
            cd.CardType = ccd.Capabilities.CardType;
            cd.DeviceId = ccd.DeviceId;
            cd.SupportsTV = ccd.Capabilities.HasTv;
            cd.SupportsRadio = ccd.Capabilities.HasRadio;
            cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
            //cd.DeviceType					= ccd.DeviceId;
            cd.RecordingPath = recFolder;
            if (cd.CardType == TVCapture.CardTypes.Digital_BDA) cd.Priority = 10;
            else if (cd.CardType == TVCapture.CardTypes.Digital_SS2) cd.Priority = 10;
            else cd.Priority = 1;
            cd.UseForRecording = true;
            cd.UseForTV = true;
            captureCards.Add(cd);
            availableVideoDeviceMonikers.RemoveAt(i);
            availableVideoDevices.RemoveAt(i);
          }
        }
      }
      SaveCaptureCards(captureCards);
    }

  }
}

