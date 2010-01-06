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

using MediaPortal.Utils.Web;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Web
{
  [TestFixture]
  [Category("Web")]
  public class HtmlHtmlSectionParserTest
  {
    [Test]
    public void ParseSection1()
    {
      // Basic search
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template =
        "<table><tr><td><#TITLE></td><td><#START></td><td><#DESCRIPTION></td><Z(><td><#GENRE></td></Z)?></tr></table>";
      HtmlSectionParser elements = new HtmlSectionParser(template);
      ParserData data = new ParserData();
      IParserData idata = (IParserData)data;
      string source = "<table><tr><td>Test</td><td>123</td><td>blah blah</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");
    }

    [Test]
    public void ParseSection2()
    {
      // Start and End for tag
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template =
        "<table><tr><td>Title:<#TITLE>(</td><td><#START></td><td><#DESCRIPTION></td><Z(><td><#GENRE></td></Z)?></tr></table>";

      HtmlSectionParser elements = new HtmlSectionParser(template);

      ParserData data = new ParserData();
      IParserData idata = (IParserData)data;
      string source = "<table><tr><td>Test</td><td>123</td><td>blah blah</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");

      data = new ParserData();
      idata = (IParserData)data;
      source = "<table><tr><td>Title:Test(1:2)</td><td>123</td><td>blah blah</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");
    }

    [Test]
    public void ParseSection3()
    {
      // Multiple tags
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template =
        "<table><tr><td><#TITLE>-<#SUBTITLE></td><td><#START></td><td><#DESCRIPTION></td><Z(><td><#GENRE></td></Z)?></tr></table>";

      HtmlSectionParser elements = new HtmlSectionParser(template);
      ParserData data = new ParserData();
      IParserData idata = (IParserData)data;
      string source = "<table><tr><td>Test-Sub</td><td>123</td><td>blah blah</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      Assert.IsTrue(data.GetElement("#SUBTITLE") == "Sub");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");
    }

    [Test]
    public void ParseSection4()
    {
      // Multiple tags one missing
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template =
        "<table><tr><td><#TITLE>-<#SUBTITLE></td><td><#START></td><td><#DESCRIPTION></td><Z(><td><#GENRE></td></Z)?></tr></table>";

      HtmlSectionParser elements = new HtmlSectionParser(template);
      ParserData data = new ParserData();
      IParserData idata = (IParserData)data;
      string source = "<table><tr><td>Test</td><td>123</td><td>blah blah</td><td>regex</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      //Assert.IsTrue(data.GetElement("#SUBTITLE") == "Sub");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");
      Assert.IsTrue(data.GetElement("#GENRE") == "regex");
    }

    [Test]
    public void ParseSection5()
    {
      // Regex
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template =
        "<table><tr><td><#TITLE>-<#SUBTITLE></td><td><#START></td><td><#DESCRIPTION></td><Z(><td><#GENRE></td></Z)?></tr></table>";


      HtmlSectionParser elements = new HtmlSectionParser(template);
      ParserData data = new ParserData();
      IParserData idata = (IParserData)data;
      string source = "<table><tr><td>Test-Sub</td><td>123</td><td>blah blah</td><td>regex</td></tr></table>";
      elements.ParseSection(source, ref idata);

      Assert.IsTrue(data.GetElement("#TITLE") == "Test");
      Assert.IsTrue(data.GetElement("#SUBTITLE") == "Sub");
      Assert.IsTrue(data.GetElement("#START") == "123");
      Assert.IsTrue(data.GetElement("#DESCRIPTION") == "blah blah");
      Assert.IsTrue(data.GetElement("#GENRE") == "regex");
    }
  }
}