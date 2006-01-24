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

/*
 * Created by SharpDevelop.
 * User: Josh
 * Date: 6/19/2004
 * Time: 11:05 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;

namespace GUIRSSFeed
{	
	/// <summary>
	/// Details form for entering a site for My News Plugin
	/// </summary>
	public class DetailsForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.TextBox textImage;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.TextBox textDescription;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.TextBox textURL;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		public int ID;
		//private SetupForm form;
		public bool isNew;
		public DetailsForm(SetupForm parent, int ID)
		{
			this.ID = ID;
			//this.form = form;
			isNew = false;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			buttonClear.Click+=new EventHandler(textClear);
			buttonSave.Click+=new EventHandler(saveInfo);
			buttonBrowse.Click+=new EventHandler(browseFile);
			
			if (ID > -1)
			{
				LoadSettings();	
				isNew = false;
			}
			else //find the next ID that is blank
			{
				string tempText;
				for (int i=0; i<20; i++)
				{
					using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
      				{
						tempText = xmlreader.GetValueAsString("rss","siteName"+i,"");
      					if (tempText == "")
      					{
      						this.ID = i;
      						i=20;
      						isNew = true;
      					}
       				}	
				}
				if (this.ID == -1)
					Console.WriteLine("No more open slots!");
				//TODO: Need message box popup here if no more slots left
			}

			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void saveInfo(object obj,EventArgs ea) 
		{
			SaveSettings();			
		}
		
		private void browseFile(object obj, System.EventArgs e)
		{
			OpenFileDialog dlg=new OpenFileDialog();
	      	dlg.CheckFileExists=true;
	      	dlg.CheckPathExists=true;
	      	dlg.RestoreDirectory=true;
	      	dlg.Filter= "image files (*.png)|*.png";
	      	dlg.FilterIndex=0;
	      	dlg.Title="Select Site Icon";
	      	dlg.ShowDialog();
	      	if (dlg.FileName!="")
	      	{
	      	  	textImage.Text=dlg.FileName;
	      	}			
		}
		
		void textClear(object obj,EventArgs ea) 
		{
			textName.Text = "";
			textURL.Text = "";
			textDescription.Text = "";
		}
		
		void LoadSettings()
		{
			using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
      		{
      		
				textName.Text = xmlreader.GetValueAsString("rss","siteName"+ID,"");
        		textURL.Text = xmlreader.GetValueAsString("rss","siteURL"+ID,"");
    	   		textDescription.Text = xmlreader.GetValueAsString("rss","siteDescription"+ID, "");
      			textImage.Text = xmlreader.GetValueAsString("rss","siteImage"+ID, "");
       		}
		}
		
		void SaveSettings()
		{			
      		using(MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings("MediaPortal.xml"))
      		{
        		xmlwriter.SetValue("rss","siteName"+this.ID,textName.Text);
        		xmlwriter.SetValue("rss","siteURL"+this.ID,textURL.Text);      
        		xmlwriter.SetValue("rss","siteDescription"+this.ID,textDescription.Text);
      			xmlwriter.SetValue("rss","siteImage"+this.ID,textImage.Text);
      		}
      		this.Close();
		}
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.textURL = new System.Windows.Forms.TextBox();
      this.buttonSave = new System.Windows.Forms.Button();
      this.textDescription = new System.Windows.Forms.TextBox();
      this.buttonClear = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.textImage = new System.Windows.Forms.TextBox();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.label = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.textName = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // textURL
      // 
      this.textURL.Location = new System.Drawing.Point(112, 88);
      this.textURL.Name = "textURL";
      this.textURL.Size = new System.Drawing.Size(352, 20);
      this.textURL.TabIndex = 4;
      this.textURL.Text = "";
      // 
      // buttonSave
      // 
      this.buttonSave.Location = new System.Drawing.Point(112, 224);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.TabIndex = 6;
      this.buttonSave.Text = "Save";
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // textDescription
      // 
      this.textDescription.Location = new System.Drawing.Point(112, 136);
      this.textDescription.Name = "textDescription";
      this.textDescription.Size = new System.Drawing.Size(192, 20);
      this.textDescription.TabIndex = 5;
      this.textDescription.Text = "";
      // 
      // buttonClear
      // 
      this.buttonClear.Location = new System.Drawing.Point(224, 224);
      this.buttonClear.Name = "buttonClear";
      this.buttonClear.TabIndex = 7;
      this.buttonClear.Text = "Clear";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(32, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 23);
      this.label2.TabIndex = 1;
      this.label2.Text = "RSS URL";
      // 
      // textImage
      // 
      this.textImage.Location = new System.Drawing.Point(112, 184);
      this.textImage.Name = "textImage";
      this.textImage.Size = new System.Drawing.Size(192, 20);
      this.textImage.TabIndex = 9;
      this.textImage.Text = "";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Location = new System.Drawing.Point(312, 184);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.TabIndex = 10;
      this.buttonBrowse.Text = "Browse";
      // 
      // label
      // 
      this.label.Location = new System.Drawing.Point(32, 40);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(72, 23);
      this.label.TabIndex = 0;
      this.label.Text = "Site Name";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(32, 184);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 23);
      this.label1.TabIndex = 8;
      this.label1.Text = "Image";
      // 
      // textName
      // 
      this.textName.Location = new System.Drawing.Point(112, 40);
      this.textName.Name = "textName";
      this.textName.Size = new System.Drawing.Size(192, 20);
      this.textName.TabIndex = 3;
      this.textName.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(32, 136);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(72, 23);
      this.label3.TabIndex = 2;
      this.label3.Text = "Description";
      // 
      // DetailsForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(488, 270);
      this.Controls.Add(this.buttonBrowse);
      this.Controls.Add(this.textImage);
      this.Controls.Add(this.textDescription);
      this.Controls.Add(this.textURL);
      this.Controls.Add(this.textName);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.buttonClear);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label);
      this.Name = "DetailsForm";
      this.Text = "DetailsForm";
      this.ResumeLayout(false);

    }
		#endregion

    private void buttonSave_Click(object sender, System.EventArgs e)
    {
    
    }
	}
}
