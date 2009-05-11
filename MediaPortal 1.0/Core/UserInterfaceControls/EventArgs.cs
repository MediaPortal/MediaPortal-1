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

using System;

/// <summary>
///	Mikael Wiberg 2003
///		mikwib@hotmail.com (usual HoTMaiL spam filters)
///		mick@ar.com.au (heavy spam filters on, harldy anything gets through, START the subject with C# and it will probably go through)
///		md5mw@mdstud.chalmers.se (heavy spam filters on, harldy anything gets through, START the subject with C# and it will probably go through)
///	
///	Feel free to use this code as you wish, as long as you do not take credit for it yourself.
///	If it is used in commercial projects or applications please mention my name.
///	Feel free to donate any amount of money if this code makes you happy ;)
///	Use this code at your own risk. If your machine blows up while using it - don't blame me.
/// </summary>
namespace MWCommon
{
	#region TextDirEventArgs & TextDirEventHandler

	/// <summary>
	/// A delegate for event TextDirEventHandler.
	/// </summary>
	public delegate void TextDirEventHandler(object sender, TextDirEventArgs e);

	/// <summary>
	/// ShadowDirectionEventArgs class.
	/// </summary>
	public class TextDirEventArgs : System.EventArgs
	{
		private TextDir tdOldTextDir = TextDir.Normal;
		private TextDir tdNewTextDir = TextDir.Normal;

		/// <summary>
		/// Standard Constructor.
		/// </summary>
		/// <param name="tdOld">The old TextDir before the property was changed.</param>
		/// <param name="tdNew">The new TextDir after the property was changed.</param>
		public TextDirEventArgs(TextDir tdOld, TextDir tdNew)
		{
			tdOldTextDir = tdOld;
			tdNewTextDir = tdNew;
		}

		/// <summary>
		/// The old TextDir before the property was changed.
		/// </summary>
		public TextDir OldTextDir
		{
			get
			{
				return tdOldTextDir;
			} 
		}

		/// <summary>
		/// The new TextDir after the property was changed.
		/// </summary>
		public TextDir NewTextDir
		{
			get
			{
				return tdNewTextDir;
			} 
		}
	}

	#endregion TextDirEventArgs & TextDirEventHandler





	#region StringFormatEnumEventArgs & StringFormatEnumEventHandler

	/// <summary>
	/// A delegate for event StringFormatEnumEventHandler.
	/// </summary>
	public delegate void StringFormatEnumEventHandler(object sender, StringFormatEnumEventArgs e);

	/// <summary>
	/// ShadowDirectionEventArgs class.
	/// </summary>
	public class StringFormatEnumEventArgs : System.EventArgs
	{
		private StringFormatEnum sfeOldStringFormatEnum = StringFormatEnum.GenericDefault;
		private StringFormatEnum sfeNewStringFormatEnum = StringFormatEnum.GenericDefault;

		/// <summary>
		/// Standard Constructor.
		/// </summary>
		/// <param name="sfeOld">The old StringFormatEnum before the property was changed.</param>
		/// <param name="sfeNew">The new StringFormatEnum after the property was changed.</param>
		public StringFormatEnumEventArgs(StringFormatEnum sfeOld, StringFormatEnum sfeNew)
		{
			sfeOldStringFormatEnum = sfeOld;
			sfeNewStringFormatEnum = sfeNew;
		}

		/// <summary>
		/// The old StringFormatEnum before the property was changed.
		/// </summary>
		public StringFormatEnum OldStringFormatEnum
		{
			get
			{
				return sfeOldStringFormatEnum;
			} 
		}

		/// <summary>
		/// The new StringFormatEnum after the property was changed.
		/// </summary>
		public StringFormatEnum NewStringFormatEnum
		{
			get
			{
				return sfeNewStringFormatEnum;
			} 
		}
	}

	#endregion StringFormatEnumEventArgs & StringFormatEnumEventHandler





	#region MWCancelEventArgs & MWCancelEventHandler

	/// <summary>
	/// A delegate for event MWCancelEventHandler.
	/// </summary>
	public delegate void MWCancelEventHandler(object sender, MWCancelEventArgs e);

	/// <summary>
	/// MWCancelEventArgs class.
	/// The MWCancelEventArgs takes two objects as arguments. These two objects are the current value and the proposed value. These objects
	///		can be used when setting up EventHandlers for the properties that use them so that the programmer will know what the current
	///		and proposed values are.
	///	Note that the MWCancelEventArgs should be used in an OnBeforePROPERTYChanged property - BEFORE the value of the property is changed.
	/// </summary>
	public class MWCancelEventArgs : System.ComponentModel.CancelEventArgs
	{
		#region Variables

		/// <summary>
		/// The current object before the property is changed.
		/// </summary>
		private object oCurrent = null;

		/// <summary>
		/// The proposed object that will be used if the property is changed.
		/// </summary>
		private object oProposed = null;

		#endregion Variables



		#region Constructors

		/// <summary>
		/// Standard constructor.
		/// </summary>
		public MWCancelEventArgs()
		{
		}

		/// <summary>
		/// Standard Constructor taking the current value of the property and the proposed value of the property as arguments.
		/// </summary>
		/// <param name="current">The current object before the property is changed.</param>
		/// <param name="proposed">The proposed object that will be used if the property is changed.</param>
		public MWCancelEventArgs(object current, object proposed)
		{
			oCurrent = current;
			oProposed = proposed;
		}

		#endregion Constructors



		#region Properties

		/// <summary>
		/// The current object before the property is changed.
		/// </summary>
		public object Current
		{
			get
			{
				return oCurrent;
			} 
		}

		/// <summary>
		/// The proposed object that will be used if the property is changed.
		/// </summary>
		public object Proposed
		{
			get
			{
				return oProposed;
			} 
		}

		#endregion Properties

	}

	#endregion MWCancelEventArgs & MWCancelEventHandler

}
