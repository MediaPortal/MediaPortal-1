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

using MediaPortal.Utils.Web;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Web
{
  [TestFixture]
  [Category("Web")]
  public class HttpRequestTest
  {
    [Test]
    public void Base()
    {
      HTTPRequest request = new HTTPRequest("http://www.somewhere.com/path/path/file.html");

      Assert.IsTrue("www.somewhere.com" == request.Host);
      Assert.IsTrue("/path/path/file.html" == request.GetQuery);
    }

    [Test]
    public void Add()
    {
      HTTPRequest requestBase = new HTTPRequest("http://www.somewhere.com/path/path/file.html");

      HTTPRequest request;
      request = requestBase.Add("../relpath/relfile.html");
      Assert.IsTrue("http://www.somewhere.com/path/relpath/relfile.html" == request.Url);

      request = requestBase.Add("/newpath/newfile.html");
      Assert.IsTrue("http://www.somewhere.com/newpath/newfile.html" == request.Url);

      request = requestBase.Add("http://www.somewhere_else.com/path/file.html");
      Assert.IsTrue("http://www.somewhere_else.com/path/file.html" == request.Url);

      requestBase = new HTTPRequest("http://spettacolo.alice.it/guidatv/cgi/index.cgi?tipo=3&channel=22");
      request = requestBase.Add("?tipo=1&qs=8353441");
      Assert.IsTrue("http://spettacolo.alice.it/guidatv/cgi/index.cgi?tipo=1&qs=8353441" == request.Url);
    }

    [Test]
    public void ReplaceTag()
    {
      HTTPRequest request = new HTTPRequest("http://www.somewhere.com/[PATH]/[FILE]");
      request.PostQuery = "[PATH]?[POSTDATA]";

      request.ReplaceTag("[PATH]", "tagpath");
      Assert.IsTrue("/tagpath/[FILE]" == request.GetQuery);
      Assert.IsTrue("tagpath?[POSTDATA]" == request.PostQuery);

      request.ReplaceTag("[FILE]", "tagfile.html");
      Assert.IsTrue("/tagpath/tagfile.html" == request.GetQuery);

      request.ReplaceTag("[POSTDATA]", "tagpostdata");
      Assert.IsTrue("tagpath?tagpostdata" == request.PostQuery);
    }

    [Test]
    public void EqualOperator()
    {
      HTTPRequest request1 = new HTTPRequest("http://www.somewhere.com/1/1");
      HTTPRequest request2 = new HTTPRequest("http://www.somewhere.com/2/2");
      HTTPRequest request3 = new HTTPRequest("http://www.somewhere.com/1/1");

      Assert.IsTrue(request1 == request3);
      Assert.IsFalse(request1 == request2);
      Assert.IsTrue(request1 != request2);

      request1.PostQuery = "post1";
      request2.PostQuery = "post2";
      request3.PostQuery = "post1";

      Assert.IsTrue(request1 == request3);
      Assert.IsFalse(request1 == request2);
      Assert.IsTrue(request1 != request2);

      HTTPRequest request = null;

      Assert.IsTrue(request == null);
      Assert.IsTrue(request1 != null);
    }
  }
}