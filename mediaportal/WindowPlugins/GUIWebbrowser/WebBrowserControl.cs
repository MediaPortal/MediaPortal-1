using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.WebBrowser
{
	

	/// <summary>
	/// Summary description for WebBrowserControl.
	/// </summary>
	public sealed class WebBrowserControl : System.Windows.Forms.UserControl
	{
        static readonly WebBrowserControl instance;

        /// <summary>
		/// Enumeration for refresh constants
		/// </summary>
		public enum RefreshConstants 
		{
			REFRESH_NORMAL = 0,
			REFRESH_IFEXPIRED = 1,
			REFRESH_CONTINUE = 2,
			REFRESH_COMPLETELY = 3
		}

		private AxMOZILLACONTROLLib.AxMozillaBrowser axMozillaBrowser1;
		private const int TOP = 55;
		private const int LEFT = 4;
		private const int HEIGHT = 493;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        static WebBrowserControl()
        {
            try
            {
                instance = new WebBrowserControl();
            }
            catch
            { }
        }

		private WebBrowserControl()
		{
            try
            {
                // This call is required by the Windows.Forms Form Designer.
                InitializeComponent();
                this.Top = 55;
                this.Left = 4;
            }
            catch
            { }
		}

		public static WebBrowserControl Instance
		{
			get
			{
                return instance;
			}
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            try
            {
                System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WebBrowserControl));
                this.axMozillaBrowser1 = new AxMOZILLACONTROLLib.AxMozillaBrowser();
                ((System.ComponentModel.ISupportInitialize)(this.axMozillaBrowser1)).BeginInit();
                this.SuspendLayout();
                // 
                // axMozillaBrowser1
                // 
                this.axMozillaBrowser1.Enabled = true;
                this.axMozillaBrowser1.Location = new System.Drawing.Point(0, 0);
                this.axMozillaBrowser1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMozillaBrowser1.OcxState")));
                this.axMozillaBrowser1.Size = new System.Drawing.Size(720, 473);
                this.axMozillaBrowser1.TabIndex = 0;
                // 
                // WebBrowserControl
                // 
                this.Controls.Add(this.axMozillaBrowser1);
                this.Name = "WebBrowserControl";
                this.Size = new System.Drawing.Size(720, 473);
                this.Layout += new System.Windows.Forms.LayoutEventHandler(this.WebBrowserControl_Layout);
                ((System.ComponentModel.ISupportInitialize)(this.axMozillaBrowser1)).EndInit();
                this.ResumeLayout(false);
                this.KeyPress += new KeyPressEventHandler(WebBrowserControl_KeyPress);
            }
            catch
            {
            }

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the Mozilla web browser control
		/// </summary>
		public AxMOZILLACONTROLLib.AxMozillaBrowser Browser
		{
			get{ return axMozillaBrowser1;}
		}

		#endregion

		/// <summary>
		/// Rescales & Reszies the browser and control when Media Portal changes its size
		/// </summary>
		private void ResizeBrowser()
		{
			//rescale & resize control
			int left = LEFT;
			int top = TOP;
			int right = this.Right;
			int bottom = this.Bottom;

			GUIGraphicsContext.ScaleRectToScreenResolution(ref left,ref top,ref right,ref bottom);
			this.Left = left;
			this.Top= top;
			ScaleVertical();
			ScaleHorizontal();
		}
	
		/// <summary>
		/// Scale y position for current resolution
		/// </summary>
		private void ScaleVertical()
		{
			float fSkinHeight=(float)GUIGraphicsContext.SkinSize.Height;;
			float fPercentY = ((float)GUIGraphicsContext.Height) / fSkinHeight;
			this.Height  = (int)Math.Round (((float)HEIGHT) * fPercentY); 
			this.Browser.Height = this.Height;
		}

		/// <summary>
		/// Scale y position for current resolution
		/// </summary>
		private void ScaleHorizontal()
		{
			this.Width = GUIGraphicsContext.Width - 4;
			this.Browser.Width = this.Width;
		}

		/// <summary>
		/// Layout Event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WebBrowserControl_Layout(object sender, LayoutEventArgs e)
		{
			ResizeBrowser();
		}

		private void WebBrowserControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			Console.WriteLine("test");
		}
	}
}
