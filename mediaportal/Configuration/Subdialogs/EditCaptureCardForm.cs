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
using System.Management;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.GUI.Library;
using TVCapture;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditCaptureCardForm.
  /// </summary>
  public class EditCaptureCardForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    static CaptureCardDefinitions mCaptureCardDefinitions = CaptureCardDefinitions.Instance;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPComboBox cardComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox useRecordingCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox useWatchingCheckBox;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private IContainer components;

    //
    // Private members
    //
    ArrayList captureFormats = new ArrayList();
    ArrayList propertyPages = new ArrayList();
    private Size m_size = new Size(0, 0);
    private MediaPortal.UserInterface.Controls.MPLabel label12;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxName;

    bool acceptuserinput = false;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageAutotune;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageAutotuneRadio;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage5;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox3Audio;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox3Video;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox2Audio;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox2Video;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1Audio;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1Video;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPLabel label17;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHiQuality;
    private System.Windows.Forms.NumericUpDown updownPrio;
    private MediaPortal.UserInterface.Controls.MPLabel label24;
    private MediaPortal.UserInterface.Controls.MPLabel label25;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxQuality;
    private MediaPortal.UserInterface.Controls.MPButton buttonBrowse;
    private MediaPortal.UserInterface.Controls.MPTextBox tbRecordingFolder;
    private MediaPortal.UserInterface.Controls.MPLabel label35;
    private MediaPortal.UserInterface.Controls.MPComboBox cbRgbVideo;
    private MediaPortal.UserInterface.Controls.MPComboBox cbRgbAudio;
    private MediaPortal.UserInterface.Controls.MPLabel label18;
    private MediaPortal.UserInterface.Controls.MPLabel label26;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox7;
    private MediaPortal.UserInterface.Controls.MPLabel label19;
    private MediaPortal.UserInterface.Controls.MPLabel label27;
    private MediaPortal.UserInterface.Controls.MPLabel label28;
    private MediaPortal.UserInterface.Controls.MPLabel label29;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPortMin;
    private MediaPortal.UserInterface.Controls.MPTextBox tbPortMax;
    private MediaPortal.UserInterface.Controls.MPTextBox tbLowMax;
    private MediaPortal.UserInterface.Controls.MPTextBox tbLowMin;
    private MediaPortal.UserInterface.Controls.MPLabel label34;
    private MediaPortal.UserInterface.Controls.MPTextBox tbMedMax;
    private MediaPortal.UserInterface.Controls.MPTextBox tbMedMin;
    private MediaPortal.UserInterface.Controls.MPLabel label36;
    private MediaPortal.UserInterface.Controls.MPTextBox tbHighMax;
    private MediaPortal.UserInterface.Controls.MPTextBox tbHighMin;
    private MediaPortal.UserInterface.Controls.MPLabel label37;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbLowVBR;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbMedVBR;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbHighVBR;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbPortVBR;
    private CheckBox checkBoxHWPidFiltering;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private ComboBox comboBoxCAM;
    int CardId;
    private MediaPortal.UserInterface.Controls.MPLabel lblFrameSize;
    private ComboBox comboBoxFrameSize;
    private MediaPortal.UserInterface.Controls.MPLabel lblFrameRate;
    private ComboBox comboBoxFrameRate;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBox5vAntennae;
    private MediaPortal.UserInterface.Controls.MPGroupBox cropGroupBox;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDownLeft;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDownBottom;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDownTop;
    private MediaPortal.UserInterface.Controls.MPLabel cropRightLabel;
    private MediaPortal.UserInterface.Controls.MPLabel cropLeftLabel;
    private MediaPortal.UserInterface.Controls.MPLabel cropBottomLabel;
    private MediaPortal.UserInterface.Controls.MPLabel cropTopLabel;
    private MediaPortal.UserInterface.Controls.MPNumericUpDown mpNumericUpDownRight;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private ComboBox comboBoxAudioQuality;
    private CheckBox cbDisableDNR;
    private ToolTip toolTip1;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbTVVBR;
    private MediaPortal.UserInterface.Controls.MPTextBox tbTVHigh;
    private MediaPortal.UserInterface.Controls.MPTextBox tbTVMin;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
    TVCaptureDevice prevDevice = null;


    /// <summary>
    /// 
    /// </summary>
    public EditCaptureCardForm(int cardId, bool addNewCard, TVCaptureDevice deviceToEdit)
    {

      ArrayList captureCards = new ArrayList();
      if (addNewCard)
      {
        try
        {
          using (FileStream fileStream = new FileStream(Config.GetFile(Config.Dir.Config,"capturecards.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {

            SoapFormatter formatter = new SoapFormatter();
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
        }
        catch
        {
					Log.Error("Recorder: EditCaptureCardForm");
        }
      }

      CardId = cardId;
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      comboBox1Audio.Items.Add("Audio-in #1");
      comboBox1Audio.Items.Add("Audio-in #2");
      comboBox1Audio.Items.Add("Audio-in #3");

      comboBox2Audio.Items.Add("Audio-in #1");
      comboBox2Audio.Items.Add("Audio-in #2");
      comboBox2Audio.Items.Add("Audio-in #3");

      comboBox3Audio.Items.Add("Audio-in #1");
      comboBox3Audio.Items.Add("Audio-in #2");
      comboBox3Audio.Items.Add("Audio-in #3");

      comboBox1Video.Items.Add("CVBS #1");
      comboBox1Video.Items.Add("CVBS #2");
      comboBox1Video.Items.Add("CVBS #3");

      comboBox2Video.Items.Add("CVBS #1");
      comboBox2Video.Items.Add("CVBS #2");
      comboBox2Video.Items.Add("CVBS #3");

      comboBox3Video.Items.Add("SVHS #1");
      comboBox3Video.Items.Add("SVHS #2");
      comboBox3Video.Items.Add("SVHS #3");

      cbRgbAudio.Items.Add("Audio-in #1");
      cbRgbAudio.Items.Add("Audio-in #2");
      cbRgbAudio.Items.Add("Audio-in #3");

      cbRgbVideo.Items.Add("RGB #1");
      cbRgbVideo.Items.Add("RGB #2");
      cbRgbVideo.Items.Add("RGB #3");
      //
      // Setup combo boxes and controls
      //
      ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();
      ArrayList availableVideoDeviceMonikers = FilterHelper.GetVideoInputDeviceMonikers();
      ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
      ArrayList availableVideoCompressors = FilterHelper.GetVideoCompressors();
      ArrayList availableAudioCompressors = FilterHelper.GetAudioCompressors();

      /* below is used for testing only 
      
      availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
      availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0803&subsys_40000070&rev_01#3&267a616a&0&48#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capture");
    

      //test for SS2
      availableVideoDevices.Add("B2C2 MPEG-2 Source");
      availableVideoDeviceMonikers.Add("B2C2 MPEG-2 Source");
      
      //test for dvb-c
      availableVideoDevices.Add("FireDTV BDA Receiver DVBC");
      availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\avc#digital_everywhere&firedtv_c#ci&typ_5&id_0#1e04003600871200#{fd0a5af4-b41d-11d2-9c95-00c04f7971e0}\{cb365890-165f-}");

      above is used for testing only */

      FilterHelper.GetMPEG2VideoEncoders(availableVideoCompressors);
      FilterHelper.GetMPEG2AudioEncoders(availableAudioCompressors);
      for (int i = 0; i < availableVideoDevices.Count; ++i)
      {
        Log.Info("device:{0} id:{1}", availableVideoDevices[i].ToString(), availableVideoDeviceMonikers[i].ToString());
      }


      if (availableVideoDevices.Count == 0)
      {
        this.Close();
        MessageBox.Show("No video device was found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = cardComboBox.Enabled = okButton.Enabled = false;
        acceptuserinput = false;
      }
      else
        acceptuserinput = true;

      // #MW#
      // Load capture card definitions, and only display those cards that are supported by MP
      // This might mean that altough capture cards are present, they're not supported, and thus
      // not displayed ;-)
      //
      // So:
      //	match cards & put in list
      //	if list is empty, inform user and block further input
      //	else continue


      bool ss2Added = false;
      //bool ttPremiumAdded = false;
      //enum all cards known in capturedefinitions.xml
      foreach (CaptureCardDefinition ccd in CaptureCardDefinitions.CaptureCards)
      {
        //enum all video capture devices on this system
        for (int i = 0; i < availableVideoDevices.Count; i++)
        {
          //treat the SSE2 DVB-S card as a general H/W card
          if (((string)(availableVideoDevices[i])) == "B2C2 MPEG-2 Source")
          {
            if (ss2Added) continue;
            if (addNewCard || (!addNewCard && deviceToEdit.VideoDevice == "B2C2 MPEG-2 Source"))
            {
              ss2Added = true;
              TVCaptureDevice cd = new TVCaptureDevice();
              cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
              cd.VideoDevice = (string)availableVideoDevices[i];
              cd.CommercialName = "Skystar 2";
              cd.CardType = TVCapture.CardTypes.Digital_SS2;
              cd.DeviceId = (string)availableVideoDevices[i];
              cd.SupportsTV = true;
              cd.SupportsRadio = false;
              cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
              cd.ID = cardId;
              ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
              bool alreadyAdded = false;
              if (addNewCard)
              {
                foreach (TVCaptureDevice dev in captureCards)
                {
                  if (dev.CardType == cd.CardType)
                    alreadyAdded = true;
                }
              }
              if (!alreadyAdded)
                cardComboBox.Items.Add(cbcc);
            }
            //Rem'd below should fix SS2 cards with other DVB devices
            //if (addNewCard) break; // must not break in edit mode
          }
          /*
          //treat the TTPremium DVB-S card as a general H/W card
          if (((string)(availableVideoDevices[i])) == "TechnoTrend SAA7146 Capture (WDM)")
          {
            if (ttPremiumAdded) continue;
            if (addNewCard || (!addNewCard && deviceToEdit.VideoDevice == "TechnoTrend SAA7146 Capture (WDM)"))
            {
              ttPremiumAdded = true;
              TVCaptureDevice cd = new TVCaptureDevice();
              cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
              cd.VideoDevice = (string)availableVideoDevices[i];
              cd.CommercialName = "Techno Trend Premium";
              cd.CardType = TVCapture.CardTypes.Digital_TTPremium;
              cd.DeviceId = (string)availableVideoDevices[i];
              cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
              cd.ID = cardId;
              cd.SupportsTV = true;
              cd.SupportsRadio = false;
              ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
              bool alreadyAdded = false;
              if (addNewCard)
              {
                foreach (TVCaptureDevice dev in captureCards)
                {
                  if (dev.CardType == cd.CardType)
                    alreadyAdded = true;
                }
              }
              if (!alreadyAdded)
                cardComboBox.Items.Add(cbcc);
            }
            //Rem'd below should fix SS2 cards with other DVB devices
            //if (addNewCard) break; // must not break in edit mode
          }
          */

          bool add = false;
          if (ccd.CaptureName != string.Empty)
          {
            string videoDev = (string)(availableVideoDevices[i]);
            string videoMon = (string)(availableVideoDeviceMonikers[i]);
            if ((String.Compare(videoDev, ccd.CaptureName, true) == 0) &&
                 (videoMon.ToLower().IndexOf(ccd.DeviceId.ToLower()) > -1)) add = true;
          }
          if (add)
          {
            TVCaptureDevice cd = new TVCaptureDevice();

            cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
            cd.ID = cardId;
            if (ccd.CaptureName != string.Empty)
            {
              cd.VideoDevice = ccd.CaptureName;////Hauppauge WinTV PVR PCI II Capture
              cd.CommercialName = ccd.CommercialName;//PVR 150MCE
              cd.CardType = ccd.Capabilities.CardType;
              cd.DeviceId = ccd.DeviceId;
              cd.SupportsTV = ccd.Capabilities.HasTv;
              cd.SupportsRadio = true;
            }

            // only 3 values ?? "Adding name:{0} capture:{1} id:{2} type:{3}",
            Log.Info("Adding name:{0} id:{1} type:{2}",
              cd.CommercialName,
              cd.DeviceId,
              cd.CardType.ToString());
            ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
            int nr = 1;
            foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
            {
              if (cb.CaptureDevice.CommercialName.ToLower() == cbcc.CaptureDevice.CommercialName.ToLower()) nr++;
            }
            cbcc.Number = nr;
            cbcc.MaxCards = nr;
            foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
            {
              if (cb.CaptureDevice.CommercialName.ToLower() == cbcc.CaptureDevice.CommercialName.ToLower())
              {
                cb.MaxCards = nr;
              }
            }
            if (!addNewCard)
            {
              if (deviceToEdit.DeviceId != null)
              {
                if (cbcc.CaptureDevice.DeviceId.ToLower() == deviceToEdit.DeviceId.ToLower())
                {
                  if (deviceToEdit.VideoDeviceMoniker.ToLower() == cbcc.VideoDeviceMoniker.ToLower() &&
                    deviceToEdit.VideoDevice.ToLower() == cbcc.CaptureDevice.VideoDevice.ToLower())
                  {
                    cardComboBox.Items.Add(cbcc);
                    if (deviceToEdit != null && deviceToEdit.CommercialName.ToLower() == cbcc.CaptureDevice.CommercialName.ToLower())
                      cardComboBox.SelectedIndex = cardComboBox.Items.Count - 1;
                  }
                }
              }
            }
            else
            {
              bool alreadyAdded = false;
              foreach (TVCaptureDevice dev in captureCards)
              {
                if (dev.DeviceId != null)
                {
                  if (cbcc.CaptureDevice.DeviceId.ToLower() == dev.DeviceId.ToLower())
                  {
                    if (dev.VideoDeviceMoniker.ToLower() == cbcc.VideoDeviceMoniker.ToLower() &&
                      dev.VideoDevice.ToLower() == cbcc.CaptureDevice.VideoDevice.ToLower())
                    {
                      alreadyAdded = true;
                    }
                  }
                }
              }
              if (!alreadyAdded)
              {
                cardComboBox.Items.Add(cbcc);
                if (cardComboBox.SelectedItem == null)
                  cardComboBox.SelectedIndex = 0;
              }
            }
          }//if (add)
        }//for (int i = 0; i < availableVideoDevices.Count; i++)
      }//foreach (CaptureCardDefinition ccd  in CaptureCardDefinitions.CaptureCards)

      if (cardComboBox.Items.Count == 0)
      {
        MessageBox.Show("No video capture card(s) were found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = cardComboBox.Enabled = okButton.Enabled = false;
        acceptuserinput = false;
      }
      else
        acceptuserinput = true;


      textBoxName.Text = String.Format("card{0}", cardId);
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCaptureCardForm));
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.cropGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpNumericUpDownRight = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpNumericUpDownLeft = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpNumericUpDownBottom = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpNumericUpDownTop = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.cropRightLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cropLeftLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cropBottomLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cropTopLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBox5vAntennae = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
      this.lblFrameRate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFrameSize = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxFrameSize = new System.Windows.Forms.ComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxCAM = new System.Windows.Forms.ComboBox();
      this.checkBoxHWPidFiltering = new System.Windows.Forms.CheckBox();
      this.label24 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.updownPrio = new System.Windows.Forms.NumericUpDown();
      this.cardComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.useRecordingCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.useWatchingCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonBrowse = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbRecordingFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label26 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.cbRgbAudio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbRgbVideo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label35 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBox3Audio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox3Video = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox2Audio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox2Video = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox1Audio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBox1Video = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label17 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage5 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxAudioQuality = new System.Windows.Forms.ComboBox();
      this.groupBox7 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbTVVBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbTVHigh = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbTVMin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbDisableDNR = new System.Windows.Forms.CheckBox();
      this.cbHighVBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbHighMax = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbHighMin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label37 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbMedVBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbMedMax = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbMedMin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label36 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbLowVBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbLowMax = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbLowMin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label34 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label29 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label28 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label27 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbPortVBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbPortMax = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tbPortMin = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label19 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label18 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label25 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxQuality = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxHiQuality = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageAutotune = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPageAutotuneRadio = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.cropGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownRight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownBottom)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownTop)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.updownPrio)).BeginInit();
      this.groupBox5.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.tabPage5.SuspendLayout();
      this.groupBox7.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage5);
      this.tabControl1.Location = new System.Drawing.Point(8, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(550, 450);
      this.tabControl1.TabIndex = 5;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.cropGroupBox);
      this.tabPage1.Controls.Add(this.checkBox5vAntennae);
      this.tabPage1.Controls.Add(this.comboBoxFrameRate);
      this.tabPage1.Controls.Add(this.lblFrameRate);
      this.tabPage1.Controls.Add(this.lblFrameSize);
      this.tabPage1.Controls.Add(this.comboBoxFrameSize);
      this.tabPage1.Controls.Add(this.mpLabel1);
      this.tabPage1.Controls.Add(this.comboBoxCAM);
      this.tabPage1.Controls.Add(this.checkBoxHWPidFiltering);
      this.tabPage1.Controls.Add(this.label24);
      this.tabPage1.Controls.Add(this.updownPrio);
      this.tabPage1.Controls.Add(this.cardComboBox);
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Controls.Add(this.useRecordingCheckBox);
      this.tabPage1.Controls.Add(this.useWatchingCheckBox);
      this.tabPage1.Controls.Add(this.groupBox5);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(542, 424);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Capture card";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // cropGroupBox
      // 
      this.cropGroupBox.Controls.Add(this.mpNumericUpDownRight);
      this.cropGroupBox.Controls.Add(this.mpNumericUpDownLeft);
      this.cropGroupBox.Controls.Add(this.mpNumericUpDownBottom);
      this.cropGroupBox.Controls.Add(this.mpNumericUpDownTop);
      this.cropGroupBox.Controls.Add(this.cropRightLabel);
      this.cropGroupBox.Controls.Add(this.cropLeftLabel);
      this.cropGroupBox.Controls.Add(this.cropBottomLabel);
      this.cropGroupBox.Controls.Add(this.cropTopLabel);
      this.cropGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cropGroupBox.Location = new System.Drawing.Point(11, 314);
      this.cropGroupBox.Name = "cropGroupBox";
      this.cropGroupBox.Size = new System.Drawing.Size(424, 100);
      this.cropGroupBox.TabIndex = 67;
      this.cropGroupBox.TabStop = false;
      this.cropGroupBox.Text = "Crop settings:";
      // 
      // mpNumericUpDownRight
      // 
      this.mpNumericUpDownRight.Location = new System.Drawing.Point(267, 43);
      this.mpNumericUpDownRight.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.mpNumericUpDownRight.Name = "mpNumericUpDownRight";
      this.mpNumericUpDownRight.Size = new System.Drawing.Size(46, 20);
      this.mpNumericUpDownRight.TabIndex = 4;
      // 
      // mpNumericUpDownLeft
      // 
      this.mpNumericUpDownLeft.Location = new System.Drawing.Point(117, 43);
      this.mpNumericUpDownLeft.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.mpNumericUpDownLeft.Name = "mpNumericUpDownLeft";
      this.mpNumericUpDownLeft.Size = new System.Drawing.Size(46, 20);
      this.mpNumericUpDownLeft.TabIndex = 3;
      // 
      // mpNumericUpDownBottom
      // 
      this.mpNumericUpDownBottom.Location = new System.Drawing.Point(192, 72);
      this.mpNumericUpDownBottom.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.mpNumericUpDownBottom.Name = "mpNumericUpDownBottom";
      this.mpNumericUpDownBottom.Size = new System.Drawing.Size(46, 20);
      this.mpNumericUpDownBottom.TabIndex = 6;
      // 
      // mpNumericUpDownTop
      // 
      this.mpNumericUpDownTop.Location = new System.Drawing.Point(192, 14);
      this.mpNumericUpDownTop.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.mpNumericUpDownTop.Name = "mpNumericUpDownTop";
      this.mpNumericUpDownTop.Size = new System.Drawing.Size(46, 20);
      this.mpNumericUpDownTop.TabIndex = 1;
      // 
      // cropRightLabel
      // 
      this.cropRightLabel.AutoSize = true;
      this.cropRightLabel.Location = new System.Drawing.Point(316, 47);
      this.cropRightLabel.Name = "cropRightLabel";
      this.cropRightLabel.Size = new System.Drawing.Size(32, 13);
      this.cropRightLabel.TabIndex = 5;
      this.cropRightLabel.Text = "Right";
      // 
      // cropLeftLabel
      // 
      this.cropLeftLabel.AutoSize = true;
      this.cropLeftLabel.Location = new System.Drawing.Point(89, 46);
      this.cropLeftLabel.Name = "cropLeftLabel";
      this.cropLeftLabel.Size = new System.Drawing.Size(25, 13);
      this.cropLeftLabel.TabIndex = 2;
      this.cropLeftLabel.Text = "Left";
      // 
      // cropBottomLabel
      // 
      this.cropBottomLabel.AutoSize = true;
      this.cropBottomLabel.Location = new System.Drawing.Point(241, 76);
      this.cropBottomLabel.Name = "cropBottomLabel";
      this.cropBottomLabel.Size = new System.Drawing.Size(40, 13);
      this.cropBottomLabel.TabIndex = 7;
      this.cropBottomLabel.Text = "Bottom";
      // 
      // cropTopLabel
      // 
      this.cropTopLabel.AutoSize = true;
      this.cropTopLabel.Location = new System.Drawing.Point(163, 17);
      this.cropTopLabel.Name = "cropTopLabel";
      this.cropTopLabel.Size = new System.Drawing.Size(26, 13);
      this.cropTopLabel.TabIndex = 0;
      this.cropTopLabel.Text = "Top";
      // 
      // checkBox5vAntennae
      // 
      this.checkBox5vAntennae.AutoSize = true;
      this.checkBox5vAntennae.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBox5vAntennae.Location = new System.Drawing.Point(226, 160);
      this.checkBox5vAntennae.Name = "checkBox5vAntennae";
      this.checkBox5vAntennae.Size = new System.Drawing.Size(233, 17);
      this.checkBox5vAntennae.TabIndex = 66;
      this.checkBox5vAntennae.Text = "TechnoTrend USB - 5V antennae activation";
      this.checkBox5vAntennae.UseVisualStyleBackColor = true;
      this.checkBox5vAntennae.CheckedChanged += new System.EventHandler(this.checkBox5vAntennae_CheckedChanged);
      // 
      // comboBoxFrameRate
      // 
      this.comboBoxFrameRate.FormattingEnabled = true;
      this.comboBoxFrameRate.Items.AddRange(new object[] {
            "29.97 fps (NTSC)",
            "25 fps (PAL/SECAM)",
            "15 fps"});
      this.comboBoxFrameRate.Location = new System.Drawing.Point(375, 125);
      this.comboBoxFrameRate.Name = "comboBoxFrameRate";
      this.comboBoxFrameRate.Size = new System.Drawing.Size(151, 21);
      this.comboBoxFrameRate.TabIndex = 64;
      // 
      // lblFrameRate
      // 
      this.lblFrameRate.Location = new System.Drawing.Point(356, 106);
      this.lblFrameRate.Name = "lblFrameRate";
      this.lblFrameRate.Size = new System.Drawing.Size(79, 16);
      this.lblFrameRate.TabIndex = 65;
      this.lblFrameRate.Text = "Frame Rate:";
      // 
      // lblFrameSize
      // 
      this.lblFrameSize.Location = new System.Drawing.Point(9, 108);
      this.lblFrameSize.Name = "lblFrameSize";
      this.lblFrameSize.Size = new System.Drawing.Size(73, 16);
      this.lblFrameSize.TabIndex = 63;
      this.lblFrameSize.Text = "Frame size:";
      // 
      // comboBoxFrameSize
      // 
      this.comboBoxFrameSize.FormattingEnabled = true;
      this.comboBoxFrameSize.Items.AddRange(new object[] {
            "768x576 PAL square pixels",
            "720x576 PAL/SECAM ITU-601 D1 (recommended)",
            "720x480 NTSC ITU-601 D1 (recommended)",
            "704x576 PAL/SECAM TV broadcast",
            "704x480 NTSC TV broadcast",
            "640x480 NTSC square pixels",
            "352x288 PAL CIF",
            "352x240 NTSC CIF",
            "320x240 NTSC CIF square pixels"});
      this.comboBoxFrameSize.Location = new System.Drawing.Point(35, 125);
      this.comboBoxFrameSize.Name = "comboBoxFrameSize";
      this.comboBoxFrameSize.Size = new System.Drawing.Size(299, 21);
      this.comboBoxFrameSize.TabIndex = 62;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(356, 46);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(110, 16);
      this.mpLabel1.TabIndex = 55;
      this.mpLabel1.Text = "Twinhan CAM type:";
      // 
      // comboBoxCAM
      // 
      this.comboBoxCAM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCAM.FormattingEnabled = true;
      this.comboBoxCAM.Items.AddRange(new object[] {
            "Default",
            "Viaccess",
            "Aston",
            "Conax",
            "Cryptoworks"});
      this.comboBoxCAM.Location = new System.Drawing.Point(405, 65);
      this.comboBoxCAM.Name = "comboBoxCAM";
      this.comboBoxCAM.Size = new System.Drawing.Size(121, 21);
      this.comboBoxCAM.TabIndex = 54;
      // 
      // checkBoxHWPidFiltering
      // 
      this.checkBoxHWPidFiltering.AutoSize = true;
      this.checkBoxHWPidFiltering.Location = new System.Drawing.Point(11, 77);
      this.checkBoxHWPidFiltering.Name = "checkBoxHWPidFiltering";
      this.checkBoxHWPidFiltering.Size = new System.Drawing.Size(323, 17);
      this.checkBoxHWPidFiltering.TabIndex = 53;
      this.checkBoxHWPidFiltering.Text = "Enable Hardware Pid filtering (needed for high-bitrate channels)";
      this.checkBoxHWPidFiltering.UseVisualStyleBackColor = true;
      // 
      // label24
      // 
      this.label24.Location = new System.Drawing.Point(9, 46);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(136, 16);
      this.label24.TabIndex = 52;
      this.label24.Text = "Priority (1=low,10=high)";
      // 
      // updownPrio
      // 
      this.updownPrio.Location = new System.Drawing.Point(152, 42);
      this.updownPrio.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.updownPrio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.updownPrio.Name = "updownPrio";
      this.updownPrio.Size = new System.Drawing.Size(56, 20);
      this.updownPrio.TabIndex = 51;
      this.updownPrio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // cardComboBox
      // 
      this.cardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cardComboBox.BorderColor = System.Drawing.Color.Empty;
      this.cardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cardComboBox.Location = new System.Drawing.Point(112, 8);
      this.cardComboBox.Name = "cardComboBox";
      this.cardComboBox.Size = new System.Drawing.Size(414, 21);
      this.cardComboBox.TabIndex = 0;
      this.cardComboBox.SelectedIndexChanged += new System.EventHandler(this.cardComboBox_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "TV Capture card";
      // 
      // useRecordingCheckBox
      // 
      this.useRecordingCheckBox.AutoSize = true;
      this.useRecordingCheckBox.Checked = true;
      this.useRecordingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useRecordingCheckBox.Enabled = false;
      this.useRecordingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useRecordingCheckBox.Location = new System.Drawing.Point(11, 183);
      this.useRecordingCheckBox.Name = "useRecordingCheckBox";
      this.useRecordingCheckBox.Size = new System.Drawing.Size(165, 17);
      this.useRecordingCheckBox.TabIndex = 9;
      this.useRecordingCheckBox.Text = "Use this card for recording TV";
      this.useRecordingCheckBox.UseVisualStyleBackColor = true;
      this.useRecordingCheckBox.CheckedChanged += new System.EventHandler(this.useRecording_Check_Changed);
      // 
      // useWatchingCheckBox
      // 
      this.useWatchingCheckBox.AutoSize = true;
      this.useWatchingCheckBox.Checked = true;
      this.useWatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useWatchingCheckBox.Enabled = false;
      this.useWatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useWatchingCheckBox.Location = new System.Drawing.Point(11, 160);
      this.useWatchingCheckBox.Name = "useWatchingCheckBox";
      this.useWatchingCheckBox.Size = new System.Drawing.Size(157, 17);
      this.useWatchingCheckBox.TabIndex = 8;
      this.useWatchingCheckBox.Text = "Use this card for viewing TV";
      this.useWatchingCheckBox.UseVisualStyleBackColor = true;
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.buttonBrowse);
      this.groupBox5.Controls.Add(this.tbRecordingFolder);
      this.groupBox5.Controls.Add(this.label26);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(11, 207);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(424, 100);
      this.groupBox5.TabIndex = 61;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Recording folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Location = new System.Drawing.Point(280, 56);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(56, 23);
      this.buttonBrowse.TabIndex = 58;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click_1);
      // 
      // tbRecordingFolder
      // 
      this.tbRecordingFolder.BorderColor = System.Drawing.Color.Empty;
      this.tbRecordingFolder.Location = new System.Drawing.Point(24, 56);
      this.tbRecordingFolder.Name = "tbRecordingFolder";
      this.tbRecordingFolder.Size = new System.Drawing.Size(248, 20);
      this.tbRecordingFolder.TabIndex = 57;
      // 
      // label26
      // 
      this.label26.Location = new System.Drawing.Point(21, 30);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(384, 23);
      this.label26.TabIndex = 60;
      this.label26.Text = "Specify the folder in which the recordings of this tv card should be saved:";
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.cbRgbAudio);
      this.tabPage3.Controls.Add(this.cbRgbVideo);
      this.tabPage3.Controls.Add(this.label35);
      this.tabPage3.Controls.Add(this.label14);
      this.tabPage3.Controls.Add(this.comboBox3Audio);
      this.tabPage3.Controls.Add(this.comboBox3Video);
      this.tabPage3.Controls.Add(this.comboBox2Audio);
      this.tabPage3.Controls.Add(this.comboBox2Video);
      this.tabPage3.Controls.Add(this.comboBox1Audio);
      this.tabPage3.Controls.Add(this.comboBox1Video);
      this.tabPage3.Controls.Add(this.label15);
      this.tabPage3.Controls.Add(this.label16);
      this.tabPage3.Controls.Add(this.label17);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(542, 424);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Audio mapping";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // cbRgbAudio
      // 
      this.cbRgbAudio.BorderColor = System.Drawing.Color.Empty;
      this.cbRgbAudio.Location = new System.Drawing.Point(72, 264);
      this.cbRgbAudio.Name = "cbRgbAudio";
      this.cbRgbAudio.Size = new System.Drawing.Size(121, 21);
      this.cbRgbAudio.TabIndex = 21;
      // 
      // cbRgbVideo
      // 
      this.cbRgbVideo.BorderColor = System.Drawing.Color.Empty;
      this.cbRgbVideo.Location = new System.Drawing.Point(72, 240);
      this.cbRgbVideo.Name = "cbRgbVideo";
      this.cbRgbVideo.Size = new System.Drawing.Size(121, 21);
      this.cbRgbVideo.TabIndex = 20;
      // 
      // label35
      // 
      this.label35.Location = new System.Drawing.Point(16, 248);
      this.label35.Name = "label35";
      this.label35.Size = new System.Drawing.Size(40, 23);
      this.label35.TabIndex = 19;
      this.label35.Text = "RGB";
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(16, 16);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(328, 40);
      this.label14.TabIndex = 18;
      this.label14.Text = "Map the video/audio inputs of your TV card to the CVBS1, CVBS2 and SVHS channels";
      // 
      // comboBox3Audio
      // 
      this.comboBox3Audio.BorderColor = System.Drawing.Color.Empty;
      this.comboBox3Audio.Location = new System.Drawing.Point(72, 208);
      this.comboBox3Audio.Name = "comboBox3Audio";
      this.comboBox3Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox3Audio.TabIndex = 16;
      // 
      // comboBox3Video
      // 
      this.comboBox3Video.BorderColor = System.Drawing.Color.Empty;
      this.comboBox3Video.Location = new System.Drawing.Point(72, 184);
      this.comboBox3Video.Name = "comboBox3Video";
      this.comboBox3Video.Size = new System.Drawing.Size(121, 21);
      this.comboBox3Video.TabIndex = 15;
      // 
      // comboBox2Audio
      // 
      this.comboBox2Audio.BorderColor = System.Drawing.Color.Empty;
      this.comboBox2Audio.Location = new System.Drawing.Point(72, 144);
      this.comboBox2Audio.Name = "comboBox2Audio";
      this.comboBox2Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox2Audio.TabIndex = 14;
      // 
      // comboBox2Video
      // 
      this.comboBox2Video.BorderColor = System.Drawing.Color.Empty;
      this.comboBox2Video.Location = new System.Drawing.Point(72, 120);
      this.comboBox2Video.Name = "comboBox2Video";
      this.comboBox2Video.Size = new System.Drawing.Size(121, 21);
      this.comboBox2Video.TabIndex = 13;
      // 
      // comboBox1Audio
      // 
      this.comboBox1Audio.BorderColor = System.Drawing.Color.Empty;
      this.comboBox1Audio.Location = new System.Drawing.Point(72, 88);
      this.comboBox1Audio.Name = "comboBox1Audio";
      this.comboBox1Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox1Audio.TabIndex = 10;
      // 
      // comboBox1Video
      // 
      this.comboBox1Video.BorderColor = System.Drawing.Color.Empty;
      this.comboBox1Video.Location = new System.Drawing.Point(72, 64);
      this.comboBox1Video.Name = "comboBox1Video";
      this.comboBox1Video.Size = new System.Drawing.Size(121, 21);
      this.comboBox1Video.TabIndex = 8;
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(16, 200);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(40, 23);
      this.label15.TabIndex = 12;
      this.label15.Text = "SVHS";
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(8, 136);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(48, 16);
      this.label16.TabIndex = 11;
      this.label16.Text = "CVBS#2";
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(8, 80);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(56, 16);
      this.label17.TabIndex = 9;
      this.label17.Text = "CVBS #1";
      // 
      // tabPage5
      // 
      this.tabPage5.Controls.Add(this.mpLabel2);
      this.tabPage5.Controls.Add(this.comboBoxAudioQuality);
      this.tabPage5.Controls.Add(this.groupBox7);
      this.tabPage5.Controls.Add(this.label18);
      this.tabPage5.Controls.Add(this.label25);
      this.tabPage5.Controls.Add(this.comboBoxQuality);
      this.tabPage5.Controls.Add(this.checkBoxHiQuality);
      this.tabPage5.Location = new System.Drawing.Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Size = new System.Drawing.Size(542, 424);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "Quality";
      this.tabPage5.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(42, 71);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(80, 16);
      this.mpLabel2.TabIndex = 13;
      this.mpLabel2.Text = "Audio Quality:";
      // 
      // comboBoxAudioQuality
      // 
      this.comboBoxAudioQuality.FormattingEnabled = true;
      this.comboBoxAudioQuality.Items.AddRange(new object[] {
            "192 Kbps",
            "224 Kbps",
            "256 Kbps",
            "384 Kbps"});
      this.comboBoxAudioQuality.Location = new System.Drawing.Point(128, 71);
      this.comboBoxAudioQuality.Name = "comboBoxAudioQuality";
      this.comboBoxAudioQuality.Size = new System.Drawing.Size(128, 21);
      this.comboBoxAudioQuality.TabIndex = 12;
      // 
      // groupBox7
      // 
      this.groupBox7.Controls.Add(this.cbTVVBR);
      this.groupBox7.Controls.Add(this.tbTVHigh);
      this.groupBox7.Controls.Add(this.tbTVMin);
      this.groupBox7.Controls.Add(this.mpLabel3);
      this.groupBox7.Controls.Add(this.cbDisableDNR);
      this.groupBox7.Controls.Add(this.cbHighVBR);
      this.groupBox7.Controls.Add(this.tbHighMax);
      this.groupBox7.Controls.Add(this.tbHighMin);
      this.groupBox7.Controls.Add(this.label37);
      this.groupBox7.Controls.Add(this.cbMedVBR);
      this.groupBox7.Controls.Add(this.tbMedMax);
      this.groupBox7.Controls.Add(this.tbMedMin);
      this.groupBox7.Controls.Add(this.label36);
      this.groupBox7.Controls.Add(this.cbLowVBR);
      this.groupBox7.Controls.Add(this.tbLowMax);
      this.groupBox7.Controls.Add(this.tbLowMin);
      this.groupBox7.Controls.Add(this.label34);
      this.groupBox7.Controls.Add(this.label29);
      this.groupBox7.Controls.Add(this.label28);
      this.groupBox7.Controls.Add(this.label27);
      this.groupBox7.Controls.Add(this.cbPortVBR);
      this.groupBox7.Controls.Add(this.tbPortMax);
      this.groupBox7.Controls.Add(this.tbPortMin);
      this.groupBox7.Controls.Add(this.label19);
      this.groupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox7.Location = new System.Drawing.Point(24, 159);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(456, 247);
      this.groupBox7.TabIndex = 11;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "Quality settings:";
      // 
      // cbTVVBR
      // 
      this.cbTVVBR.AutoSize = true;
      this.cbTVVBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTVVBR.Location = new System.Drawing.Point(248, 183);
      this.cbTVVBR.Name = "cbTVVBR";
      this.cbTVVBR.Size = new System.Drawing.Size(13, 12);
      this.cbTVVBR.TabIndex = 23;
      this.cbTVVBR.UseVisualStyleBackColor = true;
      // 
      // tbTVHigh
      // 
      this.tbTVHigh.BorderColor = System.Drawing.Color.Empty;
      this.tbTVHigh.Location = new System.Drawing.Point(168, 183);
      this.tbTVHigh.Name = "tbTVHigh";
      this.tbTVHigh.Size = new System.Drawing.Size(64, 20);
      this.tbTVHigh.TabIndex = 22;
      this.tbTVHigh.Text = "300";
      // 
      // tbTVMin
      // 
      this.tbTVMin.BorderColor = System.Drawing.Color.Empty;
      this.tbTVMin.Location = new System.Drawing.Point(80, 183);
      this.tbTVMin.Name = "tbTVMin";
      this.tbTVMin.Size = new System.Drawing.Size(64, 20);
      this.tbTVMin.TabIndex = 21;
      this.tbTVMin.Text = "100";
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(16, 183);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(64, 16);
      this.mpLabel3.TabIndex = 20;
      this.mpLabel3.Text = "Live TV";
      // 
      // cbDisableDNR
      // 
      this.cbDisableDNR.AutoSize = true;
      this.cbDisableDNR.Location = new System.Drawing.Point(19, 224);
      this.cbDisableDNR.Name = "cbDisableDNR";
      this.cbDisableDNR.Size = new System.Drawing.Size(187, 17);
      this.cbDisableDNR.TabIndex = 19;
      this.cbDisableDNR.Text = "Disable Dynamic Noise Reduction";
      this.toolTip1.SetToolTip(this.cbDisableDNR, "If you have a clean input source, such as satellite, disable DNR");
      this.cbDisableDNR.UseVisualStyleBackColor = true;
      // 
      // cbHighVBR
      // 
      this.cbHighVBR.AutoSize = true;
      this.cbHighVBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbHighVBR.Location = new System.Drawing.Point(248, 152);
      this.cbHighVBR.Name = "cbHighVBR";
      this.cbHighVBR.Size = new System.Drawing.Size(13, 12);
      this.cbHighVBR.TabIndex = 18;
      this.cbHighVBR.UseVisualStyleBackColor = true;
      // 
      // tbHighMax
      // 
      this.tbHighMax.BorderColor = System.Drawing.Color.Empty;
      this.tbHighMax.Location = new System.Drawing.Point(168, 152);
      this.tbHighMax.Name = "tbHighMax";
      this.tbHighMax.Size = new System.Drawing.Size(64, 20);
      this.tbHighMax.TabIndex = 17;
      this.tbHighMax.Text = "300";
      // 
      // tbHighMin
      // 
      this.tbHighMin.BorderColor = System.Drawing.Color.Empty;
      this.tbHighMin.Location = new System.Drawing.Point(80, 152);
      this.tbHighMin.Name = "tbHighMin";
      this.tbHighMin.Size = new System.Drawing.Size(64, 20);
      this.tbHighMin.TabIndex = 16;
      this.tbHighMin.Text = "100";
      // 
      // label37
      // 
      this.label37.Location = new System.Drawing.Point(16, 152);
      this.label37.Name = "label37";
      this.label37.Size = new System.Drawing.Size(64, 16);
      this.label37.TabIndex = 15;
      this.label37.Text = "High:";
      // 
      // cbMedVBR
      // 
      this.cbMedVBR.AutoSize = true;
      this.cbMedVBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbMedVBR.Location = new System.Drawing.Point(248, 120);
      this.cbMedVBR.Name = "cbMedVBR";
      this.cbMedVBR.Size = new System.Drawing.Size(13, 12);
      this.cbMedVBR.TabIndex = 14;
      this.cbMedVBR.UseVisualStyleBackColor = true;
      // 
      // tbMedMax
      // 
      this.tbMedMax.BorderColor = System.Drawing.Color.Empty;
      this.tbMedMax.Location = new System.Drawing.Point(168, 120);
      this.tbMedMax.Name = "tbMedMax";
      this.tbMedMax.Size = new System.Drawing.Size(64, 20);
      this.tbMedMax.TabIndex = 13;
      this.tbMedMax.Text = "300";
      // 
      // tbMedMin
      // 
      this.tbMedMin.BorderColor = System.Drawing.Color.Empty;
      this.tbMedMin.Location = new System.Drawing.Point(80, 120);
      this.tbMedMin.Name = "tbMedMin";
      this.tbMedMin.Size = new System.Drawing.Size(64, 20);
      this.tbMedMin.TabIndex = 12;
      this.tbMedMin.Text = "100";
      // 
      // label36
      // 
      this.label36.Location = new System.Drawing.Point(16, 120);
      this.label36.Name = "label36";
      this.label36.Size = new System.Drawing.Size(64, 16);
      this.label36.TabIndex = 11;
      this.label36.Text = "Medium:";
      // 
      // cbLowVBR
      // 
      this.cbLowVBR.AutoSize = true;
      this.cbLowVBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbLowVBR.Location = new System.Drawing.Point(248, 88);
      this.cbLowVBR.Name = "cbLowVBR";
      this.cbLowVBR.Size = new System.Drawing.Size(13, 12);
      this.cbLowVBR.TabIndex = 10;
      this.cbLowVBR.UseVisualStyleBackColor = true;
      // 
      // tbLowMax
      // 
      this.tbLowMax.BorderColor = System.Drawing.Color.Empty;
      this.tbLowMax.Location = new System.Drawing.Point(168, 88);
      this.tbLowMax.Name = "tbLowMax";
      this.tbLowMax.Size = new System.Drawing.Size(64, 20);
      this.tbLowMax.TabIndex = 9;
      this.tbLowMax.Text = "300";
      // 
      // tbLowMin
      // 
      this.tbLowMin.BorderColor = System.Drawing.Color.Empty;
      this.tbLowMin.Location = new System.Drawing.Point(80, 88);
      this.tbLowMin.Name = "tbLowMin";
      this.tbLowMin.Size = new System.Drawing.Size(64, 20);
      this.tbLowMin.TabIndex = 8;
      this.tbLowMin.Text = "100";
      // 
      // label34
      // 
      this.label34.Location = new System.Drawing.Point(16, 88);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(64, 16);
      this.label34.TabIndex = 7;
      this.label34.Text = "Low:";
      // 
      // label29
      // 
      this.label29.Location = new System.Drawing.Point(248, 24);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(40, 16);
      this.label29.TabIndex = 6;
      this.label29.Text = "VBR";
      // 
      // label28
      // 
      this.label28.Location = new System.Drawing.Point(168, 16);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(72, 32);
      this.label28.TabIndex = 5;
      this.label28.Text = "Peak bitrate (KBPS)";
      // 
      // label27
      // 
      this.label27.Location = new System.Drawing.Point(80, 16);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(88, 32);
      this.label27.TabIndex = 4;
      this.label27.Text = "Average bitrate (KBPS)";
      // 
      // cbPortVBR
      // 
      this.cbPortVBR.AutoSize = true;
      this.cbPortVBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbPortVBR.Location = new System.Drawing.Point(248, 56);
      this.cbPortVBR.Name = "cbPortVBR";
      this.cbPortVBR.Size = new System.Drawing.Size(13, 12);
      this.cbPortVBR.TabIndex = 3;
      this.cbPortVBR.UseVisualStyleBackColor = true;
      // 
      // tbPortMax
      // 
      this.tbPortMax.BorderColor = System.Drawing.Color.Empty;
      this.tbPortMax.Location = new System.Drawing.Point(168, 56);
      this.tbPortMax.Name = "tbPortMax";
      this.tbPortMax.Size = new System.Drawing.Size(64, 20);
      this.tbPortMax.TabIndex = 2;
      this.tbPortMax.Text = "600";
      this.tbPortMax.TextChanged += new System.EventHandler(this.tbPortMax_TextChanged);
      // 
      // tbPortMin
      // 
      this.tbPortMin.BorderColor = System.Drawing.Color.Empty;
      this.tbPortMin.Location = new System.Drawing.Point(80, 56);
      this.tbPortMin.Name = "tbPortMin";
      this.tbPortMin.Size = new System.Drawing.Size(64, 20);
      this.tbPortMin.TabIndex = 1;
      this.tbPortMin.Text = "500";
      this.tbPortMin.TextChanged += new System.EventHandler(this.tbPortMin_TextChanged);
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(16, 56);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(64, 16);
      this.label19.TabIndex = 0;
      this.label19.Text = "Portable:";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(21, 102);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(408, 60);
      this.label18.TabIndex = 10;
      this.label18.Text = resources.GetString("label18.Text");
      // 
      // label25
      // 
      this.label25.Location = new System.Drawing.Point(40, 38);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(80, 16);
      this.label25.TabIndex = 9;
      this.label25.Text = "Default quality:";
      // 
      // comboBoxQuality
      // 
      this.comboBoxQuality.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxQuality.Items.AddRange(new object[] {
            "Portable",
            "Low",
            "Medium",
            "High"});
      this.comboBoxQuality.Location = new System.Drawing.Point(128, 38);
      this.comboBoxQuality.Name = "comboBoxQuality";
      this.comboBoxQuality.Size = new System.Drawing.Size(240, 21);
      this.comboBoxQuality.TabIndex = 8;
      // 
      // checkBoxHiQuality
      // 
      this.checkBoxHiQuality.AutoSize = true;
      this.checkBoxHiQuality.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHiQuality.Location = new System.Drawing.Point(24, 11);
      this.checkBoxHiQuality.Name = "checkBoxHiQuality";
      this.checkBoxHiQuality.Size = new System.Drawing.Size(113, 17);
      this.checkBoxHiQuality.TabIndex = 7;
      this.checkBoxHiQuality.Text = "Use Quality control";
      this.checkBoxHiQuality.UseVisualStyleBackColor = true;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(391, 464);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.Location = new System.Drawing.Point(471, 464);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // textBoxName
      // 
      this.textBoxName.BorderColor = System.Drawing.Color.Empty;
      this.textBoxName.Location = new System.Drawing.Point(0, 0);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(100, 20);
      this.textBoxName.TabIndex = 0;
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(0, 0);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(100, 23);
      this.label12.TabIndex = 0;
      // 
      // tabPageAutotune
      // 
      this.tabPageAutotune.Location = new System.Drawing.Point(4, 22);
      this.tabPageAutotune.Name = "tabPageAutotune";
      this.tabPageAutotune.Size = new System.Drawing.Size(542, 424);
      this.tabPageAutotune.TabIndex = 1;
      this.tabPageAutotune.Text = "Autotune";
      this.tabPageAutotune.UseVisualStyleBackColor = true;
      this.tabPageAutotune.Enter += new System.EventHandler(this.tabPageAutotune_Enter);
      // 
      // tabPageAutotuneRadio
      // 
      this.tabPageAutotuneRadio.Location = new System.Drawing.Point(4, 22);
      this.tabPageAutotuneRadio.Name = "tabPageAutotuneRadio";
      this.tabPageAutotuneRadio.Size = new System.Drawing.Size(542, 424);
      this.tabPageAutotuneRadio.TabIndex = 1;
      this.tabPageAutotuneRadio.Text = "Autotune Radio";
      this.tabPageAutotuneRadio.UseVisualStyleBackColor = true;
      this.tabPageAutotuneRadio.Enter += new System.EventHandler(this.tabPageAutotuneRadio_Enter);
      // 
      // EditCaptureCardForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(564, 496);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MinimumSize = new System.Drawing.Size(480, 296);
      this.Name = "EditCaptureCardForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit properties of your TV card";
      this.Load += new System.EventHandler(this.EditCaptureCardForm_Load);
      this.Closing += new System.ComponentModel.CancelEventHandler(this.EditCaptureCardForm_Closing);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.cropGroupBox.ResumeLayout(false);
      this.cropGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownRight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownBottom)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.mpNumericUpDownTop)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.updownPrio)).EndInit();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.tabPage3.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.tabPage5.PerformLayout();
      this.groupBox7.ResumeLayout(false);
      this.groupBox7.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    void FillInDefaultRecordingPath()
    {
      if (tbRecordingFolder.Text != string.Empty) return;

      string recFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      recFolder += @"\My Recordings";
      try
      {
        System.IO.Directory.CreateDirectory(recFolder);
      }
      catch (Exception) { }
      tbRecordingFolder.Text = recFolder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void okButton_Click(object sender, System.EventArgs e)
    {
      if (tbRecordingFolder.Text == string.Empty)
      {
        tabControl1.SelectedTab = tabPage1;
        MessageBox.Show("No recording folder specified", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      SaveAllSettings();
      this.DialogResult = DialogResult.OK;
      this.Hide();

    }
    private void useRecording_Check_Changed(object sender, System.EventArgs e)
    {
      groupBox5.Enabled = useRecordingCheckBox.Checked;
    }

    private void EditCaptureCardForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      IEnumerator enumerator = this.tabPageAutotune.Controls.GetEnumerator();
      if (enumerator.MoveNext())
      {
        Sections.Wizard_ScanBase currentDialog = (Sections.Wizard_ScanBase)enumerator.Current;
        if ((currentDialog != null) && (currentDialog.Scanning))
        {
          tabControl1.SelectedTab = tabPageAutotune;
          MessageBox.Show("Please stop autotuning first", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          e.Cancel = true;
          return;
        }
      }
      if (CaptureCard != null)
      {
        CaptureCard.DeleteGraph();
      }
    }

    void SaveAllSettings()
    {

      if (CaptureCard != null && CaptureCard.FriendlyName != string.Empty)
      {
        try
        {
          string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), CaptureCard.FriendlyName);
          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(filename))
          {
            Log.Debug("*** HiQuality {0}", Convert.ToString(checkBoxHiQuality.Checked));
            xmlwriter.SetValueAsBool("quality", "enabled", checkBoxHiQuality.Checked);

            Log.Debug("*** quality {0}", Convert.ToString(comboBoxQuality.SelectedIndex));
            xmlwriter.SetValue("quality", "default", comboBoxQuality.SelectedIndex);

            Log.Debug("*** audio1 {0}", Convert.ToString(comboBox1Audio.SelectedIndex));
            xmlwriter.SetValue("mapping", "audio1", comboBox1Audio.SelectedIndex);

            Log.Debug("*** audio2 {0}", Convert.ToString(comboBox2Audio.SelectedIndex));
            xmlwriter.SetValue("mapping", "audio2", comboBox2Audio.SelectedIndex);

            Log.Debug("*** audio3 {0}", Convert.ToString(comboBox3Audio.SelectedIndex));
            xmlwriter.SetValue("mapping", "audio3", comboBox3Audio.SelectedIndex);

            Log.Debug("*** audio4 {0}", Convert.ToString(cbRgbAudio.SelectedIndex));
            xmlwriter.SetValue("mapping", "audio4", cbRgbAudio.SelectedIndex);

            Log.Debug("*** audiodefault {0}", Convert.ToString(comboBoxAudioQuality.SelectedIndex));
            xmlwriter.SetValue("quality", "audiodefault", comboBoxAudioQuality.SelectedIndex);

            Log.Debug("*** audioquality {0}", comboBoxAudioQuality.SelectedItem.ToString().Substring(0, 3));
            xmlwriter.SetValue("quality", "audioquality", comboBoxAudioQuality.SelectedItem.ToString().Substring(0, 3));

            Log.Debug("*** video1 {0}", Convert.ToString(comboBox1Video.SelectedIndex));
            xmlwriter.SetValue("mapping", "video1", comboBox1Video.SelectedIndex);

            Log.Debug("*** video2 {0}", Convert.ToString(comboBox2Video.SelectedIndex));
            xmlwriter.SetValue("mapping", "video2", comboBox2Video.SelectedIndex);

            Log.Debug("*** video3 {0}", Convert.ToString(comboBox3Video.SelectedIndex));
            xmlwriter.SetValue("mapping", "video3", comboBox3Video.SelectedIndex);

            Log.Debug("*** video4 {0}", Convert.ToString(cbRgbVideo.SelectedIndex));
            xmlwriter.SetValue("mapping", "video4", cbRgbVideo.SelectedIndex);


            Log.Debug("*** portLow {0}", tbPortMin.Text);
            xmlwriter.SetValue("quality", "portLow", tbPortMin.Text);

            Log.Debug("*** portMax {0}", tbPortMax.Text);
            xmlwriter.SetValue("quality", "portMax", tbPortMax.Text);

            Log.Debug("*** portVBR {0}", Convert.ToString(cbPortVBR.Checked));
            xmlwriter.SetValueAsBool("quality", "portVBR", cbPortVBR.Checked);


            Log.Debug("*** LowLow {0}", tbLowMin.Text);
            xmlwriter.SetValue("quality", "LowLow", tbLowMin.Text);

            Log.Debug("*** LowMax {0}", tbLowMax.Text);
            xmlwriter.SetValue("quality", "LowMax", tbLowMax.Text);

            Log.Debug("*** LowVBR {0}", Convert.ToString(cbLowVBR.Checked));
            xmlwriter.SetValueAsBool("quality", "LowVBR", cbLowVBR.Checked);

            Log.Debug("*** MedLow {0}", tbMedMin.Text);
            xmlwriter.SetValue("quality", "MedLow", tbMedMin.Text);

            Log.Debug("*** MedMax {0}", tbMedMax.Text);
            xmlwriter.SetValue("quality", "MedMax", tbMedMax.Text);

            Log.Debug("*** MedVBR {0}", Convert.ToString(cbMedVBR.Checked));
            xmlwriter.SetValueAsBool("quality", "MedVBR", cbMedVBR.Checked);


            Log.Debug("*** HighLow {0}", tbHighMin.Text);
            xmlwriter.SetValue("quality", "HighLow", tbHighMin.Text);

            Log.Debug("*** HighMax {0}", tbHighMax.Text);
            xmlwriter.SetValue("quality", "HighMax", tbHighMax.Text);

            Log.Debug("*** HighVBR {0}", Convert.ToString(cbHighVBR.Checked));
            xmlwriter.SetValueAsBool("quality", "HighVBR", cbHighVBR.Checked);

            Log.Debug("*** TVLow {0}", tbTVMin.Text);
            xmlwriter.SetValue("quality", "TVLow", tbTVMin.Text);

            Log.Debug("*** TVHigh {0}", tbTVHigh.Text);
            xmlwriter.SetValue("quality", "TVHigh", tbTVHigh.Text);

            Log.Debug("*** TVVBR {0}", Convert.ToString(cbTVVBR.Checked));
            xmlwriter.SetValueAsBool("quality", "TVVBR", cbTVVBR.Checked);

            Log.Debug("*** Disable DNR {0}", Convert.ToString(cbDisableDNR.Checked));
            xmlwriter.SetValueAsBool("quality", "DNR", cbDisableDNR.Checked);

            Log.Debug("*** hwfiltering {0}", Convert.ToString(checkBoxHWPidFiltering.Checked));
            if (checkBoxHWPidFiltering.Visible)
              xmlwriter.SetValueAsBool("general", "hwfiltering", checkBoxHWPidFiltering.Checked);

            Log.Debug("*** cam {0}", Convert.ToString(comboBoxCAM.SelectedItem));
            if (comboBoxCAM.Visible)
              xmlwriter.SetValue("dvbs", "cam", comboBoxCAM.SelectedItem);

            Log.Debug("*** framerate {0}", comboBoxFrameRate.Text);
            if (comboBoxFrameRate.Visible)
              xmlwriter.SetValue("analog", "framerate", comboBoxFrameRate.Text);

            Log.Debug("*** framesize {0}", comboBoxFrameSize.Text);
            if (comboBoxFrameSize.Visible)
              xmlwriter.SetValue("analog", "framesize", comboBoxFrameSize.Text);

            Log.Debug("*** Antennae5v {0}", Convert.ToString(checkBox5vAntennae.Checked));
            if (checkBox5vAntennae.Visible)
              xmlwriter.SetValueAsBool("general", "Antennae5v", checkBox5vAntennae.Checked);
          }
        }
        catch (Exception ex)
        {
          Log.Error("EditCaptureCardForm: ERROR saving card {0} - {1} , {2} , {3}", CaptureCard.FriendlyName, ex.Message, ex.Source, ex.StackTrace);
        }

        try
        {
          IEnumerator enumerator = this.tabPageAutotune.Controls.GetEnumerator();
          if (enumerator.MoveNext())
          {
            Sections.Wizard_ScanBase currentDialog = (Sections.Wizard_ScanBase)enumerator.Current;
            if (currentDialog != null)
            {
              currentDialog.SaveSettings();
            }
          }
        }
        catch (Exception ex2)
        {
          Log.Error("EditCaptureCardForm: ERROR in Wizard_ScanBase dialog {0} , {1} , {2}", ex2.Message, ex2.Source, ex2.StackTrace);
        }

        try
        {
          // save crop settings
          MediaPortal.Player.CropSettings cropSettings = new MediaPortal.Player.CropSettings(
                      Convert.ToInt32(mpNumericUpDownTop.Value),
                      Convert.ToInt32(mpNumericUpDownBottom.Value),
                      Convert.ToInt32(mpNumericUpDownLeft.Value),
                      Convert.ToInt32(mpNumericUpDownRight.Value)
                      );
          CaptureCard.CropSettings = cropSettings;
        }
        catch (Exception ex3)
        {
          Log.Error("EditCaptureCardForm: ERROR saving crop settings {0} , {1} , {2}", ex3.Message, ex3.Source, ex3.StackTrace);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void cardComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (!acceptuserinput) return;
      if (cardComboBox.SelectedItem == null) return;
      if (((ComboBoxCaptureCard)(cardComboBox.SelectedItem)).CaptureDevice == prevDevice)
      {
        return;
      }

      prevDevice = ((ComboBoxCaptureCard)(cardComboBox.SelectedItem)).CaptureDevice;
      if (!FillInAll())
      {
        tabControl1.Controls.Remove(tabPageAutotune);
        MessageBox.Show("Unable to create graph for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        FillAutotuneTab();
      }
    }



    /// <summary>
    /// #MW#
    /// Fill in all the appropriate fields. The selected card is already checked as a card that
    /// is supported, so definitions might already been loaded.
    /// The createcapturedevice() method builds up a graph, without the connections in order to
    /// display the property pages belonging to the required filters.
    /// 
    /// This was done using the Capture class, which did a mix of the (My)SinkGraph class used by the
    /// TvCapture stuff. For the time being, a kind of hack is used to re-use parts of the sinkgraph
    /// code that builds the graph according to the cards definitions. In the future, a redesign should
    /// be done to remove all obsolete code and refactor the whole capture tv/radio thingy...
    /// 
    /// Depending on the cards capabilities, the rest of the fields should be enabled/disabled
    /// 
    /// Display should give the descriptive name, ie PVR150MCE, PVR350 etc.
    /// </summary>
    bool FillInAll()
    {
      checkBoxHWPidFiltering.Visible = false;
      comboBoxCAM.Visible = false;
      mpLabel1.Visible = false;
      comboBoxFrameRate.Visible = false;
      comboBoxFrameSize.Visible = false;
      lblFrameRate.Visible = false;
      lblFrameSize.Visible = false;
      checkBox5vAntennae.Visible = false;
      TVCaptureDevice capture = CaptureCard;
      useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = cardComboBox.Text.Length > 0;
      try
      {
        if (capture != null)
        {
          if (capture.CreateGraph())
          {
            if (capture.Network != NetworkType.Analog)
            {
              if (capture.SupportsCamSelection)
              {
                comboBoxCAM.Visible = true;
                mpLabel1.Visible = true;
              }
            }
            else
            {
              comboBoxFrameRate.Visible = true;
              comboBoxFrameSize.Visible = true;
              lblFrameRate.Visible = true;
              lblFrameSize.Visible = true;
            }
            if (capture.SupportsHardwarePidFiltering)
            {
              checkBoxHWPidFiltering.Visible = true;
            }
            if (capture.Supports5vAntennae)
            {
                checkBox5vAntennae.Visible = true;
            }
            // load crop settings
            mpNumericUpDownTop.Value = capture.CropSettings.Top;
            mpNumericUpDownBottom.Value = capture.CropSettings.Bottom;
            mpNumericUpDownLeft.Value = capture.CropSettings.Left;
            mpNumericUpDownRight.Value = capture.CropSettings.Right;
          }
          else return false;
        }
      }
      catch (Exception ex)
      {
        Log.Info("FillInAll exception:{0} {1} {2}",
          ex.Message, ex.Source, ex.StackTrace);
        return false;
      }
      finally
      {
        if (capture != null)
        {
          capture.DeleteGraph();
        }
      }
      return true;
    }

    private void FillAutotuneTab()
    {
      TVCaptureDevice capture = CaptureCard;
      NetworkType networkType = capture.Network;
      tabControl1.Controls.Clear();
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPageAutotune);
      if (networkType == NetworkType.Analog)
      {
        if (capture.SupportsRadio)
        {
          this.tabControl1.Controls.Add(this.tabPageAutotuneRadio);
        }
      }
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage5);
      Sections.Wizard_ScanBase currentDialog = null;
      this.tabPageAutotune.Controls.Clear();
      if (networkType == NetworkType.Analog)
      {
        currentDialog = new Sections.Wizard_AnalogTV();
        this.tabPageAutotune.Controls.Add(currentDialog);
        Sections.Wizard_ScanBase radioDialog = new Sections.Wizard_AnalogRadio();
        this.tabPageAutotuneRadio.Controls.Add(radioDialog);
        radioDialog.OnScanFinished += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanFinishedHandler(this.dlg_OnScanFinished);
        radioDialog.OnScanStarted += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanStartedHandler(this.dlg_OnScanStarted);
        radioDialog.Card = capture;
      }
      else if (networkType == NetworkType.DVBC)
      {
        currentDialog = new Sections.Wizard_DVBCTV();
        this.tabPageAutotune.Controls.Add(currentDialog);
      }
      else if (networkType == NetworkType.DVBT)
      {
        currentDialog = new Sections.Wizard_DVBTTV();
        this.tabPageAutotune.Controls.Add(currentDialog);
      }
      else if (networkType == NetworkType.DVBS)
      {
        currentDialog = new Sections.Wizard_DVBSTV();
        this.tabPageAutotune.Controls.Add(currentDialog);
      }
      else if (networkType == NetworkType.ATSC)
      {
        currentDialog = new Sections.Wizard_ATSCTV();
        this.tabPageAutotune.Controls.Add(currentDialog);
      }
      else
      {
        return;
      }
      currentDialog.OnScanFinished += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanFinishedHandler(this.dlg_OnScanFinished);
      currentDialog.OnScanStarted += new MediaPortal.Configuration.Sections.Wizard_ScanBase.ScanStartedHandler(this.dlg_OnScanStarted);
      currentDialog.Card = capture;
    }
    private void EditCaptureCardForm_Load(object sender, System.EventArgs e)
    {
      comboBoxFrameRate.SelectedIndex = 0;
      comboBoxFrameSize.SelectedIndex = 0;
      comboBoxCAM.SelectedIndex = 0;
      if (FillInAll())
      {
        TVCaptureDevice capture = CaptureCard;
        prevDevice = capture;
        if (capture != null)
        {
          FillAutotuneTab();
          if (capture.FriendlyName != string.Empty)
          {
            string filename = String.Format(Config.GetFile(Config.Dir.Database,"card_{0}.xml"), capture.FriendlyName);
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
            {
              checkBoxHiQuality.Checked = xmlreader.GetValueAsBool("quality", "enabled", false);
              comboBoxQuality.SelectedIndex = xmlreader.GetValueAsInt("quality", "default", 2);
              comboBox1Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio1", 0);
              comboBox2Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio2", 1);
              comboBox3Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio3", 0);
              cbRgbAudio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio4", 0);
              comboBoxAudioQuality.SelectedIndex = xmlreader.GetValueAsInt("quality", "audiodefault", 0);

              comboBox1Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video1", 0);
              comboBox2Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video2", 1);
              comboBox3Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video3", 0);
              cbRgbVideo.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video4", 0);

              tbPortMin.Text = xmlreader.GetValueAsInt("quality", "portLow", 100).ToString();
              tbPortMax.Text = xmlreader.GetValueAsInt("quality", "portMax", 300).ToString();
              cbPortVBR.Checked = xmlreader.GetValueAsBool("quality", "portVBR", false);


              tbLowMin.Text = xmlreader.GetValueAsInt("quality", "LowLow", 500).ToString();
              tbLowMax.Text = xmlreader.GetValueAsInt("quality", "LowMax", 1500).ToString();
              cbLowVBR.Checked = xmlreader.GetValueAsBool("quality", "LowVBR", true);

              tbMedMin.Text = xmlreader.GetValueAsInt("quality", "MedLow", 2000).ToString();
              tbMedMax.Text = xmlreader.GetValueAsInt("quality", "MedMax", 4000).ToString();
              cbMedVBR.Checked = xmlreader.GetValueAsBool("quality", "MedVBR", true);

              tbHighMin.Text = xmlreader.GetValueAsInt("quality", "HighLow", 8000).ToString();
              tbHighMax.Text = xmlreader.GetValueAsInt("quality", "HighMax", 12000).ToString();
              if (CaptureCard != null)
              {
                string comName = CaptureCard.CommercialName.ToLower();
                if (comName.IndexOf("usb") >= 0)
                {
                  tbHighMin.Text = xmlreader.GetValueAsInt("quality", "HighLow", 768).ToString();
                  tbHighMax.Text = xmlreader.GetValueAsInt("quality", "HighMax", 4000).ToString();
                }
              }
              cbHighVBR.Checked = xmlreader.GetValueAsBool("quality", "HighVBR", true);

              tbTVMin.Text = xmlreader.GetValueAsInt("quality", "TVLow", 11000).ToString();
              tbTVHigh.Text = xmlreader.GetValueAsInt("quality", "TVHigh", 13000).ToString();
              cbTVVBR.Checked = xmlreader.GetValueAsBool("quality", "TVVBR", true);
              
              cbDisableDNR.Checked = xmlreader.GetValueAsBool("quality", "DNR", false);

              checkBoxHWPidFiltering.Checked = xmlreader.GetValueAsBool("general", "hwfiltering", false);
              checkBox5vAntennae.Checked = xmlreader.GetValueAsBool("general", "Antennae5v", false);
              comboBoxCAM.SelectedItem = xmlreader.GetValueAsString("dvbs", "cam", "Default");
              comboBoxFrameRate.Text = xmlreader.GetValueAsString("analog", "framerate", "25 fps");
              comboBoxFrameSize.Text = xmlreader.GetValueAsString("analog", "framesize", "720x576");
            }
          }
        }
      }
      else
      {
        MessageBox.Show("Unable to create graph for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }


      FillInDefaultRecordingPath();
    }




    public TVCaptureDevice CaptureCard
    {
      get
      {
        if (cardComboBox.SelectedItem == null) return null;

        ComboBoxCaptureCard combo = cardComboBox.SelectedItem as ComboBoxCaptureCard;
        TVCaptureDevice card = (combo).CaptureDevice;



        card.RecordingPath = tbRecordingFolder.Text;
        card.UseForRecording = useRecordingCheckBox.Checked;
        card.UseForTV = useWatchingCheckBox.Checked;
        card.Priority = (int)updownPrio.Value;


        card.FriendlyName = textBoxName.Text;


        if (!checkBoxHiQuality.Checked)
        {
          card.Quality = -1;
        }
        else
        {
          if (comboBoxQuality.SelectedIndex < 0) card.Quality = -1;
          else card.Quality = comboBoxQuality.SelectedIndex;
        }
        return card;
      }

      set
      {
        acceptuserinput = false;
        TVCaptureDevice card = value as TVCaptureDevice;

        if (card != null)
        {
          if (card.Quality < 0)
          {
            checkBoxHiQuality.Checked = false;
          }
          else
          {
            checkBoxHiQuality.Checked = true;
            comboBoxQuality.SelectedIndex = card.Quality;
          }
          tbRecordingFolder.Text = card.RecordingPath;
          if (tbRecordingFolder.Text == "")
          {
            FillInDefaultRecordingPath();
          }


          textBoxName.Text = card.FriendlyName;
          for (int i = 0; i < cardComboBox.Items.Count; ++i)
          {
            ComboBoxCaptureCard cd = (ComboBoxCaptureCard)cardComboBox.Items[i];
            if (card.DeviceId != null)
            {
              if (cd.CaptureDevice.DeviceId.ToLower() == card.DeviceId.ToLower())
              {
                if (card.VideoDeviceMoniker.ToLower() == cd.VideoDeviceMoniker.ToLower() &&
                  card.VideoDevice.ToLower() == cd.CaptureDevice.VideoDevice.ToLower() &&
                  card.CommercialName.ToLower() == cd.CaptureDevice.CommercialName.ToLower())
                {
                  cardComboBox.SelectedIndex = i;
                  break;
                }
              }
            }
          }

          useRecordingCheckBox.Checked = card.UseForRecording;
          useWatchingCheckBox.Checked = card.UseForTV;
          updownPrio.Value = card.Priority;


          FillInAll(); // fill all framerates & audio in types...
          // select the correct framesize

        }
        acceptuserinput = true;
      }
    }
    private void tabPageAutotune_Enter(object sender, System.EventArgs e)
    {

      TVCaptureDevice capture = CaptureCard;
      // save settings for card
      if (capture != null)
      {
        string filename = String.Format(Config.GetFile(Config.Dir.Database,"card_{0}.xml"), capture.FriendlyName);
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(filename))
        {
          if (checkBoxHWPidFiltering.Visible)
            xmlwriter.SetValueAsBool("general", "hwfiltering", checkBoxHWPidFiltering.Checked);
          if (comboBoxCAM.Visible)
            xmlwriter.SetValue("dvbs", "cam", comboBoxCAM.SelectedItem);
          if (comboBoxFrameRate.Visible)
            xmlwriter.SetValue("analog", "framerate", comboBoxFrameRate.Text);
          if (comboBoxFrameSize.Visible)
            xmlwriter.SetValue("analog", "framesize", comboBoxFrameSize.Text);
          if (checkBox5vAntennae.Visible)
            xmlwriter.SetValueAsBool("general", "Antennae5v", checkBox5vAntennae.Checked);
        }
        if (capture.CardType == TVCapture.CardTypes.Digital_SS2)/* ||
            capture.CardType == TVCapture.CardTypes.Digital_TTPremium)*/
        {
          // save settings for get the filename in mp.xml
          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            xmlwriter.SetValue("dvb_ts_cards", "filename", filename);
          }
        }
      }

      Sections.Wizard_ScanBase currentDialog = null;
      IEnumerator enumerator = this.tabPageAutotune.Controls.GetEnumerator();
      if (enumerator.MoveNext())
      {
        currentDialog = (Sections.Wizard_ScanBase)enumerator.Current;
        currentDialog.OnSectionActivated();
      }
    }
    private void tabPageAutotuneRadio_Enter(object sender, System.EventArgs e)
    {
      TVCaptureDevice capture = CaptureCard;
      // save settings for card
      if (capture != null)
      {
        string filename = String.Format(Config.GetFile(Config.Dir.Database, "card_{0}.xml"), capture.FriendlyName);
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(filename))
        {
          if (checkBoxHWPidFiltering.Visible)
            xmlwriter.SetValueAsBool("general", "hwfiltering", checkBoxHWPidFiltering.Checked);
          if (checkBox5vAntennae.Visible)
            xmlwriter.SetValueAsBool("general", "Antennae5v", checkBox5vAntennae.Checked);
          if (comboBoxCAM.Visible)
            xmlwriter.SetValue("dvbs", "cam", comboBoxCAM.SelectedItem);
          if (comboBoxFrameRate.Visible)
            xmlwriter.SetValue("analog", "framerate", comboBoxFrameRate.Text);
          if (comboBoxFrameSize.Visible)
            xmlwriter.SetValue("analog", "framesize", comboBoxFrameSize.Text);
        }
        if (capture.CardType == TVCapture.CardTypes.Digital_SS2)
        {
          // save settings for get the filename in mp.xml
          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            xmlwriter.SetValue("dvb_ts_cards", "filename", filename);
          }
        }
      }
      else
      {
        tabPageAutotuneRadio.Controls.Clear();
        return;
      }
      Sections.Wizard_ScanBase currentDialog = null;
      IEnumerator enumerator = this.tabPageAutotuneRadio.Controls.GetEnumerator();
      if (enumerator.MoveNext())
      {
        currentDialog = (Sections.Wizard_ScanBase)enumerator.Current;
        currentDialog.OnSectionActivated();
      }
    }

    void dlg_OnScanFinished(object sender, EventArgs args)
    {
      okButton.Enabled = true;
      cancelButton.Enabled = true;
      TVDatabase.ClearCache();
      MediaPortal.Configuration.Sections.TVChannels.UpdateList();
      MediaPortal.Configuration.Sections.RadioStations.UpdateList();
    }
    void dlg_OnScanStarted(object sender, EventArgs args)
    {
      okButton.Enabled = false;
      cancelButton.Enabled = false;
    }


    private void buttonBrowse_Click_1(object sender, System.EventArgs e)
    {

      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where recordings should be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = tbRecordingFolder.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          tbRecordingFolder.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void checkBox5vAntennae_CheckedChanged(object sender, EventArgs e)
    {
    
    }

    private void tbPortMin_TextChanged(object sender, EventArgs e)
    {
      if (Convert.ToInt32(tbPortMin.Text) < 500)
        tbPortMin.Text = "500";
    }

    private void tbPortMax_TextChanged(object sender, EventArgs e)
    {
      if (Convert.ToInt32(tbPortMax.Text) < 500)
        tbPortMax.Text = "500";
    }
  }
  public class CaptureFormat
  {
    public int Width;
    public int Height;
    public string Description;

    public override string ToString()
    {
      return Description;
    }
  }
}
public class ComboBoxCaptureCard
{
  private TVCaptureDevice _mCaptureDevice;
  private int m_iNumber = 1;
  private int m_iMax = 1;

  public ComboBoxCaptureCard(TVCaptureDevice pCaptureDevice)
  {
    this._mCaptureDevice = pCaptureDevice;
  }

  public TVCaptureDevice CaptureDevice
  {
    get { return _mCaptureDevice; }
  }

  public string VideoDeviceMoniker
  {
    get { return _mCaptureDevice.VideoDeviceMoniker; }
  }


  public int Number
  {
    get { return m_iNumber; }
    set { m_iNumber = value; }
  }
  public int MaxCards
  {
    get { return m_iMax; }
    set { m_iMax = value; }
  }

  // Display a more readable name by adding the commercial name of the card to the capture device
  public override string ToString()
  {
    return String.Format("{1} ({2})", m_iNumber, _mCaptureDevice.VideoDevice, _mCaptureDevice.CommercialName);
  }

  public override bool Equals(object obj)
  {
    //		return this.ToString().Equals((obj as ComboBoxCaptureCard).ToString());
    return VideoDeviceMoniker.Equals((obj as ComboBoxCaptureCard).VideoDeviceMoniker);
  }

  public override int GetHashCode()
  {
    return base.GetHashCode();
  }
}
