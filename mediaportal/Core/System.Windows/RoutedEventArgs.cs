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
  public class RoutedEventArgs : EventArgs
  {
    #region Constructors

    public RoutedEventArgs() {}

    public RoutedEventArgs(RoutedEvent routedEvent)
    {
      _routedEvent = routedEvent;
    }

    public RoutedEventArgs(RoutedEvent routedEvent, object source)
    {
      _routedEvent = routedEvent;
      _source = source;
      _originalSource = source;
    }

    #endregion Constructors

    #region Methods

    protected virtual void InvokeEventHandler(Delegate handler, object target)
    {
      // Roy Osherove
      // http://weblogs.asp.net/rosherove/articles/DefensiveEventPublishing.aspx

      if (handler == null)
      {
        return;
      }

      foreach (Delegate sink in handler.GetInvocationList())
      {
        try
        {
          sink.Method.Invoke(target, new object[] {_source, this});

          if (_isHandled)
          {
            return;
          }
        }
        catch {}
      }
    }

    protected virtual void OnSetSource(object source)
    {
      if (_originalSource == null)
      {
        _originalSource = source;
      }

      _source = source;
    }

    #endregion Methods

    #region Properties

    public bool Handled
    {
      get { return _isHandled; }
      set { _isHandled = value; }
    }

    public object OriginalSource
    {
      get { return _originalSource; }
    }

    public RoutedEvent RoutedEvent
    {
      get { return _routedEvent; }
      set { _routedEvent = value; }
    }

    public object Source
    {
      get { return _source; }
      set { OnSetSource(value); }
    }

    #endregion Properties

    #region Fields

    private bool _isHandled;
    private object _originalSource;
    private RoutedEvent _routedEvent;
    private object _source;

    #endregion Fields
  }
}