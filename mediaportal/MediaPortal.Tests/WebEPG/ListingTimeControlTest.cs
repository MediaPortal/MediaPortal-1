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
    public void StartTime()
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

      Assert.IsFalse(control.CheckAdjustTime(ref program));

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
  }
}
