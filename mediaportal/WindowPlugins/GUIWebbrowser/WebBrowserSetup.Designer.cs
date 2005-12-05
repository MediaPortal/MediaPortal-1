namespace MediaPortal.GUI.WebBrowser
{
    partial class WebBrowserSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebBrowserSetup));
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.PickFavoritesFolder = new System.Windows.Forms.Button();
            this.FavoritesFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.HomePage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(234, 64);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "&Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(315, 64);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "&Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.Favorites;
            // 
            // PickFavoritesFolder
            // 
            this.PickFavoritesFolder.Location = new System.Drawing.Point(315, 9);
            this.PickFavoritesFolder.Name = "PickFavoritesFolder";
            this.PickFavoritesFolder.Size = new System.Drawing.Size(75, 23);
            this.PickFavoritesFolder.TabIndex = 2;
            this.PickFavoritesFolder.Text = "Browse...";
            this.PickFavoritesFolder.UseVisualStyleBackColor = true;
            this.PickFavoritesFolder.Click += new System.EventHandler(this.PickFavoritesFolder_Click);
            // 
            // FavoritesFolder
            // 
            this.FavoritesFolder.Location = new System.Drawing.Point(103, 12);
            this.FavoritesFolder.Name = "FavoritesFolder";
            this.FavoritesFolder.Size = new System.Drawing.Size(206, 20);
            this.FavoritesFolder.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Favorites Folder:";
            // 
            // HomePage
            // 
            this.HomePage.Location = new System.Drawing.Point(103, 38);
            this.HomePage.Name = "HomePage";
            this.HomePage.Size = new System.Drawing.Size(206, 20);
            this.HomePage.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Home Page:";
            // 
            // WebBrowserSetup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(384, 85);
            this.Controls.Add(this.HomePage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FavoritesFolder);
            this.Controls.Add(this.PickFavoritesFolder);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WebBrowserSetup";
            this.Text = "Web Browser Setup";
            this.Load += new System.EventHandler(this.WebBrowserSetup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button PickFavoritesFolder;
        private System.Windows.Forms.TextBox FavoritesFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox HomePage;
        private System.Windows.Forms.Label label2;
    }
}