namespace MediaManager
{
    partial class FolderlistDialog
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
            this.CanclButton = new System.Windows.Forms.Button();
            this.OkayButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.FoldersListView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // CanclButton
            // 
            this.CanclButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CanclButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CanclButton.Location = new System.Drawing.Point(215, 159);
            this.CanclButton.Name = "CanclButton";
            this.CanclButton.Size = new System.Drawing.Size(75, 23);
            this.CanclButton.TabIndex = 0;
            this.CanclButton.Text = "Cancel";
            this.CanclButton.UseVisualStyleBackColor = true;
            this.CanclButton.Click += new System.EventHandler(this.CanclButton_Click);
            // 
            // OkayButton
            // 
            this.OkayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkayButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkayButton.Location = new System.Drawing.Point(134, 159);
            this.OkayButton.Name = "OkayButton";
            this.OkayButton.Size = new System.Drawing.Size(75, 23);
            this.OkayButton.TabIndex = 1;
            this.OkayButton.Text = "Okay";
            this.OkayButton.UseVisualStyleBackColor = true;
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(0, 1);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 2;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(81, 1);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 3;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // FoldersListView
            // 
            this.FoldersListView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.FoldersListView.Location = new System.Drawing.Point(0, 30);
            this.FoldersListView.Name = "FoldersListView";
            this.FoldersListView.Size = new System.Drawing.Size(303, 123);
            this.FoldersListView.TabIndex = 4;
            this.FoldersListView.UseCompatibleStateImageBehavior = false;
            this.FoldersListView.View = System.Windows.Forms.View.List;
            // 
            // FolderlistDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 184);
            this.ControlBox = false;
            this.Controls.Add(this.FoldersListView);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.OkayButton);
            this.Controls.Add(this.CanclButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FolderlistDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Folderlist";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CanclButton;
        private System.Windows.Forms.Button OkayButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListView FoldersListView;
    }
}