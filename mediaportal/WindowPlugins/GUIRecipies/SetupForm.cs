using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;

namespace GUIRecipies
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form, ISetupForm
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label lblNumber;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button cbOk;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label2;
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

		public string PluginName()
		{
			return "My Recipies";
		}

		public string Description()
		{
			return "A recipie plugin for Media Portal";
		}

		public string Author()
		{
			return "Gucky62/Domi_fan";
		}

		public void ShowPlugin()
		{
			ShowDialog();
		}
		public bool DefaultEnabled()
		{
			return false;
		}
		public bool CanEnable()
		{
			return true;
		}

    public bool HasSetup()
    {
      return true;
    }
		public int GetWindowId()
		{
			return 750;
		}

		/// <summary>
		/// If the plugin should have its own button on the home screen then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{

			strButtonText = GUILocalizeStrings.Get(10);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
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
			this.lblNumber = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.cbOk = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.label2 = new System.Windows.Forms.Label();
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
			this.txtFile.Enabled = false;
			this.txtFile.Location = new System.Drawing.Point(136, 16);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(224, 20);
			this.txtFile.TabIndex = 1;
			this.txtFile.Text = "";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(376, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// lblNumber
			// 
			this.lblNumber.Location = new System.Drawing.Point(136, 48);
			this.lblNumber.Name = "lblNumber";
			this.lblNumber.Size = new System.Drawing.Size(152, 23);
			this.lblNumber.TabIndex = 3;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(408, 16);
			this.button2.Name = "button2";
			this.button2.TabIndex = 4;
			this.button2.Text = "Import";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// cbOk
			// 
			this.cbOk.Location = new System.Drawing.Point(408, 64);
			this.cbOk.Name = "cbOk";
			this.cbOk.TabIndex = 5;
			this.cbOk.Text = "Ok";
			this.cbOk.Click += new System.EventHandler(this.cbOk_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(376, 32);
			this.label2.TabIndex = 6;
			this.label2.Text = "My recipies works with mealmaster files. A good catalog of 15.000 recipies can be" +
				" found at http://www.ffts.com/recipes.htm";
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 102);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbOk);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.lblNumber);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.txtFile);
			this.Controls.Add(this.label1);
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.ResumeLayout(false);

		}
		#endregion

		private void cbOk_Click(object sender, System.EventArgs e)
		{
			this.Visible = false;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog( this ) == DialogResult.OK )
			{
				txtFile.Text = openFileDialog1.FileName;
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if( txtFile.Text.Length < 1 )
			{
				MessageBox.Show( "Please select a file to import!" );
				return;
			}
			RecipieReader rr = new RecipieReader( txtFile.Text );
			rr.GetRecipies();
			lblNumber.Text = rr.RecipieCount + " Recipies read.";
		}
	}
}
