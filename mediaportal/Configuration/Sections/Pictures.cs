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
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.RadioButton radioButtonKenBurns;
    private System.Windows.Forms.TextBox kenburnsTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoShuffleCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox repeatSlideshowCheckBox;
    private System.Windows.Forms.GroupBox groupBox2;
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
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				durationTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "speed" ,3));
        transitionTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "transition", 20));
        kenburnsTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "kenburnsspeed", 20));
        radioButtonRandom.Checked = xmlreader.GetValueAsBool("pictures", "random", false);
        radioButtonKenBurns.Checked = xmlreader.GetValueAsBool("pictures", "kenburns", true	);
        radioButtonXFade.Checked = !radioButtonRandom.Checked && !radioButtonKenBurns.Checked;

        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
        repeatSlideshowCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoRepeat", false);
			}			
		}
		
		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("pictures", "speed", durationTextBox.Text);
        xmlwriter.SetValue("pictures", "transition", transitionTextBox.Text);
        xmlwriter.SetValue("pictures", "kenburnsspeed", kenburnsTextBox.Text);
        xmlwriter.SetValueAsBool("pictures", "random", radioButtonRandom.Checked);
        xmlwriter.SetValueAsBool("pictures", "kenburns", radioButtonKenBurns.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoShuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoRepeat", repeatSlideshowCheckBox.Checked);
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
      this.kenburnsTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.transitionTextBox = new System.Windows.Forms.TextBox();
      this.durationTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.repeatSlideshowCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.radioButtonKenBurns = new System.Windows.Forms.RadioButton();
      this.radioButtonRandom = new System.Windows.Forms.RadioButton();
      this.radioButtonXFade = new System.Windows.Forms.RadioButton();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.kenburnsTextBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.transitionTextBox);
      this.groupBox1.Controls.Add(this.durationTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.repeatSlideshowCheckBox);
      this.groupBox1.Controls.Add(this.autoShuffleCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 112);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 160);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Slideshow Settings";
      // 
      // kenburnsTextBox
      // 
      this.kenburnsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.kenburnsTextBox.Location = new System.Drawing.Point(168, 68);
      this.kenburnsTextBox.Name = "kenburnsTextBox";
      this.kenburnsTextBox.Size = new System.Drawing.Size(288, 20);
      this.kenburnsTextBox.TabIndex = 2;
      this.kenburnsTextBox.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(96, 16);
      this.label3.TabIndex = 20;
      this.label3.Text = "Ken Burns speed:";
      // 
      // transitionTextBox
      // 
      this.transitionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.transitionTextBox.Location = new System.Drawing.Point(168, 44);
      this.transitionTextBox.Name = "transitionTextBox";
      this.transitionTextBox.Size = new System.Drawing.Size(288, 20);
      this.transitionTextBox.TabIndex = 1;
      this.transitionTextBox.Text = "";
      // 
      // durationTextBox
      // 
      this.durationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.durationTextBox.Location = new System.Drawing.Point(168, 20);
      this.durationTextBox.Name = "durationTextBox";
      this.durationTextBox.Size = new System.Drawing.Size(288, 20);
      this.durationTextBox.TabIndex = 0;
      this.durationTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(104, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Transition (frames):";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(136, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Picture visible (seconds):";
      // 
      // repeatSlideshowCheckBox
      // 
      this.repeatSlideshowCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.repeatSlideshowCheckBox.Location = new System.Drawing.Point(16, 104);
      this.repeatSlideshowCheckBox.Name = "repeatSlideshowCheckBox";
      this.repeatSlideshowCheckBox.Size = new System.Drawing.Size(136, 16);
      this.repeatSlideshowCheckBox.TabIndex = 0;
      this.repeatSlideshowCheckBox.Text = "Repeat/loop slideshow";
      // 
      // autoShuffleCheckBox
      // 
      this.autoShuffleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoShuffleCheckBox.Location = new System.Drawing.Point(16, 128);
      this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
      this.autoShuffleCheckBox.Size = new System.Drawing.Size(128, 16);
      this.autoShuffleCheckBox.TabIndex = 1;
      this.autoShuffleCheckBox.Text = "Auto shuffle slideshow";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.radioButtonKenBurns);
      this.groupBox2.Controls.Add(this.radioButtonRandom);
      this.groupBox2.Controls.Add(this.radioButtonXFade);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 104);
      this.groupBox2.TabIndex = 21;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Slideshow Transitions";
      // 
      // radioButtonKenBurns
      // 
      this.radioButtonKenBurns.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonKenBurns.Location = new System.Drawing.Point(24, 24);
      this.radioButtonKenBurns.Name = "radioButtonKenBurns";
      this.radioButtonKenBurns.Size = new System.Drawing.Size(176, 16);
      this.radioButtonKenBurns.TabIndex = 3;
      this.radioButtonKenBurns.Text = "Use Ken Burns effect on pictures";
      // 
      // radioButtonRandom
      // 
      this.radioButtonRandom.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonRandom.Location = new System.Drawing.Point(24, 48);
      this.radioButtonRandom.Name = "radioButtonRandom";
      this.radioButtonRandom.Size = new System.Drawing.Size(216, 16);
      this.radioButtonRandom.TabIndex = 4;
      this.radioButtonRandom.Text = "Use random transitions between pictures";
      // 
      // radioButtonXFade
      // 
      this.radioButtonXFade.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonXFade.Location = new System.Drawing.Point(24, 72);
      this.radioButtonXFade.Name = "radioButtonXFade";
      this.radioButtonXFade.Size = new System.Drawing.Size(208, 16);
      this.radioButtonXFade.TabIndex = 5;
      this.radioButtonXFade.Text = "Use X-fade transition between pictures";
      // 
      // Pictures
      // 
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox2);
      this.Name = "Pictures";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

