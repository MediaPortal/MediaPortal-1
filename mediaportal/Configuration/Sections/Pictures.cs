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

using System;
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Pictures : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPLabel label1;
    private MPLabel label2;
    private MPTextBox durationTextBox;
    private MPTextBox transitionTextBox;
    private MPRadioButton radioButtonRandom;
    private MPRadioButton radioButtonXFade;
    private MPLabel label3;
    private MPRadioButton radioButtonKenBurns;
    private MPTextBox kenburnsTextBox;
    private MPCheckBox autoShuffleCheckBox;
    private MPCheckBox repeatSlideshowCheckBox;
    private MPGroupBox groupBox2;
    private MPGroupBox groupBoxRotation;
    private MPCheckBox checkBoxUsePicasa;
    private MPCheckBox checkBoxUseExif;
    private IContainer components = null;

    public Pictures()
      : this("Pictures")
    {
    }

    public Pictures(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        durationTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "speed", 3));
        transitionTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "transition", 15));
        kenburnsTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "kenburnsspeed", 15));
        radioButtonRandom.Checked = xmlreader.GetValueAsBool("pictures", "random", false);
        radioButtonKenBurns.Checked = xmlreader.GetValueAsBool("pictures", "kenburns", false);
        radioButtonXFade.Checked = !radioButtonRandom.Checked && !radioButtonKenBurns.Checked;

        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
        repeatSlideshowCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoRepeat", true);

        checkBoxUseExif.Checked = xmlreader.GetValueAsBool("pictures", "useExif", true);
        checkBoxUsePicasa.Checked = xmlreader.GetValueAsBool("pictures", "usePicasa", false);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("pictures", "speed", durationTextBox.Text);
        xmlwriter.SetValue("pictures", "transition", transitionTextBox.Text);
        xmlwriter.SetValue("pictures", "kenburnsspeed", kenburnsTextBox.Text);
        xmlwriter.SetValueAsBool("pictures", "random", radioButtonRandom.Checked);
        xmlwriter.SetValueAsBool("pictures", "kenburns", radioButtonKenBurns.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoShuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoRepeat", repeatSlideshowCheckBox.Checked);
        xmlwriter.SetValueAsBool("pictures", "useExif", checkBoxUseExif.Checked);
        xmlwriter.SetValueAsBool("pictures", "usePicasa", checkBoxUseExif.Checked);
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
      this.kenburnsTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.transitionTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.durationTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.repeatSlideshowCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonKenBurns = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonRandom = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonXFade = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxRotation = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxUseExif = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUsePicasa = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBoxRotation.SuspendLayout();
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
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 112);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 160);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Slideshow Settings";
      // 
      // kenburnsTextBox
      // 
      this.kenburnsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.kenburnsTextBox.BorderColor = System.Drawing.Color.Empty;
      this.kenburnsTextBox.Location = new System.Drawing.Point(168, 68);
      this.kenburnsTextBox.Name = "kenburnsTextBox";
      this.kenburnsTextBox.Size = new System.Drawing.Size(288, 20);
      this.kenburnsTextBox.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(91, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Ken Burns speed:";
      // 
      // transitionTextBox
      // 
      this.transitionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.transitionTextBox.BorderColor = System.Drawing.Color.Empty;
      this.transitionTextBox.Location = new System.Drawing.Point(168, 44);
      this.transitionTextBox.Name = "transitionTextBox";
      this.transitionTextBox.Size = new System.Drawing.Size(288, 20);
      this.transitionTextBox.TabIndex = 3;
      // 
      // durationTextBox
      // 
      this.durationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.durationTextBox.BorderColor = System.Drawing.Color.Empty;
      this.durationTextBox.Location = new System.Drawing.Point(168, 20);
      this.durationTextBox.Name = "durationTextBox";
      this.durationTextBox.Size = new System.Drawing.Size(288, 20);
      this.durationTextBox.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Transition (frames):";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(124, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Picture visible (seconds):";
      // 
      // repeatSlideshowCheckBox
      // 
      this.repeatSlideshowCheckBox.AutoSize = true;
      this.repeatSlideshowCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.repeatSlideshowCheckBox.Location = new System.Drawing.Point(16, 104);
      this.repeatSlideshowCheckBox.Name = "repeatSlideshowCheckBox";
      this.repeatSlideshowCheckBox.Size = new System.Drawing.Size(133, 17);
      this.repeatSlideshowCheckBox.TabIndex = 6;
      this.repeatSlideshowCheckBox.Text = "Repeat/loop slideshow";
      this.repeatSlideshowCheckBox.UseVisualStyleBackColor = true;
      // 
      // autoShuffleCheckBox
      // 
      this.autoShuffleCheckBox.AutoSize = true;
      this.autoShuffleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoShuffleCheckBox.Location = new System.Drawing.Point(16, 128);
      this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
      this.autoShuffleCheckBox.Size = new System.Drawing.Size(129, 17);
      this.autoShuffleCheckBox.TabIndex = 7;
      this.autoShuffleCheckBox.Text = "Auto shuffle slideshow";
      this.autoShuffleCheckBox.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.radioButtonKenBurns);
      this.groupBox2.Controls.Add(this.radioButtonRandom);
      this.groupBox2.Controls.Add(this.radioButtonXFade);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 104);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Slideshow Transitions";
      // 
      // radioButtonKenBurns
      // 
      this.radioButtonKenBurns.AutoSize = true;
      this.radioButtonKenBurns.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonKenBurns.Location = new System.Drawing.Point(16, 23);
      this.radioButtonKenBurns.Name = "radioButtonKenBurns";
      this.radioButtonKenBurns.Size = new System.Drawing.Size(180, 17);
      this.radioButtonKenBurns.TabIndex = 0;
      this.radioButtonKenBurns.Text = "Use Ken Burns effect on pictures";
      this.radioButtonKenBurns.UseVisualStyleBackColor = true;
      // 
      // radioButtonRandom
      // 
      this.radioButtonRandom.AutoSize = true;
      this.radioButtonRandom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonRandom.Location = new System.Drawing.Point(16, 47);
      this.radioButtonRandom.Name = "radioButtonRandom";
      this.radioButtonRandom.Size = new System.Drawing.Size(215, 17);
      this.radioButtonRandom.TabIndex = 1;
      this.radioButtonRandom.Text = "Use random transitions between pictures";
      this.radioButtonRandom.UseVisualStyleBackColor = true;
      // 
      // radioButtonXFade
      // 
      this.radioButtonXFade.AutoSize = true;
      this.radioButtonXFade.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonXFade.Location = new System.Drawing.Point(16, 71);
      this.radioButtonXFade.Name = "radioButtonXFade";
      this.radioButtonXFade.Size = new System.Drawing.Size(206, 17);
      this.radioButtonXFade.TabIndex = 2;
      this.radioButtonXFade.Text = "Use X-fade transition between pictures";
      this.radioButtonXFade.UseVisualStyleBackColor = true;
      // 
      // groupBoxRotation
      // 
      this.groupBoxRotation.Controls.Add(this.checkBoxUsePicasa);
      this.groupBoxRotation.Controls.Add(this.checkBoxUseExif);
      this.groupBoxRotation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxRotation.Location = new System.Drawing.Point(0, 278);
      this.groupBoxRotation.Name = "groupBoxRotation";
      this.groupBoxRotation.Size = new System.Drawing.Size(472, 78);
      this.groupBoxRotation.TabIndex = 2;
      this.groupBoxRotation.TabStop = false;
      this.groupBoxRotation.Text = "Rotation Settings";
      // 
      // checkBoxUseExif
      // 
      this.checkBoxUseExif.AutoSize = true;
      this.checkBoxUseExif.Checked = true;
      this.checkBoxUseExif.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseExif.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseExif.Location = new System.Drawing.Point(16, 28);
      this.checkBoxUseExif.Name = "checkBoxUseExif";
      this.checkBoxUseExif.Size = new System.Drawing.Size(401, 17);
      this.checkBoxUseExif.TabIndex = 7;
      this.checkBoxUseExif.Text = "Use EXIF metadata to determine the rotation (might interfere with buggy viewers)";
      this.checkBoxUseExif.UseVisualStyleBackColor = true;
      // 
      // checkBoxUsePicasa
      // 
      this.checkBoxUsePicasa.AutoSize = true;
      this.checkBoxUsePicasa.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUsePicasa.Location = new System.Drawing.Point(16, 51);
      this.checkBoxUsePicasa.Name = "checkBoxUsePicasa";
      this.checkBoxUsePicasa.Size = new System.Drawing.Size(330, 17);
      this.checkBoxUsePicasa.TabIndex = 8;
      this.checkBoxUsePicasa.Text = "Use Google Picasa.ini to determine the rotation (if file is available)";
      this.checkBoxUsePicasa.UseVisualStyleBackColor = true;
      // 
      // Pictures
      // 
      this.Controls.Add(this.groupBoxRotation);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox2);
      this.Name = "Pictures";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBoxRotation.ResumeLayout(false);
      this.groupBoxRotation.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion
  }
}