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

using System.Collections;

namespace System.Windows
{
  public abstract class FrameworkTemplate : INameScope, IResourceHost
  {
    #region Constructors

    protected FrameworkTemplate()
    {
    }

    #endregion Constructors

    #region Methods

    object INameScope.FindName(string name)
    {
      return FindName(name, null);
    }

    public DependencyObject FindName(string name, FrameworkElement templatedParent)
    {
      if (_names == null)
      {
        return null;
      }

      return (DependencyObject) _names[name];
    }

    object IResourceHost.GetResource(object key)
    {
      if (_names == null)
      {
        return null;
      }

      return (DependencyObject) _names[key];
    }

    public void RegisterName(string name, object context)
    {
      if (_names == null)
      {
        _names = new Hashtable();
      }

      _names[name] = context;
    }

    public void UnregisterName(string name)
    {
      if (_names == null)
      {
        return;
      }

      _names.Remove(name);
    }

    protected virtual void ValidateTemplatedParent(FrameworkElement templatedParent)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public bool IsSealed
    {
      get { return _isSealed; }
    }

    public ResourceDictionary Resources
    {
      get
      {
        if (_resources == null)
        {
          _resources = new ResourceDictionary();
        }
        return _resources;
      }
    }

    IResourceHost IResourceHost.ParentResourceHost
    {
      get { throw new NotImplementedException(); }
    }

    public FrameworkElementFactory VisualTree
    {
      get { return _visualTree; }
      set { _visualTree = value; }
    }

    #endregion Properties

    #region Fields

    private bool _isSealed = false;
    private Hashtable _names;
    private ResourceDictionary _resources;
    private FrameworkElementFactory _visualTree;

    #endregion Fields
  }
}