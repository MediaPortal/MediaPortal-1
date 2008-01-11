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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.WebEPG;
using System.IO;
using MediaPortal.WebEPG.Parser;

namespace MediaPortal.Tests.WebEPG
{
  [TestFixture]
  [Category("EPG")]
  public class ListingTimeControlTest
  {
    [Test]
    public void StartTimeFullDay()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 10, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);


      // Program without Date, after start time
      program.SetElement("#START", "10:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Year == 2006);
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);


      // Same Program without Date, before start time
      start = new DateTime(2006, 07, 16, 11, 15, 0);
      control = new ListingTimeControl(start);

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));


      // Next program, Time changes to 1:30 .. when 13:30 expected
      program = new ProgramData();
      program.SetElement("#START", "1:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Hour == 13);

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 18);

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 18);
    }

    [Test]
    public void StartTimeNewDay()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 10, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);


      // Program without Date, after start time
      program.SetElement("#START", "10:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Year == 2006);
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);


      Assert.IsTrue(control.CheckAdjustTime(ref program));

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      // Day already changed - call NewDay shouldn't push the day further ahead
      control.NewDay();

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 18);

      // Day already changed - call NewDay shouldn't push the day further ahead
      control.NewDay();

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 18);
    }

    [Test]
    public void StartTimePervious1()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 4, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);

      control.SetProgramCount(10);

      // Program starts on pervious day
      program.SetElement("#START", "21:30");
      Assert.IsFalse(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      Assert.IsFalse(control.CheckAdjustTime(ref program));

      // first program of current day
      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);
    }

    [Test]
    public void StartTimePervious2()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 20, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);

      control.SetProgramCount(10);

      // Program starts on pervious day
      program.SetElement("#START", "21:30");
      Assert.IsFalse(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      Assert.IsFalse(control.CheckAdjustTime(ref program));

      // first program of current day
      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);
    }

    [Test]
    public void StartTimePervious3()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 21, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);

      control.SetProgramCount(6);

      // Program starts on current day
      program.SetElement("#START", "21:30");
      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      program = new ProgramData();
      program.SetElement("#START", "2:30");
      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "3:30");
      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "4:30");
      Assert.IsTrue(control.CheckAdjustTime(ref program));
    }

    [Test]
    public void StartTimeHalfDayMorning()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 06, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);


      // Program without Date, after start time
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Year == 2006);
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "7:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "10:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));


      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      control.NewDay();

      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);
    }

    [Test]
    public void StartTimeHalfDayAfternoon()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 06, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);


      // Program without Date, after start time
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Year == 2006);
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "17:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "20:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      control.NewDay();

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);
    }


    [Test]
    public void EndTime()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 06, 30, 22, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);

      // Program without Date, after start time
      program.SetElement("#START", "22:30");
      program.SetElement("#END", "23:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.EndTime.Year == 2006);
      Assert.IsTrue(program.EndTime.Month == 6);
      Assert.IsTrue(program.EndTime.Day == 30);

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      program.SetElement("#END", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.EndTime.Month == 7);
      Assert.IsTrue(program.EndTime.Day == 1);

      program = new ProgramData();
      program.SetElement("#START", "00:30");
      program.SetElement("#END", "01:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 1);
      Assert.IsTrue(program.EndTime.Month == 7);
      Assert.IsTrue(program.EndTime.Day == 1);
    }

    [Test]
    public void EndTimeNewDay()
    {
      ProgramData program = new ProgramData();
      DateTime start = new DateTime(2006, 07, 16, 10, 15, 0);
      ListingTimeControl control = new ListingTimeControl(start);


      // Program without Date, after start time
      program.SetElement("#START", "10:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Year == 2006);
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);


      Assert.IsTrue(control.CheckAdjustTime(ref program));

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      program.SetElement("#END", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 16);

      Assert.IsTrue(program.EndTime.Month == 7);
      Assert.IsTrue(program.EndTime.Day == 17);

      // Day already changed - call NewDay shouldn't push the day further ahead
      control.NewDay();

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      // Next program, after start time
      program = new ProgramData();
      program.SetElement("#START", "11:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "12:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "16:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      program = new ProgramData();
      program.SetElement("#START", "23:30");
      program.SetElement("#END", "00:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));

      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 17);

      Assert.IsTrue(program.EndTime.Month == 7);
      Assert.IsTrue(program.EndTime.Day == 18);

      // Day already changed - call NewDay shouldn't push the day further ahead
      control.NewDay();

      // New listing starts in the morning
      program = new ProgramData();
      program.SetElement("#START", "6:30");

      Assert.IsTrue(control.CheckAdjustTime(ref program));
      Assert.IsTrue(program.StartTime.Month == 7);
      Assert.IsTrue(program.StartTime.Day == 18);
    }
  }
}
