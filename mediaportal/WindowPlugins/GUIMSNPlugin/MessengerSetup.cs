/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.GUI.MSN
{
	/// <summary>
	/// Summary description for MessengerSetup.
	/// </summary>
	public class MessengerSetup : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxEMail;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.CheckBox chkSignIn;
		private System.Windows.Forms.ComboBox InitialStatusBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox chkPopupWindow;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MessengerSetup()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxEMail = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.chkSignIn = new System.Windows.Forms.CheckBox();
			this.InitialStatusBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.chkPopupWindow = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "E-mail adres:";
			// 
			// textBoxEMail
			// 
			this.textBoxEMail.Location = new System.Drawing.Point(24, 40);
			this.textBoxEMail.Name = "textBoxEMail";
			this.textBoxEMail.Size = new System.Drawing.Size(232, 20);
			this.textBoxEMail.TabIndex = 1;
			this.textBoxEMail.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Password:";
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Location = new System.Drawing.Point(24, 92);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(232, 20);
			this.textBoxPassword.TabIndex = 2;
			this.textBoxPassword.Text = "";
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(208, 232);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "Ok";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// chkSignIn
			// 
			this.chkSignIn.Location = new System.Drawing.Point(24, 176);
			this.chkSignIn.Name = "chkSignIn";
			this.chkSignIn.Size = new System.Drawing.Size(168, 24);
			this.chkSignIn.TabIndex = 3;
			this.chkSignIn.Text = "Auto signin";
			// 
			// InitialStatusBox
			// 
			this.InitialStatusBox.Items.AddRange(new object[] {
																													"Online",
																													"Away",
																													"Busy",
																													"Be Right Back",
																													"Hidden"});
			this.InitialStatusBox.Location = new System.Drawing.Point(24, 144);
			this.InitialStatusBox.Name = "InitialStatusBox";
			this.InitialStatusBox.Size = new System.Drawing.Size(232, 21);
			this.InitialStatusBox.TabIndex = 6;
			this.InitialStatusBox.Text = "Online";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "Initial status:";
			// 
			// chkPopupWindow
			// 
			this.chkPopupWindow.Location = new System.Drawing.Point(24, 200);
			this.chkPopupWindow.Name = "chkPopupWindow";
			this.chkPopupWindow.Size = new System.Drawing.Size(256, 24);
			this.chkPopupWindow.TabIndex = 8;
			this.chkPopupWindow.Text = "Popup chat window while watching TV/Video";
			// 
			// MessengerSetup
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.Add(this.chkPopupWindow);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.InitialStatusBox);
			this.Controls.Add(this.chkSignIn);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxPassword);
			this.Controls.Add(this.textBoxEMail);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "MessengerSetup";
			this.Text = "MessengerSetup";
			this.Load += new System.EventHandler(this.MessengerSetup_Load);
			this.ResumeLayout(false);

		}
		#endregion

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("MSNmessenger","email",textBoxEMail.Text);
        xmlWriter.SetValue("MSNmessenger","password",textBoxPassword.Text);
				xmlWriter.SetValue("MSNmessenger","initialstatus",InitialStatusBox.Text);

        int iSignIn=0;
        if (chkSignIn.Checked) iSignIn=1;
        xmlWriter.SetValue("MSNmessenger","autosignin",iSignIn.ToString());

				int iPopup=0;
				if (chkPopupWindow.Checked) iPopup=1;
				xmlWriter.SetValue("MSNmessenger","popupwindow",iPopup.ToString());

      }
      this.Close();
    }

    private void MessengerSetup_Load(object sender, System.EventArgs e)
    {
      
      using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        textBoxEMail.Text = xmlreader.GetValueAsString("MSNmessenger","email","");
        textBoxPassword.Text = xmlreader.GetValueAsString("MSNmessenger","password","");
				InitialStatusBox.Text = xmlreader.GetValueAsString("MSNmessenger","initialstatus","Online");

				chkSignIn.Checked = false;
        if (xmlreader.GetValueAsInt("MSNmessenger", "autosignin", 0) == 1) chkSignIn.Checked = true;
				chkPopupWindow.Checked = false;
				if (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1) chkPopupWindow.Checked = true;
      }
    }

    private void label3_Click(object sender, System.EventArgs e)
    {
    
    }
	}
}
