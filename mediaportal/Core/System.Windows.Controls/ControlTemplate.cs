#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace System.Windows.Controls
{
  public class ControlTemplate : FrameworkTemplate
  {
    #region Constructors

    public ControlTemplate()
    {
    }

    public ControlTemplate(Type targetType)
    {
      _targetType = targetType;
    }

    #endregion Constructors

    #region Methods

    protected override void ValidateTemplatedParent(FrameworkElement templatedParent)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public Type TargetType
    {
      get { return _targetType; }
      set { _targetType = value; }
    }

    public TriggerCollection Triggers
    {
      get
      {
        if (_triggers == null)
        {
          _triggers = new TriggerCollection();
        }
        return _triggers;
      }
    }

    #endregion Properties

    #region Fields

    private Type _targetType;
    private TriggerCollection _triggers;

    #endregion Fields
  }
}