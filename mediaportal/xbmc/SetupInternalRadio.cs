using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms;
using DirectX.Capture;
using MediaPortal.GUI.Library;
namespace MediaPortal
{
	/// <summary>
	/// Summary description for SetupInternalRadio.
	/// </summary>
	public class SetupInternalRadio : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioCable;
		private System.Windows.Forms.RadioButton radioAntenne;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ComboBox comboBoxRadioDevice;
		private System.Windows.Forms.Label label3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SetupInternalRadio()
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioAntenne = new System.Windows.Forms.RadioButton();
			this.radioCable = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBoxRadioDevice = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.radioAntenne);
			this.groupBox1.Controls.Add(this.radioCable);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.comboBoxRadioDevice);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(384, 152);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Radio Tuner";
			// 
			// radioAntenne
			// 
			this.radioAntenne.Location = new System.Drawing.Point(40, 120);
			this.radioAntenne.Name = "radioAntenne";
			this.radioAntenne.Size = new System.Drawing.Size(104, 16);
			this.radioAntenne.TabIndex = 4;
			this.radioAntenne.Text = "Antenna";
			// 
			// radioCable
			// 
			this.radioCable.Location = new System.Drawing.Point(40, 104);
			this.radioCable.Name = "radioCable";
			this.radioCable.Size = new System.Drawing.Size(104, 16);
			this.radioCable.TabIndex = 3;
			this.radioCable.Text = "Cable";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Input:";
			// 
			// comboBoxRadioDevice
			// 
			this.comboBoxRadioDevice.Location = new System.Drawing.Point(72, 48);
			this.comboBoxRadioDevice.Name = "comboBoxRadioDevice";
			this.comboBoxRadioDevice.Size = new System.Drawing.Size(192, 21);
			this.comboBoxRadioDevice.TabIndex = 1;
			this.comboBoxRadioDevice.Text = "comboBox1";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Device:";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(352, 184);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(48, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(344, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Please note. Only the Hauppauge PVR 350 && USB2 are supported";
			// 
			// SetupInternalRadio
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 221);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBox1);
			this.Name = "SetupInternalRadio";
			this.Text = "SetupInternalRadio";
			this.Load += new System.EventHandler(this.SetupInternalRadio_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			using (AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				if (radioAntenne.Checked)
					xmlWriter.SetValue("radio","tuner", "Antenna");
				else
					xmlWriter.SetValue("radio","tuner", "Cable");
      
				xmlWriter.SetValue("radio","device",comboBoxRadioDevice.SelectedItem);
			}
			this.Close();
		}

		private void SetupInternalRadio_Load(object sender, System.EventArgs e)
		{
			string strRadioDevice="";
			using (AMS.Profile.Xml   xmlReader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				strRadioDevice=xmlReader.GetValueAsString("radio","device","");
				string strTunerType=xmlReader.GetValueAsString("radio","tuner","Antenna");
				if (strTunerType=="Antenna") radioAntenne.Checked=true;
				else radioCable.Checked=true;
			}

			comboBoxRadioDevice.Items.Clear();
			Filters filters = new Filters();
			int iSelect=0;
			for (int i=0; i < filters.VideoInputDevices.Count;++i)
			{
				comboBoxRadioDevice.Items.Add( filters.VideoInputDevices[i].Name);
				if (strRadioDevice.Equals(filters.VideoInputDevices[i].Name))
				{
					iSelect=i;
				}
			}
			if (comboBoxRadioDevice.Items.Count>0)
				comboBoxRadioDevice.SelectedIndex=iSelect;

		}
	}
}
