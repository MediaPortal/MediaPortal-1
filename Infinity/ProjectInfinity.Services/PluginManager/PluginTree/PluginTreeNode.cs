#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using ProjectInfinity;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Plugins
{
  /// <summary>
  /// Description of PluginTreeNode.
  /// </summary>
  public class PluginTreeNode
  {
    #region Variables
    Dictionary<string, PluginTreeNode> _childNodes = new Dictionary<string, PluginTreeNode>();
    List<NodeItem> _items = new List<NodeItem>();
    bool _isSorted = false;
    #endregion

    #region Constructors/Destructors
    public PluginTreeNode()
    {
    }
    #endregion

    #region Properties
    public Dictionary<string, PluginTreeNode> ChildNodes
    {
      get { return _childNodes; }
    }

    public List<NodeItem> Items
    {
      get { return _items; }
    }
    #endregion

    #region Public Methods
    public List<T> BuildChildItems<T>(object caller)
    {
      List<T> items = new List<T>(_items.Count);
      if (!_isSorted)
      {
        _items = (new NodeItemSort(_items)).Execute();
        _isSorted = true;
      }
      foreach (NodeItem item in _items)
      {
        ArrayList subItems = null;
        if (_childNodes.ContainsKey(item.Id))
        {
          subItems = _childNodes[item.Id].BuildChildItems(caller);
        }
        object result = item.BuildItem(caller, subItems);
        if (result == null)
          continue;
        //IBuildItemsModifier mod = result as IBuildItemsModifier;
        //if (mod != null) {
        //  mod.Apply(items);
        //} else 
        if (result is T)
        {
          items.Add((T)result);
        }
        else
        {
          throw new InvalidCastException("The PluginTreeNode <" + item.Name + " id='" + item.Id
                                         + "' returned an instance of " + result.GetType().FullName
                                         + " but the type " + typeof(T).FullName + " is expected.");
        }
      }
      return items;
    }

    // Workaround for Boo compiler (it cannot distinguish between the generic and non-generic method)
    public ArrayList BuildChildItemsArrayList(object caller)
    {
      return BuildChildItems(caller);
    }

    public ArrayList BuildChildItems(object caller)
    {
      ArrayList items = new ArrayList(_items.Count);
      if (!_isSorted)
      {
        _items = (new NodeItemSort(_items)).Execute();
        _isSorted = true;
      }
      foreach (NodeItem item in _items)
      {
        ArrayList subItems = null;
        if (_childNodes.ContainsKey(item.Id))
        {
          subItems = _childNodes[item.Id].BuildChildItems(caller);
        }
        object result = item.BuildItem(caller, subItems);
        if (result == null)
          continue;
        //IBuildItemsModifier mod = result as IBuildItemsModifier;
        //if (mod != null) {
        //  mod.Apply(items);
        //} else {
        items.Add(result);
        //}
      }
      return items;
    }

    public object BuildChildItem(string childItemID, object caller, ArrayList subItems)
    {
      foreach (NodeItem item in _items)
      {
        if (item.Id == childItemID)
        {
          return item.BuildItem(caller, subItems);
        }
      }
      throw new TreePathNotFoundException(childItemID);
    }
    #endregion
  }
}