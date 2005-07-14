using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Win32;
using System.Runtime.InteropServices;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration.Sections
{

	public class DScalerAudioFilter : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxSPDIF;
		private System.Windows.Forms.ComboBox comboBoxSpeakerConfig;
		private System.Windows.Forms.CheckBox checkBoxDynamicRange;
		private System.Windows.Forms.TextBox textBoxAudioOffset;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxMPEGOverSPDIF;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public DScalerAudioFilter() : this("DScaler Audio Decoder")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public DScalerAudioFilter(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxAudioOffset = new System.Windows.Forms.TextBox();
      this.comboBoxSpeakerConfig = new System.Windows.Forms.ComboBox();
      this.checkBoxMPEGOverSPDIF = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.checkBoxSPDIF = new System.Windows.Forms.CheckBox();
      this.checkBoxDynamicRange = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.textBoxAudioOffset);
      this.groupBox1.Controls.Add(this.comboBoxSpeakerConfig);
      this.groupBox1.Controls.Add(this.checkBoxMPEGOverSPDIF);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.checkBoxSPDIF);
      this.groupBox1.Controls.Add(this.checkBoxDynamicRange);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 168);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(188, 136);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(40, 16);
      this.label1.TabIndex = 23;
      this.label1.Text = "msec.";
      // 
      // textBoxAudioOffset
      // 
      this.textBoxAudioOffset.Location = new System.Drawing.Point(152, 132);
      this.textBoxAudioOffset.Name = "textBoxAudioOffset";
      this.textBoxAudioOffset.Size = new System.Drawing.Size(32, 20);
      this.textBoxAudioOffset.TabIndex = 22;
      this.textBoxAudioOffset.Text = "0";
      // 
      // comboBoxSpeakerConfig
      // 
      this.comboBoxSpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeakerConfig.Items.AddRange(new object[] {
                                                               "Stereo",
                                                               "Dolby Stereo",
                                                               "4.0 (2 Front + 2 Rear)",
                                                               "4.1 (2 Front + 2 Rear + 1 Sub)",
                                                               "5.0 (3 Front + 2 Rear)",
                                                               "5.1 (3 Front + 2 Rear + 1 Sub)"});
      this.comboBoxSpeakerConfig.Location = new System.Drawing.Point(168, 20);
      this.comboBoxSpeakerConfig.Name = "comboBoxSpeakerConfig";
      this.comboBoxSpeakerConfig.Size = new System.Drawing.Size(288, 21);
      this.comboBoxSpeakerConfig.TabIndex = 12;
      // 
      // checkBoxMPEGOverSPDIF
      // 
      this.checkBoxMPEGOverSPDIF.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxMPEGOverSPDIF.Location = new System.Drawing.Point(16, 104);
      this.checkBoxMPEGOverSPDIF.Name = "checkBoxMPEGOverSPDIF";
      this.checkBoxMPEGOverSPDIF.Size = new System.Drawing.Size(152, 16);
      this.checkBoxMPEGOverSPDIF.TabIndex = 21;
      this.checkBoxMPEGOverSPDIF.Text = "MPEG Audio over S/PDIF";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 136);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(136, 16);
      this.label5.TabIndex = 20;
      this.label5.Text = "S/PDIF audio time offset:";
      // 
      // checkBoxSPDIF
      // 
      this.checkBoxSPDIF.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxSPDIF.Location = new System.Drawing.Point(16, 56);
      this.checkBoxSPDIF.Name = "checkBoxSPDIF";
      this.checkBoxSPDIF.Size = new System.Drawing.Size(144, 16);
      this.checkBoxSPDIF.TabIndex = 13;
      this.checkBoxSPDIF.Text = "Use S/PDIF for AC3/DTS";
      // 
      // checkBoxDynamicRange
      // 
      this.checkBoxDynamicRange.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxDynamicRange.Location = new System.Drawing.Point(16, 80);
      this.checkBoxDynamicRange.Name = "checkBoxDynamicRange";
      this.checkBoxDynamicRange.Size = new System.Drawing.Size(136, 16);
      this.checkBoxDynamicRange.TabIndex = 11;
      this.checkBoxDynamicRange.Text = "Dynamic Range Control";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 16);
      this.label3.TabIndex = 8;
      this.label3.Text = "Speaker config:";
      // 
      // DScalerAudioFilter
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "DScalerAudioFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		public override void LoadSettings()
		{
			RegistryKey hkcu = Registry.CurrentUser;
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\DScaler5\Mpeg Audio Filter");
			if (subkey!=null)
			{
				try
				{
					Int32 regValue=(Int32)subkey.GetValue("Dynamic Range Control");
					if (regValue==1) checkBoxDynamicRange.Checked=true;
					else checkBoxDynamicRange.Checked=false;

					regValue=(Int32)subkey.GetValue("MPEG Audio over SPDIF");
					if (regValue==1) checkBoxMPEGOverSPDIF.Checked=true;
					else checkBoxMPEGOverSPDIF.Checked=false;

					regValue=(Int32)subkey.GetValue("Use SPDIF for AC3 & DTS");
					if (regValue==1) checkBoxSPDIF.Checked=true;
					else checkBoxSPDIF.Checked=false;
					
					regValue=(Int32)subkey.GetValue("SPDIF Audio Time Offset");
					textBoxAudioOffset.Text=regValue.ToString();

					regValue=(Int32)subkey.GetValue("Speaker Config");
					comboBoxSpeakerConfig.SelectedIndex=regValue;

				}
				catch(Exception )
				{
				}
				finally
				{
					subkey.Close();
				}
			}
		}

		public override void SaveSettings()
		{
			RegistryKey hkcu = Registry.CurrentUser;
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\DScaler5\Mpeg Audio Filter");
			if (subkey!=null)
			{
				Int32 regValue;
				if (checkBoxDynamicRange.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("Dynamic Range Control",regValue);

				
				if (checkBoxMPEGOverSPDIF.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("MPEG Audio over SPDIF",regValue);

				if (checkBoxSPDIF.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("Use SPDIF for AC3 & DTS",regValue);

				regValue=Int32.Parse(textBoxAudioOffset.Text);
				subkey.SetValue("SPDIF Audio Time Offset",regValue);

				regValue=comboBoxSpeakerConfig.SelectedIndex;
				subkey.SetValue("Speaker Config",regValue);

				subkey.Close();
			}
		}


	}
}

