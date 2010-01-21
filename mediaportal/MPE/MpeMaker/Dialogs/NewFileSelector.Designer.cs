namespace MpeMaker.Dialogs
{
    partial class NewFileSelector
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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFileSelector));
          this.listView = new System.Windows.Forms.ListView();
          this.imageList = new System.Windows.Forms.ImageList(this.components);
          this.btn_ok = new System.Windows.Forms.Button();
          this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
          this.SuspendLayout();
          // 
          // listView
          // 
          this.listView.HideSelection = false;
          this.listView.LargeImageList = this.imageList;
          this.listView.Location = new System.Drawing.Point(-1, -1);
          this.listView.MultiSelect = false;
          this.listView.Name = "listView";
          this.listView.Size = new System.Drawing.Size(479, 256);
          this.listView.TabIndex = 0;
          this.listView.UseCompatibleStateImageBehavior = false;
          this.listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
          this.listView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
          // 
          // imageList
          // 
          this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
          this.imageList.ImageSize = new System.Drawing.Size(32, 32);
          this.imageList.TransparentColor = System.Drawing.Color.Transparent;
          // 
          // btn_ok
          // 
          this.btn_ok.Location = new System.Drawing.Point(387, 271);
          this.btn_ok.Name = "btn_ok";
          this.btn_ok.Size = new System.Drawing.Size(75, 23);
          this.btn_ok.TabIndex = 2;
          this.btn_ok.Text = "Ok";
          this.btn_ok.UseVisualStyleBackColor = true;
          this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
          // 
          // openFileDialog
          // 
          this.openFileDialog.FileName = "openFileDialog1";
          // 
          // NewFileSelector
          // 
          this.AcceptButton = this.btn_ok;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(474, 306);
          this.Controls.Add(this.btn_ok);
          this.Controls.Add(this.listView);
          this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "NewFileSelector";
          this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
          this.Text = "New Project";
          this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}