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
  public class Wizard_DVBSTV : MediaPortal.Configuration.SectionSettings, AutoTuneCallback
  {
    private System.ComponentModel.IContainer components = null;
    class Transponder
    {
      public string SatName;
      public string FileName;
      public override string ToString()
      {
        return SatName;
      }
    }

    string _description = "";
    TVCaptureDevice captureCard;
    int m_diseqcLoops = 1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private System.Windows.Forms.ProgressBar progressBar3;
    private MediaPortal.UserInterface.Controls.MPButton button3;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private System.Windows.Forms.Panel panel1;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder;
    private MediaPortal.UserInterface.Controls.MPLabel lblStatus;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder2;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder3;
    private MediaPortal.UserInterface.Controls.MPComboBox cbTransponder4;
    int m_currentDiseqc = 1;
    string[] _tplFiles;

    public Wizard_DVBSTV()
      : this("DVB-S TV")
    {
    }

    public Wizard_DVBSTV(string name)
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
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbTransponder4 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbTransponder3 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbTransponder2 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbTransponder = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressBar3 = new System.Windows.Forms.ProgressBar();
      this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.cbTransponder4);
      this.groupBox3.Controls.Add(this.cbTransponder3);
      this.groupBox3.Controls.Add(this.cbTransponder2);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.cbTransponder);
      this.groupBox3.Controls.Add(this.panel1);
      this.groupBox3.Controls.Add(this.lblStatus);
      this.groupBox3.Controls.Add(this.progressBar3);
      this.groupBox3.Controls.Add(this.button3);
      this.groupBox3.Controls.Add(this.label7);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Setup Digital TV (DVB-S Satellite)";
      // 
      // cbTransponder4
      // 
      this.cbTransponder4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder4.Location = new System.Drawing.Point(152, 140);
      this.cbTransponder4.Name = "cbTransponder4";
      this.cbTransponder4.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder4.TabIndex = 8;
      // 
      // cbTransponder3
      // 
      this.cbTransponder3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder3.Location = new System.Drawing.Point(152, 116);
      this.cbTransponder3.Name = "cbTransponder3";
      this.cbTransponder3.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder3.TabIndex = 6;
      // 
      // cbTransponder2
      // 
      this.cbTransponder2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder2.Location = new System.Drawing.Point(152, 92);
      this.cbTransponder2.Name = "cbTransponder2";
      this.cbTransponder2.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder2.TabIndex = 4;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 144);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(120, 16);
      this.label3.TabIndex = 7;
      this.label3.Text = "Transponder for LNB4:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 120);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(120, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Transponder for LNB3:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 96);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(120, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "Transponder for LNB2:";
      // 
      // cbTransponder
      // 
      this.cbTransponder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder.Location = new System.Drawing.Point(152, 68);
      this.cbTransponder.Name = "cbTransponder";
      this.cbTransponder.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder.TabIndex = 2;
      // 
      // panel1
      // 
      this.panel1.Location = new System.Drawing.Point(432, 360);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1, 1);
      this.panel1.TabIndex = 12;
      // 
      // lblStatus
      // 
      this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.lblStatus.Location = new System.Drawing.Point(16, 232);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(440, 87);
      this.lblStatus.TabIndex = 10;
      // 
      // progressBar3
      // 
      this.progressBar3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar3.Location = new System.Drawing.Point(16, 200);
      this.progressBar3.Name = "progressBar3";
      this.progressBar3.Size = new System.Drawing.Size(440, 16);
      this.progressBar3.Step = 1;
      this.progressBar3.TabIndex = 9;
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.Location = new System.Drawing.Point(384, 336);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(72, 22);
      this.button3.TabIndex = 11;
      this.button3.Text = "Scan";
      this.button3.Click += new System.EventHandler(this.button1_Click);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 72);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(120, 16);
      this.label7.TabIndex = 1;
      this.label7.Text = "Transponder for LNB1:";
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.Location = new System.Drawing.Point(16, 24);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(440, 32);
      this.label8.TabIndex = 0;
      this.label8.Text = "Mediaportal has detected one or more digital TV cards. Select your transponder an" +
        "d press \"Scan\" to get TV and radio channels.";
      // 
      // Wizard_DVBSTV
      // 
      this.Controls.Add(this.groupBox3);
      this.Name = "Wizard_DVBSTV";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    Transponder LoadTransponderName(string fileName)
    {
      Transponder ts = new Transponder();
      ts.FileName = fileName;
      ts.SatName = fileName;

      string line;
      System.IO.TextReader tin = System.IO.File.OpenText(@"Tuningparameters\"+fileName);
      while (true)
      {
        line = tin.ReadLine();
        if (line == null) break;
        string search = line.ToLower();
        int pos = search.IndexOf("satname");
        if (pos >= 0)
        {
          pos = search.IndexOf("=");
          if (pos > 0)
          {
            ts.SatName = line.Substring(pos + 1);
            ts.SatName = ts.SatName.Trim();
            break;
          }
        }
      }
      tin.Close();

      return ts;
    }

    public override void OnSectionActivated()
    {
      lblStatus.Text = "";
      cbTransponder.Items.Clear();
      cbTransponder2.Items.Clear();
      cbTransponder3.Items.Clear();
      cbTransponder4.Items.Clear();
      string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters");
      foreach (string file in files)
      {
        string fileName = System.IO.Path.GetFileName(file);
        if (fileName.ToLower().IndexOf(".tpl") >= 0)
        {
          Transponder ts = LoadTransponderName(fileName);
          if (ts != null)
          {
            cbTransponder.Items.Add(ts);
            cbTransponder2.Items.Add(ts);
            cbTransponder3.Items.Add(ts);
            cbTransponder4.Items.Add(ts);
          }
        }
      }
      if (cbTransponder.Items.Count > 0)
        cbTransponder.SelectedIndex = 0;
      if (cbTransponder2.Items.Count > 0)
        cbTransponder2.SelectedIndex = 0;
      if (cbTransponder3.Items.Count > 0)
        cbTransponder3.SelectedIndex = 0;
      if (cbTransponder4.Items.Count > 0)
        cbTransponder4.SelectedIndex = 0;

      m_diseqcLoops = 1;
      cbTransponder2.Enabled = false;
      cbTransponder3.Enabled = false;
      cbTransponder4.Enabled = false;

      TVCaptureCards cards = new TVCaptureCards();
      cards.LoadCaptureCards();
      foreach (TVCaptureDevice dev in cards.captureCards)
      {
        if (dev.Network == NetworkType.DVBS)
        {
          captureCard = dev;
          break;
        }
      }

      if (captureCard == null) return;
      string filename = String.Format(@"database\card_{0}.xml", captureCard.FriendlyName);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        if (xmlreader.GetValueAsBool("dvbs", "useLNB2", false) == true)
        {
          m_diseqcLoops++;
          cbTransponder2.Enabled = true;
        }
        if (xmlreader.GetValueAsBool("dvbs", "useLNB3", false) == true)
        {
          m_diseqcLoops++;
          cbTransponder3.Enabled = true;
        }
        if (xmlreader.GetValueAsBool("dvbs", "useLNB4", false) == true)
        {
          m_diseqcLoops++;
          cbTransponder4.Enabled = true;
        }
      }
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      _tplFiles = new string[m_currentDiseqc];
      Transponder ts = (Transponder)cbTransponder.SelectedItem;
      _tplFiles[0] = @"Tuningparameters\" + ts.FileName;

      if (m_currentDiseqc == 1)
      {
        ts = (Transponder)cbTransponder.SelectedItem;
        _tplFiles[0] = @"Tuningparameters\" + ts.FileName;
      }
      if (m_currentDiseqc == 2)
      {
        ts = (Transponder)cbTransponder2.SelectedItem;
        _tplFiles[1] = @"Tuningparameters\" + ts.FileName;
      }
      if (m_currentDiseqc == 3)
      {
        ts = (Transponder)cbTransponder3.SelectedItem;
        _tplFiles[2] = @"Tuningparameters\" + ts.FileName;
      }
      if (m_currentDiseqc == 4)
      {
        ts = (Transponder)cbTransponder4.SelectedItem;
        _tplFiles[3] = @"Tuningparameters\" + ts.FileName;
      }

      cbTransponder.Enabled = false;
      cbTransponder2.Enabled = false;
      cbTransponder3.Enabled = false;
      cbTransponder4.Enabled = false;
      progressBar3.Visible = true;
      button3.Enabled = false;
      GUIGraphicsContext.form = this.FindForm();
      GUIGraphicsContext.VideoWindow = new Rectangle(panel1.Location, panel1.Size);
      if (captureCard != null)
      {
        Thread thread = new Thread(new ThreadStart(DoScan));
        thread.IsBackground = true;
        thread.Start();
      }
    }

    private void DoScan()
    {
      captureCard.CreateGraph();
      ITuning tuning = new DVBSTuning();
      tuning.AutoTuneTV(captureCard, this, _tplFiles);
      tuning.Start();
      while (!tuning.IsFinished())
      {
        tuning.Next();
      }
      captureCard.DeleteGraph();

      captureCard = null;
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
      lblStatus.Text = description;
    }
    public void OnStatus2(string description)
    {
      _description = description;
    }

    public void OnProgress(int percentDone)
    {
      progressBar3.Value = percentDone;
      if (percentDone >= 100)
      {
        button3.Enabled = true;
        lblStatus.Text = _description;
      }
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

