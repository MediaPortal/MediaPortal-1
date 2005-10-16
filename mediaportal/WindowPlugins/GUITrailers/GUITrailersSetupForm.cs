using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for GUITrailersSetupForm.
	/// </summary>
	public class GUITrailersSetupForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.CheckBox checkBox300;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox GermanTrailerCheckBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GUITrailersSetupForm()
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
			this.checkBox300 = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.GermanTrailerCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// checkBox300
			// 
			this.checkBox300.Location = new System.Drawing.Point(48, 24);
			this.checkBox300.Name = "checkBox300";
			this.checkBox300.Size = new System.Drawing.Size(328, 24);
			this.checkBox300.TabIndex = 1;
			this.checkBox300.Text = "Always start streaming movies with 300kb/s, skip 700kb/s.";
			this.checkBox300.CheckedChanged += new System.EventHandler(this.checkBox300_CheckedChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(344, 200);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 32);
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// GermanTrailerCheckBox
			// 
			this.GermanTrailerCheckBox.Location = new System.Drawing.Point(48, 104);
			this.GermanTrailerCheckBox.Name = "GermanTrailerCheckBox";
			this.GermanTrailerCheckBox.Size = new System.Drawing.Size(144, 24);
			this.GermanTrailerCheckBox.TabIndex = 3;
			this.GermanTrailerCheckBox.Text = "Show german trailers";
			// 
			// GUITrailersSetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 262);
			this.Controls.Add(this.GermanTrailerCheckBox);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox300);
			this.Name = "GUITrailersSetupForm";
			this.Text = "My Trailers Setup";
			this.Load += new System.EventHandler(this.GUITrailersSetupForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void checkBox300_CheckedChanged(object sender, System.EventArgs e)
		{
			
		}

		private void GUITrailersSetupForm_Load(object sender, System.EventArgs e)
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) 
			{
				if(xmlreader.GetValue("mytrailers","speed")=="300")
					checkBox300.Checked=true;
				else checkBox300.Checked=false;
				if(xmlreader.GetValueAsBool("mytrailers","Show german trailers", false)==true)
					GermanTrailerCheckBox.Checked=true;
				else GermanTrailerCheckBox.Checked=false;
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				if(checkBox300.Checked==true)
					xmlwriter.SetValue("mytrailers","speed","300".ToString());
				if(checkBox300.Checked==false)
					xmlwriter.SetValue("mytrailers","speed","700".ToString());
				if(GermanTrailerCheckBox.Checked==true)
					xmlwriter.SetValueAsBool("mytrailers","Show german trailers", true);
				if(GermanTrailerCheckBox.Checked==false)
					xmlwriter.SetValueAsBool("mytrailers","Show german trailers", false);
			}
			this.Close();
		
		}
	}
}
