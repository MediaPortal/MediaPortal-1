#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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

namespace System.Windows.Input
{
	public class CommandBinding
	{
		#region Constructors

		public CommandBinding()
		{
		}
		
		public CommandBinding(RoutedCommand command) : this(command, null)
		{
		}

		public CommandBinding(RoutedCommand command, ExecuteEventHandler executeEventHandler) : this(command, executeEventHandler, null)
		{
		}

		public CommandBinding(RoutedCommand command, ExecuteEventHandler executeEventHandler, QueryEnabledEventHandler queryEnabledEventHandler)
		{
			_command = command;
			_executeEventHandler = executeEventHandler;
			_queryEnabledEventHandler = queryEnabledEventHandler;
		}

		#endregion Constructors

		#region Events (Routed)

//		public event ExecuteEventHandler		Execute;
//		public event ExecuteEventHandler		PreviewExecute;
//		public event QueryEnabledEventHandler	PreviewQueryEnabled;
//		public event QueryEnabledEventHandler	QueryEnabled;

		#endregion Events (Routed)

		#region Properties

		public RoutedCommand Command
		{
			get { return _command; }
			set { _command = value; }
		}

		#endregion Properties

		#region Fields

		ExecuteEventHandler			_executeEventHandler;
		RoutedCommand				_command;	
		QueryEnabledEventHandler	_queryEnabledEventHandler;

		#endregion Fields
	}
}
