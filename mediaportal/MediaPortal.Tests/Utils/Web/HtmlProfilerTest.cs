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
  public class HtmlProfilerTest
  {
    [Test]
    public void MatchCount()
    {
      HtmlProfiler profiler;
      int count;

      // Normal test
      HtmlSectionTemplate template = new HtmlSectionTemplate();
      template.Tags = "T";
      template.Template = "<table><tr><td>Test</td><td>1</td><td>2</td></tr></table>";
      profiler = new HtmlProfiler(template);

      count =
        profiler.MatchCount(
          "<table><tr><td>Test</td><td>1</td><td>2</td></tr></table><div><div><div><table><tr><td>Test</td><td>1</td><td>2</td></tr></table><span><span><span><table><tr><td>Test</td><td>1</td><td>2</td><td>3</td></tr></table>");

      Assert.IsTrue(count == 2);

      // Regex test
      template.Template = "<table><tr><td>Test</td><td>1</td><td>2</td><Z(><td>3</td></Z)?></tr></table>";
      profiler = new HtmlProfiler(template);

      count =
        profiler.MatchCount(
          "<table><tr><td>Test</td><td>1</td><td>2</td></tr></table><div><div><div><table><tr><td>Test</td><td>1</td><td>2</td></tr></table><span><span><span><table><tr><td>Test</td><td>1</td><td>2</td><td>3</td></tr></table>");

      Assert.IsTrue(count == 3);
    }
  }
}