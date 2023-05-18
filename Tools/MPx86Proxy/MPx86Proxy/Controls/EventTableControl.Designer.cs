namespace MPx86Proxy.Controls
{
    partial class EventTableControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventTableControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonSystem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonInf = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonWarn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonErr = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClose = new MPx86Proxy.Controls.ToolStripButtonClose();
            this.toolStripButtonMinMax = new MPx86Proxy.Controls.ToolStripButtonMinMax();
            this.statusStripLastMessage = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelLastMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.table = new System.Windows.Forms.DataGridView();
            this.EventTable_Icon = new System.Windows.Forms.DataGridViewImageColumn();
            this.EventTable_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EventTable_Event = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip.SuspendLayout();
            this.statusStripLastMessage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSystem,
            this.toolStripSeparator,
            this.toolStripButtonInf,
            this.toolStripButtonWarn,
            this.toolStripButtonErr,
            this.toolStripSeparator1,
            this.toolStripButtonClear,
            this.toolStripButtonClose,
            this.toolStripButtonMinMax});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(480, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // toolStripButtonSystem
            // 
            this.toolStripButtonSystem.CheckOnClick = true;
            this.toolStripButtonSystem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSystem.Enabled = false;
            this.toolStripButtonSystem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSystem.Image")));
            this.toolStripButtonSystem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSystem.Name = "toolStripButtonSystem";
            this.toolStripButtonSystem.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSystem.Text = "System Messages Only";
            this.toolStripButtonSystem.ToolTipText = "System Messages Only";
            this.toolStripButtonSystem.Click += new System.EventHandler(this.toolStripButtonSystem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonInf
            // 
            this.toolStripButtonInf.Checked = true;
            this.toolStripButtonInf.CheckOnClick = true;
            this.toolStripButtonInf.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonInf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonInf.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonInf.Image")));
            this.toolStripButtonInf.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonInf.Name = "toolStripButtonInf";
            this.toolStripButtonInf.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonInf.Text = "Messages";
            this.toolStripButtonInf.Click += new System.EventHandler(this.toolStripButtonInf_Click);
            // 
            // toolStripButtonWarn
            // 
            this.toolStripButtonWarn.Checked = true;
            this.toolStripButtonWarn.CheckOnClick = true;
            this.toolStripButtonWarn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonWarn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWarn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWarn.Image")));
            this.toolStripButtonWarn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonWarn.Name = "toolStripButtonWarn";
            this.toolStripButtonWarn.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonWarn.Text = "Warning";
            this.toolStripButtonWarn.Click += new System.EventHandler(this.toolStripButtonWarn_Click);
            // 
            // toolStripButtonErr
            // 
            this.toolStripButtonErr.Checked = true;
            this.toolStripButtonErr.CheckOnClick = true;
            this.toolStripButtonErr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonErr.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonErr.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonErr.Image")));
            this.toolStripButtonErr.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonErr.Name = "toolStripButtonErr";
            this.toolStripButtonErr.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonErr.Text = "Errors";
            this.toolStripButtonErr.Click += new System.EventHandler(this.toolStripButtonErr_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonClear
            // 
            this.toolStripButtonClear.Checked = true;
            this.toolStripButtonClear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClear.Image = global::MPx86Proxy.Properties.Resources.erase;
            this.toolStripButtonClear.ImageTransparentColor = System.Drawing.Color.Gray;
            this.toolStripButtonClear.Name = "toolStripButtonClear";
            this.toolStripButtonClear.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonClear.Text = "Clear";
            this.toolStripButtonClear.Click += new System.EventHandler(this.toolStripButtonClear_Click);
            // 
            // toolStripButtonClose
            // 
            this.toolStripButtonClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonClose.AutoSize = false;
            this.toolStripButtonClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClose.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClose.Image")));
            this.toolStripButtonClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClose.Name = "toolStripButtonClose";
            this.toolStripButtonClose.Size = new System.Drawing.Size(18, 18);
            this.toolStripButtonClose.Text = "Close";
            this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
            // 
            // toolStripButtonMinMax
            // 
            this.toolStripButtonMinMax.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonMinMax.AutoSize = false;
            this.toolStripButtonMinMax.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMinMax.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMinMax.Image")));
            this.toolStripButtonMinMax.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMinMax.Name = "toolStripButtonMinMax";
            this.toolStripButtonMinMax.Size = new System.Drawing.Size(18, 18);
            this.toolStripButtonMinMax.Text = "Min/Max";
            this.toolStripButtonMinMax.Click += new System.EventHandler(this.toolStripButtonMinMax_Click);
            // 
            // statusStripLastMessage
            // 
            this.statusStripLastMessage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelLastMessage});
            this.statusStripLastMessage.Location = new System.Drawing.Point(0, 277);
            this.statusStripLastMessage.Name = "statusStripLastMessage";
            this.statusStripLastMessage.Size = new System.Drawing.Size(480, 24);
            this.statusStripLastMessage.TabIndex = 1;
            this.statusStripLastMessage.Text = "statusStrip1";
            // 
            // toolStripStatusLabelLastMessage
            // 
            this.toolStripStatusLabelLastMessage.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStatusLabelLastMessage.Image")));
            this.toolStripStatusLabelLastMessage.Name = "toolStripStatusLabelLastMessage";
            this.toolStripStatusLabelLastMessage.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.toolStripStatusLabelLastMessage.Size = new System.Drawing.Size(166, 19);
            this.toolStripStatusLabelLastMessage.Text = " 0:00:00  Message";
            // 
            // table
            // 
            this.table.AllowUserToAddRows = false;
            this.table.AllowUserToDeleteRows = false;
            this.table.AllowUserToResizeColumns = false;
            this.table.AllowUserToResizeRows = false;
            this.table.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.table.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EventTable_Icon,
            this.EventTable_Time,
            this.EventTable_Event});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.table.DefaultCellStyle = dataGridViewCellStyle3;
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(0, 25);
            this.table.Name = "table";
            this.table.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.table.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.table.RowHeadersVisible = false;
            this.table.RowHeadersWidth = 30;
            this.table.RowTemplate.Height = 19;
            this.table.RowTemplate.ReadOnly = true;
            this.table.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.table.Size = new System.Drawing.Size(480, 252);
            this.table.TabIndex = 3;
            this.table.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.table_CellPainting);
            // 
            // EventTable_Icon
            // 
            this.EventTable_Icon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.NullValue = null;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.EventTable_Icon.DefaultCellStyle = dataGridViewCellStyle2;
            this.EventTable_Icon.FillWeight = 5F;
            this.EventTable_Icon.HeaderText = "";
            this.EventTable_Icon.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Stretch;
            this.EventTable_Icon.MinimumWidth = 20;
            this.EventTable_Icon.Name = "EventTable_Icon";
            this.EventTable_Icon.ReadOnly = true;
            this.EventTable_Icon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EventTable_Icon.Width = 20;
            // 
            // EventTable_Time
            // 
            this.EventTable_Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.EventTable_Time.FillWeight = 65F;
            this.EventTable_Time.HeaderText = "Time";
            this.EventTable_Time.MinimumWidth = 70;
            this.EventTable_Time.Name = "EventTable_Time";
            this.EventTable_Time.ReadOnly = true;
            this.EventTable_Time.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EventTable_Time.Width = 70;
            // 
            // EventTable_Event
            // 
            this.EventTable_Event.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.EventTable_Event.FillWeight = 390F;
            this.EventTable_Event.HeaderText = "Event";
            this.EventTable_Event.MinimumWidth = 200;
            this.EventTable_Event.Name = "EventTable_Event";
            this.EventTable_Event.ReadOnly = true;
            this.EventTable_Event.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "Information");
            this.imageList.Images.SetKeyName(1, "Warning");
            this.imageList.Images.SetKeyName(2, "Error");
            // 
            // EventTableControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.table);
            this.Controls.Add(this.statusStripLastMessage);
            this.Controls.Add(this.toolStrip);
            this.Name = "EventTableControl";
            this.Size = new System.Drawing.Size(480, 301);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusStripLastMessage.ResumeLayout(false);
            this.statusStripLastMessage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonSystem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton toolStripButtonWarn;
        private System.Windows.Forms.ToolStripButton toolStripButtonErr;
        private System.Windows.Forms.ToolStripButton toolStripButtonInf;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonClear;
        private ToolStripButtonClose toolStripButtonClose;
        private ToolStripButtonMinMax toolStripButtonMinMax;
        private System.Windows.Forms.StatusStrip statusStripLastMessage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLastMessage;
        private System.Windows.Forms.DataGridView table;
        private System.Windows.Forms.DataGridViewImageColumn EventTable_Icon;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventTable_Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventTable_Event;
        private System.Windows.Forms.ImageList imageList;
    }
}
