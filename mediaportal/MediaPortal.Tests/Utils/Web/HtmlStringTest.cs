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

using MediaPortal.Utils.Web;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Web
{
  [TestFixture]
  [Category("Web")]
  public class HtmlStringTest
  {
    [Test]
    public void TagList()
    {
      MatchTagCollection tagList =
        HtmlString.TagList("<table><tr><#non tag><td><*junk></td><!-- comment --></tr></table>");
      Assert.IsTrue(tagList.Count == 7);
    }

    [Test]
    public void ToAscii()
    {
      string html = "Hello there <br> new line &amp; extra charaters &lt;&gt;";
      string ascii = "Hello there \n new line & extra charaters <>";

      Assert.IsTrue(ascii == HtmlString.ToAscii(html));
    }
  }
}