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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using UtilityLibrary.WinControls;

namespace SkinEditor.Forms
{
	/// <summary>
	/// Summary description for MpeStatusBar.
	/// </summary>
	public class MpeStatusBar : System.Windows.Forms.UserControl {
	
		#region Variables
		private System.Windows.Forms.StatusBar statusBar;
		private ProgressBarEx progressBar;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.PictureBox debugIcon;
		private System.Windows.Forms.PictureBox infoIcon;
		private System.Windows.Forms.PictureBox warnIcon;
		private System.Windows.Forms.PictureBox errorIcon;
		private PictureBox[] icons;
		private System.Windows.Forms.StatusBarPanel textPanel;
		private System.Windows.Forms.StatusBarPanel locationPanel;
		private MpeLogLevel currentLevel;
		private Brush panelFontBrush;
		private System.Windows.Forms.StatusBarPanel iconPanel;
		private Pen panelBorderPen;
		#endregion

		#region Constructor
		public MpeStatusBar() {
			// This call is required by the Windows.Forms Form Designer.
			
			InitializeComponent();
			progressBar = new ProgressBarEx();
			progressBar.Smooth = true;
			progressBar.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top)));
			progressBar.Size = new Size(150,16);
			progressBar.Location = new Point(statusBar.Width - 165,2);
			progressBar.Enable3DBorder = true;
			progressBar.Border3D = Border3DStyle.Flat;
			progressBar.BorderColor = Color.FromKnownColor(KnownColor.ControlDark);
			statusBar.Controls.Add(progressBar);
			currentLevel = MpeLogLevel.Debug;
			debugIcon.Hide();
			infoIcon.Hide();
			warnIcon.Hide();
			errorIcon.Hide();
			panelFontBrush = new SolidBrush(Color.Black);
			panelBorderPen = new Pen(Color.FromKnownColor(KnownColor.ControlDark),-1.0f);
			locationPanel.Text = "";
		}
		#endregion

		#region Methods
		public void Message(MpeLogLevel level, string message, int progress) {
			if (currentLevel != level) {
            currentLevel = level;
				iconPanel.Text = currentLevel.ToString();
			}
			if (textPanel.Text.Equals(message) == false) {
				textPanel.Text = message;
			}
			if (progress != progressBar.Value)
				Progress(progress);
		}
		public void Message(MpeLogLevel level, string message) {
			if (currentLevel != level) {
				currentLevel = level;
				iconPanel.Text = currentLevel.ToString();
			}
			if (textPanel.Text.Equals(message) == false) {
				textPanel.Text = message;
			}
		}
		public void Message(MpeLogLevel level, Exception e) {
			string m = "Unknown Error";
			if (e != null) {
				if (e.Source != null && e.Source.Length > 0)
					m += " [" + e.Source + "]";
				if (e.Message != null && e.Message.Length > 0)
					m = e.Message;
				if (level == MpeLogLevel.Debug) {
					m += Environment.NewLine;
					m += e.StackTrace;
				}
			}
			Message(level,m);
		}
		public void Clear() {
			Message(MpeLogLevel.Debug,"",progressBar.Minimum);
		}
		public void Debug(string message, int progress) {
			Message(MpeLogLevel.Debug,message,progress);
		}
		public void Debug(string message) {
			Message(MpeLogLevel.Debug,message,int.MinValue);
		}
		public void Debug(Exception exception) {
			Message(MpeLogLevel.Debug,exception);
		}
		public void Info(string message, int progress) {
			Message(MpeLogLevel.Info,message,progress);
		}
		public void Info(string message) {
			Message(MpeLogLevel.Info,message,int.MinValue);
		}
		public void Info(Exception exception) {
			Message(MpeLogLevel.Info,exception);
		}
		public void Warn(string message, int progress) {
			Message(MpeLogLevel.Warn,message,progress);
		}
		public void Warn(string message) {
			Message(MpeLogLevel.Warn,message,int.MinValue);
		}
		public void Warn(Exception exception) {
			Message(MpeLogLevel.Warn,exception);
		}
		public void Error(string message, int progress) {
			Message(MpeLogLevel.Error,message,progress);
		}
		public void Error(string message) {
			Message(MpeLogLevel.Error,message,int.MinValue);
		}
		public void Error(Exception exception) {
			Message(MpeLogLevel.Error,exception);
		}
		public void Progress(int min, int max, int progress) {
			progressBar.Minimum = min;
			progressBar.Maximum = max;
			Progress(progress);
		}
		public void Progress(int min, int max) {
			progressBar.Minimum = min;
			progressBar.Maximum = max;
			Progress(min);
		}
		public void Progress(int progress) {
			if (progress > progressBar.Minimum && progress <= progressBar.Maximum) {
				progressBar.Value = progress;
				progressBar.Show();
			} else {
				progressBar.Hide();
			}
		}
		#endregion

		#region Event Handlers
		private void OnDrawItem(object sender, System.Windows.Forms.StatusBarDrawItemEventArgs e) {
			if (e.Panel == iconPanel) {
				if (currentLevel == MpeLogLevel.Debug)
					e.Graphics.DrawImage(debugIcon.Image,e.Bounds.Left,e.Bounds.Top,16,16);
				else if (currentLevel == MpeLogLevel.Info)
					e.Graphics.DrawImage(infoIcon.Image,e.Bounds.Left,e.Bounds.Top,16,16);
			}
		}
		#endregion

		#region Component Designer Generated Code
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpeStatusBar));
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.textPanel = new System.Windows.Forms.StatusBarPanel();
			this.locationPanel = new System.Windows.Forms.StatusBarPanel();
			this.debugIcon = new System.Windows.Forms.PictureBox();
			this.infoIcon = new System.Windows.Forms.PictureBox();
			this.warnIcon = new System.Windows.Forms.PictureBox();
			this.errorIcon = new System.Windows.Forms.PictureBox();
			this.iconPanel = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.textPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.locationPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// statusBar
			// 
			this.statusBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusBar.Location = new System.Drawing.Point(0, 0);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																												 this.iconPanel,
																												 this.textPanel,
																												 this.locationPanel});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(512, 21);
			this.statusBar.TabIndex = 0;
			this.statusBar.DrawItem += new System.Windows.Forms.StatusBarDrawItemEventHandler(this.OnDrawItem);
			// 
			// textPanel
			// 
			this.textPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.textPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.textPanel.Width = 276;
			// 
			// locationPanel
			// 
			this.locationPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.locationPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.locationPanel.Width = 200;
			// 
			// debugIcon
			// 
			this.debugIcon.Image = ((System.Drawing.Image)(resources.GetObject("debugIcon.Image")));
			this.debugIcon.Location = new System.Drawing.Point(0, 0);
			this.debugIcon.Name = "debugIcon";
			this.debugIcon.Size = new System.Drawing.Size(20, 20);
			this.debugIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.debugIcon.TabIndex = 1;
			this.debugIcon.TabStop = false;
			// 
			// infoIcon
			// 
			this.infoIcon.Image = ((System.Drawing.Image)(resources.GetObject("infoIcon.Image")));
			this.infoIcon.Location = new System.Drawing.Point(0, 0);
			this.infoIcon.Name = "infoIcon";
			this.infoIcon.Size = new System.Drawing.Size(20, 20);
			this.infoIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.infoIcon.TabIndex = 2;
			this.infoIcon.TabStop = false;
			// 
			// warnIcon
			// 
			this.warnIcon.Image = ((System.Drawing.Image)(resources.GetObject("warnIcon.Image")));
			this.warnIcon.Location = new System.Drawing.Point(0, 0);
			this.warnIcon.Name = "warnIcon";
			this.warnIcon.Size = new System.Drawing.Size(20, 20);
			this.warnIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.warnIcon.TabIndex = 3;
			this.warnIcon.TabStop = false;
			// 
			// errorIcon
			// 
			this.errorIcon.Image = ((System.Drawing.Image)(resources.GetObject("errorIcon.Image")));
			this.errorIcon.Location = new System.Drawing.Point(0, 0);
			this.errorIcon.Name = "errorIcon";
			this.errorIcon.Size = new System.Drawing.Size(20, 20);
			this.errorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.errorIcon.TabIndex = 4;
			this.errorIcon.TabStop = false;
			// 
			// iconPanel
			// 
			this.iconPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.iconPanel.MinWidth = 20;
			this.iconPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.iconPanel.Width = 20;
			// 
			// MpeStatusBar
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.errorIcon);
			this.Controls.Add(this.warnIcon);
			this.Controls.Add(this.infoIcon);
			this.Controls.Add(this.debugIcon);
			this.Controls.Add(this.statusBar);
			this.Name = "MpeStatusBar";
			this.Size = new System.Drawing.Size(512, 21);
			((System.ComponentModel.ISupportInitialize)(this.textPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.locationPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconPanel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	
	}
}
