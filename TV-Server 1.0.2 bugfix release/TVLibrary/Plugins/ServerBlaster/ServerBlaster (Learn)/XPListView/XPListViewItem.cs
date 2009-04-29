using System;
using System.ComponentModel; 
using System.ComponentModel.Design.Serialization; 
using System.Windows.Forms; 
using System.Drawing; 
using System.Runtime.InteropServices; 
using System.Runtime.Serialization; 
//using mdobler.XPCommonControls.ListViewAPI; 

namespace XPListview
{
	
	[TypeConverter(typeof(XPListViewItemConverter))] 
	public class XPListViewItem : System.Windows.Forms.ListViewItem { 
		private int _groupIndex; 

		public XPListViewItem() : base() { 
		} 

		public XPListViewItem(string text) : base(text) { 
			
		} 

		public XPListViewItem(string text, int imageIndex) : base(text, imageIndex) { 
		} 

		public XPListViewItem(string[] items) : base(items) { 
		} 

		public XPListViewItem(string[] items, int imageIndex) : base(items, imageIndex) { 
		} 

		public XPListViewItem(XPListViewItem.ListViewSubItem[] subItems, int imageIndex) : base(subItems, imageIndex) { 
		} 

		public XPListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, Font font) : base(items, imageIndex, foreColor, backColor, font) { 
		} 

		public XPListViewItem(string text, int imageIndex, int groupIndex) : base(text, imageIndex) { 
			_groupIndex = groupIndex; 
		} 

		public XPListViewItem(string[] items, int imageIndex, int groupIndex) : base(items, imageIndex){ 
			this.GroupIndex = groupIndex; 
		} 

		public XPListViewItem(XPListViewItem.ListViewSubItem[] subItems, int imageIndex, int groupIndex) : base(subItems, imageIndex) { 
			this.GroupIndex = groupIndex; 
		} 

		public XPListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, Font font, int groupIndex) : base(items, imageIndex, foreColor, backColor, font) { 
			this.GroupIndex = groupIndex; 
		} 

		[Browsable(true), Category("Info")] 
		public int GroupIndex { 
			get { 
				return _groupIndex; 
			} 
			set { 
				_groupIndex = value; 
				ListViewAPI.AddItemToGroup(((XPListView)base.ListView), base.Index, _groupIndex); 
			} 
		} 

		[Browsable(false)] 
		internal string[] SubItemsArray { 
			get { 
				if (this.SubItems.Count == 0) { 
					return null; 
				} 

				string[] a = new string[this.SubItems.Count - 1];

				for (int i = 0; i <= this.SubItems.Count - 1; i++) { 
					a[i] = this.SubItems[i].Text; 
				} 
				return a; 
			} 
		} 
	}
}
