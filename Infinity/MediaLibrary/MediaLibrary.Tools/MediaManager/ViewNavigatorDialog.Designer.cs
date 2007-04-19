namespace MediaManager
{
    partial class ViewNavigatorDialog
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
            this.ItemsListView = new System.Windows.Forms.ListView();
            this.BackButton = new System.Windows.Forms.Button();
            this.AdvanceButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ItemsListView
            // 
            this.ItemsListView.Location = new System.Drawing.Point(1, 41);
            this.ItemsListView.Name = "ItemsListView";
            this.ItemsListView.Size = new System.Drawing.Size(292, 346);
            this.ItemsListView.TabIndex = 0;
            this.ItemsListView.UseCompatibleStateImageBehavior = false;
            this.ItemsListView.View = System.Windows.Forms.View.List;
            this.ItemsListView.ItemActivate += new System.EventHandler(this.ItemsListView_ItemActivate);
            this.ItemsListView.SelectedIndexChanged += new System.EventHandler(this.ItemsListView_SelectedIndexChanged);
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(45, 12);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(76, 23);
            this.BackButton.TabIndex = 1;
            this.BackButton.Text = "Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // AdvanceButton
            // 
            this.AdvanceButton.Location = new System.Drawing.Point(168, 12);
            this.AdvanceButton.Name = "AdvanceButton";
            this.AdvanceButton.Size = new System.Drawing.Size(75, 23);
            this.AdvanceButton.TabIndex = 2;
            this.AdvanceButton.Text = "Advance";
            this.AdvanceButton.UseVisualStyleBackColor = true;
            this.AdvanceButton.Click += new System.EventHandler(this.AdvanceButton_Click);
            // 
            // ViewNavigatorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 387);
            this.Controls.Add(this.AdvanceButton);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.ItemsListView);
            this.Name = "ViewNavigatorDialog";
            this.Text = "ViewNavigatorDialog";
            this.Load += new System.EventHandler(this.ViewNavigatorDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView ItemsListView;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button AdvanceButton;
    }
}