namespace TvMCheck
{
    partial class FormTvMCheck
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
            this.components = new System.ComponentModel.Container();
            this.btnConnect = new System.Windows.Forms.Button();
            this.treeViewStations = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDBPath = new System.Windows.Forms.TextBox();
            this.imageListSender = new System.Windows.Forms.ImageList(this.components);
            this.treeViewPrograms = new System.Windows.Forms.TreeView();
            this.lbChannels = new System.Windows.Forms.Label();
            this.lbPrograms = new System.Windows.Forms.Label();
            this.cbFavorites = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(625, 517);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Refresh";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.OnRefresh);
            // 
            // treeViewStations
            // 
            this.treeViewStations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.treeViewStations.ItemHeight = 24;
            this.treeViewStations.Location = new System.Drawing.Point(12, 35);
            this.treeViewStations.Name = "treeViewStations";
            this.treeViewStations.ShowNodeToolTips = true;
            this.treeViewStations.Size = new System.Drawing.Size(230, 476);
            this.treeViewStations.TabIndex = 1;
            this.treeViewStations.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewStations_NodeMouseClick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(666, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "00:00";
            // 
            // textBoxDBPath
            // 
            this.textBoxDBPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDBPath.Location = new System.Drawing.Point(248, 519);
            this.textBoxDBPath.Name = "textBoxDBPath";
            this.textBoxDBPath.Size = new System.Drawing.Size(371, 20);
            this.textBoxDBPath.TabIndex = 3;
            this.textBoxDBPath.Text = "D:\\Program Files\\TV Movie\\TV Movie ClickFinder\\tvdaten.mdb";
            // 
            // imageListSender
            // 
            this.imageListSender.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageListSender.ImageSize = new System.Drawing.Size(32, 22);
            this.imageListSender.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // treeViewPrograms
            // 
            this.treeViewPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewPrograms.ItemHeight = 24;
            this.treeViewPrograms.Location = new System.Drawing.Point(248, 35);
            this.treeViewPrograms.Name = "treeViewPrograms";
            this.treeViewPrograms.ShowNodeToolTips = true;
            this.treeViewPrograms.Size = new System.Drawing.Size(452, 476);
            this.treeViewPrograms.TabIndex = 4;
            // 
            // lbChannels
            // 
            this.lbChannels.AutoSize = true;
            this.lbChannels.Location = new System.Drawing.Point(12, 9);
            this.lbChannels.Name = "lbChannels";
            this.lbChannels.Size = new System.Drawing.Size(51, 13);
            this.lbChannels.TabIndex = 5;
            this.lbChannels.Text = "Channels";
            // 
            // lbPrograms
            // 
            this.lbPrograms.AutoSize = true;
            this.lbPrograms.Location = new System.Drawing.Point(269, 9);
            this.lbPrograms.Name = "lbPrograms";
            this.lbPrograms.Size = new System.Drawing.Size(51, 13);
            this.lbPrograms.TabIndex = 6;
            this.lbPrograms.Text = "Programs";
            // 
            // cbFavorites
            // 
            this.cbFavorites.AutoSize = true;
            this.cbFavorites.Checked = true;
            this.cbFavorites.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbFavorites.Location = new System.Drawing.Point(12, 521);
            this.cbFavorites.Name = "cbFavorites";
            this.cbFavorites.Size = new System.Drawing.Size(205, 17);
            this.cbFavorites.TabIndex = 7;
            this.cbFavorites.Text = "Only favorites (with EPG data present)";
            this.cbFavorites.UseVisualStyleBackColor = true;
            this.cbFavorites.CheckedChanged += new System.EventHandler(this.cbFavorites_CheckedChanged);
            // 
            // FormTVMViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 552);
            this.Controls.Add(this.cbFavorites);
            this.Controls.Add(this.lbPrograms);
            this.Controls.Add(this.lbChannels);
            this.Controls.Add(this.treeViewPrograms);
            this.Controls.Add(this.textBoxDBPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeViewStations);
            this.Controls.Add(this.btnConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormTVMViewer";
            this.Text = "TVM Debug Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TreeView treeViewStations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDBPath;
        private System.Windows.Forms.ImageList imageListSender;
        private System.Windows.Forms.TreeView treeViewPrograms;
        private System.Windows.Forms.Label lbChannels;
        private System.Windows.Forms.Label lbPrograms;
        private System.Windows.Forms.CheckBox cbFavorites;
    }
}

