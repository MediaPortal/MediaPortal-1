namespace MediaPortal.Support
{
  partial class LogCollector
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
      this.linkLabelQA = new System.Windows.Forms.LinkLabel();
      this.linkLabelHowTo = new System.Windows.Forms.LinkLabel();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelHeading = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.btnCollect = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // linkLabelQA
      // 
      this.linkLabelQA.AutoSize = true;
      this.linkLabelQA.Location = new System.Drawing.Point(23, 179);
      this.linkLabelQA.Name = "linkLabelQA";
      this.linkLabelQA.Size = new System.Drawing.Size(183, 13);
      this.linkLabelQA.TabIndex = 1;
      this.linkLabelQA.TabStop = true;
      this.linkLabelQA.Text = "MediaPortal Quality Assurance Forum";
      this.linkLabelQA.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelQA_LinkClicked);
      // 
      // linkLabelHowTo
      // 
      this.linkLabelHowTo.AutoSize = true;
      this.linkLabelHowTo.Location = new System.Drawing.Point(23, 157);
      this.linkLabelHowTo.Name = "linkLabelHowTo";
      this.linkLabelHowTo.Size = new System.Drawing.Size(101, 13);
      this.linkLabelHowTo.TabIndex = 2;
      this.linkLabelHowTo.TabStop = true;
      this.linkLabelHowTo.Text = "How to report a bug";
      this.linkLabelHowTo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHowTo_LinkClicked);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Top;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.BackColor = System.Drawing.Color.White;
      this.splitContainer1.Panel1.Controls.Add(this.label1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.BackColor = System.Drawing.Color.LightGray;
      this.splitContainer1.Panel2.Controls.Add(this.label2);
      this.splitContainer1.Panel2.Controls.Add(this.labelHeading);
      this.splitContainer1.Size = new System.Drawing.Size(292, 114);
      this.splitContainer1.SplitterDistance = 57;
      this.splitContainer1.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(45, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(206, 19);
      this.label1.TabIndex = 1;
      this.label1.Text = "MediaPortal Log Collector";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 31);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(256, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Hit \"Collect\" to collect the log files or \"Cancel\" to exit";
      // 
      // labelHeading
      // 
      this.labelHeading.AutoSize = true;
      this.labelHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHeading.Location = new System.Drawing.Point(10, 10);
      this.labelHeading.Name = "labelHeading";
      this.labelHeading.Size = new System.Drawing.Size(135, 13);
      this.labelHeading.TabIndex = 0;
      this.labelHeading.Text = "Mediaportal has exited";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(6, 133);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(113, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Some usefull links:";
      // 
      // btnCollect
      // 
      this.btnCollect.Location = new System.Drawing.Point(26, 216);
      this.btnCollect.Name = "btnCollect";
      this.btnCollect.Size = new System.Drawing.Size(75, 23);
      this.btnCollect.TabIndex = 5;
      this.btnCollect.Text = "Collect";
      this.btnCollect.UseVisualStyleBackColor = true;
      this.btnCollect.Click += new System.EventHandler(this.btnCollect_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(176, 216);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // LogCollector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 256);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnCollect);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.linkLabelHowTo);
      this.Controls.Add(this.linkLabelQA);
      this.Name = "LogCollector";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal Log Collector";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel1.PerformLayout();
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.LinkLabel linkLabelQA;
    private System.Windows.Forms.LinkLabel linkLabelHowTo;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label labelHeading;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnCollect;
    private System.Windows.Forms.Button btnCancel;

  }
}