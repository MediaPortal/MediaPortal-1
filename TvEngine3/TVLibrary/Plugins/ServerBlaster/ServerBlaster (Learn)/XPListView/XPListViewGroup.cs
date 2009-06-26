using System;
using System.ComponentModel; 

namespace XPListview
{
	[TypeConverter(typeof(XPListViewGroupConverter))] 
	public class XPListViewGroup { 
		private string _text; 
		private int _index; 

		public XPListViewGroup() { 
		} 

		public XPListViewGroup(string text, int index) { 
			_text = text; 
			_index = index; 
		} 

		public XPListViewGroup(string text) { 
			_text = text; 
		} 

		public string GroupText { 
			get { 
				return _text; 
			} 
			set { 
				_text = value; 
			} 
		} 

		public int GroupIndex { 
			get { 
				return _index; 
			} 
			set { 
				_index = value; 
			} 
		} 

		public override string ToString() { 
			return _text; 
		} 
	}
}
