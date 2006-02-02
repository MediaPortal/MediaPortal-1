#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProcessPlugins.CallerId
{
	/// <summary>
	/// Summary description for ISDNSetupForm.
	/// </summary>
	public class ISDNSetupForm : System.Windows.Forms.Form
	{
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStopMedia;
    private System.Windows.Forms.GroupBox groupBoxIncomingCall;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private System.Windows.Forms.RadioButton radioButtonManualResume;
    private System.Windows.Forms.RadioButton radioButtonAutoResume;
    private System.Windows.Forms.NumericUpDown numericUpDownTimeOut;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxTimeOut;
    private CheckBox checkBoxOutlook;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ISDNSetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        checkBoxOutlook.Checked         = xmlreader.GetValueAsBool("isdn", "useoutlook", false);
        checkBoxTimeOut.Checked         = (xmlreader.GetValueAsInt("isdn", "timeout", 0) > 0);
        checkBoxStopMedia.Checked       = xmlreader.GetValueAsBool("isdn", "stopmedia", true);
        radioButtonManualResume.Checked = (xmlreader.GetValueAsBool("isdn", "autoresume", false) == false);
        radioButtonAutoResume.Checked   = !radioButtonManualResume.Checked;
        numericUpDownTimeOut.Value      = xmlreader.GetValueAsInt("isdn", "timeout", 0);
      }
      radioButtonManualResume.Enabled = checkBoxStopMedia.Checked;
      radioButtonAutoResume.Enabled   = (checkBoxStopMedia.Checked && checkBoxTimeOut.Checked);
      numericUpDownTimeOut.Enabled    = checkBoxTimeOut.Checked;
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ISDNSetupForm));
      this.checkBoxStopMedia = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxIncomingCall = new System.Windows.Forms.GroupBox();
      this.numericUpDownTimeOut = new System.Windows.Forms.NumericUpDown();
      this.checkBoxTimeOut = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButtonAutoResume = new System.Windows.Forms.RadioButton();
      this.radioButtonManualResume = new System.Windows.Forms.RadioButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxOutlook = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxIncomingCall.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOut)).BeginInit();
      this.SuspendLayout();
      // 
      // checkBoxStopMedia
      // 
      this.checkBoxStopMedia.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxStopMedia.Location = new System.Drawing.Point(16, 73);
      this.checkBoxStopMedia.Name = "checkBoxStopMedia";
      this.checkBoxStopMedia.Size = new System.Drawing.Size(184, 16);
      this.checkBoxStopMedia.TabIndex = 2;
      this.checkBoxStopMedia.Text = "Stop media on incoming call";
      this.checkBoxStopMedia.CheckedChanged += new System.EventHandler(this.checkBoxStopMedia_CheckedChanged);
      // 
      // groupBoxIncomingCall
      // 
      this.groupBoxIncomingCall.Controls.Add(this.checkBoxOutlook);
      this.groupBoxIncomingCall.Controls.Add(this.numericUpDownTimeOut);
      this.groupBoxIncomingCall.Controls.Add(this.checkBoxTimeOut);
      this.groupBoxIncomingCall.Controls.Add(this.radioButtonAutoResume);
      this.groupBoxIncomingCall.Controls.Add(this.radioButtonManualResume);
      this.groupBoxIncomingCall.Controls.Add(this.checkBoxStopMedia);
      this.groupBoxIncomingCall.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxIncomingCall.Location = new System.Drawing.Point(16, 16);
      this.groupBoxIncomingCall.Name = "groupBoxIncomingCall";
      this.groupBoxIncomingCall.Size = new System.Drawing.Size(320, 152);
      this.groupBoxIncomingCall.TabIndex = 2;
      this.groupBoxIncomingCall.TabStop = false;
      this.groupBoxIncomingCall.Text = "Incoming call";
      // 
      // numericUpDownTimeOut
      // 
      this.numericUpDownTimeOut.Location = new System.Drawing.Point(224, 48);
      this.numericUpDownTimeOut.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownTimeOut.Name = "numericUpDownTimeOut";
      this.numericUpDownTimeOut.Size = new System.Drawing.Size(48, 20);
      this.numericUpDownTimeOut.TabIndex = 1;
      this.numericUpDownTimeOut.ValueChanged += new System.EventHandler(this.numericUpDownTimeOut_ValueChanged);
      // 
      // checkBoxTimeOut
      // 
      this.checkBoxTimeOut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxTimeOut.Location = new System.Drawing.Point(16, 49);
      this.checkBoxTimeOut.Name = "checkBoxTimeOut";
      this.checkBoxTimeOut.Size = new System.Drawing.Size(208, 16);
      this.checkBoxTimeOut.TabIndex = 0;
      this.checkBoxTimeOut.Text = "Notification window auto-timeout (sec.):";
      this.checkBoxTimeOut.CheckedChanged += new System.EventHandler(this.checkBoxTimeOut_CheckedChanged);
      // 
      // radioButtonAutoResume
      // 
      this.radioButtonAutoResume.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonAutoResume.Location = new System.Drawing.Point(32, 121);
      this.radioButtonAutoResume.Name = "radioButtonAutoResume";
      this.radioButtonAutoResume.Size = new System.Drawing.Size(272, 16);
      this.radioButtonAutoResume.TabIndex = 4;
      this.radioButtonAutoResume.Text = "Auto-resume playback after notification has timed out";
      this.radioButtonAutoResume.EnabledChanged += new System.EventHandler(this.radioButtonAutoResume_EnabledChanged);
      // 
      // radioButtonManualResume
      // 
      this.radioButtonManualResume.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonManualResume.Location = new System.Drawing.Point(32, 97);
      this.radioButtonManualResume.Name = "radioButtonManualResume";
      this.radioButtonManualResume.Size = new System.Drawing.Size(224, 16);
      this.radioButtonManualResume.TabIndex = 3;
      this.radioButtonManualResume.Text = "Closing the dialog resumes playback";
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(189, 182);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(269, 182);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      // 
      // checkBoxOutlook
      // 
      this.checkBoxOutlook.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxOutlook.Location = new System.Drawing.Point(16, 24);
      this.checkBoxOutlook.Name = "checkBoxOutlook";
      this.checkBoxOutlook.Size = new System.Drawing.Size(208, 16);
      this.checkBoxOutlook.TabIndex = 5;
      this.checkBoxOutlook.Text = "Use Microsoft Outlook address book";
      // 
      // ISDNSetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(354, 216);
      this.ControlBox = false;
      this.Controls.Add(this.groupBoxIncomingCall);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ISDNSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "ISDN Caller-ID";
      this.groupBoxIncomingCall.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeOut)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    private void okButton_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("isdn", "useoutlook", checkBoxOutlook.Checked);
        xmlwriter.SetValueAsBool("isdn", "stopmedia", checkBoxStopMedia.Checked);
        xmlwriter.SetValueAsBool("isdn", "autoresume", radioButtonAutoResume.Checked);
        if (checkBoxTimeOut.Checked)
          xmlwriter.SetValue("isdn", "timeout", numericUpDownTimeOut.Value);
        else
          xmlwriter.SetValue("isdn", "timeout", 0);
      }
      this.Close();
    }

    private void checkBoxStopMedia_CheckedChanged(object sender, System.EventArgs e)
    {
      radioButtonManualResume.Enabled = checkBoxStopMedia.Checked;
      radioButtonAutoResume.Enabled = checkBoxStopMedia.Checked;
      numericUpDownTimeOut.Enabled = (checkBoxStopMedia.Checked) && (radioButtonAutoResume.Checked);
    }

    private void checkBoxTimeOut_CheckedChanged(object sender, System.EventArgs e)
    {
      numericUpDownTimeOut.Enabled = checkBoxTimeOut.Checked;
      radioButtonAutoResume.Enabled = (checkBoxTimeOut.Checked && (numericUpDownTimeOut.Value > 0));
    }

    private void numericUpDownTimeOut_ValueChanged(object sender, System.EventArgs e)
    {
      radioButtonAutoResume.Enabled = (numericUpDownTimeOut.Value > 0);
    }

    private void radioButtonAutoResume_EnabledChanged(object sender, System.EventArgs e)
    {
      if (!radioButtonAutoResume.Enabled)
        radioButtonManualResume.Checked = true;
    }
	}
}
