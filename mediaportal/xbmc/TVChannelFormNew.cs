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
	public class TVChannelFormNew : System.Windows.Forms.Form
	{
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
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtBoxFreq;
    private System.Windows.Forms.TextBox txtBoxChannel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
    int    m_iNumber;
		public TVChannelFormNew()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.upDownChannel = new System.Windows.Forms.NumericUpDown();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtBoxFreq = new System.Windows.Forms.TextBox();
			this.txtBoxChannel = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.upDownChannel)).BeginInit();
			this.SuspendLayout();
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
			this.upDownChannel.TabIndex = 1;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(232, 248);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(48, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(168, 248);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(56, 23);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 208);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Frequency";
			// 
			// txtBoxFreq
			// 
			this.txtBoxFreq.Location = new System.Drawing.Point(88, 216);
			this.txtBoxFreq.Name = "txtBoxFreq";
			this.txtBoxFreq.TabIndex = 2;
			this.txtBoxFreq.Text = "";
			// 
			// txtBoxChannel
			// 
			this.txtBoxChannel.Location = new System.Drawing.Point(32, 48);
			this.txtBoxChannel.Name = "txtBoxChannel";
			this.txtBoxChannel.Size = new System.Drawing.Size(168, 20);
			this.txtBoxChannel.TabIndex = 0;
			this.txtBoxChannel.Text = "textBox1";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 112);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(232, 16);
			this.label4.TabIndex = 7;
			this.label4.Text = "PLEASE NOTE. Frequencies are optional!";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 128);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(272, 16);
			this.label5.TabIndex = 8;
			this.label5.Text = "In almost all cases, leave them to 0 which will use the";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 144);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(272, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = "defaults. You only need to fill in the frequency";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 160);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(256, 16);
			this.label7.TabIndex = 10;
			this.label7.Text = "When your country is using subchannels like";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 176);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(184, 16);
			this.label8.TabIndex = 11;
			this.label8.Text = "Channel 63- or 47++";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(24, 224);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(48, 16);
			this.label9.TabIndex = 12;
			this.label9.Text = "override";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(200, 216);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 16);
			this.label10.TabIndex = 13;
			this.label10.Text = "MHz.";
			// 
			// TVChannelFormNew
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 285);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtBoxChannel);
			this.Controls.Add(this.txtBoxFreq);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.upDownChannel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Name = "TVChannelFormNew";
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
        txtBoxChannel.Text=m_strChannel;      
        upDownChannel.Value=m_iNumber;
        txtBoxFreq.Text=m_strFrequency;
    }

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      m_strChannel=txtBoxChannel.Text;
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
