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
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class Pictures : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPTextBox durationTextBox;
    private MediaPortal.UserInterface.Controls.MPTextBox transitionTextBox;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonRandom;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonXFade;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonKenBurns;
    private MediaPortal.UserInterface.Controls.MPTextBox kenburnsTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoShuffleCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox repeatSlideshowCheckBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.ComponentModel.IContainer components = null;

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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        durationTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "speed", 3));
        transitionTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "transition", 15));
        kenburnsTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("pictures", "kenburnsspeed", 15));
        radioButtonRandom.Checked = xmlreader.GetValueAsBool("pictures", "random", false);
        radioButtonKenBurns.Checked = xmlreader.GetValueAsBool("pictures", "kenburns", false);
        radioButtonXFade.Checked = !radioButtonRandom.Checked && !radioButtonKenBurns.Checked;

        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
        repeatSlideshowCheckBox.Checked = xmlreader.GetValueAsBool("pictures", "autoRepeat", true);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("pictures", "speed", durationTextBox.Text);
        xmlwriter.SetValue("pictures", "transition", transitionTextBox.Text);
        xmlwriter.SetValue("pictures", "kenburnsspeed", kenburnsTextBox.Text);
        xmlwriter.SetValueAsBool("pictures", "random", radioButtonRandom.Checked);
        xmlwriter.SetValueAsBool("pictures", "kenburns", radioButtonKenBurns.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoShuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("pictures", "autoRepeat", repeatSlideshowCheckBox.Checked);
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
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
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
      this.transitionTextBox.Location = new System.Drawing.Point(168, 44);
      this.transitionTextBox.Name = "transitionTextBox";
      this.transitionTextBox.Size = new System.Drawing.Size(288, 20);
      this.transitionTextBox.TabIndex = 3;
      // 
      // durationTextBox
      // 
      this.durationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      this.radioButtonKenBurns.Location = new System.Drawing.Point(24, 24);
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
      this.radioButtonRandom.Location = new System.Drawing.Point(24, 48);
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
      this.radioButtonXFade.Location = new System.Drawing.Point(24, 72);
      this.radioButtonXFade.Name = "radioButtonXFade";
      this.radioButtonXFade.Size = new System.Drawing.Size(206, 17);
      this.radioButtonXFade.TabIndex = 2;
      this.radioButtonXFade.Text = "Use X-fade transition between pictures";
      this.radioButtonXFade.UseVisualStyleBackColor = true;
      // 
      // Pictures
      // 
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox2);
      this.Name = "Pictures";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}

