using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for IRLearnFORM.
	/// </summary>
	public class IRLearnFORM : System.Windows.Forms.Form
	{
		public System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public void SetMessage(string message)
		{
			label1.Text = message;
			Application.DoEvents();
		}

		public IRLearnFORM()
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
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(232, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// IRLearnFORM
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(272, 94);
			this.Controls.Add(this.label1);
			this.Name = "IRLearnFORM";
			this.Text = "IRLearn";
			this.ResumeLayout(false);

		}
		#endregion

		private void label1_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
