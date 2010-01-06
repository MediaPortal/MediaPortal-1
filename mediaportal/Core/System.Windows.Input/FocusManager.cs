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

namespace System.Windows.Input
{
  public sealed class FocusManager
  {
    #region Constructors

    static FocusManager()
    {
      GotFocusEvent = EventManager.RegisterRoutedEvent("GotFocus", RoutingStrategy.Direct, typeof (RoutedEventHandler),
                                                       typeof (FocusManager));
      LostFocusEvent = EventManager.RegisterRoutedEvent("LostFocus", RoutingStrategy.Direct, typeof (RoutedEventHandler),
                                                        typeof (FocusManager));

      IsFocusScopeProperty = DependencyProperty.RegisterAttached("IsFocusScope", typeof (bool), typeof (FocusManager),
                                                                 new PropertyMetadata(false));
      FocusedElementProperty = DependencyProperty.RegisterAttached("FocusedElement", typeof (IInputElement),
                                                                   typeof (FocusManager));
    }

    private FocusManager() {}

    #endregion Constructors

    #region Methods

    public static IInputElement GetFocusedElement(DependencyObject d)
    {
      return d.GetValue(FocusedElementProperty) as IInputElement;
    }

    public static DependencyObject GetFocusScope(DependencyObject d)
    {
      throw new NotImplementedException();
    }

    public static bool GetIsFocusScope(DependencyObject d)
    {
      return (bool)d.GetValue(IsFocusScopeProperty);
    }

    public static void SetFocusedElement(DependencyObject d, IInputElement focusedElement)
    {
      if (LostFocusEvent != null && _focusedElement != null)
      {
        RoutedEventArgs args = new RoutedEventArgs(GotFocusEvent, d);

        args.RoutedEvent = LostFocusEvent;

        // how to raise the event???
      }

      if (GotFocusEvent != null)
      {
        RoutedEventArgs args = new RoutedEventArgs(GotFocusEvent, d);

        args.RoutedEvent = GotFocusEvent;

        // how to raise the event???
      }

      _focusedElement = focusedElement;

      d.SetValue(FocusedElementProperty, focusedElement);
    }

    public static void SetIsFocusScope(DependencyObject d, bool isFocusScope)
    {
      d.SetValue(IsFocusScopeProperty, isFocusScope);
    }

    #endregion Methods

    #region Events (Routed)

    public static readonly RoutedEvent GotFocusEvent;
    public static readonly RoutedEvent LostFocusEvent;

    #endregion Events (Routed)

    #region Properties (Dependency)

    public static readonly DependencyProperty FocusedElementProperty;
    public static readonly DependencyProperty IsFocusScopeProperty;

    #endregion Properties (Dependency)

    #region Fields

    private static IInputElement _focusedElement;

    #endregion Fields
  }
}