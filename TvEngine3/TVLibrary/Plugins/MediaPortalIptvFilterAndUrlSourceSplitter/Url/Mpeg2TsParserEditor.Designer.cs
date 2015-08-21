namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    partial class Mpeg2TsParserEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mpeg2TsParserEditor));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageBasicSettings = new System.Windows.Forms.TabPage();
            this.groupBoxDescription = new System.Windows.Forms.GroupBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.groupBoxForceStreamIdentification = new System.Windows.Forms.GroupBox();
            this.labelForceStreamIdentification = new System.Windows.Forms.Label();
            this.textBoxProgramMapPID = new System.Windows.Forms.TextBox();
            this.textBoxProgramNumber = new System.Windows.Forms.TextBox();
            this.textBoxTransportStreamId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.checkBoxSetNotScrambled = new System.Windows.Forms.CheckBox();
            this.checkBoxDetectDiscontinuity = new System.Windows.Forms.CheckBox();
            this.checkBoxAlignToMpeg2TsPacket = new System.Windows.Forms.CheckBox();
            this.tabPageFilteringProgramElements = new System.Windows.Forms.TabPage();
            this.textBoxTransportStreamProgramMapSectionProgramNumber = new System.Windows.Forms.TextBox();
            this.labelTransportStreamProgramMapPID = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.tabControlFilterProgramElements = new System.Windows.Forms.TabControl();
            this.tabPageStreamAnalysis = new System.Windows.Forms.TabPage();
            this.splitContainerStreamAnalysis = new System.Windows.Forms.SplitContainer();
            this.labelDetectedSections = new System.Windows.Forms.Label();
            this.treeViewSections = new System.Windows.Forms.TreeView();
            this.splitContainerStreamAnalysisDetails = new System.Windows.Forms.SplitContainer();
            this.textBoxSectionData = new System.Windows.Forms.TextBox();
            this.labelSectionData = new System.Windows.Forms.Label();
            this.labelRawSectionData = new System.Windows.Forms.Label();
            this.textBoxRawSectionData = new System.Windows.Forms.TextBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabControl.SuspendLayout();
            this.tabPageBasicSettings.SuspendLayout();
            this.groupBoxDescription.SuspendLayout();
            this.groupBoxForceStreamIdentification.SuspendLayout();
            this.groupBoxGeneral.SuspendLayout();
            this.tabPageFilteringProgramElements.SuspendLayout();
            this.tabPageStreamAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStreamAnalysis)).BeginInit();
            this.splitContainerStreamAnalysis.Panel1.SuspendLayout();
            this.splitContainerStreamAnalysis.Panel2.SuspendLayout();
            this.splitContainerStreamAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStreamAnalysisDetails)).BeginInit();
            this.splitContainerStreamAnalysisDetails.Panel1.SuspendLayout();
            this.splitContainerStreamAnalysisDetails.Panel2.SuspendLayout();
            this.splitContainerStreamAnalysisDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(674, 338);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(593, 338);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            this.buttonOK.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageBasicSettings);
            this.tabControl.Controls.Add(this.tabPageFilteringProgramElements);
            this.tabControl.Controls.Add(this.tabPageStreamAnalysis);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(746, 320);
            this.tabControl.TabIndex = 0;
            this.tabControl.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // tabPageBasicSettings
            // 
            this.tabPageBasicSettings.Controls.Add(this.groupBoxDescription);
            this.tabPageBasicSettings.Controls.Add(this.groupBoxForceStreamIdentification);
            this.tabPageBasicSettings.Controls.Add(this.groupBoxGeneral);
            this.tabPageBasicSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageBasicSettings.Name = "tabPageBasicSettings";
            this.tabPageBasicSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBasicSettings.Size = new System.Drawing.Size(738, 294);
            this.tabPageBasicSettings.TabIndex = 0;
            this.tabPageBasicSettings.Text = "Basic settings";
            this.tabPageBasicSettings.UseVisualStyleBackColor = true;
            this.tabPageBasicSettings.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // groupBoxDescription
            // 
            this.groupBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDescription.Controls.Add(this.labelDescription);
            this.groupBoxDescription.Location = new System.Drawing.Point(6, 207);
            this.groupBoxDescription.Name = "groupBoxDescription";
            this.groupBoxDescription.Size = new System.Drawing.Size(574, 81);
            this.groupBoxDescription.TabIndex = 2;
            this.groupBoxDescription.TabStop = false;
            this.groupBoxDescription.Text = "Description";
            // 
            // labelDescription
            // 
            this.labelDescription.Location = new System.Drawing.Point(6, 16);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(562, 53);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = resources.GetString("labelDescription.Text");
            // 
            // groupBoxForceStreamIdentification
            // 
            this.groupBoxForceStreamIdentification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxForceStreamIdentification.Controls.Add(this.labelForceStreamIdentification);
            this.groupBoxForceStreamIdentification.Controls.Add(this.textBoxProgramMapPID);
            this.groupBoxForceStreamIdentification.Controls.Add(this.textBoxProgramNumber);
            this.groupBoxForceStreamIdentification.Controls.Add(this.textBoxTransportStreamId);
            this.groupBoxForceStreamIdentification.Controls.Add(this.label3);
            this.groupBoxForceStreamIdentification.Controls.Add(this.label2);
            this.groupBoxForceStreamIdentification.Controls.Add(this.label1);
            this.groupBoxForceStreamIdentification.Location = new System.Drawing.Point(6, 101);
            this.groupBoxForceStreamIdentification.Name = "groupBoxForceStreamIdentification";
            this.groupBoxForceStreamIdentification.Size = new System.Drawing.Size(574, 100);
            this.groupBoxForceStreamIdentification.TabIndex = 1;
            this.groupBoxForceStreamIdentification.TabStop = false;
            this.groupBoxForceStreamIdentification.Text = "Force stream identification";
            // 
            // labelForceStreamIdentification
            // 
            this.labelForceStreamIdentification.Location = new System.Drawing.Point(327, 16);
            this.labelForceStreamIdentification.Name = "labelForceStreamIdentification";
            this.labelForceStreamIdentification.Size = new System.Drawing.Size(241, 75);
            this.labelForceStreamIdentification.TabIndex = 6;
            this.labelForceStreamIdentification.Text = resources.GetString("labelForceStreamIdentification.Text");
            // 
            // textBoxProgramMapPID
            // 
            this.errorProvider.SetIconPadding(this.textBoxProgramMapPID, -120);
            this.textBoxProgramMapPID.Location = new System.Drawing.Point(221, 71);
            this.textBoxProgramMapPID.Name = "textBoxProgramMapPID";
            this.textBoxProgramMapPID.Size = new System.Drawing.Size(100, 20);
            this.textBoxProgramMapPID.TabIndex = 5;
            this.textBoxProgramMapPID.Tag = "The value of program map PID in program association section (PAT) and PID of pack" +
    "et containing transport stream program section (PMT) or empty if value don\'t hav" +
    "e to be changed.";
            this.textBoxProgramMapPID.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // textBoxProgramNumber
            // 
            this.errorProvider.SetIconPadding(this.textBoxProgramNumber, -120);
            this.textBoxProgramNumber.Location = new System.Drawing.Point(221, 45);
            this.textBoxProgramNumber.Name = "textBoxProgramNumber";
            this.textBoxProgramNumber.Size = new System.Drawing.Size(100, 20);
            this.textBoxProgramNumber.TabIndex = 4;
            this.textBoxProgramNumber.Tag = "The value of program number in program association section (PAT) and transport st" +
    "ream program map section (PMT) or empty if value don\'t have to be changed.";
            this.textBoxProgramNumber.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // textBoxTransportStreamId
            // 
            this.errorProvider.SetIconPadding(this.textBoxTransportStreamId, -120);
            this.textBoxTransportStreamId.Location = new System.Drawing.Point(221, 19);
            this.textBoxTransportStreamId.Name = "textBoxTransportStreamId";
            this.textBoxTransportStreamId.Size = new System.Drawing.Size(100, 20);
            this.textBoxTransportStreamId.TabIndex = 3;
            this.textBoxTransportStreamId.Tag = "The value of transport stream ID in program association section (PAT) or empty if" +
    " value don\'t have to be changed.";
            this.textBoxTransportStreamId.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Program map PID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Program number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transport stream ID";
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneral.Controls.Add(this.checkBoxSetNotScrambled);
            this.groupBoxGeneral.Controls.Add(this.checkBoxDetectDiscontinuity);
            this.groupBoxGeneral.Controls.Add(this.checkBoxAlignToMpeg2TsPacket);
            this.groupBoxGeneral.Location = new System.Drawing.Point(6, 6);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(574, 89);
            this.groupBoxGeneral.TabIndex = 0;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General";
            // 
            // checkBoxSetNotScrambled
            // 
            this.checkBoxSetNotScrambled.AutoSize = true;
            this.checkBoxSetNotScrambled.Location = new System.Drawing.Point(6, 65);
            this.checkBoxSetNotScrambled.Name = "checkBoxSetNotScrambled";
            this.checkBoxSetNotScrambled.Size = new System.Drawing.Size(158, 17);
            this.checkBoxSetNotScrambled.TabIndex = 2;
            this.checkBoxSetNotScrambled.Tag = "Specifies if MPEG2 TS parser have to set stream as not encrypted.";
            this.checkBoxSetNotScrambled.Text = "Set stream as not encrypted";
            this.checkBoxSetNotScrambled.UseVisualStyleBackColor = true;
            this.checkBoxSetNotScrambled.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // checkBoxDetectDiscontinuity
            // 
            this.checkBoxDetectDiscontinuity.AutoSize = true;
            this.checkBoxDetectDiscontinuity.Location = new System.Drawing.Point(6, 42);
            this.checkBoxDetectDiscontinuity.Name = "checkBoxDetectDiscontinuity";
            this.checkBoxDetectDiscontinuity.Size = new System.Drawing.Size(119, 17);
            this.checkBoxDetectDiscontinuity.TabIndex = 1;
            this.checkBoxDetectDiscontinuity.Tag = "Specifies if MPEG2 TS parser have to detect discontinuities in continuity counter" +
    "s.";
            this.checkBoxDetectDiscontinuity.Text = "Detect discontinuity";
            this.checkBoxDetectDiscontinuity.UseVisualStyleBackColor = true;
            this.checkBoxDetectDiscontinuity.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // checkBoxAlignToMpeg2TsPacket
            // 
            this.checkBoxAlignToMpeg2TsPacket.AutoSize = true;
            this.checkBoxAlignToMpeg2TsPacket.Location = new System.Drawing.Point(6, 19);
            this.checkBoxAlignToMpeg2TsPacket.Name = "checkBoxAlignToMpeg2TsPacket";
            this.checkBoxAlignToMpeg2TsPacket.Size = new System.Drawing.Size(154, 17);
            this.checkBoxAlignToMpeg2TsPacket.TabIndex = 0;
            this.checkBoxAlignToMpeg2TsPacket.Tag = "Specifies if MPEG2 TS parser have to align stream to MPEG2 TS packet boundaries. " +
    "If not checked, other functions in parser will not work and all parser settings " +
    "are ignored.";
            this.checkBoxAlignToMpeg2TsPacket.Text = "Align to MPEG2 TS packet";
            this.checkBoxAlignToMpeg2TsPacket.UseVisualStyleBackColor = true;
            this.checkBoxAlignToMpeg2TsPacket.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // tabPageFilteringProgramElements
            // 
            this.tabPageFilteringProgramElements.Controls.Add(this.textBoxTransportStreamProgramMapSectionProgramNumber);
            this.tabPageFilteringProgramElements.Controls.Add(this.labelTransportStreamProgramMapPID);
            this.tabPageFilteringProgramElements.Controls.Add(this.buttonRemove);
            this.tabPageFilteringProgramElements.Controls.Add(this.buttonAdd);
            this.tabPageFilteringProgramElements.Controls.Add(this.tabControlFilterProgramElements);
            this.tabPageFilteringProgramElements.Location = new System.Drawing.Point(4, 22);
            this.tabPageFilteringProgramElements.Name = "tabPageFilteringProgramElements";
            this.tabPageFilteringProgramElements.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFilteringProgramElements.Size = new System.Drawing.Size(738, 294);
            this.tabPageFilteringProgramElements.TabIndex = 1;
            this.tabPageFilteringProgramElements.Text = "Filtering program elements";
            this.tabPageFilteringProgramElements.UseVisualStyleBackColor = true;
            this.tabPageFilteringProgramElements.Enter += new System.EventHandler(this.checkBoxAlignToMpeg2TsPacket_Enter);
            // 
            // textBoxTransportStreamProgramMapSectionProgramNumber
            // 
            this.textBoxTransportStreamProgramMapSectionProgramNumber.Location = new System.Drawing.Point(183, 9);
            this.textBoxTransportStreamProgramMapSectionProgramNumber.Name = "textBoxTransportStreamProgramMapSectionProgramNumber";
            this.textBoxTransportStreamProgramMapSectionProgramNumber.Size = new System.Drawing.Size(74, 20);
            this.textBoxTransportStreamProgramMapSectionProgramNumber.TabIndex = 0;
            // 
            // labelTransportStreamProgramMapPID
            // 
            this.labelTransportStreamProgramMapPID.AutoSize = true;
            this.labelTransportStreamProgramMapPID.Location = new System.Drawing.Point(6, 12);
            this.labelTransportStreamProgramMapPID.Name = "labelTransportStreamProgramMapPID";
            this.labelTransportStreamProgramMapPID.Size = new System.Drawing.Size(110, 13);
            this.labelTransportStreamProgramMapPID.TabIndex = 7;
            this.labelTransportStreamProgramMapPID.Text = "PMT Program number";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(344, 7);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(263, 7);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // tabControlFilterProgramElements
            // 
            this.tabControlFilterProgramElements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlFilterProgramElements.Location = new System.Drawing.Point(6, 35);
            this.tabControlFilterProgramElements.Name = "tabControlFilterProgramElements";
            this.tabControlFilterProgramElements.SelectedIndex = 0;
            this.tabControlFilterProgramElements.Size = new System.Drawing.Size(726, 253);
            this.tabControlFilterProgramElements.TabIndex = 3;
            // 
            // tabPageStreamAnalysis
            // 
            this.tabPageStreamAnalysis.Controls.Add(this.splitContainerStreamAnalysis);
            this.tabPageStreamAnalysis.Location = new System.Drawing.Point(4, 22);
            this.tabPageStreamAnalysis.Name = "tabPageStreamAnalysis";
            this.tabPageStreamAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStreamAnalysis.Size = new System.Drawing.Size(738, 294);
            this.tabPageStreamAnalysis.TabIndex = 2;
            this.tabPageStreamAnalysis.Text = "Stream analysis";
            this.tabPageStreamAnalysis.UseVisualStyleBackColor = true;
            // 
            // splitContainerStreamAnalysis
            // 
            this.splitContainerStreamAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerStreamAnalysis.Location = new System.Drawing.Point(3, 3);
            this.splitContainerStreamAnalysis.Name = "splitContainerStreamAnalysis";
            // 
            // splitContainerStreamAnalysis.Panel1
            // 
            this.splitContainerStreamAnalysis.Panel1.Controls.Add(this.labelDetectedSections);
            this.splitContainerStreamAnalysis.Panel1.Controls.Add(this.treeViewSections);
            // 
            // splitContainerStreamAnalysis.Panel2
            // 
            this.splitContainerStreamAnalysis.Panel2.Controls.Add(this.splitContainerStreamAnalysisDetails);
            this.splitContainerStreamAnalysis.Size = new System.Drawing.Size(732, 288);
            this.splitContainerStreamAnalysis.SplitterDistance = 344;
            this.splitContainerStreamAnalysis.TabIndex = 0;
            // 
            // labelDetectedSections
            // 
            this.labelDetectedSections.AutoSize = true;
            this.labelDetectedSections.Location = new System.Drawing.Point(4, 4);
            this.labelDetectedSections.Name = "labelDetectedSections";
            this.labelDetectedSections.Size = new System.Drawing.Size(93, 13);
            this.labelDetectedSections.TabIndex = 1;
            this.labelDetectedSections.Text = "Detected sections";
            // 
            // treeViewSections
            // 
            this.treeViewSections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewSections.Location = new System.Drawing.Point(3, 20);
            this.treeViewSections.Name = "treeViewSections";
            this.treeViewSections.Size = new System.Drawing.Size(338, 265);
            this.treeViewSections.TabIndex = 0;
            this.treeViewSections.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewSections_AfterSelect);
            // 
            // splitContainerStreamAnalysisDetails
            // 
            this.splitContainerStreamAnalysisDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerStreamAnalysisDetails.Location = new System.Drawing.Point(0, 0);
            this.splitContainerStreamAnalysisDetails.Name = "splitContainerStreamAnalysisDetails";
            this.splitContainerStreamAnalysisDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerStreamAnalysisDetails.Panel1
            // 
            this.splitContainerStreamAnalysisDetails.Panel1.Controls.Add(this.textBoxSectionData);
            this.splitContainerStreamAnalysisDetails.Panel1.Controls.Add(this.labelSectionData);
            // 
            // splitContainerStreamAnalysisDetails.Panel2
            // 
            this.splitContainerStreamAnalysisDetails.Panel2.Controls.Add(this.labelRawSectionData);
            this.splitContainerStreamAnalysisDetails.Panel2.Controls.Add(this.textBoxRawSectionData);
            this.splitContainerStreamAnalysisDetails.Size = new System.Drawing.Size(384, 288);
            this.splitContainerStreamAnalysisDetails.SplitterDistance = 139;
            this.splitContainerStreamAnalysisDetails.TabIndex = 0;
            // 
            // textBoxSectionData
            // 
            this.textBoxSectionData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSectionData.Location = new System.Drawing.Point(7, 21);
            this.textBoxSectionData.Multiline = true;
            this.textBoxSectionData.Name = "textBoxSectionData";
            this.textBoxSectionData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSectionData.Size = new System.Drawing.Size(374, 115);
            this.textBoxSectionData.TabIndex = 0;
            this.textBoxSectionData.WordWrap = false;
            // 
            // labelSectionData
            // 
            this.labelSectionData.AutoSize = true;
            this.labelSectionData.Location = new System.Drawing.Point(4, 4);
            this.labelSectionData.Name = "labelSectionData";
            this.labelSectionData.Size = new System.Drawing.Size(67, 13);
            this.labelSectionData.TabIndex = 0;
            this.labelSectionData.Text = "Section data";
            // 
            // labelRawSectionData
            // 
            this.labelRawSectionData.AutoSize = true;
            this.labelRawSectionData.Location = new System.Drawing.Point(4, 4);
            this.labelRawSectionData.Name = "labelRawSectionData";
            this.labelRawSectionData.Size = new System.Drawing.Size(90, 13);
            this.labelRawSectionData.TabIndex = 1;
            this.labelRawSectionData.Text = "Raw section data";
            // 
            // textBoxRawSectionData
            // 
            this.textBoxRawSectionData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRawSectionData.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxRawSectionData.Location = new System.Drawing.Point(7, 20);
            this.textBoxRawSectionData.Multiline = true;
            this.textBoxRawSectionData.Name = "textBoxRawSectionData";
            this.textBoxRawSectionData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxRawSectionData.Size = new System.Drawing.Size(374, 122);
            this.textBoxRawSectionData.TabIndex = 0;
            this.textBoxRawSectionData.WordWrap = false;
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // Mpeg2TsParserEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(761, 373);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Mpeg2TsParserEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MPEG2 TS parser settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Mpeg2TsParserEditor_FormClosing);
            this.Shown += new System.EventHandler(this.Mpeg2TsParserEditor_Shown);
            this.tabControl.ResumeLayout(false);
            this.tabPageBasicSettings.ResumeLayout(false);
            this.groupBoxDescription.ResumeLayout(false);
            this.groupBoxForceStreamIdentification.ResumeLayout(false);
            this.groupBoxForceStreamIdentification.PerformLayout();
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.tabPageFilteringProgramElements.ResumeLayout(false);
            this.tabPageFilteringProgramElements.PerformLayout();
            this.tabPageStreamAnalysis.ResumeLayout(false);
            this.splitContainerStreamAnalysis.Panel1.ResumeLayout(false);
            this.splitContainerStreamAnalysis.Panel1.PerformLayout();
            this.splitContainerStreamAnalysis.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStreamAnalysis)).EndInit();
            this.splitContainerStreamAnalysis.ResumeLayout(false);
            this.splitContainerStreamAnalysisDetails.Panel1.ResumeLayout(false);
            this.splitContainerStreamAnalysisDetails.Panel1.PerformLayout();
            this.splitContainerStreamAnalysisDetails.Panel2.ResumeLayout(false);
            this.splitContainerStreamAnalysisDetails.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStreamAnalysisDetails)).EndInit();
            this.splitContainerStreamAnalysisDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageBasicSettings;
        private System.Windows.Forms.TabPage tabPageFilteringProgramElements;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.CheckBox checkBoxAlignToMpeg2TsPacket;
        private System.Windows.Forms.CheckBox checkBoxSetNotScrambled;
        private System.Windows.Forms.CheckBox checkBoxDetectDiscontinuity;
        private System.Windows.Forms.GroupBox groupBoxForceStreamIdentification;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxProgramMapPID;
        private System.Windows.Forms.TextBox textBoxProgramNumber;
        private System.Windows.Forms.TextBox textBoxTransportStreamId;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label labelForceStreamIdentification;
        private System.Windows.Forms.TabPage tabPageStreamAnalysis;
        private System.Windows.Forms.SplitContainer splitContainerStreamAnalysis;
        private System.Windows.Forms.Label labelDetectedSections;
        private System.Windows.Forms.TreeView treeViewSections;
        private System.Windows.Forms.SplitContainer splitContainerStreamAnalysisDetails;
        private System.Windows.Forms.Label labelRawSectionData;
        private System.Windows.Forms.TextBox textBoxRawSectionData;
        private System.Windows.Forms.Label labelSectionData;
        private System.Windows.Forms.TextBox textBoxSectionData;
        private System.Windows.Forms.TabControl tabControlFilterProgramElements;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label labelTransportStreamProgramMapPID;
        private System.Windows.Forms.TextBox textBoxTransportStreamProgramMapSectionProgramNumber;
    }
}
