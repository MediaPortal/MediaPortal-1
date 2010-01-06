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

namespace System.Windows
{
  public sealed class DependencyPropertyKey
  {
    #region Constructors

    private DependencyPropertyKey() {}

    internal DependencyPropertyKey(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata,
                                   ValidateValueCallback validateValueCallback)
    {
      _dependencyProperty = DependencyProperty.Register(name, propertyType, ownerType, defaultMetadata,
                                                        validateValueCallback);
    }

    #endregion Constructors

    #region Methods

    public void OverrideMetadata(Type ownerType, PropertyMetadata metadata)
    {
      // somehow this isn't correct!
      _dependencyProperty.OverrideMetadata(ownerType, metadata, this);
    }

    #endregion Methods

    #region Properties

    public DependencyProperty DependencyProperty
    {
      get { return _dependencyProperty; }
    }

    #endregion Properties

    #region Fields

    private DependencyProperty _dependencyProperty = null;

    #endregion Fields
  }
}