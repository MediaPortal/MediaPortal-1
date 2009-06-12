using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class GenericSerial_AdvancedSetupForm : MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
    private CheckBox cbAssertDTR;
    private CheckBox cbAssertRTS;
    private MPComboBox cbBaudRate;
    private MPComboBox cbDataBits;
    private MPComboBox cbParity;
    private CheckBox cbPositionBase1;
    private MPComboBox cbStopBits;
    private CheckBox cbToggleDTR;
    private CheckBox cbToggleRTS;
    private MPComboBox cmbBlankIdleTime;
    private readonly IContainer components = null;
    private object ControlStateLock = new object();
    private MPGroupBox groupBoxConfiguration;
    private GroupBox groupBoxDisplayCommands;
    private GroupBox groupBoxDisplayControl;
    private GroupBox groupBoxPortParameters;
    private Label lblBaudRate;
    private Label lblCmdClearDisplay;
    private Label lblCmdCursorHome;
    private Label lblCmdCursorLeft;
    private Label lblCmdCursorRight;
    private Label lblCmdDisplayClose;
    private Label lblCmdDisplayInit;
    private Label lblCursorDown;
    private Label lblCursorSet;
    private Label lblCursorUp;
    private Label lblDataBits;
    private Label lblNote1;
    private Label lblNote2;
    private Label lblNote3;
    private Label lblParity;
    private Label lblStopBits;
    private CheckBox mpBlankDisplayWhenIdle;
    private CheckBox mpBlankDisplayWithVideo;
    private CheckBox mpEnableDisplayAction;
    private MPComboBox mpEnableDisplayActionTime;
    private TextBox tbCmdClearDisplay;
    private TextBox tbCmdCursorDown;
    private TextBox tbCmdCursorHome;
    private TextBox tbCmdCursorLeft;
    private TextBox tbCmdCursorRight;
    private TextBox tbCmdCursorSet;
    private TextBox tbCmdCursorUp;
    private TextBox tbCmdDisplayClose;
    private TextBox tbCmdDisplayInit;

    public GenericSerial_AdvancedSetupForm()
    {
      Log.Debug("GenericSerial_AdvancedSetupForm(): Constructor started");
      this.InitializeComponent();
      this.cbBaudRate.DataBindings.Add("SelectedItem", GenericSerial.AdvancedSettings.Instance, "BaudRate");
      this.cbParity.DataBindings.Add("SelectedItem", GenericSerial.AdvancedSettings.Instance, "Parity");
      this.cbStopBits.DataBindings.Add("SelectedItem", GenericSerial.AdvancedSettings.Instance, "StopBits");
      this.cbDataBits.DataBindings.Add("SelectedItem", GenericSerial.AdvancedSettings.Instance, "DataBits");
      this.tbCmdDisplayInit.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_DisplayInit");
      this.tbCmdClearDisplay.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_ClearDisplay");
      this.tbCmdCursorLeft.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorLeft");
      this.tbCmdCursorRight.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorRight");
      this.tbCmdCursorUp.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorUp");
      this.tbCmdCursorDown.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorDown");
      this.tbCmdCursorHome.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorHome");
      this.tbCmdCursorSet.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_CursorSet");
      this.tbCmdDisplayClose.DataBindings.Add("Text", GenericSerial.AdvancedSettings.Instance, "CMD_DisplayClose");
      this.cbPositionBase1.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance, "PositionBase1");
      this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance,
                                                    "BlankDisplayWithVideo");
      this.mpEnableDisplayAction.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance,
                                                  "EnableDisplayAction");
      this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", GenericSerial.AdvancedSettings.Instance,
                                                      "EnableDisplayActionTime");
      this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance,
                                                   "BlankDisplayWhenIdle");
      this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", GenericSerial.AdvancedSettings.Instance, "BlankIdleTime");
      this.cbAssertRTS.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance, "AssertRTS");
      this.cbAssertDTR.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance, "AssertDTR");
      this.cbToggleRTS.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance, "CMD_ToggleRTS");
      this.cbToggleDTR.DataBindings.Add("Checked", GenericSerial.AdvancedSettings.Instance, "CMD_ToggleDTR");
      this.Refresh();
      this.SetControlState();
      Log.Debug("GenericSerial_AdvancedSetupForm(): Constructor completed");
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("GenericSerial_AdvancedSetupForm.btnOK_Click(): started");
      GenericSerial.AdvancedSettings.Save();
      base.Hide();
      base.Close();
      Log.Debug("GenericSerial_AdvancedSetupForm.btnOK_Click(): Completed");
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Log.Debug("GenericSerial_AdvancedSetupForm.btnReset_Click(): started");
      GenericSerial.AdvancedSettings.SetDefaults();
      this.cbBaudRate.SelectedItem = GenericSerial.AdvancedSettings.Instance.BaudRate;
      this.tbCmdDisplayInit.Text = GenericSerial.AdvancedSettings.Instance.CMD_DisplayInit;
      this.tbCmdClearDisplay.Text = GenericSerial.AdvancedSettings.Instance.CMD_ClearDisplay;
      this.tbCmdCursorLeft.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorLeft;
      this.tbCmdCursorRight.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorRight;
      this.tbCmdCursorUp.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorUp;
      this.tbCmdCursorDown.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorDown;
      this.tbCmdCursorHome.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorHome;
      this.tbCmdCursorSet.Text = GenericSerial.AdvancedSettings.Instance.CMD_CursorSet;
      this.tbCmdDisplayClose.Text = GenericSerial.AdvancedSettings.Instance.CMD_DisplayClose;
      this.cbPositionBase1.Checked = GenericSerial.AdvancedSettings.Instance.PositionBase1;
      this.mpBlankDisplayWithVideo.Checked = GenericSerial.AdvancedSettings.Instance.BlankDisplayWithVideo;
      this.mpEnableDisplayAction.Checked = GenericSerial.AdvancedSettings.Instance.EnableDisplayAction;
      this.mpEnableDisplayActionTime.SelectedIndex = GenericSerial.AdvancedSettings.Instance.EnableDisplayActionTime;
      this.mpBlankDisplayWhenIdle.Checked = GenericSerial.AdvancedSettings.Instance.BlankDisplayWhenIdle;
      this.cmbBlankIdleTime.SelectedIndex = GenericSerial.AdvancedSettings.Instance.BlankIdleTime;
      this.cbAssertDTR.Checked = GenericSerial.AdvancedSettings.Instance.AssertDTR;
      this.cbAssertRTS.Checked = GenericSerial.AdvancedSettings.Instance.AssertRTS;
      this.cbToggleDTR.Checked = GenericSerial.AdvancedSettings.Instance.CMD_ToggleDTR;
      this.cbToggleRTS.Checked = GenericSerial.AdvancedSettings.Instance.CMD_ToggleRTS;
      this.Refresh();
      Log.Debug("GenericSerial_AdvancedSetupForm.btnReset_Click(): Completed");
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
      this.groupBoxConfiguration = new MPGroupBox();
      this.groupBoxPortParameters = new GroupBox();
      this.cbAssertDTR = new CheckBox();
      this.cbAssertRTS = new CheckBox();
      this.lblDataBits = new Label();
      this.cbDataBits = new MPComboBox();
      this.lblStopBits = new Label();
      this.cbStopBits = new MPComboBox();
      this.lblParity = new Label();
      this.cbParity = new MPComboBox();
      this.lblBaudRate = new Label();
      this.cbBaudRate = new MPComboBox();
      this.groupBoxDisplayCommands = new GroupBox();
      this.cbToggleDTR = new CheckBox();
      this.cbToggleRTS = new CheckBox();
      this.cbPositionBase1 = new CheckBox();
      this.lblNote3 = new Label();
      this.tbCmdCursorSet = new TextBox();
      this.lblCursorSet = new Label();
      this.tbCmdDisplayClose = new TextBox();
      this.lblCmdDisplayClose = new Label();
      this.lblNote2 = new Label();
      this.lblNote1 = new Label();
      this.tbCmdCursorHome = new TextBox();
      this.tbCmdCursorDown = new TextBox();
      this.tbCmdCursorUp = new TextBox();
      this.tbCmdCursorRight = new TextBox();
      this.tbCmdCursorLeft = new TextBox();
      this.tbCmdClearDisplay = new TextBox();
      this.tbCmdDisplayInit = new TextBox();
      this.lblCmdCursorHome = new Label();
      this.lblCursorDown = new Label();
      this.lblCursorUp = new Label();
      this.lblCmdCursorRight = new Label();
      this.lblCmdClearDisplay = new Label();
      this.lblCmdCursorLeft = new Label();
      this.lblCmdDisplayInit = new Label();
      this.groupBoxDisplayControl = new GroupBox();
      this.mpEnableDisplayActionTime = new MPComboBox();
      this.cmbBlankIdleTime = new MPComboBox();
      this.mpEnableDisplayAction = new CheckBox();
      this.mpBlankDisplayWithVideo = new CheckBox();
      this.mpBlankDisplayWhenIdle = new CheckBox();
      this.btnOK = new MPButton();
      this.btnReset = new MPButton();
      this.groupBoxConfiguration.SuspendLayout();
      this.groupBoxPortParameters.SuspendLayout();
      this.groupBoxDisplayCommands.SuspendLayout();
      this.groupBoxDisplayControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxConfiguration
      // 
      this.groupBoxConfiguration.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                             | AnchorStyles.Left)
                                                            | AnchorStyles.Right)));
      this.groupBoxConfiguration.Controls.Add(this.groupBoxPortParameters);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxDisplayCommands);
      this.groupBoxConfiguration.Controls.Add(this.groupBoxDisplayControl);
      this.groupBoxConfiguration.FlatStyle = FlatStyle.Popup;
      this.groupBoxConfiguration.Location = new Point(9, 6);
      this.groupBoxConfiguration.Name = "groupBoxConfiguration";
      this.groupBoxConfiguration.Size = new Size(357, 532);
      this.groupBoxConfiguration.TabIndex = 4;
      this.groupBoxConfiguration.TabStop = false;
      this.groupBoxConfiguration.Text = " Simple Serial Display Configuration ";
      // 
      // groupBoxPortParameters
      // 
      this.groupBoxPortParameters.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                             | AnchorStyles.Right)));
      this.groupBoxPortParameters.Controls.Add(this.cbAssertDTR);
      this.groupBoxPortParameters.Controls.Add(this.cbAssertRTS);
      this.groupBoxPortParameters.Controls.Add(this.lblDataBits);
      this.groupBoxPortParameters.Controls.Add(this.cbDataBits);
      this.groupBoxPortParameters.Controls.Add(this.lblStopBits);
      this.groupBoxPortParameters.Controls.Add(this.cbStopBits);
      this.groupBoxPortParameters.Controls.Add(this.lblParity);
      this.groupBoxPortParameters.Controls.Add(this.cbParity);
      this.groupBoxPortParameters.Controls.Add(this.lblBaudRate);
      this.groupBoxPortParameters.Controls.Add(this.cbBaudRate);
      this.groupBoxPortParameters.Location = new Point(10, 19);
      this.groupBoxPortParameters.Name = "groupBoxPortParameters";
      this.groupBoxPortParameters.Size = new Size(338, 92);
      this.groupBoxPortParameters.TabIndex = 105;
      this.groupBoxPortParameters.TabStop = false;
      this.groupBoxPortParameters.Text = " Serial Port parameters ";
      // 
      // cbAssertDTR
      // 
      this.cbAssertDTR.AutoSize = true;
      this.cbAssertDTR.Location = new Point(169, 65);
      this.cbAssertDTR.Name = "cbAssertDTR";
      this.cbAssertDTR.Size = new Size(147, 17);
      this.cbAssertDTR.TabIndex = 114;
      this.cbAssertDTR.Text = "Assert DTR on Port Open";
      this.cbAssertDTR.UseVisualStyleBackColor = true;
      // 
      // cbAssertRTS
      // 
      this.cbAssertRTS.AutoSize = true;
      this.cbAssertRTS.Location = new Point(10, 65);
      this.cbAssertRTS.Name = "cbAssertRTS";
      this.cbAssertRTS.Size = new Size(146, 17);
      this.cbAssertRTS.TabIndex = 113;
      this.cbAssertRTS.Text = "Assert RTS on Port Open";
      this.cbAssertRTS.UseVisualStyleBackColor = true;
      // 
      // lblDataBits
      // 
      this.lblDataBits.AutoSize = true;
      this.lblDataBits.Location = new Point(179, 20);
      this.lblDataBits.Name = "lblDataBits";
      this.lblDataBits.Size = new Size(50, 13);
      this.lblDataBits.TabIndex = 112;
      this.lblDataBits.Text = "Data Bits";
      // 
      // cbDataBits
      // 
      this.cbDataBits.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                 | AnchorStyles.Right)));
      this.cbDataBits.BorderColor = Color.Empty;
      this.cbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbDataBits.Items.AddRange(new object[]
                                       {
                                         "5",
                                         "6",
                                         "7",
                                         "8"
                                       });
      this.cbDataBits.Location = new Point(243, 16);
      this.cbDataBits.Name = "cbDataBits";
      this.cbDataBits.Size = new Size(90, 21);
      this.cbDataBits.TabIndex = 111;
      // 
      // lblStopBits
      // 
      this.lblStopBits.AutoSize = true;
      this.lblStopBits.Location = new Point(179, 42);
      this.lblStopBits.Name = "lblStopBits";
      this.lblStopBits.Size = new Size(49, 13);
      this.lblStopBits.TabIndex = 110;
      this.lblStopBits.Text = "Stop Bits";
      // 
      // cbStopBits
      // 
      this.cbStopBits.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                 | AnchorStyles.Right)));
      this.cbStopBits.BorderColor = Color.Empty;
      this.cbStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbStopBits.Items.AddRange(new object[]
                                       {
                                         "None",
                                         "One",
                                         "OnePointFive",
                                         "Two"
                                       });
      this.cbStopBits.Location = new Point(243, 38);
      this.cbStopBits.Name = "cbStopBits";
      this.cbStopBits.Size = new Size(90, 21);
      this.cbStopBits.TabIndex = 109;
      // 
      // lblParity
      // 
      this.lblParity.AutoSize = true;
      this.lblParity.Location = new Point(5, 42);
      this.lblParity.Name = "lblParity";
      this.lblParity.Size = new Size(33, 13);
      this.lblParity.TabIndex = 108;
      this.lblParity.Text = "Parity";
      // 
      // cbParity
      // 
      this.cbParity.BorderColor = Color.Empty;
      this.cbParity.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbParity.Items.AddRange(new object[]
                                     {
                                       "Even",
                                       "Mark",
                                       "None",
                                       "Odd",
                                       "Space"
                                     });
      this.cbParity.Location = new Point(69, 38);
      this.cbParity.Name = "cbParity";
      this.cbParity.Size = new Size(90, 21);
      this.cbParity.TabIndex = 107;
      // 
      // lblBaudRate
      // 
      this.lblBaudRate.AutoSize = true;
      this.lblBaudRate.Location = new Point(5, 20);
      this.lblBaudRate.Name = "lblBaudRate";
      this.lblBaudRate.Size = new Size(58, 13);
      this.lblBaudRate.TabIndex = 106;
      this.lblBaudRate.Text = "Baud Rate";
      // 
      // cbBaudRate
      // 
      this.cbBaudRate.BorderColor = Color.Empty;
      this.cbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbBaudRate.Items.AddRange(new object[]
                                       {
                                         "1200",
                                         "2400",
                                         "4800",
                                         "9600",
                                         "19200",
                                         "38400",
                                         "115200"
                                       });
      this.cbBaudRate.Location = new Point(69, 16);
      this.cbBaudRate.Name = "cbBaudRate";
      this.cbBaudRate.Size = new Size(90, 21);
      this.cbBaudRate.TabIndex = 105;
      // 
      // groupBoxDisplayCommands
      // 
      this.groupBoxDisplayCommands.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                               | AnchorStyles.Left)
                                                              | AnchorStyles.Right)));
      this.groupBoxDisplayCommands.Controls.Add(this.cbToggleDTR);
      this.groupBoxDisplayCommands.Controls.Add(this.cbToggleRTS);
      this.groupBoxDisplayCommands.Controls.Add(this.cbPositionBase1);
      this.groupBoxDisplayCommands.Controls.Add(this.lblNote3);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorSet);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCursorSet);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdDisplayClose);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdDisplayClose);
      this.groupBoxDisplayCommands.Controls.Add(this.lblNote2);
      this.groupBoxDisplayCommands.Controls.Add(this.lblNote1);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorHome);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorDown);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorUp);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorRight);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdCursorLeft);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdClearDisplay);
      this.groupBoxDisplayCommands.Controls.Add(this.tbCmdDisplayInit);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdCursorHome);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCursorDown);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCursorUp);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdCursorRight);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdClearDisplay);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdCursorLeft);
      this.groupBoxDisplayCommands.Controls.Add(this.lblCmdDisplayInit);
      this.groupBoxDisplayCommands.Location = new Point(10, 117);
      this.groupBoxDisplayCommands.Name = "groupBoxDisplayCommands";
      this.groupBoxDisplayCommands.Size = new Size(338, 317);
      this.groupBoxDisplayCommands.TabIndex = 24;
      this.groupBoxDisplayCommands.TabStop = false;
      this.groupBoxDisplayCommands.Text = " Display Commands ";
      // 
      // cbToggleDTR
      // 
      this.cbToggleDTR.AutoSize = true;
      this.cbToggleDTR.Location = new Point(169, 262);
      this.cbToggleDTR.Name = "cbToggleDTR";
      this.cbToggleDTR.Size = new Size(155, 17);
      this.cbToggleDTR.TabIndex = 118;
      this.cbToggleDTR.Text = "Toggle DTR on CMD Send";
      this.cbToggleDTR.UseVisualStyleBackColor = true;
      // 
      // cbToggleRTS
      // 
      this.cbToggleRTS.AutoSize = true;
      this.cbToggleRTS.Location = new Point(10, 262);
      this.cbToggleRTS.Name = "cbToggleRTS";
      this.cbToggleRTS.Size = new Size(154, 17);
      this.cbToggleRTS.TabIndex = 117;
      this.cbToggleRTS.Text = "Toggle RTS on CMD Send";
      this.cbToggleRTS.UseVisualStyleBackColor = true;
      // 
      // cbPositionBase1
      // 
      this.cbPositionBase1.AutoSize = true;
      this.cbPositionBase1.Location = new Point(10, 239);
      this.cbPositionBase1.Name = "cbPositionBase1";
      this.cbPositionBase1.Size = new Size(269, 17);
      this.cbPositionBase1.TabIndex = 21;
      this.cbPositionBase1.Text = "Display uses Row = 1, Column = 1 as home position";
      this.cbPositionBase1.UseVisualStyleBackColor = true;
      // 
      // lblNote3
      // 
      this.lblNote3.AutoSize = true;
      this.lblNote3.Location = new Point(7, 193);
      this.lblNote3.Name = "lblNote3";
      this.lblNote3.Size = new Size(328, 13);
      this.lblNote3.TabIndex = 20;
      this.lblNote3.Text = "Use PL = Line, PC = column, TL = line (ASCII),  TC = column (ASCII)";
      // 
      // tbCmdCursorSet
      // 
      this.tbCmdCursorSet.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                     | AnchorStyles.Right)));
      this.tbCmdCursorSet.Location = new Point(88, 169);
      this.tbCmdCursorSet.Name = "tbCmdCursorSet";
      this.tbCmdCursorSet.Size = new Size(244, 20);
      this.tbCmdCursorSet.TabIndex = 19;
      // 
      // lblCursorSet
      // 
      this.lblCursorSet.AutoSize = true;
      this.lblCursorSet.Location = new Point(6, 172);
      this.lblCursorSet.Name = "lblCursorSet";
      this.lblCursorSet.Size = new Size(56, 13);
      this.lblCursorSet.TabIndex = 18;
      this.lblCursorSet.Text = "Cursor Set";
      // 
      // tbCmdDisplayClose
      // 
      this.tbCmdDisplayClose.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                        | AnchorStyles.Right)));
      this.tbCmdDisplayClose.Location = new Point(88, 213);
      this.tbCmdDisplayClose.Name = "tbCmdDisplayClose";
      this.tbCmdDisplayClose.Size = new Size(244, 20);
      this.tbCmdDisplayClose.TabIndex = 17;
      // 
      // lblCmdDisplayClose
      // 
      this.lblCmdDisplayClose.AutoSize = true;
      this.lblCmdDisplayClose.Location = new Point(6, 216);
      this.lblCmdDisplayClose.Name = "lblCmdDisplayClose";
      this.lblCmdDisplayClose.Size = new Size(70, 13);
      this.lblCmdDisplayClose.TabIndex = 16;
      this.lblCmdDisplayClose.Text = "Display Close";
      // 
      // lblNote2
      // 
      this.lblNote2.AutoSize = true;
      this.lblNote2.Location = new Point(57, 301);
      this.lblNote2.Name = "lblNote2";
      this.lblNote2.Size = new Size(180, 13);
      this.lblNote2.TabIndex = 15;
      this.lblNote2.Text = "seperated by a space. IE: 0x08 0x11";
      // 
      // lblNote1
      // 
      this.lblNote1.AutoSize = true;
      this.lblNote1.Location = new Point(20, 286);
      this.lblNote1.Name = "lblNote1";
      this.lblNote1.Size = new Size(283, 13);
      this.lblNote1.TabIndex = 14;
      this.lblNote1.Text = "NOTE: Command bytes should be entered in HEX notation";
      // 
      // tbCmdCursorHome
      // 
      this.tbCmdCursorHome.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.tbCmdCursorHome.Location = new Point(88, 147);
      this.tbCmdCursorHome.Name = "tbCmdCursorHome";
      this.tbCmdCursorHome.Size = new Size(244, 20);
      this.tbCmdCursorHome.TabIndex = 13;
      // 
      // tbCmdCursorDown
      // 
      this.tbCmdCursorDown.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.tbCmdCursorDown.Location = new Point(88, 125);
      this.tbCmdCursorDown.Name = "tbCmdCursorDown";
      this.tbCmdCursorDown.Size = new Size(244, 20);
      this.tbCmdCursorDown.TabIndex = 12;
      // 
      // tbCmdCursorUp
      // 
      this.tbCmdCursorUp.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                    | AnchorStyles.Right)));
      this.tbCmdCursorUp.Location = new Point(88, 103);
      this.tbCmdCursorUp.Name = "tbCmdCursorUp";
      this.tbCmdCursorUp.Size = new Size(244, 20);
      this.tbCmdCursorUp.TabIndex = 11;
      // 
      // tbCmdCursorRight
      // 
      this.tbCmdCursorRight.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                       | AnchorStyles.Right)));
      this.tbCmdCursorRight.Location = new Point(88, 81);
      this.tbCmdCursorRight.Name = "tbCmdCursorRight";
      this.tbCmdCursorRight.Size = new Size(244, 20);
      this.tbCmdCursorRight.TabIndex = 10;
      // 
      // tbCmdCursorLeft
      // 
      this.tbCmdCursorLeft.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.tbCmdCursorLeft.Location = new Point(88, 59);
      this.tbCmdCursorLeft.Name = "tbCmdCursorLeft";
      this.tbCmdCursorLeft.Size = new Size(244, 20);
      this.tbCmdCursorLeft.TabIndex = 9;
      // 
      // tbCmdClearDisplay
      // 
      this.tbCmdClearDisplay.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                        | AnchorStyles.Right)));
      this.tbCmdClearDisplay.Location = new Point(88, 37);
      this.tbCmdClearDisplay.Name = "tbCmdClearDisplay";
      this.tbCmdClearDisplay.Size = new Size(244, 20);
      this.tbCmdClearDisplay.TabIndex = 8;
      // 
      // tbCmdDisplayInit
      // 
      this.tbCmdDisplayInit.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                       | AnchorStyles.Right)));
      this.tbCmdDisplayInit.Location = new Point(88, 15);
      this.tbCmdDisplayInit.Name = "tbCmdDisplayInit";
      this.tbCmdDisplayInit.Size = new Size(244, 20);
      this.tbCmdDisplayInit.TabIndex = 7;
      // 
      // lblCmdCursorHome
      // 
      this.lblCmdCursorHome.AutoSize = true;
      this.lblCmdCursorHome.Location = new Point(6, 150);
      this.lblCmdCursorHome.Name = "lblCmdCursorHome";
      this.lblCmdCursorHome.Size = new Size(68, 13);
      this.lblCmdCursorHome.TabIndex = 6;
      this.lblCmdCursorHome.Text = "Cursor Home";
      // 
      // lblCursorDown
      // 
      this.lblCursorDown.AutoSize = true;
      this.lblCursorDown.Location = new Point(6, 128);
      this.lblCursorDown.Name = "lblCursorDown";
      this.lblCursorDown.Size = new Size(68, 13);
      this.lblCursorDown.TabIndex = 5;
      this.lblCursorDown.Text = "Cursor Down";
      // 
      // lblCursorUp
      // 
      this.lblCursorUp.AutoSize = true;
      this.lblCursorUp.Location = new Point(6, 106);
      this.lblCursorUp.Name = "lblCursorUp";
      this.lblCursorUp.Size = new Size(54, 13);
      this.lblCursorUp.TabIndex = 4;
      this.lblCursorUp.Text = "Cursor Up";
      // 
      // lblCmdCursorRight
      // 
      this.lblCmdCursorRight.AutoSize = true;
      this.lblCmdCursorRight.Location = new Point(6, 84);
      this.lblCmdCursorRight.Name = "lblCmdCursorRight";
      this.lblCmdCursorRight.Size = new Size(65, 13);
      this.lblCmdCursorRight.TabIndex = 3;
      this.lblCmdCursorRight.Text = "Cursor Right";
      // 
      // lblCmdClearDisplay
      // 
      this.lblCmdClearDisplay.AutoSize = true;
      this.lblCmdClearDisplay.Location = new Point(6, 40);
      this.lblCmdClearDisplay.Name = "lblCmdClearDisplay";
      this.lblCmdClearDisplay.Size = new Size(68, 13);
      this.lblCmdClearDisplay.TabIndex = 2;
      this.lblCmdClearDisplay.Text = "Clear Display";
      // 
      // lblCmdCursorLeft
      // 
      this.lblCmdCursorLeft.AutoSize = true;
      this.lblCmdCursorLeft.Location = new Point(6, 62);
      this.lblCmdCursorLeft.Name = "lblCmdCursorLeft";
      this.lblCmdCursorLeft.Size = new Size(58, 13);
      this.lblCmdCursorLeft.TabIndex = 1;
      this.lblCmdCursorLeft.Text = "Corsor Left";
      // 
      // lblCmdDisplayInit
      // 
      this.lblCmdDisplayInit.AutoSize = true;
      this.lblCmdDisplayInit.Location = new Point(6, 18);
      this.lblCmdDisplayInit.Name = "lblCmdDisplayInit";
      this.lblCmdDisplayInit.Size = new Size(58, 13);
      this.lblCmdDisplayInit.TabIndex = 0;
      this.lblCmdDisplayInit.Text = "Display Init";
      // 
      // groupBoxDisplayControl
      // 
      this.groupBoxDisplayControl.Anchor = ((AnchorStyles) (((AnchorStyles.Bottom | AnchorStyles.Left)
                                                             | AnchorStyles.Right)));
      this.groupBoxDisplayControl.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBoxDisplayControl.Controls.Add(this.cmbBlankIdleTime);
      this.groupBoxDisplayControl.Controls.Add(this.mpEnableDisplayAction);
      this.groupBoxDisplayControl.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBoxDisplayControl.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBoxDisplayControl.Location = new Point(10, 440);
      this.groupBoxDisplayControl.Name = "groupBoxDisplayControl";
      this.groupBoxDisplayControl.Size = new Size(338, 86);
      this.groupBoxDisplayControl.TabIndex = 23;
      this.groupBoxDisplayControl.TabStop = false;
      this.groupBoxDisplayControl.Text = " Display Control Options ";
      // 
      // mpEnableDisplayActionTime
      // 
      this.mpEnableDisplayActionTime.BorderColor = Color.Empty;
      this.mpEnableDisplayActionTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.mpEnableDisplayActionTime.Items.AddRange(new object[]
                                                      {
                                                        "0",
                                                        "1",
                                                        "2",
                                                        "3",
                                                        "4",
                                                        "5",
                                                        "6",
                                                        "7",
                                                        "8",
                                                        "9",
                                                        "10",
                                                        "11",
                                                        "12",
                                                        "13",
                                                        "14",
                                                        "15",
                                                        "16",
                                                        "17",
                                                        "18",
                                                        "19",
                                                        "20"
                                                      });
      this.mpEnableDisplayActionTime.Location = new Point(181, 36);
      this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      this.mpEnableDisplayActionTime.Size = new Size(42, 21);
      this.mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // cmbBlankIdleTime
      // 
      this.cmbBlankIdleTime.BorderColor = Color.Empty;
      this.cmbBlankIdleTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbBlankIdleTime.Items.AddRange(new object[]
                                             {
                                               "0",
                                               "1",
                                               "2",
                                               "3",
                                               "4",
                                               "5",
                                               "6",
                                               "7",
                                               "8",
                                               "9",
                                               "10",
                                               "11",
                                               "12",
                                               "13",
                                               "14",
                                               "15",
                                               "16",
                                               "17",
                                               "18",
                                               "19",
                                               "20",
                                               "21",
                                               "22",
                                               "23",
                                               "24",
                                               "25",
                                               "26",
                                               "27",
                                               "28",
                                               "29",
                                               "30"
                                             });
      this.cmbBlankIdleTime.Location = new Point(167, 58);
      this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      this.cmbBlankIdleTime.Size = new Size(42, 21);
      this.cmbBlankIdleTime.TabIndex = 98;
      // 
      // mpEnableDisplayAction
      // 
      this.mpEnableDisplayAction.AutoSize = true;
      this.mpEnableDisplayAction.Location = new Point(23, 38);
      this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
      this.mpEnableDisplayAction.Size = new Size(258, 17);
      this.mpEnableDisplayAction.TabIndex = 97;
      this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
      this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
      this.mpEnableDisplayAction.CheckedChanged += new EventHandler(this.mpEnableDisplayAction_CheckedChanged);
      // 
      // mpBlankDisplayWithVideo
      // 
      this.mpBlankDisplayWithVideo.AutoSize = true;
      this.mpBlankDisplayWithVideo.Location = new Point(7, 17);
      this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
      this.mpBlankDisplayWithVideo.Size = new Size(207, 17);
      this.mpBlankDisplayWithVideo.TabIndex = 95;
      this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
      this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWithVideo.CheckedChanged += new EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
      // 
      // mpBlankDisplayWhenIdle
      // 
      this.mpBlankDisplayWhenIdle.AutoSize = true;
      this.mpBlankDisplayWhenIdle.Location = new Point(7, 60);
      this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
      this.mpBlankDisplayWhenIdle.Size = new Size(261, 17);
      this.mpBlankDisplayWhenIdle.TabIndex = 99;
      this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
      this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWhenIdle.CheckedChanged += new EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnOK.Location = new Point(286, 544);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(80, 23);
      this.btnOK.TabIndex = 108;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnReset.Location = new Point(200, 544);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new Size(80, 23);
      this.btnReset.TabIndex = 109;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new EventHandler(this.btnReset_Click);
      // 
      // GenericSerial_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.ClientSize = new Size(378, 572);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.groupBoxConfiguration);
      this.Name = "GenericSerial_AdvancedSetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBoxConfiguration.ResumeLayout(false);
      this.groupBoxPortParameters.ResumeLayout(false);
      this.groupBoxPortParameters.PerformLayout();
      this.groupBoxDisplayCommands.ResumeLayout(false);
      this.groupBoxDisplayCommands.PerformLayout();
      this.groupBoxDisplayControl.ResumeLayout(false);
      this.groupBoxDisplayControl.PerformLayout();
      this.ResumeLayout(false);
    }

    private void mpBlankDisplayWhenIdle_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpBlankDisplayWithVideo_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpEnableDisplayAction_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void SetControlState()
    {
      if (this.mpBlankDisplayWithVideo.Checked)
      {
        this.mpEnableDisplayAction.Enabled = true;
        if (this.mpEnableDisplayAction.Checked)
        {
          this.mpEnableDisplayActionTime.Enabled = true;
        }
        else
        {
          this.mpEnableDisplayActionTime.Enabled = false;
        }
      }
      else
      {
        this.mpEnableDisplayAction.Enabled = false;
        this.mpEnableDisplayActionTime.Enabled = false;
      }
      if (this.mpBlankDisplayWhenIdle.Checked)
      {
        this.cmbBlankIdleTime.Enabled = true;
      }
      else
      {
        this.cmbBlankIdleTime.Enabled = false;
      }
    }
  }
}