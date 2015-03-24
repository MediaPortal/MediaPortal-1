namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    partial class Editor
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
                if (this.testWorker != null)
                {
                    this.testWorker.Dispose();
                    this.testWorker = null;
                    this.testPlaylistRow = null;
                }

                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabPageHttp = new System.Windows.Forms.TabPage();
            this.groupBoxCommonParametersHttp = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.labelHttpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelHttpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxHttpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxHttpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxHttpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelHttpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelHttpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxHttpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageRtmp = new System.Windows.Forms.TabPage();
            this.groupBoxCommonParametersRtmp = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.labelRtmpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtmpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxRtmpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxRtmpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxRtmpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelRtmpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtmpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxRtmpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageRtsp = new System.Windows.Forms.TabPage();
            this.groupBoxCommonParametersRtsp = new System.Windows.Forms.GroupBox();
            this.labelRtspIgnoreRtpPayloadType = new System.Windows.Forms.Label();
            this.checkBoxRtspIgnoreRtpPayloadType = new System.Windows.Forms.CheckBox();
            this.labelRtspConnectionPreference = new System.Windows.Forms.Label();
            this.label59 = new System.Windows.Forms.Label();
            this.textBoxRtspClientPortMax = new System.Windows.Forms.TextBox();
            this.labelRtspConnectionRange = new System.Windows.Forms.Label();
            this.textBoxRtspClientPortMin = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.labelRtspTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtspOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxRtspTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxRtspOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxRtspOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelRtspOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelRtspNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxRtspPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.tabPageUdpRtp = new System.Windows.Forms.TabPage();
            this.groupBoxCommonParametersUdpRtp = new System.Windows.Forms.GroupBox();
            this.label56 = new System.Windows.Forms.Label();
            this.labelUdpRtpReceiveDataCheckInterval = new System.Windows.Forms.Label();
            this.textBoxUdpRtpReceiveDataCheckInterval = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.labelUdpRtpTotalReopenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelUdpRtpOpenConnectionSleepTime = new System.Windows.Forms.Label();
            this.textBoxUdpRtpTotalReopenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.textBoxUdpRtpOpenConnectionSleepTime = new System.Windows.Forms.TextBox();
            this.textBoxUdpRtpOpenConnectionTimeout = new System.Windows.Forms.TextBox();
            this.labelUdpRtpOpenConnectionTimeout = new System.Windows.Forms.Label();
            this.labelUdpRtpNetworkInterface = new System.Windows.Forms.Label();
            this.comboBoxUdpRtpPreferredNetworkInterface = new System.Windows.Forms.ComboBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabPagePlaylistEditor = new System.Windows.Forms.TabPage();
            this.splitContainerPlaylist = new System.Windows.Forms.SplitContainer();
            this.dataGridViewPlaylist = new System.Windows.Forms.DataGridView();
            this.ColumnChannelName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnProtocol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFilterUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.propertyGridPlaylist = new System.Windows.Forms.PropertyGrid();
            this.buttonLoadPlaylist = new System.Windows.Forms.Button();
            this.buttonSavePlaylist = new System.Windows.Forms.Button();
            this.tabProtocols = new System.Windows.Forms.TabControl();
            this.tabPageDatabaseEditor = new System.Windows.Forms.TabPage();
            this.splitContainerDatabase = new System.Windows.Forms.SplitContainer();
            this.dataGridViewDatabase = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewImageColumn2 = new System.Windows.Forms.DataGridViewImageColumn();
            this.propertyGridDatabase = new System.Windows.Forms.PropertyGrid();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.buttonUpdateDatabase = new System.Windows.Forms.Button();
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.buttonStoreChanges = new System.Windows.Forms.Button();
            this.rtspConnectionPreference = new TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url.RtspConnectionPreference();
            this.buttonSetMpeg2TSParser = new System.Windows.Forms.Button();
            this.tabPageHttp.SuspendLayout();
            this.groupBoxCommonParametersHttp.SuspendLayout();
            this.tabPageRtmp.SuspendLayout();
            this.groupBoxCommonParametersRtmp.SuspendLayout();
            this.tabPageRtsp.SuspendLayout();
            this.groupBoxCommonParametersRtsp.SuspendLayout();
            this.tabPageUdpRtp.SuspendLayout();
            this.groupBoxCommonParametersUdpRtp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.tabPagePlaylistEditor.SuspendLayout();
            this.splitContainerPlaylist.Panel1.SuspendLayout();
            this.splitContainerPlaylist.Panel2.SuspendLayout();
            this.splitContainerPlaylist.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlaylist)).BeginInit();
            this.tabProtocols.SuspendLayout();
            this.tabPageDatabaseEditor.SuspendLayout();
            this.splitContainerDatabase.Panel1.SuspendLayout();
            this.splitContainerDatabase.Panel2.SuspendLayout();
            this.splitContainerDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDatabase)).BeginInit();
            this.SuspendLayout();
            // 
            // tabPageHttp
            // 
            this.tabPageHttp.Controls.Add(this.groupBoxCommonParametersHttp);
            this.tabPageHttp.Location = new System.Drawing.Point(4, 22);
            this.tabPageHttp.Name = "tabPageHttp";
            this.tabPageHttp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHttp.Size = new System.Drawing.Size(476, 368);
            this.tabPageHttp.TabIndex = 1;
            this.tabPageHttp.Text = "HTTP";
            this.tabPageHttp.UseVisualStyleBackColor = true;
            // 
            // groupBoxCommonParametersHttp
            // 
            this.groupBoxCommonParametersHttp.Controls.Add(this.label25);
            this.groupBoxCommonParametersHttp.Controls.Add(this.label22);
            this.groupBoxCommonParametersHttp.Controls.Add(this.label20);
            this.groupBoxCommonParametersHttp.Controls.Add(this.labelHttpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersHttp.Controls.Add(this.labelHttpOpenConnectionSleepTime);
            this.groupBoxCommonParametersHttp.Controls.Add(this.textBoxHttpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersHttp.Controls.Add(this.textBoxHttpOpenConnectionSleepTime);
            this.groupBoxCommonParametersHttp.Controls.Add(this.textBoxHttpOpenConnectionTimeout);
            this.groupBoxCommonParametersHttp.Controls.Add(this.labelHttpOpenConnectionTimeout);
            this.groupBoxCommonParametersHttp.Controls.Add(this.labelHttpNetworkInterface);
            this.groupBoxCommonParametersHttp.Controls.Add(this.comboBoxHttpPreferredNetworkInterface);
            this.groupBoxCommonParametersHttp.Location = new System.Drawing.Point(6, 10);
            this.groupBoxCommonParametersHttp.Name = "groupBoxCommonParametersHttp";
            this.groupBoxCommonParametersHttp.Size = new System.Drawing.Size(464, 129);
            this.groupBoxCommonParametersHttp.TabIndex = 0;
            this.groupBoxCommonParametersHttp.TabStop = false;
            this.groupBoxCommonParametersHttp.Text = "Common configuration parameters for HTTP protocol";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(278, 101);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(63, 13);
            this.label25.TabIndex = 10;
            this.label25.Text = "milliseconds";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(278, 75);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(63, 13);
            this.label22.TabIndex = 7;
            this.label22.Text = "milliseconds";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(278, 49);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(63, 13);
            this.label20.TabIndex = 4;
            this.label20.Text = "milliseconds";
            // 
            // labelHttpTotalReopenConnectionTimeout
            // 
            this.labelHttpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelHttpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelHttpTotalReopenConnectionTimeout.Name = "labelHttpTotalReopenConnectionTimeout";
            this.labelHttpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelHttpTotalReopenConnectionTimeout.TabIndex = 8;
            this.labelHttpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelHttpOpenConnectionSleepTime
            // 
            this.labelHttpOpenConnectionSleepTime.AutoSize = true;
            this.labelHttpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelHttpOpenConnectionSleepTime.Name = "labelHttpOpenConnectionSleepTime";
            this.labelHttpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelHttpOpenConnectionSleepTime.TabIndex = 5;
            this.labelHttpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxHttpTotalReopenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxHttpTotalReopenConnectionTimeout, 69);
            this.textBoxHttpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxHttpTotalReopenConnectionTimeout.Name = "textBoxHttpTotalReopenConnectionTimeout";
            this.textBoxHttpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpTotalReopenConnectionTimeout.TabIndex = 9;
            // 
            // textBoxHttpOpenConnectionSleepTime
            // 
            this.errorProvider.SetIconPadding(this.textBoxHttpOpenConnectionSleepTime, 69);
            this.textBoxHttpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxHttpOpenConnectionSleepTime.Name = "textBoxHttpOpenConnectionSleepTime";
            this.textBoxHttpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpOpenConnectionSleepTime.TabIndex = 6;
            // 
            // textBoxHttpOpenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxHttpOpenConnectionTimeout, 69);
            this.textBoxHttpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxHttpOpenConnectionTimeout.Name = "textBoxHttpOpenConnectionTimeout";
            this.textBoxHttpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxHttpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelHttpOpenConnectionTimeout
            // 
            this.labelHttpOpenConnectionTimeout.AutoSize = true;
            this.labelHttpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelHttpOpenConnectionTimeout.Name = "labelHttpOpenConnectionTimeout";
            this.labelHttpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelHttpOpenConnectionTimeout.TabIndex = 2;
            this.labelHttpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelHttpNetworkInterface
            // 
            this.labelHttpNetworkInterface.AutoSize = true;
            this.labelHttpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelHttpNetworkInterface.Name = "labelHttpNetworkInterface";
            this.labelHttpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelHttpNetworkInterface.TabIndex = 0;
            this.labelHttpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxHttpPreferredNetworkInterface
            // 
            this.comboBoxHttpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHttpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxHttpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxHttpPreferredNetworkInterface.Name = "comboBoxHttpPreferredNetworkInterface";
            this.comboBoxHttpPreferredNetworkInterface.Size = new System.Drawing.Size(286, 21);
            this.comboBoxHttpPreferredNetworkInterface.TabIndex = 1;
            // 
            // tabPageRtmp
            // 
            this.tabPageRtmp.Controls.Add(this.groupBoxCommonParametersRtmp);
            this.tabPageRtmp.Location = new System.Drawing.Point(4, 22);
            this.tabPageRtmp.Name = "tabPageRtmp";
            this.tabPageRtmp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRtmp.Size = new System.Drawing.Size(476, 368);
            this.tabPageRtmp.TabIndex = 2;
            this.tabPageRtmp.Text = "RTMP (Real Time Messaging Protocol)";
            this.tabPageRtmp.UseVisualStyleBackColor = true;
            // 
            // groupBoxCommonParametersRtmp
            // 
            this.groupBoxCommonParametersRtmp.Controls.Add(this.label17);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.label18);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.label19);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.labelRtmpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.labelRtmpOpenConnectionSleepTime);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.textBoxRtmpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.textBoxRtmpOpenConnectionSleepTime);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.textBoxRtmpOpenConnectionTimeout);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.labelRtmpOpenConnectionTimeout);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.labelRtmpNetworkInterface);
            this.groupBoxCommonParametersRtmp.Controls.Add(this.comboBoxRtmpPreferredNetworkInterface);
            this.groupBoxCommonParametersRtmp.Location = new System.Drawing.Point(6, 10);
            this.groupBoxCommonParametersRtmp.Name = "groupBoxCommonParametersRtmp";
            this.groupBoxCommonParametersRtmp.Size = new System.Drawing.Size(464, 129);
            this.groupBoxCommonParametersRtmp.TabIndex = 0;
            this.groupBoxCommonParametersRtmp.TabStop = false;
            this.groupBoxCommonParametersRtmp.Text = "Common configuration parameters for RTMP protocol";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(278, 101);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(63, 13);
            this.label17.TabIndex = 10;
            this.label17.Text = "milliseconds";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(278, 75);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(63, 13);
            this.label18.TabIndex = 7;
            this.label18.Text = "milliseconds";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(278, 49);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(63, 13);
            this.label19.TabIndex = 4;
            this.label19.Text = "milliseconds";
            // 
            // labelRtmpTotalReopenConnectionTimeout
            // 
            this.labelRtmpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelRtmpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelRtmpTotalReopenConnectionTimeout.Name = "labelRtmpTotalReopenConnectionTimeout";
            this.labelRtmpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelRtmpTotalReopenConnectionTimeout.TabIndex = 8;
            this.labelRtmpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelRtmpOpenConnectionSleepTime
            // 
            this.labelRtmpOpenConnectionSleepTime.AutoSize = true;
            this.labelRtmpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelRtmpOpenConnectionSleepTime.Name = "labelRtmpOpenConnectionSleepTime";
            this.labelRtmpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelRtmpOpenConnectionSleepTime.TabIndex = 5;
            this.labelRtmpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxRtmpTotalReopenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtmpTotalReopenConnectionTimeout, 69);
            this.textBoxRtmpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxRtmpTotalReopenConnectionTimeout.Name = "textBoxRtmpTotalReopenConnectionTimeout";
            this.textBoxRtmpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpTotalReopenConnectionTimeout.TabIndex = 9;
            // 
            // textBoxRtmpOpenConnectionSleepTime
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtmpOpenConnectionSleepTime, 69);
            this.textBoxRtmpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxRtmpOpenConnectionSleepTime.Name = "textBoxRtmpOpenConnectionSleepTime";
            this.textBoxRtmpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpOpenConnectionSleepTime.TabIndex = 6;
            // 
            // textBoxRtmpOpenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtmpOpenConnectionTimeout, 69);
            this.textBoxRtmpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxRtmpOpenConnectionTimeout.Name = "textBoxRtmpOpenConnectionTimeout";
            this.textBoxRtmpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtmpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelRtmpOpenConnectionTimeout
            // 
            this.labelRtmpOpenConnectionTimeout.AutoSize = true;
            this.labelRtmpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelRtmpOpenConnectionTimeout.Name = "labelRtmpOpenConnectionTimeout";
            this.labelRtmpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelRtmpOpenConnectionTimeout.TabIndex = 2;
            this.labelRtmpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelRtmpNetworkInterface
            // 
            this.labelRtmpNetworkInterface.AutoSize = true;
            this.labelRtmpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelRtmpNetworkInterface.Name = "labelRtmpNetworkInterface";
            this.labelRtmpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelRtmpNetworkInterface.TabIndex = 0;
            this.labelRtmpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxRtmpPreferredNetworkInterface
            // 
            this.comboBoxRtmpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRtmpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxRtmpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxRtmpPreferredNetworkInterface.Name = "comboBoxRtmpPreferredNetworkInterface";
            this.comboBoxRtmpPreferredNetworkInterface.Size = new System.Drawing.Size(286, 21);
            this.comboBoxRtmpPreferredNetworkInterface.TabIndex = 1;
            // 
            // tabPageRtsp
            // 
            this.tabPageRtsp.Controls.Add(this.groupBoxCommonParametersRtsp);
            this.tabPageRtsp.Location = new System.Drawing.Point(4, 22);
            this.tabPageRtsp.Name = "tabPageRtsp";
            this.tabPageRtsp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRtsp.Size = new System.Drawing.Size(476, 368);
            this.tabPageRtsp.TabIndex = 3;
            this.tabPageRtsp.Text = "RTSP (Real Time Streaming Protocol)";
            this.tabPageRtsp.UseVisualStyleBackColor = true;
            // 
            // groupBoxCommonParametersRtsp
            // 
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspIgnoreRtpPayloadType);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.checkBoxRtspIgnoreRtpPayloadType);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.rtspConnectionPreference);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspConnectionPreference);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.label59);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.textBoxRtspClientPortMax);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspConnectionRange);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.textBoxRtspClientPortMin);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.label26);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.label27);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.label28);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspOpenConnectionSleepTime);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.textBoxRtspTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.textBoxRtspOpenConnectionSleepTime);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.textBoxRtspOpenConnectionTimeout);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspOpenConnectionTimeout);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.labelRtspNetworkInterface);
            this.groupBoxCommonParametersRtsp.Controls.Add(this.comboBoxRtspPreferredNetworkInterface);
            this.groupBoxCommonParametersRtsp.Location = new System.Drawing.Point(6, 10);
            this.groupBoxCommonParametersRtsp.Name = "groupBoxCommonParametersRtsp";
            this.groupBoxCommonParametersRtsp.Size = new System.Drawing.Size(464, 235);
            this.groupBoxCommonParametersRtsp.TabIndex = 0;
            this.groupBoxCommonParametersRtsp.TabStop = false;
            this.groupBoxCommonParametersRtsp.Text = "Common configuration parameters for RTSP protocol";
            // 
            // labelRtspIgnoreRtpPayloadType
            // 
            this.labelRtspIgnoreRtpPayloadType.AutoSize = true;
            this.labelRtspIgnoreRtpPayloadType.Location = new System.Drawing.Point(6, 213);
            this.labelRtspIgnoreRtpPayloadType.Name = "labelRtspIgnoreRtpPayloadType";
            this.labelRtspIgnoreRtpPayloadType.Size = new System.Drawing.Size(125, 13);
            this.labelRtspIgnoreRtpPayloadType.TabIndex = 16;
            this.labelRtspIgnoreRtpPayloadType.Text = "Ignore RTP payload type";
            // 
            // checkBoxRtspIgnoreRtpPayloadType
            // 
            this.checkBoxRtspIgnoreRtpPayloadType.AutoSize = true;
            this.checkBoxRtspIgnoreRtpPayloadType.Location = new System.Drawing.Point(172, 213);
            this.checkBoxRtspIgnoreRtpPayloadType.Name = "checkBoxRtspIgnoreRtpPayloadType";
            this.checkBoxRtspIgnoreRtpPayloadType.Size = new System.Drawing.Size(15, 14);
            this.checkBoxRtspIgnoreRtpPayloadType.TabIndex = 17;
            this.checkBoxRtspIgnoreRtpPayloadType.UseVisualStyleBackColor = true;
            // 
            // labelRtspConnectionPreference
            // 
            this.labelRtspConnectionPreference.AutoSize = true;
            this.labelRtspConnectionPreference.Location = new System.Drawing.Point(6, 153);
            this.labelRtspConnectionPreference.Name = "labelRtspConnectionPreference";
            this.labelRtspConnectionPreference.Size = new System.Drawing.Size(115, 13);
            this.labelRtspConnectionPreference.TabIndex = 14;
            this.labelRtspConnectionPreference.Text = "Connection preference";
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(278, 127);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(10, 13);
            this.label59.TabIndex = 18;
            this.label59.Text = "-";
            // 
            // textBoxRtspClientPortMax
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtspClientPortMax, 6);
            this.textBoxRtspClientPortMax.Location = new System.Drawing.Point(294, 124);
            this.textBoxRtspClientPortMax.Name = "textBoxRtspClientPortMax";
            this.textBoxRtspClientPortMax.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspClientPortMax.TabIndex = 13;
            // 
            // labelRtspConnectionRange
            // 
            this.labelRtspConnectionRange.AutoSize = true;
            this.labelRtspConnectionRange.Location = new System.Drawing.Point(6, 127);
            this.labelRtspConnectionRange.Name = "labelRtspConnectionRange";
            this.labelRtspConnectionRange.Size = new System.Drawing.Size(84, 13);
            this.labelRtspConnectionRange.TabIndex = 11;
            this.labelRtspConnectionRange.Text = "Client port range";
            // 
            // textBoxRtspClientPortMin
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtspClientPortMin, -122);
            this.textBoxRtspClientPortMin.Location = new System.Drawing.Point(172, 124);
            this.textBoxRtspClientPortMin.Name = "textBoxRtspClientPortMin";
            this.textBoxRtspClientPortMin.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspClientPortMin.TabIndex = 12;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(278, 101);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(63, 13);
            this.label26.TabIndex = 10;
            this.label26.Text = "milliseconds";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(278, 75);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(63, 13);
            this.label27.TabIndex = 7;
            this.label27.Text = "milliseconds";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(278, 49);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(63, 13);
            this.label28.TabIndex = 4;
            this.label28.Text = "milliseconds";
            // 
            // labelRtspTotalReopenConnectionTimeout
            // 
            this.labelRtspTotalReopenConnectionTimeout.AutoSize = true;
            this.labelRtspTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelRtspTotalReopenConnectionTimeout.Name = "labelRtspTotalReopenConnectionTimeout";
            this.labelRtspTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelRtspTotalReopenConnectionTimeout.TabIndex = 8;
            this.labelRtspTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelRtspOpenConnectionSleepTime
            // 
            this.labelRtspOpenConnectionSleepTime.AutoSize = true;
            this.labelRtspOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelRtspOpenConnectionSleepTime.Name = "labelRtspOpenConnectionSleepTime";
            this.labelRtspOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelRtspOpenConnectionSleepTime.TabIndex = 5;
            this.labelRtspOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxRtspTotalReopenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtspTotalReopenConnectionTimeout, 69);
            this.textBoxRtspTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxRtspTotalReopenConnectionTimeout.Name = "textBoxRtspTotalReopenConnectionTimeout";
            this.textBoxRtspTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspTotalReopenConnectionTimeout.TabIndex = 9;
            // 
            // textBoxRtspOpenConnectionSleepTime
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtspOpenConnectionSleepTime, 69);
            this.textBoxRtspOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxRtspOpenConnectionSleepTime.Name = "textBoxRtspOpenConnectionSleepTime";
            this.textBoxRtspOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspOpenConnectionSleepTime.TabIndex = 6;
            // 
            // textBoxRtspOpenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxRtspOpenConnectionTimeout, 69);
            this.textBoxRtspOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxRtspOpenConnectionTimeout.Name = "textBoxRtspOpenConnectionTimeout";
            this.textBoxRtspOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxRtspOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelRtspOpenConnectionTimeout
            // 
            this.labelRtspOpenConnectionTimeout.AutoSize = true;
            this.labelRtspOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelRtspOpenConnectionTimeout.Name = "labelRtspOpenConnectionTimeout";
            this.labelRtspOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelRtspOpenConnectionTimeout.TabIndex = 2;
            this.labelRtspOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelRtspNetworkInterface
            // 
            this.labelRtspNetworkInterface.AutoSize = true;
            this.labelRtspNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelRtspNetworkInterface.Name = "labelRtspNetworkInterface";
            this.labelRtspNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelRtspNetworkInterface.TabIndex = 0;
            this.labelRtspNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxRtspPreferredNetworkInterface
            // 
            this.comboBoxRtspPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRtspPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxRtspPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxRtspPreferredNetworkInterface.Name = "comboBoxRtspPreferredNetworkInterface";
            this.comboBoxRtspPreferredNetworkInterface.Size = new System.Drawing.Size(286, 21);
            this.comboBoxRtspPreferredNetworkInterface.TabIndex = 1;
            // 
            // tabPageUdpRtp
            // 
            this.tabPageUdpRtp.Controls.Add(this.groupBoxCommonParametersUdpRtp);
            this.tabPageUdpRtp.Location = new System.Drawing.Point(4, 22);
            this.tabPageUdpRtp.Name = "tabPageUdpRtp";
            this.tabPageUdpRtp.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUdpRtp.Size = new System.Drawing.Size(476, 368);
            this.tabPageUdpRtp.TabIndex = 4;
            this.tabPageUdpRtp.Text = "UDP or RTP";
            this.tabPageUdpRtp.UseVisualStyleBackColor = true;
            // 
            // groupBoxCommonParametersUdpRtp
            // 
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.label56);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.labelUdpRtpReceiveDataCheckInterval);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.textBoxUdpRtpReceiveDataCheckInterval);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.label37);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.label54);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.label55);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.labelUdpRtpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.labelUdpRtpOpenConnectionSleepTime);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.textBoxUdpRtpTotalReopenConnectionTimeout);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.textBoxUdpRtpOpenConnectionSleepTime);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.textBoxUdpRtpOpenConnectionTimeout);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.labelUdpRtpOpenConnectionTimeout);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.labelUdpRtpNetworkInterface);
            this.groupBoxCommonParametersUdpRtp.Controls.Add(this.comboBoxUdpRtpPreferredNetworkInterface);
            this.groupBoxCommonParametersUdpRtp.Location = new System.Drawing.Point(6, 10);
            this.groupBoxCommonParametersUdpRtp.Name = "groupBoxCommonParametersUdpRtp";
            this.groupBoxCommonParametersUdpRtp.Size = new System.Drawing.Size(464, 153);
            this.groupBoxCommonParametersUdpRtp.TabIndex = 0;
            this.groupBoxCommonParametersUdpRtp.TabStop = false;
            this.groupBoxCommonParametersUdpRtp.Text = "Common configuration parameters for UDP or RTP protocol";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(278, 127);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(63, 13);
            this.label56.TabIndex = 13;
            this.label56.Text = "milliseconds";
            // 
            // labelUdpRtpReceiveDataCheckInterval
            // 
            this.labelUdpRtpReceiveDataCheckInterval.AutoSize = true;
            this.labelUdpRtpReceiveDataCheckInterval.Location = new System.Drawing.Point(6, 127);
            this.labelUdpRtpReceiveDataCheckInterval.Name = "labelUdpRtpReceiveDataCheckInterval";
            this.labelUdpRtpReceiveDataCheckInterval.Size = new System.Drawing.Size(141, 13);
            this.labelUdpRtpReceiveDataCheckInterval.TabIndex = 11;
            this.labelUdpRtpReceiveDataCheckInterval.Text = "Receive data check interval";
            // 
            // textBoxUdpRtpReceiveDataCheckInterval
            // 
            this.errorProvider.SetIconPadding(this.textBoxUdpRtpReceiveDataCheckInterval, 69);
            this.textBoxUdpRtpReceiveDataCheckInterval.Location = new System.Drawing.Point(172, 124);
            this.textBoxUdpRtpReceiveDataCheckInterval.Name = "textBoxUdpRtpReceiveDataCheckInterval";
            this.textBoxUdpRtpReceiveDataCheckInterval.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpReceiveDataCheckInterval.TabIndex = 12;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(278, 101);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(63, 13);
            this.label37.TabIndex = 10;
            this.label37.Text = "milliseconds";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(278, 75);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(63, 13);
            this.label54.TabIndex = 7;
            this.label54.Text = "milliseconds";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(278, 49);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(63, 13);
            this.label55.TabIndex = 4;
            this.label55.Text = "milliseconds";
            // 
            // labelUdpRtpTotalReopenConnectionTimeout
            // 
            this.labelUdpRtpTotalReopenConnectionTimeout.AutoSize = true;
            this.labelUdpRtpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(6, 101);
            this.labelUdpRtpTotalReopenConnectionTimeout.Name = "labelUdpRtpTotalReopenConnectionTimeout";
            this.labelUdpRtpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(160, 13);
            this.labelUdpRtpTotalReopenConnectionTimeout.TabIndex = 8;
            this.labelUdpRtpTotalReopenConnectionTimeout.Text = "Total reopen connection timeout";
            // 
            // labelUdpRtpOpenConnectionSleepTime
            // 
            this.labelUdpRtpOpenConnectionSleepTime.AutoSize = true;
            this.labelUdpRtpOpenConnectionSleepTime.Location = new System.Drawing.Point(6, 75);
            this.labelUdpRtpOpenConnectionSleepTime.Name = "labelUdpRtpOpenConnectionSleepTime";
            this.labelUdpRtpOpenConnectionSleepTime.Size = new System.Drawing.Size(139, 13);
            this.labelUdpRtpOpenConnectionSleepTime.TabIndex = 5;
            this.labelUdpRtpOpenConnectionSleepTime.Text = "Open connection sleep time";
            // 
            // textBoxUdpRtpTotalReopenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxUdpRtpTotalReopenConnectionTimeout, 69);
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Location = new System.Drawing.Point(172, 98);
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Name = "textBoxUdpRtpTotalReopenConnectionTimeout";
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpTotalReopenConnectionTimeout.TabIndex = 9;
            // 
            // textBoxUdpRtpOpenConnectionSleepTime
            // 
            this.errorProvider.SetIconPadding(this.textBoxUdpRtpOpenConnectionSleepTime, 69);
            this.textBoxUdpRtpOpenConnectionSleepTime.Location = new System.Drawing.Point(172, 72);
            this.textBoxUdpRtpOpenConnectionSleepTime.Name = "textBoxUdpRtpOpenConnectionSleepTime";
            this.textBoxUdpRtpOpenConnectionSleepTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpOpenConnectionSleepTime.TabIndex = 6;
            // 
            // textBoxUdpRtpOpenConnectionTimeout
            // 
            this.errorProvider.SetIconPadding(this.textBoxUdpRtpOpenConnectionTimeout, 69);
            this.textBoxUdpRtpOpenConnectionTimeout.Location = new System.Drawing.Point(172, 46);
            this.textBoxUdpRtpOpenConnectionTimeout.Name = "textBoxUdpRtpOpenConnectionTimeout";
            this.textBoxUdpRtpOpenConnectionTimeout.Size = new System.Drawing.Size(100, 20);
            this.textBoxUdpRtpOpenConnectionTimeout.TabIndex = 3;
            // 
            // labelUdpRtpOpenConnectionTimeout
            // 
            this.labelUdpRtpOpenConnectionTimeout.AutoSize = true;
            this.labelUdpRtpOpenConnectionTimeout.Location = new System.Drawing.Point(6, 49);
            this.labelUdpRtpOpenConnectionTimeout.Name = "labelUdpRtpOpenConnectionTimeout";
            this.labelUdpRtpOpenConnectionTimeout.Size = new System.Drawing.Size(126, 13);
            this.labelUdpRtpOpenConnectionTimeout.TabIndex = 2;
            this.labelUdpRtpOpenConnectionTimeout.Text = "Open connection timeout";
            // 
            // labelUdpRtpNetworkInterface
            // 
            this.labelUdpRtpNetworkInterface.AutoSize = true;
            this.labelUdpRtpNetworkInterface.Location = new System.Drawing.Point(6, 22);
            this.labelUdpRtpNetworkInterface.Name = "labelUdpRtpNetworkInterface";
            this.labelUdpRtpNetworkInterface.Size = new System.Drawing.Size(135, 13);
            this.labelUdpRtpNetworkInterface.TabIndex = 0;
            this.labelUdpRtpNetworkInterface.Text = "Preferred network interface";
            // 
            // comboBoxUdpRtpPreferredNetworkInterface
            // 
            this.comboBoxUdpRtpPreferredNetworkInterface.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUdpRtpPreferredNetworkInterface.FormattingEnabled = true;
            this.comboBoxUdpRtpPreferredNetworkInterface.Location = new System.Drawing.Point(172, 19);
            this.comboBoxUdpRtpPreferredNetworkInterface.Name = "comboBoxUdpRtpPreferredNetworkInterface";
            this.comboBoxUdpRtpPreferredNetworkInterface.Size = new System.Drawing.Size(286, 21);
            this.comboBoxUdpRtpPreferredNetworkInterface.TabIndex = 1;
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(375, 400);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(105, 23);
            this.buttonApply.TabIndex = 7;
            this.buttonApply.Text = "Apply settings";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // tabPagePlaylistEditor
            // 
            this.tabPagePlaylistEditor.Controls.Add(this.splitContainerPlaylist);
            this.tabPagePlaylistEditor.Location = new System.Drawing.Point(4, 22);
            this.tabPagePlaylistEditor.Name = "tabPagePlaylistEditor";
            this.tabPagePlaylistEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePlaylistEditor.Size = new System.Drawing.Size(476, 368);
            this.tabPagePlaylistEditor.TabIndex = 5;
            this.tabPagePlaylistEditor.Text = "Playlist editor";
            this.tabPagePlaylistEditor.UseVisualStyleBackColor = true;
            // 
            // splitContainerPlaylist
            // 
            this.splitContainerPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPlaylist.Location = new System.Drawing.Point(3, 3);
            this.splitContainerPlaylist.Name = "splitContainerPlaylist";
            this.splitContainerPlaylist.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerPlaylist.Panel1
            // 
            this.splitContainerPlaylist.Panel1.Controls.Add(this.dataGridViewPlaylist);
            // 
            // splitContainerPlaylist.Panel2
            // 
            this.splitContainerPlaylist.Panel2.Controls.Add(this.propertyGridPlaylist);
            this.splitContainerPlaylist.Size = new System.Drawing.Size(470, 362);
            this.splitContainerPlaylist.SplitterDistance = 181;
            this.splitContainerPlaylist.TabIndex = 2;
            // 
            // dataGridViewPlaylist
            // 
            this.dataGridViewPlaylist.AllowUserToAddRows = false;
            this.dataGridViewPlaylist.AllowUserToDeleteRows = false;
            this.dataGridViewPlaylist.AllowUserToResizeRows = false;
            this.dataGridViewPlaylist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPlaylist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnChannelName,
            this.ColumnUrl,
            this.ColumnProtocol,
            this.ColumnFilterUrl,
            this.ColumnImage});
            this.dataGridViewPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewPlaylist.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewPlaylist.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewPlaylist.Name = "dataGridViewPlaylist";
            this.dataGridViewPlaylist.RowHeadersVisible = false;
            this.dataGridViewPlaylist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewPlaylist.Size = new System.Drawing.Size(470, 181);
            this.dataGridViewPlaylist.TabIndex = 0;
            this.dataGridViewPlaylist.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            this.dataGridViewPlaylist.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_RowEnter);
            this.dataGridViewPlaylist.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            // 
            // ColumnChannelName
            // 
            this.ColumnChannelName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnChannelName.HeaderText = "Channel name";
            this.ColumnChannelName.Name = "ColumnChannelName";
            this.ColumnChannelName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnChannelName.Width = 120;
            // 
            // ColumnUrl
            // 
            this.ColumnUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnUrl.HeaderText = "Url";
            this.ColumnUrl.Name = "ColumnUrl";
            this.ColumnUrl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnProtocol
            // 
            this.ColumnProtocol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ColumnProtocol.HeaderText = "Protocol";
            this.ColumnProtocol.Name = "ColumnProtocol";
            this.ColumnProtocol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnProtocol.Width = 52;
            // 
            // ColumnFilterUrl
            // 
            this.ColumnFilterUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnFilterUrl.HeaderText = "Filter url";
            this.ColumnFilterUrl.Name = "ColumnFilterUrl";
            this.ColumnFilterUrl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnFilterUrl.Width = 49;
            // 
            // ColumnImage
            // 
            this.ColumnImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnImage.HeaderText = "";
            this.ColumnImage.Image = global::TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Properties.Resources.not_tested;
            this.ColumnImage.Name = "ColumnImage";
            this.ColumnImage.ReadOnly = true;
            this.ColumnImage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnImage.Width = 20;
            // 
            // propertyGridPlaylist
            // 
            this.propertyGridPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridPlaylist.Location = new System.Drawing.Point(0, 0);
            this.propertyGridPlaylist.Name = "propertyGridPlaylist";
            this.propertyGridPlaylist.Size = new System.Drawing.Size(470, 177);
            this.propertyGridPlaylist.TabIndex = 1;
            this.propertyGridPlaylist.ToolbarVisible = false;
            this.propertyGridPlaylist.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridPlaylist_PropertyValueChanged);
            // 
            // buttonLoadPlaylist
            // 
            this.buttonLoadPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoadPlaylist.Location = new System.Drawing.Point(3, 400);
            this.buttonLoadPlaylist.Name = "buttonLoadPlaylist";
            this.buttonLoadPlaylist.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadPlaylist.TabIndex = 3;
            this.buttonLoadPlaylist.Text = "Load playlist";
            this.buttonLoadPlaylist.UseVisualStyleBackColor = true;
            this.buttonLoadPlaylist.Click += new System.EventHandler(this.buttonLoadPlaylist_Click);
            // 
            // buttonSavePlaylist
            // 
            this.buttonSavePlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSavePlaylist.Location = new System.Drawing.Point(84, 400);
            this.buttonSavePlaylist.Name = "buttonSavePlaylist";
            this.buttonSavePlaylist.Size = new System.Drawing.Size(75, 23);
            this.buttonSavePlaylist.TabIndex = 4;
            this.buttonSavePlaylist.Text = "Save playlist";
            this.buttonSavePlaylist.UseVisualStyleBackColor = true;
            this.buttonSavePlaylist.Click += new System.EventHandler(this.buttonSavePlaylist_Click);
            // 
            // tabProtocols
            // 
            this.tabProtocols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabProtocols.Controls.Add(this.tabPageHttp);
            this.tabProtocols.Controls.Add(this.tabPageRtmp);
            this.tabProtocols.Controls.Add(this.tabPageRtsp);
            this.tabProtocols.Controls.Add(this.tabPageUdpRtp);
            this.tabProtocols.Controls.Add(this.tabPagePlaylistEditor);
            this.tabProtocols.Controls.Add(this.tabPageDatabaseEditor);
            this.tabProtocols.Location = new System.Drawing.Point(0, 0);
            this.tabProtocols.Name = "tabProtocols";
            this.tabProtocols.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabProtocols.SelectedIndex = 0;
            this.tabProtocols.ShowToolTips = true;
            this.tabProtocols.Size = new System.Drawing.Size(484, 394);
            this.tabProtocols.TabIndex = 1;
            this.tabProtocols.Tag = "";
            this.tabProtocols.SelectedIndexChanged += new System.EventHandler(this.tabProtocols_SelectedIndexChanged);
            // 
            // tabPageDatabaseEditor
            // 
            this.tabPageDatabaseEditor.Controls.Add(this.splitContainerDatabase);
            this.tabPageDatabaseEditor.Location = new System.Drawing.Point(4, 22);
            this.tabPageDatabaseEditor.Name = "tabPageDatabaseEditor";
            this.tabPageDatabaseEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDatabaseEditor.Size = new System.Drawing.Size(476, 368);
            this.tabPageDatabaseEditor.TabIndex = 6;
            this.tabPageDatabaseEditor.Text = "Database editor";
            this.tabPageDatabaseEditor.UseVisualStyleBackColor = true;
            // 
            // splitContainerDatabase
            // 
            this.splitContainerDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerDatabase.Location = new System.Drawing.Point(3, 3);
            this.splitContainerDatabase.Name = "splitContainerDatabase";
            this.splitContainerDatabase.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerDatabase.Panel1
            // 
            this.splitContainerDatabase.Panel1.Controls.Add(this.dataGridViewDatabase);
            // 
            // splitContainerDatabase.Panel2
            // 
            this.splitContainerDatabase.Panel2.Controls.Add(this.propertyGridDatabase);
            this.splitContainerDatabase.Size = new System.Drawing.Size(470, 362);
            this.splitContainerDatabase.SplitterDistance = 181;
            this.splitContainerDatabase.TabIndex = 3;
            // 
            // dataGridViewDatabase
            // 
            this.dataGridViewDatabase.AllowUserToAddRows = false;
            this.dataGridViewDatabase.AllowUserToDeleteRows = false;
            this.dataGridViewDatabase.AllowUserToResizeRows = false;
            this.dataGridViewDatabase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDatabase.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.Column1,
            this.Column2,
            this.dataGridViewImageColumn2});
            this.dataGridViewDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDatabase.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewDatabase.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewDatabase.Name = "dataGridViewDatabase";
            this.dataGridViewDatabase.RowHeadersVisible = false;
            this.dataGridViewDatabase.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDatabase.Size = new System.Drawing.Size(470, 181);
            this.dataGridViewDatabase.TabIndex = 0;
            this.dataGridViewDatabase.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDatabase_CellClick);
            this.dataGridViewDatabase.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDatabase_RowEnter);
            this.dataGridViewDatabase.SelectionChanged += new System.EventHandler(this.dataGridViewDatabase_SelectionChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn1.HeaderText = "Channel name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 120;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "Url";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridViewTextBoxColumn3.HeaderText = "Protocol";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 52;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn4.HeaderText = "Transport Stream ID";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn4.Width = 75;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column1.HeaderText = "Program number";
            this.Column1.Name = "Column1";
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column1.Width = 49;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column2.HeaderText = "Program map PID";
            this.Column2.Name = "Column2";
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column2.Width = 75;
            // 
            // dataGridViewImageColumn2
            // 
            this.dataGridViewImageColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewImageColumn2.HeaderText = "";
            this.dataGridViewImageColumn2.Image = global::TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Properties.Resources.not_tested;
            this.dataGridViewImageColumn2.Name = "dataGridViewImageColumn2";
            this.dataGridViewImageColumn2.ReadOnly = true;
            this.dataGridViewImageColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn2.Width = 20;
            // 
            // propertyGridDatabase
            // 
            this.propertyGridDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridDatabase.Location = new System.Drawing.Point(0, 0);
            this.propertyGridDatabase.Name = "propertyGridDatabase";
            this.propertyGridDatabase.Size = new System.Drawing.Size(470, 177);
            this.propertyGridDatabase.TabIndex = 1;
            this.propertyGridDatabase.ToolbarVisible = false;
            this.propertyGridDatabase.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridDatabase_PropertyValueChanged);
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(476, 368);
            this.tabPageGeneral.TabIndex = 5;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateDatabase
            // 
            this.buttonUpdateDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateDatabase.Location = new System.Drawing.Point(165, 400);
            this.buttonUpdateDatabase.Name = "buttonUpdateDatabase";
            this.buttonUpdateDatabase.Size = new System.Drawing.Size(102, 23);
            this.buttonUpdateDatabase.TabIndex = 6;
            this.buttonUpdateDatabase.Text = "Update database";
            this.buttonUpdateDatabase.UseVisualStyleBackColor = true;
            this.buttonUpdateDatabase.Visible = false;
            this.buttonUpdateDatabase.Click += new System.EventHandler(this.buttonUpdateDatabase_Click);
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewImageColumn1.HeaderText = "";
            this.dataGridViewImageColumn1.Image = global::TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Properties.Resources.Up;
            this.dataGridViewImageColumn1.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Stretch;
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.ReadOnly = true;
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn1.Width = 20;
            // 
            // buttonStoreChanges
            // 
            this.buttonStoreChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStoreChanges.Location = new System.Drawing.Point(3, 400);
            this.buttonStoreChanges.Name = "buttonStoreChanges";
            this.buttonStoreChanges.Size = new System.Drawing.Size(91, 23);
            this.buttonStoreChanges.TabIndex = 8;
            this.buttonStoreChanges.Text = "Store changes";
            this.buttonStoreChanges.UseVisualStyleBackColor = true;
            this.buttonStoreChanges.Visible = false;
            this.buttonStoreChanges.Click += new System.EventHandler(this.buttonStoreChanges_Click);
            // 
            // rtspConnectionPreference
            // 
            this.rtspConnectionPreference.Location = new System.Drawing.Point(172, 150);
            this.rtspConnectionPreference.MulticastPreference = 2;
            this.rtspConnectionPreference.Name = "rtspConnectionPreference";
            this.rtspConnectionPreference.SameConnectionPreference = 0;
            this.rtspConnectionPreference.Size = new System.Drawing.Size(126, 56);
            this.rtspConnectionPreference.TabIndex = 15;
            this.rtspConnectionPreference.UdpPreference = 1;
            // 
            // buttonSetMpeg2TSParser
            // 
            this.buttonSetMpeg2TSParser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetMpeg2TSParser.Location = new System.Drawing.Point(273, 400);
            this.buttonSetMpeg2TSParser.Name = "buttonSetMpeg2TSParser";
            this.buttonSetMpeg2TSParser.Size = new System.Drawing.Size(96, 23);
            this.buttonSetMpeg2TSParser.TabIndex = 9;
            this.buttonSetMpeg2TSParser.Text = "Set M2TS parser";
            this.buttonSetMpeg2TSParser.UseVisualStyleBackColor = true;
            this.buttonSetMpeg2TSParser.Visible = false;
            this.buttonSetMpeg2TSParser.Click += new System.EventHandler(this.buttonSetMpeg2TSParser_Click);
            // 
            // Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSetMpeg2TSParser);
            this.Controls.Add(this.buttonStoreChanges);
            this.Controls.Add(this.buttonUpdateDatabase);
            this.Controls.Add(this.tabProtocols);
            this.Controls.Add(this.buttonSavePlaylist);
            this.Controls.Add(this.buttonLoadPlaylist);
            this.Controls.Add(this.buttonApply);
            this.Name = "Editor";
            this.Size = new System.Drawing.Size(484, 426);
            this.tabPageHttp.ResumeLayout(false);
            this.groupBoxCommonParametersHttp.ResumeLayout(false);
            this.groupBoxCommonParametersHttp.PerformLayout();
            this.tabPageRtmp.ResumeLayout(false);
            this.groupBoxCommonParametersRtmp.ResumeLayout(false);
            this.groupBoxCommonParametersRtmp.PerformLayout();
            this.tabPageRtsp.ResumeLayout(false);
            this.groupBoxCommonParametersRtsp.ResumeLayout(false);
            this.groupBoxCommonParametersRtsp.PerformLayout();
            this.tabPageUdpRtp.ResumeLayout(false);
            this.groupBoxCommonParametersUdpRtp.ResumeLayout(false);
            this.groupBoxCommonParametersUdpRtp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.tabPagePlaylistEditor.ResumeLayout(false);
            this.splitContainerPlaylist.Panel1.ResumeLayout(false);
            this.splitContainerPlaylist.Panel2.ResumeLayout(false);
            this.splitContainerPlaylist.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlaylist)).EndInit();
            this.tabProtocols.ResumeLayout(false);
            this.tabPageDatabaseEditor.ResumeLayout(false);
            this.splitContainerDatabase.Panel1.ResumeLayout(false);
            this.splitContainerDatabase.Panel2.ResumeLayout(false);
            this.splitContainerDatabase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDatabase)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPageHttp;
        private System.Windows.Forms.GroupBox groupBoxCommonParametersHttp;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label labelHttpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelHttpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxHttpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxHttpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxHttpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelHttpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelHttpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxHttpPreferredNetworkInterface;
        private System.Windows.Forms.TabPage tabPageRtmp;
        private System.Windows.Forms.GroupBox groupBoxCommonParametersRtmp;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label labelRtmpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtmpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxRtmpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtmpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtmpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxRtmpPreferredNetworkInterface;
        private System.Windows.Forms.TabPage tabPageRtsp;
        private System.Windows.Forms.GroupBox groupBoxCommonParametersRtsp;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.TextBox textBoxRtspClientPortMax;
        private System.Windows.Forms.Label labelRtspConnectionRange;
        private System.Windows.Forms.TextBox textBoxRtspClientPortMin;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label labelRtspTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtspTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxRtspOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxRtspOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspOpenConnectionTimeout;
        private System.Windows.Forms.Label labelRtspNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxRtspPreferredNetworkInterface;
        private System.Windows.Forms.TabPage tabPageUdpRtp;
        private System.Windows.Forms.GroupBox groupBoxCommonParametersUdpRtp;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.Label labelUdpRtpReceiveDataCheckInterval;
        private System.Windows.Forms.TextBox textBoxUdpRtpReceiveDataCheckInterval;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.Label labelUdpRtpTotalReopenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxUdpRtpTotalReopenConnectionTimeout;
        private System.Windows.Forms.TextBox textBoxUdpRtpOpenConnectionSleepTime;
        private System.Windows.Forms.TextBox textBoxUdpRtpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpOpenConnectionTimeout;
        private System.Windows.Forms.Label labelUdpRtpNetworkInterface;
        private System.Windows.Forms.ComboBox comboBoxUdpRtpPreferredNetworkInterface;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label labelRtspConnectionPreference;
        private System.Windows.Forms.TabPage tabPagePlaylistEditor;
        private System.Windows.Forms.Button buttonSavePlaylist;
        private System.Windows.Forms.Button buttonLoadPlaylist;
        private System.Windows.Forms.TabControl tabProtocols;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.PropertyGrid propertyGridPlaylist;
        private System.Windows.Forms.Button buttonUpdateDatabase;
        private System.Windows.Forms.DataGridView dataGridViewPlaylist;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private Url.RtspConnectionPreference rtspConnectionPreference;
        private System.Windows.Forms.SplitContainer splitContainerPlaylist;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnChannelName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnProtocol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFilterUrl;
        private System.Windows.Forms.DataGridViewImageColumn ColumnImage;
        private System.Windows.Forms.Label labelRtspIgnoreRtpPayloadType;
        private System.Windows.Forms.CheckBox checkBoxRtspIgnoreRtpPayloadType;
        private System.Windows.Forms.TabPage tabPageDatabaseEditor;
        private System.Windows.Forms.SplitContainer splitContainerDatabase;
        private System.Windows.Forms.DataGridView dataGridViewDatabase;
        private System.Windows.Forms.PropertyGrid propertyGridDatabase;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn2;
        private System.Windows.Forms.Button buttonStoreChanges;
        private System.Windows.Forms.Button buttonSetMpeg2TSParser;
    }
}
