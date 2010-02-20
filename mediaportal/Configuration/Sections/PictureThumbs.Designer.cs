namespace MediaPortal.Configuration.Sections
{
  partial class PictureThumbs
  {
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
        this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.clearButton = new MediaPortal.UserInterface.Controls.MPButton();
        this.startButton = new MediaPortal.UserInterface.Controls.MPButton();
        this.sharesListBox = new System.Windows.Forms.CheckedListBox();
        this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.fileLabel = new MediaPortal.UserInterface.Controls.MPLabel();
        this.progressBar = new System.Windows.Forms.ProgressBar();
        this.groupBox1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.groupBox2);
        this.groupBox1.Controls.Add(this.clearButton);
        this.groupBox1.Controls.Add(this.startButton);
        this.groupBox1.Controls.Add(this.sharesListBox);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(3, 3);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(466, 402);
        this.groupBox1.TabIndex = 1;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Scan Picture Folders";
        // 
        // clearButton
        // 
        this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.clearButton.Location = new System.Drawing.Point(268, 283);
        this.clearButton.Name = "clearButton";
        this.clearButton.Size = new System.Drawing.Size(182, 52);
        this.clearButton.TabIndex = 3;
        this.clearButton.Text = "Reset Database";
        this.clearButton.UseVisualStyleBackColor = true;
        this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
        // 
        // startButton
        // 
        this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.startButton.Location = new System.Drawing.Point(16, 283);
        this.startButton.Name = "startButton";
        this.startButton.Size = new System.Drawing.Size(197, 52);
        this.startButton.TabIndex = 2;
        this.startButton.Text = "Update Database From Selected Shares";
        this.startButton.UseVisualStyleBackColor = true;
        this.startButton.Click += new System.EventHandler(this.startButton_Click);
        // 
        // sharesListBox
        // 
        this.sharesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.sharesListBox.CheckOnClick = true;
        this.sharesListBox.Location = new System.Drawing.Point(16, 24);
        this.sharesListBox.Name = "sharesListBox";
        this.sharesListBox.Size = new System.Drawing.Size(434, 244);
        this.sharesListBox.TabIndex = 0;
        // 
        // groupBox2
        // 
        this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox2.Controls.Add(this.fileLabel);
        this.groupBox2.Controls.Add(this.progressBar);
        this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox2.Location = new System.Drawing.Point(10, 341);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(447, 51);
        this.groupBox2.TabIndex = 5;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "Progress";
        // 
        // fileLabel
        // 
        this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.fileLabel.Location = new System.Drawing.Point(16, 23);
        this.fileLabel.Name = "fileLabel";
        this.fileLabel.Size = new System.Drawing.Size(415, 16);
        this.fileLabel.TabIndex = 0;
        // 
        // progressBar
        // 
        this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.progressBar.Location = new System.Drawing.Point(16, 23);
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new System.Drawing.Size(415, 16);
        this.progressBar.TabIndex = 1;
        // 
        // PictureThumbs
        // 
        this.Controls.Add(this.groupBox1);
        this.MinimumSize = new System.Drawing.Size(472, 408);
        this.Name = "PictureThumbs";
        this.Size = new System.Drawing.Size(472, 408);
        this.groupBox1.ResumeLayout(false);
        this.groupBox2.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton clearButton;
    private MediaPortal.UserInterface.Controls.MPButton startButton;
    private System.Windows.Forms.CheckedListBox sharesListBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel fileLabel;
    private System.Windows.Forms.ProgressBar progressBar;

  }
}
