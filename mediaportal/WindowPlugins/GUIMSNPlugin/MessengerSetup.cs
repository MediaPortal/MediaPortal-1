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
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Password:";
      // 
      // textBoxPassword
      // 
      this.textBoxPassword.Location = new System.Drawing.Point(24, 96);
      this.textBoxPassword.Name = "textBoxPassword";
      this.textBoxPassword.PasswordChar = '*';
      this.textBoxPassword.Size = new System.Drawing.Size(232, 20);
      this.textBoxPassword.TabIndex = 3;
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
      // MessengerSetup
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(292, 273);
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
      using (AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("MSNmessenger","email",textBoxEMail.Text);
        xmlWriter.SetValue("MSNmessenger","password",textBoxPassword.Text);
      }
      this.Close();
    }

    private void MessengerSetup_Load(object sender, System.EventArgs e)
    {
      
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        textBoxEMail.Text = xmlreader.GetValueAsString("MSNmessenger","email","");
        textBoxPassword.Text = xmlreader.GetValueAsString("MSNmessenger","password","");
      }
    }
	}
}
