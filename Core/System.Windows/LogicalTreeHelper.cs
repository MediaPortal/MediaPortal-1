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
using System.Windows.Input;

namespace System.Windows
{
  public sealed class LogicalTreeHelper
  {
    #region Constructors

    private LogicalTreeHelper()
    {
    }

    #endregion Constructors

    #region Methods

    public static void BringIntoView(DependencyObject current)
    {
      if (current is FrameworkElement)
      {
        FrameworkElement element = (FrameworkElement) current;

        RequestBringIntoViewEventArgs args = new RequestBringIntoViewEventArgs(current);

        args.Source = current;
        args.Handled = false;
        args.RoutedEvent = FrameworkElement.RequestBringIntoViewEvent;

        ((IInputElement) element).RaiseEvent(args);

        if (args.Handled)
        {
          element.BringIntoView();
        }

        return;
      }

      if (current is FrameworkContentElement)
      {
        throw new NotImplementedException();
      }

      // TODO:

/*			if(current is FrameworkContentElement)
			{
				FrameworkContentElement element = (FrameworkContentElement)current;

				RequestBringIntoViewEventArgs args = new RequestBringIntoViewEventArgs(current);

				args.Source = current;
				args.Handled = false;
				args.RoutedEvent = FrameworkContentElement.RequestBringIntoViewEvent;

				((IInputElement)element).RaiseEvent(args);

				if(args.Handled)
					element.BringIntoView();

				return;
			}*/
    }

    public static DependencyObject FindLogicalNode(DependencyObject current, string name)
    {
      if (current is FrameworkElement)
      {
        FrameworkElement element = (FrameworkElement) current;

        object node = element.FindName(name);

        if (node != null)
        {
          return node as DependencyObject;
        }

        return FindLogicalNode(element.Parent, name);
      }

      if (current is FrameworkContentElement)
      {
        FrameworkContentElement element = (FrameworkContentElement) current;

        object node = element.FindName(name);

        if (node != null)
        {
          return node as DependencyObject;
        }

        return FindLogicalNode(element.Parent, name);
      }

      return null;
    }

    public static IEnumerator GetChildren(DependencyObject parent)
    {
      if (parent is FrameworkElement)
      {
        return ((FrameworkElement) parent).LogicalChildren;
      }

      if (parent is FrameworkContentElement)
      {
        return ((FrameworkContentElement) parent).LogicalChildren;
      }

      return NullEnumerator.Instance;
    }

    public static IEnumerator GetChildren(FrameworkElement parent)
    {
      if (parent != null)
      {
        return parent.LogicalChildren;
      }

      return NullEnumerator.Instance;
    }

    public static IEnumerator GetChildren(FrameworkContentElement parent)
    {
      if (parent != null)
      {
        return parent.LogicalChildren;
      }

      return NullEnumerator.Instance;
    }

    public static DependencyObject GetParent(DependencyObject child)
    {
      if (child is FrameworkElement)
      {
        return ((FrameworkElement) child).Parent;
      }

      if (child is FrameworkContentElement)
      {
        return ((FrameworkContentElement) child).Parent;
      }

      return null;
    }

    #endregion Methods
  }
}