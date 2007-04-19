namespace MediaManager
{
    partial class ImportEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.ImportNameText = new System.Windows.Forms.TextBox();
            this.iMLImportBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.ImportModeText = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PluginChoicesComboBox = new System.Windows.Forms.ComboBox();
            this.ImportAbout = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.RunNow = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.WakeUpCheckBox = new System.Windows.Forms.CheckBox();
            this.RunMissedCheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.ScheduleNeverRadio = new System.Windows.Forms.RadioButton();
            this.ScheduleDailyRadio = new System.Windows.Forms.RadioButton();
            this.ScheduleAtStartRadio = new System.Windows.Forms.RadioButton();
            this.ScheduleIntervalRadio = new System.Windows.Forms.RadioButton();
            this.ScheduleIntervalUpDown = new System.Windows.Forms.NumericUpDown();
            this.ScheduleIntervalMinOrHourCombo = new System.Windows.Forms.ComboBox();
            this.ScheduleIntervalDateTimePicker = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.iMLImportBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleIntervalUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter a name for this import";
            // 
            // ImportNameText
            // 
            this.ImportNameText.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.iMLImportBindingSource, "Name", true));
            this.ImportNameText.Location = new System.Drawing.Point(13, 30);
            this.ImportNameText.Name = "ImportNameText";
            this.ImportNameText.Size = new System.Drawing.Size(385, 20);
            this.ImportNameText.TabIndex = 1;
            this.ImportNameText.Text = "New Import";
            // 
            // iMLImportBindingSource
            // 
            this.iMLImportBindingSource.DataSource = typeof(MediaLibrary.IMLImport);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Choose the import mode";
            // 
            // ImportModeText
            // 
            this.ImportModeText.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.iMLImportBindingSource, "Mode", true));
            this.ImportModeText.DisplayMember = "Display";
            this.ImportModeText.FormattingEnabled = true;
            this.ImportModeText.Location = new System.Drawing.Point(13, 74);
            this.ImportModeText.Name = "ImportModeText";
            this.ImportModeText.Size = new System.Drawing.Size(385, 21);
            this.ImportModeText.TabIndex = 3;
            this.ImportModeText.ValueMember = "Value";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(191, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Choose the plugin to perform the import";
            // 
            // PluginChoicesComboBox
            // 
            this.PluginChoicesComboBox.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.iMLImportBindingSource, "PluginID", true));
            this.PluginChoicesComboBox.FormattingEnabled = true;
            this.PluginChoicesComboBox.Location = new System.Drawing.Point(13, 119);
            this.PluginChoicesComboBox.Name = "PluginChoicesComboBox";
            this.PluginChoicesComboBox.Size = new System.Drawing.Size(303, 21);
            this.PluginChoicesComboBox.TabIndex = 5;
            this.PluginChoicesComboBox.SelectedIndexChanged += new System.EventHandler(this.PluginChoicesComboBox_SelectedIndexChanged);
            // 
            // ImportAbout
            // 
            this.ImportAbout.Location = new System.Drawing.Point(322, 119);
            this.ImportAbout.Name = "ImportAbout";
            this.ImportAbout.Size = new System.Drawing.Size(75, 23);
            this.ImportAbout.TabIndex = 6;
            this.ImportAbout.Text = "About";
            this.ImportAbout.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Set the properties for this plugin";
            // 
            // RunNow
            // 
            this.RunNow.Enabled = false;
            this.RunNow.Location = new System.Drawing.Point(13, 437);
            this.RunNow.Name = "RunNow";
            this.RunNow.Size = new System.Drawing.Size(75, 23);
            this.RunNow.TabIndex = 15;
            this.RunNow.Text = "Run Now";
            this.RunNow.UseVisualStyleBackColor = true;
            this.RunNow.Click += new System.EventHandler(this.RunNow_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(321, 436);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 16;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Enabled = false;
            this.OK.Location = new System.Drawing.Point(240, 436);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 17;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // WakeUpCheckBox
            // 
            this.WakeUpCheckBox.AutoSize = true;
            this.WakeUpCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.iMLImportBindingSource, "WakeUp", true));
            this.WakeUpCheckBox.Location = new System.Drawing.Point(13, 404);
            this.WakeUpCheckBox.Name = "WakeUpCheckBox";
            this.WakeUpCheckBox.Size = new System.Drawing.Size(117, 17);
            this.WakeUpCheckBox.TabIndex = 20;
            this.WakeUpCheckBox.Text = "Wake-up computer";
            this.WakeUpCheckBox.UseVisualStyleBackColor = true;
            // 
            // RunMissedCheckBox
            // 
            this.RunMissedCheckBox.AutoSize = true;
            this.RunMissedCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.iMLImportBindingSource, "RunMissed", true));
            this.RunMissedCheckBox.Location = new System.Drawing.Point(163, 404);
            this.RunMissedCheckBox.Name = "RunMissedCheckBox";
            this.RunMissedCheckBox.Size = new System.Drawing.Size(214, 17);
            this.RunMissedCheckBox.TabIndex = 21;
            this.RunMissedCheckBox.Text = "Run at startup if schedule time is missed";
            this.RunMissedCheckBox.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(13, 163);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(385, 162);
            this.panel1.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 328);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(196, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Schedule this import to run automatically";
            // 
            // ScheduleNeverRadio
            // 
            this.ScheduleNeverRadio.AutoSize = true;
            this.ScheduleNeverRadio.Checked = true;
            this.ScheduleNeverRadio.Location = new System.Drawing.Point(13, 345);
            this.ScheduleNeverRadio.Name = "ScheduleNeverRadio";
            this.ScheduleNeverRadio.Size = new System.Drawing.Size(54, 17);
            this.ScheduleNeverRadio.TabIndex = 11;
            this.ScheduleNeverRadio.TabStop = true;
            this.ScheduleNeverRadio.Text = "Never";
            this.ScheduleNeverRadio.UseVisualStyleBackColor = true;
            this.ScheduleNeverRadio.CheckedChanged += new System.EventHandler(this.ScheduleNeverRadio_CheckedChanged);
            // 
            // ScheduleDailyRadio
            // 
            this.ScheduleDailyRadio.AutoSize = true;
            this.ScheduleDailyRadio.Location = new System.Drawing.Point(13, 369);
            this.ScheduleDailyRadio.Name = "ScheduleDailyRadio";
            this.ScheduleDailyRadio.Size = new System.Drawing.Size(84, 17);
            this.ScheduleDailyRadio.TabIndex = 12;
            this.ScheduleDailyRadio.TabStop = true;
            this.ScheduleDailyRadio.Text = "Every day at";
            this.ScheduleDailyRadio.UseVisualStyleBackColor = true;
            this.ScheduleDailyRadio.CheckedChanged += new System.EventHandler(this.ScheduleDailyRadio_CheckedChanged);
            // 
            // ScheduleAtStartRadio
            // 
            this.ScheduleAtStartRadio.AutoSize = true;
            this.ScheduleAtStartRadio.Location = new System.Drawing.Point(199, 345);
            this.ScheduleAtStartRadio.Name = "ScheduleAtStartRadio";
            this.ScheduleAtStartRadio.Size = new System.Drawing.Size(174, 17);
            this.ScheduleAtStartRadio.TabIndex = 13;
            this.ScheduleAtStartRadio.TabStop = true;
            this.ScheduleAtStartRadio.Text = "Every time the application starts";
            this.ScheduleAtStartRadio.UseVisualStyleBackColor = true;
            this.ScheduleAtStartRadio.CheckedChanged += new System.EventHandler(this.ScheduleAtStartRadio_CheckedChanged);
            // 
            // ScheduleIntervalRadio
            // 
            this.ScheduleIntervalRadio.AutoSize = true;
            this.ScheduleIntervalRadio.Location = new System.Drawing.Point(199, 369);
            this.ScheduleIntervalRadio.Name = "ScheduleIntervalRadio";
            this.ScheduleIntervalRadio.Size = new System.Drawing.Size(52, 17);
            this.ScheduleIntervalRadio.TabIndex = 14;
            this.ScheduleIntervalRadio.TabStop = true;
            this.ScheduleIntervalRadio.Text = "Every";
            this.ScheduleIntervalRadio.UseVisualStyleBackColor = true;
            this.ScheduleIntervalRadio.CheckedChanged += new System.EventHandler(this.ScheduleIntervalRadio_CheckedChanged);
            // 
            // ScheduleIntervalUpDown
            // 
            this.ScheduleIntervalUpDown.Location = new System.Drawing.Point(257, 369);
            this.ScheduleIntervalUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScheduleIntervalUpDown.Name = "ScheduleIntervalUpDown";
            this.ScheduleIntervalUpDown.Size = new System.Drawing.Size(40, 20);
            this.ScheduleIntervalUpDown.TabIndex = 18;
            this.ScheduleIntervalUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ScheduleIntervalUpDown.ValueChanged += new System.EventHandler(this.ScheduleIntervalUpDown_ValueChanged);
            // 
            // ScheduleIntervalMinOrHourCombo
            // 
            this.ScheduleIntervalMinOrHourCombo.FormattingEnabled = true;
            this.ScheduleIntervalMinOrHourCombo.Items.AddRange(new object[] {
            "minutes",
            "hours"});
            this.ScheduleIntervalMinOrHourCombo.Location = new System.Drawing.Point(303, 369);
            this.ScheduleIntervalMinOrHourCombo.Name = "ScheduleIntervalMinOrHourCombo";
            this.ScheduleIntervalMinOrHourCombo.Size = new System.Drawing.Size(92, 21);
            this.ScheduleIntervalMinOrHourCombo.TabIndex = 19;
            this.ScheduleIntervalMinOrHourCombo.SelectedIndexChanged += new System.EventHandler(this.ScheduleIntervalMinOrHourCombo_SelectedIndexChanged);
            // 
            // ScheduleIntervalDateTimePicker
            // 
            this.ScheduleIntervalDateTimePicker.CustomFormat = "HH:mm";
            this.ScheduleIntervalDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ScheduleIntervalDateTimePicker.Location = new System.Drawing.Point(104, 369);
            this.ScheduleIntervalDateTimePicker.Name = "ScheduleIntervalDateTimePicker";
            this.ScheduleIntervalDateTimePicker.ShowUpDown = true;
            this.ScheduleIntervalDateTimePicker.Size = new System.Drawing.Size(64, 20);
            this.ScheduleIntervalDateTimePicker.TabIndex = 23;
            this.ScheduleIntervalDateTimePicker.ValueChanged += new System.EventHandler(this.ScheduleIntervalDateTimePicker_ValueChanged);
            // 
            // ImportEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 472);
            this.Controls.Add(this.ScheduleIntervalDateTimePicker);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.RunMissedCheckBox);
            this.Controls.Add(this.WakeUpCheckBox);
            this.Controls.Add(this.ScheduleIntervalMinOrHourCombo);
            this.Controls.Add(this.ScheduleIntervalUpDown);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.RunNow);
            this.Controls.Add(this.ScheduleIntervalRadio);
            this.Controls.Add(this.ScheduleAtStartRadio);
            this.Controls.Add(this.ScheduleDailyRadio);
            this.Controls.Add(this.ScheduleNeverRadio);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ImportAbout);
            this.Controls.Add(this.PluginChoicesComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ImportModeText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ImportNameText);
            this.Controls.Add(this.label1);
            this.Name = "ImportEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ImportEditor";
            this.Load += new System.EventHandler(this.ImportEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.iMLImportBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleIntervalUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ImportNameText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ImportModeText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox PluginChoicesComboBox;
        private System.Windows.Forms.Button ImportAbout;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button RunNow;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.CheckBox WakeUpCheckBox;
        private System.Windows.Forms.CheckBox RunMissedCheckBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.BindingSource iMLImportBindingSource;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton ScheduleNeverRadio;
        private System.Windows.Forms.RadioButton ScheduleDailyRadio;
        private System.Windows.Forms.RadioButton ScheduleAtStartRadio;
        private System.Windows.Forms.RadioButton ScheduleIntervalRadio;
        private System.Windows.Forms.NumericUpDown ScheduleIntervalUpDown;
        private System.Windows.Forms.ComboBox ScheduleIntervalMinOrHourCombo;
        private System.Windows.Forms.DateTimePicker ScheduleIntervalDateTimePicker;
    }
}