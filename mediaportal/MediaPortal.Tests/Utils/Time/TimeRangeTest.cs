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

using System;
using MediaPortal.Utils.Time;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Time
{
  [TestFixture]
  [Category("TimeRange")]
  public class TimeRangeTest
  {
    [Test]
    public void InRange()
    {
      TimeRange rt = new TimeRange("06:00", "18:00");

      DateTime dt = new DateTime(2009, 3, 5, 6, 0, 0, 0);
      Assert.IsFalse(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 6, 1, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 12, 0, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 18, 0, 0, 0);
      Assert.IsFalse(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 17, 59, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));
    }

    [Test]
    public void InRangeOverMidnight()
    {
      TimeRange rt = new TimeRange("23:00", "06:00");

      DateTime dt = new DateTime(2009, 3, 5, 23, 30, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 0, 10, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 3, 0, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));

      dt = new DateTime(2009, 3, 5, 5, 59, 0, 0);
      Assert.IsTrue(rt.IsInRange(dt));
    }
  }
}