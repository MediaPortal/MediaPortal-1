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

using System.Xml;

namespace System.Windows
{
  // Gaston Milano's Blog
  // http://weblogs.asp.net/gmilano/archive/2004/11/28/271383.aspx
  public sealed class VisualUtility
  {
    #region Constructors

    private VisualUtility() {}

    #endregion Constructors

    #region Methods

    public static void GetVisualTreeInfo(Visual element, XmlWriter writer)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      writer.WriteStartElement("VisualElement");
      writer.WriteElementString("Type", element.GetType().ToString());

      VisualCollection children = VisualOperations.GetChildren(element);

      if (children != null && children.Count != 0)
      {
        foreach (Visual visual in children)
        {
          GetVisualTreeInfo(visual, writer);
        }
      }

      writer.WriteEndElement();
    }

    #endregion Methods
  }
}