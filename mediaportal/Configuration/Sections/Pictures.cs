using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Pictures : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox durationTextBox;
		private System.Windows.Forms.TextBox transitionTextBox;
    private System.Windows.Forms.RadioButton radioButtonRandom;
    private System.Windows.Forms.RadioButton radioButtonXFade;
		private System.ComponentModel.IContainer components = null;

		public Pictures() : this("Pictures")
		{
		}

		public Pictures(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				durationTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "speed" ,3));
        transitionTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "transition", 20));
        radioButtonRandom.Checked = xmlreader.GetValueAsBool("pictures", "random", true);
        radioButtonXFade.Checked = !radioButtonRandom.Checked;

			}			
		}
		
		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("pictures", "speed", durationTextBox.Text);
        xmlwriter.SetValue("pictures", "transition", transitionTextBox.Text);
        xmlwriter.SetValueAsBool("pictures", "random", radioButtonRandom.Checked);
			}
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.transitionTextBox = new System.Windows.Forms.TextBox();
      this.durationTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.radioButtonRandom = new System.Windows.Forms.RadioButton();
      this.radioButtonXFade = new System.Windows.Forms.RadioButton();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.radioButtonRandom);
      this.groupBox1.Controls.Add(this.transitionTextBox);
      this.groupBox1.Controls.Add(this.durationTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.radioButtonXFade);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(376, 152);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Slideshow Settings";
      // 
      // transitionTextBox
      // 
      this.transitionTextBox.Location = new System.Drawing.Point(168, 51);
      this.transitionTextBox.Name = "transitionTextBox";
      this.transitionTextBox.Size = new System.Drawing.Size(40, 20);
      this.transitionTextBox.TabIndex = 18;
      this.transitionTextBox.Text = "";
      // 
      // durationTextBox
      // 
      this.durationTextBox.Location = new System.Drawing.Point(168, 26);
      this.durationTextBox.Name = "durationTextBox";
      this.durationTextBox.Size = new System.Drawing.Size(40, 20);
      this.durationTextBox.TabIndex = 17;
      this.durationTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 54);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(150, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "Transition (frames)";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 29);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Picture visible (seconds)";
      // 
      // radioButtonRandom
      // 
      this.radioButtonRandom.Location = new System.Drawing.Point(24, 80);
      this.radioButtonRandom.Name = "radioButtonRandom";
      this.radioButtonRandom.Size = new System.Drawing.Size(256, 24);
      this.radioButtonRandom.TabIndex = 19;
      this.radioButtonRandom.Text = "Use random transitions between pictures";
      // 
      // radioButtonXFade
      // 
      this.radioButtonXFade.Location = new System.Drawing.Point(24, 112);
      this.radioButtonXFade.Name = "radioButtonXFade";
      this.radioButtonXFade.Size = new System.Drawing.Size(224, 24);
      this.radioButtonXFade.TabIndex = 19;
      this.radioButtonXFade.Text = "Use x-fade transition between pictures";
      // 
      // Pictures
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Pictures";
      this.Size = new System.Drawing.Size(392, 360);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion


	}
}

