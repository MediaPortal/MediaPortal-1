using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class TVProgramGuide : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
		private MediaPortal.UserInterface.Controls.MPCheckBox useColorCheckBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox GrabbercomboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox AdvancedDaystextBox;
    private System.Windows.Forms.Button parametersButton;
    private System.Windows.Forms.TextBox parametersTextBox;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox daysToKeepTextBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button browseButton;
    private System.Windows.Forms.TextBox folderNameTextBox;
    private System.Windows.Forms.Label folderNameLabel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox compensateTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox useTimeZoneCheckBox;
    private System.Windows.Forms.Label label1;
		protected System.Windows.Forms.RadioButton advancedRadioButton;
		protected System.Windows.Forms.RadioButton basicRadioButton;
		private System.ComponentModel.IContainer components = null;

		public TVProgramGuide() : this("Program Guide")
		{
		}

		public TVProgramGuide(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
      // Setup grabbers
      //
      SetupGrabbers();
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
      this.useColorCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.advancedRadioButton = new System.Windows.Forms.RadioButton();
      this.label2 = new System.Windows.Forms.Label();
      this.compensateTextBox = new System.Windows.Forms.TextBox();
      this.useTimeZoneCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.browseButton = new System.Windows.Forms.Button();
      this.folderNameTextBox = new System.Windows.Forms.TextBox();
      this.folderNameLabel = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.daysToKeepTextBox = new System.Windows.Forms.TextBox();
      this.parametersButton = new System.Windows.Forms.Button();
      this.parametersTextBox = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.AdvancedDaystextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.GrabbercomboBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.basicRadioButton = new System.Windows.Forms.RadioButton();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.useColorCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 64);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General Settings";
      // 
      // useColorCheckBox
      // 
      this.useColorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useColorCheckBox.Location = new System.Drawing.Point(16, 24);
      this.useColorCheckBox.Name = "useColorCheckBox";
      this.useColorCheckBox.Size = new System.Drawing.Size(308, 24);
      this.useColorCheckBox.TabIndex = 0;
      this.useColorCheckBox.Text = "Use colors in program guide";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.advancedRadioButton);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.compensateTextBox);
      this.groupBox2.Controls.Add(this.useTimeZoneCheckBox);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.browseButton);
      this.groupBox2.Controls.Add(this.folderNameTextBox);
      this.groupBox2.Controls.Add(this.folderNameLabel);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.daysToKeepTextBox);
      this.groupBox2.Controls.Add(this.parametersButton);
      this.groupBox2.Controls.Add(this.parametersTextBox);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.AdvancedDaystextBox);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.GrabbercomboBox);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.basicRadioButton);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(8, 80);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(440, 288);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "XMLTV Settings";
      // 
      // advancedRadioButton
      // 
      this.advancedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.advancedRadioButton.Location = new System.Drawing.Point(16, 219);
      this.advancedRadioButton.Name = "advancedRadioButton";
      this.advancedRadioButton.Size = new System.Drawing.Size(184, 24);
      this.advancedRadioButton.TabIndex = 55;
      this.advancedRadioButton.Text = "Advanced Single Day Grab";
      this.advancedRadioButton.CheckedChanged += new System.EventHandler(this.advancedRadioButton_CheckedChanged);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(216, 84);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 23);
      this.label2.TabIndex = 53;
      this.label2.Text = "hour(s)";
      // 
      // compensateTextBox
      // 
      this.compensateTextBox.Location = new System.Drawing.Point(168, 81);
      this.compensateTextBox.MaxLength = 3;
      this.compensateTextBox.Name = "compensateTextBox";
      this.compensateTextBox.Size = new System.Drawing.Size(40, 20);
      this.compensateTextBox.TabIndex = 52;
      this.compensateTextBox.Text = "";
      // 
      // useTimeZoneCheckBox
      // 
      this.useTimeZoneCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useTimeZoneCheckBox.Location = new System.Drawing.Point(16, 50);
      this.useTimeZoneCheckBox.Name = "useTimeZoneCheckBox";
      this.useTimeZoneCheckBox.Size = new System.Drawing.Size(240, 24);
      this.useTimeZoneCheckBox.TabIndex = 51;
      this.useTimeZoneCheckBox.Text = "Use time zone information from XMLTV";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 84);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 23);
      this.label1.TabIndex = 50;
      this.label1.Text = "Compensate time zone with";
      // 
      // browseButton
      // 
      this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.browseButton.Location = new System.Drawing.Point(369, 22);
      this.browseButton.Name = "browseButton";
      this.browseButton.Size = new System.Drawing.Size(56, 20);
      this.browseButton.TabIndex = 49;
      this.browseButton.Text = "Browse";
      this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(96, 22);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(265, 20);
      this.folderNameTextBox.TabIndex = 48;
      this.folderNameTextBox.Text = "";
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 25);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
      this.folderNameLabel.TabIndex = 47;
      this.folderNameLabel.Text = "XMLTV folder";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(32, 247);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(136, 16);
      this.label6.TabIndex = 44;
      this.label6.Text = "Advanced days";
      // 
      // daysToKeepTextBox
      // 
      this.daysToKeepTextBox.Enabled = false;
      this.daysToKeepTextBox.Location = new System.Drawing.Point(168, 194);
      this.daysToKeepTextBox.MaxLength = 3;
      this.daysToKeepTextBox.Name = "daysToKeepTextBox";
      this.daysToKeepTextBox.Size = new System.Drawing.Size(40, 20);
      this.daysToKeepTextBox.TabIndex = 40;
      this.daysToKeepTextBox.Text = "";
      this.daysToKeepTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.daysToKeepTextBox_KeyPress);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Enabled = false;
      this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.parametersButton.Location = new System.Drawing.Point(369, 138);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(56, 20);
      this.parametersButton.TabIndex = 39;
      this.parametersButton.Text = "List";
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Enabled = false;
      this.parametersTextBox.Location = new System.Drawing.Point(168, 137);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(192, 20);
      this.parametersTextBox.TabIndex = 38;
      this.parametersTextBox.Text = "";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 140);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(80, 23);
      this.label7.TabIndex = 37;
      this.label7.Text = "Parameters";
      // 
      // AdvancedDaystextBox
      // 
      this.AdvancedDaystextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.AdvancedDaystextBox.Enabled = false;
      this.AdvancedDaystextBox.Location = new System.Drawing.Point(168, 245);
      this.AdvancedDaystextBox.Name = "AdvancedDaystextBox";
      this.AdvancedDaystextBox.Size = new System.Drawing.Size(192, 20);
      this.AdvancedDaystextBox.TabIndex = 36;
      this.AdvancedDaystextBox.Text = "";
      this.AdvancedDaystextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AdvancedDaystextBox_KeyPress);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(32, 198);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(144, 16);
      this.label4.TabIndex = 30;
      this.label4.Text = "Days to keep in guide";
      // 
      // GrabbercomboBox
      // 
      this.GrabbercomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.GrabbercomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.GrabbercomboBox.Location = new System.Drawing.Point(168, 112);
      this.GrabbercomboBox.Name = "GrabbercomboBox";
      this.GrabbercomboBox.Size = new System.Drawing.Size(256, 21);
      this.GrabbercomboBox.TabIndex = 29;
      this.GrabbercomboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GrabbercomboBox_KeyPress);
      this.GrabbercomboBox.SelectedIndexChanged += new System.EventHandler(this.GrabbercomboBox_SelectedIndexChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 115);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(144, 23);
      this.label3.TabIndex = 28;
      this.label3.Text = "Grabber";
      // 
      // basicRadioButton
      // 
      this.basicRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.basicRadioButton.Location = new System.Drawing.Point(16, 170);
      this.basicRadioButton.Name = "basicRadioButton";
      this.basicRadioButton.Size = new System.Drawing.Size(192, 24);
      this.basicRadioButton.TabIndex = 54;
      this.basicRadioButton.Text = "Basic Multiday Grab";
      this.basicRadioButton.CheckedChanged += new System.EventHandler(this.basicRadioButton_CheckedChanged);
      // 
      // TVProgramGuide
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "TVProgramGuide";
      this.Size = new System.Drawing.Size(456, 448);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    private void SetupGrabbers()
    {
      GrabbercomboBox.Items.Add("tv_grab_de_tvtoday");
      GrabbercomboBox.Items.Add("tv_grab_dk");
      GrabbercomboBox.Items.Add("tv_grab_es");
      GrabbercomboBox.Items.Add("tv_grab_es_digital");
      GrabbercomboBox.Items.Add("tv_grab_fi");
      GrabbercomboBox.Items.Add("tv_grab_fr");
      GrabbercomboBox.Items.Add("tv_grab_huro");
      GrabbercomboBox.Items.Add("tv_grab_it");
      GrabbercomboBox.Items.Add("tv_grab_nl");
      GrabbercomboBox.Items.Add("tv_grab_no");
      GrabbercomboBox.Items.Add("tv_grab_pt");
      GrabbercomboBox.Items.Add("tv_grab_se");
      GrabbercomboBox.Items.Add("tv_grab_uk_rt");
    }

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				useColorCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "colors", true);

				useTimeZoneCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "usetimezone", true);
				compensateTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("xmltv", "timezonecorrection", 0));

				folderNameTextBox.Text = xmlreader.GetValueAsString("xmltv", "folder", "");

				GrabbercomboBox.SelectedItem = xmlreader.GetValueAsString("xmltv","grabber","");
				AdvancedDaystextBox.Text=xmlreader.GetValueAsString("xmltv","days","1,2,3,5");
				parametersTextBox.Text=xmlreader.GetValueAsString("xmltv","args","");
				daysToKeepTextBox.Text = xmlreader.GetValueAsString("xmltv","daystokeep", "7");
				advancedRadioButton.Checked = xmlreader.GetValueAsBool("xmltv", "advanced", false);
				basicRadioButton.Checked = !advancedRadioButton.Checked;

			}						
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("xmltv", "colors", useColorCheckBox.Checked);
				xmlwriter.SetValueAsBool("xmltv", "usetimezone", useTimeZoneCheckBox.Checked);

				xmlwriter.SetValue("xmltv", "timezonecorrection", compensateTextBox.Text);
				xmlwriter.SetValue("xmltv", "folder", folderNameTextBox.Text);

				xmlwriter.SetValue("xmltv", "grabber",GrabbercomboBox.Text);
				xmlwriter.SetValue("xmltv", "daystokeep",daysToKeepTextBox.Text);
				xmlwriter.SetValueAsBool("xmltv", "advanced", advancedRadioButton.Checked);
				xmlwriter.SetValue("xmltv", "days",AdvancedDaystextBox.Text);
				xmlwriter.SetValue("xmltv", "args",parametersTextBox.Text);
			}
		}

		private void compensateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
      //
      // Allow only numbers, '-' and backspace.
      //
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != '-')
			{
				e.Handled = true;
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where the XMLTV data is stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}							
		}

    private void daysToKeepTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, '-' and backspace.
      //
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }    
    }

    private void GrabbercomboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      parametersButton.Enabled = parametersTextBox.Enabled = (GrabbercomboBox.SelectedItem != null);
      parametersTextBox.Text="";
      if(GrabbercomboBox.Text=="tv_grab_fi") daysToKeepTextBox.Text="10";
      else if(GrabbercomboBox.Text=="tv_grab_huro") daysToKeepTextBox.Text="8";
      else if((GrabbercomboBox.Text=="tv_grab_es")|(GrabbercomboBox.Text=="tv_grab_es_digital")|(GrabbercomboBox.Text=="tv_grab_pt")) daysToKeepTextBox.Text="3";
      else if(GrabbercomboBox.Text=="tv_grab_se") daysToKeepTextBox.Text="5";
      else daysToKeepTextBox.Text="7";

      AdvancedDaystextBox.Enabled = advancedRadioButton.Checked;
      daysToKeepTextBox.Enabled = basicRadioButton.Checked;
    }

    private void GrabbercomboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == (char)System.Windows.Forms.Keys.Delete || e.KeyChar == (char)System.Windows.Forms.Keys.Back)
      {
        GrabbercomboBox.SelectedItem = null;
        GrabbercomboBox.Text = String.Empty;
      }        
    }

    private void parametersButton_Click(object sender, System.EventArgs e)
    {
		  parametersTextBox.Text="";
		  ParameterForm parameters = new ParameterForm();
			
      if(GrabbercomboBox.Text==("tv_grab_dk") | GrabbercomboBox.Text==("tv_grab_es") | GrabbercomboBox.Text==("tv_grab_es_digital")
        | GrabbercomboBox.Text==("tv_grab_fi") | GrabbercomboBox.Text==("tv_grab_huro") | GrabbercomboBox.Text==("tv_grab_no")
        | GrabbercomboBox.Text==("tv_grab_pt") | GrabbercomboBox.Text==("tv_grab_se"))
      {
        parameters.AddParameter("", "No options available for this grabber");
      }
      else if(GrabbercomboBox.Text==("tv_grab_fr") | GrabbercomboBox.Text==("tv_grab_it") | GrabbercomboBox.Text==("tv_grab_nl"))
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
      }
      else if(GrabbercomboBox.Text=="tv_grab_de_tvtoday")
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--nosqueezeout", "Don't parse program descriptions for adiitional information (actors,director,etc");
        parameters.AddParameter("--slow --nosqueezeout", "Fetch full program details and don't parse descriptions");
      }
      else if(GrabbercomboBox.Text=="tv_grab_uk_rt")
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--slow --get-categories", "Fetch full program details and categories (but takes even longer)");
      }

      if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    private void AdvancedDaystextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == ',')
      {
        if(AdvancedDaystextBox.Text.EndsWith(","))
        {
          e.Handled = true;
          return;
        }
      }

      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != ',')
      {
        e.Handled = true;
      }        
    }

    protected void basicRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      AdvancedDaystextBox.Enabled = false;
      daysToKeepTextBox.Enabled = true;
    }

    protected void advancedRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      AdvancedDaystextBox.Enabled = true;
      daysToKeepTextBox.Enabled = false;
    }
	}
}

