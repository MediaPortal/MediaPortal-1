using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;  

namespace Pabo.MozBar
{
	
	
	#region Delegates

	public delegate void MozScrollEventHandler(object sender, MozScrollEventArgs e);
	
	#endregion
	
	[StructLayout(LayoutKind.Sequential)]
	public struct SCROLLINFO
	{
		public int cbSize;
		public int fMask;
		public int nMin;
		public int nMax;
		public int nPage;
		public int nPos;
		public int nTrackPos;
	}
		
	/// <summary>
	/// Adds the missing scroll events to the scrollable control!
	/// Written by Martin Randall - Thursday 17th June, 2004
	///
	///
	/// Modified by Patrik Bohman , May 2005
	/// 
	/// </summary>
	[ToolboxItem(false)]
	public class ScrollableControlWithScrollEvents : ScrollableControl
	{
		
		#region Win32 API Constants

		private const int WS_HSCROLL = 0x100000;

		private const int WM_HSCROLL = 0x114;
		private const int WM_VSCROLL = 0x115;
		private const int SB_HORZ = 0;
		private const int SB_VERT = 1;
		private const int SIF_RANGE =0x1;
		private const int SIF_PAGE = 0x2;
		private const int SIF_POS = 0x4;
		private const int SIF_DISABLENOSCROLL = 0x8;
		private const int SIF_TRACKPOS = 0x10;
		private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_DISABLENOSCROLL | SIF_TRACKPOS;
		
		#endregion

		#region Win32 API Functions

		[DllImport("User32", EntryPoint="GetScrollInfo")]
		private static extern bool GetScrollInfo (IntPtr hWnd, int fnBar, ref SCROLLINFO info);
		
		#endregion
		
		#region Events

		[Browsable(true)]
		[Description("Indicates that the control has been scrolled horizontally.")]
		[Category("Panel")]
		public new event MozScrollEventHandler HorizontalScroll;

		

		[Browsable(true)]
		[Description("Indicates that the control has been scrolled vertically.")]
		[Category("Panel")]
		public new event MozScrollEventHandler VerticalScroll;
		
		#endregion

		#region Overrides

		protected override CreateParams CreateParams
		{
			get
			{
				
				CreateParams p = base.CreateParams;
				//p.Style= p.Style & ~WS_HSCROLL;
				return p; //base.CreateParams;
			}
		}

		/// <summary>
		/// Intercept scroll messages to send notifications
		/// </summary>
		/// <param name="m">Message parameters</param>
		protected override void WndProc(ref Message m)
		{
			// Let the control process the message
			base.WndProc (ref m);

			// Was this a horizontal scroll message?
			if ( m.Msg == WM_HSCROLL ) 
			{
				if ( HorizontalScroll != null ) 
				{
					uint wParam = (uint)m.WParam.ToInt32();
					SCROLLINFO si = new SCROLLINFO();
					si.cbSize = Marshal.SizeOf(si);
					si.fMask = SIF_ALL;
					bool ret = GetScrollInfo(this.Handle,SB_HORZ,ref si);
					HorizontalScroll( this, 
						new MozScrollEventArgs( 
							GetEventType( wParam & 0xffff), (int)(wParam >> 16),si ) );
				}
			} 
			// or a vertical scroll message?
			else if ( m.Msg == WM_VSCROLL )
			{
				
				if ( VerticalScroll != null )
				{
					uint wParam = (uint)m.WParam.ToInt32();
					SCROLLINFO si = new SCROLLINFO();
					si.cbSize = Marshal.SizeOf(si);
					si.fMask = SIF_ALL;
					bool ret = GetScrollInfo(this.Handle,SB_VERT,ref si);
					VerticalScroll( this, 
						new MozScrollEventArgs( 
						GetEventType( wParam & 0xffff), (int)(wParam >> 16),si ) );
							
				}
			}
		}

		#endregion

		#region Methods

		// Based on SB_* constants
		private static ScrollEventType [] _events =
			new ScrollEventType[] {
									  ScrollEventType.SmallDecrement,
									  ScrollEventType.SmallIncrement,
									  ScrollEventType.LargeDecrement,
									  ScrollEventType.LargeIncrement,
									  ScrollEventType.ThumbPosition,
									  ScrollEventType.ThumbTrack,
									  ScrollEventType.First,
									  ScrollEventType.Last,
									  ScrollEventType.EndScroll
								  };
		/// <summary>
		/// Decode the type of scroll message
		/// </summary>
		/// <param name="wParam">Lower word of scroll notification</param>
		/// <returns></returns>
		private ScrollEventType GetEventType( uint wParam )
		{
			if ( wParam < _events.Length )
				return _events[wParam];
			else
				return ScrollEventType.EndScroll;
		}

		#endregion
		
	}

	#region MozScrollEventArgs
	
	public class MozScrollEventArgs
	{
		#region Class Data

		/// <summary>
		/// The color that has changed
		/// </summary>
		private ScrollEventType m_type;
		private int m_newValue;
		private SCROLLINFO m_info;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the MozItemEventArgs class with default settings
		/// </summary>
		
		public MozScrollEventArgs(ScrollEventType type , int newValue, SCROLLINFO info)
		{
			m_type = type;
			m_newValue = newValue; 
			m_info = info;
		}

		#endregion


		#region Properties

		public SCROLLINFO ScrollInfo
		{
			get
			{
				return this.m_info;
			}
		}
		public ScrollEventType Type
		{
			get
			{
				return this.m_type;
			}
		}
		public int NewValue
		{
			get
			{
				return this.m_newValue;
			}
		}

		#endregion
	}


	#endregion

}
