namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    partial class FilterProgramNumberControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.checkBoxAllowFilteringProgramElements = new System.Windows.Forms.CheckBox();
            this.checkedListBoxLeaveProgramElements = new System.Windows.Forms.CheckedListBox();
            this.labelLeaveProgramElements = new System.Windows.Forms.Label();
            this.labelProgramElementPID = new System.Windows.Forms.Label();
            this.textBoxProgramElementPID = new System.Windows.Forms.TextBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxAllowFilteringProgramElements
            // 
            this.checkBoxAllowFilteringProgramElements.AutoSize = true;
            this.checkBoxAllowFilteringProgramElements.Location = new System.Drawing.Point(3, 3);
            this.checkBoxAllowFilteringProgramElements.Name = "checkBoxAllowFilteringProgramElements";
            this.checkBoxAllowFilteringProgramElements.Size = new System.Drawing.Size(173, 17);
            this.checkBoxAllowFilteringProgramElements.TabIndex = 0;
            this.checkBoxAllowFilteringProgramElements.Text = "Allow filtering program elements";
            this.checkBoxAllowFilteringProgramElements.UseVisualStyleBackColor = true;
            this.checkBoxAllowFilteringProgramElements.CheckedChanged += new System.EventHandler(this.checkBoxAllowFilteringProgramElements_CheckedChanged);
            // 
            // checkedListBoxLeaveProgramElements
            // 
            this.checkedListBoxLeaveProgramElements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkedListBoxLeaveProgramElements.CheckOnClick = true;
            this.checkedListBoxLeaveProgramElements.FormattingEnabled = true;
            this.checkedListBoxLeaveProgramElements.Location = new System.Drawing.Point(6, 66);
            this.checkedListBoxLeaveProgramElements.Name = "checkedListBoxLeaveProgramElements";
            this.checkedListBoxLeaveProgramElements.Size = new System.Drawing.Size(173, 124);
            this.checkedListBoxLeaveProgramElements.TabIndex = 5;
            this.checkedListBoxLeaveProgramElements.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxLeaveProgramElements_ItemCheck);
            // 
            // labelLeaveProgramElements
            // 
            this.labelLeaveProgramElements.AutoSize = true;
            this.labelLeaveProgramElements.Location = new System.Drawing.Point(3, 50);
            this.labelLeaveProgramElements.Name = "labelLeaveProgramElements";
            this.labelLeaveProgramElements.Size = new System.Drawing.Size(123, 13);
            this.labelLeaveProgramElements.TabIndex = 4;
            this.labelLeaveProgramElements.Text = "Leave program elements";
            // 
            // labelProgramElementPID
            // 
            this.labelProgramElementPID.AutoSize = true;
            this.labelProgramElementPID.Location = new System.Drawing.Point(3, 30);
            this.labelProgramElementPID.Name = "labelProgramElementPID";
            this.labelProgramElementPID.Size = new System.Drawing.Size(107, 13);
            this.labelProgramElementPID.TabIndex = 1;
            this.labelProgramElementPID.Text = "Program element PID";
            // 
            // textBoxProgramElementPID
            // 
            this.errorProvider.SetIconPadding(this.textBoxProgramElementPID, 4);
            this.textBoxProgramElementPID.Location = new System.Drawing.Point(116, 27);
            this.textBoxProgramElementPID.Name = "textBoxProgramElementPID";
            this.textBoxProgramElementPID.Size = new System.Drawing.Size(100, 20);
            this.textBoxProgramElementPID.TabIndex = 2;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(240, 25);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // FilterProgramMapPIDControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.textBoxProgramElementPID);
            this.Controls.Add(this.labelProgramElementPID);
            this.Controls.Add(this.labelLeaveProgramElements);
            this.Controls.Add(this.checkedListBoxLeaveProgramElements);
            this.Controls.Add(this.checkBoxAllowFilteringProgramElements);
            this.Name = "FilterProgramMapPIDControl";
            this.Size = new System.Drawing.Size(359, 195);
            this.Load += new System.EventHandler(this.FilterProgramMapPIDControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLeaveProgramElements;
        private System.Windows.Forms.Label labelProgramElementPID;
        private System.Windows.Forms.TextBox textBoxProgramElementPID;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.CheckBox checkBoxAllowFilteringProgramElements;
        private System.Windows.Forms.CheckedListBox checkedListBoxLeaveProgramElements;

    }
}
