namespace MpeMaker.Sections
{
    partial class ToolsUpdateXml
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_gen = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_browse2 = new System.Windows.Forms.Button();
            this.txt_list2 = new System.Windows.Forms.TextBox();
            this.btn_browse1 = new System.Windows.Forms.Button();
            this.add_list = new System.Windows.Forms.Button();
            this.txt_list1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(6, 22);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(568, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(580, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_gen
            // 
            this.btn_gen.Location = new System.Drawing.Point(6, 48);
            this.btn_gen.Name = "btn_gen";
            this.btn_gen.Size = new System.Drawing.Size(114, 23);
            this.btn_gen.TabIndex = 2;
            this.btn_gen.Text = "Generate XML";
            this.btn_gen.UseVisualStyleBackColor = true;
            this.btn_gen.Click += new System.EventHandler(this.btn_gen_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Xml file|*.xml|All filses|*.*";
            this.saveFileDialog1.OverwritePrompt = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btn_gen);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(14, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(670, 84);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add the current package to a list";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btn_browse2);
            this.groupBox2.Controls.Add(this.txt_list2);
            this.groupBox2.Controls.Add(this.btn_browse1);
            this.groupBox2.Controls.Add(this.add_list);
            this.groupBox2.Controls.Add(this.txt_list1);
            this.groupBox2.Location = new System.Drawing.Point(14, 104);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(670, 110);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add a another list to a list";
            // 
            // btn_browse2
            // 
            this.btn_browse2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_browse2.Location = new System.Drawing.Point(580, 45);
            this.btn_browse2.Name = "btn_browse2";
            this.btn_browse2.Size = new System.Drawing.Size(75, 23);
            this.btn_browse2.TabIndex = 4;
            this.btn_browse2.Text = "Browse";
            this.btn_browse2.UseVisualStyleBackColor = true;
            this.btn_browse2.Click += new System.EventHandler(this.btn_browse2_Click);
            // 
            // txt_list2
            // 
            this.txt_list2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_list2.Location = new System.Drawing.Point(6, 48);
            this.txt_list2.Name = "txt_list2";
            this.txt_list2.Size = new System.Drawing.Size(568, 20);
            this.txt_list2.TabIndex = 3;
            this.txt_list2.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btn_browse1
            // 
            this.btn_browse1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_browse1.Location = new System.Drawing.Point(580, 19);
            this.btn_browse1.Name = "btn_browse1";
            this.btn_browse1.Size = new System.Drawing.Size(75, 23);
            this.btn_browse1.TabIndex = 1;
            this.btn_browse1.Text = "Browse";
            this.btn_browse1.UseVisualStyleBackColor = true;
            this.btn_browse1.Click += new System.EventHandler(this.btn_browse1_Click);
            // 
            // add_list
            // 
            this.add_list.Location = new System.Drawing.Point(6, 81);
            this.add_list.Name = "add_list";
            this.add_list.Size = new System.Drawing.Size(114, 23);
            this.add_list.TabIndex = 2;
            this.add_list.Text = "Add list";
            this.add_list.UseVisualStyleBackColor = true;
            this.add_list.Click += new System.EventHandler(this.add_list_Click);
            // 
            // txt_list1
            // 
            this.txt_list1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_list1.Location = new System.Drawing.Point(6, 22);
            this.txt_list1.Name = "txt_list1";
            this.txt_list1.Size = new System.Drawing.Size(568, 20);
            this.txt_list1.TabIndex = 0;
            this.txt_list1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // ToolsUpdateXml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ToolsUpdateXml";
            this.Size = new System.Drawing.Size(700, 371);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_gen;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_browse2;
        private System.Windows.Forms.TextBox txt_list2;
        private System.Windows.Forms.Button btn_browse1;
        private System.Windows.Forms.Button add_list;
        private System.Windows.Forms.TextBox txt_list1;
    }
}
