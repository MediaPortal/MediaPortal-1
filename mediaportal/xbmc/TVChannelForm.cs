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
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtBoxFreq;
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
      this.labelName = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.upDownChannel = new System.Windows.Forms.NumericUpDown();
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.txtBoxFreq = new System.Windows.Forms.TextBox();
      ((System.ComponentModel.ISupportInitialize)(this.upDownChannel)).BeginInit();
      this.SuspendLayout();
      // 
      // labelName
      // 
      this.labelName.Location = new System.Drawing.Point(24, 48);
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
      this.label3.Text = "Number:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(24, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(144, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Please enter TV Channel number for channel";
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
      this.upDownChannel.TabIndex = 3;
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(150, 140);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(48, 23);
      this.buttonOK.TabIndex = 4;
      this.buttonOK.Text = "OK";
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(90, 140);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(56, 23);
      this.buttonCancel.TabIndex = 5;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(24, 110);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 16);
      this.label2.TabIndex = 6;
      this.label2.Text = "Frequency:";
      // 
      // txtBoxFreq
      // 
      this.txtBoxFreq.Location = new System.Drawing.Point(88, 110);
      this.txtBoxFreq.Name = "txtBoxFreq";
      this.txtBoxFreq.TabIndex = 7;
      this.txtBoxFreq.Text = "";
      // 
      // TVChannelForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(240, 173);
      this.Controls.Add(this.txtBoxFreq);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.Controls.Add(this.upDownChannel);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.labelName);
      this.Controls.Add(this.label1);
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
