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
using ProcessPlugins.DiskSpace;
using MediaPortal.TV.Database;

namespace MediaPortal.Tests.Disk
{
  [TestFixture]
  [Category("DiskManagement")]
  public class DeleteOldRecordings
  {
    [Test]
    public void DontDeleteRecordingsWithMethodAlways()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.Always;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsWithMethodSpace()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilSpaceNeeded;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsWithMethodWatched()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.UntilWatched;
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DontDeleteRecordingsBeforeEndDate()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
      rec.KeepRecordingTill = DateTime.Now.AddDays(+5);
      Assert.IsFalse(rec.ShouldBeDeleted);
    }
    [Test]
    public void DeleteRecordingsAfterEndDate()
    {
      TVRecorded rec = new TVRecorded();
      rec.KeepRecordingMethod = TVRecorded.KeepMethod.TillDate;
      rec.KeepRecordingTill = DateTime.Now.AddDays(-5);
      Assert.IsTrue(rec.ShouldBeDeleted);
    }

  }
}
