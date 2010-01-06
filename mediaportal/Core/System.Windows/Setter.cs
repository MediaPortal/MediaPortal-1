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
  public sealed class Setter : SetterBase
  {
    #region Constructors

    public Setter() {}

    public Setter(DependencyProperty property, object value)
    {
      _property = property;
      _value = value;
    }

    public Setter(DependencyProperty property, object value, string targetName)
    {
      _property = property;
      _value = value;
      _targetName = targetName;
    }

    #endregion Constructors

    #region Properties

    public DependencyProperty Property
    {
      get { return _property; }
      set
      {
        CheckSealed();
        _property = value;
      }
    }

    public string TargetName
    {
      get { return _targetName; }
      set
      {
        CheckSealed();
        _targetName = value;
      }
    }

    public object Value
    {
      get { return _value; }
      set
      {
        CheckSealed();
        _value = value;
      }
    }

    #endregion Properties

    #region Fields

    private DependencyProperty _property;
    private string _targetName = string.Empty;
    private object _value;

    #endregion Fields
  }
}