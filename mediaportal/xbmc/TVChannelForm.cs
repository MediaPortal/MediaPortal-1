using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for TVChannelForm.
	/// </summary>
	public class TVChannelForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown upDownChannel;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
    string m_strChannel;
    string m_strFrequency;
    private System.Windows.Forms.TextBox txtBoxFreq;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label2;
    int    m_iNumber;
		public TVChannelForm()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TVChannelForm));
			this.labelName = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.upDownChannel = new System.Windows.Forms.NumericUpDown();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.txtBoxFreq = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.upDownChannel)).BeginInit();
			this.SuspendLayout();
			// 
			// labelName
			// 
			this.labelName.Location = new System.Drawing.Point(24, 32);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(160, 16);
			this.labelName.TabIndex = 1;
			this.labelName.Text = "-";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Channel:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(144, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please enter channel for:";
			// 
			// upDownChannel
			// 
			this.upDownChannel.Location = new System.Drawing.Point(88, 80);
			this.upDownChannel.Maximum = new System.Decimal(new int[] {
																																	253,
																																	0,
																																	0,
																																	0});
			this.upDownChannel.Name = "upDownChannel";
			this.upDownChannel.Size = new System.Drawing.Size(56, 20);
			this.upDownChannel.TabIndex = 0;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(208, 256);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(48, 23);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(144, 256);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(56, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// txtBoxFreq
			// 
			this.txtBoxFreq.Location = new System.Drawing.Point(96, 224);
			this.txtBoxFreq.Name = "txtBoxFreq";
			this.txtBoxFreq.TabIndex = 1;
			this.txtBoxFreq.Text = "";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 184);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(184, 16);
			this.label8.TabIndex = 16;
			this.label8.Text = "Channel 63- or 47++";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 168);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(256, 16);
			this.label7.TabIndex = 15;
			this.label7.Text = "When your country is using subchannels like";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 152);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(272, 16);
			this.label6.TabIndex = 14;
			this.label6.Text = "defaults. You only need to fill in the frequency";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 136);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(272, 16);
			this.label5.TabIndex = 13;
			this.label5.Text = "In almost all cases, leave them to 0 which will use the";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 120);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(232, 16);
			this.label4.TabIndex = 12;
			this.label4.Text = "PLEASE NOTE. Frequencies are optional!";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(24, 232);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(48, 16);
			this.label9.TabIndex = 18;
			this.label9.Text = "override";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(24, 216);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(64, 16);
			this.label10.TabIndex = 17;
			this.label10.Text = "Frequency";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(208, 224);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 16);
			this.label2.TabIndex = 19;
			this.label2.Text = "MHz.";
			// 
			// TVChannelForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 301);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtBoxFreq);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.upDownChannel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "TVChannelForm";
			this.Text = "TVChannelForm";
			this.Load += new System.EventHandler(this.TVChannelForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.upDownChannel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

    public string Channel
    {
      set { m_strChannel=value;}
      get { return m_strChannel;}
    }
    public int Number
    { 
      get { return m_iNumber;}
      set 
      {
        m_iNumber=value;
      }
    }
    public string Frequency
    {
      get { return m_strFrequency;}
      set { m_strFrequency=value;}
    }

    private void TVChannelForm_Load(object sender, System.EventArgs e)
    {
        labelName.Text=m_strChannel;      
        upDownChannel.Value=m_iNumber;
        txtBoxFreq.Text=m_strFrequency;
    }

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      m_strFrequency=txtBoxFreq.Text;
      m_iNumber=(int)upDownChannel.Value;
      this.Close();
    }

    private void buttonCancel_Click(object sender, System.EventArgs e)
    {
      m_strFrequency="";
      m_iNumber=-1;
      this.Close();
    }
	}
}
