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
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.PowerScheduler
{
  /// <summary>
  /// Summary description for PowerSchedulerSetupForm.
  /// </summary>
  public class PowerSchedulerSetupForm : MPConfigForm
  {
    private MPButton cb_ok;
    private MPLabel label2;
    private MPLabel label3;
    private MPLabel label1;
    private MPComboBox cobx_shutdown;
    private MPCheckBox cbxExtensive;
    private MPCheckBox cbxForced;
    private MPGroupBox groupBox1;
    private MPCheckBox cbxReinit;
    private MPNumericUpDown nud_wakeup;
    private MPNumericUpDown nud_shutdown;
    private MPButton btnCancel;
    private MPGroupBox groupBoxPowerSavings;
    private MPCheckBox cbxPreventMonitorPowerDown;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public PowerSchedulerSetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      LoadSettings();
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

    private void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        nud_wakeup.Value = xmlreader.GetValueAsInt("powerscheduler", "wakeupinterval", 1);
        nud_shutdown.Value = xmlreader.GetValueAsInt("powerscheduler", "shutdowninterval", 0);
        cobx_shutdown.Text = xmlreader.GetValueAsString("powerscheduler", "shutdownmode", "Suspend");
        cbxExtensive.Checked = xmlreader.GetValueAsBool("powerscheduler", "extensivelogging", false);
        cbxForced.Checked = xmlreader.GetValueAsBool("powerscheduler", "forcedshutdown", false);
        cbxReinit.Checked = xmlreader.GetValueAsBool("powerscheduler", "reinitonresume", false);
        cbxPreventMonitorPowerDown.Checked = xmlreader.GetValueAsBool("powerscheduler", "preventmonitorpowerdown", false);
      }
    }

    private bool SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("powerscheduler", "wakeupinterval", nud_wakeup.Value);
        xmlwriter.SetValue("powerscheduler", "shutdowninterval", nud_shutdown.Value);
        xmlwriter.SetValue("powerscheduler", "shutdownmode", cobx_shutdown.Text);
        xmlwriter.SetValueAsBool("powerscheduler", "extensivelogging", cbxExtensive.Checked);
        xmlwriter.SetValueAsBool("powerscheduler", "forcedshutdown", cbxForced.Checked);
        xmlwriter.SetValueAsBool("powerscheduler", "reinitonresume", cbxReinit.Checked);
        xmlwriter.SetValueAsBool("powerscheduler", "preventmonitorpowerdow", cbxPreventMonitorPowerDown.Checked);
      }
      return true;
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.cb_ok = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cobx_shutdown = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbxExtensive = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbxForced = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbxReinit = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.nud_wakeup = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.nud_shutdown = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxPowerSavings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbxPreventMonitorPowerDown = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.nud_wakeup)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.nud_shutdown)).BeginInit();
      this.groupBoxPowerSavings.SuspendLayout();
      this.SuspendLayout();
      // 
      // cb_ok
      // 
      this.cb_ok.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cb_ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cb_ok.Location = new System.Drawing.Point(205, 339);
      this.cb_ok.Name = "cb_ok";
      this.cb_ok.Size = new System.Drawing.Size(75, 23);
      this.cb_ok.TabIndex = 0;
      this.cb_ok.Text = "&OK";
      this.cb_ok.UseVisualStyleBackColor = true;
      this.cb_ok.Click += new System.EventHandler(this.cb_ok_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(9, 8);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(296, 45);
      this.label2.TabIndex = 0;
      this.label2.Text = "Time in minutes to resume system before recording starts. Zero (0) minutes disabl" +
                         "es wakeup. (1 min recommended, value must be lower then Idle time)";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(9, 58);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(304, 40);
      this.label3.TabIndex = 0;
      this.label3.Text = "Idle time in minutes (MP HOME) before system shuts down. Zero (0) minutes disable" +
                         "s shutdown. (at least 3 min recommended)";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(9, 115);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(96, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Shutdown mode";
      // 
      // cobx_shutdown
      // 
      this.cobx_shutdown.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cobx_shutdown.BorderColor = System.Drawing.Color.Empty;
      this.cobx_shutdown.Items.AddRange(new object[]
                                          {
                                            "Hibernate",
                                            "Suspend",
                                            "Shutdown (no wakeup)",
                                            "None (or windows inbuilt)"
                                          });
      this.cobx_shutdown.Location = new System.Drawing.Point(126, 112);
      this.cobx_shutdown.Name = "cobx_shutdown";
      this.cobx_shutdown.Size = new System.Drawing.Size(235, 21);
      this.cobx_shutdown.TabIndex = 14;
      // 
      // cbxExtensive
      // 
      this.cbxExtensive.AutoSize = true;
      this.cbxExtensive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbxExtensive.Location = new System.Drawing.Point(13, 42);
      this.cbxExtensive.Name = "cbxExtensive";
      this.cbxExtensive.Size = new System.Drawing.Size(107, 17);
      this.cbxExtensive.TabIndex = 17;
      this.cbxExtensive.Text = "Extensive logging";
      this.cbxExtensive.UseVisualStyleBackColor = true;
      // 
      // cbxForced
      // 
      this.cbxForced.AutoSize = true;
      this.cbxForced.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbxForced.Location = new System.Drawing.Point(13, 19);
      this.cbxForced.Name = "cbxForced";
      this.cbxForced.Size = new System.Drawing.Size(106, 17);
      this.cbxForced.TabIndex = 15;
      this.cbxForced.Text = "Forced shutdown";
      this.cbxForced.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbxReinit);
      this.groupBox1.Controls.Add(this.cbxForced);
      this.groupBox1.Controls.Add(this.cbxExtensive);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(12, 148);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(349, 97);
      this.groupBox1.TabIndex = 18;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Advanced options";
      // 
      // cbxReinit
      // 
      this.cbxReinit.AutoSize = true;
      this.cbxReinit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbxReinit.Location = new System.Drawing.Point(13, 65);
      this.cbxReinit.Name = "cbxReinit";
      this.cbxReinit.Size = new System.Drawing.Size(230, 17);
      this.cbxReinit.TabIndex = 18;
      this.cbxReinit.Text = "Re-init tuners on resume (may cause issues)";
      this.cbxReinit.UseVisualStyleBackColor = true;
      // 
      // nud_wakeup
      // 
      this.nud_wakeup.Location = new System.Drawing.Point(304, 12);
      this.nud_wakeup.Name = "nud_wakeup";
      this.nud_wakeup.Size = new System.Drawing.Size(57, 20);
      this.nud_wakeup.TabIndex = 12;
      // 
      // nud_shutdown
      // 
      this.nud_shutdown.Location = new System.Drawing.Point(304, 67);
      this.nud_shutdown.Name = "nud_shutdown";
      this.nud_shutdown.Size = new System.Drawing.Size(57, 20);
      this.nud_shutdown.TabIndex = 13;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(286, 339);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 19;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // groupBoxPowerSavings
      // 
      this.groupBoxPowerSavings.Controls.Add(this.cbxPreventMonitorPowerDown);
      this.groupBoxPowerSavings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPowerSavings.Location = new System.Drawing.Point(12, 264);
      this.groupBoxPowerSavings.Name = "groupBoxPowerSavings";
      this.groupBoxPowerSavings.Size = new System.Drawing.Size(349, 58);
      this.groupBoxPowerSavings.TabIndex = 20;
      this.groupBoxPowerSavings.TabStop = false;
      this.groupBoxPowerSavings.Text = "Power Saving Options";
      // 
      // cbxPreventMonitorPowerDown
      // 
      this.cbxPreventMonitorPowerDown.AutoSize = true;
      this.cbxPreventMonitorPowerDown.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbxPreventMonitorPowerDown.Location = new System.Drawing.Point(13, 26);
      this.cbxPreventMonitorPowerDown.Name = "cbxPreventMonitorPowerDown";
      this.cbxPreventMonitorPowerDown.Size = new System.Drawing.Size(244, 17);
      this.cbxPreventMonitorPowerDown.TabIndex = 5;
      this.cbxPreventMonitorPowerDown.Text = "Prevent Power Down of Monitor while in Home";
      this.cbxPreventMonitorPowerDown.UseVisualStyleBackColor = true;
      // 
      // PowerSchedulerSetupForm
      // 
      this.AcceptButton = this.cb_ok;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(373, 374);
      this.Controls.Add(this.groupBoxPowerSavings);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.nud_shutdown);
      this.Controls.Add(this.nud_wakeup);
      this.Controls.Add(this.cobx_shutdown);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cb_ok);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "PowerSchedulerSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "PowerScheduler - Setup";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.nud_wakeup)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.nud_shutdown)).EndInit();
      this.groupBoxPowerSavings.ResumeLayout(false);
      this.groupBoxPowerSavings.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    private bool CheckValues()
    {
      if (nud_shutdown.Value > 0)
      {
        if (nud_wakeup.Value >= nud_shutdown.Value)
        {
          MessageBox.Show(
            "Resume time (before recording) is not smaller then Idle time!\n" +
            "This will lead to recording problems!\n" +
            "\n" +
            "One Minute will be added to Resume time",
            "PROBLEM found at value check",
            MessageBoxButtons.OK);
          nud_shutdown.Value = nud_wakeup.Value + 1;
          return true;
        }
      }
      return true;
    }


    private void cb_ok_Click(object sender, EventArgs e)
    {
      if (CheckValues())
      {
        if (SaveSettings())
        {
          this.Close();
        }
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      LoadSettings();
      this.Close();
    }
  }
}