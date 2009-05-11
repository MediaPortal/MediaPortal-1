namespace MediaPortal.MPInstaller
{
    partial class post_setup
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
          this.groupBox1 = new System.Windows.Forms.GroupBox();
          this.label2 = new System.Windows.Forms.Label();
          this.label1 = new System.Windows.Forms.Label();
          this.comboBox2 = new System.Windows.Forms.ComboBox();
          this.comboBox1 = new System.Windows.Forms.ComboBox();
          this.button1 = new System.Windows.Forms.Button();
          this.button2 = new System.Windows.Forms.Button();
          this.groupBox1.SuspendLayout();
          this.SuspendLayout();
          // 
          // groupBox1
          // 
          this.groupBox1.Controls.Add(this.label2);
          this.groupBox1.Controls.Add(this.label1);
          this.groupBox1.Controls.Add(this.comboBox2);
          this.groupBox1.Controls.Add(this.comboBox1);
          this.groupBox1.Location = new System.Drawing.Point(1, 5);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(308, 94);
          this.groupBox1.TabIndex = 0;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = "After setup";
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(6, 52);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(38, 13);
          this.label2.TabIndex = 3;
          this.label2.Text = "Option";
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(6, 12);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(37, 13);
          this.label1.TabIndex = 2;
          this.label1.Text = "Action";
          // 
          // comboBox2
          // 
          this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.comboBox2.FormattingEnabled = true;
          this.comboBox2.Location = new System.Drawing.Point(6, 67);
          this.comboBox2.Name = "comboBox2";
          this.comboBox2.Size = new System.Drawing.Size(296, 21);
          this.comboBox2.TabIndex = 1;
          // 
          // comboBox1
          // 
          this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
          this.comboBox1.FormattingEnabled = true;
          this.comboBox1.Items.AddRange(new object[] {
            "Run program",
            "Configure plugin"});
          this.comboBox1.Location = new System.Drawing.Point(8, 28);
          this.comboBox1.Name = "comboBox1";
          this.comboBox1.Size = new System.Drawing.Size(294, 21);
          this.comboBox1.TabIndex = 0;
          this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
          // 
          // button1
          // 
          this.button1.Location = new System.Drawing.Point(12, 272);
          this.button1.Name = "button1";
          this.button1.Size = new System.Drawing.Size(75, 23);
          this.button1.TabIndex = 1;
          this.button1.Text = "Ok";
          this.button1.UseVisualStyleBackColor = true;
          this.button1.Click += new System.EventHandler(this.button1_Click);
          // 
          // button2
          // 
          this.button2.Location = new System.Drawing.Point(226, 272);
          this.button2.Name = "button2";
          this.button2.Size = new System.Drawing.Size(75, 23);
          this.button2.TabIndex = 2;
          this.button2.Text = "Cancel";
          this.button2.UseVisualStyleBackColor = true;
          this.button2.Click += new System.EventHandler(this.button2_Click);
          // 
          // post_setup
          // 
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(313, 307);
          this.Controls.Add(this.button2);
          this.Controls.Add(this.button1);
          this.Controls.Add(this.groupBox1);
          this.Name = "post_setup";
          this.Text = "Setup - Post Setup";
          this.Load += new System.EventHandler(this.post_setup_Load);
          this.groupBox1.ResumeLayout(false);
          this.groupBox1.PerformLayout();
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}