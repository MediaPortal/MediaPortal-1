using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Wizard_Finished : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components = null;

		public Wizard_Finished() : this("Wizard Done")
		{
		}

		public Wizard_Finished(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
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
			this.label1.Location = new System.Drawing.Point(152, 200);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Done!";
			// 
			// Wizard_Finished
			// 
			this.Controls.Add(this.label1);
			this.Name = "Wizard_Finished";
			this.Size = new System.Drawing.Size(344, 392);
			this.ResumeLayout(false);

		}
		#endregion
	}
}

