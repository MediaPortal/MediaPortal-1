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

using System.Collections;

namespace System.Windows
{
  internal class RoutedEventHandlerInfoStore
  {
    public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
      RoutedEventHandlerInfoList list = _handlers[routedEvent] as RoutedEventHandlerInfoList;

      if (list == null)
      {
        _handlers[routedEvent] = list = new RoutedEventHandlerInfoList();
      }

      list.AddHandler(routedEvent, handler, handledEventsToo);
    }

    public bool Contains(RoutedEvent routedEvent)
    {
      return _handlers.ContainsKey(routedEvent);
    }

    public void RemoveHandler(RoutedEvent routedEvent, Delegate handler) {}

    public RoutedEventHandlerInfoList this[RoutedEvent routedEvent]
    {
      get { return _handlers[routedEvent] as RoutedEventHandlerInfoList; }
    }

    #region Fields

    private Hashtable _handlers = new Hashtable();

    #endregion Fields
  }
}