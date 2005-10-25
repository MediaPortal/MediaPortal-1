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
	public abstract class Freezable : DependencyObject
	{
		#region Constructors

		protected Freezable()
		{
		}

		#endregion Constructors

		#region Events

		public event EventHandler	Changed;

		#endregion Events

		#region Methods

		public Freezable Copy()
		{
			Freezable freezable = CreateInstance();

			freezable.CopyCore(this);

			return freezable;;
		}

		protected virtual void CopyCore(Freezable sourceFreezable)
		{
		}

		protected Freezable CreateInstance()
		{
			return CreateInstanceCore();
		}

		protected abstract Freezable CreateInstanceCore();

		public void Freeze()
		{
			FreezeCore(false);
		}

		protected internal static bool Freeze(Freezable freezable, bool isChecking)
		{
			return freezable.FreezeCore(isChecking);
		}

		protected virtual bool FreezeCore(bool isChecking)
		{
			if(!isChecking && _isFrozen)
				throw new InvalidOperationException("This instance is already unmodifiable");

			if(isChecking && _isFrozen)
				return false;

			_isFrozen = true;

			return _isFrozen;
		}

		public static Freezable GetAsFrozen(Freezable freezable)
		{
			Freezable freezableCopy = freezable.Copy();

			freezableCopy.Freeze();

			return freezableCopy;
		}
	
		protected override object GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)
		{
			throw new NotImplementedException();
		}

		protected static void ModifyHandlerIfNotFrozen(Freezable freezable, EventHandler handler, bool adding)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnChanged()
		{
			if(Changed != null)
				Changed(this, EventArgs.Empty);
		}

		protected void PropagateChangedHandlers(Freezable oldValue, Freezable newValue)
		{
		}

		protected virtual void PropagateChangedHandlersCore(EventHandler handler, bool adding)
		{
		}

		protected void ReadPreamble()
		{
		}

		protected virtual void ValidateObjectState()
		{
		}

		protected void WritePostscript()
		{
		}

		protected void WritePreamble()
		{
		}

		#endregion Methods

		#region Properties

		public bool CanFreeze
		{
			get { return !_isFrozen; }
		}

		public bool IsFrozen
		{
			get { return _isFrozen; }
		}

		#endregion Properties

		#region Fields

		bool						_isFrozen = false;

		#endregion Fields
	}
}
