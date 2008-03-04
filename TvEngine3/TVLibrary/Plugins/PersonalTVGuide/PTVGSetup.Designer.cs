namespace SetupTv.Sections
{
	partial class PTVGSetup
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.debug = new System.Windows.Forms.CheckBox();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.debug);
      this.groupBox2.Location = new System.Drawing.Point(3, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(174, 44);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Debug section";
      // 
      // debug
      // 
      this.debug.AutoSize = true;
      this.debug.Location = new System.Drawing.Point(6, 19);
      this.debug.Name = "debug";
      this.debug.Size = new System.Drawing.Size(143, 17);
      this.debug.TabIndex = 0;
      this.debug.Text = "Enable extended logging";
      this.debug.UseVisualStyleBackColor = true;
      // 
      // PTVGSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox2);
      this.Name = "PTVGSetup";
      this.Size = new System.Drawing.Size(257, 100);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox debug;
	}
}
