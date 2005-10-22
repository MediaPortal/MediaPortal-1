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
using System.ComponentModel;

namespace System.Windows
{
	[TypeConverter(typeof(RoutedEventConverter))]
	public sealed class RoutedEvent
	{
		#region Constructors

		public RoutedEvent()
		{
		}

		internal RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
		{
			_name = name;
			_routingStrategy = routingStrategy;
			_handlerType = handlerType;
			_ownerType = ownerType;
		}

		#endregion Constructors

		#region Methods

		private static Type GetType(string type)
		{
			Type t = null;

			foreach(string ns in MediaPortal.Xaml.XamlParser.DefaultNamespaces)
			{
				t = Type.GetType(ns + "." + type);

				if(t != null)
					break;
			}

			return t;
		}

		public static RoutedEvent Parse(string text)
		{
			string[] tokens = text.Split('.');

			if(tokens.Length != 2)
				throw new ArgumentException("text");

			Type t = GetType(tokens[0]);

			if(t == null)
				throw new InvalidOperationException(string.Format("The type or namespace '{0}' could not be found", tokens[0]));

			return EventManager.GetRoutedEventFromName(tokens[1], t);
		}

		public RoutedEvent AddOwner(Type ownerType)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public Type HandlerType
		{
			get { return _handlerType; }
		}

		public string Name
		{
			get { return _name; }
		}

		public Type OwnerType
		{
			get { return _ownerType; }
		}

		public RoutingStrategy RoutingStrategy
		{
			get { return _routingStrategy; }
		}

		#endregion Properties

		#region Fields

		Type						_handlerType;
		string						_name;
		Type						_ownerType;
		RoutingStrategy				_routingStrategy;

		#endregion Fields
	}
}
