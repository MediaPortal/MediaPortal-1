using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProcessPlugins.CallerId
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Label labelFilter;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numericFilter;
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

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        numericFilter.Value = xmlreader.GetValueAsInt("isdn", "stripprefix", 0);
      }
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
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonOK = new System.Windows.Forms.Button();
      this.labelFilter = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.numericFilter = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numericFilter)).BeginInit();
      this.SuspendLayout();
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonCancel.Location = new System.Drawing.Point(144, 128);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(72, 24);
      this.buttonCancel.TabIndex = 1;
      this.buttonCancel.Text = "&Cancel";
      // 
      // buttonOK
      // 
      this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonOK.Location = new System.Drawing.Point(64, 128);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(72, 24);
      this.buttonOK.TabIndex = 0;
      this.buttonOK.Text = "&OK";
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // labelFilter
      // 
      this.labelFilter.Location = new System.Drawing.Point(16, 32);
      this.labelFilter.Name = "labelFilter";
      this.labelFilter.Size = new System.Drawing.Size(188, 16);
      this.labelFilter.TabIndex = 3;
      this.labelFilter.Text = "How many digits should be removed";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(144, 14);
      this.label1.TabIndex = 4;
      this.label1.Text = "from the incoming number?";
      // 
      // numericFilter
      // 
      this.numericFilter.Location = new System.Drawing.Point(160, 54);
      this.numericFilter.Maximum = new System.Decimal(new int[] {
                                                                  12,
                                                                  0,
                                                                  0,
                                                                  0});
      this.numericFilter.Name = "numericFilter";
      this.numericFilter.Size = new System.Drawing.Size(32, 20);
      this.numericFilter.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(168, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "(Usually this should be set to 0)";
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(222, 160);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.numericFilter);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.labelFilter);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SetupForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "ISDN Caller-ID";
      ((System.ComponentModel.ISupportInitialize)(this.numericFilter)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("isdn", "stripprefix", numericFilter.Value);
      }
    }

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      this.Close();
    }
  }
}
