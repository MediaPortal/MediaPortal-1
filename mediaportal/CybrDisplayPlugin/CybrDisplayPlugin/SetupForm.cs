namespace CybrDisplayPlugin
{
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.UserInterface.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    [PluginIcons("CybrDisplayPlugin.lcd.gif", "CybrDisplayPlugin.lcd_deactivated.gif")]
    public class SetupForm : Form
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
            Log.Info("CybrDisplay.SetupForm(): {0}", new object[] { CybrDisplay.Plugin_Version });
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
            if (!File.Exists(Config.GetFile(Config.Dir.Config, "CybrDisplay.xml")))
            {
                this.btnTest.Enabled = false;
            }
            else
            {
                this.btnTest.Enabled = true;
            }
            Log.Info("CybrDisplay.SetupForm(): constructor completed", new object[0]);
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
                Log.Error("CybrDisplay.SetupForm.btnAdvanced_Click(): CAUGHT EXCEPTION: {0}", new object[] { exception });
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
                new MessageEditForm("CybrDisplay.xml").ShowDialog(this);
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
                this.lcd.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth, Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth, Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight, Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
                if (!this.lcd.IsDisabled)
                {
                    this.lcd.Initialize();
                    this.lcd.SetLine(0, "CybrDisplay");
                    this.lcd.SetLine(1, this.lcd.Name);
                    Thread.Sleep(0x1388);
                    this.lcd.CleanUp();
                }
                base.Enabled = true;
                base.Activate();
            }
            catch (Exception exception)
            {
                Log.Error("CybrDisplay.SetupForm.btnAdvanced_Click(): CAUGHT EXCEPTION: {0}", new object[] { exception });
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
            this.tbBrightness.BeginInit();
            this.tbContrast.BeginInit();
            this.gbGraphMode.SuspendLayout();
            this.gbTextMode.SuspendLayout();
            ((ISupportInitialize) this.errorProvider).BeginInit();
            base.SuspendLayout();
            this.btnAdvanced.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnAdvanced.Location = new Point(0x10f, 0x29);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new Size(0x58, 0x17);
            this.btnAdvanced.TabIndex = 70;
            this.btnAdvanced.Text = "&Advanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new EventHandler(this.btnAdvanced_Click);
            this.cmbPort.BorderColor = Color.Empty;
            this.cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPort.Items.AddRange(new object[] { "LPT1", "LPT2", "LPT3", "LPT4", "USB", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "NONE", "localhost" });
            this.cmbPort.Location = new Point(40, 0x2a);
            this.cmbPort.Name = "cmbPort";
            this.cmbPort.Size = new Size(0x40, 0x15);
            this.cmbPort.TabIndex = 20;
            this.label1.Location = new Point(8, 0x2a);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x20, 0x17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Port";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;
            this.groupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
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
            this.groupBox1.Size = new Size(0x175, 0x188);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configuration";
            this.btnTest.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnTest.Location = new Point(0x10f, 0x152);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new Size(0x5c, 0x30);
            this.btnTest.TabIndex = 0x53;
            this.btnTest.Text = "Configuration Editor";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new EventHandler(this.btnTest_Click);
            this.cbControlScreenSaver.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.cbControlScreenSaver.AutoSize = true;
            this.cbControlScreenSaver.FlatStyle = FlatStyle.Popup;
            this.cbControlScreenSaver.Location = new Point(8, 0x106);
            this.cbControlScreenSaver.Name = "cbControlScreenSaver";
            this.cbControlScreenSaver.Size = new Size(0xdb, 0x11);
            this.cbControlScreenSaver.TabIndex = 0x52;
            this.cbControlScreenSaver.Text = "Disable ScreenSaver when playing Video";
            this.cbControlScreenSaver.UseVisualStyleBackColor = true;
            this.groupShutdown.Controls.Add(this.label11);
            this.groupShutdown.Controls.Add(this.label6);
            this.groupShutdown.Controls.Add(this.mpShutdown2);
            this.groupShutdown.Controls.Add(this.mpShutdown1);
            this.groupShutdown.Location = new Point(8, 0x141);
            this.groupShutdown.Name = "groupShutdown";
            this.groupShutdown.Size = new Size(0xbf, 0x41);
            this.groupShutdown.TabIndex = 0x4c;
            this.groupShutdown.TabStop = false;
            this.groupShutdown.Text = " ShutDown Message ";
            this.label11.AutoSize = true;
            this.label11.Location = new Point(6, 0x2b);
            this.label11.Name = "label11";
            this.label11.Size = new Size(0x24, 13);
            this.label11.TabIndex = 0x4f;
            this.label11.Text = "Line 2";
            this.label6.AutoSize = true;
            this.label6.Location = new Point(6, 20);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x24, 13);
            this.label6.TabIndex = 0x4e;
            this.label6.Text = "Line 1";
            this.mpShutdown2.Location = new Point(0x2e, 40);
            this.mpShutdown2.Name = "mpShutdown2";
            this.mpShutdown2.Size = new Size(0x8b, 20);
            this.mpShutdown2.TabIndex = 0x4d;
            this.mpShutdown1.Location = new Point(0x2e, 0x11);
            this.mpShutdown1.Name = "mpShutdown1";
            this.mpShutdown1.Size = new Size(0x8b, 20);
            this.mpShutdown1.TabIndex = 0x4c;
            this.cbContrast.AutoSize = true;
            this.cbContrast.Checked = true;
            this.cbContrast.CheckState = CheckState.Checked;
            this.cbContrast.FlatStyle = FlatStyle.Popup;
            this.cbContrast.Location = new Point(190, 0x106);
            this.cbContrast.Name = "cbContrast";
            this.cbContrast.Size = new Size(0x62, 0x11);
            this.cbContrast.TabIndex = 0x51;
            this.cbContrast.Text = "Control contrast";
            this.cbContrast.UseVisualStyleBackColor = true;
            this.cbContrast.Visible = false;
            this.cbDisplayOff.AutoSize = true;
            this.cbDisplayOff.FlatStyle = FlatStyle.Popup;
            this.cbDisplayOff.Location = new Point(8, 0xea);
            this.cbDisplayOff.Name = "cbDisplayOff";
            this.cbDisplayOff.Size = new Size(0x8a, 0x11);
            this.cbDisplayOff.TabIndex = 80;
            this.cbDisplayOff.Text = "Turn OFF display on exit";
            this.cbDisplayOff.UseVisualStyleBackColor = true;
            this.lblBrightness.Location = new Point(0x10, 280);
            this.lblBrightness.Name = "lblBrightness";
            this.lblBrightness.Size = new Size(0x60, 0x10);
            this.lblBrightness.TabIndex = 0x4f;
            this.lblBrightness.Text = "Brightness: ";
            this.tbBrightness.Location = new Point(15, 0x125);
            this.tbBrightness.Maximum = 0xff;
            this.tbBrightness.Name = "tbBrightness";
            this.tbBrightness.Size = new Size(160, 0x2d);
            this.tbBrightness.TabIndex = 0x4e;
            this.tbBrightness.TickFrequency = 8;
            this.tbBrightness.TickStyle = TickStyle.None;
            this.tbBrightness.Value = 0x7f;
            this.tbBrightness.ValueChanged += new EventHandler(this.tbBrightness_ValueChanged);
            this.btnTestDisplay.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnTestDisplay.Location = new Point(0xb3, 0x29);
            this.btnTestDisplay.Name = "btnTestDisplay";
            this.btnTestDisplay.Size = new Size(0x58, 0x17);
            this.btnTestDisplay.TabIndex = 0x4d;
            this.btnTestDisplay.Text = "&Test Display";
            this.btnTestDisplay.UseVisualStyleBackColor = true;
            this.btnTestDisplay.Click += new EventHandler(this.btnTestDisplay_Click);
            this.lblContrast.Location = new Point(0xbb, 280);
            this.lblContrast.Name = "lblContrast";
            this.lblContrast.Size = new Size(0x60, 0x10);
            this.lblContrast.TabIndex = 0x4a;
            this.lblContrast.Text = "Contrast:";
            this.tbContrast.Location = new Point(0xba, 0x125);
            this.tbContrast.Maximum = 0xff;
            this.tbContrast.Name = "tbContrast";
            this.tbContrast.Size = new Size(160, 0x2d);
            this.tbContrast.TabIndex = 0x49;
            this.tbContrast.TickFrequency = 8;
            this.tbContrast.TickStyle = TickStyle.None;
            this.tbContrast.Value = 0x7f;
            this.tbContrast.ValueChanged += new EventHandler(this.tbContrast_ValueChanged);
            this.txtScrollDelay.BorderColor = Color.Empty;
            this.txtScrollDelay.Location = new Point(0x60, 0xd1);
            this.txtScrollDelay.Name = "txtScrollDelay";
            this.txtScrollDelay.Size = new Size(0x30, 20);
            this.txtScrollDelay.TabIndex = 0x34;
            this.txtScrollDelay.Text = "300";
            this.gbGraphMode.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
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
            this.gbGraphMode.Location = new Point(0xa8, 0x44);
            this.gbGraphMode.Name = "gbGraphMode";
            this.gbGraphMode.Size = new Size(0xbf, 0xbc);
            this.gbGraphMode.TabIndex = 0x48;
            this.gbGraphMode.TabStop = false;
            this.gbGraphMode.Text = "GraphMode";
            this.txtPixelsToScroll.BorderColor = Color.Empty;
            this.txtPixelsToScroll.Location = new Point(0x56, 0x87);
            this.txtPixelsToScroll.Name = "txtPixelsToScroll";
            this.txtPixelsToScroll.Size = new Size(0x30, 20);
            this.txtPixelsToScroll.TabIndex = 0x39;
            this.txtPixelsToScroll.Text = "10";
            this.mpLabel5.Location = new Point(8, 0x85);
            this.mpLabel5.Name = "mpLabel5";
            this.mpLabel5.Size = new Size(80, 0x17);
            this.mpLabel5.TabIndex = 0x38;
            this.mpLabel5.Text = "Pixels to scroll";
            this.mpLabel5.TextAlign = ContentAlignment.MiddleLeft;
            this.ckForceGraphicText.AutoSize = true;
            this.ckForceGraphicText.FlatStyle = FlatStyle.Popup;
            this.ckForceGraphicText.Location = new Point(11, 0xa1);
            this.ckForceGraphicText.Name = "ckForceGraphicText";
            this.ckForceGraphicText.Size = new Size(0x7b, 0x11);
            this.ckForceGraphicText.TabIndex = 0x37;
            this.ckForceGraphicText.Text = "Force Graphical Text";
            this.ckForceGraphicText.UseVisualStyleBackColor = true;
            this.txtFontSize.BorderColor = Color.Empty;
            this.txtFontSize.Location = new Point(0x56, 110);
            this.txtFontSize.Name = "txtFontSize";
            this.txtFontSize.Size = new Size(0x30, 20);
            this.txtFontSize.TabIndex = 0x36;
            this.txtFontSize.Text = "10";
            this.mpLabel2.Location = new Point(8, 110);
            this.mpLabel2.Name = "mpLabel2";
            this.mpLabel2.Size = new Size(0x40, 0x17);
            this.mpLabel2.TabIndex = 0x35;
            this.mpLabel2.Text = "Font Size";
            this.mpLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.txtFont.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.txtFont.BorderColor = Color.Empty;
            this.txtFont.Location = new Point(0x56, 0x57);
            this.txtFont.Name = "txtFont";
            this.txtFont.Size = new Size(0x63, 0x15);
            this.txtFont.TabIndex = 0x34;
            this.txtFont.Text = "Arial Black";
            this.mpLabel1.Location = new Point(8, 0x57);
            this.mpLabel1.Name = "mpLabel1";
            this.mpLabel1.Size = new Size(80, 0x17);
            this.mpLabel1.TabIndex = 0x33;
            this.mpLabel1.Text = "Font";
            this.mpLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.label8.Location = new Point(8, 0x10);
            this.label8.Name = "label8";
            this.label8.Size = new Size(0x40, 0x17);
            this.label8.TabIndex = 3;
            this.label8.Text = "Columns";
            this.label8.TextAlign = ContentAlignment.MiddleLeft;
            this.txtTimG.BorderColor = Color.Empty;
            this.txtTimG.Location = new Point(0x56, 0x40);
            this.txtTimG.Name = "txtTimG";
            this.txtTimG.Size = new Size(0x30, 20);
            this.txtTimG.TabIndex = 50;
            this.txtTimG.Text = "1";
            this.txtRowsG.BorderColor = Color.Empty;
            this.txtRowsG.Location = new Point(0x56, 40);
            this.txtRowsG.Name = "txtRowsG";
            this.txtRowsG.Size = new Size(0x30, 20);
            this.txtRowsG.TabIndex = 40;
            this.txtRowsG.Text = "240";
            this.txtColsG.BorderColor = Color.Empty;
            this.txtColsG.Location = new Point(0x56, 0x10);
            this.txtColsG.Name = "txtColsG";
            this.txtColsG.Size = new Size(0x30, 20);
            this.txtColsG.TabIndex = 30;
            this.txtColsG.Text = "320";
            this.label9.Location = new Point(8, 0x40);
            this.label9.Name = "label9";
            this.label9.Size = new Size(80, 0x17);
            this.label9.TabIndex = 5;
            this.label9.Text = "Comm. Delay";
            this.label9.TextAlign = ContentAlignment.MiddleLeft;
            this.label10.Location = new Point(8, 40);
            this.label10.Name = "label10";
            this.label10.Size = new Size(0x48, 0x17);
            this.label10.TabIndex = 4;
            this.label10.Text = "Rows";
            this.label10.TextAlign = ContentAlignment.MiddleLeft;
            this.mpLabel3.Location = new Point(5, 0xcf);
            this.mpLabel3.Name = "mpLabel3";
            this.mpLabel3.Size = new Size(80, 0x17);
            this.mpLabel3.TabIndex = 0x33;
            this.mpLabel3.Text = "Scroll Delay";
            this.mpLabel3.TextAlign = ContentAlignment.MiddleLeft;
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
            this.gbTextMode.Location = new Point(8, 0x44);
            this.gbTextMode.Name = "gbTextMode";
            this.gbTextMode.Size = new Size(0x98, 0x88);
            this.gbTextMode.TabIndex = 0x47;
            this.gbTextMode.TabStop = false;
            this.gbTextMode.Text = "TextMode";
            this.mpPrefixChar.BorderColor = Color.Empty;
            this.mpPrefixChar.Location = new Point(0x58, 0x70);
            this.mpPrefixChar.Name = "mpPrefixChar";
            this.mpPrefixChar.Size = new Size(0x30, 20);
            this.mpPrefixChar.TabIndex = 0x38;
            this.mpPrefixChar.Visible = false;
            this.mpLabel6.Location = new Point(8, 0x70);
            this.mpLabel6.Name = "mpLabel6";
            this.mpLabel6.Size = new Size(0x58, 0x17);
            this.mpLabel6.TabIndex = 0x37;
            this.mpLabel6.Text = "Line Prefix     0x";
            this.mpLabel6.TextAlign = ContentAlignment.MiddleLeft;
            this.mpLabel6.Visible = false;
            this.txtCharsToScroll.BorderColor = Color.Empty;
            this.txtCharsToScroll.Location = new Point(0x58, 0x57);
            this.txtCharsToScroll.Name = "txtCharsToScroll";
            this.txtCharsToScroll.Size = new Size(0x30, 20);
            this.txtCharsToScroll.TabIndex = 0x36;
            this.txtCharsToScroll.Text = "1";
            this.mpLabel4.Location = new Point(8, 0x57);
            this.mpLabel4.Name = "mpLabel4";
            this.mpLabel4.Size = new Size(80, 0x17);
            this.mpLabel4.TabIndex = 0x35;
            this.mpLabel4.Text = "#Chars to scroll";
            this.mpLabel4.TextAlign = ContentAlignment.MiddleLeft;
            this.label2.Location = new Point(8, 0x10);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x40, 0x17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Columns";
            this.label2.TextAlign = ContentAlignment.MiddleLeft;
            this.txtTim.BorderColor = Color.Empty;
            this.txtTim.Location = new Point(0x58, 0x40);
            this.txtTim.Name = "txtTim";
            this.txtTim.Size = new Size(0x30, 20);
            this.txtTim.TabIndex = 50;
            this.txtTim.Text = "1";
            this.txtRows.BorderColor = Color.Empty;
            this.txtRows.Location = new Point(0x58, 40);
            this.txtRows.Name = "txtRows";
            this.txtRows.Size = new Size(0x30, 20);
            this.txtRows.TabIndex = 40;
            this.txtRows.Text = "2";
            this.txtCols.BorderColor = Color.Empty;
            this.txtCols.Location = new Point(0x58, 0x10);
            this.txtCols.Name = "txtCols";
            this.txtCols.Size = new Size(0x30, 20);
            this.txtCols.TabIndex = 30;
            this.txtCols.Text = "16";
            this.txtCols.TextChanged += new EventHandler(this.txtCols_TextChanged);
            this.label4.Location = new Point(8, 0x40);
            this.label4.Name = "label4";
            this.label4.Size = new Size(80, 0x17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Comm. Delay";
            this.label4.TextAlign = ContentAlignment.MiddleLeft;
            this.label3.Location = new Point(8, 40);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x48, 0x17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Rows";
            this.label3.TextAlign = ContentAlignment.MiddleLeft;
            this.label7.Location = new Point(8, 0x10);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x20, 0x17);
            this.label7.TabIndex = 11;
            this.label7.Text = "Type";
            this.label7.TextAlign = ContentAlignment.MiddleLeft;
            this.cmbType.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cmbType.BorderColor = Color.Empty;
            this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbType.Location = new Point(40, 0x10);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new Size(0x13f, 0x15);
            this.cmbType.Sorted = true;
            this.cmbType.TabIndex = 10;
            this.cmbType.SelectionChangeCommitted += new EventHandler(this.cmbType_SelectionChangeCommitted);
            this.cbLight.AutoSize = true;
            this.cbLight.Checked = true;
            this.cbLight.CheckState = CheckState.Checked;
            this.cbLight.FlatStyle = FlatStyle.Popup;
            this.cbLight.Location = new Point(0x13, 0x106);
            this.cbLight.Name = "cbLight";
            this.cbLight.Size = new Size(0x6c, 0x11);
            this.cbLight.TabIndex = 60;
            this.cbLight.Text = "Control brightness";
            this.cbLight.UseVisualStyleBackColor = true;
            this.cbLight.Visible = false;
            this.cbLight.CheckedChanged += new EventHandler(this.cbLight_CheckedChanged);
            this.cbPropertyBrowser.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.cbPropertyBrowser.AutoSize = true;
            this.cbPropertyBrowser.FlatStyle = FlatStyle.Popup;
            this.cbPropertyBrowser.Location = new Point(8, 0x1b5);
            this.cbPropertyBrowser.Name = "cbPropertyBrowser";
            this.cbPropertyBrowser.Size = new Size(0x84, 0x11);
            this.cbPropertyBrowser.TabIndex = 4;
            this.cbPropertyBrowser.Text = "Show property browser";
            this.cbPropertyBrowser.UseVisualStyleBackColor = true;
            this.btnOK.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnOK.Location = new Point(0xd5, 0x1a9);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4e, 0x17);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.errorProvider.ContainerControl = this;
            this.cbExtensiveLogging.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.cbExtensiveLogging.AutoSize = true;
            this.cbExtensiveLogging.FlatStyle = FlatStyle.Popup;
            this.cbExtensiveLogging.Location = new Point(8, 420);
            this.cbExtensiveLogging.Name = "cbExtensiveLogging";
            this.cbExtensiveLogging.Size = new Size(0x6b, 0x11);
            this.cbExtensiveLogging.TabIndex = 6;
            this.cbExtensiveLogging.Text = "Extensive logging";
            this.cbExtensiveLogging.UseVisualStyleBackColor = true;
            this.mpDisableGUISetup.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.mpDisableGUISetup.AutoSize = true;
            this.mpDisableGUISetup.FlatStyle = FlatStyle.Popup;
            this.mpDisableGUISetup.Location = new Point(8, 0x193);
            this.mpDisableGUISetup.Name = "mpDisableGUISetup";
            this.mpDisableGUISetup.Size = new Size(0x70, 0x11);
            this.mpDisableGUISetup.TabIndex = 0x4d;
            this.mpDisableGUISetup.Text = "Disable GUI Setup";
            this.mpDisableGUISetup.UseVisualStyleBackColor = true;
            this.btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.btnCancel.Location = new Point(0x129, 0x1a9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4e, 0x17);
            this.btnCancel.TabIndex = 0x4e;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            base.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new Size(5, 13);
            base.CancelButton = this.btnCancel;
            base.ClientSize = new Size(0x185, 0x1c6);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.mpDisableGUISetup);
            base.Controls.Add(this.cbExtensiveLogging);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.cbPropertyBrowser);
            base.Controls.Add(this.groupBox1);
            base.Name = "SetupForm";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "CybrDisplay - Setup";
            base.Load += new EventHandler(this.SetupForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupShutdown.ResumeLayout(false);
            this.groupShutdown.PerformLayout();
            this.tbBrightness.EndInit();
            this.tbContrast.EndInit();
            this.gbGraphMode.ResumeLayout(false);
            this.gbGraphMode.PerformLayout();
            this.gbTextMode.ResumeLayout(false);
            this.gbTextMode.PerformLayout();
            ((ISupportInitialize) this.errorProvider).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            Log.Info("CybrDisplay.SetupForm.Load(): called", new object[0]);
            foreach (FontFamily family in FontFamily.Families)
            {
                this.txtFont.Items.Add(family.Name);
            }
            Log.Info("CybrDisplay.SetupForm.Load(): completed", new object[0]);
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
            Log.Info("CybrDisplay.SetupForm.VerifyLCDType(): called", new object[0]);
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
            Log.Info("CybrDisplay.SetupForm.Load(): completed", new object[0]);
        }
    }
}

