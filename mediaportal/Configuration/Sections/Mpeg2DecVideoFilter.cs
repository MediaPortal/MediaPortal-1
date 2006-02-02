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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

using System.Runtime.InteropServices;

using DShowNET;
using DirectShowLib;

namespace MediaPortal.Configuration.Sections
{

	public class MPEG2DecVideoFilter : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		//private System.Windows.Forms.Label label1;
		//private System.Windows.Forms.Label label3;
		//private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbDeinterlace;
		private MediaPortal.UserInterface.Controls.MPCheckBox cbForcedSubtitles;
		private MediaPortal.UserInterface.Controls.MPCheckBox cbPlanar;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TrackBar tbBrightness;
		private System.Windows.Forms.TrackBar tbContrast;
		private System.Windows.Forms.TrackBar tbHue;
		private System.Windows.Forms.TrackBar tbSaturation;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecVideoFilter() : this("MPEG2Dec Video Decoder")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecVideoFilter(string name) : base(name)
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
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.cbDeinterlace = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbForcedSubtitles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbPlanar = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tbSaturation = new System.Windows.Forms.TrackBar();
      this.tbHue = new System.Windows.Forms.TrackBar();
      this.tbContrast = new System.Windows.Forms.TrackBar();
      this.tbBrightness = new System.Windows.Forms.TrackBar();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbHue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbDeinterlace);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbForcedSubtitles);
      this.groupBox1.Controls.Add(this.cbPlanar);
      this.groupBox1.Controls.Add(this.tbSaturation);
      this.groupBox1.Controls.Add(this.tbHue);
      this.groupBox1.Controls.Add(this.tbContrast);
      this.groupBox1.Controls.Add(this.tbBrightness);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 272);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 200);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 16);
      this.label8.TabIndex = 8;
      this.label8.Text = "Saturation:";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 160);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(32, 16);
      this.label7.TabIndex = 6;
      this.label7.Text = "Hue:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 120);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(56, 16);
      this.label6.TabIndex = 4;
      this.label6.Text = "Contrast:";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 80);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 16);
      this.label4.TabIndex = 2;
      this.label4.Text = "Brightness:";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[] {
                                                       "Auto",
                                                       "Weave",
                                                       "Blend",
                                                       "BOB"});
      this.cbDeinterlace.Location = new System.Drawing.Point(168, 236);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
      this.cbDeinterlace.TabIndex = 11;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 240);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(112, 16);
      this.label2.TabIndex = 10;
      this.label2.Text = "Deinterlace method:";
      // 
      // cbForcedSubtitles
      // 
      this.cbForcedSubtitles.Checked = true;
      this.cbForcedSubtitles.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbForcedSubtitles.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbForcedSubtitles.Location = new System.Drawing.Point(16, 48);
      this.cbForcedSubtitles.Name = "cbForcedSubtitles";
      this.cbForcedSubtitles.Size = new System.Drawing.Size(168, 16);
      this.cbForcedSubtitles.TabIndex = 1;
      this.cbForcedSubtitles.Text = "Always display forced subtitles";
      // 
      // cbPlanar
      // 
      this.cbPlanar.Checked = true;
      this.cbPlanar.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbPlanar.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbPlanar.Location = new System.Drawing.Point(16, 24);
      this.cbPlanar.Name = "cbPlanar";
      this.cbPlanar.Size = new System.Drawing.Size(264, 16);
      this.cbPlanar.TabIndex = 0;
      this.cbPlanar.Text = "Enable planar YUV media types (YV12, I420, IYUV)";
      // 
      // tbSaturation
      // 
      this.tbSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbSaturation.Location = new System.Drawing.Point(160, 196);
      this.tbSaturation.Maximum = 200;
      this.tbSaturation.Name = "tbSaturation";
      this.tbSaturation.Size = new System.Drawing.Size(304, 45);
      this.tbSaturation.TabIndex = 9;
      this.tbSaturation.TickFrequency = 20;
      this.tbSaturation.Value = 100;
      // 
      // tbHue
      // 
      this.tbHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbHue.Location = new System.Drawing.Point(160, 156);
      this.tbHue.Maximum = 360;
      this.tbHue.Name = "tbHue";
      this.tbHue.Size = new System.Drawing.Size(304, 45);
      this.tbHue.TabIndex = 7;
      this.tbHue.TickFrequency = 32;
      this.tbHue.Value = 180;
      // 
      // tbContrast
      // 
      this.tbContrast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbContrast.Location = new System.Drawing.Point(160, 116);
      this.tbContrast.Maximum = 200;
      this.tbContrast.Name = "tbContrast";
      this.tbContrast.Size = new System.Drawing.Size(304, 45);
      this.tbContrast.TabIndex = 5;
      this.tbContrast.TickFrequency = 20;
      this.tbContrast.Value = 100;
      // 
      // tbBrightness
      // 
      this.tbBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBrightness.Location = new System.Drawing.Point(160, 76);
      this.tbBrightness.Maximum = 255;
      this.tbBrightness.Name = "tbBrightness";
      this.tbBrightness.Size = new System.Drawing.Size(304, 45);
      this.tbBrightness.TabIndex = 3;
      this.tbBrightness.TickFrequency = 16;
      this.tbBrightness.Value = 128;
      // 
      // MPEG2DecVideoFilter
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "MPEG2DecVideoFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbHue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

		public override void LoadSettings()
		{
			RegistryKey hkcu = Registry.CurrentUser;
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\Mediaportal\Mpeg Video Filter");
			if (subkey!=null)
			{
				try
				{
					Int32 regValue=(Int32)subkey.GetValue("Enable Planar YUV Modes");
					if (regValue==1) cbPlanar.Checked=true;
					else cbPlanar.Checked=false;

					regValue=(Int32)subkey.GetValue("Forced Subtitles");
					if (regValue==1) cbForcedSubtitles.Checked=true;
					else cbForcedSubtitles.Checked=false;
					
					
					regValue=(Int32)subkey.GetValue("Deinterlace");
					cbDeinterlace.SelectedIndex=regValue;

					regValue=(Int32)subkey.GetValue("Brightness");
					tbBrightness.Value=regValue;
					
					regValue=(Int32)subkey.GetValue("Contrast");
					tbContrast.Value=regValue;

					regValue=(Int32)subkey.GetValue("Hue");
					tbHue.Value=regValue;

					regValue=(Int32)subkey.GetValue("Saturation");
					tbSaturation.Value=regValue;
				}
				catch (Exception)
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
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\Mediaportal\Mpeg Video Filter");
			if (subkey!=null)
			{
				Int32 regValue;
				if (cbPlanar.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("Enable Planar YUV Modes",regValue);

				if (cbForcedSubtitles.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("Forced Subtitles",regValue);

				subkey.SetValue("Deinterlace",(Int32)cbDeinterlace.SelectedIndex);

				subkey.SetValue("Brightness",(Int32)tbBrightness.Value);
				subkey.SetValue("Contrast",(Int32)tbContrast.Value);
				subkey.SetValue("Hue",(Int32)tbHue.Value);
				subkey.SetValue("Saturation",(Int32)tbSaturation.Value);

				subkey.Close();
			}
		}



	}
}

