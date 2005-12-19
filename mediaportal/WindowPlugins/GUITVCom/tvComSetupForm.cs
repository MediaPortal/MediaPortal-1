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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Setup Form for the TV.com Parser
	/// </summary>
	public class tvComSetupForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox lookupIfNoSEinFilename;
		private System.Windows.Forms.CheckBox rename;
		private System.Windows.Forms.CheckBox renameOnlyIf;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox renameFormat;
		private System.Windows.Forms.TextBox replaceSpaces;
		private System.Windows.Forms.Label help;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox titleFormat;
		private System.Windows.Forms.TextBox txtGenre;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox chkActors;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public tvComSetupForm()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

			//
			// TODO: Fügen Sie den Konstruktorcode nach dem Aufruf von InitializeComponent hinzu
			//
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
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

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.replaceSpaces = new System.Windows.Forms.TextBox();
			this.renameFormat = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.renameOnlyIf = new System.Windows.Forms.CheckBox();
			this.rename = new System.Windows.Forms.CheckBox();
			this.lookupIfNoSEinFilename = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.help = new System.Windows.Forms.Label();
			this.titleFormat = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtGenre = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.chkActors = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.replaceSpaces);
			this.groupBox1.Controls.Add(this.renameFormat);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.renameOnlyIf);
			this.groupBox1.Controls.Add(this.rename);
			this.groupBox1.Location = new System.Drawing.Point(24, 152);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(640, 200);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Renaming Options";
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 152);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(384, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Replace Spaces in Filename by this (leave empty to not use this option):";
			// 
			// replaceSpaces
			// 
			this.replaceSpaces.Location = new System.Drawing.Point(48, 176);
			this.replaceSpaces.MaxLength = 1;
			this.replaceSpaces.Name = "replaceSpaces";
			this.replaceSpaces.Size = new System.Drawing.Size(24, 20);
			this.replaceSpaces.TabIndex = 5;
			this.replaceSpaces.Text = "";
			// 
			// renameFormat
			// 
			this.renameFormat.Location = new System.Drawing.Point(48, 112);
			this.renameFormat.Name = "renameFormat";
			this.renameFormat.Size = new System.Drawing.Size(328, 20);
			this.renameFormat.TabIndex = 2;
			this.renameFormat.Text = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(48, 88);
			this.label1.Name = "label1";
			this.label1.TabIndex = 3;
			this.label1.Text = "Rename Pattern:";
			// 
			// renameOnlyIf
			// 
			this.renameOnlyIf.Location = new System.Drawing.Point(40, 48);
			this.renameOnlyIf.Name = "renameOnlyIf";
			this.renameOnlyIf.Size = new System.Drawing.Size(280, 32);
			this.renameOnlyIf.TabIndex = 1;
			this.renameOnlyIf.Text = "...but only if no Season- and Episodenumber was found in the Filename";
			// 
			// rename
			// 
			this.rename.Location = new System.Drawing.Point(16, 24);
			this.rename.Name = "rename";
			this.rename.Size = new System.Drawing.Size(136, 24);
			this.rename.TabIndex = 0;
			this.rename.Text = "Enable Renaming";
			this.rename.CheckedChanged += new System.EventHandler(this.rename_CheckedChanged);
			// 
			// lookupIfNoSEinFilename
			// 
			this.lookupIfNoSEinFilename.Location = new System.Drawing.Point(24, 16);
			this.lookupIfNoSEinFilename.Name = "lookupIfNoSEinFilename";
			this.lookupIfNoSEinFilename.Size = new System.Drawing.Size(288, 48);
			this.lookupIfNoSEinFilename.TabIndex = 1;
			this.lookupIfNoSEinFilename.Text = "Try to search by EpisodeTitle if Season and Episodenumber cannot be found in the " +
				"Filename";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(440, 472);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(560, 472);
			this.button2.Name = "button2";
			this.button2.TabIndex = 3;
			this.button2.Text = "Cancel";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// help
			// 
			this.help.Location = new System.Drawing.Point(24, 368);
			this.help.Name = "help";
			this.help.Size = new System.Drawing.Size(640, 88);
			this.help.TabIndex = 5;
			// 
			// titleFormat
			// 
			this.titleFormat.Location = new System.Drawing.Point(24, 80);
			this.titleFormat.Name = "titleFormat";
			this.titleFormat.Size = new System.Drawing.Size(328, 20);
			this.titleFormat.TabIndex = 6;
			this.titleFormat.Text = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(240, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "Construct the DB Title in the following Format:";
			// 
			// txtGenre
			// 
			this.txtGenre.Location = new System.Drawing.Point(24, 127);
			this.txtGenre.Name = "txtGenre";
			this.txtGenre.Size = new System.Drawing.Size(328, 20);
			this.txtGenre.TabIndex = 8;
			this.txtGenre.Text = "[SHOWNAME] ([GENRE])";
			this.txtGenre.TextChanged += new System.EventHandler(this.txtGenre_TextChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 107);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(496, 16);
			this.label4.TabIndex = 9;
			this.label4.Text = "Fill the Genre Field with the following (Note: this is useful for seperating Movi" +
				"es from TV-Shows):";
			// 
			// chkActors
			// 
			this.chkActors.Location = new System.Drawing.Point(320, 27);
			this.chkActors.Name = "chkActors";
			this.chkActors.Size = new System.Drawing.Size(184, 24);
			this.chkActors.TabIndex = 10;
			this.chkActors.Text = "Download Actor Information";
			// 
			// tvComSetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(688, 509);
			this.Controls.Add(this.chkActors);
			this.Controls.Add(this.txtGenre);
			this.Controls.Add(this.titleFormat);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.help);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.lookupIfNoSEinFilename);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "tvComSetupForm";
			this.Text = "Option for the TV.com Parser";
			this.Load += new System.EventHandler(this.tvComSetupForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void tvComSetupForm_Load(object sender, System.EventArgs e)
		{
			this.help.Text = "Currently the following Placeholders are supported:"+
				"\n[SHOWNAME] - Is replaced by the Name of the Show as retrieved from TV.com"+
				"\n[EPISODETITLE] - Is replaced by the Name of the Episode as retrieved from TV.com"+
				"\n[EPISODENO] - Is replaced by the Episodenumber (always in double digits)"+
				"\n[SEASONNO] - Is replaced by the Seasonnumber of the Episode (single or double digit)"+
				"\n[GENRE] - Is replaced by the Genre(s) of the Show" + 
				"\n[CHANNEL] - Is replaced by the Channel of the Show";
			
			this.titleFormat.Text = TVcomSettings.titleFormat;
			this.lookupIfNoSEinFilename.Checked = TVcomSettings.lookupIfNoSEinFilename;
			this.rename.Checked = TVcomSettings.renameFiles;
			this.chkActors.Checked = TVcomSettings.lookupActors;
			if(TVcomSettings.genreFormat != string.Empty)
				this.txtGenre.Text = TVcomSettings.genreFormat;
			cChanged();
            
		}

		private void groupBox1_Enter(object sender, System.EventArgs e)
		{
			
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			if(
				(renameFormat.Text.IndexOf("[EPISODETITLE]") != -1) ||
				(renameFormat.Text.IndexOf("[EPISODENO]") != -1 && renameFormat.Text.IndexOf("[SEASONNO]") != -1))
			{
				TVcomSettings.lookupIfNoSEinFilename = this.lookupIfNoSEinFilename.Checked;
				TVcomSettings.renameFiles = this.rename.Checked;
				TVcomSettings.renameOnlyIfNoSEinFilename = this.renameOnlyIf.Checked;

				TVcomSettings.renameFormat = this.renameFormat.Text;
				TVcomSettings.titleFormat = this.titleFormat.Text;
				TVcomSettings.lookupActors = this.chkActors.Checked;
				if(this.txtGenre.Text != string.Empty)
					TVcomSettings.genreFormat = this.txtGenre.Text;
				if(this.replaceSpaces.Text.Length < 1)
					TVcomSettings.replaceSpacesWith = ' ';
				else
					TVcomSettings.replaceSpacesWith = this.replaceSpaces.Text[0];

				TVcomSettings.writeSettings();
				this.Close();
			}
			else
			{
				
				MessageBox.Show("This Renameformat is invalid. Please correct it or disable renaming!", "Error");
			}
    
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void rename_CheckedChanged(object sender, System.EventArgs e)
		{
			cChanged();
		}

		private void cChanged()
		{
			if(!this.rename.Checked)
			{
				
				this.renameOnlyIf.Enabled = false;
				this.renameFormat.Enabled = false;
				this.replaceSpaces.Enabled = false;
			}
			else
			{
				this.renameOnlyIf.Checked = TVcomSettings.renameOnlyIfNoSEinFilename;
				this.renameFormat.Text = TVcomSettings.renameFormat;
				this.replaceSpaces.Text = TVcomSettings.replaceSpacesWith.ToString();

				this.renameOnlyIf.Enabled = true;
				this.renameFormat.Enabled = true;
				this.replaceSpaces.Enabled = true;
			}
		}

		private void txtGenre_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
