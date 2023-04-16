using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MPx86Proxy.Controls
{
    public partial class EventTableControl : UserControl
    {
        private const int _TABLE_ROWS_LIMIT_MIN = 5;
        private const int _TABLE_ROWS_LIMIT_MAX_DEFAULT = 50;

        private List<EventTableMessage> _Messages = new List<EventTableMessage>();

        private bool _Initialized = false;

        private static System.Collections.Hashtable _Bitmaps = new System.Collections.Hashtable(8);

        public EventTableMessageTypeEnum Filter
        {
            get { return this._TypeMask; }
            set
            {
                this._TypeMask = value;
                this.toolStripButtonSystem.Checked = this.toolStripButtonSystem.Enabled && (value & EventTableMessageTypeEnum.System) != EventTableMessageTypeEnum.None;
                this.toolStripButtonInf.Checked = (value & EventTableMessageTypeEnum.Info) != EventTableMessageTypeEnum.None;
                this.toolStripButtonWarn.Checked = (value & EventTableMessageTypeEnum.Warning) != EventTableMessageTypeEnum.None;
                this.toolStripButtonErr.Checked = (value & EventTableMessageTypeEnum.Error) != EventTableMessageTypeEnum.None;
                this.reloadTable();
            }
        }private EventTableMessageTypeEnum _TypeMask = EventTableMessageTypeEnum.All;

        public int TableRowsLimit
        {
            get { return this._TableRowsLimit; }
            set
            {
                if (value < _TABLE_ROWS_LIMIT_MIN)
                    this._TableRowsLimit = _TABLE_ROWS_LIMIT_MIN;
                else
                    this._TableRowsLimit = value;

                this.reloadTable();
            }
        }private int _TableRowsLimit = _TABLE_ROWS_LIMIT_MAX_DEFAULT;

        public Color BackgroundColor
        {
            get
            {
                return this.table.DefaultCellStyle.BackColor;
            }

            set
            {
                this.table.DefaultCellStyle.BackColor = value;
                this.table.BackgroundColor = value;
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return this.table.DefaultCellStyle.ForeColor;
            }

            set
            {
                this.table.DefaultCellStyle.ForeColor = value;
            }
        }

        public bool ButtonSystemEnable
        {
            get
            {
                return this.toolStripButtonSystem.Enabled;
            }

            set
            {
                this.toolStripButtonSystem.Checked = value;
                this.toolStripButtonSystem.Enabled = value;

                this._TypeMask |= EventTableMessageTypeEnum.System;

                this.reloadTable();
            }
        }

        public bool ButtonCloseEnable
        {
            get
            {
                return this.toolStripButtonClose.Enabled;
            }

            set
            {
                this.toolStripButtonClose.Enabled = value;
            }
        }

        public bool ButtonMinMaxEnable
        {
            get
            {
                return this.toolStripButtonMinMax.Enabled;
            }

            set
            {
                this.toolStripButtonMinMax.Enabled = value;
            }
        }

        public bool ShowLastMessage
        {
            get
            {
                return this.statusStripLastMessage.Visible;
            }

            set
            {
                this.statusStripLastMessage.Visible = value;
            }
        
        }

        [Browsable(false)]
        public EventTableMessage LastMessage
        {
            get { return this._LastMessage; }
        }private EventTableMessage _LastMessage = null;


        public event EventHandler ButtonCloseClick;
        public event EventHandler ButtonMinMaxClick;
        public event EventHandler ButtonClearClick;

        public event EventTableEventHandler MessageArrived;

        public EventTableControl()
        {
            InitializeComponent();

            this.toolStrip.Renderer = new Controls.Renderer.ToolStripRendererCustom();

            this._Initialized = true;
        }


        public void AppendEvent(string strText, EventTableMessageTypeEnum type = EventTableMessageTypeEnum.Info, Icon icon = null, bool bUupdateLast = false)
        {
            if (!this._Initialized || this.IsDisposed || this.Disposing)
                return;

            DateTime ts = DateTime.Now;

            lock (this._Messages)
            {
                if (bUupdateLast)
                {
                    for (int i = this._Messages.Count - 1; i >= 0; i--)
                    {
                        EventTableMessage msg = this._Messages[i];
                        if (msg.Type == type)
                        {
                            //Update last message
                            msg.Text = strText;
                            msg.TimeStamp = ts;
                            this.tableUpdateMessage(msg);
                            return;
                        }
                    }
                }
                else
                {
                    //Set default icon if null
                    if (icon == null)
                    {
                        switch (type & ~EventTableMessageTypeEnum.System)
                        {
                            case EventTableMessageTypeEnum.Warning:
                                icon = System.Drawing.SystemIcons.Warning;
                                break;

                            case EventTableMessageTypeEnum.Error:
                                icon = System.Drawing.SystemIcons.Error;
                                break;

                            default:
                                icon = System.Drawing.SystemIcons.Information;
                                break;
                        }
                    }

                    //Create new message
                    EventTableMessage msg = new EventTableMessage(strText, type, icon, ts);
                    this._Messages.Add(msg);

                    //Limit
                    int iCnt = 0;
                    for (int i = this._Messages.Count - 1; i >= 0; i--)
                    {
                        if (this._Messages[i].Type == type && iCnt++ > this._TableRowsLimit)
                            this._Messages.RemoveAt(i);
                    }

                    //Append to the table
                    this.tableAppendMessage(msg);
                }
            }
        }

        public void Clear()
        {
            lock (this._Messages)
            {
                this._Messages.Clear();
                this.reloadTable();
            }
        }


        private void tableAppendMessage(EventTableMessage msg)
        {
            if (this.table.InvokeRequired)
                this.table.BeginInvoke(new MethodInvoker(() => this.tableAppendMessage(msg)));
            else
            {
                if (this.isMessageAllowed(msg))
                {
                    //Add to the table
                    this.table.Rows.Add(this.createRow(msg));

                    //Limit
                    if (table.Rows.Count > this._TableRowsLimit)
                        this.table.Rows.RemoveAt(0);

                    //Preselection
                    if (table.Rows.Count > 0)
                        this.table.CurrentCell = table.Rows[table.Rows.Count - 1].Cells[0];

                    //Set Last Message
                    this.setLastMessage(msg);
                }
            }
        }

        private void tableUpdateMessage(EventTableMessage msg)
        {
            if (this.table.InvokeRequired)
                this.table.BeginInvoke(new MethodInvoker(() => this.tableUpdateMessage(msg)));
            else
            {
                foreach (DataGridViewRow row in this.table.Rows)
                {
                    if (row.Tag == msg)
                    {
                        //Update text only
                        row.Cells[1].Value = msg.TimeStamp.ToLongTimeString();
                        row.Cells[2].Value = msg.Text;

                        //Set Last Message
                        this.setLastMessage(msg);

                        return;
                    }
                }
            }
        }

        private void setLastMessage(EventTableMessage msg)
        {
            this.toolStripStatusLabelLastMessage.Text = string.Format(" {0}  {1}", msg.TimeStamp.ToLongTimeString(), msg.Text);
            
            if (msg.Icon == System.Drawing.SystemIcons.Information)
                this.toolStripStatusLabelLastMessage.Image = this.imageList.Images[0];
            else if (msg.Icon == System.Drawing.SystemIcons.Warning)
                this.toolStripStatusLabelLastMessage.Image = this.imageList.Images[1];
            else if (msg.Icon == System.Drawing.SystemIcons.Error)
                this.toolStripStatusLabelLastMessage.Image = this.imageList.Images[2];
            else
                this.toolStripStatusLabelLastMessage.Image = getCachedBitmap(msg.Icon);

            //Set last message
            this._LastMessage = msg;

            //Fire event
            if (this.MessageArrived != null)
            {
                try { this.MessageArrived(this, new EventTableEventArgs() { Message = msg }); }
                catch {}
            }

        }

        private static Bitmap getCachedBitmap(Icon icon)
        {
            if (icon == null)
                return null;

            Bitmap bmp = (Bitmap)_Bitmaps[icon];
            if (bmp == null)
            {
                bmp = icon.ToBitmap();
                _Bitmaps.Add(icon, bmp);
            }

            return bmp;
        }

        private void reloadTable()
        {
            if (!this._Initialized || this.IsDisposed || this.Disposing)
                return;

            this.table.Rows.Clear();

            lock (this._Messages)
            {
                for (int i = this._Messages.Count - 1; i >= 0 && this.table.Rows.Count < this._TableRowsLimit; i--)
                {
                    EventTableMessage msg = this._Messages[i];

                    if (this.isMessageAllowed(msg))
                        this.table.Rows.Insert(0, this.createRow(msg));
                }

                if (table.Rows.Count > 0)
                {
                    this.table.CurrentCell = table.Rows[table.Rows.Count - 1].Cells[0];
                    this.table.FirstDisplayedScrollingRowIndex = table.Rows.Count - 1;

                }
            }
        }

        private DataGridViewRow createRow(EventTableMessage msg)
        {
            DataGridViewRow row = (DataGridViewRow)this.table.RowTemplate.Clone();

            for (int i = 0; i < this.table.Columns.Count; i++)
            {
                row.Cells.Add((DataGridViewCell)this.table.Columns[i].CellTemplate.Clone());
            }

            row.Cells[0].Value = msg.Icon;
            row.Cells[1].Value = msg.TimeStamp.ToLongTimeString();
            row.Cells[2].Value = msg.Text;

            row.Tag = msg;

            return row;
        }

        private bool isMessageAllowed(EventTableMessage msg)
        {
            if (this.toolStripButtonSystem.Enabled && this.toolStripButtonSystem.Checked
                && (msg.Type & EventTableMessageTypeEnum.System) != EventTableMessageTypeEnum.System)
                return false;
            else
                return (msg.Type & this._TypeMask & ~EventTableMessageTypeEnum.System) != EventTableMessageTypeEnum.None;

        }


        private void table_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                e.Handled = true;
                e.Graphics.FillRectangle(new SolidBrush(e.CellStyle.BackColor), e.CellBounds);
                DataGridViewCell cell = this.table.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.Value == System.Drawing.SystemIcons.Information)
                    e.Graphics.DrawImageUnscaled(this.imageList.Images[0], e.CellBounds.X + 2, e.CellBounds.Y + 2);
                else if (cell.Value == System.Drawing.SystemIcons.Warning)
                    e.Graphics.DrawImageUnscaled(this.imageList.Images[1], e.CellBounds.X + 2, e.CellBounds.Y + 2);
                else if (cell.Value == System.Drawing.SystemIcons.Error)
                    e.Graphics.DrawImageUnscaled(this.imageList.Images[2], e.CellBounds.X + 2, e.CellBounds.Y + 2);
                else
                    e.Graphics.DrawIcon((Icon)cell.Value, new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 2, e.CellBounds.Width - 4, e.CellBounds.Height - 4));
            }
        }

        private void toolStripButtonInf_Click(object sender, EventArgs e)
        {
            if (this.toolStripButtonInf.Checked)
                this._TypeMask |= EventTableMessageTypeEnum.Info;
            else
                this._TypeMask &= ~EventTableMessageTypeEnum.Info;

            this.reloadTable();
        }

        private void toolStripButtonWarn_Click(object sender, EventArgs e)
        {
            if (this.toolStripButtonWarn.Checked)
                this._TypeMask |= EventTableMessageTypeEnum.Warning;
            else
                this._TypeMask &= ~EventTableMessageTypeEnum.Warning;

            this.reloadTable();
        }

        private void toolStripButtonErr_Click(object sender, EventArgs e)
        {
            if (this.toolStripButtonErr.Checked)
                this._TypeMask |= EventTableMessageTypeEnum.Error;
            else
                this._TypeMask &= ~EventTableMessageTypeEnum.Error;

            this.reloadTable();
        }

        private void toolStripButtonSystem_Click(object sender, EventArgs e)
        {
            if (this.toolStripButtonSystem.Checked)
                this._TypeMask |= EventTableMessageTypeEnum.System;
            else
                this._TypeMask &= ~EventTableMessageTypeEnum.System;

            this.reloadTable();
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            this.Clear();

            if (this.ButtonClearClick != null)
                this.ButtonClearClick(this, e);
        }

        private void toolStripButtonMinMax_Click(object sender, EventArgs e)
        {
            if (this.ButtonMinMaxClick != null)
                this.ButtonMinMaxClick(this, e);
        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            if (this.ButtonCloseClick != null)
                this.ButtonCloseClick(this, e);
        }
    }
}
