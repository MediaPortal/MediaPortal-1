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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Video.Database;

namespace MediaPortal.Configuration.Sections
{
	/// <summary>
	/// Summary description for DlgMovieList.
	/// </summary>
	public class DlgMovieList : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private MediaPortal.UserInterface.Controls.MPButton button1;
		private MediaPortal.UserInterface.Controls.MPButton button2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelFileName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxTitle;
		private MediaPortal.UserInterface.Controls.MPButton buttonFind;
		IMDB _imdb;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DlgMovieList()
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

		public int SelectedItem
		{
			get
			{
				if (listView1.SelectedIndices.Count<=0) return 0;
				return listView1.SelectedIndices[0];
			}
		}
		public string Filename
		{
			set
			{
				labelFileName.Text=value;
			}
		}
		public void AddMovie(string movie)
		{
			listView1.Items.Add(movie);
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.label1 = new System.Windows.Forms.Label();
      this.labelFileName = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxTitle = new System.Windows.Forms.TextBox();
      this.buttonFind = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                this.columnHeader1});
      this.listView1.Location = new System.Drawing.Point(8, 56);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(360, 200);
      this.listView1.TabIndex = 0;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 350;
      // 
      // button1
      // 
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(264, 320);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(40, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Ok";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button2.Location = new System.Drawing.Point(312, 320);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(56, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Cancel";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "IMDB resutls for:";
      // 
      // labelFileName
      // 
      this.labelFileName.Location = new System.Drawing.Point(24, 32);
      this.labelFileName.Name = "labelFileName";
      this.labelFileName.Size = new System.Drawing.Size(344, 16);
      this.labelFileName.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 272);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(48, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Title:";
      // 
      // textBoxTitle
      // 
      this.textBoxTitle.Location = new System.Drawing.Point(72, 264);
      this.textBoxTitle.Name = "textBoxTitle";
      this.textBoxTitle.Size = new System.Drawing.Size(192, 20);
      this.textBoxTitle.TabIndex = 6;
      this.textBoxTitle.Text = "";
      // 
      // buttonFind
      // 
      this.buttonFind.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonFind.Location = new System.Drawing.Point(280, 264);
      this.buttonFind.Name = "buttonFind";
      this.buttonFind.Size = new System.Drawing.Size(40, 23);
      this.buttonFind.TabIndex = 7;
      this.buttonFind.Text = "Find";
      this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
      // 
      // DlgMovieList
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(384, 350);
      this.Controls.Add(this.buttonFind);
      this.Controls.Add(this.textBoxTitle);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.labelFileName);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.listView1);
      this.Name = "DlgMovieList";
      this.Text = "IMDB Movie results";
      this.ResumeLayout(false);

    }
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.DialogResult=DialogResult.OK;
			this.Close();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.DialogResult=DialogResult.Cancel;
			this.Close();
		}

		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			base.OnDoubleClick(e);
			this.DialogResult=DialogResult.OK;
			this.Close();

		}

		private void buttonFind_Click(object sender, System.EventArgs e)
		{
			buttonFind.Enabled=false;
			button1.Enabled=false;
			button2.Enabled=false;
			textBoxTitle.Enabled=false;
			imdb.Find( textBoxTitle.Text);
			listView1.Items.Clear();
			for (int i=0; i < imdb.Count;++i)
				AddMovie(imdb[i].Title);

			buttonFind.Enabled=true;
			button1.Enabled=true;
			button2.Enabled=true;
			textBoxTitle.Enabled=true;

		}
		public IMDB imdb
		{
			get { return _imdb;}
			set { _imdb=value;}
		}
	}
}
