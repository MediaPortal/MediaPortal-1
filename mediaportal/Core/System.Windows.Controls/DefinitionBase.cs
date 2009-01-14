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
  public abstract class DefinitionBase : FrameworkContentElement
  {
    #region Constructors

    static DefinitionBase()
    {
      SharedSizeGroupProperty = DependencyProperty.Register("SharedSizeGroup", typeof (string), typeof (DefinitionBase));
//			SharedSizeGroupProperty.ValidateValueCallback = new ValidateValueCallback(ValidateSharedSizeGroup);
    }

    public DefinitionBase()
    {
    }

    #endregion Constructors

    #region Methods

    private bool ValidateSharedSizeGroup(object value)
    {
      string group = value as string;

      if (group == null)
      {
        return false;
      }

      group = group.Trim();

      // cannot be empty
      if (group == string.Empty)
      {
        return false;
      }

      // cannot not start with a digit
      if (char.IsDigit(group[0]))
      {
        return false;
      }

      // must consist of letters, digits, and underscore characters only
      foreach (char c in group)
      {
        if (!char.IsLetterOrDigit(c) && c != '_')
        {
          return false;
        }
      }

      return true;
    }

    #endregion Methods

    #region Properties

    public string SharedSizeGroup
    {
      get { return (string) GetValue(SharedSizeGroupProperty); }
      set { SetValue(SharedSizeGroupProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty SharedSizeGroupProperty;

    #endregion Properties (Dependency)
  }
}