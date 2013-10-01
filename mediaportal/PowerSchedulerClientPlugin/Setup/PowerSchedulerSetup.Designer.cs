namespace PowerScheduler.Setup
{
  partial class PowerSchedulerSetup
  {
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.ToolTip toolTip;
    private System.ComponentModel.IContainer components;
  
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PowerSchedulerSetup));
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.textBoxCommand = new System.Windows.Forms.TextBox();
      this.checkBoxRebootWakeup = new System.Windows.Forms.CheckBox();
      this.dataGridShares = new System.Windows.Forms.DataGridView();
      this.Sharename = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Hostname = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Username = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.labelShares = new System.Windows.Forms.Label();
      this.buttonSelectShare = new System.Windows.Forms.Button();
      this.checkBoxMPClientRunning = new System.Windows.Forms.CheckBox();
      this.checkBoxProcessesAwayMode = new System.Windows.Forms.CheckBox();
      this.buttonSelectProcess = new System.Windows.Forms.Button();
      this.textBoxProcesses = new System.Windows.Forms.TextBox();
      this.checkBoxEPGAwayMode = new System.Windows.Forms.CheckBox();
      this.textBoxEPGCommand = new System.Windows.Forms.TextBox();
      this.checkBoxEPGPreventStandby = new System.Windows.Forms.CheckBox();
      this.checkBoxNetworkAwayMode = new System.Windows.Forms.CheckBox();
      this.checkBoxSharesAwayMode = new System.Windows.Forms.CheckBox();
      this.buttonApply = new System.Windows.Forms.Button();
      this.comboBoxShutdownMode = new System.Windows.Forms.ComboBox();
      this.checkBoxShutdownEnabled = new System.Windows.Forms.CheckBox();
      this.checkBoxAutoPowerSettings = new System.Windows.Forms.CheckBox();
      this.buttonPowerSettings = new System.Windows.Forms.Button();
      this.checkBoxReinitializeController = new System.Windows.Forms.CheckBox();
      this.textBoxRebootCommand = new System.Windows.Forms.TextBox();
      this.textBoxReboot = new System.Windows.Forms.MaskedTextBox();
      this.numericUpDownNetworkIdleLimit = new System.Windows.Forms.NumericUpDown();
      this.checkBoxNetworkEnabled = new System.Windows.Forms.CheckBox();
      this.checkBoxSharesEnabled = new System.Windows.Forms.CheckBox();
      this.comboBoxProfile = new System.Windows.Forms.ComboBox();
      this.buttonExpertMode = new System.Windows.Forms.Button();
      this.numericUpDownIdleTimeout = new System.Windows.Forms.NumericUpDown();
      this.checkBoxHomeOnly = new System.Windows.Forms.CheckBox();
      this.checkBoxUmuteMasterVolume = new System.Windows.Forms.CheckBox();
      this.groupBoxProcesses = new System.Windows.Forms.GroupBox();
      this.groupBoxEPG = new System.Windows.Forms.GroupBox();
      this.flowLayoutPanelEPG = new System.Windows.Forms.FlowLayoutPanel();
      this.labelEPG1 = new System.Windows.Forms.Label();
      this.textBoxEPG = new System.Windows.Forms.MaskedTextBox();
      this.labelEPG2 = new System.Windows.Forms.Label();
      this.buttonEPGCommand = new System.Windows.Forms.Button();
      this.labelEPGCommand = new System.Windows.Forms.Label();
      this.panelEPG = new System.Windows.Forms.Panel();
      this.checkBoxEPGSunday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGSaturday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGFriday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGThursday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGWednesday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGTuesday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGMonday = new System.Windows.Forms.CheckBox();
      this.checkBoxEPGWakeup = new System.Windows.Forms.CheckBox();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.groupBoxStatus = new System.Windows.Forms.GroupBox();
      this.labelStandbyStatus = new System.Windows.Forms.Label();
      this.textBoxStandbyHandler = new System.Windows.Forms.TextBox();
      this.labelStandbyHandler = new System.Windows.Forms.Label();
      this.labelWakeupHandler = new System.Windows.Forms.Label();
      this.labelWakeupTime = new System.Windows.Forms.Label();
      this.labelWakeupTimeValue = new System.Windows.Forms.Label();
      this.tabPageAdvanced = new System.Windows.Forms.TabPage();
      this.groupBoxAdvanced = new System.Windows.Forms.GroupBox();
      this.buttonCommand = new System.Windows.Forms.Button();
      this.labelCommand = new System.Windows.Forms.Label();
      this.flowLayoutPanelShutdownMode = new System.Windows.Forms.FlowLayoutPanel();
      this.labelShutdownMode = new System.Windows.Forms.Label();
      this.tabPageReboot = new System.Windows.Forms.TabPage();
      this.groupBoxReboot = new System.Windows.Forms.GroupBox();
      this.flowLayoutPanelReboot = new System.Windows.Forms.FlowLayoutPanel();
      this.labelReboot1 = new System.Windows.Forms.Label();
      this.labelReboot2 = new System.Windows.Forms.Label();
      this.buttonRebootCommand = new System.Windows.Forms.Button();
      this.labelRebootCommand = new System.Windows.Forms.Label();
      this.panelReboot = new System.Windows.Forms.Panel();
      this.checkBoxRebootSunday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootSaturday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootFriday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootThursday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootWednesday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootTuesday = new System.Windows.Forms.CheckBox();
      this.checkBoxRebootMonday = new System.Windows.Forms.CheckBox();
      this.tabPageNetwork = new System.Windows.Forms.TabPage();
      this.groupBoxNetwork = new System.Windows.Forms.GroupBox();
      this.flowLayoutPanelNetworkIdleLimit = new System.Windows.Forms.FlowLayoutPanel();
      this.labelNetwork = new System.Windows.Forms.Label();
      this.tabPageShares = new System.Windows.Forms.TabPage();
      this.groupBoxShares = new System.Windows.Forms.GroupBox();
      this.tabPageProcesses = new System.Windows.Forms.TabPage();
      this.tabPageEPG = new System.Windows.Forms.TabPage();
      this.tabPageGeneral = new System.Windows.Forms.TabPage();
      this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
      this.flowLayoutPanelGeneral = new System.Windows.Forms.FlowLayoutPanel();
      this.textBoxProfile = new System.Windows.Forms.TextBox();
      this.flowLayoutPanelIdleTimeout = new System.Windows.Forms.FlowLayoutPanel();
      this.labelIdleTimeout1 = new System.Windows.Forms.Label();
      this.labelIdleTimeout2 = new System.Windows.Forms.Label();
      this.labelExpertMode = new System.Windows.Forms.Label();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageClient = new System.Windows.Forms.TabPage();
      this.groupBoxClient = new System.Windows.Forms.GroupBox();
      this.tabPageLegacy = new System.Windows.Forms.TabPage();
      this.groupBoxLegacy = new System.Windows.Forms.GroupBox();
      this.label4 = new System.Windows.Forms.Label();
      this.flowLayoutPanelStandbyHours = new System.Windows.Forms.FlowLayoutPanel();
      this.label5 = new System.Windows.Forms.Label();
      this.numericUpDownStandbyHoursFrom = new System.Windows.Forms.NumericUpDown();
      this.label6 = new System.Windows.Forms.Label();
      this.numericUpDownStandbyHoursTo = new System.Windows.Forms.NumericUpDown();
      this.label7 = new System.Windows.Forms.Label();
      this.flowLayoutPanelPreNoStandbyTime = new System.Windows.Forms.FlowLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.numericUpDownPreNoStandbyTime = new System.Windows.Forms.NumericUpDown();
      this.flowLayoutPanelPreWakeupTime = new System.Windows.Forms.FlowLayoutPanel();
      this.label3 = new System.Windows.Forms.Label();
      this.numericUpDownPreWakeupTime = new System.Windows.Forms.NumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridShares)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNetworkIdleLimit)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIdleTimeout)).BeginInit();
      this.groupBoxProcesses.SuspendLayout();
      this.groupBoxEPG.SuspendLayout();
      this.flowLayoutPanelEPG.SuspendLayout();
      this.panelEPG.SuspendLayout();
      this.groupBoxStatus.SuspendLayout();
      this.tabPageAdvanced.SuspendLayout();
      this.groupBoxAdvanced.SuspendLayout();
      this.flowLayoutPanelShutdownMode.SuspendLayout();
      this.tabPageReboot.SuspendLayout();
      this.groupBoxReboot.SuspendLayout();
      this.flowLayoutPanelReboot.SuspendLayout();
      this.panelReboot.SuspendLayout();
      this.tabPageNetwork.SuspendLayout();
      this.groupBoxNetwork.SuspendLayout();
      this.flowLayoutPanelNetworkIdleLimit.SuspendLayout();
      this.tabPageShares.SuspendLayout();
      this.groupBoxShares.SuspendLayout();
      this.tabPageProcesses.SuspendLayout();
      this.tabPageEPG.SuspendLayout();
      this.tabPageGeneral.SuspendLayout();
      this.groupBoxGeneral.SuspendLayout();
      this.flowLayoutPanelGeneral.SuspendLayout();
      this.flowLayoutPanelIdleTimeout.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.tabPageClient.SuspendLayout();
      this.groupBoxClient.SuspendLayout();
      this.tabPageLegacy.SuspendLayout();
      this.groupBoxLegacy.SuspendLayout();
      this.flowLayoutPanelStandbyHours.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStandbyHoursFrom)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStandbyHoursTo)).BeginInit();
      this.flowLayoutPanelPreNoStandbyTime.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreNoStandbyTime)).BeginInit();
      this.flowLayoutPanelPreWakeupTime.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreWakeupTime)).BeginInit();
      this.SuspendLayout();
      // 
      // textBoxCommand
      // 
      this.textBoxCommand.Location = new System.Drawing.Point(12, 71);
      this.textBoxCommand.Name = "textBoxCommand";
      this.textBoxCommand.Size = new System.Drawing.Size(329, 20);
      this.textBoxCommand.TabIndex = 2;
      this.toolTip.SetToolTip(this.textBoxCommand, "The command is executed on each system power state change. The\r\nargument (\"standb" +
              "y\", \"wakeup\", \"awaymode\" or \"runmode\") will\r\nbe added by PowerScheduler at the t" +
              "ime the command is executed.");
      this.textBoxCommand.TextChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootWakeup
      // 
      this.checkBoxRebootWakeup.AutoSize = true;
      this.checkBoxRebootWakeup.Location = new System.Drawing.Point(12, 117);
      this.checkBoxRebootWakeup.Name = "checkBoxRebootWakeup";
      this.checkBoxRebootWakeup.Size = new System.Drawing.Size(180, 17);
      this.checkBoxRebootWakeup.TabIndex = 4;
      this.checkBoxRebootWakeup.Text = "Wakeup the computer for reboot";
      this.toolTip.SetToolTip(this.checkBoxRebootWakeup, "If unchecked, the reboot will be caught up when the system is running again.\r\nThe" +
              " computer will reboot only when not busy with other tasks.");
      this.checkBoxRebootWakeup.UseVisualStyleBackColor = true;
      this.checkBoxRebootWakeup.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // dataGridShares
      // 
      this.dataGridShares.AllowUserToOrderColumns = true;
      this.dataGridShares.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.dataGridShares.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridShares.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.dataGridShares.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridShares.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sharename,
            this.Hostname,
            this.Username});
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dataGridShares.DefaultCellStyle = dataGridViewCellStyle2;
      this.dataGridShares.EnableHeadersVisualStyles = false;
      this.dataGridShares.Location = new System.Drawing.Point(34, 66);
      this.dataGridShares.MaximumSize = new System.Drawing.Size(348, 231);
      this.dataGridShares.MinimumSize = new System.Drawing.Size(348, 48);
      this.dataGridShares.Name = "dataGridShares";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridShares.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.dataGridShares.RowHeadersWidth = 30;
      this.dataGridShares.RowTemplate.Height = 24;
      this.dataGridShares.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.dataGridShares.Size = new System.Drawing.Size(348, 99);
      this.dataGridShares.TabIndex = 2;
      this.toolTip.SetToolTip(this.dataGridShares, "Enter share / client / user combinations that prevent standby\r\n(leave blank to ma" +
              "tch any value)");
      this.dataGridShares.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.buttonApply_Enable);
      // 
      // Sharename
      // 
      this.Sharename.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.Sharename.HeaderText = "Share";
      this.Sharename.MinimumWidth = 100;
      this.Sharename.Name = "Sharename";
      // 
      // Hostname
      // 
      this.Hostname.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.Hostname.HeaderText = "Client";
      this.Hostname.MinimumWidth = 100;
      this.Hostname.Name = "Hostname";
      // 
      // Username
      // 
      this.Username.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.Username.HeaderText = "User";
      this.Username.MinimumWidth = 100;
      this.Username.Name = "Username";
      // 
      // labelShares
      // 
      this.labelShares.AutoSize = true;
      this.labelShares.Location = new System.Drawing.Point(31, 47);
      this.labelShares.Name = "labelShares";
      this.labelShares.Size = new System.Drawing.Size(131, 13);
      this.labelShares.TabIndex = 1;
      this.labelShares.Text = "Shares to prevent standby";
      this.toolTip.SetToolTip(this.labelShares, "Enter share / client / user combinations that prevent standby\r\n(blank fields matc" +
              "h any value; empty list matches any active share)");
      // 
      // buttonSelectShare
      // 
      this.buttonSelectShare.AutoSize = true;
      this.buttonSelectShare.Location = new System.Drawing.Point(282, 171);
      this.buttonSelectShare.Name = "buttonSelectShare";
      this.buttonSelectShare.Size = new System.Drawing.Size(100, 23);
      this.buttonSelectShare.TabIndex = 3;
      this.buttonSelectShare.Text = "Select share";
      this.toolTip.SetToolTip(this.buttonSelectShare, "Select from currently active shares");
      this.buttonSelectShare.UseVisualStyleBackColor = true;
      this.buttonSelectShare.Click += new System.EventHandler(this.buttonSelectShare_Click);
      // 
      // checkBoxMPClientRunning
      // 
      this.checkBoxMPClientRunning.AutoSize = true;
      this.checkBoxMPClientRunning.Location = new System.Drawing.Point(12, 52);
      this.checkBoxMPClientRunning.Name = "checkBoxMPClientRunning";
      this.checkBoxMPClientRunning.Size = new System.Drawing.Size(379, 17);
      this.checkBoxMPClientRunning.TabIndex = 2;
      this.checkBoxMPClientRunning.Text = "Do not put the computer to sleep while the MediaPortal client is not running";
      this.toolTip.SetToolTip(this.checkBoxMPClientRunning, "Prevents automatic standby while doing administrative work\r\n(only recommended for" +
              " a single-seat HTPC).");
      this.checkBoxMPClientRunning.UseVisualStyleBackColor = true;
      this.checkBoxMPClientRunning.CheckedChanged += new System.EventHandler(this.checkBoxMPClientRunning_CheckedChanged);
      // 
      // checkBoxProcessesAwayMode
      // 
      this.checkBoxProcessesAwayMode.AutoSize = true;
      this.checkBoxProcessesAwayMode.Location = new System.Drawing.Point(34, 77);
      this.checkBoxProcessesAwayMode.Name = "checkBoxProcessesAwayMode";
      this.checkBoxProcessesAwayMode.Size = new System.Drawing.Size(344, 17);
      this.checkBoxProcessesAwayMode.TabIndex = 3;
      this.checkBoxProcessesAwayMode.Text = "Enter away mode when the user wants to put the computer to sleep";
      this.toolTip.SetToolTip(this.checkBoxProcessesAwayMode, "Not even a \"Power Off\" or \"Remote Control Off\"causes the\r\nsystem to go to standby" +
              " while the selected processes are running.\r\n");
      this.checkBoxProcessesAwayMode.UseVisualStyleBackColor = true;
      this.checkBoxProcessesAwayMode.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // buttonSelectProcess
      // 
      this.buttonSelectProcess.AutoSize = true;
      this.buttonSelectProcess.Location = new System.Drawing.Point(334, 19);
      this.buttonSelectProcess.Name = "buttonSelectProcess";
      this.buttonSelectProcess.Size = new System.Drawing.Size(100, 23);
      this.buttonSelectProcess.TabIndex = 1;
      this.buttonSelectProcess.Text = "Select process";
      this.toolTip.SetToolTip(this.buttonSelectProcess, "Select from currently running processes");
      this.buttonSelectProcess.UseVisualStyleBackColor = true;
      this.buttonSelectProcess.Click += new System.EventHandler(this.buttonSelectProcess_Click);
      // 
      // textBoxProcesses
      // 
      this.textBoxProcesses.Location = new System.Drawing.Point(12, 20);
      this.textBoxProcesses.Name = "textBoxProcesses";
      this.textBoxProcesses.Size = new System.Drawing.Size(315, 20);
      this.textBoxProcesses.TabIndex = 0;
      this.toolTip.SetToolTip(this.textBoxProcesses, "Enter a comma-separated list of processes which prevent\r\nthe system from going to" +
              " standby while they are active.");
      this.textBoxProcesses.TextChanged += new System.EventHandler(this.textBoxProcesses_TextChanged);
      // 
      // checkBoxEPGAwayMode
      // 
      this.checkBoxEPGAwayMode.AutoSize = true;
      this.checkBoxEPGAwayMode.Location = new System.Drawing.Point(34, 175);
      this.checkBoxEPGAwayMode.Name = "checkBoxEPGAwayMode";
      this.checkBoxEPGAwayMode.Size = new System.Drawing.Size(344, 17);
      this.checkBoxEPGAwayMode.TabIndex = 6;
      this.checkBoxEPGAwayMode.Text = "Enter away mode when the user wants to put the computer to sleep";
      this.toolTip.SetToolTip(this.checkBoxEPGAwayMode, "Not even a \"Power Off\" or \"Remote Control Off\"causes the\r\nsystem to go to standby" +
              " until EPG grabbing is completed.");
      this.checkBoxEPGAwayMode.UseVisualStyleBackColor = true;
      this.checkBoxEPGAwayMode.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // textBoxEPGCommand
      // 
      this.textBoxEPGCommand.AcceptsTab = true;
      this.textBoxEPGCommand.Location = new System.Drawing.Point(12, 220);
      this.textBoxEPGCommand.Name = "textBoxEPGCommand";
      this.textBoxEPGCommand.Size = new System.Drawing.Size(329, 20);
      this.textBoxEPGCommand.TabIndex = 8;
      this.toolTip.SetToolTip(this.textBoxEPGCommand, resources.GetString("textBoxEPGCommand.ToolTip"));
      this.textBoxEPGCommand.TextChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGPreventStandby
      // 
      this.checkBoxEPGPreventStandby.AutoSize = true;
      this.checkBoxEPGPreventStandby.Location = new System.Drawing.Point(12, 150);
      this.checkBoxEPGPreventStandby.Name = "checkBoxEPGPreventStandby";
      this.checkBoxEPGPreventStandby.Size = new System.Drawing.Size(277, 17);
      this.checkBoxEPGPreventStandby.TabIndex = 5;
      this.checkBoxEPGPreventStandby.Text = "Do not put the computer to sleep while grabbing EPG";
      this.toolTip.SetToolTip(this.checkBoxEPGPreventStandby, "The computer will not go to standby automatically until EPG grabbing is completed" +
              ".");
      this.checkBoxEPGPreventStandby.UseVisualStyleBackColor = true;
      this.checkBoxEPGPreventStandby.CheckedChanged += new System.EventHandler(this.checkBoxEPGPreventStandby_CheckedChanged);
      // 
      // checkBoxNetworkAwayMode
      // 
      this.checkBoxNetworkAwayMode.AutoSize = true;
      this.checkBoxNetworkAwayMode.Location = new System.Drawing.Point(34, 72);
      this.checkBoxNetworkAwayMode.Name = "checkBoxNetworkAwayMode";
      this.checkBoxNetworkAwayMode.Size = new System.Drawing.Size(344, 17);
      this.checkBoxNetworkAwayMode.TabIndex = 3;
      this.checkBoxNetworkAwayMode.Text = "Enter away mode when the user wants to put the computer to sleep";
      this.toolTip.SetToolTip(this.checkBoxNetworkAwayMode, "Not even a \"Power Off\" or \"Remote Control Off\"causes the\r\nsystem to go to standby" +
              " while the network is active.\r\n");
      this.checkBoxNetworkAwayMode.UseVisualStyleBackColor = true;
      this.checkBoxNetworkAwayMode.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxSharesAwayMode
      // 
      this.checkBoxSharesAwayMode.AutoSize = true;
      this.checkBoxSharesAwayMode.Location = new System.Drawing.Point(34, 200);
      this.checkBoxSharesAwayMode.Name = "checkBoxSharesAwayMode";
      this.checkBoxSharesAwayMode.Size = new System.Drawing.Size(344, 17);
      this.checkBoxSharesAwayMode.TabIndex = 4;
      this.checkBoxSharesAwayMode.Text = "Enter away mode when the user wants to put the computer to sleep";
      this.toolTip.SetToolTip(this.checkBoxSharesAwayMode, "Not even a \"Power Off\" or \"Remote Control Off\"causes the\r\nsystem to go to standby" +
              " while the selected shares are active.\r\n");
      this.checkBoxSharesAwayMode.UseVisualStyleBackColor = true;
      this.checkBoxSharesAwayMode.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // buttonApply
      // 
      this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonApply.AutoSize = true;
      this.buttonApply.Location = new System.Drawing.Point(409, 296);
      this.buttonApply.Name = "buttonApply";
      this.buttonApply.Size = new System.Drawing.Size(75, 23);
      this.buttonApply.TabIndex = 2;
      this.buttonApply.Text = "Apply";
      this.toolTip.SetToolTip(this.buttonApply, "Apply PowerScheduler settings, but do not leave TV-Server Configuration.");
      this.buttonApply.UseVisualStyleBackColor = true;
      this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
      // 
      // comboBoxShutdownMode
      // 
      this.comboBoxShutdownMode.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.comboBoxShutdownMode.Enabled = false;
      this.comboBoxShutdownMode.Items.AddRange(new object[] {
            "(Hybrid) Sleep - S3",
            "Hibernate - S4",
            "Stay on - S0",
            "Shutdown - S5"});
      this.comboBoxShutdownMode.Location = new System.Drawing.Point(84, 3);
      this.comboBoxShutdownMode.Name = "comboBoxShutdownMode";
      this.comboBoxShutdownMode.Size = new System.Drawing.Size(137, 21);
      this.comboBoxShutdownMode.TabIndex = 21;
      this.toolTip.SetToolTip(this.comboBoxShutdownMode, "Select standby mode (only for standby forced by PowerScheduler). ");
      this.comboBoxShutdownMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxShutdownMode_SelectedIndexChanged);
      // 
      // checkBoxShutdownEnabled
      // 
      this.checkBoxShutdownEnabled.AutoSize = true;
      this.checkBoxShutdownEnabled.Location = new System.Drawing.Point(12, 162);
      this.checkBoxShutdownEnabled.Name = "checkBoxShutdownEnabled";
      this.checkBoxShutdownEnabled.Size = new System.Drawing.Size(298, 17);
      this.checkBoxShutdownEnabled.TabIndex = 20;
      this.checkBoxShutdownEnabled.Text = "PowerScheduler forces system to go to standby when idle";
      this.toolTip.SetToolTip(this.checkBoxShutdownEnabled, resources.GetString("checkBoxShutdownEnabled.ToolTip"));
      this.checkBoxShutdownEnabled.UseVisualStyleBackColor = true;
      this.checkBoxShutdownEnabled.CheckedChanged += new System.EventHandler(this.checkBoxShutdownEnabled_CheckedChanged);
      // 
      // checkBoxAutoPowerSettings
      // 
      this.checkBoxAutoPowerSettings.AutoSize = true;
      this.checkBoxAutoPowerSettings.Checked = true;
      this.checkBoxAutoPowerSettings.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAutoPowerSettings.Location = new System.Drawing.Point(12, 104);
      this.checkBoxAutoPowerSettings.Name = "checkBoxAutoPowerSettings";
      this.checkBoxAutoPowerSettings.Size = new System.Drawing.Size(390, 17);
      this.checkBoxAutoPowerSettings.TabIndex = 13;
      this.checkBoxAutoPowerSettings.Text = "Apply recommended windows power settings for selected profile automatically";
      this.toolTip.SetToolTip(this.checkBoxAutoPowerSettings, "If checked, PowerScheduler will set the Windows Power Settings\r\nto some reasonabl" +
              "e values depending on the selected profile.\r\nUncheck this option to configure th" +
              "e Windows Power Settings manually.");
      this.checkBoxAutoPowerSettings.UseVisualStyleBackColor = true;
      this.checkBoxAutoPowerSettings.CheckedChanged += new System.EventHandler(this.checkBoxAutoPowerSettings_CheckedChanged);
      // 
      // buttonPowerSettings
      // 
      this.buttonPowerSettings.AutoSize = true;
      this.buttonPowerSettings.Enabled = false;
      this.buttonPowerSettings.Location = new System.Drawing.Point(34, 129);
      this.buttonPowerSettings.Name = "buttonPowerSettings";
      this.buttonPowerSettings.Size = new System.Drawing.Size(125, 23);
      this.buttonPowerSettings.TabIndex = 12;
      this.buttonPowerSettings.Text = "Configure Manually";
      this.toolTip.SetToolTip(this.buttonPowerSettings, "Configure the Windows Power Settings manually.");
      this.buttonPowerSettings.UseVisualStyleBackColor = true;
      this.buttonPowerSettings.Click += new System.EventHandler(this.buttonPowerSettings_Click);
      // 
      // checkBoxReinitializeController
      // 
      this.checkBoxReinitializeController.AutoSize = true;
      this.checkBoxReinitializeController.Location = new System.Drawing.Point(12, 22);
      this.checkBoxReinitializeController.Name = "checkBoxReinitializeController";
      this.checkBoxReinitializeController.Size = new System.Drawing.Size(303, 17);
      this.checkBoxReinitializeController.TabIndex = 0;
      this.checkBoxReinitializeController.Text = "Reinitialize TV controller on wakeup (also reinitializes tuner)";
      this.toolTip.SetToolTip(this.checkBoxReinitializeController, "Reinitializes the internal TV controller and tuner setup (does not restart the TV" +
              "-Server).");
      this.checkBoxReinitializeController.UseVisualStyleBackColor = true;
      this.checkBoxReinitializeController.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // textBoxRebootCommand
      // 
      this.textBoxRebootCommand.AcceptsTab = true;
      this.textBoxRebootCommand.Location = new System.Drawing.Point(12, 166);
      this.textBoxRebootCommand.Name = "textBoxRebootCommand";
      this.textBoxRebootCommand.Size = new System.Drawing.Size(329, 20);
      this.textBoxRebootCommand.TabIndex = 6;
      this.toolTip.SetToolTip(this.textBoxRebootCommand, resources.GetString("textBoxRebootCommand.ToolTip"));
      this.textBoxRebootCommand.TextChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // textBoxReboot
      // 
      this.textBoxReboot.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.textBoxReboot.Location = new System.Drawing.Point(98, 3);
      this.textBoxReboot.Mask = "90:00";
      this.textBoxReboot.Name = "textBoxReboot";
      this.textBoxReboot.Size = new System.Drawing.Size(42, 20);
      this.textBoxReboot.TabIndex = 1;
      this.textBoxReboot.Text = "0000";
      this.toolTip.SetToolTip(this.textBoxReboot, "System will reboot only when not busy with other tasks.");
      this.textBoxReboot.ValidatingType = typeof(System.DateTime);
      this.textBoxReboot.TextChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // numericUpDownNetworkIdleLimit
      // 
      this.numericUpDownNetworkIdleLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.numericUpDownNetworkIdleLimit.Enabled = false;
      this.numericUpDownNetworkIdleLimit.Location = new System.Drawing.Point(300, 3);
      this.numericUpDownNetworkIdleLimit.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
      this.numericUpDownNetworkIdleLimit.Name = "numericUpDownNetworkIdleLimit";
      this.numericUpDownNetworkIdleLimit.Size = new System.Drawing.Size(58, 20);
      this.numericUpDownNetworkIdleLimit.TabIndex = 2;
      this.toolTip.SetToolTip(this.numericUpDownNetworkIdleLimit, "Select network activity (both incoming and outgoing).");
      this.numericUpDownNetworkIdleLimit.ValueChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxNetworkEnabled
      // 
      this.checkBoxNetworkEnabled.AutoSize = true;
      this.checkBoxNetworkEnabled.Location = new System.Drawing.Point(12, 22);
      this.checkBoxNetworkEnabled.Name = "checkBoxNetworkEnabled";
      this.checkBoxNetworkEnabled.Size = new System.Drawing.Size(309, 17);
      this.checkBoxNetworkEnabled.TabIndex = 0;
      this.checkBoxNetworkEnabled.Text = "Do not put the computer to sleep while the network is active";
      this.toolTip.SetToolTip(this.checkBoxNetworkEnabled, "The computer will not go to standby automatically while the network is active.");
      this.checkBoxNetworkEnabled.UseVisualStyleBackColor = true;
      this.checkBoxNetworkEnabled.CheckedChanged += new System.EventHandler(this.checkBoxNetworkEnabled_CheckedChanged);
      // 
      // checkBoxSharesEnabled
      // 
      this.checkBoxSharesEnabled.AutoSize = true;
      this.checkBoxSharesEnabled.Location = new System.Drawing.Point(12, 22);
      this.checkBoxSharesEnabled.Name = "checkBoxSharesEnabled";
      this.checkBoxSharesEnabled.Size = new System.Drawing.Size(292, 17);
      this.checkBoxSharesEnabled.TabIndex = 0;
      this.checkBoxSharesEnabled.Text = "Do not put the computer to sleep while shares are active";
      this.toolTip.SetToolTip(this.checkBoxSharesEnabled, "The computer will not go to standby automatically while the selected shares are a" +
              "ctive.");
      this.checkBoxSharesEnabled.UseVisualStyleBackColor = true;
      this.checkBoxSharesEnabled.CheckedChanged += new System.EventHandler(this.checkBoxSharesEnabled_CheckedChanged);
      // 
      // comboBoxProfile
      // 
      this.comboBoxProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.comboBoxProfile.ForeColor = System.Drawing.SystemColors.WindowText;
      this.comboBoxProfile.Items.AddRange(new object[] {
            "HTPC",
            "Desktop",
            "Notebook"});
      this.comboBoxProfile.Location = new System.Drawing.Point(3, 85);
      this.comboBoxProfile.Name = "comboBoxProfile";
      this.comboBoxProfile.Size = new System.Drawing.Size(281, 21);
      this.comboBoxProfile.TabIndex = 1;
      this.toolTip.SetToolTip(this.comboBoxProfile, "Select the profile that fits best for your system.");
      this.comboBoxProfile.SelectedIndexChanged += new System.EventHandler(this.comboBoxProfile_SelectedIndexChanged);
      // 
      // buttonExpertMode
      // 
      this.buttonExpertMode.AutoSize = true;
      this.buttonExpertMode.Location = new System.Drawing.Point(12, 217);
      this.buttonExpertMode.Name = "buttonExpertMode";
      this.buttonExpertMode.Size = new System.Drawing.Size(100, 23);
      this.buttonExpertMode.TabIndex = 4;
      this.buttonExpertMode.Text = "-> Expert Mode";
      this.toolTip.SetToolTip(this.buttonExpertMode, resources.GetString("buttonExpertMode.ToolTip"));
      this.buttonExpertMode.UseVisualStyleBackColor = true;
      this.buttonExpertMode.Click += new System.EventHandler(this.buttonExpertMode_Click);
      // 
      // numericUpDownIdleTimeout
      // 
      this.numericUpDownIdleTimeout.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.numericUpDownIdleTimeout.AutoSize = true;
      this.numericUpDownIdleTimeout.Location = new System.Drawing.Point(161, 3);
      this.numericUpDownIdleTimeout.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
      this.numericUpDownIdleTimeout.Name = "numericUpDownIdleTimeout";
      this.numericUpDownIdleTimeout.Size = new System.Drawing.Size(41, 20);
      this.numericUpDownIdleTimeout.TabIndex = 11;
      this.toolTip.SetToolTip(this.numericUpDownIdleTimeout, "Adjust the time after which the system goes to standby when idle\r\n(\"0\" means \"nev" +
              "er\").");
      this.numericUpDownIdleTimeout.ValueChanged += new System.EventHandler(this.numericUpDownIdleTimeout_ValueChanged);
      this.numericUpDownIdleTimeout.EnabledChanged += new System.EventHandler(this.numericUpDownIdleTimeout_EnabledChanged);
      // 
      // checkBoxHomeOnly
      // 
      this.checkBoxHomeOnly.AutoSize = true;
      this.checkBoxHomeOnly.Location = new System.Drawing.Point(12, 22);
      this.checkBoxHomeOnly.Name = "checkBoxHomeOnly";
      this.checkBoxHomeOnly.Size = new System.Drawing.Size(226, 17);
      this.checkBoxHomeOnly.TabIndex = 2;
      this.checkBoxHomeOnly.Text = "Only allow standby when on home window";
      this.toolTip.SetToolTip(this.checkBoxHomeOnly, "Prevents standby while the MP client is not on home window");
      this.checkBoxHomeOnly.UseVisualStyleBackColor = true;
      this.checkBoxHomeOnly.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxUmuteMasterVolume
      // 
      this.checkBoxUmuteMasterVolume.AutoSize = true;
      this.checkBoxUmuteMasterVolume.Checked = true;
      this.checkBoxUmuteMasterVolume.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUmuteMasterVolume.Location = new System.Drawing.Point(12, 104);
      this.checkBoxUmuteMasterVolume.Name = "checkBoxUmuteMasterVolume";
      this.checkBoxUmuteMasterVolume.Size = new System.Drawing.Size(302, 17);
      this.checkBoxUmuteMasterVolume.TabIndex = 3;
      this.checkBoxUmuteMasterVolume.Text = "Unmute master volume on start and on leaving away mode";
      this.toolTip.SetToolTip(this.checkBoxUmuteMasterVolume, "Unmutes master volume if muted wrongly (e.g. after leaving away mode)");
      this.checkBoxUmuteMasterVolume.UseVisualStyleBackColor = true;
      this.checkBoxUmuteMasterVolume.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // groupBoxProcesses
      // 
      this.groupBoxProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxProcesses.Controls.Add(this.checkBoxMPClientRunning);
      this.groupBoxProcesses.Controls.Add(this.checkBoxProcessesAwayMode);
      this.groupBoxProcesses.Controls.Add(this.buttonSelectProcess);
      this.groupBoxProcesses.Controls.Add(this.textBoxProcesses);
      this.groupBoxProcesses.Location = new System.Drawing.Point(6, 6);
      this.groupBoxProcesses.Name = "groupBoxProcesses";
      this.groupBoxProcesses.Size = new System.Drawing.Size(464, 250);
      this.groupBoxProcesses.TabIndex = 0;
      this.groupBoxProcesses.TabStop = false;
      this.groupBoxProcesses.Text = "Processes which should prevent standby";
      // 
      // groupBoxEPG
      // 
      this.groupBoxEPG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxEPG.Controls.Add(this.flowLayoutPanelEPG);
      this.groupBoxEPG.Controls.Add(this.checkBoxEPGAwayMode);
      this.groupBoxEPG.Controls.Add(this.buttonEPGCommand);
      this.groupBoxEPG.Controls.Add(this.textBoxEPGCommand);
      this.groupBoxEPG.Controls.Add(this.labelEPGCommand);
      this.groupBoxEPG.Controls.Add(this.panelEPG);
      this.groupBoxEPG.Controls.Add(this.checkBoxEPGWakeup);
      this.groupBoxEPG.Controls.Add(this.checkBoxEPGPreventStandby);
      this.groupBoxEPG.Location = new System.Drawing.Point(6, 6);
      this.groupBoxEPG.Name = "groupBoxEPG";
      this.groupBoxEPG.Size = new System.Drawing.Size(464, 250);
      this.groupBoxEPG.TabIndex = 0;
      this.groupBoxEPG.TabStop = false;
      this.groupBoxEPG.Text = "EPG settings";
      // 
      // flowLayoutPanelEPG
      // 
      this.flowLayoutPanelEPG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelEPG.AutoSize = true;
      this.flowLayoutPanelEPG.Controls.Add(this.labelEPG1);
      this.flowLayoutPanelEPG.Controls.Add(this.textBoxEPG);
      this.flowLayoutPanelEPG.Controls.Add(this.labelEPG2);
      this.flowLayoutPanelEPG.Location = new System.Drawing.Point(6, 23);
      this.flowLayoutPanelEPG.Name = "flowLayoutPanelEPG";
      this.flowLayoutPanelEPG.Size = new System.Drawing.Size(452, 26);
      this.flowLayoutPanelEPG.TabIndex = 15;
      // 
      // labelEPG1
      // 
      this.labelEPG1.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelEPG1.AutoSize = true;
      this.labelEPG1.Location = new System.Drawing.Point(3, 6);
      this.labelEPG1.Name = "labelEPG1";
      this.labelEPG1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelEPG1.Size = new System.Drawing.Size(113, 13);
      this.labelEPG1.TabIndex = 0;
      this.labelEPG1.Text = "Start grabbing EPG at";
      // 
      // textBoxEPG
      // 
      this.textBoxEPG.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.textBoxEPG.Location = new System.Drawing.Point(122, 3);
      this.textBoxEPG.Mask = "90:00";
      this.textBoxEPG.Name = "textBoxEPG";
      this.textBoxEPG.Size = new System.Drawing.Size(42, 20);
      this.textBoxEPG.TabIndex = 1;
      this.textBoxEPG.Text = "0000";
      this.textBoxEPG.ValidatingType = typeof(System.DateTime);
      this.textBoxEPG.TextChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // labelEPG2
      // 
      this.labelEPG2.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelEPG2.AutoSize = true;
      this.labelEPG2.Location = new System.Drawing.Point(170, 6);
      this.labelEPG2.Name = "labelEPG2";
      this.labelEPG2.Size = new System.Drawing.Size(44, 13);
      this.labelEPG2.TabIndex = 2;
      this.labelEPG2.Text = "o\' clock";
      // 
      // buttonEPGCommand
      // 
      this.buttonEPGCommand.Location = new System.Drawing.Point(349, 218);
      this.buttonEPGCommand.Name = "buttonEPGCommand";
      this.buttonEPGCommand.Size = new System.Drawing.Size(25, 23);
      this.buttonEPGCommand.TabIndex = 9;
      this.buttonEPGCommand.Text = "...";
      this.buttonEPGCommand.UseVisualStyleBackColor = true;
      this.buttonEPGCommand.Click += new System.EventHandler(this.buttonEPGCommand_Click);
      // 
      // labelEPGCommand
      // 
      this.labelEPGCommand.AutoSize = true;
      this.labelEPGCommand.Location = new System.Drawing.Point(9, 204);
      this.labelEPGCommand.Name = "labelEPGCommand";
      this.labelEPGCommand.Size = new System.Drawing.Size(254, 13);
      this.labelEPGCommand.TabIndex = 7;
      this.labelEPGCommand.Text = "Run command before internal handlers are triggered:";
      // 
      // panelEPG
      // 
      this.panelEPG.AutoSize = true;
      this.panelEPG.Controls.Add(this.checkBoxEPGSunday);
      this.panelEPG.Controls.Add(this.checkBoxEPGSaturday);
      this.panelEPG.Controls.Add(this.checkBoxEPGFriday);
      this.panelEPG.Controls.Add(this.checkBoxEPGThursday);
      this.panelEPG.Controls.Add(this.checkBoxEPGWednesday);
      this.panelEPG.Controls.Add(this.checkBoxEPGTuesday);
      this.panelEPG.Controls.Add(this.checkBoxEPGMonday);
      this.panelEPG.Location = new System.Drawing.Point(24, 44);
      this.panelEPG.Name = "panelEPG";
      this.panelEPG.Size = new System.Drawing.Size(400, 62);
      this.panelEPG.TabIndex = 3;
      // 
      // checkBoxEPGSunday
      // 
      this.checkBoxEPGSunday.AutoSize = true;
      this.checkBoxEPGSunday.Location = new System.Drawing.Point(210, 34);
      this.checkBoxEPGSunday.Name = "checkBoxEPGSunday";
      this.checkBoxEPGSunday.Size = new System.Drawing.Size(62, 17);
      this.checkBoxEPGSunday.TabIndex = 6;
      this.checkBoxEPGSunday.Text = "Sunday";
      this.checkBoxEPGSunday.UseVisualStyleBackColor = true;
      this.checkBoxEPGSunday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGSaturday
      // 
      this.checkBoxEPGSaturday.AutoSize = true;
      this.checkBoxEPGSaturday.Location = new System.Drawing.Point(110, 34);
      this.checkBoxEPGSaturday.Name = "checkBoxEPGSaturday";
      this.checkBoxEPGSaturday.Size = new System.Drawing.Size(68, 17);
      this.checkBoxEPGSaturday.TabIndex = 5;
      this.checkBoxEPGSaturday.Text = "Saturday";
      this.checkBoxEPGSaturday.UseVisualStyleBackColor = true;
      this.checkBoxEPGSaturday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGFriday
      // 
      this.checkBoxEPGFriday.AutoSize = true;
      this.checkBoxEPGFriday.Location = new System.Drawing.Point(10, 34);
      this.checkBoxEPGFriday.Name = "checkBoxEPGFriday";
      this.checkBoxEPGFriday.Size = new System.Drawing.Size(54, 17);
      this.checkBoxEPGFriday.TabIndex = 4;
      this.checkBoxEPGFriday.Text = "Friday";
      this.checkBoxEPGFriday.UseVisualStyleBackColor = true;
      this.checkBoxEPGFriday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGThursday
      // 
      this.checkBoxEPGThursday.AutoSize = true;
      this.checkBoxEPGThursday.Location = new System.Drawing.Point(310, 9);
      this.checkBoxEPGThursday.Name = "checkBoxEPGThursday";
      this.checkBoxEPGThursday.Size = new System.Drawing.Size(70, 17);
      this.checkBoxEPGThursday.TabIndex = 3;
      this.checkBoxEPGThursday.Text = "Thursday";
      this.checkBoxEPGThursday.UseVisualStyleBackColor = true;
      this.checkBoxEPGThursday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGWednesday
      // 
      this.checkBoxEPGWednesday.AutoSize = true;
      this.checkBoxEPGWednesday.Location = new System.Drawing.Point(210, 9);
      this.checkBoxEPGWednesday.Name = "checkBoxEPGWednesday";
      this.checkBoxEPGWednesday.Size = new System.Drawing.Size(83, 17);
      this.checkBoxEPGWednesday.TabIndex = 2;
      this.checkBoxEPGWednesday.Text = "Wednesday";
      this.checkBoxEPGWednesday.UseVisualStyleBackColor = true;
      this.checkBoxEPGWednesday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGTuesday
      // 
      this.checkBoxEPGTuesday.AutoSize = true;
      this.checkBoxEPGTuesday.Location = new System.Drawing.Point(110, 9);
      this.checkBoxEPGTuesday.Name = "checkBoxEPGTuesday";
      this.checkBoxEPGTuesday.Size = new System.Drawing.Size(67, 17);
      this.checkBoxEPGTuesday.TabIndex = 1;
      this.checkBoxEPGTuesday.Text = "Tuesday";
      this.checkBoxEPGTuesday.UseVisualStyleBackColor = true;
      this.checkBoxEPGTuesday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGMonday
      // 
      this.checkBoxEPGMonday.AutoSize = true;
      this.checkBoxEPGMonday.Location = new System.Drawing.Point(10, 9);
      this.checkBoxEPGMonday.Name = "checkBoxEPGMonday";
      this.checkBoxEPGMonday.Size = new System.Drawing.Size(64, 17);
      this.checkBoxEPGMonday.TabIndex = 0;
      this.checkBoxEPGMonday.Text = "Monday";
      this.checkBoxEPGMonday.UseVisualStyleBackColor = true;
      this.checkBoxEPGMonday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxEPGWakeup
      // 
      this.checkBoxEPGWakeup.AutoSize = true;
      this.checkBoxEPGWakeup.Location = new System.Drawing.Point(12, 117);
      this.checkBoxEPGWakeup.Name = "checkBoxEPGWakeup";
      this.checkBoxEPGWakeup.Size = new System.Drawing.Size(216, 17);
      this.checkBoxEPGWakeup.TabIndex = 4;
      this.checkBoxEPGWakeup.Text = "Wakeup the computer for EPG grabbing";
      this.checkBoxEPGWakeup.UseVisualStyleBackColor = true;
      this.checkBoxEPGWakeup.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // openFileDialog
      // 
      this.openFileDialog.Filter = "Executable Files (*.exe; *.cmd; *.bat)|*.exe; *.cmd; *.bat|All Files (*.*)|*.*";
      this.openFileDialog.Title = "Choose command";
      // 
      // groupBoxStatus
      // 
      this.groupBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxStatus.BackColor = System.Drawing.SystemColors.Control;
      this.groupBoxStatus.Controls.Add(this.labelStandbyStatus);
      this.groupBoxStatus.Controls.Add(this.textBoxStandbyHandler);
      this.groupBoxStatus.Controls.Add(this.labelStandbyHandler);
      this.groupBoxStatus.Controls.Add(this.labelWakeupHandler);
      this.groupBoxStatus.Controls.Add(this.labelWakeupTime);
      this.groupBoxStatus.Controls.Add(this.labelWakeupTimeValue);
      this.groupBoxStatus.Location = new System.Drawing.Point(0, 326);
      this.groupBoxStatus.Name = "groupBoxStatus";
      this.groupBoxStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.groupBoxStatus.Size = new System.Drawing.Size(484, 100);
      this.groupBoxStatus.TabIndex = 3;
      this.groupBoxStatus.TabStop = false;
      this.groupBoxStatus.Text = "Wakeup / Standby Status";
      // 
      // labelStandbyStatus
      // 
      this.labelStandbyStatus.AutoSize = true;
      this.labelStandbyStatus.Location = new System.Drawing.Point(12, 42);
      this.labelStandbyStatus.Name = "labelStandbyStatus";
      this.labelStandbyStatus.Size = new System.Drawing.Size(158, 13);
      this.labelStandbyStatus.TabIndex = 5;
      this.labelStandbyStatus.Text = "Standby is handled by Windows";
      // 
      // textBoxStandbyHandler
      // 
      this.textBoxStandbyHandler.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxStandbyHandler.BackColor = System.Drawing.SystemColors.Control;
      this.textBoxStandbyHandler.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxStandbyHandler.Location = new System.Drawing.Point(132, 61);
      this.textBoxStandbyHandler.Multiline = true;
      this.textBoxStandbyHandler.Name = "textBoxStandbyHandler";
      this.textBoxStandbyHandler.ReadOnly = true;
      this.textBoxStandbyHandler.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.textBoxStandbyHandler.Size = new System.Drawing.Size(342, 33);
      this.textBoxStandbyHandler.TabIndex = 3;
      // 
      // labelStandbyHandler
      // 
      this.labelStandbyHandler.AutoSize = true;
      this.labelStandbyHandler.Location = new System.Drawing.Point(12, 61);
      this.labelStandbyHandler.Name = "labelStandbyHandler";
      this.labelStandbyHandler.Size = new System.Drawing.Size(114, 13);
      this.labelStandbyHandler.TabIndex = 4;
      this.labelStandbyHandler.Text = "Standby prevented by:";
      // 
      // labelWakeupHandler
      // 
      this.labelWakeupHandler.AutoSize = true;
      this.labelWakeupHandler.Location = new System.Drawing.Point(244, 22);
      this.labelWakeupHandler.Name = "labelWakeupHandler";
      this.labelWakeupHandler.Size = new System.Drawing.Size(0, 13);
      this.labelWakeupHandler.TabIndex = 2;
      // 
      // labelWakeupTime
      // 
      this.labelWakeupTime.AutoSize = true;
      this.labelWakeupTime.Location = new System.Drawing.Point(12, 23);
      this.labelWakeupTime.Name = "labelWakeupTime";
      this.labelWakeupTime.Size = new System.Drawing.Size(95, 13);
      this.labelWakeupTime.TabIndex = 0;
      this.labelWakeupTime.Text = "Next wakeup time:";
      // 
      // labelWakeupTimeValue
      // 
      this.labelWakeupTimeValue.AutoSize = true;
      this.labelWakeupTimeValue.Location = new System.Drawing.Point(130, 22);
      this.labelWakeupTimeValue.Name = "labelWakeupTimeValue";
      this.labelWakeupTimeValue.Size = new System.Drawing.Size(0, 13);
      this.labelWakeupTimeValue.TabIndex = 1;
      // 
      // tabPageAdvanced
      // 
      this.tabPageAdvanced.Controls.Add(this.groupBoxAdvanced);
      this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
      this.tabPageAdvanced.Name = "tabPageAdvanced";
      this.tabPageAdvanced.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageAdvanced.Size = new System.Drawing.Size(476, 264);
      this.tabPageAdvanced.TabIndex = 1;
      this.tabPageAdvanced.Text = "Advanced";
      this.tabPageAdvanced.UseVisualStyleBackColor = true;
      // 
      // groupBoxAdvanced
      // 
      this.groupBoxAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAdvanced.Controls.Add(this.checkBoxAutoPowerSettings);
      this.groupBoxAdvanced.Controls.Add(this.buttonPowerSettings);
      this.groupBoxAdvanced.Controls.Add(this.buttonCommand);
      this.groupBoxAdvanced.Controls.Add(this.textBoxCommand);
      this.groupBoxAdvanced.Controls.Add(this.labelCommand);
      this.groupBoxAdvanced.Controls.Add(this.checkBoxReinitializeController);
      this.groupBoxAdvanced.Controls.Add(this.flowLayoutPanelShutdownMode);
      this.groupBoxAdvanced.Controls.Add(this.checkBoxShutdownEnabled);
      this.groupBoxAdvanced.Location = new System.Drawing.Point(6, 6);
      this.groupBoxAdvanced.Name = "groupBoxAdvanced";
      this.groupBoxAdvanced.Size = new System.Drawing.Size(464, 250);
      this.groupBoxAdvanced.TabIndex = 0;
      this.groupBoxAdvanced.TabStop = false;
      this.groupBoxAdvanced.Text = "Advanced settings";
      // 
      // buttonCommand
      // 
      this.buttonCommand.AutoSize = true;
      this.buttonCommand.Location = new System.Drawing.Point(350, 69);
      this.buttonCommand.Name = "buttonCommand";
      this.buttonCommand.Size = new System.Drawing.Size(26, 23);
      this.buttonCommand.TabIndex = 3;
      this.buttonCommand.Text = "...";
      this.buttonCommand.UseVisualStyleBackColor = true;
      this.buttonCommand.Click += new System.EventHandler(this.buttonStandbyWakeupCommand_Click);
      // 
      // labelCommand
      // 
      this.labelCommand.AutoSize = true;
      this.labelCommand.Location = new System.Drawing.Point(9, 55);
      this.labelCommand.Name = "labelCommand";
      this.labelCommand.Size = new System.Drawing.Size(226, 13);
      this.labelCommand.TabIndex = 1;
      this.labelCommand.Text = "Run command on system power state change:";
      // 
      // flowLayoutPanelShutdownMode
      // 
      this.flowLayoutPanelShutdownMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelShutdownMode.AutoSize = true;
      this.flowLayoutPanelShutdownMode.Controls.Add(this.labelShutdownMode);
      this.flowLayoutPanelShutdownMode.Controls.Add(this.comboBoxShutdownMode);
      this.flowLayoutPanelShutdownMode.Location = new System.Drawing.Point(28, 181);
      this.flowLayoutPanelShutdownMode.Name = "flowLayoutPanelShutdownMode";
      this.flowLayoutPanelShutdownMode.Size = new System.Drawing.Size(426, 27);
      this.flowLayoutPanelShutdownMode.TabIndex = 23;
      // 
      // labelShutdownMode
      // 
      this.labelShutdownMode.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelShutdownMode.AutoSize = true;
      this.labelShutdownMode.Location = new System.Drawing.Point(3, 7);
      this.labelShutdownMode.Name = "labelShutdownMode";
      this.labelShutdownMode.Size = new System.Drawing.Size(75, 13);
      this.labelShutdownMode.TabIndex = 22;
      this.labelShutdownMode.Text = "Standby mode";
      // 
      // tabPageReboot
      // 
      this.tabPageReboot.Controls.Add(this.groupBoxReboot);
      this.tabPageReboot.Location = new System.Drawing.Point(4, 22);
      this.tabPageReboot.Name = "tabPageReboot";
      this.tabPageReboot.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageReboot.Size = new System.Drawing.Size(476, 264);
      this.tabPageReboot.TabIndex = 6;
      this.tabPageReboot.Text = "Reboot";
      this.tabPageReboot.UseVisualStyleBackColor = true;
      // 
      // groupBoxReboot
      // 
      this.groupBoxReboot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxReboot.Controls.Add(this.flowLayoutPanelReboot);
      this.groupBoxReboot.Controls.Add(this.buttonRebootCommand);
      this.groupBoxReboot.Controls.Add(this.textBoxRebootCommand);
      this.groupBoxReboot.Controls.Add(this.labelRebootCommand);
      this.groupBoxReboot.Controls.Add(this.panelReboot);
      this.groupBoxReboot.Controls.Add(this.checkBoxRebootWakeup);
      this.groupBoxReboot.Location = new System.Drawing.Point(6, 6);
      this.groupBoxReboot.Name = "groupBoxReboot";
      this.groupBoxReboot.Size = new System.Drawing.Size(464, 250);
      this.groupBoxReboot.TabIndex = 0;
      this.groupBoxReboot.TabStop = false;
      this.groupBoxReboot.Text = "Reboot settings";
      // 
      // flowLayoutPanelReboot
      // 
      this.flowLayoutPanelReboot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelReboot.AutoSize = true;
      this.flowLayoutPanelReboot.Controls.Add(this.labelReboot1);
      this.flowLayoutPanelReboot.Controls.Add(this.textBoxReboot);
      this.flowLayoutPanelReboot.Controls.Add(this.labelReboot2);
      this.flowLayoutPanelReboot.Location = new System.Drawing.Point(6, 23);
      this.flowLayoutPanelReboot.Name = "flowLayoutPanelReboot";
      this.flowLayoutPanelReboot.Size = new System.Drawing.Size(452, 26);
      this.flowLayoutPanelReboot.TabIndex = 16;
      // 
      // labelReboot1
      // 
      this.labelReboot1.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelReboot1.AutoSize = true;
      this.labelReboot1.Location = new System.Drawing.Point(3, 6);
      this.labelReboot1.Name = "labelReboot1";
      this.labelReboot1.Size = new System.Drawing.Size(89, 13);
      this.labelReboot1.TabIndex = 0;
      this.labelReboot1.Text = "Reboot system at";
      // 
      // labelReboot2
      // 
      this.labelReboot2.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelReboot2.AutoSize = true;
      this.labelReboot2.Location = new System.Drawing.Point(146, 6);
      this.labelReboot2.Name = "labelReboot2";
      this.labelReboot2.Size = new System.Drawing.Size(44, 13);
      this.labelReboot2.TabIndex = 2;
      this.labelReboot2.Text = "o\' clock";
      // 
      // buttonRebootCommand
      // 
      this.buttonRebootCommand.Location = new System.Drawing.Point(349, 164);
      this.buttonRebootCommand.Name = "buttonRebootCommand";
      this.buttonRebootCommand.Size = new System.Drawing.Size(25, 23);
      this.buttonRebootCommand.TabIndex = 7;
      this.buttonRebootCommand.Text = "...";
      this.buttonRebootCommand.UseVisualStyleBackColor = true;
      this.buttonRebootCommand.Click += new System.EventHandler(this.buttonRebootCommand_Click);
      // 
      // labelRebootCommand
      // 
      this.labelRebootCommand.AutoSize = true;
      this.labelRebootCommand.Location = new System.Drawing.Point(9, 150);
      this.labelRebootCommand.Name = "labelRebootCommand";
      this.labelRebootCommand.Size = new System.Drawing.Size(145, 13);
      this.labelRebootCommand.TabIndex = 5;
      this.labelRebootCommand.Text = "Run command before reboot:";
      // 
      // panelReboot
      // 
      this.panelReboot.AutoSize = true;
      this.panelReboot.Controls.Add(this.checkBoxRebootSunday);
      this.panelReboot.Controls.Add(this.checkBoxRebootSaturday);
      this.panelReboot.Controls.Add(this.checkBoxRebootFriday);
      this.panelReboot.Controls.Add(this.checkBoxRebootThursday);
      this.panelReboot.Controls.Add(this.checkBoxRebootWednesday);
      this.panelReboot.Controls.Add(this.checkBoxRebootTuesday);
      this.panelReboot.Controls.Add(this.checkBoxRebootMonday);
      this.panelReboot.Location = new System.Drawing.Point(24, 44);
      this.panelReboot.Name = "panelReboot";
      this.panelReboot.Size = new System.Drawing.Size(400, 62);
      this.panelReboot.TabIndex = 3;
      // 
      // checkBoxRebootSunday
      // 
      this.checkBoxRebootSunday.AutoSize = true;
      this.checkBoxRebootSunday.Location = new System.Drawing.Point(210, 34);
      this.checkBoxRebootSunday.Name = "checkBoxRebootSunday";
      this.checkBoxRebootSunday.Size = new System.Drawing.Size(62, 17);
      this.checkBoxRebootSunday.TabIndex = 6;
      this.checkBoxRebootSunday.Text = "Sunday";
      this.checkBoxRebootSunday.UseVisualStyleBackColor = true;
      this.checkBoxRebootSunday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootSaturday
      // 
      this.checkBoxRebootSaturday.AutoSize = true;
      this.checkBoxRebootSaturday.Location = new System.Drawing.Point(110, 34);
      this.checkBoxRebootSaturday.Name = "checkBoxRebootSaturday";
      this.checkBoxRebootSaturday.Size = new System.Drawing.Size(68, 17);
      this.checkBoxRebootSaturday.TabIndex = 5;
      this.checkBoxRebootSaturday.Text = "Saturday";
      this.checkBoxRebootSaturday.UseVisualStyleBackColor = true;
      this.checkBoxRebootSaturday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootFriday
      // 
      this.checkBoxRebootFriday.AutoSize = true;
      this.checkBoxRebootFriday.Location = new System.Drawing.Point(10, 34);
      this.checkBoxRebootFriday.Name = "checkBoxRebootFriday";
      this.checkBoxRebootFriday.Size = new System.Drawing.Size(54, 17);
      this.checkBoxRebootFriday.TabIndex = 4;
      this.checkBoxRebootFriday.Text = "Friday";
      this.checkBoxRebootFriday.UseVisualStyleBackColor = true;
      this.checkBoxRebootFriday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootThursday
      // 
      this.checkBoxRebootThursday.AutoSize = true;
      this.checkBoxRebootThursday.Location = new System.Drawing.Point(310, 9);
      this.checkBoxRebootThursday.Name = "checkBoxRebootThursday";
      this.checkBoxRebootThursday.Size = new System.Drawing.Size(70, 17);
      this.checkBoxRebootThursday.TabIndex = 3;
      this.checkBoxRebootThursday.Text = "Thursday";
      this.checkBoxRebootThursday.UseVisualStyleBackColor = true;
      this.checkBoxRebootThursday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootWednesday
      // 
      this.checkBoxRebootWednesday.AutoSize = true;
      this.checkBoxRebootWednesday.Location = new System.Drawing.Point(210, 9);
      this.checkBoxRebootWednesday.Name = "checkBoxRebootWednesday";
      this.checkBoxRebootWednesday.Size = new System.Drawing.Size(83, 17);
      this.checkBoxRebootWednesday.TabIndex = 2;
      this.checkBoxRebootWednesday.Text = "Wednesday";
      this.checkBoxRebootWednesday.UseVisualStyleBackColor = true;
      this.checkBoxRebootWednesday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootTuesday
      // 
      this.checkBoxRebootTuesday.AutoSize = true;
      this.checkBoxRebootTuesday.Location = new System.Drawing.Point(110, 9);
      this.checkBoxRebootTuesday.Name = "checkBoxRebootTuesday";
      this.checkBoxRebootTuesday.Size = new System.Drawing.Size(67, 17);
      this.checkBoxRebootTuesday.TabIndex = 1;
      this.checkBoxRebootTuesday.Text = "Tuesday";
      this.checkBoxRebootTuesday.UseVisualStyleBackColor = true;
      this.checkBoxRebootTuesday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // checkBoxRebootMonday
      // 
      this.checkBoxRebootMonday.AutoSize = true;
      this.checkBoxRebootMonday.Location = new System.Drawing.Point(10, 9);
      this.checkBoxRebootMonday.Name = "checkBoxRebootMonday";
      this.checkBoxRebootMonday.Size = new System.Drawing.Size(64, 17);
      this.checkBoxRebootMonday.TabIndex = 0;
      this.checkBoxRebootMonday.Text = "Monday";
      this.checkBoxRebootMonday.UseVisualStyleBackColor = true;
      this.checkBoxRebootMonday.CheckedChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // tabPageNetwork
      // 
      this.tabPageNetwork.Controls.Add(this.groupBoxNetwork);
      this.tabPageNetwork.Location = new System.Drawing.Point(4, 22);
      this.tabPageNetwork.Name = "tabPageNetwork";
      this.tabPageNetwork.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageNetwork.Size = new System.Drawing.Size(476, 264);
      this.tabPageNetwork.TabIndex = 5;
      this.tabPageNetwork.Text = "Network";
      this.tabPageNetwork.UseVisualStyleBackColor = true;
      // 
      // groupBoxNetwork
      // 
      this.groupBoxNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxNetwork.Controls.Add(this.flowLayoutPanelNetworkIdleLimit);
      this.groupBoxNetwork.Controls.Add(this.checkBoxNetworkAwayMode);
      this.groupBoxNetwork.Controls.Add(this.checkBoxNetworkEnabled);
      this.groupBoxNetwork.Location = new System.Drawing.Point(6, 6);
      this.groupBoxNetwork.Name = "groupBoxNetwork";
      this.groupBoxNetwork.Size = new System.Drawing.Size(464, 250);
      this.groupBoxNetwork.TabIndex = 0;
      this.groupBoxNetwork.TabStop = false;
      this.groupBoxNetwork.Text = "Network activity which should prevent standby";
      // 
      // flowLayoutPanelNetworkIdleLimit
      // 
      this.flowLayoutPanelNetworkIdleLimit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelNetworkIdleLimit.AutoSize = true;
      this.flowLayoutPanelNetworkIdleLimit.Controls.Add(this.labelNetwork);
      this.flowLayoutPanelNetworkIdleLimit.Controls.Add(this.numericUpDownNetworkIdleLimit);
      this.flowLayoutPanelNetworkIdleLimit.Location = new System.Drawing.Point(27, 42);
      this.flowLayoutPanelNetworkIdleLimit.Name = "flowLayoutPanelNetworkIdleLimit";
      this.flowLayoutPanelNetworkIdleLimit.Size = new System.Drawing.Size(431, 26);
      this.flowLayoutPanelNetworkIdleLimit.TabIndex = 16;
      // 
      // labelNetwork
      // 
      this.labelNetwork.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelNetwork.AutoSize = true;
      this.labelNetwork.Location = new System.Drawing.Point(3, 6);
      this.labelNetwork.Name = "labelNetwork";
      this.labelNetwork.Size = new System.Drawing.Size(291, 13);
      this.labelNetwork.TabIndex = 1;
      this.labelNetwork.Text = "Minimum transfer rate considered as network activity in KB/s";
      // 
      // tabPageShares
      // 
      this.tabPageShares.Controls.Add(this.groupBoxShares);
      this.tabPageShares.Location = new System.Drawing.Point(4, 22);
      this.tabPageShares.Name = "tabPageShares";
      this.tabPageShares.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageShares.Size = new System.Drawing.Size(476, 264);
      this.tabPageShares.TabIndex = 4;
      this.tabPageShares.Text = "Shares";
      this.tabPageShares.UseVisualStyleBackColor = true;
      // 
      // groupBoxShares
      // 
      this.groupBoxShares.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxShares.Controls.Add(this.buttonSelectShare);
      this.groupBoxShares.Controls.Add(this.checkBoxSharesAwayMode);
      this.groupBoxShares.Controls.Add(this.labelShares);
      this.groupBoxShares.Controls.Add(this.dataGridShares);
      this.groupBoxShares.Controls.Add(this.checkBoxSharesEnabled);
      this.groupBoxShares.Location = new System.Drawing.Point(6, 6);
      this.groupBoxShares.Name = "groupBoxShares";
      this.groupBoxShares.Size = new System.Drawing.Size(464, 250);
      this.groupBoxShares.TabIndex = 0;
      this.groupBoxShares.TabStop = false;
      this.groupBoxShares.Text = "Active Shares which should prevent standby";
      // 
      // tabPageProcesses
      // 
      this.tabPageProcesses.Controls.Add(this.groupBoxProcesses);
      this.tabPageProcesses.Location = new System.Drawing.Point(4, 22);
      this.tabPageProcesses.Name = "tabPageProcesses";
      this.tabPageProcesses.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageProcesses.Size = new System.Drawing.Size(476, 264);
      this.tabPageProcesses.TabIndex = 3;
      this.tabPageProcesses.Text = "Processes";
      this.tabPageProcesses.UseVisualStyleBackColor = true;
      // 
      // tabPageEPG
      // 
      this.tabPageEPG.Controls.Add(this.groupBoxEPG);
      this.tabPageEPG.Location = new System.Drawing.Point(4, 22);
      this.tabPageEPG.Name = "tabPageEPG";
      this.tabPageEPG.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageEPG.Size = new System.Drawing.Size(476, 264);
      this.tabPageEPG.TabIndex = 2;
      this.tabPageEPG.Text = "EPG";
      this.tabPageEPG.UseVisualStyleBackColor = true;
      // 
      // tabPageGeneral
      // 
      this.tabPageGeneral.Controls.Add(this.groupBoxGeneral);
      this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneral.Name = "tabPageGeneral";
      this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageGeneral.Size = new System.Drawing.Size(476, 264);
      this.tabPageGeneral.TabIndex = 5;
      this.tabPageGeneral.Text = "General";
      this.tabPageGeneral.UseVisualStyleBackColor = true;
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGeneral.Controls.Add(this.flowLayoutPanelGeneral);
      this.groupBoxGeneral.Controls.Add(this.labelExpertMode);
      this.groupBoxGeneral.Controls.Add(this.buttonExpertMode);
      this.groupBoxGeneral.Location = new System.Drawing.Point(6, 6);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(464, 252);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General settings";
      // 
      // flowLayoutPanelGeneral
      // 
      this.flowLayoutPanelGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelGeneral.AutoSize = true;
      this.flowLayoutPanelGeneral.Controls.Add(this.textBoxProfile);
      this.flowLayoutPanelGeneral.Controls.Add(this.comboBoxProfile);
      this.flowLayoutPanelGeneral.Controls.Add(this.flowLayoutPanelIdleTimeout);
      this.flowLayoutPanelGeneral.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.flowLayoutPanelGeneral.Location = new System.Drawing.Point(9, 23);
      this.flowLayoutPanelGeneral.Name = "flowLayoutPanelGeneral";
      this.flowLayoutPanelGeneral.Size = new System.Drawing.Size(455, 158);
      this.flowLayoutPanelGeneral.TabIndex = 15;
      // 
      // textBoxProfile
      // 
      this.textBoxProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxProfile.BackColor = System.Drawing.SystemColors.Window;
      this.textBoxProfile.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.textBoxProfile.Location = new System.Drawing.Point(3, 3);
      this.textBoxProfile.Multiline = true;
      this.textBoxProfile.Name = "textBoxProfile";
      this.textBoxProfile.ReadOnly = true;
      this.textBoxProfile.Size = new System.Drawing.Size(446, 76);
      this.textBoxProfile.TabIndex = 0;
      this.textBoxProfile.Text = resources.GetString("textBoxProfile.Text");
      this.textBoxProfile.WordWrap = false;
      // 
      // flowLayoutPanelIdleTimeout
      // 
      this.flowLayoutPanelIdleTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelIdleTimeout.AutoSize = true;
      this.flowLayoutPanelIdleTimeout.Controls.Add(this.labelIdleTimeout1);
      this.flowLayoutPanelIdleTimeout.Controls.Add(this.numericUpDownIdleTimeout);
      this.flowLayoutPanelIdleTimeout.Controls.Add(this.labelIdleTimeout2);
      this.flowLayoutPanelIdleTimeout.Location = new System.Drawing.Point(3, 129);
      this.flowLayoutPanelIdleTimeout.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
      this.flowLayoutPanelIdleTimeout.Name = "flowLayoutPanelIdleTimeout";
      this.flowLayoutPanelIdleTimeout.Size = new System.Drawing.Size(446, 26);
      this.flowLayoutPanelIdleTimeout.TabIndex = 14;
      // 
      // labelIdleTimeout1
      // 
      this.labelIdleTimeout1.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelIdleTimeout1.AutoSize = true;
      this.labelIdleTimeout1.Location = new System.Drawing.Point(3, 6);
      this.labelIdleTimeout1.Name = "labelIdleTimeout1";
      this.labelIdleTimeout1.Size = new System.Drawing.Size(152, 13);
      this.labelIdleTimeout1.TabIndex = 10;
      this.labelIdleTimeout1.Text = "Put the computer to sleep after";
      // 
      // labelIdleTimeout2
      // 
      this.labelIdleTimeout2.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.labelIdleTimeout2.AutoSize = true;
      this.labelIdleTimeout2.Location = new System.Drawing.Point(208, 6);
      this.labelIdleTimeout2.Name = "labelIdleTimeout2";
      this.labelIdleTimeout2.Size = new System.Drawing.Size(43, 13);
      this.labelIdleTimeout2.TabIndex = 12;
      this.labelIdleTimeout2.Text = "minutes";
      // 
      // labelExpertMode
      // 
      this.labelExpertMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelExpertMode.AutoSize = true;
      this.labelExpertMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelExpertMode.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.labelExpertMode.Location = new System.Drawing.Point(237, 212);
      this.labelExpertMode.Name = "labelExpertMode";
      this.labelExpertMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.labelExpertMode.Size = new System.Drawing.Size(213, 31);
      this.labelExpertMode.TabIndex = 13;
      this.labelExpertMode.Text = "Plug&&Play Mode";
      this.labelExpertMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // tabControl
      // 
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageGeneral);
      this.tabControl.Controls.Add(this.tabPageClient);
      this.tabControl.Controls.Add(this.tabPageEPG);
      this.tabControl.Controls.Add(this.tabPageReboot);
      this.tabControl.Controls.Add(this.tabPageProcesses);
      this.tabControl.Controls.Add(this.tabPageShares);
      this.tabControl.Controls.Add(this.tabPageNetwork);
      this.tabControl.Controls.Add(this.tabPageAdvanced);
      this.tabControl.Controls.Add(this.tabPageLegacy);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.tabControl.SelectedIndex = 0;
      this.tabControl.ShowToolTips = true;
      this.tabControl.Size = new System.Drawing.Size(484, 290);
      this.tabControl.TabIndex = 1;
      this.tabControl.Tag = "";
      // 
      // tabPageClient
      // 
      this.tabPageClient.Controls.Add(this.groupBoxClient);
      this.tabPageClient.Location = new System.Drawing.Point(4, 22);
      this.tabPageClient.Name = "tabPageClient";
      this.tabPageClient.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageClient.Size = new System.Drawing.Size(476, 264);
      this.tabPageClient.TabIndex = 7;
      this.tabPageClient.Text = "Client";
      this.tabPageClient.UseVisualStyleBackColor = true;
      // 
      // groupBoxClient
      // 
      this.groupBoxClient.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxClient.Controls.Add(this.checkBoxUmuteMasterVolume);
      this.groupBoxClient.Controls.Add(this.checkBoxHomeOnly);
      this.groupBoxClient.Location = new System.Drawing.Point(6, 6);
      this.groupBoxClient.Name = "groupBoxClient";
      this.groupBoxClient.Size = new System.Drawing.Size(464, 252);
      this.groupBoxClient.TabIndex = 1;
      this.groupBoxClient.TabStop = false;
      this.groupBoxClient.Text = "Client settings";
      // 
      // tabPageLegacy
      // 
      this.tabPageLegacy.Controls.Add(this.groupBoxLegacy);
      this.tabPageLegacy.Location = new System.Drawing.Point(4, 22);
      this.tabPageLegacy.Name = "tabPageLegacy";
      this.tabPageLegacy.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageLegacy.Size = new System.Drawing.Size(476, 264);
      this.tabPageLegacy.TabIndex = 8;
      this.tabPageLegacy.Text = "Legacy";
      this.tabPageLegacy.UseVisualStyleBackColor = true;
      // 
      // groupBoxLegacy
      // 
      this.groupBoxLegacy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLegacy.Controls.Add(this.label4);
      this.groupBoxLegacy.Controls.Add(this.flowLayoutPanelStandbyHours);
      this.groupBoxLegacy.Controls.Add(this.flowLayoutPanelPreNoStandbyTime);
      this.groupBoxLegacy.Controls.Add(this.flowLayoutPanelPreWakeupTime);
      this.groupBoxLegacy.Location = new System.Drawing.Point(6, 7);
      this.groupBoxLegacy.Name = "groupBoxLegacy";
      this.groupBoxLegacy.Size = new System.Drawing.Size(464, 250);
      this.groupBoxLegacy.TabIndex = 1;
      this.groupBoxLegacy.TabStop = false;
      this.groupBoxLegacy.Text = "Legacy settings";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(9, 95);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(113, 13);
      this.label4.TabIndex = 29;
      this.label4.Text = "Allowed standby hours";
      // 
      // flowLayoutPanelStandbyHours
      // 
      this.flowLayoutPanelStandbyHours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelStandbyHours.AutoSize = true;
      this.flowLayoutPanelStandbyHours.Controls.Add(this.label5);
      this.flowLayoutPanelStandbyHours.Controls.Add(this.numericUpDownStandbyHoursFrom);
      this.flowLayoutPanelStandbyHours.Controls.Add(this.label6);
      this.flowLayoutPanelStandbyHours.Controls.Add(this.numericUpDownStandbyHoursTo);
      this.flowLayoutPanelStandbyHours.Controls.Add(this.label7);
      this.flowLayoutPanelStandbyHours.Location = new System.Drawing.Point(6, 114);
      this.flowLayoutPanelStandbyHours.Name = "flowLayoutPanelStandbyHours";
      this.flowLayoutPanelStandbyHours.Size = new System.Drawing.Size(448, 26);
      this.flowLayoutPanelStandbyHours.TabIndex = 28;
      // 
      // label5
      // 
      this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(3, 6);
      this.label5.Name = "label5";
      this.label5.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label5.Size = new System.Drawing.Size(142, 13);
      this.label5.TabIndex = 0;
      this.label5.Text = "Only allow standby between";
      // 
      // numericUpDownStandbyHoursFrom
      // 
      this.numericUpDownStandbyHoursFrom.Location = new System.Drawing.Point(151, 3);
      this.numericUpDownStandbyHoursFrom.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
      this.numericUpDownStandbyHoursFrom.Name = "numericUpDownStandbyHoursFrom";
      this.numericUpDownStandbyHoursFrom.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownStandbyHoursFrom.TabIndex = 3;
      this.numericUpDownStandbyHoursFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numericUpDownStandbyHoursFrom.ValueChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // label6
      // 
      this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(204, 6);
      this.label6.Name = "label6";
      this.label6.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label6.Size = new System.Drawing.Size(28, 13);
      this.label6.TabIndex = 4;
      this.label6.Text = "and";
      // 
      // numericUpDownStandbyHoursTo
      // 
      this.numericUpDownStandbyHoursTo.Location = new System.Drawing.Point(238, 3);
      this.numericUpDownStandbyHoursTo.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
      this.numericUpDownStandbyHoursTo.Name = "numericUpDownStandbyHoursTo";
      this.numericUpDownStandbyHoursTo.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownStandbyHoursTo.TabIndex = 5;
      this.numericUpDownStandbyHoursTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numericUpDownStandbyHoursTo.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
      this.numericUpDownStandbyHoursTo.ValueChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // label7
      // 
      this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(291, 6);
      this.label7.Name = "label7";
      this.label7.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label7.Size = new System.Drawing.Size(47, 13);
      this.label7.TabIndex = 6;
      this.label7.Text = "o\' clock";
      // 
      // flowLayoutPanelPreNoStandbyTime
      // 
      this.flowLayoutPanelPreNoStandbyTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelPreNoStandbyTime.AutoSize = true;
      this.flowLayoutPanelPreNoStandbyTime.Controls.Add(this.label1);
      this.flowLayoutPanelPreNoStandbyTime.Controls.Add(this.numericUpDownPreNoStandbyTime);
      this.flowLayoutPanelPreNoStandbyTime.Location = new System.Drawing.Point(6, 56);
      this.flowLayoutPanelPreNoStandbyTime.Name = "flowLayoutPanelPreNoStandbyTime";
      this.flowLayoutPanelPreNoStandbyTime.Size = new System.Drawing.Size(448, 26);
      this.flowLayoutPanelPreNoStandbyTime.TabIndex = 27;
      // 
      // label1
      // 
      this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 6);
      this.label1.Name = "label1";
      this.label1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Size = new System.Drawing.Size(157, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Pre-no-standby time in seconds";
      // 
      // numericUpDownPreNoStandbyTime
      // 
      this.numericUpDownPreNoStandbyTime.Location = new System.Drawing.Point(166, 3);
      this.numericUpDownPreNoStandbyTime.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownPreNoStandbyTime.Name = "numericUpDownPreNoStandbyTime";
      this.numericUpDownPreNoStandbyTime.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownPreNoStandbyTime.TabIndex = 3;
      this.numericUpDownPreNoStandbyTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numericUpDownPreNoStandbyTime.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
      this.numericUpDownPreNoStandbyTime.ValueChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // flowLayoutPanelPreWakeupTime
      // 
      this.flowLayoutPanelPreWakeupTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanelPreWakeupTime.AutoSize = true;
      this.flowLayoutPanelPreWakeupTime.Controls.Add(this.label3);
      this.flowLayoutPanelPreWakeupTime.Controls.Add(this.numericUpDownPreWakeupTime);
      this.flowLayoutPanelPreWakeupTime.Location = new System.Drawing.Point(6, 23);
      this.flowLayoutPanelPreWakeupTime.Name = "flowLayoutPanelPreWakeupTime";
      this.flowLayoutPanelPreWakeupTime.Size = new System.Drawing.Size(448, 26);
      this.flowLayoutPanelPreWakeupTime.TabIndex = 26;
      // 
      // label3
      // 
      this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(3, 6);
      this.label3.Name = "label3";
      this.label3.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Size = new System.Drawing.Size(143, 13);
      this.label3.TabIndex = 0;
      this.label3.Text = "Pre-wakeup time in seconds";
      // 
      // numericUpDownPreWakeupTime
      // 
      this.numericUpDownPreWakeupTime.Location = new System.Drawing.Point(152, 3);
      this.numericUpDownPreWakeupTime.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
      this.numericUpDownPreWakeupTime.Name = "numericUpDownPreWakeupTime";
      this.numericUpDownPreWakeupTime.Size = new System.Drawing.Size(47, 20);
      this.numericUpDownPreWakeupTime.TabIndex = 3;
      this.numericUpDownPreWakeupTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numericUpDownPreWakeupTime.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.numericUpDownPreWakeupTime.ValueChanged += new System.EventHandler(this.buttonApply_Enable);
      // 
      // PowerSchedulerSetup
      // 
      this.Controls.Add(this.groupBoxStatus);
      this.Controls.Add(this.buttonApply);
      this.Controls.Add(this.tabControl);
      this.Name = "PowerSchedulerSetup";
      this.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.Size = new System.Drawing.Size(484, 426);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridShares)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNetworkIdleLimit)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIdleTimeout)).EndInit();
      this.groupBoxProcesses.ResumeLayout(false);
      this.groupBoxProcesses.PerformLayout();
      this.groupBoxEPG.ResumeLayout(false);
      this.groupBoxEPG.PerformLayout();
      this.flowLayoutPanelEPG.ResumeLayout(false);
      this.flowLayoutPanelEPG.PerformLayout();
      this.panelEPG.ResumeLayout(false);
      this.panelEPG.PerformLayout();
      this.groupBoxStatus.ResumeLayout(false);
      this.groupBoxStatus.PerformLayout();
      this.tabPageAdvanced.ResumeLayout(false);
      this.groupBoxAdvanced.ResumeLayout(false);
      this.groupBoxAdvanced.PerformLayout();
      this.flowLayoutPanelShutdownMode.ResumeLayout(false);
      this.flowLayoutPanelShutdownMode.PerformLayout();
      this.tabPageReboot.ResumeLayout(false);
      this.groupBoxReboot.ResumeLayout(false);
      this.groupBoxReboot.PerformLayout();
      this.flowLayoutPanelReboot.ResumeLayout(false);
      this.flowLayoutPanelReboot.PerformLayout();
      this.panelReboot.ResumeLayout(false);
      this.panelReboot.PerformLayout();
      this.tabPageNetwork.ResumeLayout(false);
      this.groupBoxNetwork.ResumeLayout(false);
      this.groupBoxNetwork.PerformLayout();
      this.flowLayoutPanelNetworkIdleLimit.ResumeLayout(false);
      this.flowLayoutPanelNetworkIdleLimit.PerformLayout();
      this.tabPageShares.ResumeLayout(false);
      this.groupBoxShares.ResumeLayout(false);
      this.groupBoxShares.PerformLayout();
      this.tabPageProcesses.ResumeLayout(false);
      this.tabPageEPG.ResumeLayout(false);
      this.tabPageGeneral.ResumeLayout(false);
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      this.flowLayoutPanelGeneral.ResumeLayout(false);
      this.flowLayoutPanelGeneral.PerformLayout();
      this.flowLayoutPanelIdleTimeout.ResumeLayout(false);
      this.flowLayoutPanelIdleTimeout.PerformLayout();
      this.tabControl.ResumeLayout(false);
      this.tabPageClient.ResumeLayout(false);
      this.groupBoxClient.ResumeLayout(false);
      this.groupBoxClient.PerformLayout();
      this.tabPageLegacy.ResumeLayout(false);
      this.groupBoxLegacy.ResumeLayout(false);
      this.groupBoxLegacy.PerformLayout();
      this.flowLayoutPanelStandbyHours.ResumeLayout(false);
      this.flowLayoutPanelStandbyHours.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStandbyHoursFrom)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStandbyHoursTo)).EndInit();
      this.flowLayoutPanelPreNoStandbyTime.ResumeLayout(false);
      this.flowLayoutPanelPreNoStandbyTime.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreNoStandbyTime)).EndInit();
      this.flowLayoutPanelPreWakeupTime.ResumeLayout(false);
      this.flowLayoutPanelPreWakeupTime.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPreWakeupTime)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private System.Windows.Forms.Button buttonApply;
    private System.Windows.Forms.GroupBox groupBoxStatus;
    private System.Windows.Forms.TextBox textBoxStandbyHandler;
    private System.Windows.Forms.Label labelStandbyHandler;
    private System.Windows.Forms.Label labelWakeupHandler;
    private System.Windows.Forms.Label labelWakeupTimeValue;
    private System.Windows.Forms.Label labelWakeupTime;
    private System.Windows.Forms.TabPage tabPageAdvanced;
    private System.Windows.Forms.GroupBox groupBoxAdvanced;
    private System.Windows.Forms.Button buttonCommand;
    private System.Windows.Forms.TextBox textBoxCommand;
    private System.Windows.Forms.Label labelCommand;
    private System.Windows.Forms.CheckBox checkBoxReinitializeController;
    private System.Windows.Forms.TabPage tabPageReboot;
    private System.Windows.Forms.GroupBox groupBoxReboot;
    private System.Windows.Forms.Label labelReboot1;
    private System.Windows.Forms.Label labelReboot2;
    private System.Windows.Forms.Button buttonRebootCommand;
    private System.Windows.Forms.TextBox textBoxRebootCommand;
    private System.Windows.Forms.Label labelRebootCommand;
    private System.Windows.Forms.Panel panelReboot;
    private System.Windows.Forms.CheckBox checkBoxRebootSunday;
    private System.Windows.Forms.CheckBox checkBoxRebootSaturday;
    private System.Windows.Forms.CheckBox checkBoxRebootFriday;
    private System.Windows.Forms.CheckBox checkBoxRebootThursday;
    private System.Windows.Forms.CheckBox checkBoxRebootWednesday;
    private System.Windows.Forms.CheckBox checkBoxRebootTuesday;
    private System.Windows.Forms.CheckBox checkBoxRebootMonday;
    private System.Windows.Forms.MaskedTextBox textBoxReboot;
    private System.Windows.Forms.CheckBox checkBoxRebootWakeup;
    private System.Windows.Forms.TabPage tabPageNetwork;
    private System.Windows.Forms.GroupBox groupBoxNetwork;
    private System.Windows.Forms.CheckBox checkBoxNetworkAwayMode;
    private System.Windows.Forms.Label labelNetwork;
    private System.Windows.Forms.NumericUpDown numericUpDownNetworkIdleLimit;
    private System.Windows.Forms.CheckBox checkBoxNetworkEnabled;
    private System.Windows.Forms.TabPage tabPageShares;
    private System.Windows.Forms.GroupBox groupBoxShares;
    private System.Windows.Forms.Button buttonSelectShare;
    private System.Windows.Forms.CheckBox checkBoxSharesAwayMode;
    private System.Windows.Forms.Label labelShares;
    private System.Windows.Forms.DataGridView dataGridShares;
    private System.Windows.Forms.DataGridViewTextBoxColumn Sharename;
    private System.Windows.Forms.DataGridViewTextBoxColumn Hostname;
    private System.Windows.Forms.DataGridViewTextBoxColumn Username;
    private System.Windows.Forms.CheckBox checkBoxSharesEnabled;
    private System.Windows.Forms.TabPage tabPageProcesses;
    private System.Windows.Forms.GroupBox groupBoxProcesses;
    private System.Windows.Forms.CheckBox checkBoxMPClientRunning;
    private System.Windows.Forms.CheckBox checkBoxProcessesAwayMode;
    private System.Windows.Forms.Button buttonSelectProcess;
    private System.Windows.Forms.TextBox textBoxProcesses;
    private System.Windows.Forms.TabPage tabPageEPG;
    private System.Windows.Forms.GroupBox groupBoxEPG;
    private System.Windows.Forms.CheckBox checkBoxEPGAwayMode;
    private System.Windows.Forms.Label labelEPG1;
    private System.Windows.Forms.Label labelEPG2;
    private System.Windows.Forms.Button buttonEPGCommand;
    private System.Windows.Forms.TextBox textBoxEPGCommand;
    private System.Windows.Forms.Label labelEPGCommand;
    private System.Windows.Forms.Panel panelEPG;
    private System.Windows.Forms.CheckBox checkBoxEPGSunday;
    private System.Windows.Forms.CheckBox checkBoxEPGSaturday;
    private System.Windows.Forms.CheckBox checkBoxEPGFriday;
    private System.Windows.Forms.CheckBox checkBoxEPGThursday;
    private System.Windows.Forms.CheckBox checkBoxEPGWednesday;
    private System.Windows.Forms.CheckBox checkBoxEPGTuesday;
    private System.Windows.Forms.CheckBox checkBoxEPGMonday;
    private System.Windows.Forms.MaskedTextBox textBoxEPG;
    private System.Windows.Forms.CheckBox checkBoxEPGWakeup;
    private System.Windows.Forms.CheckBox checkBoxEPGPreventStandby;
    private System.Windows.Forms.TabPage tabPageGeneral;
    private System.Windows.Forms.GroupBox groupBoxGeneral;
    private System.Windows.Forms.TextBox textBoxProfile;
    private System.Windows.Forms.ComboBox comboBoxProfile;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.CheckBox checkBoxAutoPowerSettings;
    private System.Windows.Forms.Button buttonPowerSettings;
    private System.Windows.Forms.Button buttonExpertMode;
    private System.Windows.Forms.Label labelIdleTimeout1;
    private System.Windows.Forms.NumericUpDown numericUpDownIdleTimeout;
    private System.Windows.Forms.Label labelIdleTimeout2;
    private System.Windows.Forms.Label labelShutdownMode;
    private System.Windows.Forms.ComboBox comboBoxShutdownMode;
    private System.Windows.Forms.CheckBox checkBoxShutdownEnabled;
    private System.Windows.Forms.Label labelStandbyStatus;
    private System.Windows.Forms.Label labelExpertMode;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelEPG;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelShutdownMode;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelReboot;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelNetworkIdleLimit;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelIdleTimeout;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelGeneral;
    private System.Windows.Forms.TabPage tabPageClient;
    private System.Windows.Forms.GroupBox groupBoxClient;
    private System.Windows.Forms.CheckBox checkBoxHomeOnly;
    private System.Windows.Forms.CheckBox checkBoxUmuteMasterVolume;
    private System.Windows.Forms.TabPage tabPageLegacy;
    private System.Windows.Forms.GroupBox groupBoxLegacy;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelStandbyHours;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.NumericUpDown numericUpDownStandbyHoursFrom;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPreNoStandbyTime;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numericUpDownPreNoStandbyTime;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPreWakeupTime;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown numericUpDownPreWakeupTime;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.NumericUpDown numericUpDownStandbyHoursTo;
    private System.Windows.Forms.Label label7;

  }
}