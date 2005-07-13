using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for editName.
	/// </summary>
	public class editName : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.Button btnOk;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public editName()
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
      this.textBoxName = new System.Windows.Forms.TextBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(200, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Enter a name for this channel";
      // 
      // textBoxName
      // 
      this.textBoxName.Location = new System.Drawing.Point(16, 40);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size(256, 20);
      this.textBoxName.TabIndex = 0;
      this.textBoxName.Text = "";
      this.textBoxName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editName_KeyDown);
      // 
      // btnOk
      // 
      this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnOk.Location = new System.Drawing.Point(232, 72);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(40, 23);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "Ok";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // editName
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(292, 102);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.textBoxName);
      this.Controls.Add(this.label1);
      this.Name = "editName";
      this.Text = "Enter a name for this channel";
      this.ResumeLayout(false);

    }
		#endregion

		private void editName_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode== Keys.Enter && textBoxName.Text != String.Empty)
			{
				this.Close();
			}
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		
		public string ChannelName
		{
			get { return textBoxName.Text;}
		}
	}
}
