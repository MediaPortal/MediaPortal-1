#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralSkipSteps : SectionSettings
  {
    private IContainer components = null;

    private MPTabControl tabControlSkipSteps;
    private MPGroupBox groupBoxSkipSteps;
    private MPCheckBox checkBoxStep16;
    private MPButton buttonResetSkipSteps;
    private MPCheckBox checkBoxStep4;
    private MPCheckBox checkBoxStep15;
    private MPCheckBox checkBoxStep12;
    private MPCheckBox checkBoxStep11;
    private MPCheckBox checkBoxStep10;
    private MPCheckBox checkBoxStep9;
    private MPCheckBox checkBoxStep5;
    private MPCheckBox checkBoxStep6;
    private MPCheckBox checkBoxStep14;
    private MPCheckBox checkBoxStep8;
    private MPCheckBox checkBoxStep13;
    private MPCheckBox checkBoxStep7;
    private MPCheckBox checkBoxStep3;
    private MPCheckBox checkBoxStep2;
    private MPCheckBox checkBoxStep1;
    private MPGroupBox groupBoxTimeout;
    private MPLabel labelSkipTimeout;
    private MPNumericUpDown numericUpDownSkipTimeout;
    private MPTextBox textBoxManualSkipSteps;
    private MPLabel label1;
    private Label labelError;
    private MPTabPage tabPageSteps;
    private SortedList _stepList = new SortedList();
    private MPGroupBox mpGroupBox1;
    private MPRadioButton mpRadioButton1;
    private MPRadioButton mpRadioButton2;
    private MPLabel mpLabel1;
    private MPGroupBox mpGroupBox2;
    private MPTextBox textBoxImmediateSkipSteps;
    private Label labelError2;
    private const string DEFAULT_SETTING = "15,30,60,180,300,600,900,1800,3600,7200";

    public GeneralSkipSteps()
      : this("Skip steps") {}

    public GeneralSkipSteps(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      ArrayList StepArray = new ArrayList();
      string regValue;

      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          regValue = xmlreader.GetValueAsString("movieplayer", "skipsteps", DEFAULT_SETTING);
          if (regValue == string.Empty) // config after wizard run 1st
          {
            regValue = DEFAULT_SETTING;
            Log.Info("GeneralSkipSteps - creating new Skip-Settings {0}", "");
          }
          else if (OldStyle(regValue))
          {
            regValue = ConvertToNewStyle(regValue);
          }
          textBoxManualSkipSteps.Text = regValue;
        }
        catch (Exception ex)
        {
          Log.Info("GeneralSkipSteps - Exception while loading Skip-Settings: {0}", ex.ToString());
        }

        string timeout = (xmlreader.GetValueAsString("movieplayer", "skipsteptimeout", "1500"));

        if (timeout == string.Empty)
        {
          numericUpDownSkipTimeout.Value = 1500;
        }
        else
        {
          numericUpDownSkipTimeout.Value = Convert.ToInt16(timeout);
        }
        bool immediateSkipType = (xmlreader.GetValueAsBool("movieplayer", "immediateskipstepsisrelative", true));
        if (immediateSkipType)
        {
          mpRadioButton1.Checked = true;
          mpRadioButton2.Checked = false;
        }
        else
        {
          mpRadioButton1.Checked = false;
          mpRadioButton2.Checked = true;
        }


        int immediateSkipSize = (xmlreader.GetValueAsInt("movieplayer", "immediateskipstepsize", 10));
        textBoxImmediateSkipSteps.Text = immediateSkipSize.ToString();
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        if (labelError.Text != string.Empty)
        {
          textBoxManualSkipSteps.Text = DEFAULT_SETTING;
        }
        if (labelError2.Text != string.Empty)
        {
          textBoxImmediateSkipSteps.Text = "10";
        }
        xmlwriter.SetValue("movieplayer", "skipsteps", textBoxManualSkipSteps.Text);
        xmlwriter.SetValue("movieplayer", "skipsteptimeout", numericUpDownSkipTimeout.Value);
        xmlwriter.SetValueAsBool("movieplayer", "immediateskipstepsisrelative", mpRadioButton1.Checked);
        xmlwriter.SetValue("movieplayer", "immediateskipstepsize", textBoxImmediateSkipSteps.Text);
      }
    }

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

    private void buttonResetSkipSteps_Click(object sender, EventArgs e)
    {
      textBoxManualSkipSteps.Text = DEFAULT_SETTING;
      mpRadioButton1.Checked = true;
      mpRadioButton2.Checked = false;
      textBoxImmediateSkipSteps.Text = "10";
      numericUpDownSkipTimeout.Value = 1500;
    }

    private void InitializeComponent()
    {
      this.tabControlSkipSteps = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSteps = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelError2 = new System.Windows.Forms.Label();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpRadioButton2 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpRadioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.textBoxImmediateSkipSteps = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxTimeout = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.numericUpDownSkipTimeout = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.labelSkipTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonResetSkipSteps = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxSkipSteps = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelError = new System.Windows.Forms.Label();
      this.textBoxManualSkipSteps = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxStep16 = new MediaPortal.UserInterface.Controls.MPCheckBox();
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
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.groupBoxTimeout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSkipTimeout)).BeginInit();
      this.groupBoxSkipSteps.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlSkipSteps
      // 
      this.tabControlSkipSteps.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlSkipSteps.Controls.Add(this.tabPageSteps);
      this.tabControlSkipSteps.Location = new System.Drawing.Point(0, 0);
      this.tabControlSkipSteps.Name = "tabControlSkipSteps";
      this.tabControlSkipSteps.SelectedIndex = 0;
      this.tabControlSkipSteps.Size = new System.Drawing.Size(472, 509);
      this.tabControlSkipSteps.TabIndex = 0;
      // 
      // tabPageSteps
      // 
      this.tabPageSteps.Controls.Add(this.mpGroupBox1);
      this.tabPageSteps.Controls.Add(this.groupBoxTimeout);
      this.tabPageSteps.Controls.Add(this.buttonResetSkipSteps);
      this.tabPageSteps.Controls.Add(this.groupBoxSkipSteps);
      this.tabPageSteps.Location = new System.Drawing.Point(4, 22);
      this.tabPageSteps.Name = "tabPageSteps";
      this.tabPageSteps.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSteps.Size = new System.Drawing.Size(464, 483);
      this.tabPageSteps.TabIndex = 0;
      this.tabPageSteps.Text = "Player steps";
      this.tabPageSteps.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.labelError2);
      this.mpGroupBox1.Controls.Add(this.mpGroupBox2);
      this.mpGroupBox1.Controls.Add(this.textBoxImmediateSkipSteps);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 320);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(347, 77);
      this.mpGroupBox1.TabIndex = 2;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Immediate skip steps (up / down)";
      // 
      // labelError2
      // 
      this.labelError2.Location = new System.Drawing.Point(145, 54);
      this.labelError2.Name = "labelError2";
      this.labelError2.Size = new System.Drawing.Size(196, 17);
      this.labelError2.TabIndex = 54;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpRadioButton2);
      this.mpGroupBox2.Controls.Add(this.mpRadioButton1);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 17);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(130, 54);
      this.mpGroupBox2.TabIndex = 7;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Skip type";
      // 
      // mpRadioButton2
      // 
      this.mpRadioButton2.AutoSize = true;
      this.mpRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton2.Location = new System.Drawing.Point(7, 34);
      this.mpRadioButton2.Name = "mpRadioButton2";
      this.mpRadioButton2.Size = new System.Drawing.Size(115, 17);
      this.mpRadioButton2.TabIndex = 3;
      this.mpRadioButton2.Text = "Constant (seconds)";
      this.mpRadioButton2.UseVisualStyleBackColor = true;
      // 
      // mpRadioButton1
      // 
      this.mpRadioButton1.AutoSize = true;
      this.mpRadioButton1.Checked = true;
      this.mpRadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRadioButton1.Location = new System.Drawing.Point(7, 14);
      this.mpRadioButton1.Name = "mpRadioButton1";
      this.mpRadioButton1.Size = new System.Drawing.Size(80, 17);
      this.mpRadioButton1.TabIndex = 2;
      this.mpRadioButton1.TabStop = true;
      this.mpRadioButton1.Text = "Relative (%)";
      this.mpRadioButton1.UseVisualStyleBackColor = true;
      this.mpRadioButton1.CheckedChanged += new System.EventHandler(this.textBoxImmediateSkipSteps_TextChanged);
      // 
      // textBoxImmediateSkipSteps
      // 
      this.textBoxImmediateSkipSteps.AcceptsReturn = true;
      this.textBoxImmediateSkipSteps.BorderColor = System.Drawing.Color.Empty;
      this.textBoxImmediateSkipSteps.Location = new System.Drawing.Point(148, 33);
      this.textBoxImmediateSkipSteps.Name = "textBoxImmediateSkipSteps";
      this.textBoxImmediateSkipSteps.Size = new System.Drawing.Size(55, 20);
      this.textBoxImmediateSkipSteps.TabIndex = 6;
      this.textBoxImmediateSkipSteps.TextChanged += new System.EventHandler(this.textBoxImmediateSkipSteps_TextChanged);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(145, 17);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(168, 13);
      this.mpLabel1.TabIndex = 5;
      this.mpLabel1.Text = "Skip value (in seconds or percent)";
      // 
      // groupBoxTimeout
      // 
      this.groupBoxTimeout.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTimeout.Controls.Add(this.numericUpDownSkipTimeout);
      this.groupBoxTimeout.Controls.Add(this.labelSkipTimeout);
      this.groupBoxTimeout.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTimeout.Location = new System.Drawing.Point(16, 274);
      this.groupBoxTimeout.Name = "groupBoxTimeout";
      this.groupBoxTimeout.Size = new System.Drawing.Size(428, 40);
      this.groupBoxTimeout.TabIndex = 1;
      this.groupBoxTimeout.TabStop = false;
      this.groupBoxTimeout.Text = "Timeout";
      // 
      // numericUpDownSkipTimeout
      // 
      this.numericUpDownSkipTimeout.Increment = new decimal(new int[]
                                                              {
                                                                100,
                                                                0,
                                                                0,
                                                                0
                                                              });
      this.numericUpDownSkipTimeout.Location = new System.Drawing.Point(213, 14);
      this.numericUpDownSkipTimeout.Maximum = new decimal(new int[]
                                                            {
                                                              10000,
                                                              0,
                                                              0,
                                                              0
                                                            });
      this.numericUpDownSkipTimeout.Name = "numericUpDownSkipTimeout";
      this.numericUpDownSkipTimeout.Size = new System.Drawing.Size(56, 20);
      this.numericUpDownSkipTimeout.TabIndex = 1;
      this.numericUpDownSkipTimeout.Value = new decimal(new int[]
                                                          {
                                                            1500,
                                                            0,
                                                            0,
                                                            0
                                                          });
      // 
      // labelSkipTimeout
      // 
      this.labelSkipTimeout.AutoSize = true;
      this.labelSkipTimeout.Location = new System.Drawing.Point(12, 16);
      this.labelSkipTimeout.Name = "labelSkipTimeout";
      this.labelSkipTimeout.Size = new System.Drawing.Size(195, 13);
      this.labelSkipTimeout.TabIndex = 0;
      this.labelSkipTimeout.Text = "Timeout before skipping occurs (msec.):";
      // 
      // buttonResetSkipSteps
      // 
      this.buttonResetSkipSteps.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonResetSkipSteps.Location = new System.Drawing.Point(369, 374);
      this.buttonResetSkipSteps.Name = "buttonResetSkipSteps";
      this.buttonResetSkipSteps.Size = new System.Drawing.Size(75, 23);
      this.buttonResetSkipSteps.TabIndex = 2;
      this.buttonResetSkipSteps.Text = "Defaults";
      this.buttonResetSkipSteps.UseVisualStyleBackColor = true;
      this.buttonResetSkipSteps.Click += new System.EventHandler(this.buttonResetSkipSteps_Click);
      // 
      // groupBoxSkipSteps
      // 
      this.groupBoxSkipSteps.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkipSteps.Controls.Add(this.labelError);
      this.groupBoxSkipSteps.Controls.Add(this.textBoxManualSkipSteps);
      this.groupBoxSkipSteps.Controls.Add(this.label1);
      this.groupBoxSkipSteps.Controls.Add(this.checkBoxStep16);
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
      this.groupBoxSkipSteps.Size = new System.Drawing.Size(428, 252);
      this.groupBoxSkipSteps.TabIndex = 0;
      this.groupBoxSkipSteps.TabStop = false;
      this.groupBoxSkipSteps.Text = "Skip steps (left / right)";
      // 
      // labelError
      // 
      this.labelError.Location = new System.Drawing.Point(210, 206);
      this.labelError.Name = "labelError";
      this.labelError.Size = new System.Drawing.Size(212, 16);
      this.labelError.TabIndex = 53;
      // 
      // textBoxManualSkipSteps
      // 
      this.textBoxManualSkipSteps.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxManualSkipSteps.BorderColor = System.Drawing.Color.Empty;
      this.textBoxManualSkipSteps.Location = new System.Drawing.Point(158, 222);
      this.textBoxManualSkipSteps.Name = "textBoxManualSkipSteps";
      this.textBoxManualSkipSteps.Size = new System.Drawing.Size(264, 20);
      this.textBoxManualSkipSteps.TabIndex = 17;
      this.textBoxManualSkipSteps.Text = "15,30,60,180,300,600,900,1800,3600,7200";
      this.textBoxManualSkipSteps.TextChanged += new System.EventHandler(this.textBoxManualSkipSteps_TextChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(17, 225);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(135, 13);
      this.label1.TabIndex = 16;
      this.label1.Text = "Define skip steps manually:";
      // 
      // checkBoxStep16
      // 
      this.checkBoxStep16.AutoSize = true;
      this.checkBoxStep16.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep16.Location = new System.Drawing.Point(210, 98);
      this.checkBoxStep16.Name = "checkBoxStep16";
      this.checkBoxStep16.Size = new System.Drawing.Size(56, 17);
      this.checkBoxStep16.TabIndex = 15;
      this.checkBoxStep16.Text = "+/- 3 h";
      this.checkBoxStep16.UseVisualStyleBackColor = true;
      this.checkBoxStep16.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // checkBoxStep4
      // 
      this.checkBoxStep4.AutoSize = true;
      this.checkBoxStep4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep4.Location = new System.Drawing.Point(15, 98);
      this.checkBoxStep4.Name = "checkBoxStep4";
      this.checkBoxStep4.Size = new System.Drawing.Size(73, 17);
      this.checkBoxStep4.TabIndex = 3;
      this.checkBoxStep4.Text = "+/- 45 sec";
      this.checkBoxStep4.UseVisualStyleBackColor = true;
      this.checkBoxStep4.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep15.TabIndex = 14;
      this.checkBoxStep15.Text = "+/- 2 h";
      this.checkBoxStep15.UseVisualStyleBackColor = true;
      this.checkBoxStep15.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // checkBoxStep12
      // 
      this.checkBoxStep12.AutoSize = true;
      this.checkBoxStep12.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep12.Location = new System.Drawing.Point(113, 190);
      this.checkBoxStep12.Name = "checkBoxStep12";
      this.checkBoxStep12.Size = new System.Drawing.Size(72, 17);
      this.checkBoxStep12.TabIndex = 11;
      this.checkBoxStep12.Text = "+/- 45 min";
      this.checkBoxStep12.UseVisualStyleBackColor = true;
      this.checkBoxStep12.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep11.TabIndex = 10;
      this.checkBoxStep11.Text = "+/- 30 min";
      this.checkBoxStep11.UseVisualStyleBackColor = true;
      this.checkBoxStep11.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep10.TabIndex = 9;
      this.checkBoxStep10.Text = "+/- 15 min";
      this.checkBoxStep10.UseVisualStyleBackColor = true;
      this.checkBoxStep10.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep9.TabIndex = 8;
      this.checkBoxStep9.Text = "+/- 10 min";
      this.checkBoxStep9.UseVisualStyleBackColor = true;
      this.checkBoxStep9.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep5.TabIndex = 4;
      this.checkBoxStep5.Text = "+/- 1 min";
      this.checkBoxStep5.UseVisualStyleBackColor = true;
      this.checkBoxStep5.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep6.TabIndex = 5;
      this.checkBoxStep6.Text = "+/- 3 min";
      this.checkBoxStep6.UseVisualStyleBackColor = true;
      this.checkBoxStep6.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // checkBoxStep14
      // 
      this.checkBoxStep14.AutoSize = true;
      this.checkBoxStep14.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep14.Location = new System.Drawing.Point(210, 52);
      this.checkBoxStep14.Name = "checkBoxStep14";
      this.checkBoxStep14.Size = new System.Drawing.Size(65, 17);
      this.checkBoxStep14.TabIndex = 13;
      this.checkBoxStep14.Text = "+/- 1,5 h";
      this.checkBoxStep14.UseVisualStyleBackColor = true;
      this.checkBoxStep14.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // checkBoxStep8
      // 
      this.checkBoxStep8.AutoSize = true;
      this.checkBoxStep8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep8.Location = new System.Drawing.Point(113, 98);
      this.checkBoxStep8.Name = "checkBoxStep8";
      this.checkBoxStep8.Size = new System.Drawing.Size(66, 17);
      this.checkBoxStep8.TabIndex = 7;
      this.checkBoxStep8.Text = "+/- 7 min";
      this.checkBoxStep8.UseVisualStyleBackColor = true;
      this.checkBoxStep8.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep13.TabIndex = 12;
      this.checkBoxStep13.Text = "+/- 1 h";
      this.checkBoxStep13.UseVisualStyleBackColor = true;
      this.checkBoxStep13.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep7.TabIndex = 6;
      this.checkBoxStep7.Text = "+/- 5 min";
      this.checkBoxStep7.UseVisualStyleBackColor = true;
      this.checkBoxStep7.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep3.TabIndex = 2;
      this.checkBoxStep3.Text = "+/- 30 sec";
      this.checkBoxStep3.UseVisualStyleBackColor = true;
      this.checkBoxStep3.Click += new System.EventHandler(this.checkBoxStep_Click);
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
      this.checkBoxStep2.TabIndex = 1;
      this.checkBoxStep2.Text = "+/- 15 sec";
      this.checkBoxStep2.UseVisualStyleBackColor = true;
      this.checkBoxStep2.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // checkBoxStep1
      // 
      this.checkBoxStep1.AutoSize = true;
      this.checkBoxStep1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxStep1.Location = new System.Drawing.Point(15, 29);
      this.checkBoxStep1.Name = "checkBoxStep1";
      this.checkBoxStep1.Size = new System.Drawing.Size(67, 17);
      this.checkBoxStep1.TabIndex = 0;
      this.checkBoxStep1.Text = "+/- 5 sec";
      this.checkBoxStep1.UseVisualStyleBackColor = true;
      this.checkBoxStep1.Click += new System.EventHandler(this.checkBoxStep_Click);
      // 
      // GeneralSkipSteps
      // 
      this.Controls.Add(this.tabControlSkipSteps);
      this.Name = "GeneralSkipSteps";
      this.Size = new System.Drawing.Size(472, 476);
      this.tabControlSkipSteps.ResumeLayout(false);
      this.tabPageSteps.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.groupBoxTimeout.ResumeLayout(false);
      this.groupBoxTimeout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSkipTimeout)).EndInit();
      this.groupBoxSkipSteps.ResumeLayout(false);
      this.groupBoxSkipSteps.PerformLayout();
      this.ResumeLayout(false);
    }

    private void textBoxManualSkipSteps_TextChanged(object sender, EventArgs e)
    {
      bool errorfound = false;
      bool check1 = false, check2 = false, check3 = false, check4 = false, check5 = false, check6 = false;
      bool check7 = false, check8 = false, check9 = false, check10 = false, check11 = false, check12 = false;
      bool check13 = false, check14 = false, check15 = false, check16 = false;
      foreach (string token in textBoxManualSkipSteps.Text.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int step = Convert.ToInt16(token);
          if (step < 0)
          {
            SetError("Postive values only!");
            errorfound = true;
          }
          else if (step == 0)
          {
            SetError("Zero skip is not allowed!");
            errorfound = true;
          }
          else if (step > 10800)
          {
            SetError("3 hour skip is maximum!");
            errorfound = true;
          }
          else
          {
            // Check that whole minutes are entered
            if (step > 60 && (step % 60) != 0)
            {
              SetError("Enter whole minutes only!");
              errorfound = true;
            }
          }

          switch (step)
          {
            case 5:
              check1 = true;
              break;
            case 15:
              check2 = true;
              break;
            case 30:
              check3 = true;
              break;
            case 45:
              check4 = true;
              break;
            case 60:
              check5 = true;
              break;
            case 180:
              check6 = true;
              break;
            case 300:
              check7 = true;
              break;
            case 420:
              check8 = true;
              break;
            case 600:
              check9 = true;
              break;
            case 900:
              check10 = true;
              break;
            case 1800:
              check11 = true;
              break;
            case 2700:
              check12 = true;
              break;
            case 3600:
              check13 = true;
              break;
            case 5400:
              check14 = true;
              break;
            case 7200:
              check15 = true;
              break;
            case 10800:
              check16 = true;
              break;
            default:
              break; // Do nothing
          }
        }
        catch (Exception)
        {
          SetError("Invalid value: " + token);
          errorfound = true;
        }
      }
      if (!errorfound)
      {
        if (!CheckForDoubles() || !CheckOrder())
        {
          return;
        }
        SetError(null);
        checkBoxStep1.Checked = check1;
        checkBoxStep2.Checked = check2;
        checkBoxStep3.Checked = check3;
        checkBoxStep4.Checked = check4;
        checkBoxStep5.Checked = check5;
        checkBoxStep6.Checked = check6;
        checkBoxStep7.Checked = check7;
        checkBoxStep8.Checked = check8;
        checkBoxStep9.Checked = check9;
        checkBoxStep10.Checked = check10;
        checkBoxStep11.Checked = check11;
        checkBoxStep12.Checked = check12;
        checkBoxStep13.Checked = check13;
        checkBoxStep14.Checked = check14;
        checkBoxStep15.Checked = check15;
        checkBoxStep16.Checked = check16;
      }
    }

    private bool CheckForDoubles()
    {
      _stepList.Clear();
      foreach (string token in textBoxManualSkipSteps.Text.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        int value;
        try
        {
          value = Convert.ToInt16(token);
        }
        catch (Exception)
        {
          SetError("Invalid value: " + token);
          return false;
        }
        try
        {
          _stepList.Add(value, value);
        }
        catch (ArgumentException)
        {
          SetError("Value already exists: " + token);
          return false;
        }
      }
      return true;
    }

    private bool CheckOrder()
    {
      IList orderedList = _stepList.GetValueList();
      int index = 0;
      foreach (string token in textBoxManualSkipSteps.Text.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        int value;
        try
        {
          value = Convert.ToInt16(token);
        }
        catch (Exception)
        {
          SetError("Invalid value: " + token);
          return false;
        }
        int tempVal = (int)orderedList[index++];
        if (tempVal != value)
        {
          SetError("Place values in correct order!");
          return false;
        }
      }
      return true;
    }

    private void checkBoxStep_Click(object sender, EventArgs e)
    {
      int stepSize = 5;
      if (sender == checkBoxStep1)
      {
        stepSize = 5;
      }
      else if (sender == checkBoxStep2)
      {
        stepSize = 15;
      }
      else if (sender == checkBoxStep3)
      {
        stepSize = 30;
      }
      else if (sender == checkBoxStep4)
      {
        stepSize = 45;
      }
      else if (sender == checkBoxStep5)
      {
        stepSize = 60;
      }
      else if (sender == checkBoxStep6)
      {
        stepSize = 180;
      }
      else if (sender == checkBoxStep7)
      {
        stepSize = 300;
      }
      else if (sender == checkBoxStep8)
      {
        stepSize = 420;
      }
      else if (sender == checkBoxStep9)
      {
        stepSize = 600;
      }
      else if (sender == checkBoxStep10)
      {
        stepSize = 900;
      }
      else if (sender == checkBoxStep11)
      {
        stepSize = 1800;
      }
      else if (sender == checkBoxStep12)
      {
        stepSize = 2700;
      }
      else if (sender == checkBoxStep13)
      {
        stepSize = 3600;
      }
      else if (sender == checkBoxStep14)
      {
        stepSize = 5400;
      }
      else if (sender == checkBoxStep15)
      {
        stepSize = 7200;
      }
      else if (sender == checkBoxStep16)
      {
        stepSize = 10800;
      }

      if (!((MPCheckBox)sender).Checked)
      {
        RemoveStep(stepSize);
      }
      else
      {
        AddStep(stepSize);
      }
    }

    private void RemoveStep(int stepsize)
    {
      string newText = string.Empty;
      foreach (string token in textBoxManualSkipSteps.Text.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          if (Convert.ToInt16(token) != stepsize)
          {
            newText += token;
            newText += ",";
          }
        }
        catch (Exception)
        {
          return;
        }
      }
      textBoxManualSkipSteps.Text = (newText.Length > 0 ? newText.Substring(0, newText.Length - 1) : string.Empty);
    }

    private void AddStep(int stepsize)
    {
      string newText = string.Empty;
      bool stepAdded = false;
      foreach (string token in textBoxManualSkipSteps.Text.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          if (stepsize < curInt && !stepAdded)
          {
            newText += Convert.ToString(stepsize);
            newText += ",";
            stepAdded = true;
          }
          else if (stepsize == curInt)
          {
            stepAdded = true; // Should never get here, but just in case...
          }
          newText += token;
          newText += ",";
        }
        catch (Exception)
        {
          return;
        }
      }
      if (!stepAdded)
      {
        newText += Convert.ToString(stepsize);
        newText += ",";
      }
      textBoxManualSkipSteps.Text = (newText.Length > 0 ? newText.Substring(0, newText.Length - 1) : string.Empty);
    }

    private void SetError(string errorText)
    {
      if (errorText == null)
      {
        textBoxManualSkipSteps.BackColor = Color.White;
        labelError.Text = string.Empty;
      }
      else
      {
        textBoxManualSkipSteps.BackColor = Color.Red;
        labelError.Text = errorText;
      }
    }

    private void SetError2(string errorText)
    {
      if (errorText == null)
      {
        textBoxImmediateSkipSteps.BackColor = Color.White;
        labelError2.Text = string.Empty;
      }
      else
      {
        textBoxImmediateSkipSteps.BackColor = Color.Red;
        labelError2.Text = errorText;
      }
    }

    private bool OldStyle(string strSteps)
    {
      int count = 0;
      bool foundOtherThanZeroOrOne = false;
      foreach (string token in strSteps.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          if (curInt != 0 && curInt != 1)
          {
            foundOtherThanZeroOrOne = true;
          }
          count++;
        }
        catch (Exception)
        {
          return true;
        }
      }
      return (count == 16 && !foundOtherThanZeroOrOne);
    }

    private string ConvertToNewStyle(string strSteps)
    {
      int count = 0;
      string newStyle = string.Empty;
      foreach (string token in strSteps.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          count++;
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          count++;
          if (curInt == 1)
          {
            switch (count)
            {
              case 1:
                newStyle += "5,";
                break;
              case 2:
                newStyle += "15,";
                break;
              case 3:
                newStyle += "30,";
                break;
              case 4:
                newStyle += "45,";
                break;
              case 5:
                newStyle += "60,";
                break;
              case 6:
                newStyle += "180,";
                break;
              case 7:
                newStyle += "300,";
                break;
              case 8:
                newStyle += "420,";
                break;
              case 9:
                newStyle += "600,";
                break;
              case 10:
                newStyle += "900,";
                break;
              case 11:
                newStyle += "1800,";
                break;
              case 12:
                newStyle += "2700,";
                break;
              case 13:
                newStyle += "3600,";
                break;
              case 14:
                newStyle += "5400,";
                break;
              case 15:
                newStyle += "7200,";
                break;
              case 16:
                newStyle += "10800,";
                break;
              default:
                break; // Do nothing
            }
          }
        }
        catch (Exception)
        {
          return DEFAULT_SETTING;
        }
      }
      return (newStyle == string.Empty ? string.Empty : newStyle.Substring(0, newStyle.Length - 1));
    }

    private void textBoxImmediateSkipSteps_TextChanged(object sender, EventArgs e)
    {
      try
      {
        int step = Convert.ToInt16(textBoxImmediateSkipSteps.Text);
        if (step < 0)
        {
          SetError2("Positive values only!");
          return;
        }
        else if (step == 0)
        {
          SetError2("Zero skip is not allowed!");
          return;
        }
        else if ((mpRadioButton2.Checked) && (step > 10800))
        {
          SetError2("3 hour skip is maximum!");
          return;
        }
        else if ((mpRadioButton1.Checked) && (step > 99))
        {
          SetError2("99 % skip is maximum!");
          return;
        }
      }
      catch (Exception)
      {
        SetError2("Invalid value: " + textBoxImmediateSkipSteps.Text + " Use only numbers");
        return;
      }
      SetError2(null);
    }
  }
}