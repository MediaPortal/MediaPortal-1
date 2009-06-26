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

using MediaPortal.Drawing;

namespace System.Windows
{
  public class SizeChangedEventArgs : RoutedEventArgs
  {
    #region Constructors

    public SizeChangedEventArgs()
    {
    }

    #endregion Constructors

    #region Methods

    protected override void InvokeEventHandler(Delegate handler, object target)
    {
      base.InvokeEventHandler(handler, target);
    }

    #endregion Methods

    #region Properties

    public bool HeightChanged
    {
      get { return _heightChanged; }
    }

    public Size NewSize
    {
      get { return _newSize; }
    }

    public Size PreviousSize
    {
      get { return _previousSize; }
    }

    public bool WidthChanged
    {
      get { return _widthChanged; }
    }

    #endregion Properties

    #region Fields

    private bool _heightChanged = false;
    private Size _newSize = Size.Empty;
    private Size _previousSize = Size.Empty;
    private bool _widthChanged = false;

    #endregion Fields
  }
}