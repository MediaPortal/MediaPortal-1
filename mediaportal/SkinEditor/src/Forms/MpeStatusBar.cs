using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Crownwood.Magic.Menus;

namespace Mpe.Forms
{
	/// <summary>
	/// Summary description for MpeStatusBar.
	/// </summary>
	public class MpeStatusBar : StatusBar {
	
		#region Variables
		private ProgressBar progressBar;
		private ImageList iconImageList;
		private MpeLogLevel currentLevel;
		
		private System.Windows.Forms.StatusBarPanel iconPanel;
		private System.Windows.Forms.StatusBarPanel textPanel;
		private System.Windows.Forms.StatusBarPanel progressPanel;

		private Pen panelBorderPen;
		private Brush panelFontBrush;
		private System.Windows.Forms.StatusBarPanel locationPanel;
		private System.Windows.Forms.StatusBarPanel sizePanel;
		private Brush progressBrush;

		private PopupMenu menu;
		#endregion

		#region Constructor
		public MpeStatusBar(ImageList icons) : base() {
			iconImageList = icons;
			currentLevel = MpeLogLevel.Debug;

			InitializeComponent();
			progressBar = new ProgressBar();
			
			panelFontBrush = new SolidBrush(Color.Black);
			progressBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Highlight));
			panelBorderPen = new Pen(Color.FromKnownColor(KnownColor.ControlDark),-1.0f);			
			
			menu = new PopupMenu();
			menu.MenuCommands.Add(new MenuCommand("Debug",iconImageList,0));
			menu.MenuCommands.Add(new MenuCommand("Info",iconImageList,1));
			menu.MenuCommands.Add(new MenuCommand("Warn",iconImageList,2));
			menu.MenuCommands.Add(new MenuCommand("Error",iconImageList,3));

		}
		#endregion

		#region Methods
		public override void Refresh() {
			currentLevel = MpeLog.Threshold;
			base.Refresh();
		}

		private Image GetIconImage(MpeLogLevel level) {
			return iconImageList.Images[(int)level];
		}
		private int GetProgressWidth(int width) {
			if (width == 0 || progressBar.Maximum == 0)
				return 0;
			int w = (int)((double)width * ((double)progressBar.Value / (double)progressBar.Maximum));
			return w;
		}
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
				m += "[" + e.GetType().ToString() + "] ";
				if (e.Message != null && e.Message.Length > 0)
					m = e.Message;
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
				progressPanel.Text = progress.ToString();
				progressPanel.Width = 120;
			} else {
				progressPanel.Text = "";
				progressPanel.Width = 1;
			}
		}
		public void LocationStatus(Point p) {
			locationPanel.Text = "Location: (" + p.X + ", " + p.Y + ")";
		}
		public void LocationStatus(int x, int y) {
			locationPanel.Text = "Location: (" + x + ", " + y + ")";
		}
		public void LocationStatus() {
			locationPanel.Text = "";
		}
		public void SizeStatus(Size s) {
			sizePanel.Text = "Size: (" + s.Width + ", " + s.Height +")";
		}
		public void SizeStatus(int w, int h) {
			sizePanel.Text = "Size: (" + w + ", " + h +")";
		}
		public void SizeStatus() {
			sizePanel.Text = "";
		}
		#endregion

		#region Event Handlers
		private void OnDrawItem(object sender, System.Windows.Forms.StatusBarDrawItemEventArgs e) {
			if (e.Panel == iconPanel) {
				e.Graphics.DrawImage(GetIconImage(currentLevel),e.Bounds.Left + 2,e.Bounds.Top,16,16);
			} else if (e.Panel == progressPanel && progressPanel.Width > 1) {
				e.Graphics.DrawRectangle(panelBorderPen,e.Bounds.Left,e.Bounds.Top,e.Bounds.Width-1,e.Bounds.Height-1);
				e.Graphics.FillRectangle(progressBrush,e.Bounds.Left+3,e.Bounds.Top+3,GetProgressWidth(e.Bounds.Width)-6,e.Bounds.Height-6);
			} else if (e.Panel == locationPanel && locationPanel.Text.Length > 0) {
				e.Graphics.DrawRectangle(panelBorderPen,e.Bounds.Left,e.Bounds.Top,e.Bounds.Width-1,e.Bounds.Height-1);
				e.Graphics.DrawString(locationPanel.Text, Font, panelFontBrush, e.Bounds.Left+2,e.Bounds.Top+2);
			} else if (e.Panel == sizePanel && sizePanel.Text.Length > 0) {
				e.Graphics.DrawRectangle(panelBorderPen,e.Bounds.Left,e.Bounds.Top,e.Bounds.Width-1,e.Bounds.Height-1);
				e.Graphics.DrawString(sizePanel.Text, Font, panelFontBrush, e.Bounds.Left+2,e.Bounds.Top+2);
			}
		}
		private void OnPanelClick(object sender, System.Windows.Forms.StatusBarPanelClickEventArgs e) {
			if (e.StatusBarPanel == iconPanel && sender == this) {
				MenuCommand c = menu.TrackPopup(PointToScreen(new Point(0,-90)),false);
				if (c != null) {
					switch (c.Text) {
						case "Debug":
							MpeLog.Threshold = MpeLogLevel.Debug;
							MpeLog.Debug("Log Level set to Debug");
							break;
						case "Info":
							MpeLog.Threshold = MpeLogLevel.Info;
							MpeLog.Info("Log Level set to Info");
							break;
						case "Warn":
							MpeLog.Threshold = MpeLogLevel.Warn;
							MpeLog.Warn("Log Level set to Warn");
							break;
						case "Error":
							MpeLog.Threshold = MpeLogLevel.Error;
							MpeLog.Error("Log Level set to Error");
							break;
					}
				}
			}
		}
		#endregion

		#region Generated Code
		private void InitializeComponent() {
			this.iconPanel = new System.Windows.Forms.StatusBarPanel();
			this.textPanel = new System.Windows.Forms.StatusBarPanel();
			this.progressPanel = new System.Windows.Forms.StatusBarPanel();
			this.locationPanel = new System.Windows.Forms.StatusBarPanel();
			this.sizePanel = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.iconPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.progressPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.locationPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sizePanel)).BeginInit();
			// 
			// iconPanel
			// 
			this.iconPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.iconPanel.MinWidth = 20;
			this.iconPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.iconPanel.Width = 20;
			// 
			// textPanel
			// 
			this.textPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.textPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.textPanel.Text = "Ready";
			this.textPanel.Width = 10;
			// 
			// progressPanel
			// 
			this.progressPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.progressPanel.MinWidth = 0;
			this.progressPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.progressPanel.Width = 120;
			// 
			// locationPanel
			// 
			this.locationPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.locationPanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.locationPanel.MinWidth = 1;
			this.locationPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.locationPanel.Width = 10;
			// 
			// sizePanel
			// 
			this.sizePanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.sizePanel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
			this.sizePanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
			this.sizePanel.Width = 10;
			// 
			// MpeStatusBar
			// 
			this.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																									this.iconPanel,
																									this.textPanel,
																									this.locationPanel,
																									this.sizePanel,
																									this.progressPanel});
			this.ShowPanels = true;
			this.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this.OnPanelClick);
			this.DrawItem += new System.Windows.Forms.StatusBarDrawItemEventHandler(this.OnDrawItem);
			((System.ComponentModel.ISupportInitialize)(this.iconPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.progressPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.locationPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sizePanel)).EndInit();

		}
		#endregion
		
	}
}
