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
  [Category("WorldTimeZone")]
  public class WorldTimeZoneTest
  {
    [Test]
    public void ToLocalTime()
    {
      WorldTimeZone tz = new WorldTimeZone("Greenwich Standard Time");

      DateTime nowDT = DateTime.Now;
      DateTime utcDT = nowDT.ToUniversalTime();
      DateTime localDT = tz.ToLocalTime(utcDT);
      Assert.IsTrue(nowDT == localDT);
    }

    [Test]
    public void ToUTCTime()
    {
      WorldTimeZone tz = new WorldTimeZone(TimeZone.CurrentTimeZone.StandardName);

      DateTime nowDT = DateTime.Now;
      DateTime utcDT = nowDT.ToUniversalTime();
      DateTime tzUTCDT = tz.ToUniversalTime(nowDT);
      Assert.IsTrue(utcDT == tzUTCDT);
    }
  }
}