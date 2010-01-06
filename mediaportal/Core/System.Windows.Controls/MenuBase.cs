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

using System.Windows.Input;

namespace System.Windows.Controls
{
  public abstract class MenuBase : ItemsControl
  {
    #region Constructors

    protected MenuBase() {}

    #endregion Constructors

    #region Methods

    protected override DependencyObject GetContainerForItemOverride(object item)
    {
      throw new NotImplementedException();
    }

    protected virtual void HandleMouseButton(MouseButtonEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
      // Returns true if the item is its own ItemContainer;
      throw new NotImplementedException();
    }

    protected override void OnInitialized(EventArgs e)
    {
      throw new NotImplementedException();
    }

    protected internal override void OnIsFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      throw new NotImplementedException();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      throw new NotImplementedException();
    }

    #endregion Methods
  }
}