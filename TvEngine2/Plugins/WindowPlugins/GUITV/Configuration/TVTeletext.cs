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
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.TVE2.Sections
{
  public class TVTeletext : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPRadioButton radioButton1;
    private IContainer components = null;
    private bool _init = false;
    private MPCheckBox cbHiddenMode;
    private MPCheckBox cbTransparentMode;
    private MPCheckBox cbRememberValue;
    private MPLabel FontSizeLbl;
    private MPLabel FontSizeValueLbl;
    private MPNumericUpDown nudFontSize;
    public int pluginVersion;

    public TVTeletext()
      : this("Teletext")
    {
    }

    public TVTeletext(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        LoadSettings();
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.FontSizeValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.FontSizeLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbHiddenMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbRememberValue = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTransparentMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.nudFontSize = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.nudFontSize)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.nudFontSize);
      this.groupBox1.Controls.Add(this.FontSizeValueLbl);
      this.groupBox1.Controls.Add(this.FontSizeLbl);
      this.groupBox1.Controls.Add(this.cbHiddenMode);
      this.groupBox1.Controls.Add(this.cbRememberValue);
      this.groupBox1.Controls.Add(this.cbTransparentMode);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 127);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      this.FontSizeValueLbl.AutoSize = true;
      this.FontSizeValueLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
                                                           System.Drawing.FontStyle.Regular,
                                                           System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.FontSizeValueLbl.Location = new System.Drawing.Point(134, 99);
      this.FontSizeValueLbl.Name = "FontSizeValueLbl";
      this.FontSizeValueLbl.Size = new System.Drawing.Size(105, 13);
      this.FontSizeValueLbl.TabIndex = 3;
      this.FontSizeValueLbl.Text = "% of maximum height";
      this.FontSizeValueLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // FontSizeLbl
      // 
      this.FontSizeLbl.AutoSize = true;
      this.FontSizeLbl.Location = new System.Drawing.Point(14, 99);
      this.FontSizeLbl.Name = "FontSizeLbl";
      this.FontSizeLbl.Size = new System.Drawing.Size(52, 13);
      this.FontSizeLbl.TabIndex = 1;
      this.FontSizeLbl.Text = "Font size:";
      // 
      // cbHiddenMode
      // 
      this.cbHiddenMode.AutoSize = true;
      this.cbHiddenMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbHiddenMode.Location = new System.Drawing.Point(17, 28);
      this.cbHiddenMode.Name = "cbHiddenMode";
      this.cbHiddenMode.Size = new System.Drawing.Size(87, 17);
      this.cbHiddenMode.TabIndex = 0;
      this.cbHiddenMode.Text = "Hidden mode";
      this.cbHiddenMode.UseVisualStyleBackColor = true;
      // 
      // cbRememberValue
      // 
      this.cbRememberValue.AutoSize = true;
      this.cbRememberValue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbRememberValue.Location = new System.Drawing.Point(17, 74);
      this.cbRememberValue.Name = "cbRememberValue";
      this.cbRememberValue.Size = new System.Drawing.Size(123, 17);
      this.cbRememberValue.TabIndex = 2;
      this.cbRememberValue.Text = "Remember last value";
      this.cbRememberValue.UseVisualStyleBackColor = true;
      // 
      // cbTransparentMode
      // 
      this.cbTransparentMode.AutoSize = true;
      this.cbTransparentMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTransparentMode.Location = new System.Drawing.Point(17, 51);
      this.cbTransparentMode.Name = "cbTransparentMode";
      this.cbTransparentMode.Size = new System.Drawing.Size(169, 17);
      this.cbTransparentMode.TabIndex = 1;
      this.cbTransparentMode.Text = "Transparent mode in fullscreen";
      this.cbTransparentMode.UseVisualStyleBackColor = true;
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      this.nudFontSize.Location = new System.Drawing.Point(72, 97);
      this.nudFontSize.Minimum = new decimal(new int[]
                                               {
                                                 50,
                                                 0,
                                                 0,
                                                 0
                                               });
      this.nudFontSize.Name = "nudFontSize";
      this.nudFontSize.Size = new System.Drawing.Size(56, 20);
      this.nudFontSize.TabIndex = 1;
      this.nudFontSize.Value = new decimal(new int[]
                                             {
                                               80,
                                               0,
                                               0,
                                               0
                                             });
      // TVTeletext
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVTeletext";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.nudFontSize)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }


      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbHiddenMode.Checked = xmlreader.GetValueAsBool("mytv", "teletextHidden", false);
        cbTransparentMode.Checked = xmlreader.GetValueAsBool("mytv", "teletextTransparent", false);
        cbRememberValue.Checked = xmlreader.GetValueAsBool("mytv", "teletextRemember", true);
        nudFontSize.Value = xmlreader.GetValueAsInt("mytv", "teletextMaxFontSize", 100);
      }
    }


    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("mytv", "teletextHidden", cbHiddenMode.Checked);
        xmlwriter.SetValueAsBool("mytv", "teletextTransparent", cbTransparentMode.Checked);
        xmlwriter.SetValueAsBool("mytv", "teletextRemember", cbRememberValue.Checked);
        xmlwriter.SetValue("mytv", "teletextMaxFontSize", nudFontSize.Value);
      }
    }
  }
}