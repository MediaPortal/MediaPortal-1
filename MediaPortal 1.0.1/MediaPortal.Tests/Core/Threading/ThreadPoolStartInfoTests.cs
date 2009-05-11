#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#region Usings

using System;
using System.Threading;
using MediaPortal.Threading;
using NUnit.Framework;

#endregion

namespace MediaPortal.Tests.Core.Threading
{

  #region TestFixture

  [TestFixture]
  public class ThreadPoolStartInfoTests
  {
    #region Tests

    [Test]
    public void TestValidParameters()
    {
      ThreadPoolStartInfo tpsi1 = new ThreadPoolStartInfo(30);
      ThreadPoolStartInfo tpsi2 = new ThreadPoolStartInfo(30, 31);
      ThreadPoolStartInfo tpsi3 = new ThreadPoolStartInfo(10, 25, 30000);
      ThreadPoolStartInfo tpsi4 = new ThreadPoolStartInfo();
      tpsi4.DefaultThreadPriority = ThreadPriority.Lowest;
      try
      {
        ThreadPoolStartInfo.Validate(tpsi1);
        ThreadPoolStartInfo.Validate(tpsi2);
        ThreadPoolStartInfo.Validate(tpsi3);
        ThreadPoolStartInfo.Validate(tpsi4);
        Assert.AreEqual(30, tpsi1.MaximumThreads);
      }
      catch (ArgumentOutOfRangeException e)
      {
        Assert.Fail("Exception occurred while validating ThreadPoolStartInfo: message:{0} actualvalue:{1}", e.Message,
                    e.ActualValue);
      }
    }

    [Test]
    public void TestMinThreadsOutOfRange()
    {
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo();
      tpsi.MinimumThreads = -1;
      try
      {
        ThreadPoolStartInfo.Validate(tpsi);
        Assert.Fail("ArgumentOutOfRangeException not thrown");
      }
      catch (ArgumentOutOfRangeException e)
      {
        Assert.IsTrue(e.ParamName == "MinimumThreads");
        Assert.IsTrue(e.ActualValue is int, "ArgumentOutOfRangeException ActualValue is not of expected type");
        Assert.AreEqual(-1, (int) e.ActualValue);
      }
    }

    [Test]
    public void TestMaxThreadsLowerThanMin()
    {
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(10, 5);
      try
      {
        ThreadPoolStartInfo.Validate(tpsi);
        Assert.Fail("ArgumentOutOfRangeException not thrown");
      }
      catch (ArgumentOutOfRangeException e)
      {
        Assert.IsTrue(e.ParamName == "MinimumThreads");
        Assert.IsTrue(e.ActualValue is int, "ArgumentOutOfRangeException ActualValue is not of expected type");
        Assert.AreEqual(10, (int) e.ActualValue);
      }
    }

    [Test]
    public void TestMaxThreadsOutOfRange()
    {
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(0, 0);
      try
      {
        ThreadPoolStartInfo.Validate(tpsi);
        Assert.Fail("ArgumentOutOfRangeException not thrown");
      }
      catch (ArgumentOutOfRangeException e)
      {
        Assert.IsTrue(e.ParamName == "MaximumThreads");
        Assert.IsTrue(e.ActualValue is int, "ArgumentOutOfRangeException ActualValue is not of expected type");
        Assert.AreEqual(0, (int) e.ActualValue);
      }
    }

    [Test]
    public void TestThreadIdleTimeoutOutOfRange()
    {
      ThreadPoolStartInfo tpsi = new ThreadPoolStartInfo(2, 5, -1);
      try
      {
        ThreadPoolStartInfo.Validate(tpsi);
        Assert.Fail("ArgumentOutOfRangeException not thrown");
      }
      catch (ArgumentOutOfRangeException e)
      {
        Assert.IsTrue(e.ParamName == "ThreadIdleTimeout");
        Assert.IsTrue(e.ActualValue is int, "ArgumentOutOfRangeException ActualValue is not of expected type");
        Assert.AreEqual(-1, (int) e.ActualValue);
      }
    }

    #endregion
  }

  #endregion
}