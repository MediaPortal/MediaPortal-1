﻿namespace MpeMaker.Sections
{
    partial class ActionEdit
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
            this.cmb_type = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_params = new System.Windows.Forms.Button();
            this.lbl_description = new System.Windows.Forms.Label();
            this.cmb_group = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmb_execute = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmb_execution_condition = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmb_type
            // 
            this.cmb_type.Enabled = false;
            this.cmb_type.FormattingEnabled = true;
            this.cmb_type.Location = new System.Drawing.Point(12, 24);
            this.cmb_type.Name = "cmb_type";
            this.cmb_type.Size = new System.Drawing.Size(308, 21);
            this.cmb_type.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(12, 64);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(308, 20);
            this.textBoxName.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Name";
            // 
            // btn_ok
            // 
            this.btn_ok.Location = new System.Drawing.Point(12, 325);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 23);
            this.btn_ok.TabIndex = 0;
            this.btn_ok.Text = "Ok";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(245, 325);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // btn_params
            // 
            this.btn_params.Location = new System.Drawing.Point(111, 211);
            this.btn_params.Name = "btn_params";
            this.btn_params.Size = new System.Drawing.Size(75, 23);
            this.btn_params.TabIndex = 4;
            this.btn_params.Text = "Edit Params";
            this.btn_params.UseVisualStyleBackColor = true;
            this.btn_params.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbl_description
            // 
            this.lbl_description.Location = new System.Drawing.Point(12, 237);
            this.lbl_description.Name = "lbl_description";
            this.lbl_description.Size = new System.Drawing.Size(308, 68);
            this.lbl_description.TabIndex = 5;
            this.lbl_description.Text = "label2";
            // 
            // cmb_group
            // 
            this.cmb_group.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_group.FormattingEnabled = true;
            this.cmb_group.Location = new System.Drawing.Point(12, 103);
            this.cmb_group.Name = "cmb_group";
            this.cmb_group.Size = new System.Drawing.Size(308, 21);
            this.cmb_group.TabIndex = 6;
            this.cmb_group.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Condition group";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 168);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Execute when";
            // 
            // cmb_execute
            // 
            this.cmb_execute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_execute.FormattingEnabled = true;
            this.cmb_execute.Items.AddRange(new object[] {
            " Before Wizard Screen Show",
            " After Wizard Screen Show",
            " After Wizard Screen Hide"});
            this.cmb_execute.Location = new System.Drawing.Point(12, 184);
            this.cmb_execute.Name = "cmb_execute";
            this.cmb_execute.Size = new System.Drawing.Size(308, 21);
            this.cmb_execute.TabIndex = 9;
            this.cmb_execute.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Execution condition";
            // 
            // cmb_execution_condition
            // 
            this.cmb_execution_condition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_execution_condition.FormattingEnabled = true;
            this.cmb_execution_condition.Location = new System.Drawing.Point(12, 144);
            this.cmb_execution_condition.Name = "cmb_execution_condition";
            this.cmb_execution_condition.Size = new System.Drawing.Size(308, 21);
            this.cmb_execution_condition.TabIndex = 11;
            this.cmb_execution_condition.SelectedIndexChanged += new System.EventHandler(this.cmb_execution_condition_SelectedIndexChanged);
            // 
            // ActionEdit
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(332, 360);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.cmb_execution_condition);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmb_execute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmb_group);
            this.Controls.Add(this.lbl_description);
            this.Controls.Add(this.btn_params);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmb_type);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ActionEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Action";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox cmb_type;
        private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxName;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button btn_ok;
    private System.Windows.Forms.Button btn_cancel;
    private System.Windows.Forms.Button btn_params;
    private System.Windows.Forms.Label lbl_description;
    private System.Windows.Forms.ComboBox cmb_group;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cmb_execute;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cmb_execution_condition;
  }
}