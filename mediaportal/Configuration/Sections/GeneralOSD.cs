#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Globalization;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralOSD : MediaPortal.Configuration.SectionSettings
  {
    private System.ComponentModel.IContainer components = null;

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxOSD;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxDisplayTimeout;
    private MediaPortal.UserInterface.Controls.MPLabel labelDisplayTimeout;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlGeneralOSD;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageOSD;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxZapOSD;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxZapDelay;
    private MediaPortal.UserInterface.Controls.MPLabel labelZapDelay;
    private MediaPortal.UserInterface.Controls.MPLabel labelZapTimeOut;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSkipSteps;
    private CheckBox checkBoxStep5;
    private CheckBox checkBoxStep6;
    private CheckBox checkBoxStep14;
    private CheckBox checkBoxStep8;
    private CheckBox checkBoxStep13;
    private CheckBox checkBoxStep7;
    private CheckBox checkBoxStep3;
    private CheckBox checkBoxStep2;
    private CheckBox checkBoxStep1;
    private CheckBox checkBoxStep10;
    private CheckBox checkBoxStep9;
    private CheckBox checkBoxStep12;
    private CheckBox checkBoxStep11;
    private CheckBox checkBoxStep15;
    private CheckBox checkBoxStep4;
    private Button buttonResetSkipSteps;
    private CheckBox checkBoxStep16;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxZapTimeout;

    public GeneralOSD()
      : this("On-Screen Display")
    {
    }

    public GeneralOSD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      ArrayList StepArray = new ArrayList();
      string regValue;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textBoxDisplayTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));
        textBoxZapDelay.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2));
        textBoxZapTimeout.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5));
        try
          {
          regValue = xmlreader.GetValueAsString("movieplayer", "skipsteps", "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1;0");
          if ( regValue == String.Empty ) // config after wizard run 1st
            {
            regValue = "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1;0";
            MediaPortal.GUI.Library.Log.Write("GeneralOSD - creating new Skip-Settings {0}", "");
            }
          foreach ( string token in regValue.Split(new char[] { ',', ';', ' ' }) )
            {
            if ( token == string.Empty )
              StepArray.Add(0);
            else
              StepArray.Add(Convert.ToInt16(token));
            }
          checkBoxStep1.Checked = Convert.ToBoolean(StepArray[0]);
          checkBoxStep2.Checked = Convert.ToBoolean(StepArray[1]);
          checkBoxStep3.Checked = Convert.ToBoolean(StepArray[2]);
          checkBoxStep4.Checked = Convert.ToBoolean(StepArray[3]);
          checkBoxStep5.Checked = Convert.ToBoolean(StepArray[4]);
          checkBoxStep6.Checked = Convert.ToBoolean(StepArray[5]);
          checkBoxStep7.Checked = Convert.ToBoolean(StepArray[6]);
          checkBoxStep8.Checked = Convert.ToBoolean(StepArray[7]);
          checkBoxStep9.Checked = Convert.ToBoolean(StepArray[8]);
          checkBoxStep10.Checked = Convert.ToBoolean(StepArray[9]);
          checkBoxStep11.Checked = Convert.ToBoolean(StepArray[10]);
          checkBoxStep12.Checked = Convert.ToBoolean(StepArray[11]);
          checkBoxStep13.Checked = Convert.ToBoolean(StepArray[12]);
          checkBoxStep14.Checked = Convert.ToBoolean(StepArray[13]);
          checkBoxStep15.Checked = Convert.ToBoolean(StepArray[14]);
          checkBoxStep16.Checked = Convert.ToBoolean(StepArray[15]);
          }
        catch (Exception ex)
          {
            MediaPortal.GUI.Library.Log.Write("GeneralOSD - Exception while loading Skip-Settings: {0}", ex.ToString());
          }
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("movieplayer", "osdtimeout", textBoxDisplayTimeout.Text);
        xmlwriter.SetValue("movieplayer", "zapdelay", textBoxZapDelay.Text);
        xmlwriter.SetValue("movieplayer", "zaptimeout", textBoxZapTimeout.Text);

        string skipSteps = ( Convert.ToInt16(checkBoxStep1.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep2.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep3.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep4.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep5.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep6.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep7.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep8.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep9.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep10.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep11.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep12.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep13.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep14.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep15.Checked) ).ToString() + ";" +
                           ( Convert.ToInt16(checkBoxStep16.Checked) ).ToString();
        xmlwriter.SetValue("movieplayer", "skipsteps", skipSteps);
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
    this.groupBoxOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this.textBoxDisplayTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
    this.labelDisplayTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
    this.tabControlGeneralOSD = new MediaPortal.UserInterface.Controls.MPTabControl();
    this.tabPageOSD = new MediaPortal.UserInterface.Controls.MPTabPage();
    this.groupBoxSkipSteps = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this.buttonResetSkipSteps = new System.Windows.Forms.Button();
    this.checkBoxStep4 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep15 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep12 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep11 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep10 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep9 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep5 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep6 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep14 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep8 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep13 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep7 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep3 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep2 = new System.Windows.Forms.CheckBox();
    this.checkBoxStep1 = new System.Windows.Forms.CheckBox();
    this.groupBoxZapOSD = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this.textBoxZapTimeout = new MediaPortal.UserInterface.Controls.MPTextBox();
    this.labelZapTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
    this.textBoxZapDelay = new MediaPortal.UserInterface.Controls.MPTextBox();
    this.labelZapDelay = new MediaPortal.UserInterface.Controls.MPLabel();
    this.checkBoxStep16 = new System.Windows.Forms.CheckBox();
    this.groupBoxOSD.SuspendLayout();
    this.tabControlGeneralOSD.SuspendLayout();
    this.tabPageOSD.SuspendLayout();
    this.groupBoxSkipSteps.SuspendLayout();
    this.groupBoxZapOSD.SuspendLayout();
    this.SuspendLayout();
    // 
    // groupBoxOSD
    // 
    this.groupBoxOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.groupBoxOSD.Controls.Add(this.textBoxDisplayTimeout);
    this.groupBoxOSD.Controls.Add(this.labelDisplayTimeout);
    this.groupBoxOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBoxOSD.Location = new System.Drawing.Point(16, 16);
    this.groupBoxOSD.Name = "groupBoxOSD";
    this.groupBoxOSD.Size = new System.Drawing.Size(432, 50);
    this.groupBoxOSD.TabIndex = 0;
    this.groupBoxOSD.TabStop = false;
    this.groupBoxOSD.Text = "On-Screen Display";
    // 
    // textBoxDisplayTimeout
    // 
    this.textBoxDisplayTimeout.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.textBoxDisplayTimeout.Location = new System.Drawing.Point(160, 20);
    this.textBoxDisplayTimeout.Name = "textBoxDisplayTimeout";
    this.textBoxDisplayTimeout.Size = new System.Drawing.Size(256, 20);
    this.textBoxDisplayTimeout.TabIndex = 1;
    // 
    // labelDisplayTimeout
    // 
    this.labelDisplayTimeout.AutoSize = true;
    this.labelDisplayTimeout.Location = new System.Drawing.Point(16, 24);
    this.labelDisplayTimeout.Name = "labelDisplayTimeout";
    this.labelDisplayTimeout.Size = new System.Drawing.Size(110, 13);
    this.labelDisplayTimeout.TabIndex = 0;
    this.labelDisplayTimeout.Text = "Display timeout (sec.):";
    // 
    // tabControlGeneralOSD
    // 
    this.tabControlGeneralOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.tabControlGeneralOSD.Controls.Add(this.tabPageOSD);
    this.tabControlGeneralOSD.Location = new System.Drawing.Point(0, 0);
    this.tabControlGeneralOSD.Name = "tabControlGeneralOSD";
    this.tabControlGeneralOSD.SelectedIndex = 0;
    this.tabControlGeneralOSD.Size = new System.Drawing.Size(472, 408);
    this.tabControlGeneralOSD.TabIndex = 0;
    // 
    // tabPageOSD
    // 
    this.tabPageOSD.Controls.Add(this.groupBoxSkipSteps);
    this.tabPageOSD.Controls.Add(this.groupBoxZapOSD);
    this.tabPageOSD.Controls.Add(this.groupBoxOSD);
    this.tabPageOSD.Location = new System.Drawing.Point(4, 22);
    this.tabPageOSD.Name = "tabPageOSD";
    this.tabPageOSD.Size = new System.Drawing.Size(464, 382);
    this.tabPageOSD.TabIndex = 3;
    this.tabPageOSD.Text = "On-Screen Display";
    this.tabPageOSD.UseVisualStyleBackColor = true;
    // 
    // groupBoxSkipSteps
    // 
    this.groupBoxSkipSteps.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep16);
    this.groupBoxSkipSteps.Controls.Add(this.buttonResetSkipSteps);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep4);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep15);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep12);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep11);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep10);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep9);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep5);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep6);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep14);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep8);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep13);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep7);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep3);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep2);
    this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep1);
    this.groupBoxSkipSteps.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBoxSkipSteps.Location = new System.Drawing.Point(16, 152);
    this.groupBoxSkipSteps.Name = "groupBoxSkipSteps";
    this.groupBoxSkipSteps.Size = new System.Drawing.Size(432, 213);
    this.groupBoxSkipSteps.TabIndex = 2;
    this.groupBoxSkipSteps.TabStop = false;
    this.groupBoxSkipSteps.Text = "Skip steps";
    // 
    // buttonResetSkipSteps
    // 
    this.buttonResetSkipSteps.Location = new System.Drawing.Point(341, 17);
    this.buttonResetSkipSteps.Name = "buttonResetSkipSteps";
    this.buttonResetSkipSteps.Size = new System.Drawing.Size(75, 23);
    this.buttonResetSkipSteps.TabIndex = 15;
    this.buttonResetSkipSteps.Text = "Defaults";
    this.buttonResetSkipSteps.UseVisualStyleBackColor = true;
    this.buttonResetSkipSteps.Click += new System.EventHandler(this.buttonResetSkipSteps_Click);
    // 
    // checkBoxStep4
    // 
    this.checkBoxStep4.AutoSize = true;
    this.checkBoxStep4.Location = new System.Drawing.Point(19, 90);
    this.checkBoxStep4.Name = "checkBoxStep4";
    this.checkBoxStep4.Size = new System.Drawing.Size(75, 17);
    this.checkBoxStep4.TabIndex = 14;
    this.checkBoxStep4.Text = "+/- 45 sec";
    this.checkBoxStep4.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep15
    // 
    this.checkBoxStep15.AutoSize = true;
    this.checkBoxStep15.Checked = true;
    this.checkBoxStep15.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep15.Location = new System.Drawing.Point(214, 67);
    this.checkBoxStep15.Name = "checkBoxStep15";
    this.checkBoxStep15.Size = new System.Drawing.Size(58, 17);
    this.checkBoxStep15.TabIndex = 13;
    this.checkBoxStep15.Text = "+/- 2 h";
    this.checkBoxStep15.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep12
    // 
    this.checkBoxStep12.AutoSize = true;
    this.checkBoxStep12.Location = new System.Drawing.Point(117, 182);
    this.checkBoxStep12.Name = "checkBoxStep12";
    this.checkBoxStep12.Size = new System.Drawing.Size(74, 17);
    this.checkBoxStep12.TabIndex = 12;
    this.checkBoxStep12.Text = "+/- 45 min";
    this.checkBoxStep12.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep11
    // 
    this.checkBoxStep11.AutoSize = true;
    this.checkBoxStep11.Checked = true;
    this.checkBoxStep11.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep11.Location = new System.Drawing.Point(117, 159);
    this.checkBoxStep11.Name = "checkBoxStep11";
    this.checkBoxStep11.Size = new System.Drawing.Size(74, 17);
    this.checkBoxStep11.TabIndex = 11;
    this.checkBoxStep11.Text = "+/- 30 min";
    this.checkBoxStep11.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep10
    // 
    this.checkBoxStep10.AutoSize = true;
    this.checkBoxStep10.Checked = true;
    this.checkBoxStep10.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep10.Location = new System.Drawing.Point(117, 136);
    this.checkBoxStep10.Name = "checkBoxStep10";
    this.checkBoxStep10.Size = new System.Drawing.Size(74, 17);
    this.checkBoxStep10.TabIndex = 10;
    this.checkBoxStep10.Text = "+/- 15 min";
    this.checkBoxStep10.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep9
    // 
    this.checkBoxStep9.AutoSize = true;
    this.checkBoxStep9.Checked = true;
    this.checkBoxStep9.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep9.Location = new System.Drawing.Point(117, 113);
    this.checkBoxStep9.Name = "checkBoxStep9";
    this.checkBoxStep9.Size = new System.Drawing.Size(74, 17);
    this.checkBoxStep9.TabIndex = 9;
    this.checkBoxStep9.Text = "+/- 10 min";
    this.checkBoxStep9.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep5
    // 
    this.checkBoxStep5.AutoSize = true;
    this.checkBoxStep5.Checked = true;
    this.checkBoxStep5.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep5.Location = new System.Drawing.Point(117, 21);
    this.checkBoxStep5.Name = "checkBoxStep5";
    this.checkBoxStep5.Size = new System.Drawing.Size(68, 17);
    this.checkBoxStep5.TabIndex = 8;
    this.checkBoxStep5.Text = "+/- 1 min";
    this.checkBoxStep5.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep6
    // 
    this.checkBoxStep6.AutoSize = true;
    this.checkBoxStep6.Checked = true;
    this.checkBoxStep6.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep6.Location = new System.Drawing.Point(117, 44);
    this.checkBoxStep6.Name = "checkBoxStep6";
    this.checkBoxStep6.Size = new System.Drawing.Size(68, 17);
    this.checkBoxStep6.TabIndex = 7;
    this.checkBoxStep6.Text = "+/- 3 min";
    this.checkBoxStep6.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep14
    // 
    this.checkBoxStep14.AutoSize = true;
    this.checkBoxStep14.Location = new System.Drawing.Point(214, 44);
    this.checkBoxStep14.Name = "checkBoxStep14";
    this.checkBoxStep14.Size = new System.Drawing.Size(67, 17);
    this.checkBoxStep14.TabIndex = 6;
    this.checkBoxStep14.Text = "+/- 1,5 h";
    this.checkBoxStep14.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep8
    // 
    this.checkBoxStep8.AutoSize = true;
    this.checkBoxStep8.Location = new System.Drawing.Point(117, 90);
    this.checkBoxStep8.Name = "checkBoxStep8";
    this.checkBoxStep8.Size = new System.Drawing.Size(68, 17);
    this.checkBoxStep8.TabIndex = 5;
    this.checkBoxStep8.Text = "+/- 7 min";
    this.checkBoxStep8.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep13
    // 
    this.checkBoxStep13.AutoSize = true;
    this.checkBoxStep13.Checked = true;
    this.checkBoxStep13.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep13.Location = new System.Drawing.Point(214, 21);
    this.checkBoxStep13.Name = "checkBoxStep13";
    this.checkBoxStep13.Size = new System.Drawing.Size(58, 17);
    this.checkBoxStep13.TabIndex = 4;
    this.checkBoxStep13.Text = "+/- 1 h";
    this.checkBoxStep13.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep7
    // 
    this.checkBoxStep7.AutoSize = true;
    this.checkBoxStep7.Checked = true;
    this.checkBoxStep7.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep7.Location = new System.Drawing.Point(117, 67);
    this.checkBoxStep7.Name = "checkBoxStep7";
    this.checkBoxStep7.Size = new System.Drawing.Size(68, 17);
    this.checkBoxStep7.TabIndex = 3;
    this.checkBoxStep7.Text = "+/- 5 min";
    this.checkBoxStep7.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep3
    // 
    this.checkBoxStep3.AutoSize = true;
    this.checkBoxStep3.Checked = true;
    this.checkBoxStep3.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep3.Location = new System.Drawing.Point(19, 67);
    this.checkBoxStep3.Name = "checkBoxStep3";
    this.checkBoxStep3.Size = new System.Drawing.Size(75, 17);
    this.checkBoxStep3.TabIndex = 2;
    this.checkBoxStep3.Text = "+/- 30 sec";
    this.checkBoxStep3.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep2
    // 
    this.checkBoxStep2.AutoSize = true;
    this.checkBoxStep2.Checked = true;
    this.checkBoxStep2.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep2.Location = new System.Drawing.Point(19, 44);
    this.checkBoxStep2.Name = "checkBoxStep2";
    this.checkBoxStep2.Size = new System.Drawing.Size(75, 17);
    this.checkBoxStep2.TabIndex = 1;
    this.checkBoxStep2.Text = "+/- 15 sec";
    this.checkBoxStep2.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep1
    // 
    this.checkBoxStep1.AutoSize = true;
    this.checkBoxStep1.Location = new System.Drawing.Point(19, 21);
    this.checkBoxStep1.Name = "checkBoxStep1";
    this.checkBoxStep1.Size = new System.Drawing.Size(69, 17);
    this.checkBoxStep1.TabIndex = 0;
    this.checkBoxStep1.Text = "+/- 5 sec";
    this.checkBoxStep1.UseVisualStyleBackColor = true;
    // 
    // groupBoxZapOSD
    // 
    this.groupBoxZapOSD.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.groupBoxZapOSD.Controls.Add(this.textBoxZapTimeout);
    this.groupBoxZapOSD.Controls.Add(this.labelZapTimeOut);
    this.groupBoxZapOSD.Controls.Add(this.textBoxZapDelay);
    this.groupBoxZapOSD.Controls.Add(this.labelZapDelay);
    this.groupBoxZapOSD.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.groupBoxZapOSD.Location = new System.Drawing.Point(16, 72);
    this.groupBoxZapOSD.Name = "groupBoxZapOSD";
    this.groupBoxZapOSD.Size = new System.Drawing.Size(432, 74);
    this.groupBoxZapOSD.TabIndex = 1;
    this.groupBoxZapOSD.TabStop = false;
    this.groupBoxZapOSD.Text = "Zap On-Screen Display";
    // 
    // textBoxZapTimeout
    // 
    this.textBoxZapTimeout.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.textBoxZapTimeout.Location = new System.Drawing.Point(160, 44);
    this.textBoxZapTimeout.Name = "textBoxZapTimeout";
    this.textBoxZapTimeout.Size = new System.Drawing.Size(256, 20);
    this.textBoxZapTimeout.TabIndex = 3;
    // 
    // labelZapTimeOut
    // 
    this.labelZapTimeOut.AutoSize = true;
    this.labelZapTimeOut.Location = new System.Drawing.Point(16, 48);
    this.labelZapTimeOut.Name = "labelZapTimeOut";
    this.labelZapTimeOut.Size = new System.Drawing.Size(95, 13);
    this.labelZapTimeOut.TabIndex = 2;
    this.labelZapTimeOut.Text = "Zap timeout (sec.):";
    // 
    // textBoxZapDelay
    // 
    this.textBoxZapDelay.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                | System.Windows.Forms.AnchorStyles.Right ) ) );
    this.textBoxZapDelay.Location = new System.Drawing.Point(160, 20);
    this.textBoxZapDelay.Name = "textBoxZapDelay";
    this.textBoxZapDelay.Size = new System.Drawing.Size(256, 20);
    this.textBoxZapDelay.TabIndex = 1;
    // 
    // labelZapDelay
    // 
    this.labelZapDelay.AutoSize = true;
    this.labelZapDelay.Location = new System.Drawing.Point(16, 24);
    this.labelZapDelay.Name = "labelZapDelay";
    this.labelZapDelay.Size = new System.Drawing.Size(86, 13);
    this.labelZapDelay.TabIndex = 0;
    this.labelZapDelay.Text = "Zap delay (sec.):";
    // 
    // checkBoxStep16
    // 
    this.checkBoxStep16.AutoSize = true;
    this.checkBoxStep16.Location = new System.Drawing.Point(214, 90);
    this.checkBoxStep16.Name = "checkBoxStep16";
    this.checkBoxStep16.Size = new System.Drawing.Size(58, 17);
    this.checkBoxStep16.TabIndex = 16;
    this.checkBoxStep16.Text = "+/- 3 h";
    this.checkBoxStep16.UseVisualStyleBackColor = true;
    // 
    // GeneralOSD
    // 
    this.Controls.Add(this.tabControlGeneralOSD);
    this.Name = "GeneralOSD";
    this.Size = new System.Drawing.Size(472, 408);
    this.groupBoxOSD.ResumeLayout(false);
    this.groupBoxOSD.PerformLayout();
    this.tabControlGeneralOSD.ResumeLayout(false);
    this.tabPageOSD.ResumeLayout(false);
    this.groupBoxSkipSteps.ResumeLayout(false);
    this.groupBoxSkipSteps.PerformLayout();
    this.groupBoxZapOSD.ResumeLayout(false);
    this.groupBoxZapOSD.PerformLayout();
    this.ResumeLayout(false);

    }
    #endregion
    // "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1"
    private void buttonResetSkipSteps_Click(object sender, EventArgs e)
    {
      checkBoxStep1.Checked = false;
      checkBoxStep2.Checked = true;
      checkBoxStep3.Checked = true;
      checkBoxStep4.Checked = false;
      checkBoxStep5.Checked = true;
      checkBoxStep6.Checked = true;
      checkBoxStep7.Checked = true;
      checkBoxStep8.Checked = false;
      checkBoxStep9.Checked = true;
      checkBoxStep10.Checked = true;
      checkBoxStep11.Checked = true;
      checkBoxStep12.Checked = false;
      checkBoxStep13.Checked = true;
      checkBoxStep14.Checked = false;
      checkBoxStep15.Checked = true;
      checkBoxStep16.Checked = false;
    }

  }
}