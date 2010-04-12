/*
 * 
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IBindable.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.ComponentModel;

namespace Gentle.Common
{
	/// <summary>
	/// The <c>RemoveEventHandler</c> handles the cancellation of edit
	/// actions in a DataGrid control.
	/// </summary>
	/// <remarks>
	/// If the user cancels the data editing of a new line in the 
	/// DataGrid, the <see cref="System.ComponentModel.IEditableObject.CancelEdit"/> event
	/// is fired and must be handled by the object which is being edited. The 
	/// <see cref="TypedArrayList"/> must be informed to remove the
	/// object from it's list. This will be done by firing a RemoveObject event in the
	/// <c>CancelEdit</c> event handler.
	/// </remarks>
	public delegate void RemoveEventHandler( object sender, EventArgs e );

	/// <summary>
	/// Interface for types which will be bound to a DataGrid using the
	/// <see cref="TypedArrayList"/>.
	/// </summary>
	/// <remarks>
	/// All types stored in a <see cref="TypedArrayList"/> which
	/// will be bound to a DataGrid control should implement this interface.
	/// It defines all methods and events needed to support the editing facilities
	/// of the DataGrid control.
	/// </remarks>
	public interface IBindable : IEditableObject
	{
		/// <summary>
		/// The event should be fired, if the user cancels the editing of
		/// a newly created object in the DataGrid control.
		/// </summary>
		event RemoveEventHandler RemoveObject;
	}
}