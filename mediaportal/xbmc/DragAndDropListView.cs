using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MediaPortal.WinControls
{
	
	public class ListViewEx : ListView
	{
	/// <summary>
	/// Event Handler for SubItemClicked event
	/// </summary>
	public delegate void SubItemClickEventHandler(object sender, SubItemClickEventArgs e);

  public delegate void OnEditEndedHandler(ListViewItem item, int iSubItem);
  public event OnEditEndedHandler onEditEnded;
  ListViewItem m_ListItem;
  int m_iSubItem;
	/// <summary>
	/// Event Args for SubItemClicked event
	/// </summary>
	public class SubItemClickEventArgs : EventArgs
	{
		public SubItemClickEventArgs(ListViewItem item, int subItem)
		{
			_subItemIndex = subItem;
			_item = item;
		}
		private int _subItemIndex = -1;
		private ListViewItem _item = null;
		public int SubItem
		{
			get { return _subItemIndex; }
		}
		public ListViewItem Item
		{
			get { return _item; }
		}
	}

		/// <summary>
		/// MessageHeader for WM_NOTIFY
		/// </summary>
		private struct NMHDR 
		{ 
			public IntPtr hwndFrom; 
			public Int32  idFrom; 
			public Int32  code; 
		}


		[DllImport("user32.dll")]
		private	static extern IntPtr SendMessage(IntPtr hWnd, int msg,	IntPtr wPar, IntPtr	lPar);
		[DllImport("user32.dll", CharSet=CharSet.Ansi)]
		private	static extern IntPtr SendMessage(IntPtr	hWnd, int msg, int len,	ref	int	[] order);

		// ListView messages
		private const int LVM_FIRST					= 0x1000;
		private const int LVM_GETCOLUMNORDERARRAY	= (LVM_FIRST + 59);

		///	<summary>
		///	Required designer variable.
		///	</summary>
		private	System.ComponentModel.Container	components = null;

		public event SubItemClickEventHandler SubItemClicked;

		private const string REORDER = "Reorder";
		private bool allowRowReorder = true;
		public bool AllowRowReorder
		{
			get
			{
				return this.allowRowReorder;
			}
			set
			{
				this.allowRowReorder = value;
				base.AllowDrop = value;
			}
		}

		public new SortOrder Sorting
		{
			get
			{
				return SortOrder.None;
			}
			set
			{
				base.Sorting = SortOrder.None;
			}
		}

		public ListViewEx()
		{
			this.AllowRowReorder = true;
			InitializeComponent();
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);
			if(!this.AllowRowReorder)
			{
				return;
			}
			if(base.SelectedItems.Count==0)
			{
				return;
			}
			Point cp = base.PointToClient(new Point(e.X, e.Y));
			ListViewItem dragToItem = base.GetItemAt(cp.X, cp.Y);
			if(dragToItem==null)
			{
				return;
			}
			int dropIndex = dragToItem.Index;
			if(dropIndex>base.SelectedItems[0].Index)
			{
				dropIndex++;
			}
			ArrayList insertItems = 
				new ArrayList(base.SelectedItems.Count);
			foreach(ListViewItem item in base.SelectedItems)
			{
				insertItems.Add(item.Clone());
			}
			for(int i=insertItems.Count-1;i>=0;i--)
			{
				ListViewItem insertItem =
					(ListViewItem)insertItems[i];
				base.Items.Insert(dropIndex, insertItem);
			}
			foreach(ListViewItem removeItem in base.SelectedItems)
			{
				base.Items.Remove(removeItem);
			}
		}
		
		protected override void OnDragOver(DragEventArgs e)
		{
			if(!this.AllowRowReorder)
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			if(!e.Data.GetDataPresent(DataFormats.Text))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			Point cp = base.PointToClient(new Point(e.X, e.Y));
			ListViewItem hoverItem = base.GetItemAt(cp.X, cp.Y);
			if(hoverItem==null)
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			foreach(ListViewItem moveItem in base.SelectedItems)
			{
				if(moveItem.Index==hoverItem.Index)
				{
					e.Effect = DragDropEffects.None;
					hoverItem.EnsureVisible();
					return;
				}
			}
			base.OnDragOver(e);
			String text = (String)e.Data.GetData(REORDER.GetType());
			if(text.CompareTo(REORDER)==0)
			{
				e.Effect = DragDropEffects.Move;
				hoverItem.EnsureVisible();
			}
			else
			{
				e.Effect = DragDropEffects.None;	
			}
		}		

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			if(!this.AllowRowReorder)
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			if(!e.Data.GetDataPresent(DataFormats.Text))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			base.OnDragEnter(e);
			String text = (String)e.Data.GetData(REORDER.GetType());
			if(text.CompareTo(REORDER)==0)
			{
				e.Effect = DragDropEffects.Move;
			}
			else
			{
				e.Effect = DragDropEffects.None;	
			}
		}

		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			base.OnItemDrag(e);
			if(!this.AllowRowReorder)
			{
				return;
			}
			base.DoDragDrop(REORDER, DragDropEffects.Move);
		}


		///	<summary>
		///	Clean up any resources being used.
		///	</summary>
		protected override void	Dispose( bool disposing	)
		{
			if(	disposing )
			{
				if(	components != null )
					components.Dispose();
			}
			base.Dispose( disposing	);
		}

		#region Component	Designer generated code
		///	<summary>
		///	Required method	for	Designer support - do not modify 
		///	the	contents of	this method	with the code editor.
		///	</summary>
		private	void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		/// <summary>
		/// Retrieve the order in which columns appear
		/// </summary>
		/// <returns>Current display order of column indices</returns>
		public int[] GetColumnOrder()
		{
			IntPtr lPar	= Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);

			IntPtr res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lPar);
			if (res.ToInt32() == 0)	// Something went wrong
			{
				Marshal.FreeHGlobal(lPar);
				return null;
			}

			int	[] order = new int[Columns.Count];
			Marshal.Copy(lPar, order, 0, Columns.Count);

			Marshal.FreeHGlobal(lPar);

			return order;
		}


		/// <summary>
		/// Find ListViewItem and SubItem Index at position (x,y)
		/// </summary>
		/// <param name="x">relative to ListView</param>
		/// <param name="y">relative to ListView</param>
		/// <param name="item">Item at position (x,y)</param>
		/// <returns>SubItem index</returns>
		public int GetSubItemAt(int x, int y, out ListViewItem item)
		{
			item = this.GetItemAt(x, y);
		
			if (item !=	null)
			{
				int[] order = GetColumnOrder();
				Rectangle lviBounds;
				int	subItemX;

				lviBounds =	item.GetBounds(ItemBoundsPortion.Entire);
				subItemX = lviBounds.Left;
				for (int i=0; i<order.Length; i++)
				{
					ColumnHeader h = this.Columns[order[i]];
					if (x <	subItemX+h.Width)
					{
						return h.Index;
					}
					subItemX += h.Width;
				}
			}
			
			return -1;
		}

		// Windows Messages which abort editing
		private	const int WM_HSCROLL = 0x114;
		private	const int WM_VSCROLL = 0x115;
		private const int WM_SIZE	 = 0x05;
		private const int WM_NOTIFY	 = 0x4E;

		private const int HDN_FIRST = -300;
		private const int HDN_BEGINDRAG = (HDN_FIRST-10);
		private const int HDN_ITEMCHANGINGA = (HDN_FIRST-0);
		private const int HDN_ITEMCHANGINGW = (HDN_FIRST-20);

		protected override void	WndProc(ref	Message	msg)
		{
			switch (msg.Msg)
			{
					// Look	for	WM_VSCROLL,WM_HSCROLL or WM_SIZE messages.
				case WM_VSCROLL:
				case WM_HSCROLL:
				case WM_SIZE:
					Focus();
					break;
				case WM_NOTIFY:
					// Look for WM_NOTIFY of events that might also change the
					// editor's position/size: Column reordering or resizing
					NMHDR h = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
					if (h.code == HDN_BEGINDRAG ||
						h.code == HDN_ITEMCHANGINGA ||
						h.code == HDN_ITEMCHANGINGW)
						Focus();
					break;
			}

			base.WndProc(ref msg);
		}

		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseUp(e);

			ListViewItem item;
			int idx = GetSubItemAt(e.X, e.Y, out item);

			if (idx >= 0)
			{
				if (SubItemClicked != null)
					SubItemClicked(this, new SubItemClickEventArgs(item, idx));
			}
		}

		/// <summary>
		/// Get bounds for a SubItem
		/// </summary>
		/// <param name="Item">Target ListViewItem</param>
		/// <param name="SubItem">Target SubItem index</param>
		/// <returns>Bounds of SubItem (relative to ListView)</returns>
		public Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
		{
			int[] order = GetColumnOrder();

			Rectangle subItemRect = Rectangle.Empty;
			if (SubItem >= order.Length)
				throw new IndexOutOfRangeException("SubItem "+SubItem+" out of range");

			if (Item == null)
				throw new ArgumentNullException("Item");
			
			Rectangle lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
			int	subItemX = lviBounds.Left;

			ColumnHeader col;
			int i;
			for (i=0; i<order.Length; i++)
			{
				col = this.Columns[order[i]];
				if (col.Index == SubItem)
					break;
				subItemX += col.Width;
			} 
			subItemRect	= new Rectangle(subItemX, lviBounds.Top, this.Columns[order[i]].Width, lviBounds.Height);
			return subItemRect;
		}

		#region In-place editing functions
		// The control performing the actual editing
		private Control _editingControl;
		// The LVI being edited
		private ListViewItem _editItem;
		// The SubItem being edited
		private int _editSubItem;

		/// <summary>
		/// Begin in-place editing of given cell
		/// </summary>
		/// <param name="c">Control used as cell editor</param>
		/// <param name="Item">ListViewItem to edit</param>
		/// <param name="SubItem">SubItem index to edit</param>
		public void StartEditing(Control c, ListViewItem Item, int SubItem)
		{
      m_ListItem=Item;
      m_iSubItem=SubItem;
			Rectangle rcSubItem = GetSubItemBounds(Item, SubItem);

			if (rcSubItem.X < 0)
			{
				// Left edge of SubItem not visible - adjust rectangle position and width
				rcSubItem.Width += rcSubItem.X;
				rcSubItem.X=0;
			}
			if (rcSubItem.X+rcSubItem.Width > this.Width)
			{
				// Right edge of SubItem not visible - adjust rectangle width
				rcSubItem.Width = this.Width-rcSubItem.Left;
			}

			// Subitem bounds are relative to the location of the ListView!
			rcSubItem.Offset(Left, Top);

			// Position and show editor
			c.Bounds = rcSubItem;
			c.Text = Item.SubItems[SubItem].Text;
			c.Visible = true;
			c.BringToFront();
			c.Focus();

			_editingControl = c;
			_editingControl.Leave += new EventHandler(_editControl_Leave);
			_editingControl.KeyPress += new KeyPressEventHandler(_editControl_KeyPress);

			_editItem = Item;
			_editSubItem = SubItem;
		}

		private void _editControl_Leave(object sender, EventArgs e)
		{
			// cell editor losing focus
			EndEditing(true);
		}

		private void _editControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			switch (e.KeyChar)
			{
				case (char)(int)Keys.Escape:
				{
					EndEditing(false);
					break;
				}

				case (char)(int)Keys.Enter:
				{  
					EndEditing(true);
					break;
				}
			}
		}

		/// <summary>
		/// Accept or discard current value of cell editor control
		/// </summary>
		/// <param name="AcceptChanges"></param>
		public void EndEditing(bool AcceptChanges)
		{
			if (_editingControl == null)
				return;

			if (AcceptChanges)
				_editItem.SubItems[_editSubItem].Text = _editingControl.Text;
			else
				_editingControl.Text = _editItem.SubItems[_editSubItem].Text;

			_editingControl.Leave -= new EventHandler(_editControl_Leave);
			_editingControl.KeyPress -= new KeyPressEventHandler(_editControl_KeyPress);

			_editingControl.Visible = false;

			_editingControl = null;
			_editItem = null;
			_editSubItem = -1;
      if (onEditEnded!=null) onEditEnded(m_ListItem, m_iSubItem);
		}
		#endregion
	}
}
