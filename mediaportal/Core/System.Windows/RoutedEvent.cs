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
  [TypeConverter(typeof (RoutedEventConverter))]
  public sealed class RoutedEvent
  {
    #region Constructors

    public RoutedEvent() {}

    internal RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
    {
      _name = name;
      _routingStrategy = routingStrategy;
      _handlerType = handlerType;
      _ownerType = ownerType;
    }

    #endregion Constructors

    #region Events

//		public event EventHandler			Activated;
//		public event EventHandler			Closed;
//		public event CancelEventHandler		Closing;
//		public event EventHandler			Deactivated;
//		public event EventHandler			Loading;
//		public event EventHandler			LocationChanged;
//		public event EventHandler			StateChanged;
//		public event EventHandler			WindowSizeChanged;

    #endregion Events

    #region Methods

    public RoutedEvent AddOwner(Type ownerType)
    {
      return EventManager.RegisterRoutedEvent(_name, _routingStrategy, _handlerType, ownerType);
    }

    public override int GetHashCode()
    {
      return _globalIndex;
    }

    public override string ToString()
    {
      // TODO: what should this look like???
      return string.Format("{0}.{1}", _ownerType.Name, _name);
    }

    #endregion Methods

    #region Properties

    public Type HandlerType
    {
      get { return _handlerType; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Type OwnerType
    {
      get { return _ownerType; }
    }

    public RoutingStrategy RoutingStrategy
    {
      get { return _routingStrategy; }
    }

    #endregion Properties

    #region Fields

    private readonly int _globalIndex = _globalIndexNext++;
    private static int _globalIndexNext = 0;
    private Type _handlerType;
    private string _name;
    private Type _ownerType;
    private RoutingStrategy _routingStrategy;

    #endregion Fields
  }
}