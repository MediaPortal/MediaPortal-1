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

using System.Collections;

namespace System.Windows
{
  public class FrameworkContentElement : ContentElement
    //, IFrameworkInputElement, IInputElement, ISupportInitialize, IResourceHost
  {
    #region Constructors

    public FrameworkContentElement() {}

    #endregion Constructors

    #region Methods

    public object FindName(string name)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public DependencyObject Parent
    {
      get { throw new NotImplementedException(); }
    }

    protected internal virtual IEnumerator LogicalChildren
    {
      get { return NullEnumerator.Instance; }
    }

    #endregion Properties
  }
}