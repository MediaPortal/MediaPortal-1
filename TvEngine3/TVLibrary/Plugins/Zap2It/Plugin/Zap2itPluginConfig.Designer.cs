namespace ProcessPlugins.EpgGrabber
{
    partial class Zap2itPluginConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.LinkLabel linkZap2it;
            System.Windows.Forms.Label mpLabel3;
            System.Windows.Forms.Label mpLabel1;
            System.Windows.Forms.Label mpLabel2;
            System.Windows.Forms.Label mpLabel4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Zap2itPluginConfig ) );
            System.Windows.Forms.Label mpLabel5;
            System.Windows.Forms.GroupBox groupBoxImportOptions;
            System.Windows.Forms.GroupBox groupBoxLogon;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox mpGroupBoxChannelNaming;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ColumnHeader colHdrChannelName;
            System.Windows.Forms.ColumnHeader colHdrChannelNumber;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.GroupBox groupBox4;
            this.checkBoxNotification = new System.Windows.Forms.CheckBox();
            this.numericUpDownDays = new System.Windows.Forms.NumericUpDown();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.comboBoxExternalInputCountry = new System.Windows.Forms.ComboBox();
            this.checkBoxAddChannels = new System.Windows.Forms.CheckBox();
            this.comboBoxExternalInput = new System.Windows.Forms.ComboBox();
            this.comboBoxNameFormat = new System.Windows.Forms.ComboBox();
            this.checkBoxRenameChannels = new System.Windows.Forms.CheckBox();
            this.checkboxForceUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowChannelNumberOnlyMapping = new System.Windows.Forms.CheckBox();
            this.checkBoxDeleteChannelsWithNoEPGMapping = new System.Windows.Forms.CheckBox();
            this.checkBoxSortChannelsByChannelNumber = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider( this.components );
            this.toolTip = new System.Windows.Forms.ToolTip( this.components );
            this.pbZap2itLogo = new System.Windows.Forms.PictureBox();
            this.mpTabControl = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.tabPageLineupManager = new System.Windows.Forms.TabPage();
            this.toolStripLineupManager = new System.Windows.Forms.ToolStrip();
            this.toolStripLineups = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripBtnRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnLoad = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnSave = new System.Windows.Forms.ToolStripButton();
            this.channelListView = new System.Windows.Forms.ListView();
            this.contextMenuStripLineUp = new System.Windows.Forms.ContextMenuStrip( this.components );
            this.toolStripMenuItemChannelName = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEnabled = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemListView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemIconView = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListIcons = new System.Windows.Forms.ImageList( this.components );
            this.bgLineupGrabber = new System.ComponentModel.BackgroundWorker();
            this.bgChannelGrabber = new System.ComponentModel.BackgroundWorker();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.bgLineupSaver = new System.ComponentModel.BackgroundWorker();
            this.checkBoxAppendMeta = new System.Windows.Forms.CheckBox();
            linkZap2it = new System.Windows.Forms.LinkLabel();
            mpLabel3 = new System.Windows.Forms.Label();
            mpLabel1 = new System.Windows.Forms.Label();
            mpLabel2 = new System.Windows.Forms.Label();
            mpLabel4 = new System.Windows.Forms.Label();
            mpLabel5 = new System.Windows.Forms.Label();
            groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            groupBoxLogon = new System.Windows.Forms.GroupBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            mpGroupBoxChannelNaming = new System.Windows.Forms.GroupBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            colHdrChannelName = new System.Windows.Forms.ColumnHeader();
            colHdrChannelNumber = new System.Windows.Forms.ColumnHeader();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBoxImportOptions.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.numericUpDownDays ) ).BeginInit();
            groupBoxLogon.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            mpGroupBoxChannelNaming.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.errorProvider ) ).BeginInit();
            ( (System.ComponentModel.ISupportInitialize)( this.pbZap2itLogo ) ).BeginInit();
            this.mpTabControl.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.tabPageLineupManager.SuspendLayout();
            this.toolStripLineupManager.SuspendLayout();
            this.contextMenuStripLineUp.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkZap2it
            // 
            linkZap2it.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            linkZap2it.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            linkZap2it.LinkArea = new System.Windows.Forms.LinkArea( 48, 11 );
            linkZap2it.Location = new System.Drawing.Point( 6, 89 );
            linkZap2it.Name = "linkZap2it";
            linkZap2it.Size = new System.Drawing.Size( 337, 34 );
            linkZap2it.TabIndex = 5;
            linkZap2it.TabStop = true;
            linkZap2it.Text = "To get your member login information, signup at Zap2it Labs using certificate cod" +
                "e MEPO-ZE5N-CBGB";
            linkZap2it.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            linkZap2it.UseCompatibleTextRendering = true;
            // 
            // mpLabel3
            // 
            mpLabel3.AutoSize = true;
            mpLabel3.Location = new System.Drawing.Point( 34, 50 );
            mpLabel3.Name = "mpLabel3";
            mpLabel3.Size = new System.Drawing.Size( 188, 13 );
            mpLabel3.TabIndex = 6;
            mpLabel3.Text = "&Number of days of guide data to keep:";
            this.toolTip.SetToolTip( mpLabel3, "You can specify between 1 and 13 days of program\r\nguide data to download and keep" +
                    " in MediaPortal.\r\n" );
            // 
            // mpLabel1
            // 
            mpLabel1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            mpLabel1.AutoSize = true;
            mpLabel1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            mpLabel1.Location = new System.Drawing.Point( 3, 6 );
            mpLabel1.Name = "mpLabel1";
            mpLabel1.Size = new System.Drawing.Size( 63, 13 );
            mpLabel1.TabIndex = 2;
            mpLabel1.Text = "&Username";
            this.toolTip.SetToolTip( mpLabel1, "Your username is between 6 and 25 characters\r\nand must match your name on Zap2it " +
                    "exactly." );
            // 
            // mpLabel2
            // 
            mpLabel2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            mpLabel2.AutoSize = true;
            mpLabel2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            mpLabel2.Location = new System.Drawing.Point( 5, 37 );
            mpLabel2.Name = "mpLabel2";
            mpLabel2.Size = new System.Drawing.Size( 61, 13 );
            mpLabel2.TabIndex = 3;
            mpLabel2.Text = "&Password";
            this.toolTip.SetToolTip( mpLabel2, "Your password is between 6 and 25 characters and must\r\nmatch your password on Zap" +
                    "2it exactly. It is stored encrypted.\r\n" );
            // 
            // mpLabel4
            // 
            mpLabel4.AutoSize = true;
            mpLabel4.Location = new System.Drawing.Point( 31, 19 );
            mpLabel4.Name = "mpLabel4";
            mpLabel4.Size = new System.Drawing.Size( 110, 13 );
            mpLabel4.TabIndex = 19;
            mpLabel4.Text = "&Channel name format:";
            this.toolTip.SetToolTip( mpLabel4, resources.GetString( "mpLabel4.ToolTip" ) );
            // 
            // mpLabel5
            // 
            mpLabel5.AutoSize = true;
            mpLabel5.Location = new System.Drawing.Point( 7, 43 );
            mpLabel5.Name = "mpLabel5";
            mpLabel5.Size = new System.Drawing.Size( 189, 13 );
            mpLabel5.TabIndex = 23;
            mpLabel5.Text = "&External Video Input for new channels:";
            this.toolTip.SetToolTip( mpLabel5, "Only used for external tuners when \r\nadding channels automatically." );
            // 
            // groupBoxImportOptions
            // 
            groupBoxImportOptions.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            groupBoxImportOptions.Controls.Add( this.checkBoxAppendMeta );
            groupBoxImportOptions.Controls.Add( this.checkBoxNotification );
            groupBoxImportOptions.Controls.Add( this.numericUpDownDays );
            groupBoxImportOptions.Controls.Add( mpLabel3 );
            groupBoxImportOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            groupBoxImportOptions.Location = new System.Drawing.Point( 6, 146 );
            groupBoxImportOptions.Name = "groupBoxImportOptions";
            groupBoxImportOptions.Size = new System.Drawing.Size( 346, 113 );
            groupBoxImportOptions.TabIndex = 3;
            groupBoxImportOptions.TabStop = false;
            groupBoxImportOptions.Text = "EPG Import Options";
            // 
            // checkBoxNotification
            // 
            this.checkBoxNotification.AutoSize = true;
            this.checkBoxNotification.Checked = true;
            this.checkBoxNotification.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNotification.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxNotification.Location = new System.Drawing.Point( 19, 24 );
            this.checkBoxNotification.Name = "checkBoxNotification";
            this.checkBoxNotification.Size = new System.Drawing.Size( 264, 17 );
            this.checkBoxNotification.TabIndex = 10;
            this.checkBoxNotification.Text = "&Display notification on completion of import process";
            this.toolTip.SetToolTip( this.checkBoxNotification, "If checked you will see a notification box \r\ndisplayed when the import completes." +
                    "" );
            this.checkBoxNotification.UseVisualStyleBackColor = true;
            // 
            // numericUpDownDays
            // 
            this.numericUpDownDays.Location = new System.Drawing.Point( 227, 46 );
            this.numericUpDownDays.Maximum = new decimal( new int[] {
            13,
            0,
            0,
            0} );
            this.numericUpDownDays.Minimum = new decimal( new int[] {
            1,
            0,
            0,
            0} );
            this.numericUpDownDays.Name = "numericUpDownDays";
            this.numericUpDownDays.Size = new System.Drawing.Size( 49, 20 );
            this.numericUpDownDays.TabIndex = 0;
            this.numericUpDownDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip( this.numericUpDownDays, "You can specify between 1 and 13 days of program\r\nguide data to download and keep" +
                    " in MediaPortal." );
            this.numericUpDownDays.Value = new decimal( new int[] {
            5,
            0,
            0,
            0} );
            // 
            // groupBoxLogon
            // 
            groupBoxLogon.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            groupBoxLogon.Controls.Add( linkZap2it );
            groupBoxLogon.Controls.Add( tableLayoutPanel1 );
            groupBoxLogon.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            groupBoxLogon.Location = new System.Drawing.Point( 3, 11 );
            groupBoxLogon.Name = "groupBoxLogon";
            groupBoxLogon.Size = new System.Drawing.Size( 349, 130 );
            groupBoxLogon.TabIndex = 2;
            groupBoxLogon.TabStop = false;
            groupBoxLogon.Text = "Zap2it Labs Member Login";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle() );
            tableLayoutPanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle() );
            tableLayoutPanel1.Controls.Add( this.textBoxUsername, 1, 0 );
            tableLayoutPanel1.Controls.Add( this.textBoxPassword, 1, 1 );
            tableLayoutPanel1.Controls.Add( mpLabel1, 0, 0 );
            tableLayoutPanel1.Controls.Add( mpLabel2, 0, 1 );
            tableLayoutPanel1.Location = new System.Drawing.Point( 31, 19 );
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add( new System.Windows.Forms.RowStyle() );
            tableLayoutPanel1.RowStyles.Add( new System.Windows.Forms.RowStyle() );
            tableLayoutPanel1.Size = new System.Drawing.Size( 291, 62 );
            tableLayoutPanel1.TabIndex = 0;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.errorProvider.SetIconPadding( this.textBoxUsername, 5 );
            this.textBoxUsername.Location = new System.Drawing.Point( 72, 3 );
            this.textBoxUsername.MaxLength = 25;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size( 184, 20 );
            this.textBoxUsername.TabIndex = 1;
            this.toolTip.SetToolTip( this.textBoxUsername, "Your username is between 6 and 25 characters\r\nand must match your name on Zap2it " +
                    "exactly." );
            this.textBoxUsername.Validating += new System.ComponentModel.CancelEventHandler( this.textBoxUsername_Validating );
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.errorProvider.SetIconPadding( this.textBoxPassword, 5 );
            this.textBoxPassword.Location = new System.Drawing.Point( 72, 34 );
            this.textBoxPassword.MaxLength = 25;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size( 184, 20 );
            this.textBoxPassword.TabIndex = 2;
            this.toolTip.SetToolTip( this.textBoxPassword, "Your password is between 6 and 25 characters and must\r\nmatch your password on Zap" +
                    "2it exactly. It is stored encrypted." );
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.Validating += new System.ComponentModel.CancelEventHandler( this.textBoxPassword_Validating );
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            groupBox1.Controls.Add( this.comboBoxExternalInputCountry );
            groupBox1.Controls.Add( label1 );
            groupBox1.Controls.Add( this.checkBoxAddChannels );
            groupBox1.Controls.Add( this.comboBoxExternalInput );
            groupBox1.Controls.Add( mpLabel5 );
            groupBox1.Location = new System.Drawing.Point( 6, 78 );
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size( 346, 93 );
            groupBox1.TabIndex = 19;
            groupBox1.TabStop = false;
            groupBox1.Text = "Digital Cable/Satellite Channel Management";
            // 
            // comboBoxExternalInputCountry
            // 
            this.comboBoxExternalInputCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxExternalInputCountry.Location = new System.Drawing.Point( 201, 66 );
            this.comboBoxExternalInputCountry.Name = "comboBoxExternalInputCountry";
            this.comboBoxExternalInputCountry.Size = new System.Drawing.Size( 139, 21 );
            this.comboBoxExternalInputCountry.TabIndex = 25;
            this.toolTip.SetToolTip( this.comboBoxExternalInputCountry, "Only used for external tuners when \r\nadding channels automatically." );
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point( 7, 69 );
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size( 177, 13 );
            label1.TabIndex = 26;
            label1.Text = "Coun&try Selection for new channels:";
            this.toolTip.SetToolTip( label1, "Only used for external tuners when \r\nadding channels automatically." );
            // 
            // checkBoxAddChannels
            // 
            this.checkBoxAddChannels.AutoSize = true;
            this.checkBoxAddChannels.Checked = true;
            this.checkBoxAddChannels.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxAddChannels.Location = new System.Drawing.Point( 15, 18 );
            this.checkBoxAddChannels.Name = "checkBoxAddChannels";
            this.checkBoxAddChannels.Size = new System.Drawing.Size( 308, 17 );
            this.checkBoxAddChannels.TabIndex = 26;
            this.checkBoxAddChannels.Text = "&Automatically add new Zap2it digital cable/satellite channels";
            this.toolTip.SetToolTip( this.checkBoxAddChannels, "If this is checked then when new digital cable and satellite channels are added t" +
                    "o \r\nZap2it they will be added to MediaPortal." );
            this.checkBoxAddChannels.UseVisualStyleBackColor = true;
            this.checkBoxAddChannels.CheckedChanged += new System.EventHandler( this.checkBoxAddChannels_CheckedChanged );
            // 
            // comboBoxExternalInput
            // 
            this.comboBoxExternalInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxExternalInput.Location = new System.Drawing.Point( 201, 40 );
            this.comboBoxExternalInput.Name = "comboBoxExternalInput";
            this.comboBoxExternalInput.Size = new System.Drawing.Size( 139, 21 );
            this.comboBoxExternalInput.TabIndex = 24;
            this.toolTip.SetToolTip( this.comboBoxExternalInput, "Only used for external tuners when \r\nadding channels automatically." );
            // 
            // mpGroupBoxChannelNaming
            // 
            mpGroupBoxChannelNaming.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            mpGroupBoxChannelNaming.Controls.Add( this.comboBoxNameFormat );
            mpGroupBoxChannelNaming.Controls.Add( mpLabel4 );
            mpGroupBoxChannelNaming.Controls.Add( this.checkBoxRenameChannels );
            mpGroupBoxChannelNaming.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            mpGroupBoxChannelNaming.Location = new System.Drawing.Point( 6, 7 );
            mpGroupBoxChannelNaming.Name = "mpGroupBoxChannelNaming";
            mpGroupBoxChannelNaming.Size = new System.Drawing.Size( 346, 65 );
            mpGroupBoxChannelNaming.TabIndex = 18;
            mpGroupBoxChannelNaming.TabStop = false;
            mpGroupBoxChannelNaming.Text = "Channel Naming";
            // 
            // comboBoxNameFormat
            // 
            this.comboBoxNameFormat.FormattingEnabled = true;
            this.comboBoxNameFormat.Items.AddRange( new object[] {
            "{number} {callsign}",
            "{number} {name}",
            "{name} {affiliate}"} );
            this.comboBoxNameFormat.Location = new System.Drawing.Point( 147, 15 );
            this.comboBoxNameFormat.Name = "comboBoxNameFormat";
            this.comboBoxNameFormat.Size = new System.Drawing.Size( 145, 21 );
            this.comboBoxNameFormat.TabIndex = 20;
            this.comboBoxNameFormat.Text = "{number} {callsign}";
            this.toolTip.SetToolTip( this.comboBoxNameFormat, resources.GetString( "comboBoxNameFormat.ToolTip" ) );
            this.comboBoxNameFormat.Validating += new System.ComponentModel.CancelEventHandler( this.comboBoxNameFormat_Validating );
            // 
            // checkBoxRenameChannels
            // 
            this.checkBoxRenameChannels.AutoSize = true;
            this.checkBoxRenameChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxRenameChannels.Location = new System.Drawing.Point( 15, 41 );
            this.checkBoxRenameChannels.Name = "checkBoxRenameChannels";
            this.checkBoxRenameChannels.Size = new System.Drawing.Size( 293, 17 );
            this.checkBoxRenameChannels.TabIndex = 18;
            this.checkBoxRenameChannels.Text = "&Rename existing channels and logos to new name format";
            this.toolTip.SetToolTip( this.checkBoxRenameChannels, "If checked, your existing channels will be renamed according\r\nto the format speci" +
                    "fied below each time the import runs." );
            this.checkBoxRenameChannels.UseVisualStyleBackColor = true;
            this.checkBoxRenameChannels.CheckedChanged += new System.EventHandler( this.checkBoxAddChannels_CheckedChanged );
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size( 6, 25 );
            // 
            // colHdrChannelName
            // 
            colHdrChannelName.DisplayIndex = 1;
            colHdrChannelName.Text = "Name";
            colHdrChannelName.Width = 250;
            // 
            // colHdrChannelNumber
            // 
            colHdrChannelNumber.DisplayIndex = 0;
            colHdrChannelNumber.Text = "Channel";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size( 131, 6 );
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size( 131, 6 );
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add( this.checkboxForceUpdate );
            groupBox2.Location = new System.Drawing.Point( 6, 173 );
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size( 346, 40 );
            groupBox2.TabIndex = 20;
            groupBox2.TabStop = false;
            groupBox2.Text = "Scheduler Options";
            // 
            // checkboxForceUpdate
            // 
            this.checkboxForceUpdate.AutoSize = true;
            this.checkboxForceUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkboxForceUpdate.Location = new System.Drawing.Point( 15, 16 );
            this.checkboxForceUpdate.Name = "checkboxForceUpdate";
            this.checkboxForceUpdate.Size = new System.Drawing.Size( 237, 17 );
            this.checkboxForceUpdate.TabIndex = 27;
            this.checkboxForceUpdate.Text = "&Force guide update on next TVServer startup";
            this.toolTip.SetToolTip( this.checkboxForceUpdate, "If this is checked then a guide update will be\r\nforced when TVServer next starts." +
                    " \r\n*Requires a stop and start of the TVServer Service." );
            this.checkboxForceUpdate.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add( this.checkBoxAllowChannelNumberOnlyMapping );
            groupBox3.Location = new System.Drawing.Point( 6, 216 );
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size( 346, 40 );
            groupBox3.TabIndex = 21;
            groupBox3.TabStop = false;
            groupBox3.Text = "Mapping Options";
            // 
            // checkBoxAllowChannelNumberOnlyMapping
            // 
            this.checkBoxAllowChannelNumberOnlyMapping.AutoSize = true;
            this.checkBoxAllowChannelNumberOnlyMapping.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxAllowChannelNumberOnlyMapping.Location = new System.Drawing.Point( 15, 14 );
            this.checkBoxAllowChannelNumberOnlyMapping.Name = "checkBoxAllowChannelNumberOnlyMapping";
            this.checkBoxAllowChannelNumberOnlyMapping.Size = new System.Drawing.Size( 280, 17 );
            this.checkBoxAllowChannelNumberOnlyMapping.TabIndex = 28;
            this.checkBoxAllowChannelNumberOnlyMapping.Text = "Allo&w EPG Channel Mapping by Channel Number Only";
            this.toolTip.SetToolTip( this.checkBoxAllowChannelNumberOnlyMapping, "If this is checked then a last chance mapping by channel number \r\nwill be attempt" +
                    "ed even with a missing frequency.\r\nCaution: May cause unpredictable results!" );
            this.checkBoxAllowChannelNumberOnlyMapping.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add( this.checkBoxDeleteChannelsWithNoEPGMapping );
            groupBox4.Controls.Add( this.checkBoxSortChannelsByChannelNumber );
            groupBox4.Location = new System.Drawing.Point( 6, 260 );
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size( 346, 60 );
            groupBox4.TabIndex = 22;
            groupBox4.TabStop = false;
            groupBox4.Text = "Additional Channel Options";
            // 
            // checkBoxDeleteChannelsWithNoEPGMapping
            // 
            this.checkBoxDeleteChannelsWithNoEPGMapping.AutoSize = true;
            this.checkBoxDeleteChannelsWithNoEPGMapping.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxDeleteChannelsWithNoEPGMapping.Location = new System.Drawing.Point( 15, 37 );
            this.checkBoxDeleteChannelsWithNoEPGMapping.Name = "checkBoxDeleteChannelsWithNoEPGMapping";
            this.checkBoxDeleteChannelsWithNoEPGMapping.Size = new System.Drawing.Size( 261, 17 );
            this.checkBoxDeleteChannelsWithNoEPGMapping.TabIndex = 30;
            this.checkBoxDeleteChannelsWithNoEPGMapping.Text = "&Delete any channel without external EPG mapping";
            this.toolTip.SetToolTip( this.checkBoxDeleteChannelsWithNoEPGMapping, "If this is checked then any channel without external \r\nEPG mapping present will b" +
                    "e deleted. *Use with caution." );
            this.checkBoxDeleteChannelsWithNoEPGMapping.UseVisualStyleBackColor = true;
            // 
            // checkBoxSortChannelsByChannelNumber
            // 
            this.checkBoxSortChannelsByChannelNumber.AutoSize = true;
            this.checkBoxSortChannelsByChannelNumber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxSortChannelsByChannelNumber.Location = new System.Drawing.Point( 15, 16 );
            this.checkBoxSortChannelsByChannelNumber.Name = "checkBoxSortChannelsByChannelNumber";
            this.checkBoxSortChannelsByChannelNumber.Size = new System.Drawing.Size( 182, 17 );
            this.checkBoxSortChannelsByChannelNumber.TabIndex = 29;
            this.checkBoxSortChannelsByChannelNumber.Text = "&Sort channels by channel number";
            this.toolTip.SetToolTip( this.checkBoxSortChannelsByChannelNumber, "If this is checked then an attempt will be made to sort \r\nthe entre channel list " +
                    "by channel number." );
            this.checkBoxSortChannelsByChannelNumber.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point( 304, 395 );
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size( 75, 23 );
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Visible = false;
            // 
            // okButton
            // 
            this.okButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point( 224, 395 );
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size( 75, 23 );
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Visible = false;
            this.okButton.Click += new System.EventHandler( this.okButton_Click );
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkRate = 500;
            this.errorProvider.ContainerControl = this;
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 250;
            this.toolTip.AutoPopDelay = 15000;
            this.toolTip.InitialDelay = 250;
            this.toolTip.IsBalloon = true;
            this.toolTip.ReshowDelay = 50;
            // 
            // pbZap2itLogo
            // 
            this.pbZap2itLogo.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.pbZap2itLogo.Image = global::ProcessPlugins.EpgGrabber.Properties.Resources.zap2itlogo_45x40;
            this.pbZap2itLogo.Location = new System.Drawing.Point( 12, 379 );
            this.pbZap2itLogo.Name = "pbZap2itLogo";
            this.pbZap2itLogo.Size = new System.Drawing.Size( 45, 40 );
            this.pbZap2itLogo.TabIndex = 4;
            this.pbZap2itLogo.TabStop = false;
            this.toolTip.SetToolTip( this.pbZap2itLogo, "Data provided by Zap2it Labs" );
            this.pbZap2itLogo.Click += new System.EventHandler( this.pbZap2itLogo_Click );
            // 
            // mpTabControl
            // 
            this.mpTabControl.Controls.Add( this.tabPageGeneral );
            this.mpTabControl.Controls.Add( this.tabPageAdvanced );
            this.mpTabControl.Controls.Add( this.tabPageLineupManager );
            this.mpTabControl.Location = new System.Drawing.Point( 13, 13 );
            this.mpTabControl.Name = "mpTabControl";
            this.mpTabControl.SelectedIndex = 0;
            this.mpTabControl.Size = new System.Drawing.Size( 366, 356 );
            this.mpTabControl.TabIndex = 5;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add( groupBoxImportOptions );
            this.tabPageGeneral.Controls.Add( groupBoxLogon );
            this.tabPageGeneral.Location = new System.Drawing.Point( 4, 22 );
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPageGeneral.Size = new System.Drawing.Size( 358, 330 );
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "Configuration";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add( groupBox4 );
            this.tabPageAdvanced.Controls.Add( groupBox3 );
            this.tabPageAdvanced.Controls.Add( groupBox2 );
            this.tabPageAdvanced.Controls.Add( groupBox1 );
            this.tabPageAdvanced.Controls.Add( mpGroupBoxChannelNaming );
            this.tabPageAdvanced.Location = new System.Drawing.Point( 4, 22 );
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPageAdvanced.Size = new System.Drawing.Size( 358, 330 );
            this.tabPageAdvanced.TabIndex = 1;
            this.tabPageAdvanced.Text = "Advanced Options";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // tabPageLineupManager
            // 
            this.tabPageLineupManager.Controls.Add( this.toolStripLineupManager );
            this.tabPageLineupManager.Controls.Add( this.channelListView );
            this.tabPageLineupManager.Location = new System.Drawing.Point( 4, 22 );
            this.tabPageLineupManager.Name = "tabPageLineupManager";
            this.tabPageLineupManager.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPageLineupManager.Size = new System.Drawing.Size( 358, 330 );
            this.tabPageLineupManager.TabIndex = 2;
            this.tabPageLineupManager.Text = "Zap2it Lineups";
            this.tabPageLineupManager.UseVisualStyleBackColor = true;
            this.tabPageLineupManager.Enter += new System.EventHandler( this.tabPageLineupManager_Enter );
            // 
            // toolStripLineupManager
            // 
            this.toolStripLineupManager.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripLineupManager.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLineups,
            this.toolStripBtnRefresh,
            this.toolStripBtnLoad,
            toolStripSeparator1,
            this.toolStripBtnSave} );
            this.toolStripLineupManager.Location = new System.Drawing.Point( 3, 3 );
            this.toolStripLineupManager.Name = "toolStripLineupManager";
            this.toolStripLineupManager.Size = new System.Drawing.Size( 352, 25 );
            this.toolStripLineupManager.TabIndex = 3;
            this.toolStripLineupManager.Text = "Lineup Manager Tools";
            // 
            // toolStripLineups
            // 
            this.toolStripLineups.BackColor = System.Drawing.Color.GhostWhite;
            this.toolStripLineups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripLineups.Items.AddRange( new object[] {
            "Click the refresh button ..."} );
            this.toolStripLineups.Margin = new System.Windows.Forms.Padding( 5, 2, 1, 2 );
            this.toolStripLineups.Name = "toolStripLineups";
            this.toolStripLineups.Size = new System.Drawing.Size( 250, 21 );
            this.toolStripLineups.SelectedIndexChanged += new System.EventHandler( this.toolStripLineups_SelectedIndexChanged );
            // 
            // toolStripBtnRefresh
            // 
            this.toolStripBtnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBtnRefresh.Image = ( (System.Drawing.Image)( resources.GetObject( "toolStripBtnRefresh.Image" ) ) );
            this.toolStripBtnRefresh.ImageTransparentColor = System.Drawing.Color.Black;
            this.toolStripBtnRefresh.Name = "toolStripBtnRefresh";
            this.toolStripBtnRefresh.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripBtnRefresh.Text = "Refresh Lineup List";
            this.toolStripBtnRefresh.ToolTipText = "Refresh lineup List from Zap2it";
            this.toolStripBtnRefresh.Click += new System.EventHandler( this.toolStripBtnRefresh_Click );
            // 
            // toolStripBtnLoad
            // 
            this.toolStripBtnLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBtnLoad.Enabled = false;
            this.toolStripBtnLoad.Image = ( (System.Drawing.Image)( resources.GetObject( "toolStripBtnLoad.Image" ) ) );
            this.toolStripBtnLoad.ImageTransparentColor = System.Drawing.Color.Black;
            this.toolStripBtnLoad.Name = "toolStripBtnLoad";
            this.toolStripBtnLoad.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripBtnLoad.Text = "Load";
            this.toolStripBtnLoad.ToolTipText = "Load lineup from Zap2it";
            this.toolStripBtnLoad.Click += new System.EventHandler( this.toolStripBtnLoad_Click );
            // 
            // toolStripBtnSave
            // 
            this.toolStripBtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripBtnSave.Enabled = false;
            this.toolStripBtnSave.Image = ( (System.Drawing.Image)( resources.GetObject( "toolStripBtnSave.Image" ) ) );
            this.toolStripBtnSave.ImageTransparentColor = System.Drawing.Color.Black;
            this.toolStripBtnSave.Name = "toolStripBtnSave";
            this.toolStripBtnSave.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripBtnSave.Text = "Save Lineup";
            this.toolStripBtnSave.ToolTipText = "Save Lineup to Zap2it";
            this.toolStripBtnSave.Click += new System.EventHandler( this.toolStripBtnSave_Click );
            // 
            // channelListView
            // 
            this.channelListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.channelListView.AllowColumnReorder = true;
            this.channelListView.AllowDrop = true;
            this.channelListView.BackColor = System.Drawing.SystemColors.Window;
            this.channelListView.CheckBoxes = true;
            this.channelListView.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            colHdrChannelName,
            colHdrChannelNumber} );
            this.channelListView.ContextMenuStrip = this.contextMenuStripLineUp;
            this.channelListView.FullRowSelect = true;
            this.channelListView.GridLines = true;
            this.channelListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.channelListView.LargeImageList = this.imageListIcons;
            this.channelListView.Location = new System.Drawing.Point( 3, 31 );
            this.channelListView.MultiSelect = false;
            this.channelListView.Name = "channelListView";
            this.channelListView.Size = new System.Drawing.Size( 352, 293 );
            this.channelListView.TabIndex = 1;
            this.channelListView.UseCompatibleStateImageBehavior = false;
            this.channelListView.View = System.Windows.Forms.View.Details;
            this.channelListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler( this.channelListView_ItemChecked );
            // 
            // contextMenuStripLineUp
            // 
            this.contextMenuStripLineUp.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemChannelName,
            toolStripSeparator3,
            this.toolStripMenuItemEnabled,
            toolStripSeparator2,
            this.toolStripMenuItemListView,
            this.toolStripMenuItemIconView} );
            this.contextMenuStripLineUp.Name = "contextMenuStripLineUp";
            this.contextMenuStripLineUp.Size = new System.Drawing.Size( 135, 104 );
            this.contextMenuStripLineUp.Opening += new System.ComponentModel.CancelEventHandler( this.contextMenuStrip_Opening );
            // 
            // toolStripMenuItemChannelName
            // 
            this.toolStripMenuItemChannelName.Font = new System.Drawing.Font( "Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            this.toolStripMenuItemChannelName.Name = "toolStripMenuItemChannelName";
            this.toolStripMenuItemChannelName.Size = new System.Drawing.Size( 134, 22 );
            this.toolStripMenuItemChannelName.Text = "CHANNEL";
            // 
            // toolStripMenuItemEnabled
            // 
            this.toolStripMenuItemEnabled.Checked = true;
            this.toolStripMenuItemEnabled.CheckOnClick = true;
            this.toolStripMenuItemEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemEnabled.Name = "toolStripMenuItemEnabled";
            this.toolStripMenuItemEnabled.Size = new System.Drawing.Size( 134, 22 );
            this.toolStripMenuItemEnabled.Text = "Enabled";
            this.toolStripMenuItemEnabled.Click += new System.EventHandler( this.toolStripMenuItemEnabled_Click );
            // 
            // toolStripMenuItemListView
            // 
            this.toolStripMenuItemListView.Image = ( (System.Drawing.Image)( resources.GetObject( "toolStripMenuItemListView.Image" ) ) );
            this.toolStripMenuItemListView.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.toolStripMenuItemListView.Name = "toolStripMenuItemListView";
            this.toolStripMenuItemListView.Size = new System.Drawing.Size( 134, 22 );
            this.toolStripMenuItemListView.Text = "List View";
            this.toolStripMenuItemListView.Click += new System.EventHandler( this.toolStripMenuItemListView_Click );
            // 
            // toolStripMenuItemIconView
            // 
            this.toolStripMenuItemIconView.Image = ( (System.Drawing.Image)( resources.GetObject( "toolStripMenuItemIconView.Image" ) ) );
            this.toolStripMenuItemIconView.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.toolStripMenuItemIconView.Name = "toolStripMenuItemIconView";
            this.toolStripMenuItemIconView.Size = new System.Drawing.Size( 134, 22 );
            this.toolStripMenuItemIconView.Text = "Icon View";
            this.toolStripMenuItemIconView.Click += new System.EventHandler( this.toolStripMenuItemIconView_Click );
            // 
            // imageListIcons
            // 
            this.imageListIcons.ImageStream = ( (System.Windows.Forms.ImageListStreamer)( resources.GetObject( "imageListIcons.ImageStream" ) ) );
            this.imageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIcons.Images.SetKeyName( 0, "tvx.jpg" );
            this.imageListIcons.Images.SetKeyName( 1, "tv.jpg" );
            // 
            // bgLineupGrabber
            // 
            this.bgLineupGrabber.WorkerSupportsCancellation = true;
            this.bgLineupGrabber.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgLineupGrabber_DoWork );
            this.bgLineupGrabber.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgLineupGrabber_RunWorkerCompleted );
            // 
            // bgChannelGrabber
            // 
            this.bgChannelGrabber.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgChannelGrabber_DoWork );
            this.bgChannelGrabber.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgChannelGrabber_RunWorkerCompleted );
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.progressBar.Enabled = false;
            this.progressBar.Location = new System.Drawing.Point( 224, 375 );
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size( 154, 14 );
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // bgLineupSaver
            // 
            this.bgLineupSaver.DoWork += new System.ComponentModel.DoWorkEventHandler( this.bgLineupSaver_DoWork );
            this.bgLineupSaver.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler( this.bgLineupSaver_RunWorkerCompleted );
            // 
            // checkBoxAppendMeta
            // 
            this.checkBoxAppendMeta.AutoSize = true;
            this.checkBoxAppendMeta.Checked = true;
            this.checkBoxAppendMeta.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAppendMeta.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxAppendMeta.Location = new System.Drawing.Point( 19, 83 );
            this.checkBoxAppendMeta.Name = "checkBoxAppendMeta";
            this.checkBoxAppendMeta.Size = new System.Drawing.Size( 248, 17 );
            this.checkBoxAppendMeta.TabIndex = 11;
            this.checkBoxAppendMeta.Text = "&Append program meta information to description";
            this.toolTip.SetToolTip( this.checkBoxAppendMeta, "If checked you will see meta information about a program appended to the descript" +
                    "ion, ie. repeat, rating, advisories, etc..." );
            this.checkBoxAppendMeta.UseVisualStyleBackColor = true;
            // 
            // Zap2itPluginConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.progressBar );
            this.Controls.Add( this.mpTabControl );
            this.Controls.Add( this.pbZap2itLogo );
            this.Controls.Add( this.cancelButton );
            this.Controls.Add( this.okButton );
            this.Name = "Zap2itPluginConfig";
            this.Size = new System.Drawing.Size( 391, 428 );
            groupBoxImportOptions.ResumeLayout( false );
            groupBoxImportOptions.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.numericUpDownDays ) ).EndInit();
            groupBoxLogon.ResumeLayout( false );
            tableLayoutPanel1.ResumeLayout( false );
            tableLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout( false );
            groupBox1.PerformLayout();
            mpGroupBoxChannelNaming.ResumeLayout( false );
            mpGroupBoxChannelNaming.PerformLayout();
            groupBox2.ResumeLayout( false );
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout( false );
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout( false );
            groupBox4.PerformLayout();
            ( (System.ComponentModel.ISupportInitialize)( this.errorProvider ) ).EndInit();
            ( (System.ComponentModel.ISupportInitialize)( this.pbZap2itLogo ) ).EndInit();
            this.mpTabControl.ResumeLayout( false );
            this.tabPageGeneral.ResumeLayout( false );
            this.tabPageAdvanced.ResumeLayout( false );
            this.tabPageLineupManager.ResumeLayout( false );
            this.tabPageLineupManager.PerformLayout();
            this.toolStripLineupManager.ResumeLayout( false );
            this.toolStripLineupManager.PerformLayout();
            this.contextMenuStripLineUp.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Button  cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.PictureBox pbZap2itLogo;
        private System.Windows.Forms.TabControl  mpTabControl;
        private System.Windows.Forms.TabPage tabPageGeneral;
        public System.Windows.Forms.CheckBox checkBoxNotification;
        public System.Windows.Forms.NumericUpDown numericUpDownDays;
        public System.Windows.Forms.TextBox textBoxUsername;
        public System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        public System.Windows.Forms.CheckBox checkBoxAddChannels;
        public System.Windows.Forms.ComboBox comboBoxExternalInput;
        public System.Windows.Forms.ComboBox comboBoxNameFormat;
        public System.Windows.Forms.CheckBox checkBoxRenameChannels;
        private System.Windows.Forms.TabPage tabPageLineupManager;
        private System.Windows.Forms.ListView channelListView;
        private System.Windows.Forms.ToolStrip toolStripLineupManager;
        private System.Windows.Forms.ToolStripComboBox toolStripLineups;
        private System.Windows.Forms.ToolStripButton toolStripBtnLoad;
        private System.Windows.Forms.ToolStripButton toolStripBtnSave;
        private System.Windows.Forms.ToolStripButton toolStripBtnRefresh;
        private System.ComponentModel.BackgroundWorker bgLineupGrabber;
        private System.ComponentModel.BackgroundWorker bgChannelGrabber;
        private System.Windows.Forms.ImageList imageListIcons;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLineUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEnabled;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemIconView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemChannelName;
        public System.Windows.Forms.CheckBox checkboxForceUpdate;
        private System.Windows.Forms.ProgressBar progressBar;
       private System.ComponentModel.BackgroundWorker bgLineupSaver;
       public System.Windows.Forms.CheckBox checkBoxAllowChannelNumberOnlyMapping;
       public System.Windows.Forms.ComboBox comboBoxExternalInputCountry;
       public System.Windows.Forms.CheckBox checkBoxSortChannelsByChannelNumber;
       public System.Windows.Forms.CheckBox checkBoxDeleteChannelsWithNoEPGMapping;
        public System.Windows.Forms.CheckBox checkBoxAppendMeta;

    }
}