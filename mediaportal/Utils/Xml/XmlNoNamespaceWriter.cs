#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Utils.Xml
{
  public class XmlNoNamespaceWriter : System.Xml.XmlTextWriter
  {
    bool _skipAttribute = false;

    public XmlNoNamespaceWriter(System.IO.TextWriter writer)
      : base(writer)
    {
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      base.WriteStartElement(null, localName, null);
    }


    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      //If the prefix or localname are "xmlns", don't write it.
      if ( (prefix != null && prefix.CompareTo("xmlns") == 0 )|| 
        ( localName != null && localName.CompareTo("xmlns") == 0))
      {
        _skipAttribute = true;
      }
      else
      {
        base.WriteStartAttribute(null, localName, null);
      }
    }

    public override void WriteString(string text)
    {
      //If we are writing an attribute, the text for the xmlns
      //or xmlns:prefix declaration would occur here.  Skip
      //it if this is the case.
      if (!_skipAttribute)
      {
        base.WriteString(text);
      }
    }

    public override void WriteEndAttribute()
    {
      //If we skipped the WriteStartAttribute call, we have to
      //skip the WriteEndAttribute call as well or else the XmlWriter
      //will have an invalid state.
      if (!_skipAttribute)
      {
        base.WriteEndAttribute();
      }
      //reset the boolean for the next attribute.
      _skipAttribute = false;
    }


    public override void WriteQualifiedName(string localName, string ns)
    {
      //Always write the qualified name using only the
      //localname.
      base.WriteQualifiedName(localName, null);
    }
  }


}
