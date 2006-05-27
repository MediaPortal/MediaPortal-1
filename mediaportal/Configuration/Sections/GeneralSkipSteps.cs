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
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
  {
  public class GeneralSkipSteps : MediaPortal.Configuration.SectionSettings
    {
    private System.ComponentModel.IContainer components = null;

    private MediaPortal.UserInterface.Controls.MPTabControl tabControlSkipSteps;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSkipSteps;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep16;
    private MediaPortal.UserInterface.Controls.MPButton buttonResetSkipSteps;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep4;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep15;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep12;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep11;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep10;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep9;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep5;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep6;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep14;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep8;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep13;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep7;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep3;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep2;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxStep1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageSteps;

    

    public GeneralSkipSteps()
      : this("Skip steps")
      {
      }

    public GeneralSkipSteps( string name )
      : base(name)
      {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      }

    public override void LoadSettings()
      {
      ArrayList StepArray = new ArrayList();
      string regValue;

      using ( MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml") )
        {
        try
          {
          regValue = xmlreader.GetValueAsString("movieplayer", "skipsteps", "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1;0");
          if ( regValue == String.Empty ) // config after wizard run 1st
            {
            regValue = "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1;0";
            MediaPortal.GUI.Library.Log.Write("GeneralSkipSteps - creating new Skip-Settings {0}", "");
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
        catch ( Exception ex )
          {
          MediaPortal.GUI.Library.Log.Write("GeneralSkipSteps - Exception while loading Skip-Settings: {0}", ex.ToString());
          }
        }
      }

    public override void SaveSettings()
      {
      using ( MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml") )
        {
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

    protected override void Dispose( bool disposing )
      {
      if ( disposing )
        {
        if ( components != null )
          {
          components.Dispose();
          }
        }
      base.Dispose(disposing);
      }

    private void buttonResetSkipSteps_Click( object sender, EventArgs e )
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

    private void InitializeComponent()
    {
    this.tabControlSkipSteps = new MediaPortal.UserInterface.Controls.MPTabControl();
    this.tabPageSteps = new MediaPortal.UserInterface.Controls.MPTabPage();
    this.groupBoxSkipSteps = new MediaPortal.UserInterface.Controls.MPGroupBox();
    this.checkBoxStep16 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.buttonResetSkipSteps = new MediaPortal.UserInterface.Controls.MPButton();
    this.checkBoxStep4 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep15 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep12 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep11 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep10 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep9 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep5 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep6 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep14 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep8 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep13 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep7 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.checkBoxStep1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
    this.tabControlSkipSteps.SuspendLayout();
    this.tabPageSteps.SuspendLayout();
    this.groupBoxSkipSteps.SuspendLayout();
    this.SuspendLayout();
    // 
    // tabControlSkipSteps
    // 
    this.tabControlSkipSteps.Controls.Add(this.tabPageSteps);
    this.tabControlSkipSteps.Location = new System.Drawing.Point(0, 0);
    this.tabControlSkipSteps.Name = "tabControlSkipSteps";
    this.tabControlSkipSteps.SelectedIndex = 0;
    this.tabControlSkipSteps.Size = new System.Drawing.Size(472, 408);
    this.tabControlSkipSteps.TabIndex = 0;
    // 
    // tabPageSteps
    // 
    this.tabPageSteps.Controls.Add(this.groupBoxSkipSteps);
    this.tabPageSteps.Location = new System.Drawing.Point(4, 22);
    this.tabPageSteps.Name = "tabPageSteps";
    this.tabPageSteps.Padding = new System.Windows.Forms.Padding(3);
    this.tabPageSteps.Size = new System.Drawing.Size(464, 382);
    this.tabPageSteps.TabIndex = 0;
    this.tabPageSteps.Text = "Player steps";
    this.tabPageSteps.UseVisualStyleBackColor = true;
    // 
    // groupBoxSkipSteps
    // 
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
    this.groupBoxSkipSteps.Location = new System.Drawing.Point(16, 16);
    this.groupBoxSkipSteps.Name = "groupBoxSkipSteps";
    this.groupBoxSkipSteps.Size = new System.Drawing.Size(428, 223);
    this.groupBoxSkipSteps.TabIndex = 0;
    this.groupBoxSkipSteps.TabStop = false;
    this.groupBoxSkipSteps.Text = "Skip steps (left / right)";
    // 
    // checkBoxStep16
    // 
    this.checkBoxStep16.AutoSize = true;
    this.checkBoxStep16.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep16.Location = new System.Drawing.Point(210, 98);
    this.checkBoxStep16.Name = "checkBoxStep16";
    this.checkBoxStep16.Size = new System.Drawing.Size(56, 17);
    this.checkBoxStep16.TabIndex = 50;
    this.checkBoxStep16.Text = "+/- 3 h";
    this.checkBoxStep16.UseVisualStyleBackColor = true;
    // 
    // buttonResetSkipSteps
    // 
    this.buttonResetSkipSteps.Location = new System.Drawing.Point(338, 26);
    this.buttonResetSkipSteps.Name = "buttonResetSkipSteps";
    this.buttonResetSkipSteps.Size = new System.Drawing.Size(75, 23);
    this.buttonResetSkipSteps.TabIndex = 49;
    this.buttonResetSkipSteps.Text = "Defaults";
    this.buttonResetSkipSteps.UseVisualStyleBackColor = true;
    this.buttonResetSkipSteps.Click += new System.EventHandler(this.buttonResetSkipSteps_Click);
    // 
    // checkBoxStep4
    // 
    this.checkBoxStep4.AutoSize = true;
    this.checkBoxStep4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep4.Location = new System.Drawing.Point(15, 98);
    this.checkBoxStep4.Name = "checkBoxStep4";
    this.checkBoxStep4.Size = new System.Drawing.Size(73, 17);
    this.checkBoxStep4.TabIndex = 48;
    this.checkBoxStep4.Text = "+/- 45 sec";
    this.checkBoxStep4.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep15
    // 
    this.checkBoxStep15.AutoSize = true;
    this.checkBoxStep15.Checked = true;
    this.checkBoxStep15.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep15.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep15.Location = new System.Drawing.Point(210, 75);
    this.checkBoxStep15.Name = "checkBoxStep15";
    this.checkBoxStep15.Size = new System.Drawing.Size(56, 17);
    this.checkBoxStep15.TabIndex = 47;
    this.checkBoxStep15.Text = "+/- 2 h";
    this.checkBoxStep15.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep12
    // 
    this.checkBoxStep12.AutoSize = true;
    this.checkBoxStep12.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep12.Location = new System.Drawing.Point(113, 190);
    this.checkBoxStep12.Name = "checkBoxStep12";
    this.checkBoxStep12.Size = new System.Drawing.Size(72, 17);
    this.checkBoxStep12.TabIndex = 46;
    this.checkBoxStep12.Text = "+/- 45 min";
    this.checkBoxStep12.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep11
    // 
    this.checkBoxStep11.AutoSize = true;
    this.checkBoxStep11.Checked = true;
    this.checkBoxStep11.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep11.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep11.Location = new System.Drawing.Point(113, 167);
    this.checkBoxStep11.Name = "checkBoxStep11";
    this.checkBoxStep11.Size = new System.Drawing.Size(72, 17);
    this.checkBoxStep11.TabIndex = 45;
    this.checkBoxStep11.Text = "+/- 30 min";
    this.checkBoxStep11.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep10
    // 
    this.checkBoxStep10.AutoSize = true;
    this.checkBoxStep10.Checked = true;
    this.checkBoxStep10.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep10.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep10.Location = new System.Drawing.Point(113, 144);
    this.checkBoxStep10.Name = "checkBoxStep10";
    this.checkBoxStep10.Size = new System.Drawing.Size(72, 17);
    this.checkBoxStep10.TabIndex = 44;
    this.checkBoxStep10.Text = "+/- 15 min";
    this.checkBoxStep10.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep9
    // 
    this.checkBoxStep9.AutoSize = true;
    this.checkBoxStep9.Checked = true;
    this.checkBoxStep9.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep9.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep9.Location = new System.Drawing.Point(113, 121);
    this.checkBoxStep9.Name = "checkBoxStep9";
    this.checkBoxStep9.Size = new System.Drawing.Size(72, 17);
    this.checkBoxStep9.TabIndex = 43;
    this.checkBoxStep9.Text = "+/- 10 min";
    this.checkBoxStep9.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep5
    // 
    this.checkBoxStep5.AutoSize = true;
    this.checkBoxStep5.Checked = true;
    this.checkBoxStep5.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep5.Location = new System.Drawing.Point(113, 29);
    this.checkBoxStep5.Name = "checkBoxStep5";
    this.checkBoxStep5.Size = new System.Drawing.Size(66, 17);
    this.checkBoxStep5.TabIndex = 42;
    this.checkBoxStep5.Text = "+/- 1 min";
    this.checkBoxStep5.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep6
    // 
    this.checkBoxStep6.AutoSize = true;
    this.checkBoxStep6.Checked = true;
    this.checkBoxStep6.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep6.Location = new System.Drawing.Point(113, 52);
    this.checkBoxStep6.Name = "checkBoxStep6";
    this.checkBoxStep6.Size = new System.Drawing.Size(66, 17);
    this.checkBoxStep6.TabIndex = 41;
    this.checkBoxStep6.Text = "+/- 3 min";
    this.checkBoxStep6.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep14
    // 
    this.checkBoxStep14.AutoSize = true;
    this.checkBoxStep14.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep14.Location = new System.Drawing.Point(210, 52);
    this.checkBoxStep14.Name = "checkBoxStep14";
    this.checkBoxStep14.Size = new System.Drawing.Size(65, 17);
    this.checkBoxStep14.TabIndex = 40;
    this.checkBoxStep14.Text = "+/- 1,5 h";
    this.checkBoxStep14.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep8
    // 
    this.checkBoxStep8.AutoSize = true;
    this.checkBoxStep8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep8.Location = new System.Drawing.Point(113, 98);
    this.checkBoxStep8.Name = "checkBoxStep8";
    this.checkBoxStep8.Size = new System.Drawing.Size(66, 17);
    this.checkBoxStep8.TabIndex = 39;
    this.checkBoxStep8.Text = "+/- 7 min";
    this.checkBoxStep8.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep13
    // 
    this.checkBoxStep13.AutoSize = true;
    this.checkBoxStep13.Checked = true;
    this.checkBoxStep13.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep13.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep13.Location = new System.Drawing.Point(210, 29);
    this.checkBoxStep13.Name = "checkBoxStep13";
    this.checkBoxStep13.Size = new System.Drawing.Size(56, 17);
    this.checkBoxStep13.TabIndex = 38;
    this.checkBoxStep13.Text = "+/- 1 h";
    this.checkBoxStep13.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep7
    // 
    this.checkBoxStep7.AutoSize = true;
    this.checkBoxStep7.Checked = true;
    this.checkBoxStep7.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep7.Location = new System.Drawing.Point(113, 75);
    this.checkBoxStep7.Name = "checkBoxStep7";
    this.checkBoxStep7.Size = new System.Drawing.Size(66, 17);
    this.checkBoxStep7.TabIndex = 37;
    this.checkBoxStep7.Text = "+/- 5 min";
    this.checkBoxStep7.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep3
    // 
    this.checkBoxStep3.AutoSize = true;
    this.checkBoxStep3.Checked = true;
    this.checkBoxStep3.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep3.Location = new System.Drawing.Point(15, 75);
    this.checkBoxStep3.Name = "checkBoxStep3";
    this.checkBoxStep3.Size = new System.Drawing.Size(73, 17);
    this.checkBoxStep3.TabIndex = 36;
    this.checkBoxStep3.Text = "+/- 30 sec";
    this.checkBoxStep3.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep2
    // 
    this.checkBoxStep2.AutoSize = true;
    this.checkBoxStep2.Checked = true;
    this.checkBoxStep2.CheckState = System.Windows.Forms.CheckState.Checked;
    this.checkBoxStep2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep2.Location = new System.Drawing.Point(15, 52);
    this.checkBoxStep2.Name = "checkBoxStep2";
    this.checkBoxStep2.Size = new System.Drawing.Size(73, 17);
    this.checkBoxStep2.TabIndex = 35;
    this.checkBoxStep2.Text = "+/- 15 sec";
    this.checkBoxStep2.UseVisualStyleBackColor = true;
    // 
    // checkBoxStep1
    // 
    this.checkBoxStep1.AutoSize = true;
    this.checkBoxStep1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
    this.checkBoxStep1.Location = new System.Drawing.Point(15, 29);
    this.checkBoxStep1.Name = "checkBoxStep1";
    this.checkBoxStep1.Size = new System.Drawing.Size(67, 17);
    this.checkBoxStep1.TabIndex = 34;
    this.checkBoxStep1.Text = "+/- 5 sec";
    this.checkBoxStep1.UseVisualStyleBackColor = true;
    // 
    // GeneralSkipSteps
    // 
    this.Controls.Add(this.tabControlSkipSteps);
    this.Name = "GeneralSkipSteps";
    this.Size = new System.Drawing.Size(472, 408);
    this.tabControlSkipSteps.ResumeLayout(false);
    this.tabPageSteps.ResumeLayout(false);
    this.groupBoxSkipSteps.ResumeLayout(false);
    this.groupBoxSkipSteps.PerformLayout();
    this.ResumeLayout(false);

    }
    }
  }
