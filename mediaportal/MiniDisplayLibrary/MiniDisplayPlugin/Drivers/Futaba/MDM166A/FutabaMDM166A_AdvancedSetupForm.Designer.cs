namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    partial class FutabaMDM166A_AdvancedSetupForm
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
            this.pbCharDesign = new System.Windows.Forms.PictureBox();
            this.pbCharPreview = new System.Windows.Forms.PictureBox();
            this.txtNewCharacter = new System.Windows.Forms.TextBox();
            this.btnCharSave = new System.Windows.Forms.Button();
            this.btnExitAdvSetup = new System.Windows.Forms.Button();
            this.lblNewCharacter = new System.Windows.Forms.Label();
            this.btnCharClear = new System.Windows.Forms.Button();
            this.nudSpace = new System.Windows.Forms.NumericUpDown();
            this.lblSpace = new System.Windows.Forms.Label();
            this.cbSelectCharacter = new System.Windows.Forms.ComboBox();
            this.lblNoOfChars = new System.Windows.Forms.Label();
            this.lblSelectCharacter = new System.Windows.Forms.Label();
            this.lblEditor = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.txtNoOfChars = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbCharDesign)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCharPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpace)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCharDesign
            // 
            this.pbCharDesign.BackColor = System.Drawing.Color.Black;
            this.pbCharDesign.Location = new System.Drawing.Point(12, 26);
            this.pbCharDesign.Name = "pbCharDesign";
            this.pbCharDesign.Size = new System.Drawing.Size(150, 200);
            this.pbCharDesign.TabIndex = 0;
            this.pbCharDesign.TabStop = false;
            this.pbCharDesign.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pbCharDesign.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // pbCharPreview
            // 
            this.pbCharPreview.BackColor = System.Drawing.Color.Black;
            this.pbCharPreview.Location = new System.Drawing.Point(303, 26);
            this.pbCharPreview.Name = "pbCharPreview";
            this.pbCharPreview.Size = new System.Drawing.Size(40, 52);
            this.pbCharPreview.TabIndex = 1;
            this.pbCharPreview.TabStop = false;
            this.pbCharPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            // 
            // txtNewCharacter
            // 
            this.txtNewCharacter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.22642F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewCharacter.Location = new System.Drawing.Point(303, 181);
            this.txtNewCharacter.MaxLength = 1;
            this.txtNewCharacter.Multiline = true;
            this.txtNewCharacter.Name = "txtNewCharacter";
            this.txtNewCharacter.Size = new System.Drawing.Size(22, 28);
            this.txtNewCharacter.TabIndex = 2;
            this.txtNewCharacter.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnCharSave
            // 
            this.btnCharSave.Location = new System.Drawing.Point(12, 250);
            this.btnCharSave.Name = "btnCharSave";
            this.btnCharSave.Size = new System.Drawing.Size(63, 23);
            this.btnCharSave.TabIndex = 3;
            this.btnCharSave.Text = "Save";
            this.btnCharSave.UseCompatibleTextRendering = true;
            this.btnCharSave.UseVisualStyleBackColor = true;
            this.btnCharSave.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // btnExitAdvSetup
            // 
            this.btnExitAdvSetup.Location = new System.Drawing.Point(286, 250);
            this.btnExitAdvSetup.Name = "btnExitAdvSetup";
            this.btnExitAdvSetup.Size = new System.Drawing.Size(57, 23);
            this.btnExitAdvSetup.TabIndex = 4;
            this.btnExitAdvSetup.Text = "Exit";
            this.btnExitAdvSetup.UseCompatibleTextRendering = true;
            this.btnExitAdvSetup.UseVisualStyleBackColor = true;
            this.btnExitAdvSetup.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // lblNewCharacter
            // 
            this.lblNewCharacter.AutoSize = true;
            this.lblNewCharacter.Location = new System.Drawing.Point(186, 186);
            this.lblNewCharacter.Name = "lblNewCharacter";
            this.lblNewCharacter.Size = new System.Drawing.Size(113, 17);
            this.lblNewCharacter.TabIndex = 5;
            this.lblNewCharacter.Text = "Enter New Character:";
            this.lblNewCharacter.UseCompatibleTextRendering = true;
            // 
            // btnCharClear
            // 
            this.btnCharClear.Location = new System.Drawing.Point(108, 250);
            this.btnCharClear.Name = "btnCharClear";
            this.btnCharClear.Size = new System.Drawing.Size(54, 23);
            this.btnCharClear.TabIndex = 6;
            this.btnCharClear.Text = "Clear";
            this.btnCharClear.UseCompatibleTextRendering = true;
            this.btnCharClear.UseVisualStyleBackColor = true;
            this.btnCharClear.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // nudSpace
            // 
            this.nudSpace.Location = new System.Drawing.Point(211, 250);
            this.nudSpace.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudSpace.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSpace.Name = "nudSpace";
            this.nudSpace.Size = new System.Drawing.Size(37, 20);
            this.nudSpace.TabIndex = 7;
            this.nudSpace.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudSpace.Visible = false;
            this.nudSpace.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // lblSpace
            // 
            this.lblSpace.AutoSize = true;
            this.lblSpace.Location = new System.Drawing.Point(211, 230);
            this.lblSpace.Name = "lblSpace";
            this.lblSpace.Size = new System.Drawing.Size(49, 17);
            this.lblSpace.TabIndex = 8;
            this.lblSpace.Text = "Space %";
            this.lblSpace.UseCompatibleTextRendering = true;
            this.lblSpace.Visible = false;
            // 
            // cbSelectCharacter
            // 
            this.cbSelectCharacter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectCharacter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.22642F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbSelectCharacter.FormattingEnabled = true;
            this.cbSelectCharacter.Location = new System.Drawing.Point(303, 136);
            this.cbSelectCharacter.Name = "cbSelectCharacter";
            this.cbSelectCharacter.Size = new System.Drawing.Size(40, 28);
            this.cbSelectCharacter.TabIndex = 9;
            this.cbSelectCharacter.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // lblNoOfChars
            // 
            this.lblNoOfChars.AutoSize = true;
            this.lblNoOfChars.Location = new System.Drawing.Point(183, 99);
            this.lblNoOfChars.Name = "lblNoOfChars";
            this.lblNoOfChars.Size = new System.Drawing.Size(121, 17);
            this.lblNoOfChars.TabIndex = 10;
            this.lblNoOfChars.Text = "Number of Characters: ";
            this.lblNoOfChars.UseCompatibleTextRendering = true;
            // 
            // lblSelectCharacter
            // 
            this.lblSelectCharacter.AutoSize = true;
            this.lblSelectCharacter.Location = new System.Drawing.Point(167, 142);
            this.lblSelectCharacter.Name = "lblSelectCharacter";
            this.lblSelectCharacter.Size = new System.Drawing.Size(134, 17);
            this.lblSelectCharacter.TabIndex = 11;
            this.lblSelectCharacter.Text = "Select Existing Character:";
            this.lblSelectCharacter.UseCompatibleTextRendering = true;
            // 
            // lblEditor
            // 
            this.lblEditor.AutoSize = true;
            this.lblEditor.Location = new System.Drawing.Point(12, 9);
            this.lblEditor.Name = "lblEditor";
            this.lblEditor.Size = new System.Drawing.Size(34, 17);
            this.lblEditor.TabIndex = 12;
            this.lblEditor.Text = "Editor";
            this.lblEditor.UseCompatibleTextRendering = true;
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new System.Drawing.Point(253, 44);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(48, 17);
            this.lblPreview.TabIndex = 13;
            this.lblPreview.Text = "Preview:";
            this.lblPreview.UseCompatibleTextRendering = true;
            // 
            // txtNoOfChars
            // 
            this.txtNoOfChars.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.txtNoOfChars.Enabled = false;
            this.txtNoOfChars.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.22642F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNoOfChars.Location = new System.Drawing.Point(303, 93);
            this.txtNoOfChars.MaxLength = 1;
            this.txtNoOfChars.Multiline = true;
            this.txtNoOfChars.Name = "txtNoOfChars";
            this.txtNoOfChars.Size = new System.Drawing.Size(40, 28);
            this.txtNoOfChars.TabIndex = 14;
            // 
            // FutabaMDM166A_AdvancedSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 288);
            this.Controls.Add(this.txtNoOfChars);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.lblEditor);
            this.Controls.Add(this.lblSelectCharacter);
            this.Controls.Add(this.lblNoOfChars);
            this.Controls.Add(this.cbSelectCharacter);
            this.Controls.Add(this.lblSpace);
            this.Controls.Add(this.nudSpace);
            this.Controls.Add(this.btnCharClear);
            this.Controls.Add(this.lblNewCharacter);
            this.Controls.Add(this.btnExitAdvSetup);
            this.Controls.Add(this.btnCharSave);
            this.Controls.Add(this.txtNewCharacter);
            this.Controls.Add(this.pbCharPreview);
            this.Controls.Add(this.pbCharDesign);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FutabaMDM166A_AdvancedSetupForm";
            this.Text = "FutabaMDM166A Advanced Setup";
            this.Load += new System.EventHandler(this.DesignForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbCharDesign)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCharPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCharDesign;
        private System.Windows.Forms.PictureBox pbCharPreview;
        private System.Windows.Forms.TextBox txtNewCharacter;
        private System.Windows.Forms.Button btnCharSave;
        private System.Windows.Forms.Button btnExitAdvSetup;
        private System.Windows.Forms.Label lblNewCharacter;
        private System.Windows.Forms.Button btnCharClear;
        private System.Windows.Forms.NumericUpDown nudSpace;
        private System.Windows.Forms.Label lblSpace;
        private System.Windows.Forms.ComboBox cbSelectCharacter;
        private System.Windows.Forms.Label lblNoOfChars;
        private System.Windows.Forms.Label lblSelectCharacter;
        private System.Windows.Forms.Label lblEditor;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.TextBox txtNoOfChars;
    }
}