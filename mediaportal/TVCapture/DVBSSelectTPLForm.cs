using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for DVBSSelectTPLForm.
	/// </summary>
	public class DVBSSelectTPLForm : System.Windows.Forms.Form
	{
		class Transponder
		{
			public string SatName;
			public string FileName;
			public override string ToString()
			{
				return SatName;
			}
		}

		private System.Windows.Forms.ComboBox cbTransponder4;
		private System.Windows.Forms.ComboBox cbTransponder3;
		private System.Windows.Forms.ComboBox cbTransponder2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbTransponder;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button button1;
		private int numberOfLNBs=1;
		private string[] TPLFiles = new string[5];
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DVBSSelectTPLForm()
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
			this.cbTransponder4 = new System.Windows.Forms.ComboBox();
			this.cbTransponder3 = new System.Windows.Forms.ComboBox();
			this.cbTransponder2 = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cbTransponder = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cbTransponder4
			// 
			this.cbTransponder4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTransponder4.Location = new System.Drawing.Point(160, 112);
			this.cbTransponder4.Name = "cbTransponder4";
			this.cbTransponder4.Size = new System.Drawing.Size(192, 21);
			this.cbTransponder4.TabIndex = 28;
			// 
			// cbTransponder3
			// 
			this.cbTransponder3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTransponder3.Location = new System.Drawing.Point(160, 88);
			this.cbTransponder3.Name = "cbTransponder3";
			this.cbTransponder3.Size = new System.Drawing.Size(192, 21);
			this.cbTransponder3.TabIndex = 27;
			// 
			// cbTransponder2
			// 
			this.cbTransponder2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTransponder2.Location = new System.Drawing.Point(160, 64);
			this.cbTransponder2.Name = "cbTransponder2";
			this.cbTransponder2.Size = new System.Drawing.Size(192, 21);
			this.cbTransponder2.TabIndex = 26;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 112);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 16);
			this.label3.TabIndex = 25;
			this.label3.Text = "Transponder for LNB4:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 16);
			this.label2.TabIndex = 24;
			this.label2.Text = "Transponder for LNB3:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 23;
			this.label1.Text = "Transponder for LNB2:";
			// 
			// cbTransponder
			// 
			this.cbTransponder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTransponder.Location = new System.Drawing.Point(160, 40);
			this.cbTransponder.Name = "cbTransponder";
			this.cbTransponder.Size = new System.Drawing.Size(192, 21);
			this.cbTransponder.TabIndex = 22;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(32, 40);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(120, 16);
			this.label7.TabIndex = 21;
			this.label7.Text = "Transponder for LNB1:";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(360, 152);
			this.button1.Name = "button1";
			this.button1.TabIndex = 29;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// DVBSSelectTPLForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 206);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.cbTransponder4);
			this.Controls.Add(this.cbTransponder3);
			this.Controls.Add(this.cbTransponder2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbTransponder);
			this.Controls.Add(this.label7);
			this.Name = "DVBSSelectTPLForm";
			this.Text = "Select transponder for each LNB";
			this.Load += new System.EventHandler(this.DVBSSelectTPLForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			Transponder ts = cbTransponder.SelectedItem as Transponder;
			if (ts!=null)
				TPLFiles[0]=ts.FileName;

			ts = cbTransponder2.SelectedItem as Transponder;
			if (ts!=null)
				TPLFiles[1]=ts.FileName;

			ts = cbTransponder3.SelectedItem as Transponder;
			if (ts!=null)
				TPLFiles[2]=ts.FileName;

			ts = cbTransponder4.SelectedItem as Transponder;
			if (ts!=null)
				TPLFiles[3]=ts.FileName;
			this.Close();
		}

		private void DVBSSelectTPLForm_Load(object sender, System.EventArgs e)
		{
			TPLFiles[0]=String.Empty;
			TPLFiles[1]=String.Empty;
			TPLFiles[2]=String.Empty;
			TPLFiles[3]=String.Empty;
			cbTransponder.Items.Clear();
			cbTransponder2.Items.Clear();
			cbTransponder3.Items.Clear();
			cbTransponder4.Items.Clear();
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters");
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".tpl") >=0)
				{
					Transponder ts = LoadTransponder(file);
					cbTransponder.Items.Add(ts);
					cbTransponder2.Items.Add(ts);
					cbTransponder3.Items.Add(ts);
					cbTransponder4.Items.Add(ts);
				}
			}
			if (cbTransponder.Items.Count>0)
				cbTransponder.SelectedIndex=0;
			if (cbTransponder2.Items.Count>0)
				cbTransponder2.SelectedIndex=0;
			if (cbTransponder3.Items.Count>0)
				cbTransponder3.SelectedIndex=0;
			if (cbTransponder4.Items.Count>0)
				cbTransponder4.SelectedIndex=0;

			if (NumberOfLNBs==2)
				cbTransponder2.Enabled=false;
			if (NumberOfLNBs==3)
				cbTransponder3.Enabled=false;
			if (NumberOfLNBs==4)			
				cbTransponder4.Enabled=false;
		}
	
		public int NumberOfLNBs
		{
			get { return numberOfLNBs;}
			set { numberOfLNBs=value;}
		}

		Transponder LoadTransponder(string file)
		{
			System.IO.TextReader tin = System.IO.File.OpenText(file);
			Transponder ts = new Transponder();
			ts.FileName=file;
			string line=null;
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				{
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						int pos=line.IndexOf("satname=");
						if (pos>=0)
						{
							ts.SatName=line.Substring(pos+"satname=".Length);
							tin.Close();
							return ts;
						}
					}
				}
			} while (!(line == null));
			tin.Close();
			return null;
		}


		public string[] TransponderFiles
		{
			get 
			{
				return TPLFiles;
			}
		}
	}
}
