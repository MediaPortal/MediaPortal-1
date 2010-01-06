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

using System.Windows.Serialization;

namespace System.Windows
{
  public sealed class Trigger : TriggerBase, IAddChild
  {
    #region Constructors

    public Trigger()
    {
      // TODO: prevent unused compiler warning
      if (Invalidated != null)
      {
        Invalidated(null, null);
      }
    }

    #endregion Constructors

    #region Events

    public event TriggerInvalidatedEventHandler Invalidated;

    #endregion Events

    #region Methods

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is SetterBase == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (SetterBase)));
      }

      if (_setters == null)
      {
        _setters = new SetterBaseCollection();
      }

      _setters.Add((SetterBase)child);
    }

    void IAddChild.AddText(string text) {}

    #endregion Methods

    #region Properties

    public DependencyProperty Property
    {
      get { return _property; }
      set { _property = value; }
    }

    public SetterBaseCollection Setters
    {
      get
      {
        if (_setters == null)
        {
          _setters = new SetterBaseCollection();
        }
        return _setters;
      }
    }

    public string SourceName
    {
      get { return _sourceName; }
      set { _sourceName = value; }
    }

    public object Value
    {
      get { return _value; }
      set { _value = value; }
    }

    #endregion Properties

    #region Fields

    private DependencyProperty _property;
    private SetterBaseCollection _setters;
    private string _sourceName;
    private object _value;

    #endregion Fields
  }
}