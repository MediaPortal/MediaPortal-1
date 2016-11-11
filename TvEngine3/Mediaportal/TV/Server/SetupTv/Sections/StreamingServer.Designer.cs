using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  partial class StreamingServer
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
      this.numericUpDownPort = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelPort = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxInterface = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelInterface = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.groupBoxClients = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.listViewClients = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPListView();
      this.columnHeaderStreamId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderClientSessionId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderClientIpAddress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderStreamDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderClientConnectedSince = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderClientIsActive = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.columnHeaderStreamUrl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPColumnHeader();
      this.buttonKick = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.groupBoxSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelStatusValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
      this.groupBoxClients.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // numericUpDownPort
      // 
      this.numericUpDownPort.Location = new System.Drawing.Point(341, 20);
      this.numericUpDownPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.numericUpDownPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownPort.Name = "numericUpDownPort";
      this.numericUpDownPort.Size = new System.Drawing.Size(55, 20);
      this.numericUpDownPort.TabIndex = 3;
      this.numericUpDownPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownPort.Value = new decimal(new int[] {
            554,
            0,
            0,
            0});
      // 
      // labelPort
      // 
      this.labelPort.AutoSize = true;
      this.labelPort.Location = new System.Drawing.Point(306, 22);
      this.labelPort.Name = "labelPort";
      this.labelPort.Size = new System.Drawing.Size(29, 13);
      this.labelPort.TabIndex = 2;
      this.labelPort.Text = "Port:";
      // 
      // comboBoxInterface
      // 
      this.comboBoxInterface.FormattingEnabled = true;
      this.comboBoxInterface.Location = new System.Drawing.Point(64, 19);
      this.comboBoxInterface.Name = "comboBoxInterface";
      this.comboBoxInterface.Size = new System.Drawing.Size(222, 21);
      this.comboBoxInterface.TabIndex = 1;
      // 
      // labelInterface
      // 
      this.labelInterface.AutoSize = true;
      this.labelInterface.Location = new System.Drawing.Point(6, 22);
      this.labelInterface.Name = "labelInterface";
      this.labelInterface.Size = new System.Drawing.Size(52, 13);
      this.labelInterface.TabIndex = 0;
      this.labelInterface.Text = "Interface:";
      // 
      // groupBoxClients
      // 
      this.groupBoxClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxClients.Controls.Add(this.listViewClients);
      this.groupBoxClients.Controls.Add(this.buttonKick);
      this.groupBoxClients.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxClients.Location = new System.Drawing.Point(6, 82);
      this.groupBoxClients.Name = "groupBoxClients";
      this.groupBoxClients.Size = new System.Drawing.Size(468, 328);
      this.groupBoxClients.TabIndex = 1;
      this.groupBoxClients.TabStop = false;
      this.groupBoxClients.Text = "Clients";
      // 
      // listViewClients
      // 
      this.listViewClients.AllowColumnReorder = true;
      this.listViewClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStreamId,
            this.columnHeaderClientSessionId,
            this.columnHeaderClientIpAddress,
            this.columnHeaderStreamDescription,
            this.columnHeaderClientConnectedSince,
            this.columnHeaderClientIsActive,
            this.columnHeaderStreamUrl});
      this.listViewClients.FullRowSelect = true;
      this.listViewClients.Location = new System.Drawing.Point(9, 19);
      this.listViewClients.Name = "listViewClients";
      this.listViewClients.Size = new System.Drawing.Size(450, 274);
      this.listViewClients.TabIndex = 0;
      this.listViewClients.UseCompatibleStateImageBehavior = false;
      this.listViewClients.View = System.Windows.Forms.View.Details;
      // 
      // columnHeaderStreamId
      // 
      this.columnHeaderStreamId.Text = "Stream";
      this.columnHeaderStreamId.Width = 50;
      // 
      // columnHeaderClientSessionId
      // 
      this.columnHeaderClientSessionId.Text = "Session";
      // 
      // columnHeaderClientIpAddress
      // 
      this.columnHeaderClientIpAddress.Text = "IP Address";
      this.columnHeaderClientIpAddress.Width = 100;
      // 
      // columnHeaderStreamDescription
      // 
      this.columnHeaderStreamDescription.Text = "Description";
      this.columnHeaderStreamDescription.Width = 120;
      // 
      // columnHeaderClientConnectedSince
      // 
      this.columnHeaderClientConnectedSince.Text = "Connected Since";
      this.columnHeaderClientConnectedSince.Width = 120;
      // 
      // columnHeaderClientIsActive
      // 
      this.columnHeaderClientIsActive.Text = "Active?";
      this.columnHeaderClientIsActive.Width = 50;
      // 
      // columnHeaderStreamUrl
      // 
      this.columnHeaderStreamUrl.Text = "URL";
      this.columnHeaderStreamUrl.Width = 100;
      // 
      // buttonKick
      // 
      this.buttonKick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonKick.Location = new System.Drawing.Point(9, 299);
      this.buttonKick.Name = "buttonKick";
      this.buttonKick.Size = new System.Drawing.Size(55, 23);
      this.buttonKick.TabIndex = 1;
      this.buttonKick.Text = "&Kick";
      this.buttonKick.UseVisualStyleBackColor = true;
      this.buttonKick.Click += new System.EventHandler(this.buttonKick_Click);
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Controls.Add(this.labelStatusValue);
      this.groupBoxSettings.Controls.Add(this.labelStatus);
      this.groupBoxSettings.Controls.Add(this.numericUpDownPort);
      this.groupBoxSettings.Controls.Add(this.labelPort);
      this.groupBoxSettings.Controls.Add(this.comboBoxInterface);
      this.groupBoxSettings.Controls.Add(this.labelInterface);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSettings.Location = new System.Drawing.Point(6, 6);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(425, 70);
      this.groupBoxSettings.TabIndex = 0;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // labelStatusValue
      // 
      this.labelStatusValue.AutoSize = true;
      this.labelStatusValue.ForeColor = System.Drawing.Color.Red;
      this.labelStatusValue.Location = new System.Drawing.Point(61, 46);
      this.labelStatusValue.Name = "labelStatusValue";
      this.labelStatusValue.Size = new System.Drawing.Size(346, 13);
      this.labelStatusValue.TabIndex = 5;
      this.labelStatusValue.Text = "Server is not running. Check configured interface and port are available.";
      // 
      // labelStatus
      // 
      this.labelStatus.AutoSize = true;
      this.labelStatus.Location = new System.Drawing.Point(6, 46);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(40, 13);
      this.labelStatus.TabIndex = 4;
      this.labelStatus.Text = "Status:";
      // 
      // StreamingServer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBoxSettings);
      this.Controls.Add(this.groupBoxClients);
      this.Name = "StreamingServer";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
      this.groupBoxClients.ResumeLayout(false);
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MPNumericUpDown numericUpDownPort;
    private MPLabel labelPort;
    private MPComboBox comboBoxInterface;
    private MPLabel labelInterface;
    private MPGroupBox groupBoxClients;
    private MPListView listViewClients;
    private MPColumnHeader columnHeaderStreamId;
    private MPColumnHeader columnHeaderClientIpAddress;
    private MPColumnHeader columnHeaderClientConnectedSince;
    private MPColumnHeader columnHeaderStreamDescription;
    private MPButton buttonKick;
    private MPGroupBox groupBoxSettings;
    private MPLabel labelStatusValue;
    private MPLabel labelStatus;
    private MPColumnHeader columnHeaderStreamUrl;
    private MPColumnHeader columnHeaderClientSessionId;
    private MPColumnHeader columnHeaderClientIsActive;
  }
}