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
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;


namespace MediaPortal.Configuration.Sections
{
  public class Wizard_DVBCTV : MediaPortal.Configuration.SectionSettings, AutoTuneCallback
  {
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel labelStatus;
    private MediaPortal.UserInterface.Controls.MPComboBox cbCountry;

    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private System.Windows.Forms.Panel panel1;
    string _description;

    public Wizard_DVBCTV()
      : this("DVBC TV")
    {
    }

    public Wizard_DVBCTV(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
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
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.labelStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbCountry = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Controls.Add(this.labelStatus);
      this.groupBox1.Controls.Add(this.progressBar1);
      this.groupBox1.Controls.Add(this.button1);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbCountry);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Setup Digital TV (DVB-C Cable)";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(16, 264);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(440, 56);
      this.label3.TabIndex = 6;
      this.label3.Text = @"NOTE: If your country is not present or if MediaPortal is unable to find any channels, MediaPortal probably doesn't know which frequencies to scan for your country. Edit the .dvbc file in the ""TuningParameters"" subfolder and fill in all the frequencies needed for your country.";
      // 
      // panel1
      // 
      this.panel1.Location = new System.Drawing.Point(424, 320);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1, 1);
      this.panel1.TabIndex = 7;
      // 
      // labelStatus
      // 
      this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.labelStatus.Location = new System.Drawing.Point(16, 169);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(440, 63);
      this.labelStatus.TabIndex = 5;
      // 
      // progressBar1
      // 
      this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar1.Location = new System.Drawing.Point(16, 128);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(440, 16);
      this.progressBar1.TabIndex = 4;
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(384, 67);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(72, 22);
      this.button1.TabIndex = 3;
      this.button1.Text = "Scan";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 16);
      this.label2.TabIndex = 1;
      this.label2.Text = "Country/Region:";
      // 
      // cbCountry
      // 
      this.cbCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCountry.Location = new System.Drawing.Point(144, 68);
      this.cbCountry.Name = "cbCountry";
      this.cbCountry.Size = new System.Drawing.Size(232, 21);
      this.cbCountry.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(440, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Mediaportal has detected one or more digital TV cards. Select your country and pr" +
        "ess \"Scan\" to get TV and radio channels.";
      // 
      // Wizard_DVBCTV
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Wizard_DVBCTV";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion




    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      labelStatus.Text = "";
      string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters");
      foreach (string file in files)
      {
        if (file.ToLower().IndexOf(".dvbc") >= 0)
        {
          cbCountry.Items.Add(System.IO.Path.GetFileName(file));
        }
      }

      if (cbCountry.Items.Count > 0)
        cbCountry.SelectedIndex = 0;
    }


    private void button1_Click(object sender, System.EventArgs e)
    {
      progressBar1.Visible = true;
      Thread thread = new Thread(new ThreadStart(DoScan));
      thread.Start();
    }

    private void DoScan()
    {
      cbCountry.Enabled = false;
      button1.Enabled = false;
      string countryName = (string)cbCountry.SelectedItem;
      GUIGraphicsContext.form = this.FindForm();
      GUIGraphicsContext.VideoWindow = new Rectangle(panel1.Location, panel1.Size);

      TVCaptureDevice captureCard = null;
      TVCaptureCards cards = new TVCaptureCards();
      cards.LoadCaptureCards();
      foreach (TVCaptureDevice dev in cards.captureCards)
      {
        if (dev.Network == NetworkType.DVBC)
        {
          captureCard = dev;
          captureCard.CreateGraph();
          break;
        }
      }
      if (captureCard == null)
      {
        button1.Enabled = true;
        cbCountry.Enabled = true;
        progressBar1.Value = 100;
        return;
      }
      captureCard.CreateGraph();
      ITuning tuning = new DVBCTuning();
      tuning.AutoTuneTV(captureCard, this, @"Tuningparameters\"+countryName);
      tuning.Start();
      while (!tuning.IsFinished())
      {
        tuning.Next();
      }
      captureCard.DeleteGraph();

      labelStatus.Text = _description;
      progressBar1.Value = 100;
      captureCard = null;
      button1.Enabled = true;
      cbCountry.Enabled = true;
    }

    #region AutoTuneCallback
    public void OnNewChannel()
    {
    }

    public void OnSignal(int quality, int strength)
    {
    }

    public void OnStatus(string description)
    {
      labelStatus.Text = description;
    }
    public void OnStatus2(string description)
    {
      _description = description;
    }

    public void OnProgress(int percentDone)
    {
      progressBar1.Value = percentDone;
    }

    public void OnEnded()
    {
    }
    public void UpdateList()
    {
    }


    #endregion
  }
}


