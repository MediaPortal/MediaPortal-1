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
  // http://channel9.msdn.com/ShowPost.aspx?PostID=73455
  public struct RoutedEventHandlerInfo
  {
    #region Constructors

    internal RoutedEventHandlerInfo(Delegate handler, bool isInvokeHandledEventsToo)
    {
      _handler = handler;
      _isInvokeHandledEventsToo = isInvokeHandledEventsToo;
      _globalIndex = _globalIndexNext++;
    }

    #endregion Constructors

    #region Methods

    public override bool Equals(Object other)
    {
      if (other is RoutedEventHandlerInfo)
      {
        return this.Equals((RoutedEventHandlerInfo)other);
      }

      return false;
    }

    public bool Equals(RoutedEventHandlerInfo handlerInfo)
    {
      return this == handlerInfo;
    }

    public override int GetHashCode()
    {
      return _handler.GetHashCode();
    }

    internal void InvokeHandler(object target, RoutedEventArgs routedEventArgs)
    {
      _handler.Method.Invoke(target, new object[] {routedEventArgs});
    }

    #endregion Methods

    #region Operators

    public static bool operator ==(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
    {
      return handlerInfo1._handler == handlerInfo2._handler &&
             handlerInfo1._isInvokeHandledEventsToo == handlerInfo2._isInvokeHandledEventsToo;
    }

    public static bool operator !=(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2)
    {
      return handlerInfo1._handler != handlerInfo2._handler &&
             handlerInfo1._isInvokeHandledEventsToo != handlerInfo2._isInvokeHandledEventsToo;
    }

    #endregion Operators

    #region Properties

    public Delegate Handler
    {
      get { return _handler; }
    }

    public bool InvokeHandledEventsToo
    {
      get { return _isInvokeHandledEventsToo; }
    }

    #endregion Properties

    #region Fields

    private Delegate _handler;
    private bool _isInvokeHandledEventsToo;
    private readonly int _globalIndex;
    private static int _globalIndexNext = 0;

    #endregion Fields
  }
}