#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.ComponentModel;

namespace System.Windows
{
  public class EventHandlersStore
  {
    #region Constructors

    public EventHandlersStore() {}

    #endregion Constructors

    #region Methods

    public void Add(EventPrivateKey key, Delegate handler)
    {
      if (_directEventHandlers == null)
      {
        _directEventHandlers = new EventHandlerList();
      }

      _directEventHandlers.AddHandler(key, handler);
    }

    public void AddRoutedEventHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
      if (_routedEventHandlers == null)
      {
        _routedEventHandlers = new RoutedEventHandlerInfoList();
      }

      _routedEventHandlers.AddHandler(routedEvent, handler, handledEventsToo);
    }

    public bool Contains(RoutedEvent routedEvent)
    {
      if (_routedEventHandlers == null)
      {
        return false;
      }

      return _routedEventHandlers.Contains(routedEvent);
    }

    public Delegate Get(EventPrivateKey key)
    {
      if (_directEventHandlers == null)
      {
        return null;
      }

      return _directEventHandlers[key];
    }

    public RoutedEventHandlerInfo[] GetRoutedEventHandlers(RoutedEvent routedEvent)
    {
      throw new NotImplementedException();
    }

    public RoutedEvent[] GetRoutedEventsWithHandlers()
    {
      throw new NotImplementedException();
    }

    public void Remove(EventPrivateKey key, Delegate handler)
    {
      if (_directEventHandlers == null)
      {
        return;
      }

      _directEventHandlers.RemoveHandler(key, handler);
    }

    public void RemoveRoutedEventHandler(RoutedEvent routedEvent, Delegate handler)
    {
      if (_routedEventHandlers == null)
      {
        return;
      }

      _routedEventHandlers.RemoveHandler(routedEvent, handler);
    }

    #endregion Methods

    #region Fields

    private EventHandlerList _directEventHandlers;
    private RoutedEventHandlerInfoList _routedEventHandlers;

    #endregion Fields
  }
}