#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPRepository.Support
{

  /// <summary>
  /// This class is a base class for unique persistent items, handling the
  /// Id field as well as the requirements for hashed sets (ISet).
  /// The implementation is based on sample code by Gabriel Schenker from
  /// http://blogs.hibernatingrhinos.com/nhibernate/archive/2008/04/04/identity-field-equality-and-hash-code.aspx
  /// </summary>
  /// <typeparam name="T">The type with Id</typeparam>
  public class IdentityFieldProvider<T>
      where T : IdentityFieldProvider<T>
  {
    private Int64 _id;
    private int? _oldHashCode;

    public virtual Int64 Id
    {
      get { return _id; }
      set { _id = value; }
    }

    public override bool Equals(object obj)
    {
      T other = obj as T;
      if (other == null)
        return false;

      // handle the case of comparing two NEW objects
      bool otherIsTransient = Equals(other.Id, 0);
      bool thisIsTransient = Equals(Id, 0);
      if (otherIsTransient && thisIsTransient)
        return ReferenceEquals(other, this);

      return other.Id.Equals(Id);
    }

    public override int GetHashCode()
    {
      // Once we have a hash code we'll never change it
      if (_oldHashCode.HasValue)
        return _oldHashCode.Value;

      bool thisIsTransient = Equals(Id, 0);

      // When this instance is transient, we use the base GetHashCode()
      // and remember it, so an instance can NEVER change its hash code.
      if (thisIsTransient)
      {
        _oldHashCode = base.GetHashCode();
        return _oldHashCode.Value;
      }
      return Id.GetHashCode();
    }



    public static bool operator ==(IdentityFieldProvider<T> x, IdentityFieldProvider<T> y)
    {
      return Equals(x, y);
    }

    public static bool operator !=(IdentityFieldProvider<T> x, IdentityFieldProvider<T> y)
    {
      return !(x == y);
    }
 
  }


}
