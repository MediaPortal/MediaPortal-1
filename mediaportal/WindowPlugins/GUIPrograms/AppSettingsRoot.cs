using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WindowPlugins.GUIPrograms
{
	public class AppSettingsRoot : WindowPlugins.GUIPrograms.AppSettings
	{
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.IContainer components = null;

		public AppSettingsRoot()
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
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(216, 32);
			this.label3.TabIndex = 81;
			this.label3.Text = "my Programs root";
			// 
			// AppSettingsRoot
			// 
			this.Controls.Add(this.label3);
			this.Name = "AppSettingsRoot";
			this.Size = new System.Drawing.Size(256, 248);
			this.ResumeLayout(false);

		}
		#endregion
	}
}

