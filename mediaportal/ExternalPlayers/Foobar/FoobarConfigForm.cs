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

using System.Text;

using System.IO;



namespace MediaPortal.FoobarPlugin

{

	/// <summary>

	/// Summary description for FoobarConfigForm.

	/// </summary>

	public class FoobarConfigForm : System.Windows.Forms.Form

	{

    private System.Windows.Forms.Label label1;

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.Label label3;

    private System.Windows.Forms.Label label5;

    private System.Windows.Forms.Button browseButton;

    private System.Windows.Forms.TextBox extensionsTextBox;

    private System.Windows.Forms.TextBox portTextBox;

    private System.Windows.Forms.TextBox hotnameTextBox;

    private System.Windows.Forms.TextBox foobarLocationTextBox;

    private System.Windows.Forms.LinkLabel linkLabel1;

		/// <summary>

		/// Required designer variable.

		/// </summary>

		private System.ComponentModel.Container components = null;



		public FoobarConfigForm()

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FoobarConfigForm));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.foobarLocationTextBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.hotnameTextBox = new System.Windows.Forms.TextBox();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.extensionsTextBox = new System.Windows.Forms.TextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Foobar Location";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 48);
			this.label2.Name = "label2";
			this.label2.TabIndex = 1;
			this.label2.Text = "Hostname";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 80);
			this.label3.Name = "label3";
			this.label3.TabIndex = 2;
			this.label3.Text = "Port Number";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 112);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 24);
			this.label5.TabIndex = 4;
			this.label5.Text = "Extensions";
			// 
			// foobarLocationTextBox
			// 
			this.foobarLocationTextBox.Enabled = false;
			this.foobarLocationTextBox.Location = new System.Drawing.Point(128, 16);
			this.foobarLocationTextBox.Name = "foobarLocationTextBox";
			this.foobarLocationTextBox.Size = new System.Drawing.Size(232, 20);
			this.foobarLocationTextBox.TabIndex = 5;
			this.foobarLocationTextBox.Text = "";
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(362, 14);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 23);
			this.browseButton.TabIndex = 1;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// hotnameTextBox
			// 
			this.hotnameTextBox.Location = new System.Drawing.Point(128, 48);
			this.hotnameTextBox.Name = "hotnameTextBox";
			this.hotnameTextBox.Size = new System.Drawing.Size(232, 20);
			this.hotnameTextBox.TabIndex = 2;
			this.hotnameTextBox.Text = "localhost";
			// 
			// portTextBox
			// 
			this.portTextBox.Location = new System.Drawing.Point(128, 80);
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(232, 20);
			this.portTextBox.TabIndex = 3;
			this.portTextBox.Text = "8989";
			// 
			// extensionsTextBox
			// 
			this.extensionsTextBox.Location = new System.Drawing.Point(128, 112);
			this.extensionsTextBox.Name = "extensionsTextBox";
			this.extensionsTextBox.Size = new System.Drawing.Size(232, 20);
			this.extensionsTextBox.TabIndex = 4;
			this.extensionsTextBox.Text = ".cda,.mp3,.mid";
			// 
			// linkLabel1
			// 
			this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(33, 19);
			this.linkLabel1.Location = new System.Drawing.Point(16, 152);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(376, 32);
			this.linkLabel1.TabIndex = 5;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "NOTE:  Remember to install/setup foo_httpserver_ctrl (version B1) in your foobar/" +
				"components directory.";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// FoobarConfigForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 189);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.extensionsTextBox);
			this.Controls.Add(this.portTextBox);
			this.Controls.Add(this.hotnameTextBox);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.foobarLocationTextBox);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FoobarConfigForm";
			this.Text = "FoobarConfigForm";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FoobarConfigForm_Closing);
			this.Load += new System.EventHandler(this.FoobarConfigForm_Load);
			this.ResumeLayout(false);

		}

		#endregion



    /// <summary>

    /// This method is called whenever the browse button is click

    /// </summary>

    /// <param name="sender">the sender instance</param>

    /// <param name="e">the event.  In this case click!</param>

    private void browseButton_Click(object sender, System.EventArgs e)

    {

      string curDir = Directory.GetCurrentDirectory();

      // The filter for the dialog window is foobar2000.exe

      OpenFileDialog dlg = new OpenFileDialog();

      dlg.AddExtension = true;

      dlg.Filter = "Foobar2000 (Foobar2000.exe)|Foobar2000.exe|All files (*.*)|*.*" ;

      // start in media folder

      //dlg.InitialDirectory = @"C:\";    

      // open dialog

      if(dlg.ShowDialog(this) == DialogResult.OK)

      {

        foobarLocationTextBox.Text = dlg.FileName;

      }

      Directory.SetCurrentDirectory(curDir);

    }



    /// <summary>

    /// When this form loads, read the configuration file for the variables that this

    /// form sets up

    /// </summary>

    /// <param name="sender">the sender instance</param>

    /// <param name="e">the event.  Form load!</param>

    private void FoobarConfigForm_Load(object sender, System.EventArgs e)

    {

      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))

      {

        extensionsTextBox.Text = xmlreader.GetValueAsString("foobarplugin", "enabledextensions",".cda,.mp3,.mid");

        portTextBox.Text = xmlreader.GetValueAsString("foobarplugin", "port","8989");

        hotnameTextBox.Text = xmlreader.GetValueAsString("foobarplugin", "host","localhost");

        foobarLocationTextBox.Text = xmlreader.GetValueAsString("foobarplugin", "path","");

      }



    }



    /// <summary>

    /// When this form closes, write the variables from the form to the configuration file

    /// </summary>

    /// <param name="sender">the sender instance</param>

    /// <param name="e">the event.  Closing!</param>

    private void FoobarConfigForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)

    {

      using (MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("foobarplugin", "port", portTextBox.Text);
        xmlWriter.SetValue("foobarplugin", "host", hotnameTextBox.Text);
        xmlWriter.SetValue("foobarplugin", "path", foobarLocationTextBox.Text);

        // make sure all the extensions starts with "."  If not, add it in...
        string[] exts = extensionsTextBox.Text.Split(new char[]{','});
        StringBuilder buff = new StringBuilder();
        foreach(string ext in exts)
        {
          if(buff.Length != 0)
            buff.Append(',');
          if(!ext.StartsWith("."))
            buff.Append('.');
          buff.Append(ext);
        }

        xmlWriter.SetValue("foobarplugin", "enabledextensions", buff.ToString());
        //xmlWriter.Save();
      }

    }



    /// <summary>

    /// The link will open the link on a browser to get the foobar plugin from the source

    /// </summary>

    /// <param name="sender"></param>

    /// <param name="e"></param>

    private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)

    {

      // Determine which link was clicked within the LinkLabel.

      this.linkLabel1.Links[linkLabel1.Links.IndexOf(e.Link)].Visited = true;

      try

      {

        Help.ShowHelp(this, "http://sourceforge.net/projects/foohttpserver");

      }

      catch

      {

      }



    }

	}

}

