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
using MediaPortal.TV.Recording;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108,649

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_ATSCTV : Wizard_ScanBase
  {
    private MPGroupBox groupBox1;
    private MPLabel label1;
    private MPLabel label2;
    private MPLabel mpLabel2;
    private MPLabel mpLabel1;
    private MPLabel mpLabel3;


    public Wizard_ATSCTV()
      : this("DVBT TV")
    {
    }

    public Wizard_ATSCTV(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      _card = null;

      // TODO: Add any initialization after the InitializeComponent call
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (Wizard_ATSCTV));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
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
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.lblStatus2);
      this.groupBox1.Controls.Add(this.mpLabel3);
      this.groupBox1.Controls.Add(this.progressBarQuality);
      this.groupBox1.Controls.Add(this.mpLabel2);
      this.groupBox1.Controls.Add(this.mpLabel1);
      this.groupBox1.Controls.Add(this.progressBarStrength);
      this.groupBox1.Controls.Add(this.lblStatus);
      this.groupBox1.Controls.Add(this.progressBarProgress);
      this.groupBox1.Controls.Add(this.buttonScan);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(5, 5);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(532, 391);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Setup Digital TV (ATSC)";
      // 
      // lblStatus2
      // 
      this.lblStatus2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular,
                                                     System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.lblStatus2.Location = new System.Drawing.Point(16, 172);
      this.lblStatus2.Name = "lblStatus2";
      this.lblStatus2.Size = new System.Drawing.Size(440, 22);
      this.lblStatus2.TabIndex = 18;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(17, 65);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(100, 16);
      this.mpLabel3.TabIndex = 17;
      this.mpLabel3.Text = "Progress:";
      // 
      // progressBarQuality
      // 
      this.progressBarQuality.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarQuality.Location = new System.Drawing.Point(144, 108);
      this.progressBarQuality.Name = "progressBarQuality";
      this.progressBarQuality.Size = new System.Drawing.Size(280, 16);
      this.progressBarQuality.Step = 1;
      this.progressBarQuality.TabIndex = 16;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(17, 108);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(100, 16);
      this.mpLabel2.TabIndex = 15;
      this.mpLabel2.Text = "Signal quality:";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(17, 87);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(100, 16);
      this.mpLabel1.TabIndex = 14;
      this.mpLabel1.Text = "Signal strength:";
      // 
      // progressBarStrength
      // 
      this.progressBarStrength.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarStrength.Location = new System.Drawing.Point(144, 87);
      this.progressBarStrength.Name = "progressBarStrength";
      this.progressBarStrength.Size = new System.Drawing.Size(280, 16);
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
      this.lblStatus.Location = new System.Drawing.Point(16, 139);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(440, 23);
      this.lblStatus.TabIndex = 5;
      // 
      // progressBarProgress
      // 
      this.progressBarProgress.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarProgress.Location = new System.Drawing.Point(144, 65);
      this.progressBarProgress.Name = "progressBarProgress";
      this.progressBarProgress.Size = new System.Drawing.Size(280, 16);
      this.progressBarProgress.TabIndex = 4;
      // 
      // buttonScan
      // 
      this.buttonScan.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonScan.Location = new System.Drawing.Point(444, 62);
      this.buttonScan.Name = "button1";
      this.buttonScan.Size = new System.Drawing.Size(72, 22);
      this.buttonScan.TabIndex = 3;
      this.buttonScan.Text = "Scan";
      this.buttonScan.UseVisualStyleBackColor = true;
      this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
      // 
      // label1
      // 
      this.label1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(440, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Press \"Scan\" to scan for TV and radio channels.";
      // 
      // Wizard_DVBTTV
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Wizard_ATSCTV";
      this.Size = new System.Drawing.Size(545, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_card == null)
      {
        TVCaptureCards cards = new TVCaptureCards();
        cards.LoadCaptureCards();
        foreach (TVCaptureDevice dev in cards.captureCards)
        {
          if (dev.Network == NetworkType.ATSC)
          {
            _card = dev;
            break;
          }
        }
      }
      this.OnScanFinished += new ScanFinishedHandler(this.dlg_OnScanFinished);
      this.OnScanStarted += new ScanStartedHandler(this.dlg_OnScanStarted);
    }

    protected override String[] GetScanParameters()
    {
      return null;
    }

    private void dlg_OnScanFinished(object sender, EventArgs args)
    {
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(false);
        wizard.DisableNext(false);
      }
    }

    private void dlg_OnScanStarted(object sender, EventArgs args)
    {
      WizardForm wizard = WizardForm.Form;
      if (wizard != null)
      {
        wizard.DisableBack(true);
        wizard.DisableNext(true);
      }
    }
  }
}