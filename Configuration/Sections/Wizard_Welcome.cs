#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Wizard_Welcome : SectionSettings
  {
    private PictureBox pictureBox;
    private Panel panel1;
    private Panel panel2;
    private PictureBox itemPictureBox;
    private MPLabel headerLabel;
    private MPLabel bodyLabel;
    private IContainer components = null;

    public Wizard_Welcome() : this("Wizard Welcome")
    {
    }

    public Wizard_Welcome(string name) : base(name)
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

    public override void LoadWizardSettings(XmlNode node)
    {
      //
      // Fetch section information
      //
      XmlNode headerNode = node.SelectSingleNode("header");
      XmlNode bodyNode = node.SelectSingleNode("body");
      XmlNode imageNode = node.SelectSingleNode("image");

      if (headerNode != null && headerNode.InnerText.Length > 0)
      {
        SetHeader(headerNode.InnerText);
      }

      if (bodyNode != null && bodyNode.InnerText.Length > 0)
      {
        SetBody(bodyNode.InnerText);
      }

      if (imageNode != null && imageNode.InnerText.Length > 0)
      {
        SetImage(Image.FromFile(imageNode.InnerText));
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (Wizard_Welcome));
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.itemPictureBox = new System.Windows.Forms.PictureBox();
      this.bodyLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.itemPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold,
                                                      System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.headerLabel.Location = new System.Drawing.Point(192, 8);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.Size = new System.Drawing.Size(280, 23);
      this.headerLabel.TabIndex = 1;
      this.headerLabel.Text = "MediaPortal configuration";
      // 
      // pictureBox
      // 
      this.pictureBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.pictureBox.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (46)))), ((int) (((byte) (68)))),
                                                                ((int) (((byte) (150)))));
      this.pictureBox.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox.Image")));
      this.pictureBox.Location = new System.Drawing.Point(0, 0);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(184, 408);
      this.pictureBox.TabIndex = 1;
      this.pictureBox.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
      this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1, 408);
      this.panel1.TabIndex = 2;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel2.Location = new System.Drawing.Point(1, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(1, 408);
      this.panel2.TabIndex = 0;
      // 
      // itemPictureBox
      // 
      this.itemPictureBox.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (46)))), ((int) (((byte) (68)))),
                                                                    ((int) (((byte) (150)))));
      this.itemPictureBox.Location = new System.Drawing.Point(30, 208);
      this.itemPictureBox.Name = "itemPictureBox";
      this.itemPictureBox.Size = new System.Drawing.Size(128, 128);
      this.itemPictureBox.TabIndex = 4;
      this.itemPictureBox.TabStop = false;
      // 
      // bodyLabel
      // 
      this.bodyLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.bodyLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular,
                                                    System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.bodyLabel.Location = new System.Drawing.Point(192, 48);
      this.bodyLabel.Name = "bodyLabel";
      this.bodyLabel.Size = new System.Drawing.Size(276, 360);
      this.bodyLabel.TabIndex = 2;
      this.bodyLabel.Text = "This wizard will walk you through the basic installation of MediaPortal. You can " +
                            "always start \"MediaPortal Setup\" later to change your settings";
      // 
      // Wizard_Welcome
      // 
      this.Controls.Add(this.bodyLabel);
      this.Controls.Add(this.itemPictureBox);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.pictureBox);
      this.Controls.Add(this.headerLabel);
      this.Name = "Wizard_Welcome";
      this.Size = new System.Drawing.Size(472, 408);
      ((System.ComponentModel.ISupportInitialize) (this.pictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.itemPictureBox)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion
  }
}