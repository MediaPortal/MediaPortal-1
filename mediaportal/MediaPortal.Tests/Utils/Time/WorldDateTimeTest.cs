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

using System;
using MediaPortal.Utils.Time;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Time
{
  [TestFixture]
  [Category("WorldDateTime")]
  public class WorldDateTimeTest
  {
    [Test]
    public void NewWithLong()
    {
      WorldDateTime wdt = new WorldDateTime("20070323044500 +0200", false);

      Assert.IsTrue(wdt.Year == 2007);
      Assert.IsTrue(wdt.Month == 3);
      Assert.IsTrue(wdt.Day == 23);
      Assert.IsTrue(wdt.Hour == 4);
      Assert.IsTrue(wdt.Minute == 45);


      wdt = new WorldDateTime("20070323044500 +0200", true);

      Assert.IsTrue(wdt.Year == 2007);
      Assert.IsTrue(wdt.Month == 3);
      Assert.IsTrue(wdt.Day == 23);
      Assert.IsTrue(wdt.Hour == 6);
      Assert.IsTrue(wdt.Minute == 45);

      wdt = new WorldDateTime("20070323044500", false);

      Assert.IsTrue(wdt.Year == 2007);
      Assert.IsTrue(wdt.Month == 3);
      Assert.IsTrue(wdt.Day == 23);
      Assert.IsTrue(wdt.Hour == 4);
      Assert.IsTrue(wdt.Minute == 45);

      wdt = new WorldDateTime("20070323044500", true);

      Assert.IsTrue(wdt.Year == 2007);
      Assert.IsTrue(wdt.Month == 3);
      Assert.IsTrue(wdt.Day == 23);
      Assert.IsTrue(wdt.Hour == 4);
      Assert.IsTrue(wdt.Minute == 45);
    }

    [Test]
    public void ToLocalTime()
    {
      WorldDateTime wdt = new WorldDateTime(DateTime.Now);

      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 8:00:00 AM");

      Assert.IsTrue(wdt.DaysSince(dtEpochStartTime) == wdt.ToEpochDate());
      Assert.IsTrue(wdt.SecondsSince(dtEpochStartTime) == wdt.ToEpochTime());
    }
  }
}