using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.Configuration.Sections
{
	public class Wizard_Finished : MediaPortal.Configuration.SectionSettings
	{
    private System.Windows.Forms.Label headerLabel;
    private System.Windows.Forms.Label bodyLabel;
    private System.Windows.Forms.PictureBox pictureBox;
		private System.ComponentModel.IContainer components = null;

		public Wizard_Finished() : this("Wizard Done")
		{
		}

		public Wizard_Finished(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

    public void SetHeader(string header)
    {
      headerLabel.Text = header;
    }

    public void SetBody(string body)
    {
      bodyLabel.Text = body;
    }

    public override void LoadWizardSettings(System.Xml.XmlNode node)
    {
      //
      // Fetch section information
      //
      XmlNode headerNode = node.SelectSingleNode("header");
      XmlNode bodyNode = node.SelectSingleNode("body");
      
      if(headerNode != null && headerNode.InnerText.Length > 0)
      {
        SetHeader(headerNode.InnerText);
      }

      if(bodyNode != null && bodyNode.InnerText.Length > 0)
      {
        SetBody(bodyNode.InnerText);
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Wizard_Finished));
      this.headerLabel = new System.Windows.Forms.Label();
      this.bodyLabel = new System.Windows.Forms.Label();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.SuspendLayout();
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.headerLabel.Location = new System.Drawing.Point(192, 8);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.Size = new System.Drawing.Size(360, 23);
      this.headerLabel.TabIndex = 6;
      this.headerLabel.Text = "Header";
      // 
      // bodyLabel
      // 
      this.bodyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.bodyLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.bodyLabel.Location = new System.Drawing.Point(196, 48);
      this.bodyLabel.Name = "bodyLabel";
      this.bodyLabel.Size = new System.Drawing.Size(360, 264);
      this.bodyLabel.TabIndex = 8;
      this.bodyLabel.Text = "Body";
      // 
      // pictureBox
      // 
      this.pictureBox.Dock = System.Windows.Forms.DockStyle.Left;
      this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
      this.pictureBox.Location = new System.Drawing.Point(0, 0);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(184, 326);
      this.pictureBox.TabIndex = 7;
      this.pictureBox.TabStop = false;
      // 
      // Wizard_Finished
      // 
      this.Controls.Add(this.bodyLabel);
      this.Controls.Add(this.pictureBox);
      this.Controls.Add(this.headerLabel);
      this.Name = "Wizard_Finished";
      this.Size = new System.Drawing.Size(560, 326);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

