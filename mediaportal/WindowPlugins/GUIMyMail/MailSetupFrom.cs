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
using System.Net;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MyMail
{
	/// <summary>
	/// Zusammenfassung für MailSetupFrom.
	/// </summary>
	public class MailSetupFrom : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button2;
		ArrayList m_mailBox=new ArrayList();
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.PropertyGrid grid;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label5;

		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MailSetupFrom()
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
			this.button1 = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.grid = new System.Windows.Forms.PropertyGrid();
			this.button4 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(272, 208);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 24);
			this.button1.TabIndex = 0;
			this.button1.Text = "Add Mailbox";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// listBox1
			// 
			this.listBox1.Location = new System.Drawing.Point(272, 48);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(184, 147);
			this.listBox1.TabIndex = 1;
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(360, 208);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(96, 24);
			this.button2.TabIndex = 2;
			this.button2.Text = "Delete selected";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(408, 320);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(80, 24);
			this.button3.TabIndex = 11;
			this.button3.Text = "Done";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// grid
			// 
			this.grid.CommandsVisibleIfAvailable = true;
			this.grid.LargeButtons = false;
			this.grid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.grid.Location = new System.Drawing.Point(16, 48);
			this.grid.Name = "grid";
			this.grid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
			this.grid.Size = new System.Drawing.Size(240, 184);
			this.grid.TabIndex = 12;
			this.grid.Text = "Mailbox Parameter";
			this.grid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.grid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(360, 248);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(96, 24);
			this.button4.TabIndex = 13;
			this.button4.Text = "Save Settings";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textBox2);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.textBox1);
			this.groupBox1.Controls.Add(this.grid);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.button4);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.button2);
			this.groupBox1.Controls.Add(this.listBox1);
			this.groupBox1.Location = new System.Drawing.Point(16, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(472, 304);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = " Mailbox Config ";
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 268);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(104, 16);
			this.label5.TabIndex = 19;
			this.label5.Text = "Re-type Password:";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(120, 265);
			this.textBox2.Name = "textBox2";
			this.textBox2.PasswordChar = '*';
			this.textBox2.Size = new System.Drawing.Size(136, 20);
			this.textBox2.TabIndex = 18;
			this.textBox2.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 243);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 16);
			this.label4.TabIndex = 17;
			this.label4.Text = "Mailbox Password:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(120, 240);
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '*';
			this.textBox1.Size = new System.Drawing.Size(136, 20);
			this.textBox1.TabIndex = 16;
			this.textBox1.Text = "";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(176, 16);
			this.label3.TabIndex = 15;
			this.label3.Text = "Mailbox Properties:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(272, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 16);
			this.label2.TabIndex = 14;
			this.label2.Text = "Current Mailboxlist:";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(264, 320);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(40, 20);
			this.numericUpDown1.TabIndex = 15;
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 0});
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 324);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 16);
			this.label1.TabIndex = 16;
			this.label1.Text = "Time-Interval in minutes to Auto-Check for Mail:";
			// 
			// MailSetupFrom
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(498, 352);
			this.ControlBox = false;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.button3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MailSetupFrom";
			this.Text = "Mailbox Configuration";
			this.Load += new System.EventHandler(this.MailSetupFrom_Load);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void MailSetupFrom_Load(object sender, System.EventArgs e)
		{
			LoadSettings();
			grid.PropertyValueChanged+=new PropertyValueChangedEventHandler(grid_PropertyValueChanged);
			textBox1.LostFocus+=new EventHandler(SetMBPassword);
			textBox2.LostFocus+=new EventHandler(SetMBPassword);
		}

		void RefreshListBox()
		{
				listBox1.Items.Clear();
				foreach(MailBox mb in m_mailBox)
				{
					listBox1.Items.Add(mb);
				}
			
		}


		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(listBox1.SelectedIndex!=-1)
				if(listBox1.SelectedIndex<m_mailBox.Count)
				{
					MailBox mb=(MailBox)m_mailBox[listBox1.SelectedIndex];
					grid.SelectedObject=mb;
					textBox1.Text=mb.Password;
					textBox2.Text=mb.Password;
				}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if(listBox1.SelectedIndex!=-1)
				if(m_mailBox[listBox1.SelectedIndex]!=null)
				{
					m_mailBox.RemoveAt(listBox1.SelectedIndex);
					SaveConfigFile();
					RefreshListBox();
					listBox1.SelectedIndex=-1;
					grid.SelectedObject=null;
				}		

		}

		bool SaveConfigFile()
		{
			int count=0;
			foreach(MailBox mb in m_mailBox)
			{
				string tmpLabel=mb.BoxLabel;
				for(int i=0;i<m_mailBox.Count-1;i++)
					if(tmpLabel.ToLower()==((MailBox)m_mailBox[i]).BoxLabel.ToLower() && i!=count)
					{
						MessageBox.Show("There are indentical Mail-Box Labels. Please change!");
						return false;
					}
				count++;
			}

      string applicationPath=Application.ExecutablePath;
      applicationPath=System.IO.Path.GetFullPath(applicationPath);
      applicationPath=System.IO.Path.GetDirectoryName(applicationPath);

			if(textBox1.Text.Equals(textBox2.Text))	
				using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					MailBox tmpBox;
					int boxCount=m_mailBox.Count;
					xmlwriter.SetValue("mymail","mailBoxCount",boxCount);
					for(int i=0;i<boxCount;i++)
					{
						tmpBox=(MailBox)m_mailBox[i];
						textBox1.Text=tmpBox.Password;
						textBox2.Text=tmpBox.Password;
						// check the must set properties
						if(tmpBox.MailboxFolder=="") // this must set
							tmpBox.MailboxFolder=tmpBox.BoxLabel+"__Folder";
						if(tmpBox.AttachmentFolder=="") // this must set
							tmpBox.AttachmentFolder=tmpBox.MailboxFolder+@"\Attachments";
            
            // check full pathnames
            if (!System.IO.Path.IsPathRooted(tmpBox.AttachmentFolder)) 
              tmpBox.AttachmentFolder= applicationPath+@"\email\"+tmpBox.AttachmentFolder;
            if (!System.IO.Path.IsPathRooted(tmpBox.MailboxFolder)) 
              tmpBox.MailboxFolder = applicationPath+@"\email\"+tmpBox.MailboxFolder;

						if(tmpBox.BoxLabel=="")
						{
							MessageBox.Show("The BoxLabel property cant be empty!");
							return false;
						}
						//
						string mailBoxString=tmpBox.BoxLabel+";"+tmpBox.Username+";"+tmpBox.Password+";"+tmpBox.ServerAddress+";"+Convert.ToString(tmpBox.Port)+";"+tmpBox.MailboxFolder+";"+tmpBox.AttachmentFolder;
						
						xmlwriter.SetValue("mymail","mailBox"+Convert.ToString(i),mailBoxString);
					}
					xmlwriter.SetValue("mymail","timer",numericUpDown1.Value*60000);
					return true;
				}
			else
			{
				MessageBox.Show("The Password given dont match. Please try again");
				return false;
			}
		}
		bool ServerExists(MailBox mb)
		{
			try
			{
				IPHostEntry hostIP = Dns.Resolve(mb.ServerAddress); 
				IPAddress[] addr = hostIP.AddressList;
			}
			catch
			{
			return false;
			}
			return true;
		}
		private void button1_Click(object sender, System.EventArgs e)
		{
			MailBox mailbox=new MailBox("New MailBox","","","",110,@"MyMailFiles"+Convert.ToString(m_mailBox.Count+1),@"MailBoxAttachments");
			m_mailBox.Add(mailbox);
			SaveConfigFile();
			RefreshListBox();
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			if(SaveConfigFile()==true)
				this.Close();
		}

		void grid_PropertyValueChanged(object sender,System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			MailBox mb;
			switch(e.ChangedItem.Label)
			{
				case "AttachmentFolder":
					mb=(MailBox)e.ChangedItem.Parent.Value;
					break;
				case "MailboxFolder":
					mb=(MailBox)e.ChangedItem.Parent.Value;
					break;
				case "ServerAddress":
					mb=(MailBox)e.ChangedItem.Parent.Value;
					if(ServerExists(mb)==false)
					{
						MessageBox.Show("The Host '"+e.ChangedItem.Value.ToString()+"' can not be resolved."+((Char)13)+"The previous entry will be restored.");
						mb.ServerAddress=e.OldValue.ToString();
						grid.SelectedObject=mb;
					}
					break;
				case "Username":
					mb=(MailBox)e.ChangedItem.Parent.Value;
					if(mb.Username.IndexOf(";")!=-1)
						mb.Username=e.OldValue.ToString();
					break;
				case "BoxLabel":
					mb=(MailBox)e.ChangedItem.Parent.Value;
					if(mb.BoxLabel.IndexOf(";")!=-1)
						mb.BoxLabel=e.OldValue.ToString();
					break;
			}

		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			SaveConfigFile();
			RefreshListBox();
		}

		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				int boxCount=0;
				MailBox tmpBox;
				m_mailBox.Clear();
				numericUpDown1.Value=xmlreader.GetValueAsInt("mymail","timer",300000)/60000;
				boxCount=xmlreader.GetValueAsInt("mymail","mailBoxCount",0);
				
				if(boxCount>0)
				{
					for(int i=0;i<boxCount;i++)
					{
						string[] boxData=null;
						string mailBoxString=xmlreader.GetValueAsString("mymail","mailBox"+Convert.ToString(i),"");
						if(mailBoxString.Length>0)
						{
							
							boxData=mailBoxString.Split(new char[]{';'});
							if(boxData.Length==7)
							{
								tmpBox=new MailBox(boxData[0],boxData[1],boxData[2],boxData[3],Convert.ToInt16(boxData[4]),boxData[5],boxData[6]);
								if(tmpBox!=null)
									m_mailBox.Add(tmpBox);
							}
						}
					}
					if(m_mailBox.Count>0)
						RefreshListBox();
				}

			}
		}

		private void groupBox1_Enter(object sender, System.EventArgs e)
		{
		
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
		}
		
		private void SetMBPassword(object sender,System.EventArgs e)
		{
			MailBox mb=(MailBox)grid.SelectedObject;
			if(textBox1.Text.Equals(textBox2.Text))
				mb.Password=textBox1.Text;
			else
			{
				if(textBox2.Text.Length>0)
				{
					MessageBox.Show("The Password given dont match. Please try again");
          textBox1.Text="";
          textBox2.Text="";
          textBox1.Focus();
				}
				else
				textBox2.Focus();
				
			}
		}


	}
}
