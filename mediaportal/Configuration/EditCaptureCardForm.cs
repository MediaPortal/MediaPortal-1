/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using MediaPortal.GUI.Library;
using TVCapture;
using MediaPortal.TV.Database;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for EditCaptureCardForm.
  /// </summary>
  public class EditCaptureCardForm : System.Windows.Forms.Form
  {
    static CaptureCardDefinitions mCaptureCardDefinitions = CaptureCardDefinitions.Instance;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cardComboBox;
    private System.Windows.Forms.CheckBox useRecordingCheckBox;
    private System.Windows.Forms.CheckBox useWatchingCheckBox;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    //
    // Private members
    //
    ArrayList captureFormats = new ArrayList();
    ArrayList propertyPages = new ArrayList();
    private Size m_size = new Size(0, 0);
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox textBoxName;

    bool acceptuserinput = false;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage3;
    private System.Windows.Forms.TabPage tabPage4;
    private System.Windows.Forms.TabPage tabPage5;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.ComboBox comboBox3Audio;
    private System.Windows.Forms.ComboBox comboBox3Video;
    private System.Windows.Forms.ComboBox comboBox2Audio;
    private System.Windows.Forms.ComboBox comboBox2Video;
    private System.Windows.Forms.ComboBox comboBox1Audio;
    private System.Windows.Forms.ComboBox comboBox1Video;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.ComboBox lnbkind1;
    private System.Windows.Forms.ComboBox lnbconfig1;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.TextBox circularMHZ;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.TextBox cbandMHZ;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.TextBox lnb1MHZ;
    private System.Windows.Forms.Label lnb1;
    private System.Windows.Forms.TextBox lnbswMHZ;
    private System.Windows.Forms.Label switchMHZ;
    private System.Windows.Forms.TextBox lnb0MHZ;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.CheckBox checkBoxHiQuality;
    private System.Windows.Forms.NumericUpDown updownPrio;
    private System.Windows.Forms.Label label24;
    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.ComboBox comboBoxQuality;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ComboBox lnbkind4;
    private System.Windows.Forms.ComboBox lnbkind3;
    private System.Windows.Forms.ComboBox lnbkind2;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.Label label31;
    private System.Windows.Forms.Label label32;
    private System.Windows.Forms.ComboBox lnbconfig4;
    private System.Windows.Forms.ComboBox lnbconfig3;
    private System.Windows.Forms.ComboBox lnbconfig2;
    private System.Windows.Forms.ComboBox diseqcd;
    private System.Windows.Forms.ComboBox diseqcc;
    private System.Windows.Forms.ComboBox diseqcb;
    private System.Windows.Forms.ComboBox diseqca;
    private System.Windows.Forms.CheckBox useLNB1;
    private System.Windows.Forms.CheckBox useLNB2;
    private System.Windows.Forms.CheckBox useLNB3;
    private System.Windows.Forms.CheckBox useLNB4;
    private System.Windows.Forms.TabPage tabPage7;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.TextBox tbRecordingFolder;
    private System.Windows.Forms.Button btnRadio;
    private System.Windows.Forms.Label label35;
    private System.Windows.Forms.ComboBox cbRgbVideo;
    private System.Windows.Forms.ComboBox cbRgbAudio;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.Label label26;
    private System.Windows.Forms.GroupBox groupBox5;
    private System.Windows.Forms.GroupBox groupBox7;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.TextBox tbPortMin;
    private System.Windows.Forms.TextBox tbPortMax;
    private System.Windows.Forms.TextBox tbLowMax;
    private System.Windows.Forms.TextBox tbLowMin;
    private System.Windows.Forms.Label label34;
    private System.Windows.Forms.TextBox tbMedMax;
    private System.Windows.Forms.TextBox tbMedMin;
    private System.Windows.Forms.Label label36;
    private System.Windows.Forms.TextBox tbHighMax;
    private System.Windows.Forms.TextBox tbHighMin;
    private System.Windows.Forms.Label label37;
    private System.Windows.Forms.CheckBox cbLowVBR;
    private System.Windows.Forms.CheckBox cbMedVBR;
    private System.Windows.Forms.CheckBox cbHighVBR;
    private System.Windows.Forms.CheckBox cbPortVBR;
    int CardId;

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
          using (FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {

            SoapFormatter formatter = new SoapFormatter();
            captureCards = (ArrayList)formatter.Deserialize(fileStream);
            for (int i = 0; i < captureCards.Count; i++)
            {
              ((TVCaptureDevice)captureCards[i]).ID = (i + 1);
              ((TVCaptureDevice)captureCards[i]).LoadDefinitions();
            }
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
        }
        catch
        {
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

      /* below is used for testing only */
      /*
      availableVideoDevices.Add("Hauppauge WinTV PVR PCI II Capture");
      availableVideoDeviceMonikers.Add(@"@device:pnp:\\?\pci#ven_4444&dev_0803&subsys_40000070&rev_01#3&267a616a&0&48#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capture");
    

      //test for SS2
      availableVideoDevices.Add("B2C2 MPEG-2 Source");
      availableVideoDeviceMonikers.Add("B2C2 MPEG-2 Source");
      */

      /* below is used for testing only */

      FilterHelper.GetMPEG2VideoEncoders(availableVideoCompressors);
      FilterHelper.GetMPEG2AudioEncoders(availableAudioCompressors);
      for (int i = 0; i < availableVideoDevices.Count; ++i)
      {
        Log.Write("device:{0} id:{1}", availableVideoDevices[i].ToString(), availableVideoDeviceMonikers[i].ToString());
      }


      if (availableVideoDevices.Count == 0)
      {
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


      for (int i = 0; i < availableVideoDevices.Count; i++)
      {
        Log.Write("Found capture #{0} card:{1} moniker:{2}",
          i,
          ((string)(availableVideoDevices[i])),
          ((string)(availableVideoDeviceMonikers[i])));
      }
      bool ss2Added = false;
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
              cd.FriendlyName = String.Format("card{0}", captureCards.Count + 1);
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
            break;
          }

          bool add = false;
          if (ccd.CaptureName != String.Empty)
          {
            if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
              ((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1)) add = true;
          }
          if (add)
          {
            TVCaptureDevice cd = new TVCaptureDevice();
            cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
            if (ccd.CaptureName != String.Empty)
            {
              cd.VideoDevice = ccd.CaptureName;////Hauppauge WinTV PVR PCI II Capture
              cd.CommercialName = ccd.CommercialName;//PVR 150MCE
              cd.LoadDefinitions();
              cd.CardType = ccd.Capabilities.CardType;
              cd.DeviceId = ccd.DeviceId;
            }

            Log.Write("Adding name:{0} capture:{1} id:{2} type:{3}",
              cd.CommercialName,
              cd.DeviceId,
              cd.CardType.ToString());
            ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
            int nr = 1;
            foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
            {
              if (cb.CaptureDevice.CommercialName == cbcc.CaptureDevice.CommercialName) nr++;
            }
            cbcc.Number = nr;
            cbcc.MaxCards = nr;
            foreach (ComboBoxCaptureCard cb in cardComboBox.Items)
            {
              if (cb.CaptureDevice.CommercialName == cbcc.CaptureDevice.CommercialName)
              {
                cb.MaxCards = nr;
              }
            }
            if (!addNewCard)
            {
              if (deviceToEdit.DeviceId != null)
              {
                if (cbcc.CaptureDevice.DeviceId == deviceToEdit.DeviceId)
                {
                  if (deviceToEdit.VideoDeviceMoniker == cbcc.VideoDeviceMoniker &&
                    deviceToEdit.VideoDevice == cbcc.CaptureDevice.VideoDevice)
                  {
                    cardComboBox.Items.Add(cbcc);
                    if (deviceToEdit != null && deviceToEdit.CommercialName == cbcc.CaptureDevice.CommercialName)
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
                  if (cbcc.CaptureDevice.DeviceId == dev.DeviceId)
                  {
                    if (dev.VideoDeviceMoniker == cbcc.VideoDeviceMoniker &&
                      dev.VideoDevice == cbcc.CaptureDevice.VideoDevice)
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCaptureCardForm));
      this.checkBoxHiQuality = new System.Windows.Forms.CheckBox();
      this.useRecordingCheckBox = new System.Windows.Forms.CheckBox();
      this.useWatchingCheckBox = new System.Windows.Forms.CheckBox();
      this.cardComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxName = new System.Windows.Forms.TextBox();
      this.label12 = new System.Windows.Forms.Label();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.label24 = new System.Windows.Forms.Label();
      this.updownPrio = new System.Windows.Forms.NumericUpDown();
      this.tabPage7 = new System.Windows.Forms.TabPage();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.tbRecordingFolder = new System.Windows.Forms.TextBox();
      this.label26 = new System.Windows.Forms.Label();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.cbRgbAudio = new System.Windows.Forms.ComboBox();
      this.cbRgbVideo = new System.Windows.Forms.ComboBox();
      this.label35 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.comboBox3Audio = new System.Windows.Forms.ComboBox();
      this.comboBox3Video = new System.Windows.Forms.ComboBox();
      this.comboBox2Audio = new System.Windows.Forms.ComboBox();
      this.comboBox2Video = new System.Windows.Forms.ComboBox();
      this.comboBox1Audio = new System.Windows.Forms.ComboBox();
      this.comboBox1Video = new System.Windows.Forms.ComboBox();
      this.label15 = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.label17 = new System.Windows.Forms.Label();
      this.tabPage5 = new System.Windows.Forms.TabPage();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.cbHighVBR = new System.Windows.Forms.CheckBox();
      this.tbHighMax = new System.Windows.Forms.TextBox();
      this.tbHighMin = new System.Windows.Forms.TextBox();
      this.label37 = new System.Windows.Forms.Label();
      this.cbMedVBR = new System.Windows.Forms.CheckBox();
      this.tbMedMax = new System.Windows.Forms.TextBox();
      this.tbMedMin = new System.Windows.Forms.TextBox();
      this.label36 = new System.Windows.Forms.Label();
      this.cbLowVBR = new System.Windows.Forms.CheckBox();
      this.tbLowMax = new System.Windows.Forms.TextBox();
      this.tbLowMin = new System.Windows.Forms.TextBox();
      this.label34 = new System.Windows.Forms.Label();
      this.label29 = new System.Windows.Forms.Label();
      this.label28 = new System.Windows.Forms.Label();
      this.label27 = new System.Windows.Forms.Label();
      this.cbPortVBR = new System.Windows.Forms.CheckBox();
      this.tbPortMax = new System.Windows.Forms.TextBox();
      this.tbPortMin = new System.Windows.Forms.TextBox();
      this.label19 = new System.Windows.Forms.Label();
      this.label18 = new System.Windows.Forms.Label();
      this.label25 = new System.Windows.Forms.Label();
      this.comboBoxQuality = new System.Windows.Forms.ComboBox();
      this.tabPage4 = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.useLNB4 = new System.Windows.Forms.CheckBox();
      this.useLNB3 = new System.Windows.Forms.CheckBox();
      this.useLNB2 = new System.Windows.Forms.CheckBox();
      this.useLNB1 = new System.Windows.Forms.CheckBox();
      this.lnbkind4 = new System.Windows.Forms.ComboBox();
      this.lnbkind3 = new System.Windows.Forms.ComboBox();
      this.lnbkind2 = new System.Windows.Forms.ComboBox();
      this.label30 = new System.Windows.Forms.Label();
      this.label31 = new System.Windows.Forms.Label();
      this.label32 = new System.Windows.Forms.Label();
      this.lnbconfig4 = new System.Windows.Forms.ComboBox();
      this.lnbconfig3 = new System.Windows.Forms.ComboBox();
      this.lnbconfig2 = new System.Windows.Forms.ComboBox();
      this.diseqcd = new System.Windows.Forms.ComboBox();
      this.diseqcc = new System.Windows.Forms.ComboBox();
      this.diseqcb = new System.Windows.Forms.ComboBox();
      this.diseqca = new System.Windows.Forms.ComboBox();
      this.lnbkind1 = new System.Windows.Forms.ComboBox();
      this.lnbconfig1 = new System.Windows.Forms.ComboBox();
      this.label23 = new System.Windows.Forms.Label();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.circularMHZ = new System.Windows.Forms.TextBox();
      this.label20 = new System.Windows.Forms.Label();
      this.cbandMHZ = new System.Windows.Forms.TextBox();
      this.label21 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.lnb1MHZ = new System.Windows.Forms.TextBox();
      this.lnb1 = new System.Windows.Forms.Label();
      this.lnbswMHZ = new System.Windows.Forms.TextBox();
      this.switchMHZ = new System.Windows.Forms.Label();
      this.lnb0MHZ = new System.Windows.Forms.TextBox();
      this.label22 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.btnRadio = new System.Windows.Forms.Button();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.updownPrio)).BeginInit();
      this.tabPage7.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.tabPage5.SuspendLayout();
      this.groupBox7.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // checkBoxHiQuality
      // 
      this.checkBoxHiQuality.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxHiQuality.Location = new System.Drawing.Point(24, 24);
      this.checkBoxHiQuality.Name = "checkBoxHiQuality";
      this.checkBoxHiQuality.Size = new System.Drawing.Size(368, 16);
      this.checkBoxHiQuality.TabIndex = 7;
      this.checkBoxHiQuality.Text = "Use Quality control";
      // 
      // useRecordingCheckBox
      // 
      this.useRecordingCheckBox.Checked = true;
      this.useRecordingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useRecordingCheckBox.Enabled = false;
      this.useRecordingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useRecordingCheckBox.Location = new System.Drawing.Point(11, 71);
      this.useRecordingCheckBox.Name = "useRecordingCheckBox";
      this.useRecordingCheckBox.Size = new System.Drawing.Size(168, 16);
      this.useRecordingCheckBox.TabIndex = 9;
      this.useRecordingCheckBox.Text = "Use this card for recording TV";
      // 
      // useWatchingCheckBox
      // 
      this.useWatchingCheckBox.Checked = true;
      this.useWatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useWatchingCheckBox.Enabled = false;
      this.useWatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useWatchingCheckBox.Location = new System.Drawing.Point(11, 47);
      this.useWatchingCheckBox.Name = "useWatchingCheckBox";
      this.useWatchingCheckBox.Size = new System.Drawing.Size(168, 24);
      this.useWatchingCheckBox.TabIndex = 8;
      this.useWatchingCheckBox.Text = "Use this card for viewing TV";
      // 
      // cardComboBox
      // 
      this.cardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cardComboBox.Location = new System.Drawing.Point(112, 8);
      this.cardComboBox.Name = "cardComboBox";
      this.cardComboBox.Size = new System.Drawing.Size(400, 21);
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
      // textBoxName
      // 
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
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(461, 450);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 4;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(381, 450);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage7);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage5);
      this.tabControl1.Controls.Add(this.tabPage4);
      this.tabControl1.Location = new System.Drawing.Point(8, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(536, 432);
      this.tabControl1.TabIndex = 5;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.label24);
      this.tabPage1.Controls.Add(this.updownPrio);
      this.tabPage1.Controls.Add(this.cardComboBox);
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Controls.Add(this.useRecordingCheckBox);
      this.tabPage1.Controls.Add(this.useWatchingCheckBox);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(528, 406);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Capture card";
      // 
      // label24
      // 
      this.label24.Location = new System.Drawing.Point(8, 93);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(136, 16);
      this.label24.TabIndex = 52;
      this.label24.Text = "Priority (1=low,10=high)";
      // 
      // updownPrio
      // 
      this.updownPrio.Location = new System.Drawing.Point(152, 93);
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
      // tabPage7
      // 
      this.tabPage7.Controls.Add(this.groupBox5);
      this.tabPage7.Location = new System.Drawing.Point(4, 22);
      this.tabPage7.Name = "tabPage7";
      this.tabPage7.Size = new System.Drawing.Size(528, 406);
      this.tabPage7.TabIndex = 6;
      this.tabPage7.Text = "Recordings";
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.buttonBrowse);
      this.groupBox5.Controls.Add(this.tbRecordingFolder);
      this.groupBox5.Controls.Add(this.label26);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox5.Location = new System.Drawing.Point(8, 8);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(424, 100);
      this.groupBox5.TabIndex = 61;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Recording folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonBrowse.Location = new System.Drawing.Point(280, 56);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(56, 23);
      this.buttonBrowse.TabIndex = 58;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click_1);
      // 
      // tbRecordingFolder
      // 
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
      this.tabPage3.Size = new System.Drawing.Size(528, 406);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Audio mapping";
      // 
      // cbRgbAudio
      // 
      this.cbRgbAudio.Location = new System.Drawing.Point(72, 264);
      this.cbRgbAudio.Name = "cbRgbAudio";
      this.cbRgbAudio.Size = new System.Drawing.Size(121, 21);
      this.cbRgbAudio.TabIndex = 21;
      // 
      // cbRgbVideo
      // 
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
      this.comboBox3Audio.Location = new System.Drawing.Point(72, 208);
      this.comboBox3Audio.Name = "comboBox3Audio";
      this.comboBox3Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox3Audio.TabIndex = 16;
      // 
      // comboBox3Video
      // 
      this.comboBox3Video.Location = new System.Drawing.Point(72, 184);
      this.comboBox3Video.Name = "comboBox3Video";
      this.comboBox3Video.Size = new System.Drawing.Size(121, 21);
      this.comboBox3Video.TabIndex = 15;
      // 
      // comboBox2Audio
      // 
      this.comboBox2Audio.Location = new System.Drawing.Point(72, 144);
      this.comboBox2Audio.Name = "comboBox2Audio";
      this.comboBox2Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox2Audio.TabIndex = 14;
      // 
      // comboBox2Video
      // 
      this.comboBox2Video.Location = new System.Drawing.Point(72, 120);
      this.comboBox2Video.Name = "comboBox2Video";
      this.comboBox2Video.Size = new System.Drawing.Size(121, 21);
      this.comboBox2Video.TabIndex = 13;
      // 
      // comboBox1Audio
      // 
      this.comboBox1Audio.Location = new System.Drawing.Point(72, 88);
      this.comboBox1Audio.Name = "comboBox1Audio";
      this.comboBox1Audio.Size = new System.Drawing.Size(121, 21);
      this.comboBox1Audio.TabIndex = 10;
      // 
      // comboBox1Video
      // 
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
      this.tabPage5.Controls.Add(this.groupBox7);
      this.tabPage5.Controls.Add(this.label18);
      this.tabPage5.Controls.Add(this.label25);
      this.tabPage5.Controls.Add(this.comboBoxQuality);
      this.tabPage5.Controls.Add(this.checkBoxHiQuality);
      this.tabPage5.Location = new System.Drawing.Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Size = new System.Drawing.Size(528, 406);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "Quality";
      this.tabPage5.Click += new System.EventHandler(this.tabPage5_Click);
      // 
      // groupBox7
      // 
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
      this.groupBox7.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox7.Location = new System.Drawing.Point(24, 144);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(456, 224);
      this.groupBox7.TabIndex = 11;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "Quality settings:";
      // 
      // cbHighVBR
      // 
      this.cbHighVBR.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbHighVBR.Location = new System.Drawing.Point(248, 152);
      this.cbHighVBR.Name = "cbHighVBR";
      this.cbHighVBR.Size = new System.Drawing.Size(32, 24);
      this.cbHighVBR.TabIndex = 18;
      // 
      // tbHighMax
      // 
      this.tbHighMax.Location = new System.Drawing.Point(168, 152);
      this.tbHighMax.Name = "tbHighMax";
      this.tbHighMax.Size = new System.Drawing.Size(64, 20);
      this.tbHighMax.TabIndex = 17;
      this.tbHighMax.Text = "300";
      // 
      // tbHighMin
      // 
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
      this.cbMedVBR.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbMedVBR.Location = new System.Drawing.Point(248, 120);
      this.cbMedVBR.Name = "cbMedVBR";
      this.cbMedVBR.Size = new System.Drawing.Size(32, 24);
      this.cbMedVBR.TabIndex = 14;
      // 
      // tbMedMax
      // 
      this.tbMedMax.Location = new System.Drawing.Point(168, 120);
      this.tbMedMax.Name = "tbMedMax";
      this.tbMedMax.Size = new System.Drawing.Size(64, 20);
      this.tbMedMax.TabIndex = 13;
      this.tbMedMax.Text = "300";
      // 
      // tbMedMin
      // 
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
      this.cbLowVBR.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbLowVBR.Location = new System.Drawing.Point(248, 88);
      this.cbLowVBR.Name = "cbLowVBR";
      this.cbLowVBR.Size = new System.Drawing.Size(32, 24);
      this.cbLowVBR.TabIndex = 10;
      // 
      // tbLowMax
      // 
      this.tbLowMax.Location = new System.Drawing.Point(168, 88);
      this.tbLowMax.Name = "tbLowMax";
      this.tbLowMax.Size = new System.Drawing.Size(64, 20);
      this.tbLowMax.TabIndex = 9;
      this.tbLowMax.Text = "300";
      // 
      // tbLowMin
      // 
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
      this.cbPortVBR.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbPortVBR.Location = new System.Drawing.Point(248, 56);
      this.cbPortVBR.Name = "cbPortVBR";
      this.cbPortVBR.Size = new System.Drawing.Size(32, 24);
      this.cbPortVBR.TabIndex = 3;
      // 
      // tbPortMax
      // 
      this.tbPortMax.Location = new System.Drawing.Point(168, 56);
      this.tbPortMax.Name = "tbPortMax";
      this.tbPortMax.Size = new System.Drawing.Size(64, 20);
      this.tbPortMax.TabIndex = 2;
      this.tbPortMax.Text = "300";
      // 
      // tbPortMin
      // 
      this.tbPortMin.Location = new System.Drawing.Point(80, 56);
      this.tbPortMin.Name = "tbPortMin";
      this.tbPortMin.Size = new System.Drawing.Size(64, 20);
      this.tbPortMin.TabIndex = 1;
      this.tbPortMin.Text = "100";
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
      this.label18.Location = new System.Drawing.Point(24, 80);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(408, 72);
      this.label18.TabIndex = 10;
      this.label18.Text = resources.GetString("label18.Text");
      // 
      // label25
      // 
      this.label25.Location = new System.Drawing.Point(40, 48);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(80, 16);
      this.label25.TabIndex = 9;
      this.label25.Text = "Default quality:";
      // 
      // comboBoxQuality
      // 
      this.comboBoxQuality.Items.AddRange(new object[] {
            "Portable",
            "Low",
            "Medium",
            "High"});
      this.comboBoxQuality.Location = new System.Drawing.Point(128, 48);
      this.comboBoxQuality.Name = "comboBoxQuality";
      this.comboBoxQuality.Size = new System.Drawing.Size(240, 21);
      this.comboBoxQuality.TabIndex = 8;
      // 
      // tabPage4
      // 
      this.tabPage4.Controls.Add(this.groupBox2);
      this.tabPage4.Controls.Add(this.label23);
      this.tabPage4.Controls.Add(this.groupBox4);
      this.tabPage4.Controls.Add(this.groupBox3);
      this.tabPage4.Location = new System.Drawing.Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new System.Drawing.Size(528, 406);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "DVB-S LNB";
      this.tabPage4.Click += new System.EventHandler(this.tabPage4_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.useLNB4);
      this.groupBox2.Controls.Add(this.useLNB3);
      this.groupBox2.Controls.Add(this.useLNB2);
      this.groupBox2.Controls.Add(this.useLNB1);
      this.groupBox2.Controls.Add(this.lnbkind4);
      this.groupBox2.Controls.Add(this.lnbkind3);
      this.groupBox2.Controls.Add(this.lnbkind2);
      this.groupBox2.Controls.Add(this.label30);
      this.groupBox2.Controls.Add(this.label31);
      this.groupBox2.Controls.Add(this.label32);
      this.groupBox2.Controls.Add(this.lnbconfig4);
      this.groupBox2.Controls.Add(this.lnbconfig3);
      this.groupBox2.Controls.Add(this.lnbconfig2);
      this.groupBox2.Controls.Add(this.diseqcd);
      this.groupBox2.Controls.Add(this.diseqcc);
      this.groupBox2.Controls.Add(this.diseqcb);
      this.groupBox2.Controls.Add(this.diseqca);
      this.groupBox2.Controls.Add(this.lnbkind1);
      this.groupBox2.Controls.Add(this.lnbconfig1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(24, 192);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(408, 144);
      this.groupBox2.TabIndex = 32;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "DiSeqC (Skystar2) / LNB-Config";
      this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
      // 
      // useLNB4
      // 
      this.useLNB4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useLNB4.Location = new System.Drawing.Point(312, 112);
      this.useLNB4.Name = "useLNB4";
      this.useLNB4.Size = new System.Drawing.Size(80, 16);
      this.useLNB4.TabIndex = 31;
      this.useLNB4.Text = "In Use";
      // 
      // useLNB3
      // 
      this.useLNB3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useLNB3.Location = new System.Drawing.Point(312, 88);
      this.useLNB3.Name = "useLNB3";
      this.useLNB3.Size = new System.Drawing.Size(80, 16);
      this.useLNB3.TabIndex = 30;
      this.useLNB3.Text = "In Use";
      // 
      // useLNB2
      // 
      this.useLNB2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useLNB2.Location = new System.Drawing.Point(312, 64);
      this.useLNB2.Name = "useLNB2";
      this.useLNB2.Size = new System.Drawing.Size(80, 16);
      this.useLNB2.TabIndex = 29;
      this.useLNB2.Text = "In Use";
      // 
      // useLNB1
      // 
      this.useLNB1.Checked = true;
      this.useLNB1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useLNB1.Enabled = false;
      this.useLNB1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useLNB1.Location = new System.Drawing.Point(312, 40);
      this.useLNB1.Name = "useLNB1";
      this.useLNB1.Size = new System.Drawing.Size(80, 16);
      this.useLNB1.TabIndex = 28;
      this.useLNB1.Text = "In Use";
      // 
      // lnbkind4
      // 
      this.lnbkind4.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind4.Location = new System.Drawing.Point(232, 109);
      this.lnbkind4.Name = "lnbkind4";
      this.lnbkind4.Size = new System.Drawing.Size(72, 21);
      this.lnbkind4.TabIndex = 26;
      // 
      // lnbkind3
      // 
      this.lnbkind3.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind3.Location = new System.Drawing.Point(232, 85);
      this.lnbkind3.Name = "lnbkind3";
      this.lnbkind3.Size = new System.Drawing.Size(72, 21);
      this.lnbkind3.TabIndex = 25;
      // 
      // lnbkind2
      // 
      this.lnbkind2.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind2.Location = new System.Drawing.Point(232, 61);
      this.lnbkind2.Name = "lnbkind2";
      this.lnbkind2.Size = new System.Drawing.Size(72, 21);
      this.lnbkind2.TabIndex = 24;
      // 
      // label30
      // 
      this.label30.Location = new System.Drawing.Point(232, 21);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(56, 16);
      this.label30.TabIndex = 22;
      this.label30.Text = "LNB:";
      // 
      // label31
      // 
      this.label31.Location = new System.Drawing.Point(136, 21);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(64, 16);
      this.label31.TabIndex = 21;
      this.label31.Text = "LNBSelect:";
      // 
      // label32
      // 
      this.label32.Location = new System.Drawing.Point(16, 21);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(80, 16);
      this.label32.TabIndex = 20;
      this.label32.Text = "DiSEqC:";
      // 
      // lnbconfig4
      // 
      this.lnbconfig4.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig4.Location = new System.Drawing.Point(136, 109);
      this.lnbconfig4.Name = "lnbconfig4";
      this.lnbconfig4.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig4.TabIndex = 19;
      // 
      // lnbconfig3
      // 
      this.lnbconfig3.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig3.Location = new System.Drawing.Point(136, 85);
      this.lnbconfig3.Name = "lnbconfig3";
      this.lnbconfig3.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig3.TabIndex = 18;
      // 
      // lnbconfig2
      // 
      this.lnbconfig2.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig2.Location = new System.Drawing.Point(136, 61);
      this.lnbconfig2.Name = "lnbconfig2";
      this.lnbconfig2.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig2.TabIndex = 17;
      // 
      // diseqcd
      // 
      this.diseqcd.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcd.Location = new System.Drawing.Point(16, 109);
      this.diseqcd.Name = "diseqcd";
      this.diseqcd.Size = new System.Drawing.Size(104, 21);
      this.diseqcd.TabIndex = 14;
      this.diseqcd.Text = "None";
      this.diseqcd.SelectedIndexChanged += new System.EventHandler(this.diseqcd_SelectedIndexChanged);
      // 
      // diseqcc
      // 
      this.diseqcc.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcc.Location = new System.Drawing.Point(16, 85);
      this.diseqcc.Name = "diseqcc";
      this.diseqcc.Size = new System.Drawing.Size(104, 21);
      this.diseqcc.TabIndex = 13;
      this.diseqcc.Text = "None";
      this.diseqcc.SelectedIndexChanged += new System.EventHandler(this.diseqcc_SelectedIndexChanged);
      // 
      // diseqcb
      // 
      this.diseqcb.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqcb.Location = new System.Drawing.Point(16, 61);
      this.diseqcb.Name = "diseqcb";
      this.diseqcb.Size = new System.Drawing.Size(104, 21);
      this.diseqcb.TabIndex = 12;
      this.diseqcb.Text = "None";
      this.diseqcb.SelectedIndexChanged += new System.EventHandler(this.diseqcb_SelectedIndexChanged);
      // 
      // diseqca
      // 
      this.diseqca.Items.AddRange(new object[] {
            "None",
            "Simple A",
            "Simple B",
            "Level 1 A/A",
            "Level 1 B/A",
            "Level 1 A/B",
            "Level 1 B/B"});
      this.diseqca.Location = new System.Drawing.Point(16, 37);
      this.diseqca.Name = "diseqca";
      this.diseqca.Size = new System.Drawing.Size(104, 21);
      this.diseqca.TabIndex = 1;
      this.diseqca.Text = "None";
      this.diseqca.SelectedIndexChanged += new System.EventHandler(this.diseqca_SelectedIndexChanged);
      // 
      // lnbkind1
      // 
      this.lnbkind1.Items.AddRange(new object[] {
            "Ku-Band",
            "C-Band",
            "Circular"});
      this.lnbkind1.Location = new System.Drawing.Point(232, 37);
      this.lnbkind1.Name = "lnbkind1";
      this.lnbkind1.Size = new System.Drawing.Size(72, 21);
      this.lnbkind1.TabIndex = 27;
      // 
      // lnbconfig1
      // 
      this.lnbconfig1.Items.AddRange(new object[] {
            "0 KHz",
            "22 KHz",
            "33 Khz",
            "44 KHz"});
      this.lnbconfig1.Location = new System.Drawing.Point(136, 37);
      this.lnbconfig1.Name = "lnbconfig1";
      this.lnbconfig1.Size = new System.Drawing.Size(80, 21);
      this.lnbconfig1.TabIndex = 24;
      // 
      // label23
      // 
      this.label23.Location = new System.Drawing.Point(24, 16);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(352, 32);
      this.label23.TabIndex = 31;
      this.label23.Text = "Specify your LNB settings. Please note this is only necessary for DVB-S (sattelit" +
          "e) tv capture cards!";
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.circularMHZ);
      this.groupBox4.Controls.Add(this.label20);
      this.groupBox4.Controls.Add(this.cbandMHZ);
      this.groupBox4.Controls.Add(this.label21);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox4.Location = new System.Drawing.Point(256, 72);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(176, 96);
      this.groupBox4.TabIndex = 29;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "C-Band / Circular Config:";
      // 
      // circularMHZ
      // 
      this.circularMHZ.Location = new System.Drawing.Point(96, 45);
      this.circularMHZ.Name = "circularMHZ";
      this.circularMHZ.Size = new System.Drawing.Size(64, 20);
      this.circularMHZ.TabIndex = 3;
      this.circularMHZ.Text = "10750";
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(16, 48);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(80, 16);
      this.label20.TabIndex = 2;
      this.label20.Text = "Circular (MHz)";
      // 
      // cbandMHZ
      // 
      this.cbandMHZ.Location = new System.Drawing.Point(96, 21);
      this.cbandMHZ.Name = "cbandMHZ";
      this.cbandMHZ.Size = new System.Drawing.Size(64, 20);
      this.cbandMHZ.TabIndex = 1;
      this.cbandMHZ.Text = "5150";
      // 
      // label21
      // 
      this.label21.Location = new System.Drawing.Point(16, 24);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(80, 16);
      this.label21.TabIndex = 0;
      this.label21.Text = "C-Band (MHz)";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.lnb1MHZ);
      this.groupBox3.Controls.Add(this.lnb1);
      this.groupBox3.Controls.Add(this.lnbswMHZ);
      this.groupBox3.Controls.Add(this.switchMHZ);
      this.groupBox3.Controls.Add(this.lnb0MHZ);
      this.groupBox3.Controls.Add(this.label22);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox3.Location = new System.Drawing.Point(24, 72);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(176, 96);
      this.groupBox3.TabIndex = 28;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Ku-Band Config:";
      // 
      // lnb1MHZ
      // 
      this.lnb1MHZ.Location = new System.Drawing.Point(104, 69);
      this.lnb1MHZ.Name = "lnb1MHZ";
      this.lnb1MHZ.Size = new System.Drawing.Size(56, 20);
      this.lnb1MHZ.TabIndex = 7;
      this.lnb1MHZ.Text = "10600";
      // 
      // lnb1
      // 
      this.lnb1.Location = new System.Drawing.Point(16, 72);
      this.lnb1.Name = "lnb1";
      this.lnb1.Size = new System.Drawing.Size(72, 16);
      this.lnb1.TabIndex = 6;
      this.lnb1.Text = "LNB1 (Mhz)";
      // 
      // lnbswMHZ
      // 
      this.lnbswMHZ.Location = new System.Drawing.Point(104, 45);
      this.lnbswMHZ.Name = "lnbswMHZ";
      this.lnbswMHZ.Size = new System.Drawing.Size(56, 20);
      this.lnbswMHZ.TabIndex = 3;
      this.lnbswMHZ.Text = "11700";
      // 
      // switchMHZ
      // 
      this.switchMHZ.Location = new System.Drawing.Point(16, 48);
      this.switchMHZ.Name = "switchMHZ";
      this.switchMHZ.Size = new System.Drawing.Size(80, 16);
      this.switchMHZ.TabIndex = 2;
      this.switchMHZ.Text = "Switch (MHz)";
      // 
      // lnb0MHZ
      // 
      this.lnb0MHZ.Location = new System.Drawing.Point(104, 21);
      this.lnb0MHZ.Name = "lnb0MHZ";
      this.lnb0MHZ.Size = new System.Drawing.Size(56, 20);
      this.lnb0MHZ.TabIndex = 1;
      this.lnb0MHZ.Text = "9750";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(16, 24);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(72, 16);
      this.label22.TabIndex = 0;
      this.label22.Text = "LNB0 (Mhz)";
      // 
      // button1
      // 
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(192, 450);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 6;
      this.button1.Text = "Autotune";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // btnRadio
      // 
      this.btnRadio.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnRadio.Location = new System.Drawing.Point(80, 450);
      this.btnRadio.Name = "btnRadio";
      this.btnRadio.Size = new System.Drawing.Size(96, 23);
      this.btnRadio.TabIndex = 7;
      this.btnRadio.Text = "Autotune Radio";
      this.btnRadio.Click += new System.EventHandler(this.btnRadio_Click);
      // 
      // EditCaptureCardForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(554, 482);
      this.Controls.Add(this.btnRadio);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MinimumSize = new System.Drawing.Size(480, 296);
      this.Name = "EditCaptureCardForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit properties of your TV card";
      this.Load += new System.EventHandler(this.EditCaptureCardForm_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.updownPrio)).EndInit();
      this.tabPage7.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.tabPage3.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      this.groupBox7.PerformLayout();
      this.tabPage4.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    void FillInDefaultRecordingPath()
    {
      if (tbRecordingFolder.Text != String.Empty) return;

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
      if (tbRecordingFolder.Text == String.Empty)
      {
        tabControl1.SelectedTab = tabPage7;
        MessageBox.Show("No recording folder specified", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      if (CaptureCard != null && CaptureCard.FriendlyName != String.Empty)
      {
        string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);
        using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(filename))
        {
          xmlwriter.SetValueAsBool("quality", "enabled", checkBoxHiQuality.Checked);
          xmlwriter.SetValue("mapping", "audio1", comboBox1Audio.SelectedIndex);
          xmlwriter.SetValue("mapping", "audio2", comboBox2Audio.SelectedIndex);
          xmlwriter.SetValue("mapping", "audio3", comboBox3Audio.SelectedIndex);
          xmlwriter.SetValue("mapping", "audio4", cbRgbAudio.SelectedIndex);


          xmlwriter.SetValue("mapping", "video1", comboBox1Video.SelectedIndex);
          xmlwriter.SetValue("mapping", "video2", comboBox2Video.SelectedIndex);
          xmlwriter.SetValue("mapping", "video3", comboBox3Video.SelectedIndex);
          xmlwriter.SetValue("mapping", "video4", cbRgbVideo.SelectedIndex);


          xmlwriter.SetValue("quality", "portLow", tbPortMin.Text);
          xmlwriter.SetValue("quality", "portMax", tbPortMax.Text);
          xmlwriter.SetValueAsBool("quality", "portVBR", cbPortVBR.Checked);


          xmlwriter.SetValue("quality", "LowLow", tbLowMin.Text);
          xmlwriter.SetValue("quality", "LowMax", tbLowMax.Text);
          xmlwriter.SetValueAsBool("quality", "LowVBR", cbLowVBR.Checked);

          xmlwriter.SetValue("quality", "MedLow", tbMedMin.Text);
          xmlwriter.SetValue("quality", "MedMax", tbMedMax.Text);
          xmlwriter.SetValueAsBool("quality", "MedVBR", cbMedVBR.Checked);


          xmlwriter.SetValue("quality", "HighLow", tbHighMin.Text);
          xmlwriter.SetValue("quality", "HighMax", tbHighMax.Text);
          xmlwriter.SetValueAsBool("quality", "HighVBR", cbHighVBR.Checked);


        }
      }
      SaveDVBSSettings();

      this.DialogResult = DialogResult.OK;
      this.Hide();
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

      if (!FillInAll())
      {
        MessageBox.Show("Unable to create graph for this device!!", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
      bool result = true;
      try
      {
        btnRadio.Visible = false;
        button1.Visible = false;
        //
        // Setup frame sizes
        //
        TVCaptureDevice capture = CaptureCard;

        useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = cardComboBox.Text.Length > 0;

        if (capture != null)
        {
          if (capture.CreateGraph())
          {
            if (capture.Network != NetworkType.DVBC &&
              capture.Network != NetworkType.DVBS &&
              capture.Network != NetworkType.DVBT &&
              capture.Network != NetworkType.ATSC)
            {
              btnRadio.Visible = true;
            }
            button1.Visible = true;


            capture.DeleteGraph();
          }
          else result = false;
        }
      }
      //
      //

        //
      //
      catch (Exception ex)
      {
        Log.Write("FillInAll exception:{0} {1} {2}",
          ex.Message, ex.Source, ex.StackTrace);
      }
      return result;
    }


    private void EditCaptureCardForm_Load(object sender, System.EventArgs e)
    {
      bool result = FillInAll();

      if (CaptureCard != null)
      {
        if (CaptureCard.FriendlyName != String.Empty)
        {
          string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);
          using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename))
          {
            checkBoxHiQuality.Checked = xmlreader.GetValueAsBool("quality", "enabled", false);

            comboBox1Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio1", 0);
            comboBox2Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio2", 1);
            comboBox3Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio3", 0);
            cbRgbAudio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio4", 0);


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
          }
        }
      }
      LoadDVBSSettings();
      if (!result)
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
        card.ID = CardId;
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
              if (cd.CaptureDevice.DeviceId == card.DeviceId)
              {
                if (card.VideoDeviceMoniker == cd.VideoDeviceMoniker &&
                  card.VideoDevice == cd.CaptureDevice.VideoDevice &&
                  card.CommercialName == cd.CaptureDevice.CommercialName)
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
    void LoadDVBSSettings()
    {
      if (CaptureCard == null) return;
      string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);


      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename))
      {
        lnb0MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750).ToString();
        lnb1MHZ.Text = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600).ToString();
        lnbswMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Switch", 11700).ToString();
        cbandMHZ.Text = xmlreader.GetValueAsInt("dvbs", "CBand", 5150).ToString();
        circularMHZ.Text = xmlreader.GetValueAsInt("dvbs", "Circular", 10750).ToString();
        useLNB1.Checked = true;
        useLNB2.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB2", false);
        useLNB3.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB3", false);
        useLNB4.Checked = xmlreader.GetValueAsBool("dvbs", "useLNB4", false);

        int lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 44);
        switch (lnbKhz)
        {
          case 0: lnbconfig1.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig1.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig1.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig1.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 44);
        switch (lnbKhz)
        {
          case 0: lnbconfig2.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig2.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig2.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig2.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 44);
        switch (lnbKhz)
        {
          case 0: lnbconfig3.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig3.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig3.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig3.SelectedItem = "44 KHz"; break;
        }
        lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 44);
        switch (lnbKhz)
        {
          case 0: lnbconfig4.SelectedItem = "0 KHz"; break;
          case 22: lnbconfig4.SelectedItem = "22 KHz"; break;
          case 33: lnbconfig4.SelectedItem = "33 KHz"; break;
          case 44: lnbconfig4.SelectedItem = "44 KHz"; break;
        }

        int lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
        switch (lnbKind)
        {
          case 0: lnbkind1.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind1.SelectedItem = "C-Band"; break;
          case 2: lnbkind1.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
        switch (lnbKind)
        {
          case 0: lnbkind2.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind2.SelectedItem = "C-Band"; break;
          case 2: lnbkind2.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
        switch (lnbKind)
        {
          case 0: lnbkind3.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind3.SelectedItem = "C-Band"; break;
          case 2: lnbkind3.SelectedItem = "Circular"; break;
        }
        lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
        switch (lnbKind)
        {
          case 0: lnbkind4.SelectedItem = "Ku-Band"; break;
          case 1: lnbkind4.SelectedItem = "C-Band"; break;
          case 2: lnbkind4.SelectedItem = "Circular"; break;
        }
        int diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 0);
        switch (diseqc)
        {
          case 0: diseqca.SelectedItem = "None"; break;
          case 1: diseqca.SelectedItem = "Simple A"; break;
          case 2: diseqca.SelectedItem = "Simple B"; break;
          case 3: diseqca.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqca.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqca.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqca.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 0);
        switch (diseqc)
        {
          case 0: diseqcb.SelectedItem = "None"; break;
          case 1: diseqcb.SelectedItem = "Simple A"; break;
          case 2: diseqcb.SelectedItem = "Simple B"; break;
          case 3: diseqcb.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcb.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcb.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcb.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 0);
        switch (diseqc)
        {
          case 0: diseqcc.SelectedItem = "None"; break;
          case 1: diseqcc.SelectedItem = "Simple A"; break;
          case 2: diseqcc.SelectedItem = "Simple B"; break;
          case 3: diseqcc.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcc.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcc.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcc.SelectedItem = "Level 1 B/B"; break;

        }
        diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 0);
        switch (diseqc)
        {
          case 0: diseqcd.SelectedItem = "None"; break;
          case 1: diseqcd.SelectedItem = "Simple A"; break;
          case 2: diseqcd.SelectedItem = "Simple B"; break;
          case 3: diseqcd.SelectedItem = "Level 1 A/A"; break;
          case 4: diseqcd.SelectedItem = "Level 1 B/A"; break;
          case 5: diseqcd.SelectedItem = "Level 1 A/B"; break;
          case 6: diseqcd.SelectedItem = "Level 1 B/B"; break;

        }


      }
    }

    void SaveDVBSSettings()
    {
      // save settings
      if (CaptureCard != null && CaptureCard.FriendlyName != String.Empty)
      {
        string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);


        using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(filename))
        {
          xmlwriter.SetValue("dvbs", "LNB0", lnb0MHZ.Text);
          xmlwriter.SetValue("dvbs", "LNB1", lnb1MHZ.Text);
          xmlwriter.SetValue("dvbs", "Switch", lnbswMHZ.Text);
          xmlwriter.SetValue("dvbs", "CBand", cbandMHZ.Text);
          xmlwriter.SetValue("dvbs", "Circular", circularMHZ.Text);
          xmlwriter.SetValueAsBool("dvbs", "useLNB1", true);
          xmlwriter.SetValueAsBool("dvbs", "useLNB2", useLNB2.Checked);
          xmlwriter.SetValueAsBool("dvbs", "useLNB3", useLNB3.Checked);
          xmlwriter.SetValueAsBool("dvbs", "useLNB4", useLNB4.Checked);

          if (diseqca.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "diseqc", diseqca.SelectedIndex);
          }
          if (diseqcb.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "diseqc2", diseqcb.SelectedIndex);
          }
          if (diseqcc.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "diseqc3", diseqcc.SelectedIndex);
          }
          if (diseqcd.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "diseqc4", diseqcd.SelectedIndex);
          }
          if (lnbconfig1.SelectedIndex >= 0)
          {
            switch (lnbconfig1.SelectedIndex)
            {
              case 0: xmlwriter.SetValue("dvbs", "lnb", 0); break;
              case 1: xmlwriter.SetValue("dvbs", "lnb", 22); break;
              case 2: xmlwriter.SetValue("dvbs", "lnb", 33); break;
              case 3: xmlwriter.SetValue("dvbs", "lnb", 44); break;
            }
          }
          if (lnbconfig2.SelectedIndex >= 0)
          {
            switch (lnbconfig2.SelectedIndex)
            {
              case 0: xmlwriter.SetValue("dvbs", "lnb2", 0); break;
              case 1: xmlwriter.SetValue("dvbs", "lnb2", 22); break;
              case 2: xmlwriter.SetValue("dvbs", "lnb2", 33); break;
              case 3: xmlwriter.SetValue("dvbs", "lnb2", 44); break;
            }
          }
          if (lnbconfig3.SelectedIndex >= 0)
          {
            switch (lnbconfig3.SelectedIndex)
            {
              case 0: xmlwriter.SetValue("dvbs", "lnb3", 0); break;
              case 1: xmlwriter.SetValue("dvbs", "lnb3", 22); break;
              case 2: xmlwriter.SetValue("dvbs", "lnb3", 33); break;
              case 3: xmlwriter.SetValue("dvbs", "lnb3", 44); break;
            }
          }
          if (lnbconfig4.SelectedIndex >= 0)
          {
            switch (lnbconfig4.SelectedIndex)
            {
              case 0: xmlwriter.SetValue("dvbs", "lnb4", 0); break;
              case 1: xmlwriter.SetValue("dvbs", "lnb4", 22); break;
              case 2: xmlwriter.SetValue("dvbs", "lnb4", 33); break;
              case 3: xmlwriter.SetValue("dvbs", "lnb4", 44); break;
            }
          }

          if (lnbkind1.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "lnbKind", lnbkind1.SelectedIndex);
          }
          if (lnbkind2.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "lnbKind2", lnbkind2.SelectedIndex);
          }
          if (lnbkind3.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "lnbKind3", lnbkind3.SelectedIndex);
          }
          if (lnbkind4.SelectedIndex >= 0)
          {
            xmlwriter.SetValue("dvbs", "lnbKind4", lnbkind4.SelectedIndex);
          }
        }
      }
    }

    private void tabPage4_Click(object sender, System.EventArgs e)
    {

    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      // save settings for card
      if (CaptureCard != null)
      {
        if (CaptureCard.CardType == TVCapture.CardTypes.Digital_SS2)
        {
          string filename = String.Format(@"database\card_{0}.xml", CaptureCard.FriendlyName);
          // save settings for get the filename in mp.xml
          using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
          {
            xmlwriter.SetValue("dvb_ts_cards", "filename", filename);
          }
        }
      }

      if (CaptureCard.Network == NetworkType.Analog)
      {
        AnalogTVTuningForm dialog = new AnalogTVTuningForm();
        ITuning tuning = GraphFactory.CreateTuning(CaptureCard);
        if (tuning != null)
        {
          dialog.Tuning = tuning;
          dialog.Card = CaptureCard;
          dialog.ShowDialog(this);
        }
        else
        {
          MessageBox.Show("This device does not support auto tuning", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      else
      {
        DigitalTVTuningForm dialog = new DigitalTVTuningForm();
        ITuning tuning = GraphFactory.CreateTuning(CaptureCard);
        if (tuning != null)
        {
          dialog.Tuning = tuning;
          dialog.Card = CaptureCard;
          dialog.ShowDialog(this);
        }
        else
        {
          MessageBox.Show("This device does not support auto tuning", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
    }


    private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      try
      {
        if (tabControl1.SelectedTab == tabPage4)
        {

          TVCaptureDevice capture = CaptureCard;
          if (capture == null)
          {
            MessageBox.Show("Please fill in the capture card details first", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tabControl1.SelectedIndex = 0;
            return;
          }

          if (!capture.CreateGraph())
          {
            MessageBox.Show("Unable to create directshow graph", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tabControl1.SelectedIndex = 0;
            return;
          }
          if (capture.Network != NetworkType.DVBS)
          {
            capture.DeleteGraph();
            MessageBox.Show("This section is only for DVB-S cards", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tabControl1.SelectedIndex = 0;
            return;
          }
          capture.DeleteGraph();
        }


      }
      catch (Exception ex)
      {
        Log.Write("Switch tabs exception:{0} {1} {2}",
          ex.Message, ex.Source, ex.StackTrace);
      }
    }


    private void groupBox2_Enter(object sender, System.EventArgs e)
    {

    }

    private void diseqca_SelectedIndexChanged(object sender, System.EventArgs e)
    {

    }

    private void button5_Click(object sender, System.EventArgs e)
    {

    }

    private void diseqcb_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void diseqcc_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void diseqcd_SelectedIndexChanged(object sender, System.EventArgs e)
    {
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

    private void btnRadio_Click(object sender, System.EventArgs e)
    {
      if (CaptureCard.Network == NetworkType.Analog)
      {
        AnalogRadioTuningForm dialog = new AnalogRadioTuningForm();
        ITuning tuning = new AnalogRadioTuning();
        if (tuning != null)
        {
          dialog.Tuning = tuning;
          dialog.Card = CaptureCard;
          dialog.ShowDialog(this);
        }
        else
        {
          MessageBox.Show("This device does not support auto tuning", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
    }


    private void tabPage6_Click(object sender, System.EventArgs e)
    {

    }

    private void tabPage5_Click(object sender, System.EventArgs e)
    {

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
