#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Threading;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for SplashScreen.
	/// </summary>
	public class SplashScreen : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label versionLabel;
    private System.Windows.Forms.Label informationLabel;
    private System.Windows.Forms.PictureBox pictureBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SplashScreen()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
      SetInformation("Loading...");
      SetVersion(System.Windows.Forms.Application.ProductVersion);
		}

    public void SetInformation(string information)
    {
      informationLabel.Text = information;
      Update();
    }

    public void SetVersion(string version)
    {
      versionLabel.Text = version;
      Update();
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

	  //JoeDalton: cross-thread updating of controls is not supported in .NET
    //A control can only be (safely) changed from the thread it is created.
    //Sometimes it works, sometimes it crashes.  
	  //When starting from VS.NET, an exception will occur whenever you try to
	  //update a control that was created on another thread.
	  //So we need to change the way the splash screen is created/handled.
	  //See Control.InvokeRequired property and Control.Invoke method for more information.
	  //I tried to fix the fadein method using the methods described in the documentation, 
	  //but then it isn't show at all, because the main thread (= the thread that created the
	  //splash screen) is too busy starting MediaPortal.  Whenever the thread gets some time
	  //the splash screen is already closed, and then these methods crash because the splash 
	  //screen is already disposed.
    public void FadeIn()
    {
    //  Thread fadeInThread = new Thread(new ThreadStart(FadeInThread));
    //  fadeInThread.Start();
    //}

    //void FadeInThread()
    //{
      while (Opacity <= 0.9)
      {
        Opacity += 0.02;
        Thread.Sleep(10);
      }
    }

    public void FadeOut()
    {
      while (Opacity >= 0.02)
      {
        Opacity -= 0.08;
        Thread.Sleep(10);
      }
      this.Hide();
    }

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.panel1 = new System.Windows.Forms.Panel();
      this.informationLabel = new System.Windows.Forms.Label();
      this.versionLabel = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.White;
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Controls.Add(this.informationLabel);
      this.panel1.Controls.Add(this.versionLabel);
      this.panel1.Controls.Add(this.pictureBox1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(390, 126);
      this.panel1.TabIndex = 0;
      // 
      // informationLabel
      // 
      this.informationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.informationLabel.BackColor = System.Drawing.Color.Transparent;
      this.informationLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.informationLabel.Location = new System.Drawing.Point(15, 99);
      this.informationLabel.Name = "informationLabel";
      this.informationLabel.Size = new System.Drawing.Size(358, 16);
      this.informationLabel.TabIndex = 4;
      this.informationLabel.Text = "Information";
      this.informationLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // versionLabel
      // 
      this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.versionLabel.BackColor = System.Drawing.Color.Transparent;
      this.versionLabel.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.versionLabel.Location = new System.Drawing.Point(280, 79);
      this.versionLabel.Name = "versionLabel";
      this.versionLabel.Size = new System.Drawing.Size(85, 16);
      this.versionLabel.TabIndex = 5;
      this.versionLabel.Text = "Version";
      this.versionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
      this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.pictureBox1.Image = global::MediaPortal.Properties.Resources.mplogo_new;
      this.pictureBox1.Location = new System.Drawing.Point(25, 14);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(338, 65);
      this.pictureBox1.TabIndex = 3;
      this.pictureBox1.TabStop = false;
      // 
      // SplashScreen
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(390, 126);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.Name = "SplashScreen";
      this.Opacity = 0;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "SplashScreen";
      this.TopMost = true;
      this.panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

		}
		#endregion
	}
}
