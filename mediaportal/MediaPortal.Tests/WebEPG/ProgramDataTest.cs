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
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;
using MediaPortal.WebEPG.Parser;
using System.IO;

namespace MediaPortal.Tests.WebEPG.Parser
{
  [TestFixture]
  [Category("EPG")]
  public class ProgramDataTest
  {
    [SetUp]
    public void Init()
    {
      GlobalServiceProvider.Replace<ILog>(new NoLog());
    }

    [Test]
    public void SetElementTime()
    {
      ProgramData testData = new ProgramData();
      testData.ChannelId = "myChannel.tv";

      // #START/#END Tags
      // Test usual hour values, with each separator

      testData.SetElement("#START", "0:30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#END", "0:30");            // Only one test on endTime
      Assert.IsTrue(testData.EndTime.Hour == 0);        // is enough
      Assert.IsTrue(testData.EndTime.Minute == 30);     // as it exactly behaves as StartTime do

      testData.SetElement("#END", "(02:30)");            
      Assert.IsTrue(testData.EndTime.Hour == 2);       
      Assert.IsTrue(testData.EndTime.Minute == 30);     

      testData.SetElement("#START", "10h30");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "12.30");
      Assert.IsTrue(testData.StartTime.Hour == 12);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      // Test special  values, with each separator 

      testData.SetElement("#START", "24:30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "-0.30");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "-0h09");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 9);

      // Test am/pm 
      testData.SetElement("#START", "10:30 pm");
      Assert.IsTrue(testData.StartTime.Hour == 22);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "10:30pm");
      Assert.IsTrue(testData.StartTime.Hour == 22);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "10:30 am");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "10:30am");
      Assert.IsTrue(testData.StartTime.Hour == 10);
      Assert.IsTrue(testData.StartTime.Minute == 30);

      testData.SetElement("#START", "12:00 pm");
      Assert.IsTrue(testData.StartTime.Hour == 12);
      Assert.IsTrue(testData.StartTime.Minute == 0);

      testData.SetElement("#START", "12:00 am");
      Assert.IsTrue(testData.StartTime.Hour == 0);
      Assert.IsTrue(testData.StartTime.Minute == 0);

      // <#DAY>
      //testData.SetElement("<#DAY>", "09");
      //Assert.IsTrue(testData.Day == 9);

      // <#DESCRIPTION> 
      testData.SetElement("#DESCRIPTION", "   This is description, isn't it?   ");
      Assert.IsTrue(testData.Description == "This is description, isn't it?");

    }
  }
}
