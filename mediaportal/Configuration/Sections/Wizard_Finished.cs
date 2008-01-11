#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
	public class Wizard_Finished : MediaPortal.Configuration.SectionSettings
	{
    private MediaPortal.UserInterface.Controls.MPLabel headerLabel;
    private MediaPortal.UserInterface.Controls.MPLabel bodyLabel;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.PictureBox itemPictureBox;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="image"></param>
    public void SetImage(Image image)
    {
      itemPictureBox.Image = image;
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
      XmlNode imageNode = node.SelectSingleNode("image");
      
      if(headerNode != null && headerNode.InnerText.Length > 0)
      {
        SetHeader(headerNode.InnerText);
      }

      if(bodyNode != null && bodyNode.InnerText.Length > 0)
      {
        SetBody(bodyNode.InnerText);
      }

      if(imageNode != null && imageNode.InnerText.Length > 0)
      {
        SetImage(Image.FromFile(imageNode.InnerText));
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
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bodyLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.itemPictureBox = new System.Windows.Forms.PictureBox();
      this.SuspendLayout();
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.headerLabel.Location = new System.Drawing.Point(192, 8);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.Size = new System.Drawing.Size(280, 23);
      this.headerLabel.TabIndex = 6;
      this.headerLabel.Text = "Configuration finished";
      // 
      // bodyLabel
      // 
      this.bodyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.bodyLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.bodyLabel.Location = new System.Drawing.Point(192, 48);
      this.bodyLabel.Name = "bodyLabel";
      this.bodyLabel.Size = new System.Drawing.Size(276, 360);
      this.bodyLabel.TabIndex = 8;
      this.bodyLabel.Text = "MediaPortal is now configured and ready to use";
      // 
      // pictureBox
      // 
      this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
      this.pictureBox.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(46)), ((System.Byte)(68)), ((System.Byte)(150)));
      this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
      this.pictureBox.Location = new System.Drawing.Point(0, 0);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(184, 408);
      this.pictureBox.TabIndex = 7;
      this.pictureBox.TabStop = false;
      // 
      // itemPictureBox
      // 
      this.itemPictureBox.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(46)), ((System.Byte)(68)), ((System.Byte)(150)));
      this.itemPictureBox.Location = new System.Drawing.Point(30, 208);
      this.itemPictureBox.Name = "itemPictureBox";
      this.itemPictureBox.Size = new System.Drawing.Size(128, 128);
      this.itemPictureBox.TabIndex = 9;
      this.itemPictureBox.TabStop = false;
      // 
      // Wizard_Finished
      // 
      this.Controls.Add(this.itemPictureBox);
      this.Controls.Add(this.bodyLabel);
      this.Controls.Add(this.pictureBox);
      this.Controls.Add(this.headerLabel);
      this.Name = "Wizard_Finished";
      this.Size = new System.Drawing.Size(472, 408);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

