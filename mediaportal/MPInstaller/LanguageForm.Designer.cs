namespace MediaPortal.MPInstaller
{
  partial class Form2
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
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.languageComboBox = new System.Windows.Forms.ComboBox();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.button3 = new System.Windows.Forms.Button();
      this.listView2 = new System.Windows.Forms.ListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.button5 = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.button8 = new System.Windows.Forms.Button();
      this.languageComboBox2 = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(12, 291);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(65, 21);
      this.button1.TabIndex = 0;
      this.button1.Text = "Add";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(180, 291);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(65, 21);
      this.button2.TabIndex = 1;
      this.button2.Text = "Remove";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // languageComboBox
      // 
      this.languageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.languageComboBox.FormattingEnabled = true;
      this.languageComboBox.Location = new System.Drawing.Point(83, 253);
      this.languageComboBox.Name = "languageComboBox";
      this.languageComboBox.Size = new System.Drawing.Size(154, 21);
      this.languageComboBox.TabIndex = 2;
      // 
      // listView1
      // 
      this.listView1.AllowColumnReorder = true;
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
      this.listView1.FullRowSelect = true;
      this.listView1.GridLines = true;
      this.listView1.LabelEdit = true;
      this.listView1.Location = new System.Drawing.Point(12, 11);
      this.listView1.Name = "listView1";
      this.listView1.ShowItemToolTips = true;
      this.listView1.Size = new System.Drawing.Size(372, 235);
      this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listView1.TabIndex = 3;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
      this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
      this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Id";
      this.columnHeader1.Width = 68;
      // 
      // columnHeader2
      // 
      this.columnHeader2.DisplayIndex = 2;
      this.columnHeader2.Text = "Value";
      this.columnHeader2.Width = 137;
      // 
      // columnHeader3
      // 
      this.columnHeader3.DisplayIndex = 1;
      this.columnHeader3.Text = "Language";
      this.columnHeader3.Width = 155;
      // 
      // textBox1
      // 
      this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox1.Location = new System.Drawing.Point(12, 253);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(62, 20);
      this.textBox1.TabIndex = 4;
      // 
      // textBox2
      // 
      this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox2.Location = new System.Drawing.Point(243, 253);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(130, 20);
      this.textBox2.TabIndex = 5;
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.Location = new System.Drawing.Point(545, 302);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(65, 21);
      this.button3.TabIndex = 6;
      this.button3.Text = "Save";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // listView2
      // 
      this.listView2.AllowColumnReorder = true;
      this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
      this.listView2.FullRowSelect = true;
      this.listView2.HideSelection = false;
      this.listView2.Location = new System.Drawing.Point(434, 12);
      this.listView2.Name = "listView2";
      this.listView2.Size = new System.Drawing.Size(227, 235);
      this.listView2.TabIndex = 9;
      this.listView2.UseCompatibleStateImageBehavior = false;
      this.listView2.View = System.Windows.Forms.View.Details;
      this.listView2.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView2_MouseDoubleClick);
      this.listView2.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Id";
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Value";
      this.columnHeader5.Width = 134;
      // 
      // button5
      // 
      this.button5.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.button5.Location = new System.Drawing.Point(390, 121);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(38, 23);
      this.button5.TabIndex = 10;
      this.button5.Text = "<-";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button6
      // 
      this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button6.Location = new System.Drawing.Point(83, 291);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(76, 21);
      this.button6.TabIndex = 11;
      this.button6.Text = "Add all lang.";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // button7
      // 
      this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button7.Location = new System.Drawing.Point(322, 291);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(65, 21);
      this.button7.TabIndex = 12;
      this.button7.Text = "Revert";
      this.button7.UseVisualStyleBackColor = true;
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // button8
      // 
      this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.button8.Location = new System.Drawing.Point(251, 291);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(65, 21);
      this.button8.TabIndex = 13;
      this.button8.Text = "Clear";
      this.button8.UseVisualStyleBackColor = true;
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // languageComboBox2
      // 
      this.languageComboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.languageComboBox2.FormattingEnabled = true;
      this.languageComboBox2.Location = new System.Drawing.Point(517, 253);
      this.languageComboBox2.Name = "languageComboBox2";
      this.languageComboBox2.Size = new System.Drawing.Size(144, 21);
      this.languageComboBox2.TabIndex = 14;
      this.languageComboBox2.SelectedIndexChanged += new System.EventHandler(this.languageComboBox2_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(433, 256);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(78, 13);
      this.label1.TabIndex = 15;
      this.label1.Text = "Ref. Language";
      // 
      // Form2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(673, 335);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.languageComboBox2);
      this.Controls.Add(this.button8);
      this.Controls.Add(this.button7);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.listView2);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.listView1);
      this.Controls.Add(this.languageComboBox);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.MaximizeBox = false;
      this.Name = "Form2";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "String Editor";
      this.Load += new System.EventHandler(this.Form2_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.ComboBox languageComboBox;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.TextBox textBox2;
      private System.Windows.Forms.Button button3;
      private System.Windows.Forms.ColumnHeader columnHeader3;
      private System.Windows.Forms.ListView listView2;
      private System.Windows.Forms.Button button5;
      private System.Windows.Forms.ColumnHeader columnHeader4;
      private System.Windows.Forms.ColumnHeader columnHeader5;
      private System.Windows.Forms.Button button6;
      private System.Windows.Forms.Button button7;
      private System.Windows.Forms.Button button8;
      private System.Windows.Forms.ComboBox languageComboBox2;
      private System.Windows.Forms.Label label1;
  }
}