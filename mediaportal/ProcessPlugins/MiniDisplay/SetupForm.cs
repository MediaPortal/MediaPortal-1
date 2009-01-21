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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  [PluginIcons("MiniDisplayPlugin.lcd.gif", "MiniDisplayPlugin.lcd_deactivated.gif")]
  public class SetupForm : MPConfigForm
  {
    private MPButton btnAdvanced;
    private MPButton btnCancel;
    private MPButton btnOK;
    private MPButton btnTest;
    private MPButton btnTestDisplay;
    private MPCheckBox cbContrast;
    private MPCheckBox cbControlScreenSaver;
    private MPCheckBox cbDisplayOff;
    private MPCheckBox cbExtensiveLogging;
    private MPCheckBox cbLight;
    private MPCheckBox cbPropertyBrowser;
    private MPCheckBox ckForceGraphicText;
    private MPComboBox cmbPort;
    private MPComboBox cmbType;
    private IContainer components;
    private ErrorProvider errorProvider;
    private MPGroupBox gbGraphMode;
    private MPGroupBox gbTextMode;
    private MPGroupBox groupBox1;
    private GroupBox groupShutdown;
    private MPLabel label1;
    private MPLabel label10;
    private Label label11;
    private MPLabel label2;
    private MPLabel label3;
    private MPLabel label4;
    private Label label6;
    private MPLabel label7;
    private MPLabel label8;
    private MPLabel label9;
    private MPLabel lblBrightness;
    private MPLabel lblContrast;
    private IDisplay lcd;
    private MPCheckBox mpDisableGUISetup;
    private MPLabel mpLabel1;
    private MPLabel mpLabel2;
    private MPLabel mpLabel3;
    private MPLabel mpLabel4;
    private MPLabel mpLabel5;
    private MPLabel mpLabel6;
    private MPTextBox mpPrefixChar;
    private TextBox mpShutdown1;
    private TextBox mpShutdown2;
    private TrackBar tbBrightness;
    private TrackBar tbContrast;
    private MPTextBox txtCharsToScroll;
    private MPTextBox txtCols;
    private MPTextBox txtColsG;
    private MPComboBox txtFont;
    private MPTextBox txtFontSize;
    private MPTextBox txtPixelsToScroll;
    private MPTextBox txtRows;
    private MPTextBox txtRowsG;
    private MPTextBox txtScrollDelay;
    private MPTextBox txtTim;
    private MPTextBox txtTimG;

    public SetupForm()
    {
      Log.Info("MiniDisplay.SetupForm(): {0}", new object[] {MiniDisplayHelper.Plugin_Version});
      this.InitializeComponent();
      this.cmbPort.SelectedIndex = 0;
      this.cmbType.DataSource = Settings.Instance.Drivers;
      this.cmbType.DisplayMember = "Description";
      this.cmbType.DataBindings.Add("SelectedItem", Settings.Instance, "LCDType");
      this.cmbPort.DataBindings.Add("SelectedItem", Settings.Instance, "GUIPort");
      this.cbPropertyBrowser.DataBindings.Add("Checked", Settings.Instance, "ShowPropertyBrowser");
      this.mpDisableGUISetup.DataBindings.Add("Checked", Settings.Instance, "DisableGUISetup");
      this.cbExtensiveLogging.DataBindings.Add("Checked", Settings.Instance, "ExtensiveLogging");
      this.txtCols.DataBindings.Add("Text", Settings.Instance, "TextWidth");
      this.txtRows.DataBindings.Add("Text", Settings.Instance, "TextHeight");
      this.txtColsG.DataBindings.Add("Text", Settings.Instance, "GraphicWidth");
      this.txtRowsG.DataBindings.Add("Text", Settings.Instance, "GraphicHeight");
      this.txtTim.DataBindings.Add("Text", Settings.Instance, "TextComDelay");
      this.txtTimG.DataBindings.Add("Text", Settings.Instance, "GraphicComDelay");
      this.cbDisplayOff.DataBindings.Add("Checked", Settings.Instance, "BlankOnExit");
      this.cbLight.DataBindings.Add("Checked", Settings.Instance, "BackLightControl");
      this.tbBrightness.DataBindings.Add("Value", Settings.Instance, "Backlight");
      this.cbContrast.DataBindings.Add("Checked", Settings.Instance, "ContrastControl");
      this.tbContrast.DataBindings.Add("Value", Settings.Instance, "Contrast");
      this.txtFont.DataBindings.Add("Text", Settings.Instance, "Font");
      this.txtFontSize.DataBindings.Add("Text", Settings.Instance, "FontSize");
      this.txtScrollDelay.DataBindings.Add("Text", Settings.Instance, "ScrollDelay");
      this.ckForceGraphicText.DataBindings.Add("Checked", Settings.Instance, "ForceGraphicText");
      this.txtPixelsToScroll.DataBindings.Add("Text", Settings.Instance, "PixelsToScroll");
      this.txtCharsToScroll.DataBindings.Add("Text", Settings.Instance, "CharsToScroll");
      this.mpPrefixChar.DataBindings.Add("Text", Settings.Instance, "PrefixChar");
      this.mpShutdown1.DataBindings.Add("Text", Settings.Instance, "Shutdown1");
      this.mpShutdown2.DataBindings.Add("Text", Settings.Instance, "Shutdown2");
      this.cbControlScreenSaver.DataBindings.Add("Checked", Settings.Instance, "ControlScreenSaver");
      this.lcd = Settings.Instance.LCDType;
      this.cmbType.SelectedItem = this.lcd;
      if (this.cbLight.Checked)
      {
        this.groupShutdown.Enabled = false;
      }
      else
      {
        this.groupShutdown.Enabled = true;
      }
      this.VerifyLCDType();
      if (!File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml")))
      {
        this.btnTest.Enabled = false;
      }
      else
      {
        this.btnTest.Enabled = true;
      }
      Log.Info("MiniDisplay.SetupForm(): constructor completed", new object[0]);
    }

    private void btnAdvanced_Click(object sender, EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      try
      {
        this.lcd.Configure();
        this.VerifyLCDType();
      }
      catch (Exception exception)
      {
        Log.Error("MiniDisplay.SetupForm.btnAdvanced_Click(): CAUGHT EXCEPTION: {0}", new object[] {exception});
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Settings.Save();
      base.Close();
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      try
      {
        new MessageEditForm().ShowDialog(this);
        base.Close();
      }
      catch
      {
        base.Close();
      }
    }

    private void btnTestDisplay_Click(object sender, EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      try
      {
        base.Enabled = false;
        this.lcd.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
                       Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth,
                       Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl,
                       Settings.Instance.Backlight, Settings.Instance.ContrastControl, Settings.Instance.Contrast,
                       Settings.Instance.BlankOnExit);
        if (!this.lcd.IsDisabled)
        {
          this.lcd.Initialize();
          this.lcd.SetLine(0, "MiniDisplay");
          this.lcd.SetLine(1, this.lcd.Name);
          Thread.Sleep(5000);
          this.lcd.CleanUp();
        }
        base.Enabled = true;
        base.Activate();
      }
      catch (Exception exception)
      {
        Log.Error("MiniDisplay.SetupForm.btnAdvanced_Click(): CAUGHT EXCEPTION: {0}", new object[] {exception});
        base.Enabled = true;
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void cbLight_CheckedChanged(object sender, EventArgs e)
    {
      if (this.cbLight.Checked)
      {
        this.groupShutdown.Enabled = false;
      }
      else
      {
        this.groupShutdown.Enabled = true;
      }
    }

    private void cmbType_SelectionChangeCommitted(object sender, EventArgs e)
    {
      this.VerifyLCDType();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = new Container();
      this.btnAdvanced = new MPButton();
      this.cmbPort = new MPComboBox();
      this.label1 = new MPLabel();
      this.groupBox1 = new MPGroupBox();
      this.btnTest = new MPButton();
      this.cbControlScreenSaver = new MPCheckBox();
      this.groupShutdown = new GroupBox();
      this.label11 = new Label();
      this.label6 = new Label();
      this.mpShutdown2 = new TextBox();
      this.mpShutdown1 = new TextBox();
      this.cbContrast = new MPCheckBox();
      this.cbDisplayOff = new MPCheckBox();
      this.lblBrightness = new MPLabel();
      this.tbBrightness = new TrackBar();
      this.btnTestDisplay = new MPButton();
      this.lblContrast = new MPLabel();
      this.tbContrast = new TrackBar();
      this.txtScrollDelay = new MPTextBox();
      this.gbGraphMode = new MPGroupBox();
      this.txtPixelsToScroll = new MPTextBox();
      this.mpLabel5 = new MPLabel();
      this.ckForceGraphicText = new MPCheckBox();
      this.txtFontSize = new MPTextBox();
      this.mpLabel2 = new MPLabel();
      this.txtFont = new MPComboBox();
      this.mpLabel1 = new MPLabel();
      this.label8 = new MPLabel();
      this.txtTimG = new MPTextBox();
      this.txtRowsG = new MPTextBox();
      this.txtColsG = new MPTextBox();
      this.label9 = new MPLabel();
      this.label10 = new MPLabel();
      this.mpLabel3 = new MPLabel();
      this.gbTextMode = new MPGroupBox();
      this.mpPrefixChar = new MPTextBox();
      this.mpLabel6 = new MPLabel();
      this.txtCharsToScroll = new MPTextBox();
      this.mpLabel4 = new MPLabel();
      this.label2 = new MPLabel();
      this.txtTim = new MPTextBox();
      this.txtRows = new MPTextBox();
      this.txtCols = new MPTextBox();
      this.label4 = new MPLabel();
      this.label3 = new MPLabel();
      this.label7 = new MPLabel();
      this.cmbType = new MPComboBox();
      this.cbLight = new MPCheckBox();
      this.cbPropertyBrowser = new MPCheckBox();
      this.btnOK = new MPButton();
      this.errorProvider = new ErrorProvider(this.components);
      this.cbExtensiveLogging = new MPCheckBox();
      this.mpDisableGUISetup = new MPCheckBox();
      this.btnCancel = new MPButton();
      this.groupBox1.SuspendLayout();
      this.groupShutdown.SuspendLayout();
      ((ISupportInitialize) (this.tbBrightness)).BeginInit();
      ((ISupportInitialize) (this.tbContrast)).BeginInit();
      this.gbGraphMode.SuspendLayout();
      this.gbTextMode.SuspendLayout();
      ((ISupportInitialize) (this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // btnAdvanced
      // 
      this.btnAdvanced.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.btnAdvanced.Location = new Point(271, 41);
      this.btnAdvanced.Name = "btnAdvanced";
      this.btnAdvanced.Size = new Size(88, 23);
      this.btnAdvanced.TabIndex = 70;
      this.btnAdvanced.Text = "&Advanced";
      this.btnAdvanced.UseVisualStyleBackColor = true;
      this.btnAdvanced.Click += new EventHandler(this.btnAdvanced_Click);
      // 
      // cmbPort
      // 
      this.cmbPort.BorderColor = Color.Empty;
      this.cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbPort.Items.AddRange(new object[]
                                    {
                                      "LPT1",
                                      "LPT2",
                                      "LPT3",
                                      "LPT4",
                                      "USB",
                                      "COM1",
                                      "COM2",
                                      "COM3",
                                      "COM4",
                                      "COM5",
                                      "COM6",
                                      "COM7",
                                      "COM8",
                                      "NONE",
                                      "localhost"
                                    });
      this.cmbPort.Location = new Point(40, 42);
      this.cmbPort.Name = "cmbPort";
      this.cmbPort.Size = new Size(64, 21);
      this.cmbPort.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.Location = new Point(8, 42);
      this.label1.Name = "label1";
      this.label1.Size = new Size(32, 23);
      this.label1.TabIndex = 2;
      this.label1.Text = "Port";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                 | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnTest);
      this.groupBox1.Controls.Add(this.cbControlScreenSaver);
      this.groupBox1.Controls.Add(this.groupShutdown);
      this.groupBox1.Controls.Add(this.cbContrast);
      this.groupBox1.Controls.Add(this.cbDisplayOff);
      this.groupBox1.Controls.Add(this.lblBrightness);
      this.groupBox1.Controls.Add(this.tbBrightness);
      this.groupBox1.Controls.Add(this.btnTestDisplay);
      this.groupBox1.Controls.Add(this.lblContrast);
      this.groupBox1.Controls.Add(this.tbContrast);
      this.groupBox1.Controls.Add(this.txtScrollDelay);
      this.groupBox1.Controls.Add(this.gbGraphMode);
      this.groupBox1.Controls.Add(this.mpLabel3);
      this.groupBox1.Controls.Add(this.gbTextMode);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.cmbType);
      this.groupBox1.Controls.Add(this.cbLight);
      this.groupBox1.Controls.Add(this.btnAdvanced);
      this.groupBox1.Controls.Add(this.cmbPort);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = FlatStyle.Popup;
      this.groupBox1.Location = new Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(373, 392);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Configuration";
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnTest.Location = new Point(271, 338);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new Size(92, 48);
      this.btnTest.TabIndex = 83;
      this.btnTest.Text = "Configuration Editor";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new EventHandler(this.btnTest_Click);
      // 
      // cbControlScreenSaver
      // 
      this.cbControlScreenSaver.AutoSize = true;
      this.cbControlScreenSaver.FlatStyle = FlatStyle.Popup;
      this.cbControlScreenSaver.Location = new Point(8, 262);
      this.cbControlScreenSaver.Name = "cbControlScreenSaver";
      this.cbControlScreenSaver.Size = new Size(219, 17);
      this.cbControlScreenSaver.TabIndex = 82;
      this.cbControlScreenSaver.Text = "Disable ScreenSaver when playing Video";
      this.cbControlScreenSaver.UseVisualStyleBackColor = true;
      // 
      // groupShutdown
      // 
      this.groupShutdown.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.groupShutdown.Controls.Add(this.label11);
      this.groupShutdown.Controls.Add(this.label6);
      this.groupShutdown.Controls.Add(this.mpShutdown2);
      this.groupShutdown.Controls.Add(this.mpShutdown1);
      this.groupShutdown.Location = new Point(8, 321);
      this.groupShutdown.Name = "groupShutdown";
      this.groupShutdown.Size = new Size(191, 65);
      this.groupShutdown.TabIndex = 76;
      this.groupShutdown.TabStop = false;
      this.groupShutdown.Text = " ShutDown Message ";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new Point(6, 43);
      this.label11.Name = "label11";
      this.label11.Size = new Size(36, 13);
      this.label11.TabIndex = 79;
      this.label11.Text = "Line 2";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new Point(6, 20);
      this.label6.Name = "label6";
      this.label6.Size = new Size(36, 13);
      this.label6.TabIndex = 78;
      this.label6.Text = "Line 1";
      // 
      // mpShutdown2
      // 
      this.mpShutdown2.Location = new Point(46, 40);
      this.mpShutdown2.Name = "mpShutdown2";
      this.mpShutdown2.Size = new Size(139, 20);
      this.mpShutdown2.TabIndex = 77;
      // 
      // mpShutdown1
      // 
      this.mpShutdown1.Location = new Point(46, 17);
      this.mpShutdown1.Name = "mpShutdown1";
      this.mpShutdown1.Size = new Size(139, 20);
      this.mpShutdown1.TabIndex = 76;
      // 
      // cbContrast
      // 
      this.cbContrast.AutoSize = true;
      this.cbContrast.Checked = true;
      this.cbContrast.CheckState = CheckState.Checked;
      this.cbContrast.FlatStyle = FlatStyle.Popup;
      this.cbContrast.Location = new Point(190, 262);
      this.cbContrast.Name = "cbContrast";
      this.cbContrast.Size = new Size(98, 17);
      this.cbContrast.TabIndex = 81;
      this.cbContrast.Text = "Control contrast";
      this.cbContrast.UseVisualStyleBackColor = true;
      this.cbContrast.Visible = false;
      // 
      // cbDisplayOff
      // 
      this.cbDisplayOff.AutoSize = true;
      this.cbDisplayOff.FlatStyle = FlatStyle.Popup;
      this.cbDisplayOff.Location = new Point(8, 234);
      this.cbDisplayOff.Name = "cbDisplayOff";
      this.cbDisplayOff.Size = new Size(138, 17);
      this.cbDisplayOff.TabIndex = 80;
      this.cbDisplayOff.Text = "Turn OFF display on exit";
      this.cbDisplayOff.UseVisualStyleBackColor = true;
      // 
      // lblBrightness
      // 
      this.lblBrightness.Location = new Point(16, 280);
      this.lblBrightness.Name = "lblBrightness";
      this.lblBrightness.Size = new Size(96, 16);
      this.lblBrightness.TabIndex = 79;
      this.lblBrightness.Text = "Brightness: ";
      // 
      // tbBrightness
      // 
      this.tbBrightness.Location = new Point(15, 293);
      this.tbBrightness.Maximum = 255;
      this.tbBrightness.Name = "tbBrightness";
      this.tbBrightness.Size = new Size(160, 45);
      this.tbBrightness.TabIndex = 78;
      this.tbBrightness.TickFrequency = 8;
      this.tbBrightness.TickStyle = TickStyle.None;
      this.tbBrightness.Value = 127;
      this.tbBrightness.ValueChanged += new EventHandler(this.tbBrightness_ValueChanged);
      // 
      // btnTestDisplay
      // 
      this.btnTestDisplay.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.btnTestDisplay.Location = new Point(179, 41);
      this.btnTestDisplay.Name = "btnTestDisplay";
      this.btnTestDisplay.Size = new Size(88, 23);
      this.btnTestDisplay.TabIndex = 77;
      this.btnTestDisplay.Text = "&Test Display";
      this.btnTestDisplay.UseVisualStyleBackColor = true;
      this.btnTestDisplay.Click += new EventHandler(this.btnTestDisplay_Click);
      // 
      // lblContrast
      // 
      this.lblContrast.Location = new Point(187, 280);
      this.lblContrast.Name = "lblContrast";
      this.lblContrast.Size = new Size(96, 16);
      this.lblContrast.TabIndex = 74;
      this.lblContrast.Text = "Contrast:";
      // 
      // tbContrast
      // 
      this.tbContrast.Location = new Point(186, 293);
      this.tbContrast.Maximum = 255;
      this.tbContrast.Name = "tbContrast";
      this.tbContrast.Size = new Size(160, 45);
      this.tbContrast.TabIndex = 73;
      this.tbContrast.TickFrequency = 8;
      this.tbContrast.TickStyle = TickStyle.None;
      this.tbContrast.Value = 127;
      this.tbContrast.ValueChanged += new EventHandler(this.tbContrast_ValueChanged);
      // 
      // txtScrollDelay
      // 
      this.txtScrollDelay.BorderColor = Color.Empty;
      this.txtScrollDelay.Location = new Point(96, 209);
      this.txtScrollDelay.Name = "txtScrollDelay";
      this.txtScrollDelay.Size = new Size(48, 20);
      this.txtScrollDelay.TabIndex = 52;
      this.txtScrollDelay.Text = "300";
      // 
      // gbGraphMode
      // 
      this.gbGraphMode.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.gbGraphMode.Controls.Add(this.txtPixelsToScroll);
      this.gbGraphMode.Controls.Add(this.mpLabel5);
      this.gbGraphMode.Controls.Add(this.ckForceGraphicText);
      this.gbGraphMode.Controls.Add(this.txtFontSize);
      this.gbGraphMode.Controls.Add(this.mpLabel2);
      this.gbGraphMode.Controls.Add(this.txtFont);
      this.gbGraphMode.Controls.Add(this.mpLabel1);
      this.gbGraphMode.Controls.Add(this.label8);
      this.gbGraphMode.Controls.Add(this.txtTimG);
      this.gbGraphMode.Controls.Add(this.txtRowsG);
      this.gbGraphMode.Controls.Add(this.txtColsG);
      this.gbGraphMode.Controls.Add(this.label9);
      this.gbGraphMode.Controls.Add(this.label10);
      this.gbGraphMode.FlatStyle = FlatStyle.Popup;
      this.gbGraphMode.Location = new Point(168, 68);
      this.gbGraphMode.Name = "gbGraphMode";
      this.gbGraphMode.Size = new Size(191, 188);
      this.gbGraphMode.TabIndex = 72;
      this.gbGraphMode.TabStop = false;
      this.gbGraphMode.Text = "GraphMode";
      // 
      // txtPixelsToScroll
      // 
      this.txtPixelsToScroll.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                        | AnchorStyles.Right)));
      this.txtPixelsToScroll.BorderColor = Color.Empty;
      this.txtPixelsToScroll.Location = new Point(86, 135);
      this.txtPixelsToScroll.Name = "txtPixelsToScroll";
      this.txtPixelsToScroll.Size = new Size(48, 20);
      this.txtPixelsToScroll.TabIndex = 57;
      this.txtPixelsToScroll.Text = "10";
      // 
      // mpLabel5
      // 
      this.mpLabel5.Location = new Point(8, 133);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new Size(80, 23);
      this.mpLabel5.TabIndex = 56;
      this.mpLabel5.Text = "Pixels to scroll";
      this.mpLabel5.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // ckForceGraphicText
      // 
      this.ckForceGraphicText.AutoSize = true;
      this.ckForceGraphicText.FlatStyle = FlatStyle.Popup;
      this.ckForceGraphicText.Location = new Point(11, 161);
      this.ckForceGraphicText.Name = "ckForceGraphicText";
      this.ckForceGraphicText.Size = new Size(123, 17);
      this.ckForceGraphicText.TabIndex = 55;
      this.ckForceGraphicText.Text = "Force Graphical Text";
      this.ckForceGraphicText.UseVisualStyleBackColor = true;
      // 
      // txtFontSize
      // 
      this.txtFontSize.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                  | AnchorStyles.Right)));
      this.txtFontSize.BorderColor = Color.Empty;
      this.txtFontSize.Location = new Point(86, 110);
      this.txtFontSize.Name = "txtFontSize";
      this.txtFontSize.Size = new Size(48, 20);
      this.txtFontSize.TabIndex = 54;
      this.txtFontSize.Text = "10";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new Point(8, 110);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new Size(64, 23);
      this.mpLabel2.TabIndex = 53;
      this.mpLabel2.Text = "Font Size";
      this.mpLabel2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // txtFont
      // 
      this.txtFont.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                              | AnchorStyles.Right)));
      this.txtFont.BorderColor = Color.Empty;
      this.txtFont.Location = new Point(86, 87);
      this.txtFont.Name = "txtFont";
      this.txtFont.Size = new Size(99, 21);
      this.txtFont.TabIndex = 52;
      this.txtFont.Text = "Arial Black";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new Point(8, 87);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new Size(80, 23);
      this.mpLabel1.TabIndex = 51;
      this.mpLabel1.Text = "Font";
      this.mpLabel1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // label8
      // 
      this.label8.Location = new Point(8, 16);
      this.label8.Name = "label8";
      this.label8.Size = new Size(64, 23);
      this.label8.TabIndex = 3;
      this.label8.Text = "Columns";
      this.label8.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // txtTimG
      // 
      this.txtTimG.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                              | AnchorStyles.Right)));
      this.txtTimG.BorderColor = Color.Empty;
      this.txtTimG.Location = new Point(86, 64);
      this.txtTimG.Name = "txtTimG";
      this.txtTimG.Size = new Size(48, 20);
      this.txtTimG.TabIndex = 50;
      this.txtTimG.Text = "1";
      // 
      // txtRowsG
      // 
      this.txtRowsG.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.txtRowsG.BorderColor = Color.Empty;
      this.txtRowsG.Location = new Point(86, 40);
      this.txtRowsG.Name = "txtRowsG";
      this.txtRowsG.Size = new Size(48, 20);
      this.txtRowsG.TabIndex = 40;
      this.txtRowsG.Text = "240";
      // 
      // txtColsG
      // 
      this.txtColsG.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.txtColsG.BorderColor = Color.Empty;
      this.txtColsG.Location = new Point(86, 16);
      this.txtColsG.Name = "txtColsG";
      this.txtColsG.Size = new Size(48, 20);
      this.txtColsG.TabIndex = 30;
      this.txtColsG.Text = "320";
      // 
      // label9
      // 
      this.label9.Location = new Point(8, 64);
      this.label9.Name = "label9";
      this.label9.Size = new Size(80, 23);
      this.label9.TabIndex = 5;
      this.label9.Text = "Comm. Delay";
      this.label9.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // label10
      // 
      this.label10.Location = new Point(8, 40);
      this.label10.Name = "label10";
      this.label10.Size = new Size(72, 23);
      this.label10.TabIndex = 4;
      this.label10.Text = "Rows";
      this.label10.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new Point(5, 207);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new Size(80, 23);
      this.mpLabel3.TabIndex = 51;
      this.mpLabel3.Text = "Scroll Delay";
      this.mpLabel3.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // gbTextMode
      // 
      this.gbTextMode.Controls.Add(this.mpPrefixChar);
      this.gbTextMode.Controls.Add(this.mpLabel6);
      this.gbTextMode.Controls.Add(this.txtCharsToScroll);
      this.gbTextMode.Controls.Add(this.mpLabel4);
      this.gbTextMode.Controls.Add(this.label2);
      this.gbTextMode.Controls.Add(this.txtTim);
      this.gbTextMode.Controls.Add(this.txtRows);
      this.gbTextMode.Controls.Add(this.txtCols);
      this.gbTextMode.Controls.Add(this.label4);
      this.gbTextMode.Controls.Add(this.label3);
      this.gbTextMode.FlatStyle = FlatStyle.Popup;
      this.gbTextMode.Location = new Point(8, 68);
      this.gbTextMode.Name = "gbTextMode";
      this.gbTextMode.Size = new Size(152, 136);
      this.gbTextMode.TabIndex = 71;
      this.gbTextMode.TabStop = false;
      this.gbTextMode.Text = "TextMode";
      // 
      // mpPrefixChar
      // 
      this.mpPrefixChar.BorderColor = Color.Empty;
      this.mpPrefixChar.Location = new Point(88, 112);
      this.mpPrefixChar.Name = "mpPrefixChar";
      this.mpPrefixChar.Size = new Size(48, 20);
      this.mpPrefixChar.TabIndex = 56;
      this.mpPrefixChar.Visible = false;
      // 
      // mpLabel6
      // 
      this.mpLabel6.Location = new Point(8, 112);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new Size(88, 23);
      this.mpLabel6.TabIndex = 55;
      this.mpLabel6.Text = "Line Prefix     0x";
      this.mpLabel6.TextAlign = ContentAlignment.MiddleLeft;
      this.mpLabel6.Visible = false;
      // 
      // txtCharsToScroll
      // 
      this.txtCharsToScroll.BorderColor = Color.Empty;
      this.txtCharsToScroll.Location = new Point(88, 87);
      this.txtCharsToScroll.Name = "txtCharsToScroll";
      this.txtCharsToScroll.Size = new Size(48, 20);
      this.txtCharsToScroll.TabIndex = 54;
      this.txtCharsToScroll.Text = "1";
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new Point(8, 87);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new Size(80, 23);
      this.mpLabel4.TabIndex = 53;
      this.mpLabel4.Text = "#Chars to scroll";
      this.mpLabel4.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Location = new Point(8, 16);
      this.label2.Name = "label2";
      this.label2.Size = new Size(64, 23);
      this.label2.TabIndex = 3;
      this.label2.Text = "Columns";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // txtTim
      // 
      this.txtTim.BorderColor = Color.Empty;
      this.txtTim.Location = new Point(88, 64);
      this.txtTim.Name = "txtTim";
      this.txtTim.Size = new Size(48, 20);
      this.txtTim.TabIndex = 50;
      this.txtTim.Text = "1";
      // 
      // txtRows
      // 
      this.txtRows.BorderColor = Color.Empty;
      this.txtRows.Location = new Point(88, 40);
      this.txtRows.Name = "txtRows";
      this.txtRows.Size = new Size(48, 20);
      this.txtRows.TabIndex = 40;
      this.txtRows.Text = "2";
      // 
      // txtCols
      // 
      this.txtCols.BorderColor = Color.Empty;
      this.txtCols.Location = new Point(88, 16);
      this.txtCols.Name = "txtCols";
      this.txtCols.Size = new Size(48, 20);
      this.txtCols.TabIndex = 30;
      this.txtCols.Text = "16";
      this.txtCols.TextChanged += new EventHandler(this.txtCols_TextChanged);
      // 
      // label4
      // 
      this.label4.Location = new Point(8, 64);
      this.label4.Name = "label4";
      this.label4.Size = new Size(80, 23);
      this.label4.TabIndex = 5;
      this.label4.Text = "Comm. Delay";
      this.label4.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Location = new Point(8, 40);
      this.label3.Name = "label3";
      this.label3.Size = new Size(72, 23);
      this.label3.TabIndex = 4;
      this.label3.Text = "Rows";
      this.label3.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Location = new Point(8, 16);
      this.label7.Name = "label7";
      this.label7.Size = new Size(32, 23);
      this.label7.TabIndex = 11;
      this.label7.Text = "Type";
      this.label7.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      this.cmbType.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                              | AnchorStyles.Right)));
      this.cmbType.BorderColor = Color.Empty;
      this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbType.Location = new Point(40, 16);
      this.cmbType.Name = "cmbType";
      this.cmbType.Size = new Size(319, 21);
      this.cmbType.Sorted = true;
      this.cmbType.TabIndex = 10;
      this.cmbType.SelectionChangeCommitted += new EventHandler(this.cmbType_SelectionChangeCommitted);
      // 
      // cbLight
      // 
      this.cbLight.AutoSize = true;
      this.cbLight.Checked = true;
      this.cbLight.CheckState = CheckState.Checked;
      this.cbLight.FlatStyle = FlatStyle.Popup;
      this.cbLight.Location = new Point(19, 262);
      this.cbLight.Name = "cbLight";
      this.cbLight.Size = new Size(108, 17);
      this.cbLight.TabIndex = 60;
      this.cbLight.Text = "Control brightness";
      this.cbLight.UseVisualStyleBackColor = true;
      this.cbLight.Visible = false;
      this.cbLight.CheckedChanged += new EventHandler(this.cbLight_CheckedChanged);
      // 
      // cbPropertyBrowser
      // 
      this.cbPropertyBrowser.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.cbPropertyBrowser.AutoSize = true;
      this.cbPropertyBrowser.FlatStyle = FlatStyle.Popup;
      this.cbPropertyBrowser.Location = new Point(8, 437);
      this.cbPropertyBrowser.Name = "cbPropertyBrowser";
      this.cbPropertyBrowser.Size = new Size(132, 17);
      this.cbPropertyBrowser.TabIndex = 4;
      this.cbPropertyBrowser.Text = "Show property browser";
      this.cbPropertyBrowser.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnOK.Location = new Point(213, 425);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(78, 23);
      this.btnOK.TabIndex = 5;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // cbExtensiveLogging
      // 
      this.cbExtensiveLogging.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.cbExtensiveLogging.AutoSize = true;
      this.cbExtensiveLogging.FlatStyle = FlatStyle.Popup;
      this.cbExtensiveLogging.Location = new Point(8, 420);
      this.cbExtensiveLogging.Name = "cbExtensiveLogging";
      this.cbExtensiveLogging.Size = new Size(107, 17);
      this.cbExtensiveLogging.TabIndex = 6;
      this.cbExtensiveLogging.Text = "Extensive logging";
      this.cbExtensiveLogging.UseVisualStyleBackColor = true;
      // 
      // mpDisableGUISetup
      // 
      this.mpDisableGUISetup.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.mpDisableGUISetup.AutoSize = true;
      this.mpDisableGUISetup.FlatStyle = FlatStyle.Popup;
      this.mpDisableGUISetup.Location = new Point(8, 403);
      this.mpDisableGUISetup.Name = "mpDisableGUISetup";
      this.mpDisableGUISetup.Size = new Size(112, 17);
      this.mpDisableGUISetup.TabIndex = 77;
      this.mpDisableGUISetup.Text = "Disable GUI Setup";
      this.mpDisableGUISetup.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(297, 425);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(78, 23);
      this.btnCancel.TabIndex = 78;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
      // 
      // SetupForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new Size(389, 454);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.mpDisableGUISetup);
      this.Controls.Add(this.cbExtensiveLogging);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.cbPropertyBrowser);
      this.Controls.Add(this.groupBox1);
      this.Name = "SetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup";
      this.Load += new EventHandler(this.SetupForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupShutdown.ResumeLayout(false);
      this.groupShutdown.PerformLayout();
      ((ISupportInitialize) (this.tbBrightness)).EndInit();
      ((ISupportInitialize) (this.tbContrast)).EndInit();
      this.gbGraphMode.ResumeLayout(false);
      this.gbGraphMode.PerformLayout();
      this.gbTextMode.ResumeLayout(false);
      this.gbTextMode.PerformLayout();
      ((ISupportInitialize) (this.errorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      Log.Info("MiniDisplay.SetupForm.Load(): called", new object[0]);
      foreach (FontFamily family in FontFamily.Families)
      {
        this.txtFont.Items.Add(family.Name);
      }
      Log.Info("MiniDisplay.SetupForm.Load(): completed", new object[0]);
    }

    private void tbBrightness_ValueChanged(object sender, EventArgs e)
    {
      this.lblBrightness.Text = "Brightness: " + this.tbBrightness.Value.ToString();
    }

    private void tbContrast_ValueChanged(object sender, EventArgs e)
    {
      this.lblContrast.Text = "Contrast: " + this.tbContrast.Value.ToString();
    }

    private void txtCols_TextChanged(object sender, EventArgs e)
    {
      int length = int.Parse(this.txtCols.Text);
      this.mpShutdown1.MaxLength = length;
      if (this.mpShutdown1.Text.Length > length)
      {
        this.mpShutdown1.Text = this.mpShutdown1.Text.Substring(0, length);
      }
      this.mpShutdown2.MaxLength = length;
      if (this.mpShutdown2.Text.Length > length)
      {
        this.mpShutdown2.Text = this.mpShutdown2.Text.Substring(0, length);
      }
    }

    private void VerifyLCDType()
    {
      Log.Info("MiniDisplay.SetupForm.VerifyLCDType(): called", new object[0]);
      this.lcd = this.cmbType.SelectedItem as IDisplay;
      if (this.lcd.IsDisabled)
      {
        this.errorProvider.SetError(this.cmbType, this.lcd.ErrorMessage);
        this.btnOK.Enabled = false;
      }
      else
      {
        this.errorProvider.SetError(this.cmbType, null);
        this.btnOK.Enabled = true;
      }
      this.gbGraphMode.Visible = this.lcd.SupportsGraphics;
      this.gbTextMode.Visible = this.lcd.SupportsText;
      Settings.Instance.LCDType = this.lcd;
      Log.Info("MiniDisplay.SetupForm.Load(): completed", new object[0]);
    }
  }
}