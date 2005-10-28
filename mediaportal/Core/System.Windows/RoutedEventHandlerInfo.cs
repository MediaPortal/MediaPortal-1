#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;

namespace System.Windows
{
	// http://channel9.msdn.com/ShowPost.aspx?PostID=73455
	public struct RoutedEventHandlerInfo
	{
		#region Constructors
		
		internal RoutedEventHandlerInfo(Delegate handler, bool isInvokeHandlerEventsToo)
		{
			_handler = handler;
			_isInvokeHandlerEventsToo = isInvokeHandlerEventsToo;
			_globalIndex = _globalIndexNext++;
		}

		#endregion Constructors

		#region Methods

		public override bool Equals(Object other)
		{
			if(other is RoutedEventHandlerInfo)
				return this.Equals((RoutedEventHandlerInfo)other);

			return false;
		}

		public bool Equals(RoutedEventHandlerInfo handlerInfo)
		{
			return this == handlerInfo;
		}

		public override int GetHashCode()
		{
			return _handler.GetHashCode();
		}

		internal void InvokeHandler(object target, RoutedEventArgs routedEventArgs)
		{
			// RoutedEventArgs.InvokeEventHandler
		}
	
		#endregion Methods

		#region Operators

		public static bool operator ==(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
		{
			return handlerInfo1._handler == handlerInfo2._handler && handlerInfo1._isInvokeHandlerEventsToo == handlerInfo2._isInvokeHandlerEventsToo;
		}

		public static bool operator !=(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
		{
			return handlerInfo1._handler != handlerInfo2._handler && handlerInfo1._isInvokeHandlerEventsToo != handlerInfo2._isInvokeHandlerEventsToo;
		}

		#endregion Operators

		#region Properties

		public Delegate Handler
		{
			get { return _handler; }
		}

		public bool InvokeHandledEventsToo
		{
			get { return _isInvokeHandlerEventsToo; }
		}

		#endregion Properties

		#region Fields

		Delegate					_handler;
		bool						_isInvokeHandlerEventsToo;
		readonly int				_globalIndex;
		static int					_globalIndexNext = 0;

		#endregion Fields
	}
}
