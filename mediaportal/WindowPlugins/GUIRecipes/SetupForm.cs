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
using MediaPortal.GUI.Library;

namespace GUIRecipes
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button cbOk;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textFileCat;
		private System.Windows.Forms.Label lblNumber;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			LoadSettings();
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
			this.txtFile = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.cbOk = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textFileCat = new System.Windows.Forms.TextBox();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.lblNumber = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mealmaster file to Import: ";
			// 
			// txtFile
			// 
			this.txtFile.Location = new System.Drawing.Point(160, 16);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(200, 20);
			this.txtFile.TabIndex = 1;
			this.txtFile.Text = "";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(472, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(504, 16);
			this.button2.Name = "button2";
			this.button2.TabIndex = 4;
			this.button2.Text = "Import";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// cbOk
			// 
			this.cbOk.Location = new System.Drawing.Point(504, 200);
			this.cbOk.Name = "cbOk";
			this.cbOk.Size = new System.Drawing.Size(72, 24);
			this.cbOk.TabIndex = 5;
			this.cbOk.Text = "Ok";
			this.cbOk.Click += new System.EventHandler(this.cbOk_Click);
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(160, 48);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(88, 16);
			this.checkBox1.TabIndex = 6;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "Use SubCategories";
			// 
			// textFileCat
			// 
			this.textFileCat.Enabled = false;
			this.textFileCat.Location = new System.Drawing.Point(160, 72);
			this.textFileCat.Name = "textFileCat";
			this.textFileCat.Size = new System.Drawing.Size(200, 20);
			this.textFileCat.TabIndex = 8;
			this.textFileCat.Text = "";
			// 
			// button3
			// 
			this.button3.Enabled = false;
			this.button3.Location = new System.Drawing.Point(472, 72);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(24, 23);
			this.button3.TabIndex = 9;
			this.button3.Text = "...";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Enabled = false;
			this.button4.Location = new System.Drawing.Point(504, 72);
			this.button4.Name = "button4";
			this.button4.TabIndex = 10;
			this.button4.Text = "Import";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// label3
			// 
			this.label3.Enabled = false;
			this.label3.Location = new System.Drawing.Point(8, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 16);
			this.label3.TabIndex = 11;
			this.label3.Text = "Categorie file to Import: ";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 112);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(144, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "Use Online Recipe Update";
			// 
			// checkBox2
			// 
			this.checkBox2.Location = new System.Drawing.Point(160, 112);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(96, 16);
			this.checkBox2.TabIndex = 12;
			this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
			// 
			// textBox2
			// 
			this.textBox2.Enabled = false;
			this.textBox2.Location = new System.Drawing.Point(160, 144);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(144, 20);
			this.textBox2.TabIndex = 15;
			this.textBox2.Text = "";
			// 
			// label5
			// 
			this.label5.Enabled = false;
			this.label5.Location = new System.Drawing.Point(8, 144);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 16);
			this.label5.TabIndex = 14;
			this.label5.Text = "Online User Name:";
			// 
			// textBox3
			// 
			this.textBox3.Enabled = false;
			this.textBox3.Location = new System.Drawing.Point(160, 176);
			this.textBox3.Name = "textBox3";
			this.textBox3.PasswordChar = '*';
			this.textBox3.Size = new System.Drawing.Size(144, 20);
			this.textBox3.TabIndex = 17;
			this.textBox3.Text = "";
			// 
			// label6
			// 
			this.label6.Enabled = false;
			this.label6.Location = new System.Drawing.Point(8, 176);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(136, 16);
			this.label6.TabIndex = 16;
			this.label6.Text = "Online Passport: ";
			// 
			// lblNumber
			// 
			this.lblNumber.Location = new System.Drawing.Point(8, 208);
			this.lblNumber.Name = "lblNumber";
			this.lblNumber.Size = new System.Drawing.Size(456, 16);
			this.lblNumber.TabIndex = 18;
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(600, 238);
			this.Controls.Add(this.lblNumber);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textFileCat);
			this.Controls.Add(this.txtFile);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.checkBox2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.cbOk);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.ResumeLayout(false);

		}
		#endregion
	
		private void cbOk_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			this.Visible = false;
		}

		#region Private Methods
		/// <summary>
		/// Saves my status settings to the profile xml.
		/// </summary>
		
		private void SaveSettings() {
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml")) {
				xmlwriter.SetValueAsBool("recipe","subcats",checkBox1.Checked); 
				xmlwriter.SetValueAsBool("recipe","online",checkBox2.Checked); 
			}
		}

		/// <summary>
		/// Loads my status settings from the profile xml.
		/// </summary>
		private void LoadSettings() {
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) {
				checkBox1.Checked = xmlreader.GetValueAsBool("recipe","subcats",false);
				checkBox2.Checked = xmlreader.GetValueAsBool("recipe","online",false);
			}
		}

		#endregion

		private void button1_Click(object sender, System.EventArgs e) {
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog( this ) == DialogResult.OK )
			{
				txtFile.Text = openFileDialog1.FileName;
			}
		}

		private void button2_Click(object sender, System.EventArgs e) {
			if( txtFile.Text.Length < 1 ) {
				MessageBox.Show( "Please select a file to import!" );
				return;
			}
			try {
				RecipeReader rr = new RecipeReader( txtFile.Text );
				rr.GetRecipes();
				lblNumber.Text = rr.RecipeCount + " Recipes read.";
			} catch {
				txtFile.Text="";
			}
		}

		private void button3_Click(object sender, System.EventArgs e) {
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog( this ) == DialogResult.OK ) {
				textFileCat.Text = openFileDialog1.FileName;
			}
		}

		private void button4_Click(object sender, System.EventArgs e) {
			if( textFileCat.Text.Length < 1 ) {
				MessageBox.Show( "Please select a catfile to import!" );
				return;
			}
			try {
				CatReader cc = new CatReader( textFileCat.Text );
				cc.GetCategories();
				lblNumber.Text = cc.CatCount + " Categories read.";
			} catch {
				textFileCat.Text="";
				textFileCat.Enabled=false;
				label3.Enabled=false;
				button3.Enabled=false;
				button4.Enabled=false;
				checkBox1.Checked=false;
			}
		}

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e) {
			if (checkBox1.Checked==false) {
				textFileCat.Enabled=false;
				label3.Enabled=false;
				button3.Enabled=false;
				button4.Enabled=false;
			} else {
				textFileCat.Enabled=true;
				label3.Enabled=true;
				button3.Enabled=true;
				button4.Enabled=true;
			}
		}

		private void checkBox2_CheckedChanged(object sender, System.EventArgs e) {
			if (checkBox2.Checked==false) {
				textBox2.Enabled=false;
				textBox3.Enabled=false;
				label5.Enabled=false;
				label6.Enabled=false;
			} else {
				textBox2.Enabled=true;
				textBox3.Enabled=true;
				label5.Enabled=true;
				label6.Enabled=true;
			}
		}
	}
}
