/*
 * Base class for all classes that need a PersistenceBroker instance
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: BrokerLock.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Xml.Serialization;
using Gentle.Common.Attributes;

namespace Gentle.Framework
{
	/// <summary>
	/// Abstract base class for all classes that need to be locked to a
	/// specific PersistenceBroker instance.
	/// </summary>
	[Serializable]
	public abstract class BrokerLock : IBrokerLock
	{
		[NonSerialized]
		protected PersistenceBroker broker;

		protected BrokerLock( PersistenceBroker broker ) : this( broker, null )
		{
		}

		protected BrokerLock( PersistenceBroker broker, Type type )
		{
			if( broker == null )
			{
				if( type == null )
				{
					type = GetType();
				}
				this.broker = new PersistenceBroker( type );
			}
			else // use supplied broker and associated provider
			{
				this.broker = broker;
			}
		}

		/// <summary>
		/// The session broker provides a lock to the database engine. This is
		/// useful when connecting to multiple databases in a single application.
		/// </summary>
		[XmlIgnore, Visible( false )]
		public PersistenceBroker SessionBroker
		{
			get { return broker; }
			set { broker = value; }
		}
	}
}